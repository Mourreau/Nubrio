using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Nubrio.Application.Common.Errors;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Domain.Enums;
using Nubrio.Presentation.DTOs.Forecast.Response;

namespace Nubrio.Tests.Presentation.ControllersTests.IntegrationTests.WeatherControllerTests;

public class GetDailyForecastMeanByCityTests : IClassFixture<WeatherApiFactory>
{
    private readonly WeatherApiFactory _factory;
    private readonly FakeWeatherForecastService _fakeService;
    private readonly HttpClient _client;


    public GetDailyForecastMeanByCityTests(WeatherApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        _fakeService = scope.ServiceProvider.GetRequiredService<FakeWeatherForecastService>();
    }


    [Fact]
    public async Task GetDailyForecast_Should_Return_200_WhenForecastFound()
    {
        // Arrange
        var checkCity = "Moscow";
        var resultDto = new DailyForecastMeanDto
        {
            City = checkCity,
            Condition = WeatherConditions.Clear,
            Date = new DateOnly(2025, 11, 11),
            TemperatureMean = 11,
            FetchedAt = new DateTime(2000, 1, 1)
        };
        _fakeService.NextDailyResult = Result.Ok(resultDto);
        var url = $"/api/weather/{checkCity}?date=2025-11-11";


        // Act
        var response = await _client.GetAsync(url);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseDto = await response.Content.ReadFromJsonAsync<DailyForecastResponseDto>();

        responseDto.Should().NotBeNull();
        responseDto!.City.Should().Be(checkCity);
        responseDto.Date.Should().Be(resultDto.Date);
        responseDto.TemperatureC.Should().Be(11);
    }


    [Fact]
    public async Task GetDailyForecast_Should_Return_404_When_Location_Not_Found()
    {
        // Arrange
        var appError = new Error("No location found for city 'eueyuyu'")
            .WithMetadata(ProviderErrorMetadataKeys.ServiceCode, AppErrorCode.LocationNotFound);

        _fakeService.NextDailyResult = Result.Fail(appError);
        var url = "/api/weather/eueyuyu?date=2025-11-11";


        // Act
        var response = await _client.GetAsync(url);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
}