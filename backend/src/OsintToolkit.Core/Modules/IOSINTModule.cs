using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

public interface IOSINTModule
{
    string Name { get; }
    Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default);
}