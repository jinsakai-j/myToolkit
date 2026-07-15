using Microsoft.EntityFrameworkCore;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Infrastructure.Data;
using OsintToolkit.Infrastructure.Repositories;
using Xunit;

namespace OsintToolkit.Infrastructure.Tests;

public sealed class ScanRepositoryTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAndGetById_PersistsAndLoadsScanWithResults()
    {
        await using var dbContext = CreateDbContext();
        var repository = new ScanRepository(dbContext);

        var scan = new Scan
        {
            Target = "test@example.com",
            TargetType = TargetType.Email,
            Status = ScanStatus.Completed
        };
        scan.Results.Add(new ScanResult
        {
            ModuleName = "DnsLookup",
            Status = ModuleStatus.Skipped
        });

        await repository.AddAsync(scan);
        await repository.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(scan.Id);

        Assert.NotNull(loaded);
        Assert.Equal("test@example.com", loaded.Target);
        Assert.Equal(TargetType.Email, loaded.TargetType);
        Assert.Single(loaded.Results);
        Assert.Equal("DnsLookup", loaded.Results.First().ModuleName);
    }

    [Fact]
    public async Task GetAll_ReturnsAllScansOrderedByCreatedAtDescending()
    {
        await using var dbContext = CreateDbContext();
        var repository = new ScanRepository(dbContext);

        var scan1 = new Scan { Target = "1.1.1.1", TargetType = TargetType.IpAddress, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5) };
        var scan2 = new Scan { Target = "2.2.2.2", TargetType = TargetType.IpAddress, CreatedAt = DateTimeOffset.UtcNow };

        await repository.AddAsync(scan1);
        await repository.AddAsync(scan2);
        await repository.SaveChangesAsync();

        var list = await repository.GetAllAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal("2.2.2.2", list[0].Target); // Should be first (descending)
        Assert.Equal("1.1.1.1", list[1].Target);
    }

    [Fact]
    public async Task Delete_RemovesScan()
    {
        await using var dbContext = CreateDbContext();
        var repository = new ScanRepository(dbContext);

        var scan = new Scan { Target = "username", TargetType = TargetType.Username };
        await repository.AddAsync(scan);
        await repository.SaveChangesAsync();

        repository.Delete(scan);
        await repository.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(scan.Id);
        Assert.Null(loaded);
    }
}
