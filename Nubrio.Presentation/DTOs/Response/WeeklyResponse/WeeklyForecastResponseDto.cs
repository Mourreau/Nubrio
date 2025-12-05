namespace Nubrio.Presentation.DTOs.Response.WeeklyResponse;

public record WeeklyForecastResponseDto
{
    public required string City { get; init; }

    public required IReadOnlyList<DaysResponseDto> Days { get; init; }

    public required string Source { get; init; } = "Source is not found";
    public required DateTimeOffset FetchedAt { get; init; }
}

public record DaysResponseDto
{
    public required DateOnly Date { get; init; }
    public required string Condition { get; init; }
    public required double TemperatureC { get; init; }
    public required string IconUrl { get; init; } = "Icon is not found";
}