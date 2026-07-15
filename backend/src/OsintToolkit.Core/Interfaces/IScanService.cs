using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Interfaces;

public interface IScanService
{
    Task<Scan> CreateScanAsync(string target, TargetType targetType, List<string> modules, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Scan>> GetScansAsync(CancellationToken cancellationToken = default);
    Task<Scan> GetScanByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteScanAsync(Guid id, CancellationToken cancellationToken = default);
}
