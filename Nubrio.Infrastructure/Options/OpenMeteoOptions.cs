namespace Nubrio.Infrastructure.Options;

public sealed class OpenMeteoOptions
{
    public string ForecastBaseUrl { get; init; } = default!;
    public string GeocodingBaseUrl { get; init; } = default!;
    public int TimeoutSeconds { get; init; } = 5;
    public int CacheTtlSeconds { get; init; } = 120;
}