namespace OsintToolkit.Api.Contracts.Responses;

public sealed class ReportResponse
{
    public Guid Id { get; set; }
    public Guid ScanId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
}