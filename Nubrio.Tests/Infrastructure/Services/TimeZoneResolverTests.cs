using Nubrio.Infrastructure.Services;

namespace Nubrio.Tests.Infrastructure.Services;

public class TimeZoneResolverTests
{
    private readonly TimeZoneResolver _resolver = new();

    [Fact]
    public void GetTimeZoneInfo_ValidId_ShouldReturnSuccess()
    {
        var result = _resolver.GetTimeZoneInfo("Europe/Moscow");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Europe/Moscow", result.Value.Id);
    }

    [Fact]
    public void GetTimeZoneInfo_RepeatedCall_ShouldReturnFromCache()
    {
        var first = _resolver.GetTimeZoneInfo("Europe/Moscow");
        var second = _resolver.GetTimeZoneInfo("Europe/Moscow");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Same(first.Value, second.Value); // тот же объект из кеша
    }

    [Fact]
    public void GetTimeZoneInfo_InvalidId_ShouldFail()
    {
        var result = _resolver.GetTimeZoneInfo("No/Such/Zone");

        Assert.True(result.IsFailed);
        Assert.Contains("Cannot resolve", result.Errors.First().Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void GetTimeZoneInfo_NullOrEmpty_ShouldFail(string id)
    {
        var result = _resolver.GetTimeZoneInfo(id);

        Assert.True(result.IsFailed);
        Assert.Contains("null or empty", result.Errors.First().Message);
    }

    [Fact]
    public void GetTimeZoneInfo_WithSpaces_ShouldNormalizeAndSucceed()
    {
        var result = _resolver.GetTimeZoneInfo("   Europe/Moscow   ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Europe/Moscow", result.Value.Id);
    }

    [Fact]
    public void GetTimeZoneInfo_DifferentZones_ShouldCacheBoth()
    {
        var moscow = _resolver.GetTimeZoneInfo("Europe/Moscow");
        var yekaterinburg = _resolver.GetTimeZoneInfo("Asia/Yekaterinburg");

        Assert.True(moscow.IsSuccess);
        Assert.True(yekaterinburg.IsSuccess);
        Assert.NotSame(moscow.Value, yekaterinburg.Value);
    }
}