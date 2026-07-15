using OsintToolkit.Core.Enums;

namespace OsintToolkit.Api.Contracts.Responses;

public class ScanResultResponse
{
    public Guid Id { get; set; }
    public Guid ScanId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public ModuleStatus Status { get; set; }
    public string? Summary { get; set; }
    public string? RawData { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
