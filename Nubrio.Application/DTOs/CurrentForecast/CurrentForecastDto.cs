
namespace Nubrio.Application.DTOs.CurrentForecast;

public record CurrentForecastDto
{
    public required string City { get; init; }
    
    public required DateTimeOffset Date { get; init; }
    
    public required string Condition { get; init; }
    
    public required double Temperature { get; init; }
    
    public required DateTimeOffset FetchedAt{ get; init; }
    
}
