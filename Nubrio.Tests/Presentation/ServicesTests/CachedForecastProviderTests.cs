using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Nubrio.Application.Common;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Nubrio.Infrastructure.Providers.CacheProvider;

namespace Nubrio.Tests.Presentation.ServicesTests;

public class CachedForecastProviderTests
{
    private readonly CachedForecastProvider _cacheForecastProvider;

    private readonly Mock<IWeatherForecastCache> _cache;
    private readonly string _providerKey;
    private readonly Mock<IForecastProvider> _forecastProvider;
    private readonly Mock<IClock> _clock;
    private readonly Mock<ICacheHitAccessor> _accessor;
    private readonly Mock<ILogger<CachedForecastProvider>> _logger;

    public CachedForecastProviderTests()
    {
        _cache = new Mock<IWeatherForecastCache>();
        _providerKey = "Test-Provider";
        _forecastProvider = new Mock<IForecastProvider>();
        _clock = new Mock<IClock>();
        _accessor = new Mock<ICacheHitAccessor>();
        _logger = new Mock<ILogger<CachedForecastProvider>>();

        _cacheForecastProvider = new CachedForecastProvider(
            _cache.Object,
            _forecastProvider.Object,
            _providerKey,
            _logger.Object,
            _clock.Object,
            _accessor.Object);
    }

    [Fact]
    public async Task GetDailyForecast_IfCacheHitTrue_ReturnsCache()
    {
        // Arrange
        var location = GetLocation();
        var date = new DateOnly(2025, 12, 22);
        var dateUtc = DateTimeOffset.Parse("2025-12-22T10:00:00Z");
        var cached = new DailyForecastMean(
            date: date,
            locationId: location.Id,
            condition: WeatherConditions.Clear,
            temperatureMean: 10,
            fetchedAtUtc: dateUtc);

        _cache.Setup(x => x.GetDailyAsync(_providerKey, location.ExternalLocationId.Value, date)).ReturnsAsync(cached);

        // Act
        var result = await _cacheForecastProvider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(cached);

        _forecastProvider.Verify(x =>
                x.GetDailyForecastMeanAsync(It.IsAny<Location>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _accessor.Verify(x => x.SetCacheHit(true), Times.Once);
    }


    [Fact]
    public async Task GetDailyForecast_IfCacheHitFalse_ReturnsNull()
    {
        var location = GetLocation();
        var date = new DateOnly(2025, 12, 22);
        var dateUtc = DateTimeOffset.Parse("2025-12-22T10:00:00Z");

        var forecast = new DailyForecastMean(
            date: date,
            locationId: location.Id,
            condition: WeatherConditions.Clear,
            temperatureMean: 10,
            fetchedAtUtc: dateUtc);

        _cache.Setup(x =>
                x.GetDailyAsync(_providerKey, location.ExternalLocationId.Value, date))
            .ReturnsAsync((DailyForecastMean?)null);

        _forecastProvider.Setup(x => x.GetDailyForecastMeanAsync(location, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(forecast));

        // Act
        var result = await _cacheForecastProvider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);


        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(forecast);

        _forecastProvider.Verify(x =>
                x.GetDailyForecastMeanAsync(It.IsAny<Location>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _accessor.Verify(x => x.SetCacheHit(false), Times.Once);
        
        _cache.Verify(x => x.SetDailyAsync(forecast, _providerKey, location.ExternalLocationId.Value, date), Times.Once);
    }

    [Fact]
    public async Task GetDailyForecast_IfCacheHitFalse_And_ProviderFail_CacheHasNotBeenWritten()
    {
        var location = GetLocation();
        var date = new DateOnly(2025, 12, 22);
        var dateUtc = DateTimeOffset.Parse("2025-12-22T10:00:00Z");

        var forecast = new DailyForecastMean(
            date: date,
            locationId: location.Id,
            condition: WeatherConditions.Clear,
            temperatureMean: 10,
            fetchedAtUtc: dateUtc);

        _cache.Setup(x =>
                x.GetDailyAsync(_providerKey, location.ExternalLocationId.Value, date))
            .ReturnsAsync((DailyForecastMean?)null);

        _forecastProvider.Setup(x => x.GetDailyForecastMeanAsync(location, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<DailyForecastMean>(new Error("some error")));
        
        
        // Act
        var result = await _cacheForecastProvider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);


        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("some error");
        
        _cache.Verify(x => 
                x.SetDailyAsync(It.IsAny<DailyForecastMean>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Never);
    }


    private Location GetLocation()
    {
        return new Location
        (Guid.NewGuid(),
            "city",
            new Coordinates(50, 100),
            "timeZoneIana",
            new ExternalLocationId("test-provider", "00001"));
    }
}