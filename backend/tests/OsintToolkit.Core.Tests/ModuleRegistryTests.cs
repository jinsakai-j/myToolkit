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
        Assert.IsType<UsernameCheckerModule>(ModuleRegistry.Resolve("UsernameChecker"));
        Assert.IsType<IpReputationModule>(ModuleRegistry.Resolve("IpReputation"));
    }

    [Fact]
    public void Resolve_ReturnsNullForUnknownModule()
    {
        Assert.Null(ModuleRegistry.Resolve("TotallyUnknown"));
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
    public async Task IpReputationModule_WithInvalidIp_ReturnsFailedWithoutNetwork()
    {
        var result = await new IpReputationModule().ExecuteAsync("999.1.1.1", TargetType.IpAddress);

        Assert.Equal(ModuleStatus.Failed, result.Status);
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
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new UsernameCheckerModule().ExecuteAsync("someuser", TargetType.Username, cts.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new IpReputationModule().ExecuteAsync("8.8.8.8", TargetType.IpAddress, cts.Token));
    }
}