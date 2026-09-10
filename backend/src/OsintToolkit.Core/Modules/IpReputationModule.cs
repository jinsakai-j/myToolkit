using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// IP reputation module. It enriches an IP address with public ipinfo.io data
/// (hostname, organization, geolocation, anycast flag) and derives a heuristic
/// risk score from signals such as Tor exit nodes, proxies, VPNs, and abusive
/// host names. The heuristic only uses public data and DNS-derived hints.
/// </summary>
public sealed class IpReputationModule : IOSINTModule
{
    private const string IpInfoUrl = "https://ipinfo.io/";

    private static readonly string[] RiskKeywords =
    {
        "tor", "exit", "proxy", "vpn", "spam", "abuse", "botnet", "malware", "darknet"
    };

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public string Name => "IpReputation";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        if (!IPAddress.TryParse(target.Trim(), out var ip))
        {
            return new OSINTModuleResult
            {
                Status = ModuleStatus.Failed,
                Summary = "Invalid IP address format",
                RawData = JsonSerializer.Serialize(new { ip = target, error = "Invalid IP address" })
            };
        }

        try
        {
            using var response = await HttpClient.GetAsync($"{IpInfoUrl}{ip}/json", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new OSINTModuleResult
                {
                    Status = ModuleStatus.Failed,
                    Summary = $"IP lookup failed: HTTP {(int)response.StatusCode}",
                    RawData = JsonSerializer.Serialize(new { ip = ip.ToString(), httpStatus = (int)response.StatusCode })
                };
            }

            var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken).ConfigureAwait(false);
            var hostname = GetString(data, "hostname");
            var org = GetString(data, "org");
            var city = GetString(data, "city");
            var region = GetString(data, "region");
            var country = GetString(data, "country");
            var loc = GetString(data, "loc");
            var isAnycast = data.TryGetProperty("anycast", out var anycast) && anycast.ValueKind == JsonValueKind.True;

            var signals = new List<string>();
            var riskScore = 0;

            var haystack = $"{hostname} {org}".ToLowerInvariant();
            if (RiskKeywords.Any(k => haystack.Contains(k)))
            {
                foreach (var keyword in RiskKeywords)
                {
                    if (!haystack.Contains(keyword))
                    {
                        continue;
                    }

                    signals.Add(keyword);
                    riskScore += keyword switch
                    {
                        "tor" => 30,
                        "exit" => 25,
                        "proxy" => 25,
                        "vpn" => 15,
                        "abuse" => 15,
                        "spam" => 15,
                        "darknet" => 20,
                        "botnet" => 25,
                        "malware" => 20,
                        _ => 5
                    };
                }
            }

            if (isAnycast)
            {
                signals.Add("anycast");
                riskScore += 5;
            }

            var finalScore = Math.Min(100, riskScore);

            var payload = new
            {
                ip = ip.ToString(),
                hostname,
                org,
                city,
                region,
                country,
                loc,
                anycast = isAnycast,
                signals,
                riskScore = finalScore,
                source = "ipinfo.io + heuristic hostname/org signals"
            };

            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = finalScore >= 50
                    ? $"Risk score {finalScore}/100 - elevated signals: {string.Join(", ", signals)}"
                    : finalScore > 0
                        ? $"Risk score {finalScore}/100 - signals: {string.Join(", ", signals)}"
                        : $"Risk score 0/100 - no elevated signals detected",
                RiskScore = finalScore,
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
                Summary = $"IP reputation lookup failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { ip = ip.ToString(), error = ex.Message })
            };
        }
    }

    private static string? GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
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