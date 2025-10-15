using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Nubrio.Domain.Enums;
using Nubrio.Infrastructure.Services;
using Xunit.Abstractions;

namespace Nubrio.Tests.Infrastructure.Services;

public class OpenMeteoConditionStringMapperTests
{
    private readonly Mock<ILogger<OpenMeteoConditionStringMapper>> _logger;
    private readonly OpenMeteoConditionStringMapper _mapper;

    public OpenMeteoConditionStringMapperTests(ITestOutputHelper testOutputHelper)
    {
        _logger = new Mock<ILogger<OpenMeteoConditionStringMapper>>();
        _mapper = new OpenMeteoConditionStringMapper(_logger.Object);
    }

    [Theory]
    [InlineData(WeatherConditions.Clear, "clear")]
    [InlineData(WeatherConditions.PartlyCloudy, "partly cloudy")]
    [InlineData(WeatherConditions.Cloudy, "cloudy")]
    [InlineData(WeatherConditions.LightRain, "light rain")]
    [InlineData(WeatherConditions.HeavyRain, "heavy rain")]
    [InlineData(WeatherConditions.LightSnow, "light snow")]
    [InlineData(WeatherConditions.HeavySnow, "heavy snow")]
    [InlineData(WeatherConditions.Thunderstorm, "thunderstorm")]
    [InlineData(WeatherConditions.Unknown, "unknown")]
    public void From_ShouldSplitPascalCase_AndReturnLowerInvariant(
        WeatherConditions input, string expected)
    {
        var result = _mapper.From(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void From_Idempotent_SameInputSameOutput()
    {
        var first = _mapper.From(WeatherConditions.PartlyCloudy);
        var second = _mapper.From(WeatherConditions.PartlyCloudy);

        Assert.Equal(first, second);
    }
    
    
    [Fact]
    public void From_UsesInvariantLowercasing_IgnoresCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            // Турецкая культура — классика для проверки регистра I/i
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            var result = _mapper.From(WeatherConditions.LightRain);

            // всё равно должно быть invariant-lowercase
            Assert.Equal("light rain", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
    
    // хелпер: вычистить статический кэш перед тестом (иначе тесты могут влиять друг на друга)
    private static void ClearInternalCache()
    {
        var field = typeof(OpenMeteoConditionStringMapper)
            .GetField("_stringWmoCache", BindingFlags.NonPublic | BindingFlags.Static);

        var cache = (ConcurrentDictionary<WeatherConditions, string>)field!.GetValue(null)!;
        cache.Clear();
    }

    [Fact]
    public void From_LogsCacheMissThenHit_OnRepeatedCalls()
    {
        ClearInternalCache();

        // 1-й вызов — кэша нет
        var _ = _mapper.From(WeatherConditions.HeavyRain);

        // 2-й вызов — уже из кэша
        _ = _mapper.From(WeatherConditions.HeavyRain);

        // Проверяем, что был лог “is not cached” ровно 1 раз
        _logger.VerifyLogContains("is not cached", Times.Once());

        // Проверяем, что был лог “is cached” (про запись в кэш) ровно 1 раз
        _logger.VerifyLogContains("is cached", Times.Once());

        // Проверяем, что был лог “already cached” ровно 1 раз (второй вызов)
        _logger.VerifyLogContains("already cached", Times.Once());
    }
}

// Удобный extension для Moq-проверки ILogger.Log(...)
internal static class LoggerMoqExtensions
{
    public static void VerifyLogContains<T>(
        this Mock<ILogger<T>> logger,
        string contains,
        Times times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(contains)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}