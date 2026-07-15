using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;

namespace OsintToolkit.Core.Services;

public sealed class ScanService(IScanRepository scanRepository) : IScanService
{
    public async Task<Scan> CreateScanAsync(string target, TargetType targetType, List<string> modules, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ValidationException("Target cannot be empty.");
        }

        if (!TargetClassifier.IsSupported(targetType))
        {
            throw new ValidationException($"Target type '{targetType}' is not supported.");
        }

        if (!TargetClassifier.IsValid(target, targetType))
        {
            throw new ValidationException($"Invalid target format '{target}' for type '{targetType}'.");
        }

        var scan = new Scan
        {
            Id = Guid.NewGuid(),
            Target = target.Trim(),
            TargetType = targetType,
            Status = ScanStatus.Completed, // Completed immediately for Sprint 1 MVP since no real lookup runs
            CreatedAt = DateTimeOffset.UtcNow,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Notes = "Sprint 1 MVP - Placeholder Results"
        };

        if (modules != null && modules.Count > 0)
        {
            foreach (var module in modules)
            {
                scan.Results.Add(new ScanResult
                {
                    Id = Guid.NewGuid(),
                    ScanId = scan.Id,
                    ModuleName = module,
                    Status = ModuleStatus.Skipped, // Skipped because module isn't implemented yet
                    Summary = $"Module '{module}' not implemented yet (Sprint 1 MVP)",
                    RawData = "{}",
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await scanRepository.AddAsync(scan, cancellationToken);
        await scanRepository.SaveChangesAsync(cancellationToken);

        return scan;
    }

    public async Task<IReadOnlyList<Scan>> GetScansAsync(CancellationToken cancellationToken = default)
    {
        return await scanRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Scan> GetScanByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scan = await scanRepository.GetByIdAsync(id, cancellationToken);
        if (scan == null)
        {
            throw new NotFoundException($"Scan with ID '{id}' was not found.");
        }
        return scan;
    }

    public async Task DeleteScanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scan = await scanRepository.GetByIdAsync(id, cancellationToken);
        if (scan == null)
        {
            throw new NotFoundException($"Scan with ID '{id}' was not found.");
        }

        scanRepository.Delete(scan);
        await scanRepository.SaveChangesAsync(cancellationToken);
    }
}
