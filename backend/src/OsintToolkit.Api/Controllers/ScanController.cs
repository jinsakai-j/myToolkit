using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OsintToolkit.Api.Contracts.Requests;
using OsintToolkit.Api.Contracts.Responses;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Interfaces;

namespace OsintToolkit.Api.Controllers;

[ApiController]
[Route("api/scans")]
public sealed class ScanController(IScanService scanService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ScanResponse>> CreateScan([FromBody] CreateScanRequest request, CancellationToken cancellationToken)
    {
        var scan = await scanService.CreateScanAsync(request.Target, request.TargetType, request.Modules, cancellationToken);
        return CreatedAtAction(nameof(GetScanDetail), new { scanId = scan.Id }, MapToScanResponse(scan));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ScanResponse>>> GetScans(
        [FromQuery] TargetType? targetType,
        [FromQuery] ScanStatus? status,
        CancellationToken cancellationToken)
    {
        var scans = await scanService.GetScansAsync(cancellationToken);

        var filteredScans = scans.AsEnumerable();
        if (targetType.HasValue)
        {
            filteredScans = filteredScans.Where(s => s.TargetType == targetType.Value);
        }
        if (status.HasValue)
        {
            filteredScans = filteredScans.Where(s => s.Status == status.Value);
        }

        return Ok(filteredScans.Select(MapToScanResponse).ToList());
    }

    [HttpGet("{scanId:guid}")]
    public async Task<ActionResult<ScanDetailResponse>> GetScanDetail([FromRoute] Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await scanService.GetScanByIdAsync(scanId, cancellationToken);
        return Ok(MapToScanDetailResponse(scan));
    }

    [HttpDelete("{scanId:guid}")]
    public async Task<IActionResult> DeleteScan([FromRoute] Guid scanId, CancellationToken cancellationToken)
    {
        await scanService.DeleteScanAsync(scanId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{scanId:guid}/notes")]
    public async Task<ActionResult<ScanResponse>> UpdateScanNotes([FromRoute] Guid scanId, [FromBody] UpdateScanNotesRequest request, CancellationToken cancellationToken)
    {
        var scan = await scanService.UpdateNotesAsync(scanId, request.Notes, cancellationToken);
        return Ok(MapToScanResponse(scan));
    }

    [HttpGet("{scanId:guid}/export")]
    public async Task<IActionResult> ExportScanJson([FromRoute] Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await scanService.GetScanByIdAsync(scanId, cancellationToken);
        var payload = MapToScanDetailResponse(scan);
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        });

        return File(
            Encoding.UTF8.GetBytes(json),
            "application/json",
            $"scan-{SanitizeFileName(scan.Target)}.json");
    }

    private static string SanitizeFileName(string target)
    {
        var sanitized = new StringBuilder(target.Length);
        foreach (var c in target)
        {
            sanitized.Append(char.IsLetterOrDigit(c) || c is '.' or '-' ? c : '_');
        }
        return sanitized.ToString();
    }

    private static ScanResponse MapToScanResponse(Scan scan)
    {
        return new ScanResponse
        {
            Id = scan.Id,
            Target = scan.Target,
            TargetType = scan.TargetType,
            Status = scan.Status,
            RiskScore = scan.RiskScore,
            CreatedAt = scan.CreatedAt,
            StartedAt = scan.StartedAt,
            CompletedAt = scan.CompletedAt,
            Notes = scan.Notes
        };
    }

    private static ScanResultResponse MapToScanResultResponse(ScanResult result)
    {
        return new ScanResultResponse
        {
            Id = result.Id,
            ScanId = result.ScanId,
            ModuleName = result.ModuleName,
            Status = result.Status,
            Summary = result.Summary,
            RawData = result.RawData,
            CreatedAt = result.CreatedAt
        };
    }

    private static ScanDetailResponse MapToScanDetailResponse(Scan scan)
    {
        return new ScanDetailResponse
        {
            Id = scan.Id,
            Target = scan.Target,
            TargetType = scan.TargetType,
            Status = scan.Status,
            RiskScore = scan.RiskScore,
            CreatedAt = scan.CreatedAt,
            StartedAt = scan.StartedAt,
            CompletedAt = scan.CompletedAt,
            Notes = scan.Notes,
            Results = scan.Results.Select(MapToScanResultResponse).ToList()
        };
    }
}
