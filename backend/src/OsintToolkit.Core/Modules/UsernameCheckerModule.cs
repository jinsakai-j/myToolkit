using System.Net.Http.Json;
using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// Username checker module. It checks whether a username exists on a set of
/// popular platforms using their public, unauthenticated APIs and shows profile
/// metadata where available. It only reads publicly visible profile pages and
/// never attempts login or enumeration beyond a single predictable lookup.
/// </summary>
public sealed class UsernameCheckerModule : IOSINTModule
{
    private const string GitHubApi = "https://api.github.com/users/";
    private const string GitLabApi = "https://gitlab.com/api/v4/users";
    private const string HackerNewsApi = "https://hacker-news.firebaseio.com/v0/user/";

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public string Name => "UsernameChecker";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        var username = target.Trim();
        var found = new List<object>();
        var foundPlatforms = new List<string>();

        try
        {
            var tasks = new[]
            {
                CheckGitHubAsync(username, cancellationToken),
                CheckGitLabAsync(username, cancellationToken),
                CheckHackerNewsAsync(username, cancellationToken)
            };
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var result in results)
            {
                if (result is null)
                {
                    continue;
                }

                if (result.Payload is not null)
                {
                    found.Add(result.Payload);
                }

                if (result.Available)
                {
                    foundPlatforms.Add(result.Platform);
                }
            }

            var payload = new
            {
                username,
                platformsChecked = new[] { "GitHub", "GitLab", "HackerNews" },
                platformsFound = foundPlatforms,
                results = found,
                totalFound = found.Count
            };

            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = foundPlatforms.Count > 0
                    ? $"Found on {foundPlatforms.Count} of 3 platforms ({string.Join(", ", foundPlatforms)})"
                    : "Username not found on GitHub, GitLab, or HackerNews",
                RawData = JsonSerializer.Serialize(payload)
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new OSINTModuleResult
            {
                Status = ModuleStatus.Failed,
                Summary = $"Username check failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { username, error = ex.Message })
            };
        }
    }

    private static async Task<PlatformCheck?> CheckGitHubAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await HttpClient.GetAsync($"{GitHubApi}{Uri.EscapeDataString(username)}", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                return PlatformCheck.Error("GitHub", (int)response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return PlatformCheck.Found("GitHub", new
            {
                platform = "GitHub",
                available = true,
                login = data.TryGetProperty("login", out var login) ? login.GetString() : null,
                name = data.TryGetProperty("name", out var name) ? name.GetString() : null,
                company = data.TryGetProperty("company", out var company) ? company.GetString() : null,
                publicRepos = data.TryGetProperty("public_repos", out var repos) ? repos.GetInt32() : 0,
                followers = data.TryGetProperty("followers", out var followers) ? followers.GetInt32() : 0,
                createdAt = data.TryGetProperty("created_at", out var created) ? created.GetString() : null,
                bio = data.TryGetProperty("bio", out var bio) ? bio.GetString() : null
            });
        }
        catch (HttpRequestException ex)
        {
            return PlatformCheck.Error("GitHub", ex.Message);
        }
    }

    private static async Task<PlatformCheck?> CheckGitLabAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await HttpClient.GetAsync($"{GitLabApi}?username={Uri.EscapeDataString(username)}", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return PlatformCheck.Error("GitLab", (int)response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<List<JsonElement>>(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (data == null || data.Count == 0)
            {
                return null;
            }

            var user = data[0];
            return PlatformCheck.Found("GitLab", new
            {
                platform = "GitLab",
                available = true,
                username = user.TryGetProperty("username", out var gitlabUser) ? gitlabUser.GetString() : null,
                name = user.TryGetProperty("name", out var name) ? name.GetString() : null,
                state = user.TryGetProperty("state", out var state) ? state.GetString() : null,
                webUrl = user.TryGetProperty("web_url", out var webUrl) ? webUrl.GetString() : null
            });
        }
        catch (HttpRequestException ex)
        {
            return PlatformCheck.Error("GitLab", ex.Message);
        }
    }

    private static async Task<PlatformCheck?> CheckHackerNewsAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await HttpClient.GetAsync($"{HackerNewsApi}{Uri.EscapeDataString(username)}.json", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return PlatformCheck.Error("HackerNews", (int)response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (data.ValueKind == JsonValueKind.Null || data.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return PlatformCheck.Found("HackerNews", new
            {
                platform = "HackerNews",
                available = true,
                id = data.TryGetProperty("id", out var id) ? id.GetString() : null,
                karma = data.TryGetProperty("karma", out var karma) ? karma.GetInt32() : 0,
                created = data.TryGetProperty("created", out var created) ? created.GetInt64() : 0
            });
        }
        catch (HttpRequestException ex)
        {
            return PlatformCheck.Error("HackerNews", ex.Message);
        }
    }

    private sealed record PlatformCheck(string Platform, bool Available, object? Payload)
    {
        public static PlatformCheck Found(string platform, object payload) =>
            new(platform, true, payload);

        public static PlatformCheck Error(string platform, object error) =>
            new(platform, false, new { platform, available = false, error });
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("osint-toolkit-local/0.2 (local OSINT toolkit; public data only)");
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}