namespace Nubrio.Infrastructure.Persistence.Entities;

public sealed class Request
{
    private Request()
    {
    }

    public Request(
        DateTimeOffset timestampUtc,
        string endpoint,
        string city,
        DateOnly? date,
        bool? cacheHit,
        int statusCode,
        int latencyMs)
    {
        if (timestampUtc == default)
            throw new ArgumentOutOfRangeException(nameof(timestampUtc), "Request timestamp is required");

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or whitespace.", nameof(city));

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));

        if (latencyMs < 0)
            throw new ArgumentOutOfRangeException(nameof(latencyMs), "Latency Ms cannot be negative.");

        if (statusCode <= 0)
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Status Code cannot be negative or zero.");

        Id = Guid.NewGuid();
        TimestampUtc = timestampUtc;
        Endpoint = endpoint;
        City = city;
        Date = date;
        CacheHit = cacheHit;
        StatusCode = statusCode;
        LatencyMs = latencyMs;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset TimestampUtc { get; private set; }
    public string Endpoint { get; private set; }
    public string City { get; private set; }
    public DateOnly? Date { get; private set; }
    public bool? CacheHit { get; private set; }
    public int StatusCode { get; private set; }
    public int LatencyMs { get; private set; }
}