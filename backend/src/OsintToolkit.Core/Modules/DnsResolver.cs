using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace OsintToolkit.Core.Modules;

internal enum DnsRecordType : ushort
{
    A = 1,
    NS = 2,
    CNAME = 5,
    SOA = 6,
    PTR = 12,
    MX = 15,
    TXT = 16,
    AAAA = 28
}

internal sealed record DnsRecord(string Type, string Value, int Ttl);

/// <summary>
/// Minimal DNS-over-UDP resolver used by the OSINT modules. It performs standard
/// (recursive) queries against the system-configured resolver and parses the
/// common record types needed for OSINT lookups. It intentionally avoids any
/// external NuGet dependency so it stays within the .NET base class library.
/// </summary>
internal static class DnsResolver
{
    private static IPEndPoint? _resolverEndPoint;
    private static readonly ConcurrentDictionary<string, IReadOnlyList<DnsRecord>> Cache = new();

    public static async Task<IReadOnlyList<DnsRecord>> ResolveAsync(string name, DnsRecordType type, CancellationToken cancellationToken = default)
    {
        var key = $"{name}:{type}";
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var records = await QueryAsync(name, type, cancellationToken).ConfigureAwait(false);
        Cache[key] = records;
        return records;
    }

    public static void ClearCache() => Cache.Clear();

    private static IPEndPoint GetResolverEndPoint()
    {
        if (_resolverEndPoint != null)
        {
            return _resolverEndPoint;
        }

        try
        {
            var host = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                .SelectMany(n => n.GetIPProperties().DnsAddresses)
                .FirstOrDefault();

            if (host != null)
            {
                _resolverEndPoint = new IPEndPoint(host, 53);
            }
        }
        catch
        {
            // fall through to loopback default
        }

        _resolverEndPoint ??= new IPEndPoint(IPAddress.Parse("127.0.0.53"), 53);
        return _resolverEndPoint;
    }

    private static async Task<IReadOnlyList<DnsRecord>> QueryAsync(
        string name, DnsRecordType type, CancellationToken cancellationToken)
    {
        var queryId = (ushort)(Random.Shared.Next() & 0xFFFF);
        var query = BuildQuery(queryId, name, type);

        var endPoint = GetResolverEndPoint();
        using var udp = new UdpClient(AddressFamily.InterNetwork);
        udp.Client.ReceiveTimeout = 3000;

        await udp.SendAsync(query, endPoint, cancellationToken).ConfigureAwait(false);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(3000);

        var response = await udp.ReceiveAsync(timeoutCts.Token).ConfigureAwait(false);
        return ParseResponse(response.Buffer, queryId);
    }

