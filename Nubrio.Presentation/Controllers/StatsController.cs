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


    [HttpGet("top-cities")]
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


    [HttpGet("requests")]
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


// GET /api/stats/top-cities?from=YYYY-MM-DD&to=YYYY-MM-DD&limit=10
// GET /api/stats/requests?from=…&to=…