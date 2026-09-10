using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OsintToolkit.Api.Contracts.Requests;
using OsintToolkit.Api.Contracts.Responses;
using OsintToolkit.Core.Enums;
using Xunit;

namespace OsintToolkit.Api.Tests;

public sealed class ReportEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _tempReportDir;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ReportEndpointTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        _tempReportDir = Path.Combine(Path.GetTempPath(), "report-api-tests-" + Guid.NewGuid().ToString("N"));
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Report:OutputDirectory", _tempReportDir);
        });
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempReportDir, recursive: true);
        }
        catch
        {
            // best effort cleanup
        }
    }

    [Fact]
    public async Task GenerateReport_ReturnsCreatedWithPdf()
    {
        using var client = _factory.CreateClient();

        var scanResponse = await client.PostAsJsonAsync("/api/scans", new CreateScanRequest
        {
            Target = "example.com",
            TargetType = TargetType.Domain,
            Modules = new List<string>()
        }, SerializerOptions);
        var scan = await scanResponse.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(scan);

        var response = await client.PostAsJsonAsync($"/api/scans/{scan.Id}/reports", new GenerateReportRequest(), SerializerOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<ReportResponse>(SerializerOptions);
        Assert.NotNull(report);
        Assert.Equal(scan.Id, report.ScanId);
        Assert.EndsWith(".pdf", report.FileName);
    }

    [Fact]
    public async Task GetReports_WhenScanHasReport_ReturnsIt()
    {
        using var client = _factory.CreateClient();

        var scanResponse = await client.PostAsJsonAsync("/api/scans", new CreateScanRequest
        {
            Target = "example.com",
            TargetType = TargetType.Domain,
            Modules = new List<string>()
        }, SerializerOptions);
        var scan = await scanResponse.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(scan);

        await client.PostAsJsonAsync($"/api/scans/{scan.Id}/reports", new GenerateReportRequest(), SerializerOptions);

        var response = await client.GetAsync($"/api/scans/{scan.Id}/reports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reports = await response.Content.ReadFromJsonAsync<List<ReportResponse>>(SerializerOptions);
        Assert.NotNull(reports);
        Assert.NotEmpty(reports);
    }

    [Fact]
    public async Task DownloadReport_ReturnsPdfContentType()
    {
        using var client = _factory.CreateClient();

        var scanResponse = await client.PostAsJsonAsync("/api/scans", new CreateScanRequest
        {
            Target = "example.com",
            TargetType = TargetType.Domain,
            Modules = new List<string>()
        }, SerializerOptions);
        var scan = await scanResponse.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(scan);

        var createResponse = await client.PostAsJsonAsync($"/api/scans/{scan.Id}/reports", new GenerateReportRequest(), SerializerOptions);
        var report = await createResponse.Content.ReadFromJsonAsync<ReportResponse>(SerializerOptions);
        Assert.NotNull(report);

        var downloadResponse = await client.GetAsync($"/api/reports/{report.Id}/download");

        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        Assert.Equal("application/pdf", downloadResponse.Content.Headers.ContentType?.MediaType);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 1000, "PDF content should be generated");
    }

    [Fact]
    public async Task GenerateReport_ForMissingScan_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/scans/{Guid.NewGuid()}/reports", new GenerateReportRequest(), SerializerOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}