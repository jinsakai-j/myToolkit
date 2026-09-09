using OsintToolkit.Core.Enums;
using OsintToolkit.Core.Modules;
using Xunit;

namespace OsintToolkit.Core.Tests;

public sealed class ModuleRegistryTests
{
    [Theory]
    [InlineData("DnsLookup", true)]
    [InlineData("DNSSLookup", false)]
    [InlineData("dnslookup", true)]
    [InlineData("WhoisLookup", true)]
    [InlineData("EmailValidation", true)]
    [InlineData("UsernameChecker", true)]
    [InlineData("IpReputation", true)]
    [InlineData("Nope", false)]
    public void IsKnown_MatchesFrontendModuleIds(string moduleName, bool expected)
    {
        Assert.Equal(expected, ModuleRegistry.IsKnown(moduleName));
    }

    [Fact]
    public void Resolve_ReturnsImplementedModules()
    {
        Assert.IsType<DnsLookupModule>(ModuleRegistry.Resolve("DnsLookup"));
        Assert.IsType<WhoisLookupModule>(ModuleRegistry.Resolve("WhoisLookup"));
        Assert.IsType<EmailValidationModule>(ModuleRegistry.Resolve("EmailValidation"));
    }

    [Fact]
    public void Resolve_ReturnsNullForRecognizedButNotImplementedModules()
    {
        Assert.Null(ModuleRegistry.Resolve("UsernameChecker"));
        Assert.Null(ModuleRegistry.Resolve("IpReputation"));
    }

    [Theory]
    [InlineData(TargetType.Domain, new[] { "DnsLookup", "WhoisLookup" })]
    [InlineData(TargetType.Email, new[] { "EmailValidation" })]
    [InlineData(TargetType.Username, new[] { "UsernameChecker" })]
    [InlineData(TargetType.IpAddress, new[] { "DnsLookup", "IpReputation" })]
    public void SupportedFor_MatchesFrontendModuleDefinitions(TargetType targetType, string[] expected)
    {
        Assert.Equal(expected, ModuleRegistry.SupportedFor(targetType));
    }

    [Fact]
    public async Task Modules_RespectCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DnsLookupModule().ExecuteAsync("example.com", TargetType.Domain, cts.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new WhoisLookupModule().ExecuteAsync("example.com", TargetType.Domain, cts.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new EmailValidationModule().ExecuteAsync("user@example.com", TargetType.Email, cts.Token));
    }
}