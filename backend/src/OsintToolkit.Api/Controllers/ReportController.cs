using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OsintToolkit.Api.Contracts.Requests;
using OsintToolkit.Api.Contracts.Responses;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Interfaces;

namespace OsintToolkit.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ReportController(IReportService reportService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("scans/{scanId:guid}/reports")]
    public async Task<ActionResult<ReportResponse>> GenerateReport(
        [FromRoute] Guid scanId,
        [FromBody] GenerateReportRequest? request,
        CancellationToken cancellationToken)
    {
        var outputDirectory = configuration["Report:OutputDirectory"] ?? "reports/generated";
        var report = await reportService.GeneratePdfAsync(
            scanId,
            outputDirectory,
            request?.FileName,
            cancellationToken);

        return CreatedAtAction(nameof(DownloadReport), new { reportId = report.Id }, MapToReportResponse(report));
    }

    [HttpGet("scans/{scanId:guid}/reports")]
    public async Task<ActionResult<IReadOnlyList<ReportResponse>>> GetReports(
        [FromRoute] Guid scanId,
        CancellationToken cancellationToken)
    {
        var reports = await reportService.GetReportsAsync(scanId, cancellationToken);
        return Ok(reports.Select(MapToReportResponse).ToList());
    }

    [HttpGet("reports/{reportId:guid}/download")]
    public async Task<ActionResult> DownloadReport([FromRoute] Guid reportId, CancellationToken cancellationToken)
    {
        var report = await reportService.GetReportByIdAsync(reportId, cancellationToken);
        if (!System.IO.File.Exists(report.FilePath))
        {
            return NotFound(new { error = "Report file no longer exists on disk." });
        }

        var stream = System.IO.File.OpenRead(report.FilePath);
        return File(stream, "application/pdf", report.FileName);
    }

    private static ReportResponse MapToReportResponse(Report report)
    {
        return new ReportResponse
        {
            Id = report.Id,
            ScanId = report.ScanId,
            FileName = report.FileName,
            GeneratedAt = report.GeneratedAt
        };
    }
}