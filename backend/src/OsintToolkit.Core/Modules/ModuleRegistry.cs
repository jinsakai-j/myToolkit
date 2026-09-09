using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

/// <summary>
/// Holds the set of implemented OSINT modules and resolves a module by its
/// identifier, matching the ids exposed by the frontend UI.
/// </summary>
public sealed class ModuleRegistry
{
    private static readonly Dictionary<string, IOSINTModule> Modules = new(
        StringComparer.OrdinalIgnoreCase)
    {
        [new DnsLookupModule().Name] = new DnsLookupModule(),
        [new WhoisLookupModule().Name] = new WhoisLookupModule(),
        [new EmailValidationModule().Name] = new EmailValidationModule()
    };

    private static readonly HashSet<string> KnownIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "DnsLookup",
        "WhoisLookup",
        "EmailValidation",
        "UsernameChecker",
        "IpReputation"
    };

    public static bool IsKnown(string moduleName) => KnownIds.Contains(moduleName);

    public static IOSINTModule? Resolve(string moduleName)
    {
        return Modules.TryGetValue(moduleName, out var module) ? module : null;
    }

    public static string[] SupportedFor(TargetType targetType)
    {
        return targetType switch
        {
            TargetType.Domain => new[] { "DnsLookup", "WhoisLookup" },
            TargetType.Email => new[] { "EmailValidation" },
            TargetType.Username => new[] { "UsernameChecker" },
            TargetType.IpAddress => new[] { "DnsLookup", "IpReputation" },
            _ => Array.Empty<string>()
        };
    }
}