namespace Nubrio.Application.DTOs.WeeklyForecast;

public record WeeklyForecastDto
{
    public required string City { get; init; }
    public required IReadOnlyList<DaysDto> Days { get; init; }
    public required DateTimeOffset FetchedAt{ get; init; }
}