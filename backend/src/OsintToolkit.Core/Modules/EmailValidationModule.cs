using System.Text.Json;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// Email validation module. It checks the syntactic format of the address and then
/// queries the DNS MX records of the mail domain to confirm the domain is capable
/// of receiving email. No messages are ever sent and no login is attempted, keeping
/// the check within the project's ethical scope.
/// </summary>
public sealed class EmailValidationModule : IOSINTModule
{
    public string Name => "EmailValidation";

    public async Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
    {
        var email = target.Trim();

        if (!Malformed(email))
        {
            return new OSINTModuleResult
            {
                Status = ModuleStatus.Failed,
                Summary = "Invalid email format",
                RawData = JsonSerializer.Serialize(new { email, valid = false, reason = "Invalid format" })
            };
        }

        var domain = email[(email.IndexOf('@') + 1)..];

        try
        {
            var mxRecords = await DnsResolver.ResolveAsync(domain, DnsRecordType.MX, cancellationToken).ConfigureAwait(false);
            var hasMx = mxRecords.Count > 0;

            var payload = new
            {
                email,
                formatValid = true,
                domain,
                mxRecords = mxRecords.Select(r => r.Value).ToList(),
                hasMx,
                canReceiveMail = hasMx
            };

            return new OSINTModuleResult
            {
                Status = hasMx ? ModuleStatus.Completed : ModuleStatus.Failed,
                Summary = hasMx
                    ? $"{mxRecords.Count} MX record(s) found for {domain}"
                    : $"No MX record(s) found for {domain}",
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
                Summary = $"Email validation failed: {ex.Message}",
                RawData = JsonSerializer.Serialize(new { email, error = ex.Message })
            };
        }
    }

    private static bool Malformed(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
        {
            return false;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
        {
            return false;
        }

        var local = email[..atIndex];
        var domain = email[(atIndex + 1)..];

        if (local.Length > 64 || string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        return domain.Contains('.');
    }
}