using Microsoft.AspNetCore.Mvc;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Validators.Errors;
using Nubrio.Presentation.DTOs.Response;
using Nubrio.Presentation.DTOs.Response.WeeklyResponse;
using Nubrio.Presentation.Mappers;

namespace Nubrio.Presentation.Controllers;

[ApiController]
[Route("api/weather/{city}")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherForecastService _weatherForecastService;
    private readonly IClock _clock;


    public WeatherController(IWeatherForecastService weatherForecastService, IClock clock)
    {
        _weatherForecastService = weatherForecastService;
        _clock = clock;
    }

    [HttpGet("current")]
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
    [HttpGet]
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

        if (date > forecastDateOffset) return BadRequest($"Date must not be later than {forecastDateOffset}");

        var dailyForecast =
            await _weatherForecastService.GetDailyForecastByDateAsync(city, date, cancellationToken);

        if (dailyForecast.IsFailed)
        {
            var firstError = dailyForecast.Errors.First();
            var code = firstError.Metadata?["Code"] as string;

            if (code == ForecastServiceErrorCodes.EmptyCity)
                return BadRequest(firstError.Message);

            if (code == ForecastServiceErrorCodes.GeocodingNotFound)
                return NotFound(firstError.Message);

            return Problem(
                detail: firstError.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var forecastDto = dailyForecast.Value;


        return Ok(ForecastMapper.ToDailyResponse(forecastDto));
    }


    /// <summary>
    /// Получает прогноз погоды на неделю для указанного города.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// <c>GET /api/weather/{city}/week</c>
    ///
    /// Ограничения:
    /// - <c>city</c> — обязательный параметр, кириллица или латиница (без транслита)
    /// - Прогноз всегда возвращается на 7 дней вперёд (значение фиксировано на стороне провайдера)
    /// </remarks>
    ///
    /// <param name="city">
    /// Название города, как его вводит пользователь.  
    /// Примеры: "Москва", "Berlin", "Yerevan".
    /// </param>
    /// <param name="cancellationToken">
    /// Токен отмены HTTP-запроса. Позволяет прервать операцию досрочно.
    /// </param>
    ///
    /// <response code="200"> Успешно. Возвращает недельный прогноз погоды для указанного города. </response>
    ///
    /// <response code="400"> Некорректный запрос — параметр <c>city</c> пустой или содержит только пробелы.</response>
    ///
    /// <response code="404">
    /// Город не найден геокодинг-провайдером.  
    /// Возвращается, когда внешнее API не может определить координаты.
    /// </response>
    ///
    /// <response code="500"> Внутренняя ошибка сервера, либо внешнее API вернуло неожиданный результат. </response>
    [HttpGet("week")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WeeklyForecastResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeeklyForecastResponseDto>> GetWeeklyForecastByCity(
        [FromRoute] string city,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City cannot be null or whitespace");

        var weeklyForecast =
            await _weatherForecastService.GetForecastByWeekAsync(city, cancellationToken);

        if (weeklyForecast.IsFailed)
        {
            var firstError = weeklyForecast.Errors.First();
            var code = firstError.Metadata?["Code"] as string;

            if (code == ForecastServiceErrorCodes.EmptyCity)
                return BadRequest(firstError.Message);

            if (code == ForecastServiceErrorCodes.GeocodingNotFound)
                return NotFound(firstError.Message);

            return Problem(
                detail: firstError.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var forecastDto = weeklyForecast.Value;

        return Ok(ForecastMapper.ToWeeklyResponse(forecastDto));
    }
}