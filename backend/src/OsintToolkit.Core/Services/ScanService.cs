using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Exceptions;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Core.Modules;

namespace OsintToolkit.Core.Services;

public sealed class ScanService : IScanService
{
    private readonly IScanRepository _scanRepository;
    private readonly Func<string, IOSINTModule?> _moduleResolver;

    public ScanService(IScanRepository scanRepository, Func<string, IOSINTModule?>? moduleResolver = null)
    {
        _scanRepository = scanRepository;
        _moduleResolver = moduleResolver ?? ModuleRegistry.Resolve;
    }

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
            Status = ScanStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            StartedAt = DateTimeOffset.UtcNow,
            Notes = "Sprint 2 - Real OSINT Modules"
        };

        var moduleNames = (modules ?? new List<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (moduleNames.Count > 0)
        {
            foreach (var moduleName in moduleNames)
            {
                if (!ModuleRegistry.IsKnown(moduleName))
                {
                    scan.Results.Add(CreateResult(scan.Id, moduleName, ModuleStatus.Failed,
                        $"Unknown module '{moduleName}'", "{}"));
                    continue;
                }

                var runner = _moduleResolver(moduleName);
                if (runner == null)
                {
                    scan.Results.Add(CreateResult(scan.Id, moduleName, ModuleStatus.Skipped,
                        $"Module '{moduleName}' is recognized but not implemented yet", "{}"));
                    continue;
                }

                var result = await runner.ExecuteAsync(scan.Target, scan.TargetType, cancellationToken).ConfigureAwait(false);
                scan.Results.Add(CreateResult(scan.Id, moduleName, result.Status, result.Summary, result.RawData));
            }
        }

        scan.Status = scan.Results.Any(r => r.Status == ModuleStatus.Failed)
            ? ScanStatus.Failed
            : ScanStatus.Completed;
        scan.CompletedAt = DateTimeOffset.UtcNow;

        await _scanRepository.AddAsync(scan, cancellationToken);
        await _scanRepository.SaveChangesAsync(cancellationToken);

        return scan;
    }

    public async Task<IReadOnlyList<Scan>> GetScansAsync(CancellationToken cancellationToken = default)
    {
        return await _scanRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Scan> GetScanByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scan = await _scanRepository.GetByIdAsync(id, cancellationToken);
        if (scan == null)
        {
            throw new NotFoundException($"Scan with ID '{id}' was not found.");
        }
        return scan;
    }

    public async Task DeleteScanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scan = await _scanRepository.GetByIdAsync(id, cancellationToken);
        if (scan == null)
        {
            throw new NotFoundException($"Scan with ID '{id}' was not found.");
        }

        _scanRepository.Delete(scan);
        await _scanRepository.SaveChangesAsync(cancellationToken);
    }

    private static ScanResult CreateResult(Guid scanId, string moduleName, ModuleStatus status, string? summary, string rawData)
    {
        return new ScanResult
        {
            Id = Guid.NewGuid(),
            ScanId = scanId,
            ModuleName = moduleName,
            Status = status,
            Summary = summary,
            RawData = rawData,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
