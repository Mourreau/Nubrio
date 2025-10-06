using Nubrio.Domain.Enums;

namespace Nubrio.Application.DTOs.DailyForecast;

public class DailyForecastResponseDto
{
    public string City { get; set; }
    public List<DateOnly> Dates { get; set; }
    public List<string> Conditions { get; set; }
    public List<double> Temperatures { get; set; }
    public List<string> IconUrl { get; set; } 
    public string Source { get; set; }
    public DateTimeOffset FetchedAt{ get; set; }
}