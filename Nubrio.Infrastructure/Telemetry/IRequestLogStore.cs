using Nubrio.Infrastructure.Persistence;
using Nubrio.Infrastructure.Persistence.Entities;

namespace Nubrio.Infrastructure.Telemetry;

public interface IRequestLogStore
{
    Task LogAsync(RequestLogEntry entry, CancellationToken ct);
}

public record RequestLogEntry(
    DateTimeOffset TimestampUtc,
    string Endpoint,
    string City,
    DateOnly? Date,
    bool? CacheHit,
    int StatusCode,
    int LatencyMs
);

public class EfRequestLogStore : IRequestLogStore
{
    private readonly NubrioDbContext _context;

    public EfRequestLogStore(NubrioDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(RequestLogEntry entry, CancellationToken ct)
    {

        var request = new Request(
            entry.TimestampUtc,
            entry.Endpoint,
            entry.City,
            entry.Date,
            entry.CacheHit,
            entry.StatusCode,
            entry.LatencyMs
        );
    
        _context.Requests.Add(request);
        await _context.SaveChangesAsync(ct);
    }
}