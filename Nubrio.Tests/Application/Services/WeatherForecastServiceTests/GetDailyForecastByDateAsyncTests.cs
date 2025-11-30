using FluentAssertions;
using FluentResults;
using Moq;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Services;
using Nubrio.Domain.Enums;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Xunit.Abstractions;

namespace Nubrio.Tests.Application.Services.WeatherForecastServiceTests;

public class GetDailyForecastByDateAsyncTests
{
    #region Setup

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WeatherForecastService _weatherForecastService;

    private readonly Mock<IForecastProvider> _weatherProviderMock;
    private readonly Mock<IGeocodingProvider> _geocodingServiceMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<IConditionStringMapper> _conditionStringMapperMock;
    private readonly Mock<ITimeZoneResolver> _timeZoneResolverMock;
    private readonly Mock<ILanguageResolver> _languageResolverMock;

    public GetDailyForecastByDateAsyncTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _weatherProviderMock = new Mock<IForecastProvider>();
        _geocodingServiceMock = new Mock<IGeocodingProvider>();
        _clockMock = new Mock<IClock>();
        _conditionStringMapperMock = new Mock<IConditionStringMapper>();
        _timeZoneResolverMock = new Mock<ITimeZoneResolver>();
        _languageResolverMock = new Mock<ILanguageResolver>();

