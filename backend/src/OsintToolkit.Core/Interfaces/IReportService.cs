using OsintToolkit.Core.Entities;

namespace OsintToolkit.Core.Interfaces;

public interface IReportService
{
    Task<Report> GeneratePdfAsync(Guid scanId, string outputDirectory, string? fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetReportsAsync(Guid scanId, CancellationToken cancellationToken = default);
    Task<Report> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);
}