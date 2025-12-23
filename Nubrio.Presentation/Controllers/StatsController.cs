using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Nubrio.Application.Interfaces;
using Nubrio.Presentation.DTOs.Stats;

namespace Nubrio.Presentation.Controllers;

[ApiController]
[Route("api/stats/")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }


    /// <summary>
    /// Возвращает список городов с наибольшим количеством запросов за указанный период.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// <c>GET /api/stats/top-cities?from=2025-01-01&amp;to=2025-01-31&amp;limit=5</c>
    ///
    /// Ограничения:
    /// - <c>from</c> — обязательный параметр, начало периода (включительно)
    /// - <c>to</c> — обязательный параметр, конец периода (включительно)
    /// - <c>limit</c> — количество городов в ответе (от 1 до 50)
    /// </remarks>
    ///
    /// <param name="fromDate">
    /// Дата начала периода в формате <c>yyyy-MM-dd</c>.
    /// </param>
    /// <param name="toDate">
    /// Дата окончания периода в формате <c>yyyy-MM-dd</c>.
    /// </param>
    /// <param name="limit">
    /// Максимальное количество городов в ответе.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен отмены HTTP-запроса.
    /// </param>
    ///
    /// <response code="200">
    /// Успешно. Возвращает список городов и количество запросов для каждого.
    /// </response>
    /// <response code="400">
    /// Некорректный запрос — неверный диапазон дат или значение <c>limit</c>.
    /// </response>
    /// <response code="500">
    /// Внутренняя ошибка сервера.
    /// </response>
    [HttpGet("top-cities")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TopCitiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopCitiesResponse>> GetTopCitiesByDateSpan(
        [FromQuery(Name = "from")] DateOnly fromDate,
        [FromQuery(Name = "to")] DateOnly toDate,
        [FromQuery] int limit = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await _statsService.GetTopCitiesAsync(fromDate, toDate, limit, cancellationToken);

        if (result.IsFailed)
            return this.FromResult(Result.Fail(result.Errors));

        var topCities = result.Value;

        return Ok(MapToTopCitiesResponse(topCities));
    }


    /// <summary>
    /// Возвращает список сырых HTTP-запросов за указанный период с пагинацией.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// <c>GET /api/stats/requests?from=2025-01-01&amp;to=2025-01-31&amp;page=1&amp;pageSize=10</c>
    ///
    /// Ограничения:
    /// - <c>from</c> — обязательный параметр, начало периода
    /// - <c>to</c> — обязательный параметр, конец периода
    /// - <c>page</c> — номер страницы (начиная с 1)
    /// - <c>pageSize</c> — размер страницы (от 1 до 50)
    /// </remarks>
    ///
    /// <param name="fromDate">
    /// Дата начала периода в формате <c>yyyy-MM-dd</c>.
    /// </param>
    /// <param name="toDate">
    /// Дата окончания периода в формате <c>yyyy-MM-dd</c>.
    /// </param>
    /// <param name="page">
    /// Номер страницы (начиная с 1).
    /// </param>
    /// <param name="pageSize">
    /// Количество записей на странице.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен отмены HTTP-запроса.
    /// </param>
    ///
    /// <response code="200">
    /// Успешно. Возвращает страницу HTTP-запросов.
    /// </response>
    /// <response code="400">
    /// Некорректный запрос — неверный диапазон дат или параметры пагинации.
    /// </response>
    /// <response code="500">
    /// Внутренняя ошибка сервера.
    /// </response>
    [HttpGet("requests")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RequestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RequestsResponse>> GetRawRequests(
        [FromQuery(Name = "from")] DateOnly fromDate,
        [FromQuery(Name = "to")] DateOnly toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await _statsService.GetRequestsAsync(fromDate, toDate, page, pageSize, cancellationToken);

        if (result.IsFailed)
            return this.FromResult(Result.Fail(result.Errors));

        var requests = result.Value;


        return Ok(MapToRequestsResponse(requests));
    }


    private static RequestsResponse MapToRequestsResponse(RequestsPageResult result)
    {
        var requestDtoList = result.Entries.Select(x =>
            new RequestDto(
                TimestampUtc: x.TimestampUtc,
                Endpoint: x.Endpoint,
                City: x.City,
                Date: x.Date,
                CacheHit: x.CacheHit,
                StatusCode: x.StatusCode,
                LatencyMs: x.LatencyMs)).ToList();

        return new RequestsResponse(
            From: result.From,
            To: result.To,
            Page: result.Page,
            PageSize: result.PageSize,
            Total: result.Total,
            Requests: requestDtoList);
    }

    private static TopCitiesResponse MapToTopCitiesResponse(TopCitiesResult result)
    {
        var topCitiesDtoList = result.TopCities.Select(x =>
            new TopCitieDto(
                City: x.City,
                Count: x.ResultsCount)).ToList();

        return new TopCitiesResponse(
            From: result.From,
            To: result.To,
            Limit: result.Limit,
            Cities: topCitiesDtoList);
    }
}
