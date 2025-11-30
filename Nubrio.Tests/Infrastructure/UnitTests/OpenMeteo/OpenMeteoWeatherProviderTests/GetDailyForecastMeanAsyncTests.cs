using System.Globalization;
using FluentAssertions;
using FluentResults;
using Moq;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.Clients.ForecastClient;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.Validators.Errors;
using Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.TestData.OpenMeteoWeatherProviderTestData;
using Xunit.Abstractions;

namespace Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.OpenMeteoWeatherProviderTests;

public class GetDailyForecastMeanAsyncTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IForecastProvider _forecastProvider;

    private readonly Mock<IForecastClient> _openMeteoClientMock;
    private readonly Mock<IWeatherCodeTranslator> _weatherCodeTranslatorMock;

    public GetDailyForecastMeanAsyncTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _openMeteoClientMock = new Mock<IForecastClient>();
        _weatherCodeTranslatorMock = new Mock<IWeatherCodeTranslator>();

        _forecastProvider = new OpenMeteoForecastProvider(
            _openMeteoClientMock.Object,
            _weatherCodeTranslatorMock.Object);
    }

    [Theory]
    [InlineData("2025-10-20", WeatherConditions.HeavyRain, 1.5, 47)]
    public async Task GetDailyForecastMean_WhenResponseIsValid_ReturnsDailyForecastMean(
        string dateString, WeatherConditions weatherCondition, double temperatureMean, int weatherCode)
    {
        // Arrange
        var dateStingList = MakeDataArray(dateString);
        var temperatureList = MakeDataArray(temperatureMean);
        var weatherCodeList = MakeDataArray(weatherCode);
        var date = DateOnly.Parse(dateString, CultureInfo.InvariantCulture);
        var id = Guid.NewGuid();

        var testLocation = MakeLocation(id);

        var clientResponse = MakeClientResponseDto(dateStingList, temperatureList, weatherCodeList);


        _openMeteoClientMock.Setup(client =>
            client.GetOpenMeteoDailyMeanAsync(
                testLocation.Coordinates.Latitude,
                testLocation.Coordinates.Longitude,
                date,
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(clientResponse));

        _weatherCodeTranslatorMock.Setup(translator =>
            translator.Translate(clientResponse.Daily.WeatherCode[0])).Returns(weatherCondition);

        // Act
        var result = await _forecastProvider.GetDailyForecastMeanAsync(
            testLocation, date,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value;

        dto.LocationId.Should().Be(id);
        dto.Date.Should().Be(date);
        dto.Condition.Should().Be(weatherCondition);
        dto.TemperatureMean.Should().Be(temperatureMean);

        _openMeteoClientMock.Verify(c => c.GetOpenMeteoDailyMeanAsync(
            testLocation.Coordinates.Latitude,
            testLocation.Coordinates.Longitude,
            date,
            It.IsAny<CancellationToken>()), Times.Once);

        _weatherCodeTranslatorMock.Verify(translator => translator.Translate(weatherCode), Times.Once);
    }


    [Fact]
    public async Task GetDailyForecastMean_WhenDailyArraysEmpty_ReturnsFail()
    {
        // Arrange
        var dateString = "2025-10-20";
        var date = DateOnly.Parse(dateString, CultureInfo.InvariantCulture);
        var id = Guid.NewGuid();
        var weatherCode = 47;


        var testLocation = MakeLocation(id);

        var clientResponse = MakeClientResponseDto([], [], []);

        _openMeteoClientMock.Setup(client =>
            client.GetOpenMeteoDailyMeanAsync(
                testLocation.Coordinates.Latitude,
                testLocation.Coordinates.Longitude,
                date,
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(clientResponse));


        // Act
        var result = await _forecastProvider.GetDailyForecastMeanAsync(testLocation, date, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e =>
            e.Metadata != null
            && e.Metadata.ContainsKey("Code")
            && (e.Metadata["Code"].ToString() == OpenMeteoErrorCodes.MalformedDailyMean));

        _weatherCodeTranslatorMock.Verify(translator => translator.Translate(weatherCode), Times.Never);
        _openMeteoClientMock.Verify(c => c.GetOpenMeteoDailyMeanAsync(
            testLocation.Coordinates.Latitude,
            testLocation.Coordinates.Longitude,
            date,
            It.IsAny<CancellationToken>()), Times.Once);
    }


    [Theory]
    [MemberData(
        nameof(GetDailyForecastMeanAsyncTestData.NotEqualArrays),
        MemberType = typeof(GetDailyForecastMeanAsyncTestData))]
    public async Task GetDailyForecastMean_WhenDailyArraysNotEqual_ReturnsFail(List<string> dates, List<double> temps,
        List<int> codes)
    {
        // Arrange
        var date = DateOnly.Parse(dates[0], CultureInfo.InvariantCulture);
        var id = Guid.NewGuid();


        var testLocation = MakeLocation(id);

        var clientResponse = MakeClientResponseDto(dates, temps, codes);


        _openMeteoClientMock.Setup(client =>
            client.GetOpenMeteoDailyMeanAsync(
                testLocation.Coordinates.Latitude,
                testLocation.Coordinates.Longitude,
                date,
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(clientResponse));


        // Act
        var result = await _forecastProvider.GetDailyForecastMeanAsync(testLocation, date, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e =>
            e.Metadata != null
            && e.Metadata.ContainsKey("Code")
            && (e.Metadata["Code"].ToString() == OpenMeteoErrorCodes.MalformedDailyMean));

        _weatherCodeTranslatorMock.Verify(translator => translator.Translate(codes[0]), Times.Never);
        _openMeteoClientMock.Verify(c => c.GetOpenMeteoDailyMeanAsync(
            testLocation.Coordinates.Latitude,
            testLocation.Coordinates.Longitude,
            date,
            It.IsAny<CancellationToken>()), Times.Once);

        _testOutputHelper.WriteLine(result.Errors[0].Message);
    }

    [Theory]
    [MemberData(
        nameof(GetDailyForecastMeanAsyncTestData.TwoElementsInArray),
        MemberType = typeof(GetDailyForecastMeanAsyncTestData))]
    public async Task GetDailyForecastMean_WhenDailyArraysContainsMoreThenOneElement_ReturnsFail(
        List<string> dates,
        List<double> temps,
        List<int> codes)
    {
        // Arrange
        var date = DateOnly.Parse(dates[0], CultureInfo.InvariantCulture);
        var id = Guid.NewGuid();


        var testLocation = MakeLocation(id);

        var clientResponse = MakeClientResponseDto(dates, temps, codes);


        _openMeteoClientMock.Setup(client =>
            client.GetOpenMeteoDailyMeanAsync(
                testLocation.Coordinates.Latitude,
                testLocation.Coordinates.Longitude,
                date,
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(clientResponse));


        // Act
        var result = await _forecastProvider.GetDailyForecastMeanAsync(testLocation, date, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e =>
            e.Metadata != null
            && e.Metadata.ContainsKey("Code")
            && (e.Metadata["Code"].ToString() == OpenMeteoErrorCodes.MalformedDailyMean));

        _weatherCodeTranslatorMock.Verify(translator => translator.Translate(codes[0]), Times.Never);
        _openMeteoClientMock.Verify(c => c.GetOpenMeteoDailyMeanAsync(
            testLocation.Coordinates.Latitude,
            testLocation.Coordinates.Longitude,
            date,
            It.IsAny<CancellationToken>()), Times.Once);

        _testOutputHelper.WriteLine($"Error is: {result.Errors[0].Message}");
    }


    #region Heplers

    private static Location MakeLocation(Guid id)
    {
        return new Location(
            id,
            "Moscow",
            new Coordinates(50, 100),
            "Europe/Moscow");
    }

    private static OpenMeteoDailyMeanResponseDto MakeClientResponseDto(
        List<string> dates, List<double> temperatures, List<int> weatherCodes)
    {
        return new OpenMeteoDailyMeanResponseDto
        {
            Latitude = 56.75,
            Longitude = 74.75,
            GenerationTimeMs = 0.154376029968262,
            UtcOffsetSeconds = 18000,
            Timezone = "Europe/Moscow",
            TimezoneAbbreviation = "GMT+3",
            Elevation = 228,

            DailyUnits = new DailyUnitsMeanDto
            {
                Time = "iso8601",
                Temperature2mMean = "°C",
                WeatherCode = "wmo code"
            },

            Daily = new DailyDataMeanDto
            {
                Time = dates,
                Temperature2mMean = temperatures,
                WeatherCode = weatherCodes
            }
        };
    }

    private static List<T> MakeDataArray<T>(T data) => [data];

    #endregion
}