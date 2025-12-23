using FluentResults;

namespace Nubrio.Application.Interfaces.Repository;

public interface IStatsRepository
{
    Task<Result<int>> CountRequestsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken ct);

    Task<Result<IReadOnlyList<RequestEntry>>> GetRequestsPageAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toExclusiveUtc,
        int skip,
        int take,
        CancellationToken ct);

    Task<Result<IReadOnlyList<TopCityEntry>>> GetTopCitiesAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int limit,
        CancellationToken ct);
}