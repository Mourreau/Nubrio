namespace Nubrio.Presentation.DTOs.Response;

public record DailyForecastResponseDto
{
    public required string City { get; init; }
    public required DateOnly Date { get; init; }
    public required string Condition { get; init; }
    public required double TemperatureC { get; init; }
    public required string IconUrl { get; init; } = "Icon is not found";
    public required string Source { get; init; } = "Source is not found";
    public required DateTimeOffset FetchedAt { get; init; }
}