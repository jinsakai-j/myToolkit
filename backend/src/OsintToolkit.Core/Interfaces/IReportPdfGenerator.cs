using OsintToolkit.Core.Entities;

namespace OsintToolkit.Core.Interfaces;

public interface IReportPdfGenerator
{
    Task<byte[]> GenerateAsync(Scan scan, CancellationToken cancellationToken = default);
}