using Microsoft.EntityFrameworkCore;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Infrastructure.Data;

namespace OsintToolkit.Infrastructure.Repositories;

public sealed class ReportRepository(AppDbContext dbContext) : IReportRepository
{
    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reports.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetByScanIdAsync(Guid scanId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reports
            .Where(r => r.ScanId == scanId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await dbContext.Reports.AddAsync(report, cancellationToken);
    }

    public void Delete(Report report)
    {
        dbContext.Reports.Remove(report);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}