using OsintToolkit.Core.Entities;

namespace OsintToolkit.Core.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetByScanIdAsync(Guid scanId, CancellationToken cancellationToken = default);
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    void Delete(Report report);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}