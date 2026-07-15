using System.Net;
using System.Text.RegularExpressions;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Services;

public static class TargetClassifier
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_\-\.]{1,100}$", RegexOptions.Compiled);
    private static readonly Regex DomainRegex = new(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsSupported(TargetType targetType)
    {
        return Enum.IsDefined(targetType);
    }

    public static bool IsValid(string target, TargetType targetType)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        return targetType switch
        {
            TargetType.Domain => DomainRegex.IsMatch(target),
            TargetType.Email => EmailRegex.IsMatch(target),
            TargetType.Username => UsernameRegex.IsMatch(target),
            TargetType.IpAddress => IPAddress.TryParse(target, out _),
            _ => false
        };
    }
}


