using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Nubrio.Application.Common.Errors;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Domain.Enums;
using Nubrio.Presentation.DTOs.Forecast.Response.WeeklyResponse;

namespace Nubrio.Tests.Presentation.ControllersTests.IntegrationTests.WeatherControllerTests;

public class GetWeeklyForecastMeanByCityTests : IClassFixture<WeatherApiFactory>
{
    private readonly WeatherApiFactory _factory;
    private readonly FakeWeatherForecastService _fakeService;
    private readonly HttpClient _client;


    public GetWeeklyForecastMeanByCityTests(WeatherApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        _fakeService = scope.ServiceProvider.GetRequiredService<FakeWeatherForecastService>();
    }

    [Fact]
    public async Task GetWeeklyForecast_Should_Return_404_When_Location_Not_Found()
    {
        // Arrange
        var appError = new Error("No location found for city 'eueyuyu'")
            .WithMetadata(ProviderErrorMetadataKeys.ServiceCode, AppErrorCode.LocationNotFound);
        
        _fakeService.NextWeeklyResult = Result.Fail(appError);
        var url = "/api/weather/eueyuyu/week";
        
        
        // Act
        var response = await _client.GetAsync(url);
        
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
    
    [Fact]
    public async Task GetWeeklyForecast_Should_Return_200_WhenForecastFound()
    {
        // Arrange
        var checkCity = "Moscow";
        var resultDto = new WeeklyForecastMeanDto
        {
            City = checkCity,
            Days = new List<DaysDto>{new DaysDto(
                Condition: WeatherConditions.Clear,
                Date: new DateOnly(2025, 11, 11),
                TemperatureMean: 11)},
            FetchedAt = new DateTime(2000,
                1,
                1)
        };
        _fakeService.NextWeeklyResult = Result.Ok(resultDto);
        var url = $"/api/weather/{checkCity}/week";
        
        
        // Act
        var response = await _client.GetAsync(url);
        
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseDto = await response.Content.ReadFromJsonAsync<WeeklyForecastResponseDto>();
        
        responseDto.Should().NotBeNull();
        responseDto!.City.Should().Be(checkCity);
        responseDto.Days.Should().HaveCount(1);
        responseDto.Days[0].TemperatureC.Should().Be(11);
    }
}