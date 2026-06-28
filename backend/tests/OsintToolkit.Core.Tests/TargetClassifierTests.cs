using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Services;
using Xunit;

namespace OsintToolkit.Core.Tests;

public sealed class TargetClassifierTests
{
    [Theory]
    [InlineData(TargetType.Domain)]
    [InlineData(TargetType.Email)]
    [InlineData(TargetType.Username)]
    [InlineData(TargetType.IpAddress)]
    public void IsSupported_ReturnsTrue_ForSupportedTargetTypes(TargetType targetType)
    {
        Assert.True(TargetClassifier.IsSupported(targetType));
    }
}
