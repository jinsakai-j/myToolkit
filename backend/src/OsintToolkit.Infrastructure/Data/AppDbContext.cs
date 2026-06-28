using Microsoft.EntityFrameworkCore;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Interfaces;

namespace OsintToolkit.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Scan> Scans => Set<Scan>();
    public DbSet<ScanResult> ScanResults => Set<ScanResult>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