    private static byte[] BuildQuery(ushort id, string name, DnsRecordType type)
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte)(id >> 8));
        ms.WriteByte((byte)(id & 0xFF));

        // flags: 0x0100 = recursion desired
        ms.WriteByte(0x01);
        ms.WriteByte(0x00);

        // QDCOUNT = 1
        ms.WriteByte(0x00);
        ms.WriteByte(0x01);
        // ANCOUNT, NSCOUNT, ARCOUNT = 0
        ms.WriteByte(0x00); ms.WriteByte(0x00);
        ms.WriteByte(0x00); ms.WriteByte(0x00);
        ms.WriteByte(0x00); ms.WriteByte(0x00);

        // QNAME
        foreach (var label in name.Split('.'))
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(label);
            ms.WriteByte((byte)bytes.Length);
            ms.Write(bytes);
        }
        ms.WriteByte(0x00);

        // QTYPE
        WriteUInt16(ms, (ushort)type);
        // QCLASS = IN (1)
        WriteUInt16(ms, 1);

        return ms.ToArray();
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value & 0xFF));
    }

    private static IReadOnlyList<DnsRecord> ParseResponse(byte[] buffer, ushort queryId)
    {
        if (buffer.Length < 12)
        {
            return Array.Empty<DnsRecord>();
        }

        var id = (ushort)((buffer[0] << 8) | buffer[1]);
        if (id != queryId)
        {
            return Array.Empty<DnsRecord>();
        }

        var rcode = buffer[3] & 0x0F;
        if (rcode != 0)
        {
            return Array.Empty<DnsRecord>();
        }

        var qdCount = (buffer[4] << 8) | buffer[5];
        var anCount = (buffer[6] << 8) | buffer[7];

        var offset = 12;

        // Skip the question
        for (var i = 0; i < qdCount; i++)
        {
            offset += ReadNameLength(buffer, offset);
            offset += 4; // QTYPE + QCLASS
        }

        var records = new List<DnsRecord>();
        for (var i = 0; i < anCount; i++)
        {
            offset += ReadNameLength(buffer, offset);

            if (offset + 10 > buffer.Length)
            {
                break;
            }

            var type = (DnsRecordType)((buffer[offset] << 8) | buffer[offset + 1]);
            var dataLength = (buffer[offset + 8] << 8) | buffer[offset + 9];
            var ttl = (int)ReadUInt32(buffer, offset + 4);
            offset += 10;

            if (offset + dataLength > buffer.Length)
            {
                break;
            }

            var value = ParseRecordValue(buffer, offset, dataLength, type, null);
            if (value != null)
            {
                records.Add(new DnsRecord(type.ToString(), value, ttl));
            }

            offset += dataLength;
        }

        return records;
    }

    private static uint ReadUInt32(byte[] buffer, int offset) =>
        (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3]);

    private static string? ParseRecordValue(byte[] buffer, int offset, int length, DnsRecordType type, string? ownerName)
    {
        switch (type)
        {
            case DnsRecordType.A when length >= 4:
                return $"{buffer[offset]}.{buffer[offset + 1]}.{buffer[offset + 2]}.{buffer[offset + 3]}";
            case DnsRecordType.AAAA when length >= 16:
                return new IPAddress(buffer[offset..(offset + 16)]).ToString();
            case DnsRecordType.NS:
            case DnsRecordType.CNAME:
            case DnsRecordType.PTR:
                return ReadName(buffer, offset, out _);
            case DnsRecordType.MX when length >= 3:
                var preference = (buffer[offset] << 8) | buffer[offset + 1];
                return $"{preference} {ReadName(buffer, offset + 2, out _)}";
            case DnsRecordType.TXT:
                return ParseTxt(buffer, offset, length);
            case DnsRecordType.SOA:
                return ParseSoa(buffer, offset);
            default:
                return Convert.ToHexString(buffer[offset..(offset + length)]);
        }
    }

    private static string ParseTxt(byte[] buffer, int offset, int length)
    {
        var parts = new List<string>();
        var end = offset + length;
        var i = offset;
        while (i < end)
        {
            var len = buffer[i];
            i++;
            var segmentLength = Math.Min(len, end - i);
            parts.Add(System.Text.Encoding.UTF8.GetString(buffer, i, segmentLength));
            i += segmentLength;
        }
        return string.Join("", parts);
    }

    private static string ParseSoa(byte[] buffer, int offset)
    {
        var reportedLength = ReadNameLength(buffer, offset);
        var name = ReadName(buffer, offset, out _);
        offset += reportedLength;
        var rname = ReadName(buffer, offset, out var rnameLength);
        offset += rnameLength;
        // 5x 32-bit fields
        var fields = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            if (offset + 4 > buffer.Length)
            {
                break;
            }
            var secondaries = ReadUInt32(buffer, offset);
            fields.Add(((int)secondaries).ToString());
            offset += 4;
        }
        return $"mname={name}; rname={rname}; ({string.Join(",", fields)})";
    }

    /// <summary>
    /// Reads a DNS name (possibly with compression pointers) and returns it, advancing
    /// only the returned length to track the absolute next offset.
    /// </summary>
    private static string ReadName(byte[] buffer, int offset, out int nameLength)
    {
        var result = new List<string>();
        var jumped = false;
        var pos = offset;
        var total = 0;

        while (pos < buffer.Length)
        {
            var len = buffer[pos];
            if (len == 0)
            {
                total += 1;
                break;
            }

            if ((len & 0xC0) == 0xC0)
            {
                if (!jumped)
                {
                    total += 2;
                    jumped = true;
                }
                var pointer = ((len & 0x3F) << 8) | buffer[pos + 1];
                pos = pointer;
                continue;
            }

            if (pos + 1 + len > buffer.Length)
            {
                break;
            }

            result.Add(System.Text.Encoding.ASCII.GetString(buffer, pos + 1, len));
            total += 1 + len;
            pos += 1 + len;
        }

        nameLength = total;
        return result.Count > 0 ? string.Join(".", result) : ".";
    }

    private static int ReadNameLength(byte[] buffer, int offset)
    {
        var pos = offset;
        while (pos < buffer.Length)
        {
            var len = buffer[pos];
            if (len == 0)
            {
                return pos - offset + 1;
            }
            if ((len & 0xC0) == 0xC0)
            {
                return pos - offset + 2;
            }
            pos += 1 + len;
        }
        return pos - offset;
    }
}