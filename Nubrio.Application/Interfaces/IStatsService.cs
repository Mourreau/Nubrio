using FluentResults;

namespace Nubrio.Application.Interfaces;

public interface IStatsService
{
    Task<Result<TopCitiesResult>> GetTopCitiesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int limit,
        CancellationToken ct);

    Task<Result<RequestsPageResult>> GetRequestsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int page,
        int pageSize,
        CancellationToken ct);
}

public sealed record RequestsPageResult(
    DateOnly From,
    DateOnly To,
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<RequestEntry> Entries);

public sealed record TopCitiesResult(
    DateOnly From,
    DateOnly To,
    int Limit,
    IReadOnlyList<TopCityEntry> TopCities);

public sealed record RequestEntry(
    DateTimeOffset TimestampUtc,
    string Endpoint,
    string City,
    DateOnly? Date,
    bool? CacheHit,
    int StatusCode,
    int LatencyMs);

public sealed record TopCityEntry(string City, int ResultsCount);