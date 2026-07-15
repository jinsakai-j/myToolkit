using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;
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
    public async Task CreateScanAsync_WithValidTarget_SavesScanWithPlaceholderResults()
    {
        var repository = new FakeScanRepository();
        var service = new ScanService(repository);
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
        Assert.Equal(ModuleStatus.Skipped, firstResult.Status);
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
}
