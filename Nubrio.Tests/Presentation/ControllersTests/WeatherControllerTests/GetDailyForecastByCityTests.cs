using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Validators.Errors;
using Nubrio.Presentation.Controllers;
using Nubrio.Presentation.DTOs.Response;

namespace Nubrio.Tests.Presentation.ControllersTests.WeatherControllerTests;

public class GetDailyForecastByCityTests
{
    private readonly Mock<IWeatherForecastService> _weatherForecastServiceMock;
    private readonly WeatherController _controller;

    public GetDailyForecastByCityTests()
    {
        _weatherForecastServiceMock = new Mock<IWeatherForecastService>();

        _controller = new WeatherController(_weatherForecastServiceMock.Object);
    }


    [Theory]
    [InlineData("Moscow", "2025-02-11", "heavy rain", 17)]
    [InlineData("Minsk", "2025-09-08", "sunny", -9.4)]
    [InlineData("Yekaterinburg", "2025-06-01", "clear", 1.7)]
    public async Task GetDailyForecastByCity_WhenServiceReturnsSuccess_ShouldReturnOkWithMappedResponse(
        string city, string date, string condition, double tempMean)
    {
        // Arrange
        var dateOnly = DateOnly.Parse(date);

        var serviceDto = GetWeatherForecastServiceDto(city, dateOnly, condition, tempMean);

        _weatherForecastServiceMock
            .Setup(s => s.GetDailyForecastByDateAsync(city, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(serviceDto));

        // Act
        var actionResult = await _controller.GetDailyForecastByCity(city, dateOnly, CancellationToken.None);

        // Assert
        // Проверяем, что это OkObjectResult
        var okResult = actionResult.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        // Проверяем тип и значение в Body
        var response = okResult.Value.Should().BeOfType<DailyForecastResponseDto>().Subject;

        response.City.Should().Be(city);
        response.Date.Should().Be(dateOnly);
        response.Condition.Should().Be(condition);
        response.TemperatureC.Should().Be(tempMean);
        response.FetchedAt.Should().Be(serviceDto.FetchedAt);
        response.Source.Should().Be("Open-Meteo");

        // 3) Убедимся, что сервис вызывался ровно один раз
        _weatherForecastServiceMock.Verify(
            s => s.GetDailyForecastByDateAsync(city, dateOnly, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetDailyForecastByCity_WhenCityIsEmpty_ShouldReturnBadRequest(string emptyCity)
    {
        // Arrange 
        var dateOnly = DateOnly.Parse("2020-10-20");

        // Act
        var actionResult = await _controller.GetDailyForecastByCity(emptyCity, dateOnly, CancellationToken.None);

        // Assert
        var badRequest = actionResult.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest.StatusCode.Should().Be(400);

        badRequest.Value.Should().Be("City cannot be null or whitespace");

        _weatherForecastServiceMock.Verify(x => x.GetDailyForecastByDateAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDailyForecastByCity_WhenDateIsTooFar_ShouldReturnBadRequest()
    {
        // Arrange 
        const string city = "Valid City";
        var farDateOnly = DateOnly.FromDateTime(DateTime.Now).AddMonths(4); // Передаваемая дата в контроллер
        var farDate =
            DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(3); // Самая крайняя дата на которую может быть дан прогноз

        // Act
        var actionResult = await _controller.GetDailyForecastByCity(city, farDateOnly, CancellationToken.None);

        // Assert
        var badRequest = actionResult.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest.StatusCode.Should().Be(400);

        badRequest.Value.Should().Be($"Date must not be later than 3 months: {farDate}");

        _weatherForecastServiceMock.Verify(x => x.GetDailyForecastByDateAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDailyForecastByCity_WhenForecastServiceReturnsEmptyCity_ShouldReturnBadRequest()
    {
        // Arrange 
        const string city = "Valid City";
        var dateOnly = DateOnly.Parse("2025-02-11");
        var cityIsEmptyError = new Error("City cannot be null or whitespace")
            .WithMetadata("Code", ForecastServiceErrorCodes.EmptyCity);

        _weatherForecastServiceMock
            .Setup(s => s.GetDailyForecastByDateAsync(city, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(cityIsEmptyError));


        // Act
        var actionResult = await _controller.GetDailyForecastByCity(city, dateOnly, CancellationToken.None);

        // Assert
        var badRequest = actionResult.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest.StatusCode.Should().Be(400);

        badRequest.Value.Should().Be("City cannot be null or whitespace");

        _weatherForecastServiceMock.Verify(x => x.GetDailyForecastByDateAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDailyForecastByCity_WithInternalError_ShouldReturnInternalError()
    {
        // Arrange 
        const string city = "Valid City";
        var dateOnly = DateOnly.Parse("2025-02-11");
        var internalError = new Error("Some internal error")
            .WithMetadata("Code", ForecastServiceErrorCodes.InternalError);

        _weatherForecastServiceMock
            .Setup(s => s.GetDailyForecastByDateAsync(city, dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(internalError));

        // Act
        var actionResult = await _controller.GetDailyForecastByCity(city, dateOnly, CancellationToken.None);

        // Assert
        var objResult = actionResult.Result as ObjectResult;
        objResult.Should().NotBeNull();
        objResult.StatusCode.Should().Be(500);

        objResult.Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)objResult.Value!;
        problem.Detail.Should().Be("Some internal error");

        _weatherForecastServiceMock.Verify(x => x.GetDailyForecastByDateAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    #region Helpers

    private DailyForecastDto GetWeatherForecastServiceDto(string city, DateOnly dateOnly, string condition,
        double tempMean)
    {
        return new DailyForecastDto
        {
            City = city,
            Dates = [dateOnly],
            Conditions = [condition],
            FetchedAt = new DateTimeOffset(dateOnly.Year, dateOnly.Month, dateOnly.Day, 12, 0, 0, TimeSpan.Zero),
            TemperaturesMean = [tempMean]
        };
    }

    #endregion
}