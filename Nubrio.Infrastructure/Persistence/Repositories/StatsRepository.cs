using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Interfaces.Repository;

namespace Nubrio.Infrastructure.Persistence.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly NubrioDbContext _context;

    public StatsRepository(NubrioDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> CountRequestsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken ct)
    {
        var requestsCount =
            await _context.Requests
                .AsNoTracking()
                .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc < toUtc)
                .CountAsync(ct);

        return Result.Ok(requestsCount);
    }

    public async Task<Result<IReadOnlyList<RequestEntry>>> GetRequestsPageAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toExclusiveUtc,
        int skip,
        int take,
        CancellationToken ct)
    {
        IReadOnlyList<RequestEntry> requestEntries =
            await _context.Requests
                .AsNoTracking()
                .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc < toExclusiveUtc)
                .OrderByDescending(x => x.TimestampUtc)
                .ThenByDescending(r => r.Id)
                .Skip(skip)
                .Take(take)
                .Select(r => new RequestEntry(
                    r.TimestampUtc,
                    r.Endpoint,
                    r.City,
                    r.Date,
                    r.CacheHit,
                    r.StatusCode,
                    r.LatencyMs))
                .ToListAsync(ct);

        return Result.Ok(requestEntries);
    }

    public async Task<Result<IReadOnlyList<TopCityEntry>>> GetTopCitiesAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int limit,
        CancellationToken ct)
    {
        var rows = await _context.Requests
            .AsNoTracking()
            .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc < toUtc)
            .GroupBy(x => x.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync(ct);

        IReadOnlyList<TopCityEntry> cityEntries = rows
            .Select(x => new TopCityEntry(x.City, x.Count))
            .ToList();


        return Result.Ok(cityEntries);
    }
}