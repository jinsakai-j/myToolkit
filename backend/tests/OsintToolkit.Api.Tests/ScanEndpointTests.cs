using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using OsintToolkit.Api.Contracts.Requests;
using OsintToolkit.Api.Contracts.Responses;
using OsintToolkit.Core.Enums;
using Xunit;

namespace OsintToolkit.Api.Tests;

public sealed class ScanEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ScanEndpointTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        _factory = factory;
    }

    [Fact]
    public async Task CreateScan_WithValidPayload_ReturnsCreated()
    {
        using var client = _factory.CreateClient();
        var request = new CreateScanRequest
        {
            Target = "example.com",
            TargetType = TargetType.Domain,
            Modules = new List<string> { "DnsLookup" }
        };

        var response = await client.PostAsJsonAsync("/api/scans", request, SerializerOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var scan = await response.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(scan);
        Assert.Equal("example.com", scan.Target);
        Assert.Equal(TargetType.Domain, scan.TargetType);
        Assert.Equal(ScanStatus.Completed, scan.Status);
    }

    [Fact]
    public async Task CreateScan_WithInvalidPayload_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var request = new CreateScanRequest
        {
            Target = "invalid-domain",
            TargetType = TargetType.Domain,
            Modules = new List<string>()
        };

        var response = await client.PostAsJsonAsync("/api/scans", request, SerializerOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetScans_ReturnsScanList()
    {
        using var client = _factory.CreateClient();
        
        // Seed a scan first
        var request = new CreateScanRequest
        {
            Target = "1.1.1.1",
            TargetType = TargetType.IpAddress
        };
        await client.PostAsJsonAsync("/api/scans", request, SerializerOptions);

        var response = await client.GetAsync("/api/scans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var scans = await response.Content.ReadFromJsonAsync<List<ScanResponse>>(SerializerOptions);
        Assert.NotNull(scans);
        Assert.NotEmpty(scans);
    }

    [Fact]
    public async Task GetScanDetail_WhenExists_ReturnsScanDetail()
    {
        using var client = _factory.CreateClient();

        // Seed a scan first
        var request = new CreateScanRequest
        {
            Target = "user_name",
            TargetType = TargetType.Username,
            Modules = new List<string> { "UsernameChecker" }
        };
        var createResponse = await client.PostAsJsonAsync("/api/scans", request, SerializerOptions);
        var createdScan = await createResponse.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(createdScan);

        var response = await client.GetAsync($"/api/scans/{createdScan.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ScanDetailResponse>(SerializerOptions);
        Assert.NotNull(detail);
        Assert.Equal("user_name", detail.Target);
        Assert.Single(detail.Results);
        Assert.Equal("UsernameChecker", detail.Results[0].ModuleName);
    }

    [Fact]
    public async Task GetScanDetail_WhenNotExists_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/scans/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteScan_WhenExists_ReturnsNoContent()
    {
        using var client = _factory.CreateClient();

        // Seed a scan first
        var request = new CreateScanRequest
        {
            Target = "test@example.com",
            TargetType = TargetType.Email
        };
        var createResponse = await client.PostAsJsonAsync("/api/scans", request, SerializerOptions);
        var createdScan = await createResponse.Content.ReadFromJsonAsync<ScanResponse>(SerializerOptions);
        Assert.NotNull(createdScan);

        var deleteResponse = await client.DeleteAsync($"/api/scans/{createdScan.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it was deleted
        var getResponse = await client.GetAsync($"/api/scans/{createdScan.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
