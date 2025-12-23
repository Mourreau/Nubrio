namespace Nubrio.Presentation.DTOs.Stats;

public sealed record RequestsResponse(
    DateOnly From,
    DateOnly To,
    int Page,
    int PageSize,
    int Total,
    List<RequestDto> Requests);

public sealed record RequestDto(
    DateTimeOffset TimestampUtc,
    string Endpoint,
    string City,
    DateOnly? Date,
    bool? CacheHit,
    int StatusCode,
    int LatencyMs);