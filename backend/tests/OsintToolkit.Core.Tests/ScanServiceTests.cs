using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Core.Modules;
using OsintToolkit.Core.Services;
using Xunit;

namespace OsintToolkit.Core.Tests;

public sealed class ScanServiceTests
{
    private sealed class FakeScanRepository : IScanRepository
    {
        public List<Scan> Scans { get; } = new();
        public bool SaveChangesCalled { get; private set; }

        public Task<Scan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var scan = Scans.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(scan);
        }

        public Task<IReadOnlyList<Scan>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Scan> result = Scans.OrderByDescending(s => s.CreatedAt).ToList();
            return Task.FromResult(result);
        }

        public Task AddAsync(Scan scan, CancellationToken cancellationToken = default)
        {
            Scans.Add(scan);
            return Task.CompletedTask;
        }

        public void Delete(Scan scan)
        {
            Scans.Remove(scan);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(1);
        }
    }

    [Fact]
    public async Task CreateScanAsync_WithInvalidTarget_ThrowsValidationException()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateScanAsync("invalid_domain", TargetType.Domain, new List<string>()));
    }

    [Fact]
    public async Task CreateScanAsync_WithValidTarget_SavesScanWithModuleResults()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository, _ => new FakeModule(ModuleStatus.Completed, "ok"));
        var modules = new List<string> { "DnsLookup", "WhoisLookup" };

        var scan = await service.CreateScanAsync("google.com", TargetType.Domain, modules);

        Assert.NotNull(scan);
        Assert.Equal("google.com", scan.Target);
        Assert.Equal(TargetType.Domain, scan.TargetType);
        Assert.Equal(ScanStatus.Completed, scan.Status);
        Assert.Equal(2, scan.Results.Count);
        Assert.True(repository.SaveChangesCalled);
        Assert.Contains(repository.Scans, s => s.Id == scan.Id);

        var firstResult = scan.Results.First();
        Assert.Equal("DnsLookup", firstResult.ModuleName);
        Assert.Equal(ModuleStatus.Completed, firstResult.Status);
        Assert.Equal("ok", firstResult.Summary);
    }

    [Fact]
    public async Task CreateScanAsync_WithUnknownModule_RecordsFailedResult()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository, _ => null);
        var modules = new List<string> { "UnknownModule" };

        var scan = await service.CreateScanAsync("google.com", TargetType.Domain, modules);

        Assert.Equal(ScanStatus.Failed, scan.Status);
        var result = Assert.Single(scan.Results);
        Assert.Equal(ModuleStatus.Failed, result.Status);
    }

    [Fact]
    public async Task CreateScanAsync_WithDuplicateModules_RunsOnce()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository, _ => new FakeModule(ModuleStatus.Completed, "ok"));
        var modules = new List<string> { "DnsLookup", "dnslookup" };

        var scan = await service.CreateScanAsync("google.com", TargetType.Domain, modules);

        Assert.Single(scan.Results);
    }

    [Fact]
    public async Task CreateScanAsync_SetsRiskScoreFromModule()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository, _ => new FakeModule(ModuleStatus.Completed, "ok", riskScore: 42));
        var modules = new List<string> { "IpReputation" };

        var scan = await service.CreateScanAsync("8.8.8.8", TargetType.IpAddress, modules);

        Assert.Equal(42, scan.RiskScore);
    }

    [Fact]
    public async Task CreateScanAsync_WithoutRiskModule_LeavesRiskScoreNull()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository, _ => new FakeModule(ModuleStatus.Completed, "ok"));
        var modules = new List<string> { "DnsLookup" };

        var scan = await service.CreateScanAsync("google.com", TargetType.Domain, modules);

        Assert.Null(scan.RiskScore);
    }

    [Fact]
    public async Task GetScanByIdAsync_WhenExists_ReturnsScan()
    {
        var repository = new FakeScanRepository();
        var scan = new Scan { Target = "test", TargetType = TargetType.Username };
        repository.Scans.Add(scan);

        var service = new ScanService(repository);
        var result = await service.GetScanByIdAsync(scan.Id);

        Assert.Equal(scan.Id, result.Id);
    }

    [Fact]
    public async Task GetScanByIdAsync_WhenNotExists_ThrowsNotFoundException()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetScanByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteScanAsync_WhenExists_RemovesScan()
    {
        var repository = new FakeScanRepository();
        var scan = new Scan { Target = "test", TargetType = TargetType.Username };
        repository.Scans.Add(scan);

        var service = new ScanService(repository);
        await service.DeleteScanAsync(scan.Id);

        Assert.Empty(repository.Scans);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task DeleteScanAsync_WhenNotExists_ThrowsNotFoundException()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteScanAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateNotesAsync_WhenExists_UpdatesAndSaves()
    {
        var repository = new FakeScanRepository();
        var scan = new Scan { Target = "test", TargetType = TargetType.Username };
        repository.Scans.Add(scan);

        var service = new ScanService(repository);
        var updated = await service.UpdateNotesAsync(scan.Id, "  follow up next week  ");

        Assert.Equal("follow up next week", updated.Notes);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task UpdateNotesAsync_WithBlankNotes_ClearsNotes()
    {
        var repository = new FakeScanRepository();
        var scan = new Scan { Target = "test", TargetType = TargetType.Username, Notes = "old" };
        repository.Scans.Add(scan);

        var service = new ScanService(repository);
        var updated = await service.UpdateNotesAsync(scan.Id, "   ");

        Assert.Null(updated.Notes);
    }

    [Fact]
    public async Task UpdateNotesAsync_WhenNotExists_ThrowsNotFoundException()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateNotesAsync(Guid.NewGuid(), "notes"));
    }

    private sealed class FakeModule : IOSINTModule
    {
        private readonly ModuleStatus _status;
        private readonly string _summary;
        private readonly int? _riskScore;

        public FakeModule(ModuleStatus status, string summary, int? riskScore = null)
        {
            _status = status;
            _summary = summary;
            _riskScore = riskScore;
        }

        public string Name => "Fake";

        public Task<OSINTModuleResult> ExecuteAsync(string target, TargetType targetType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OSINTModuleResult
            {
                Status = _status,
                Summary = _summary,
                RawData = "{\"fake\": true}",
                RiskScore = _riskScore
            });
        }
    }
}
