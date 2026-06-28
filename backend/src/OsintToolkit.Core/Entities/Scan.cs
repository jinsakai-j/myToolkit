using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Entities;

public sealed class Scan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Target { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public ScanStatus Status { get; set; } = ScanStatus.Pending;
    public int? RiskScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<ScanResult> Results { get; set; } = new List<ScanResult>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}

