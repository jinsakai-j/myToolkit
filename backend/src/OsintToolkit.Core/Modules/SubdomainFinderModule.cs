using System.Net.Http.Json;
using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// Passive subdomain finder module. It queries public certificate
/// transparency (CT) logs via crt.sh and collects the unique subdomains
/// attested by issued certificates for a target domain. The lookup is
/// entirely passive and does not touch the target infrastructure.
/// </summary>
public sealed class SubdomainFinderModule : IOSINTModule
{
    internal const string CertificateTransparencyApi = "https://crt.sh/?q=%25.{0}&output=json";

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public string Name => "SubdomainFinder";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        var domain = NormalizeDomain(target);
        try
        {
            var json = await FetchCertificateDataAsync(domain, cancellationToken).ConfigureAwait(false);
            var subdomains = ParseCertificateData(json, domain);

            var payload = new
            {
                domain,
                source = "crt.sh (Certificate Transparency)",
                subdomainCount = subdomains.Count,
                subdomains = subdomains.Take(500).ToList()
            };

            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = subdomains.Count > 0
                    ? $"Found {subdomains.Count} unique subdomain(s) for {domain} via certificate transparency"
                    : $"No subdomains found for {domain} in certificate transparency logs",
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
                Summary = $"Subdomain enumeration failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { domain, error = ex.Message })
            };
        }
    }

    /// <summary>
    /// Fetches the crt.sh JSON payload for a domain with a single retry, since
    /// crt.sh occasionally returns transient 5xx responses.
    /// </summary>
    internal static async Task<string> FetchCertificateDataAsync(string domain, CancellationToken cancellationToken)
    {
        const int maxAttempts = 2;
        Exception? lastError = null;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationToken).ConfigureAwait(false);
            }

            var url = string.Format(CertificateTransparencyApi, Uri.EscapeDataString(domain));
            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                }

                lastError = new HttpRequestException($"Certificate transparency query failed with status {(int)response.StatusCode}.");
            }
            catch (HttpRequestException ex)
            {
                lastError = ex;
            }
        }

        throw lastError ?? new HttpRequestException("Certificate transparency query failed.");
    }

    /// <summary>
    /// Parses the crt.sh JSON array into a sorted, de-duplicated list of
    /// subdomains strictly under the given domain. Wildcard entries and names
    /// that do not belong to the domain are ignored. Split into a public
    /// static helper so the parsing logic is unit-testable without the network.
    /// </summary>
    public static List<string> ParseCertificateData(string json, string domain)
    {
        var normalizedDomain = NormalizeDomain(domain);
        var result = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(json))
        {
            return result.ToList();
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return result.ToList();
            }

            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                IEnumerable<string> candidateNames = Array.Empty<string>();

                if (entry.TryGetProperty("name_value", out var nameValue) && nameValue.ValueKind == JsonValueKind.String)
                {
                    candidateNames = nameValue.GetString()?.Split('\n') ?? Array.Empty<string>();
                }

                if (!candidateNames.Any() &&
                    entry.TryGetProperty("common_name", out var commonName) &&
                    commonName.ValueKind == JsonValueKind.String)
                {
                    candidateNames = new[] { commonName.GetString() ?? string.Empty };
                }

                foreach (var name in candidateNames)
                {
                    var candidate = name.Trim().TrimEnd('.').ToLowerInvariant();
                    if (candidate.Length == 0 ||
                        candidate.Contains('*') ||
                        !candidate.EndsWith("." + normalizedDomain, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    result.Add(candidate);
                }
            }
        }
        catch (JsonException)
        {
            // Malformed response: treat as no subdomains found.
        }

        return result.ToList();
    }

    private static string NormalizeDomain(string target)
    {
        return target.Trim().TrimEnd('.').ToLowerInvariant();
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("osint-toolkit-local/0.4 (local OSINT toolkit; public data only)");
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}