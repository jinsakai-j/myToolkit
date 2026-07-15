using Microsoft.EntityFrameworkCore;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Infrastructure.Data;

namespace OsintToolkit.Infrastructure.Repositories;

public sealed class ScanRepository(AppDbContext dbContext) : IScanRepository
{
    public async Task<Scan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Scans
            .Include(s => s.Results)
            .Include(s => s.Reports)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Scan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Scans
            .Include(s => s.Results)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Scan scan, CancellationToken cancellationToken = default)
    {
        await dbContext.Scans.AddAsync(scan, cancellationToken);
    }

    public void Delete(Scan scan)
    {
        dbContext.Scans.Remove(scan);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
