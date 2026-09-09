using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Modules;

public sealed class OSINTModuleResult
{
    public ModuleStatus Status { get; set; } = ModuleStatus.Pending;
    public string? Summary { get; set; }
    public string RawData { get; set; } = "{}";
}