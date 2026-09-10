using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// Subdomain verification module. It takes the passive certificate
/// transparency candidates discovered by <see cref="SubdomainFinderModule"/>
/// (re-querying the same passive source) and resolves a bounded set of them to
/// A/AAAA records. It never guesses hostnames, so no brute-force enumeration is
/// involved: only names already attested in public CT logs are resolved.
/// </summary>
public sealed class SubdomainResolveModule : IOSINTModule
{
    private const int CandidateLimit = 25;

    public string Name => "SubdomainResolve";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        var domain = target.Trim().TrimEnd('.').ToLowerInvariant();
        try
        {
            var json = await SubdomainFinderModule.FetchCertificateDataAsync(domain, cancellationToken).ConfigureAwait(false);
            var candidates = SubdomainFinderModule.ParseCertificateData(json, domain).Take(CandidateLimit).ToList();

            var names = new List<object>();
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var aRecords = await DnsResolver.ResolveAsync(candidate, DnsRecordType.A, cancellationToken).ConfigureAwait(false);
                var aaaaRecords = await DnsResolver.ResolveAsync(candidate, DnsRecordType.AAAA, cancellationToken).ConfigureAwait(false);

                names.Add(new
                {
                    name = candidate,
                    ipv4 = aRecords.Select(r => r.Value).ToList(),
                    ipv6 = aaaaRecords.Select(r => r.Value).ToList()
                });
            }

            var resolvedCount = names.Count(entry =>
            {
                using var doc = JsonDocument.Parse(JsonSerializer.Serialize(entry));
                var root = doc.RootElement;
                bool HasAddresses(string property) =>
                    root.TryGetProperty(property, out var arr) &&
                    arr.ValueKind == JsonValueKind.Array &&
                    arr.GetArrayLength() > 0;

                return HasAddresses("ipv4") || HasAddresses("ipv6");
            });

            var payload = new
            {
                domain,
                source = "crt.sh (Certificate Transparency) + DNS A/AAAA resolution",
                candidatesExamined = candidates.Count,
                resolvedCount,
                names
            };

            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = candidates.Count > 0
                    ? $"{resolvedCount} of {candidates.Count} passive subdomain candidate(s) resolved to IP addresses"
                    : $"No subdomain candidates found via certificate transparency for {domain}",
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
                Summary = $"Subdomain verification failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { domain, error = ex.Message })
            };
        }
    }
}