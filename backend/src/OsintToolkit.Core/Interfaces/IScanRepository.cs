using OsintToolkit.Core.Entities;

namespace OsintToolkit.Core.Interfaces;

public interface IScanRepository
{
    Task<Scan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Scan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Scan scan, CancellationToken cancellationToken = default);
    void Delete(Scan scan);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
