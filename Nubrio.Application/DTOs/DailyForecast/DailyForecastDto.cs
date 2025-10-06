using Nubrio.Domain.Enums;

namespace Nubrio.Application.DTOs.DailyForecast;

public class DailyForecastDto
{
    public string City { get; set; }
    public List<DateOnly> Dates { get; set; }
    public List<string> Conditions { get; set; }
    public List<double> TemperaturesAvg { get; set; }
    public DateTimeOffset FetchedAt{ get; set; }
}