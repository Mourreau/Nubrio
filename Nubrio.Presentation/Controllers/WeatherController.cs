using Microsoft.AspNetCore.Mvc;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Validators.Errors;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;
using Nubrio.Presentation.DTOs.Response;
using Nubrio.Presentation.Mappers;

namespace Nubrio.Presentation.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherForecastService _weatherForecastService;
    private readonly IClock _clock;


    public WeatherController(IWeatherForecastService weatherForecastService, IClock clock)
    {
        _weatherForecastService = weatherForecastService;
        _clock = clock;
    }

    [HttpGet("{city}/current")]
    public async Task<ActionResult<CurrentWeatherResponseDto>> GetCurrentForecastByCity(
        [FromRoute] string city,
        CancellationToken cancellationToken)
    {
        var currentForecast = await _weatherForecastService.GetCurrentForecastAsync(city, cancellationToken);

        if (currentForecast.IsFailed)
            return BadRequest(currentForecast.Errors);


        return Ok(ForecastMapper.ToCurrentResponseDto(currentForecast.Value));
    }


    /// <summary>
    /// Получает среднесуточный прогноз погоды для указанного города и даты.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// <c>GET /api/weather/Berlin?date=2025-10-20</c>
    ///
    /// Ограничения:
    /// - <c>city</c> — обязательный параметр, кириллица или латиница (без транслита)
    /// - <c>date</c> — обязательный параметр, не далее чем на 3 месяца вперёд
    /// </remarks>
    /// <param name="city">
    /// Название города, как его вводит пользователь. Примеры: "Москва", "Berlin".
    /// </param>
    /// <param name="date">
    /// Дата, на которую нужен прогноз, в формате <c>yyyy-MM-dd</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен отмены HTTP-запроса. Используется для досрочного завершения операции.
    /// </param>
    /// <response code="200">
    /// Успешно. Возвращает среднесуточный прогноз погоды для города и указанной даты.
    /// </response>
    /// <response code="400">
    /// Некорректный запрос — пустой <c>city</c> или дата дальше, чем на 3 месяца вперёд.
    /// </response>
    /// <response code="404">Некорректный запрос - во время геокодинга для <c>city</c> не найдена локация</response>
    /// <response code="500">
    /// Внутренняя ошибка сервера или ошибка внешнего провайдера погоды.
    /// </response>
    [HttpGet("{city}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DailyForecastResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DailyForecastResponseDto>> GetDailyForecastByCity(
        [FromRoute] string city,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City cannot be null or whitespace");

        var forecastDateOffset =
            DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime)
                .AddMonths(3); // Прогноз погоды может быть сделан до 3-х месяцев вперед

        if (date > forecastDateOffset) return BadRequest($"Date must not be later than 3 months: {forecastDateOffset}");

        var dailyForecast =
            await _weatherForecastService.GetDailyForecastByDateAsync(city, date, cancellationToken);

        if (dailyForecast.IsFailed)
        {
            var firstError = dailyForecast.Errors.First();
            var code = firstError.Metadata?["Code"] as string;

            if (code == ForecastServiceErrorCodes.EmptyCity)
                return BadRequest(firstError.Message);
            
            if (code == OpenMeteoErrorCodes.GeocodingNotFound)
                return NotFound(firstError.Message);

            return Problem(
                detail: firstError.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var forecastDto = dailyForecast.Value;

        var result = new DailyForecastResponseDto
        {
            City = forecastDto.City,
            Condition = forecastDto.Conditions[0],
            Date = forecastDto.Dates[0],
            TemperatureC = forecastDto.TemperaturesMean[0],
            FetchedAt = forecastDto.FetchedAt,
            IconUrl = "Blank-Text", // TODO: Добавить иконки
            Source = "Open-Meteo", // TODO: Информацию о провайдере контроллер должен получать извне
        };

        return Ok(result);
    }
}