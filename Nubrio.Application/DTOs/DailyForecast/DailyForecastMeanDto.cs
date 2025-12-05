namespace Nubrio.Application.DTOs.DailyForecast;

public record DailyForecastMeanDto
{
    public required string City { get; init; }
    
    public required DateOnly Date { get; init; }
    
    public required string Condition { get; init; }
    
    public required double TemperatureMean { get; init; }
    
    public required DateTimeOffset FetchedAt{ get; init; }
}
