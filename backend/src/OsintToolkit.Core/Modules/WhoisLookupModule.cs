using System.Net.Http.Json;
using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// WHOIS Lookup module. It queries WHOIS information through the RDAP (Registration
/// Data Access Protocol) bootstrap service over HTTPS, which is the modern,
/// machine-readable successor to the classic port-43 WHOIS protocol and works in
/// restricted local networks where outbound TCP port 43 may be blocked.
/// </summary>
public sealed class WhoisLookupModule : IOSINTModule
{
    private const string BootstrapUrl = "https://rdap.org/domain/";

    private static readonly HttpClient HttpClient = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        // Some RDAP bootstrap targets (e.g. Verisign) reject clients with the
        // default .NET User-Agent, so identify as a standards-compliant client.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("osint-toolkit-local/0.1 (+https://github.com/; RFC 9083 RDAP client)");
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/rdap+json"));
        return client;
    }

    public string Name => "WhoisLookup";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync($"{BootstrapUrl}{target}", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new OSINTModuleResult
                {
                    Status = ModuleStatus.Failed,
                    Summary = $"WHOIS lookup failed: HTTP {(int)response.StatusCode}",
                    RawData = JsonSerializer.Serialize(new { httpStatus = (int)response.StatusCode })
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = BuildSummary(payload),
                RawData = payload.GetRawText()
            };
        }
        catch (TaskCanceledException)
        {
            throw;
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
                Summary = $"WHOIS lookup failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private static string BuildSummary(JsonElement payload)
    {
        var parts = new List<string>();

        if (payload.TryGetProperty("ldhName", out var ldh) && ldh.ValueKind == JsonValueKind.String)
        {
            parts.Add(ldh.GetString()!);
        }

        if (payload.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.Array)
        {
            var statuses = status.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .ToList();
            if (statuses.Count > 0)
            {
                parts.Add($"status: {string.Join(", ", statuses)}");
            }
        }

        if (payload.TryGetProperty("events", out var events) && events.ValueKind == JsonValueKind.Array)
        {
            var expiry = events.EnumerateArray().FirstOrDefault(e =>
                e.TryGetProperty("eventAction", out var action) &&
                action.ValueKind == JsonValueKind.String &&
                action.GetString() == "expiration");
            if (expiry.ValueKind == JsonValueKind.Object &&
                expiry.TryGetProperty("eventDate", out var eventDate) &&
                eventDate.ValueKind == JsonValueKind.String)
            {
                parts.Add($"expires: {eventDate.GetString()}");
            }
        }

        return parts.Count > 0
            ? string.Join(" | ", parts)
            : "WHOIS data retrieved via RDAP";
    }
}