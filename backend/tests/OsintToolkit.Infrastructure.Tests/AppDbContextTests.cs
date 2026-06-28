using Microsoft.EntityFrameworkCore;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Infrastructure.Data;
using Xunit;

namespace OsintToolkit.Infrastructure.Tests;

public sealed class AppDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsScan()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        dbContext.Scans.Add(new Scan
        {
            Target = "example.com",
            TargetType = TargetType.Domain,
            Status = ScanStatus.Pending
        });

        await dbContext.SaveChangesAsync();

        Assert.Equal(1, await dbContext.Scans.CountAsync());
    }
}
