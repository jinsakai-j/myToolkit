using System.ComponentModel.DataAnnotations;
using OsintToolkit.Core.Enums;

namespace OsintToolkit.Api.Contracts.Requests;

public sealed class CreateScanRequest
{
    [Required]
    public string Target { get; set; } = string.Empty;

    [Required]
    public TargetType TargetType { get; set; }

    public List<string> Modules { get; set; } = new();
}
