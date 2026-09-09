using System.Net;
using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// DNS Lookup module. For a domain it resolves A, AAAA, MX, TXT and NS records.
/// For an IP address it performs a reverse (PTR) lookup. Output is structured JSON
/// consumed by the scan detail UI.
/// </summary>
public sealed class DnsLookupModule : IOSINTModule
{
    public string Name => "DnsLookup";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = targetType switch
            {
                TargetType.IpAddress => await ReverseLookupAsync(target, cancellationToken).ConfigureAwait(false),
                _ => await ForwardLookupAsync(target, cancellationToken).ConfigureAwait(false)
            };

            return new OSINTModuleResult
            {
                Status = ModuleStatus.Completed,
                Summary = BuildSummary(payload),
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
                Summary = $"DNS lookup failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private static async Task<object> ForwardLookupAsync(string domain, CancellationToken cancellationToken)
    {
        var aRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.A, cancellationToken).ConfigureAwait(false);
        var aaaaRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.AAAA, cancellationToken).ConfigureAwait(false);
        var mxRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.MX, cancellationToken).ConfigureAwait(false);
        var txtRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.TXT, cancellationToken).ConfigureAwait(false);
        var nsRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.NS, cancellationToken).ConfigureAwait(false);

        var hostEntry = await GetHostEntryAsync(domain).ConfigureAwait(false);

        return new
        {
            domain,
            a = aRecords.Select(r => r.Value).ToList(),
            aaaa = aaaaRecords.Select(r => r.Value).ToList(),
            mx = mxRecords.Select(r => r.Value).ToList(),
            txt = txtRecords.Select(r => r.Value).ToList(),
            ns = nsRecords.Select(r => r.Value).ToList(),
            canonicalName = hostEntry?.HostName,
            resolvedAddresses = hostEntry?.AddressList.Select(ip => ip.ToString()).ToList() ?? new List<string>()
        };
    }

    private static async Task<object> ReverseLookupAsync(string ip, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ip, out var address))
        {
            return new { ip, error = "Invalid IP address", ptr = Array.Empty<string>() };
        }

        var reversed = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            ? BuildReverseIpv4(address)
            : BuildReverseIpv6(address);

        if (reversed == null)
        {
            return new { ip, ptr = Array.Empty<string>() };
        }

        // PTR is exposed under the in-addr.arpa / ip6.arpa name (which is what we query).
        var ptrRecords = await DnsResolver.ResolveAsync(reversed, DnsRecordType.PTR, cancellationToken).ConfigureAwait(false);
        return new { ip, ptr = ptrRecords.Select(r => r.Value).ToList() };
    }

    private static string? BuildReverseIpv4(IPAddress address)
    {
        var octets = address.GetAddressBytes();
        return $"{octets[3]}.{octets[2]}.{octets[1]}.{octets[0]}.in-addr.arpa";
    }

    private static string? BuildReverseIpv6(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        var parts = new System.Text.StringBuilder();
        for (var i = bytes.Length - 1; i >= 0; i--)
        {
            parts.Append(Convert.ToString(bytes[i] >> 4, 16));
            parts.Append(Convert.ToString(bytes[i] & 0x0F, 16));
            parts.Append('.');
        }
        parts.Append("ip6.arpa");
        return parts.ToString();
    }

    private static async Task<IPHostEntry?> GetHostEntryAsync(string domain)
    {
        try
        {
            return await Dns.GetHostEntryAsync(domain).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    private static string BuildSummary(object payload)
    {
        // Serialize and inspect A/PTR counts for a concise summary.
        var json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("ptr", out var ptr) && ptr.GetArrayLength() > 0)
        {
            return $"{ptr.GetArrayLength()} PTR record(s) found";
        }

        var aCount = root.TryGetProperty("a", out var a) ? a.GetArrayLength() : 0;
        var aaaaCount = root.TryGetProperty("aaaa", out var aaaa) ? aaaa.GetArrayLength() : 0;
        var mxCount = root.TryGetProperty("mx", out var mx) ? mx.GetArrayLength() : 0;
        var nsCount = root.TryGetProperty("ns", out var ns) ? ns.GetArrayLength() : 0;

        return $"{aCount} A, {aaaaCount} AAAA, {mxCount} MX, {nsCount} NS record(s) found";
    }
}