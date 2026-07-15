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

    [Theory]
    [InlineData("example.com", true)]
    [InlineData("sub.example.com", true)]
    [InlineData("my-domain.co.id", true)]
    [InlineData("example", false)]
    [InlineData("http://example.com", false)]
    [InlineData("example.com/path", false)]
    public void IsValid_Domain_ValidatesCorrectly(string target, bool expected)
    {
        var result = TargetClassifier.IsValid(target, TargetType.Domain);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name+label@domain.co.id", true)]
    [InlineData("test@example", false)]
    [InlineData("testexample.com", false)]
    [InlineData("@example.com", false)]
    public void IsValid_Email_ValidatesCorrectly(string target, bool expected)
    {
        var result = TargetClassifier.IsValid(target, TargetType.Email);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("john_doe", true)]
    [InlineData("john.doe-123", true)]
    [InlineData("john doe", false)] // spaces not allowed
    [InlineData("john#doe", false)] // special chars not allowed
    [InlineData("", false)]
    public void IsValid_Username_ValidatesCorrectly(string target, bool expected)
    {
        var result = TargetClassifier.IsValid(target, TargetType.Username);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("8.8.8.8", true)]
    [InlineData("2001:db8::1", true)]
    [InlineData("999.999.999.999", false)]
    [InlineData("invalid-ip", false)]
    public void IsValid_IpAddress_ValidatesCorrectly(string target, bool expected)
    {
        var result = TargetClassifier.IsValid(target, TargetType.IpAddress);
        Assert.Equal(expected, result);
    }
}
