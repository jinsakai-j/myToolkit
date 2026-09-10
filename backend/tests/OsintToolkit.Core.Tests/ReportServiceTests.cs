using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Core.Services;
using Xunit;

namespace OsintToolkit.Core.Tests;

public sealed class ReportServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ReportServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "report-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // best effort cleanup
        }
    }

    [Fact]
    public async Task GeneratePdfAsync_WhenScanNotFound_ThrowsNotFoundException()
    {
        var service = CreateService(null, new FakeReportRepository(), _ => throw new NotFoundException(""));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GeneratePdfAsync(Guid.NewGuid(), _tempDir, null));
    }

    [Fact]
    public async Task GeneratePdfAsync_SavesFileAndPersistsReport()
    {
        var scan = new Scan { Target = "example.com", TargetType = TargetType.Domain, Status = ScanStatus.Completed };
        var reportRepository = new FakeReportRepository();
        var service = CreateService(scan, reportRepository, scan => new byte[] { 0x25, 0x50, 0x44, 0x46 });

        var report = await service.GeneratePdfAsync(scan.Id, _tempDir, null);

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(await System.IO.File.ReadAllBytesAsync(report.FilePath)));
        Assert.True(System.IO.File.Exists(report.FilePath));
        Assert.StartsWith($"scan-{scan.Id:N}.pdf", report.FileName);
        Assert.Contains(reportRepository.Reports, r => r.FileName == report.FileName);
        Assert.True(reportRepository.SaveChangesCalled);
    }

    [Fact]
    public async Task GeneratePdfAsync_WithCustomFileName_AddsPdfExtension()
    {
        var scan = new Scan { Target = "example.com", TargetType = TargetType.Domain, Status = ScanStatus.Completed };
        var reportRepository = new FakeReportRepository();
        var service = CreateService(scan, reportRepository, _ => new byte[] { 0x25, 0x50, 0x44, 0x46 });

        var report = await service.GeneratePdfAsync(scan.Id, _tempDir, "my-report");

        Assert.Equal("my-report.pdf", report.FileName);
        Assert.True(System.IO.File.Exists(report.FilePath));
    }

    [Fact]
    public async Task GetReportsAsync_ReturnsReportsForScan()
    {
        var scan = new Scan { Target = "x.com", TargetType = TargetType.Domain };
        var reportRepository = new FakeReportRepository();
        reportRepository.Reports.Add(new Report { Id = Guid.NewGuid(), ScanId = scan.Id, FileName = "a.pdf" });
        var service = CreateService(scan, reportRepository, _ => Array.Empty<byte>());

        var reports = await service.GetReportsAsync(scan.Id);

        Assert.Single(reports);
    }

    [Fact]
    public async Task GetReportByIdAsync_WhenMissing_ThrowsNotFoundException()
    {
        var service = CreateService(null, new FakeReportRepository(), _ => Array.Empty<byte>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetReportByIdAsync(Guid.NewGuid()));
    }

    private static ReportService CreateService(
        Scan? scan,
        FakeReportRepository reportRepository,
        Func<Scan, byte[]> generate)
    {
        var scanRepository = new FakeScanRepositoryForReports(scan);
        var pdfGenerator = new FakePdfGenerator(generate);
        return new ReportService(scanRepository, reportRepository, pdfGenerator);
    }

    private sealed class FakeScanRepositoryForReports : IScanRepository
    {
        private readonly Scan? _scan;

        public FakeScanRepositoryForReports(Scan? scan)
        {
            _scan = scan;
        }

        public Task<Scan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_scan);

        public Task<IReadOnlyList<Scan>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Scan>>(new List<Scan>());

        public Task AddAsync(Scan scan, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Delete(Scan scan)
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeReportRepository : IReportRepository
    {
        public List<Report> Reports { get; } = new();
        public bool SaveChangesCalled { get; private set; }

        public Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Reports.FirstOrDefault(r => r.Id == id));

        public Task<IReadOnlyList<Report>> GetByScanIdAsync(Guid scanId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Report>>(Reports.Where(r => r.ScanId == scanId).ToList());

        public Task AddAsync(Report report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }

        public void Delete(Report report)
        {
            Reports.Remove(report);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(1);
        }
    }

    private sealed class FakePdfGenerator(Func<Scan, byte[]> generate) : IReportPdfGenerator
    {
        public Task<byte[]> GenerateAsync(Scan scan, CancellationToken cancellationToken = default) =>
            Task.FromResult(generate(scan));
    }
}