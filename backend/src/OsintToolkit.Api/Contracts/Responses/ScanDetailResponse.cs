namespace OsintToolkit.Api.Contracts.Responses;

public sealed class ScanDetailResponse : ScanResponse
{
    public List<ScanResultResponse> Results { get; set; } = new();
}
