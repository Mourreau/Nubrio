using Nubrio.Domain.Models;

namespace Nubrio.Application.DTOs.CurrentForecast;

public class CurrentForecastResponseDto
{
    public string City { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Condition { get; set; }
    public double Temperature { get; set; }
    public string IconUrl { get; set; } 
    public string Source { get; set; }
    public DateTimeOffset FetchedAt{ get; set; }
}
