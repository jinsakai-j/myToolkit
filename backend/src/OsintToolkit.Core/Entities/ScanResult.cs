using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Entities;

public sealed class ScanResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScanId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public ModuleStatus Status { get; set; } = ModuleStatus.Pending;
    public string? Summary { get; set; }
    public string? RawData { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Scan? Scan { get; set; }
}

