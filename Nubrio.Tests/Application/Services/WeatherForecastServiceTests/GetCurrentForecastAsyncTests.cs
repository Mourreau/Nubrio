using FluentAssertions;
using FluentResults;
using Moq;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Services;
using Nubrio.Domain.Enums;
using Nubrio.Domain.Models;
using Xunit.Abstractions;

namespace Nubrio.Tests.Application.Services.WeatherForecastServiceTests;

public class GetCurrentForecastAsyncTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WeatherForecastService _weatherForecastService;

    private readonly Mock<IForecastProvider> _weatherProviderMock;
    private readonly Mock<IGeocodingProvider> _geocodingServiceMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<IConditionStringMapper> _conditionStringMapperMock;
    private readonly Mock<ITimeZoneResolver> _timeZoneResolverMock;
    private readonly Mock<ILanguageResolver> _languageResolverMock;

    public GetCurrentForecastAsyncTests(ITestOutputHelper testOutputHelper)
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

    [Theory]
    [InlineData("Minsk", "Europe/Minsk", 17.5, WeatherConditions.HeavyRain, "heavy rain")]
    [InlineData("Москва", "Europe/Moscow", -6.9, WeatherConditions.PartlyCloudy, "partly cloudy")]
    public async Task GetCurrentForecastAsync_WhenCityIsValidAndDependenciesSucceed_ShouldReturnSuccessResult(
        string city, string timeZoneIana, double temperature, WeatherConditions enumCondition,
        string conditionNormalized)
    {
        // Arrange

        // фиксированные времена, чтобы ассерты были детерминированными
        var observedAtUtc =
            new DateTimeOffset(2025, 9, 30, 8, 15, 0, TimeSpan.Zero); // то, что пришло из внешнего API (UTC)
        var fixedNowUtc = new DateTimeOffset(2025, 10, 1, 12, 00, 00, TimeSpan.Zero); // системные «сейчас»


        // 1. Геокодинг - правильный
        var geocodingData = new Location
            (Guid.NewGuid(), city, new Coordinates(50, 100), timeZoneIana);

        _languageResolverMock.Setup(l => l.Resolve(city)).Returns("en");

        _geocodingServiceMock.Setup(geocode =>
                geocode.ResolveAsync(city, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(geocodingData));

        // 2. Текущая погода
        var currentForecast = new CurrentForecast
        (
            observedAtUtc,
            geocodingData.LocationId,
            temperature,
            enumCondition
        );

        _weatherProviderMock.Setup(provider =>
                provider.GetCurrentForecastAsync(geocodingData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(currentForecast));

        // 3. Получение локального часового пояса
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIana);

        _timeZoneResolverMock.Setup(tzResolver =>
                tzResolver.GetTimeZoneInfoById(timeZoneIana))
            .Returns(Result.Ok(tz));

        // 3.5 Переводим время в локальное

        var clock = _clockMock.SetupGet(clock =>
            clock.UtcNow).Returns(fixedNowUtc);

        // Ожидаемые локальные времена
        var expectedObservedLocal = TimeZoneInfo.ConvertTime(observedAtUtc, tz);
        var expectedFetchedLocal = TimeZoneInfo.ConvertTime(fixedNowUtc, tz);

        // 4. Перевод в DTO - настройка ConditionStringMapper
        _conditionStringMapperMock.Setup(mapper =>
                mapper.From(enumCondition))
            .Returns(conditionNormalized);

        // Act
        var result = await _weatherForecastService.GetCurrentForecastAsync(city, CancellationToken.None);
        var dto = result.Value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        dto.City.Should().Be(city);
        dto.Condition.Should().Be(conditionNormalized);
        dto.Temperature.Should().Be(temperature);

        dto.Date.Should().Be(expectedObservedLocal);
        dto.FetchedAt.Should().Be(expectedFetchedLocal);

        _geocodingServiceMock.Verify(x => x.ResolveAsync(city, "en", It.IsAny<CancellationToken>()), Times.Once);
        _weatherProviderMock.Verify(x => x.GetCurrentForecastAsync(geocodingData, It.IsAny<CancellationToken>()),
            Times.Once);
        _timeZoneResolverMock.Verify(x => x.GetTimeZoneInfoById(timeZoneIana), Times.Once);
        _conditionStringMapperMock.Verify(x => x.From(enumCondition), Times.Once);
        _languageResolverMock.Verify(x => x.Resolve(city), Times.Once);

        _testOutputHelper.WriteLine($"condition: {result.Value.Condition}");
        _testOutputHelper.WriteLine($"city: {result.Value.City}");
        _testOutputHelper.WriteLine($"temperature: {result.Value.Temperature}");
        _testOutputHelper.WriteLine($"fetch time: {result.Value.FetchedAt}");
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetCurrentForecastAsync_WhenCityIsEmpty_ShouldReturnFailResult(string emptyCity)
    {
        // Act
        var result = await _weatherForecastService.GetCurrentForecastAsync(emptyCity, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("City cannot be null or whitespace");

        _geocodingServiceMock.Verify(geocode =>
            geocode.ResolveAsync(emptyCity, "en", It.IsAny<CancellationToken>()), Times.Never);

        _testOutputHelper.WriteLine($"city: {emptyCity} is null or empty or whitespace");
    }

    [Fact]
    public async Task GetCurrentForecastAsync_WhenGeocodingFails_ShouldReturnFailResult()
    {
        // Arrange 
        var city = "MockCity";
        // 1. Геокодинг - с ошибкой
        _languageResolverMock.Setup(l => l.Resolve(city)).Returns("en");

        _geocodingServiceMock.Setup(geocode =>
                geocode.ResolveAsync(city, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("City cannot be found"));


        // Act
        var result = await _weatherForecastService.GetCurrentForecastAsync(city, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("City cannot be found");

        _weatherProviderMock.Verify(provider =>
            provider.GetCurrentForecastAsync(
                It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}