namespace Nubrio.Infrastructure.Options;

public sealed class OpenMeteoOptions
{
    public string BaseUrl { get; init; } = default!;
    public int TimeoutSeconds { get; init; } = 5;
    public int CacheTtlSeconds { get; init; } = 120;
}