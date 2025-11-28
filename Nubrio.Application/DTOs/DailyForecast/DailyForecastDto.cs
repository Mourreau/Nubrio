namespace Nubrio.Application.DTOs.DailyForecast;

public record DailyForecastDto
{
    public required string City { get; init; }
    
    public required IReadOnlyList<DateOnly> Dates { get; init; } = [];
    
    public required IReadOnlyList<string> Conditions { get; init; } = [];
    
    public required IReadOnlyList<double> TemperaturesMean { get; init; } = [];
    
    public required DateTimeOffset FetchedAt{ get; init; }
}