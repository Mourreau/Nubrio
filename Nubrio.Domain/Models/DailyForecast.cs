using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models;

public class DailyForecast
{
    public DateTimeOffset Date { get; set; }
    public Location ForecastLocation { get; set; }
    public WeatherConditions Conditions { get; set; }
    public double MaxTemperature { get; set; }
    public double MinTemperature { get; set; }
}