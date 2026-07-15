using OsintToolkit.Core.Enums;

namespace OsintToolkit.Api.Contracts.Responses;

public class ScanResponse
{
    public Guid Id { get; set; }
    public string Target { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public ScanStatus Status { get; set; }
    public int? RiskScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }
}
