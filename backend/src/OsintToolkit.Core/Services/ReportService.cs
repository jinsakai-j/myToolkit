using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;

namespace OsintToolkit.Core.Services;

public sealed class ReportService(
    IScanRepository scanRepository,
    IReportRepository reportRepository,
    IReportPdfGenerator pdfGenerator) : IReportService
{
    public async Task<Report> GeneratePdfAsync(
        Guid scanId, string outputDirectory, string? fileName, CancellationToken cancellationToken = default)
    {
        var scan = await scanRepository.GetByIdAsync(scanId, cancellationToken);
        if (scan == null)
        {
            throw new NotFoundException($"Scan with ID '{scanId}' was not found.");
        }

        var pdfBytes = await pdfGenerator.GenerateAsync(scan, cancellationToken);

        var safeFileName = FileNameOrDefault(scan, fileName);
        var directory = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, safeFileName);
        await File.WriteAllBytesAsync(fullPath, pdfBytes, cancellationToken);

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ScanId = scan.Id,
            FileName = safeFileName,
            FilePath = fullPath,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        await reportRepository.AddAsync(report, cancellationToken);
        await reportRepository.SaveChangesAsync(cancellationToken);

        return report;
    }

    public async Task<IReadOnlyList<Report>> GetReportsAsync(Guid scanId, CancellationToken cancellationToken = default)
    {
        return await reportRepository.GetByScanIdAsync(scanId, cancellationToken);
    }

    public async Task<Report> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await reportRepository.GetByIdAsync(reportId, cancellationToken);
        if (report == null)
        {
            throw new NotFoundException($"Report with ID '{reportId}' was not found.");
        }
        return report;
    }

    private static string FileNameOrDefault(Scan scan, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var cleaned = SanitizeFileName(fileName);
            return cleaned.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? cleaned : $"{cleaned}.pdf";
        }

        return $"scan-{scan.Id:N}.pdf";
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = fileName.Trim().Replace('/', '_').Replace('\\', '_').Replace('"', '_');
        return string.IsNullOrWhiteSpace(name) ? "report.pdf" : name;
    }
}