        _weatherForecastService = new WeatherForecastService
        (
            _weatherProviderMock.Object,
            _geocodingServiceMock.Object,
            _clockMock.Object,
            _conditionStringMapperMock.Object,
            _timeZoneResolverMock.Object,
            _languageResolverMock.Object
        );
    }

    #endregion


    [Theory]
    [InlineData("Minsk", "Europe/Minsk", 17.5, WeatherConditions.HeavyRain, "heavy rain", "2025-11-20")]
    [InlineData("Москва", "Europe/Moscow", -6.9, WeatherConditions.PartlyCloudy, "partly cloudy", "2025-12-05")]
    public async Task GetDailyForecastByDateAsync_WhenCityIsValidAndDependenciesSucceed_ShouldReturnSuccessResult(
        string city, string timeZoneIana, double temperature, WeatherConditions enumCondition,
        string conditionNormalized, string date)
    {
        // Arrange

        // фиксированные времена, чтобы ассерты были детерминированными
        var fixedNowUtc = new DateTimeOffset(2025, 10, 1, 12, 00, 00, TimeSpan.Zero); // системные «сейчас»

        var dateOnly = DateOnly.Parse(date);


        // 1. Геокодинг - правильный
        var geocodingData = GetValidGeocodingData(city, timeZoneIana);


        // 2. Погода на дату
        var dailyForecast = new DailyForecastMean
        (
            dateOnly,
            geocodingData.LocationId,
            enumCondition,
            temperature
        );

        _weatherProviderMock.Setup(provider =>
                provider.GetDailyForecastMeanAsync(geocodingData, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(dailyForecast));

        // 3. Получение локального часового пояса
        var tz = TimeZoneInfo.Utc;

        _timeZoneResolverMock.Setup(tzResolver =>
                tzResolver.GetTimeZoneInfoById(timeZoneIana))
            .Returns(Result.Ok(tz));

        _clockMock.Setup(clock => clock.UtcNow).Returns(fixedNowUtc);


        // Ожидаемые локальные времена
        var expectedFetchedLocal = TimeZoneInfo.ConvertTime(fixedNowUtc, tz);

        // 4. Перевод в DTO - настройка ConditionStringMapper
        _conditionStringMapperMock.Setup(mapper =>
                mapper.From(enumCondition))
            .Returns(conditionNormalized);


        // Act
        var result = await _weatherForecastService.GetDailyForecastByDateAsync(city, dateOnly, CancellationToken.None);
        var dto = result.Value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        dto.City.Should().Be(city);
        dto.Conditions[0].Should().Be(conditionNormalized);
        dto.TemperaturesMean[0].Should().Be(temperature);

        dto.Dates[0].Should().Be(dateOnly);
        dto.FetchedAt.Should().Be(expectedFetchedLocal);

        _geocodingServiceMock.Verify(x => x.ResolveAsync(city, "en", It.IsAny<CancellationToken>()), Times.Once);
        _weatherProviderMock.Verify(
            x => x.GetDailyForecastMeanAsync(geocodingData, dateOnly, It.IsAny<CancellationToken>()),
            Times.Once);
        _timeZoneResolverMock.Verify(x => x.GetTimeZoneInfoById(timeZoneIana), Times.Once);
        _conditionStringMapperMock.Verify(x => x.From(enumCondition), Times.Once);
        _languageResolverMock.Verify(x => x.Resolve(city), Times.Once);

        _testOutputHelper.WriteLine($"condition: {result.Value.Conditions[0]}");
        _testOutputHelper.WriteLine($"city: {result.Value.City}");
        _testOutputHelper.WriteLine($"temperature: {result.Value.TemperaturesMean[0]}");
        _testOutputHelper.WriteLine($"fetch time: {result.Value.FetchedAt}");
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetDailyForecastByDateAsync_WhenCityIsEmpty_ShouldReturnFailResult(string emptyCity)
    {
        // Arrange 
        var dateOnly = DateOnly.Parse("2020-10-20");

        // Act
        var result =
            await _weatherForecastService.GetDailyForecastByDateAsync(emptyCity, dateOnly, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("City cannot be null or whitespace");

        _languageResolverMock.Verify(x => x.Resolve(It.IsAny<string>()), Times.Never);

        _geocodingServiceMock.Verify(geocode =>
            geocode.ResolveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);


        _testOutputHelper.WriteLine($"city: {emptyCity} is null or empty or whitespace");
    }

    [Fact]
    public async Task GetDailyForecastByDateAsync_WhenGeocodingFails_ShouldReturnFailResult()
    {
        // Arrange 
        const string city = "MockCity";
        var dateOnly = DateOnly.Parse("2020-10-20");

        // 1. Геокодинг - с ошибкой
        _languageResolverMock.Setup(l => l.Resolve(city)).Returns("en");

        _geocodingServiceMock.Setup(geocode =>
                geocode.ResolveAsync(city, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("City cannot be found"));


        // Act
        var result = await _weatherForecastService.GetDailyForecastByDateAsync(city, dateOnly, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("City cannot be found");

        _weatherProviderMock.Verify(provider =>
            provider.GetDailyForecastMeanAsync(
                It.IsAny<Location>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Minsk", "Europe/Minsk", WeatherConditions.HeavyRain, "2025-11-20")]
    [InlineData("Москва", "Europe/Moscow", WeatherConditions.PartlyCloudy, "2025-12-05")]
    public async Task GetDailyForecastByDateAsync_WhenWeatherProviderFails_ShouldReturnFailResult(
        string city, string timeZoneIana, WeatherConditions enumCondition, string date)
    {
        // Arrange
        var dateOnly = DateOnly.Parse(date);


        // 1. Геокодинг - правильный
        var geocodingData = GetValidGeocodingData(city, timeZoneIana);


        // Провайдер погоды возвращает Error
        _weatherProviderMock.Setup(provider =>
                provider.GetDailyForecastMeanAsync(geocodingData, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("OpenMeteo error"));


        // Act
        var result = await _weatherForecastService.GetDailyForecastByDateAsync(city, dateOnly, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("OpenMeteo error");

        _timeZoneResolverMock.Verify(x => x.GetTimeZoneInfoById(timeZoneIana), Times.Never);
        _conditionStringMapperMock.Verify(x => x.From(enumCondition), Times.Never);
        _clockMock.Verify(x => x.UtcNow, Times.Never);
    }

    [Theory]
    [InlineData("Minsk", "Europe/Minsk", 17.5, WeatherConditions.HeavyRain, "2025-11-20")]
    [InlineData("Москва", "Europe/Moscow", -6.9, WeatherConditions.PartlyCloudy, "2025-12-05")]
    public async Task WhenTimeZoneResolverFails_ShouldReturnFailAndSkipOtherDependencies(
        string city, string timeZoneIana, double temperature, WeatherConditions enumCondition, string date)
    {
        // Arrange
        var dateOnly = DateOnly.Parse(date);

        // 1. Геокодинг - правильный
        var geocodingData = GetValidGeocodingData(city, timeZoneIana);


        // 2. Погода на дату
        var dailyForecast = new DailyForecastMean
        (
            dateOnly,
            geocodingData.LocationId,
            enumCondition,
            temperature
        );

        _weatherProviderMock.Setup(provider =>
                provider.GetDailyForecastMeanAsync(geocodingData, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(dailyForecast));

        // 3. timeZoneResolver выдает ошибку
        _timeZoneResolverMock.Setup(tzResolver =>
                tzResolver.GetTimeZoneInfoById(timeZoneIana))
            .Returns(Result.Fail("TimeZoneResolver error"));

        // Act
        var result = await _weatherForecastService.GetDailyForecastByDateAsync(city, dateOnly, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("TimeZoneResolver error");


        _timeZoneResolverMock.Verify(x => x.GetTimeZoneInfoById(It.IsAny<string>()), Times.Once);
        _clockMock.Verify(x => x.UtcNow, Times.Never);
        _conditionStringMapperMock.Verify(x => x.From(It.IsAny<WeatherConditions>()), Times.Never);
    }


    #region Helpers

    private Location GetValidGeocodingData(string city, string timeZoneIana)
    {
        var geocodingData = new Location
            (Guid.NewGuid(), city, new Coordinates(50, 100), timeZoneIana);


        _languageResolverMock.Setup(l => l.Resolve(city)).Returns("en");

        _geocodingServiceMock.Setup(geocode =>
                geocode.ResolveAsync(city, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(geocodingData));
        return geocodingData;
    }

    #endregion
}