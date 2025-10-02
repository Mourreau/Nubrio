using System.Runtime.Serialization;
using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models;

public class CurrentForecast
{
    public DateOnly Date { get; set; }
    public Location ForecastLocation { get; set; }
    public double Temperature { get; set; }
    public WeatherConditions Conditions { get; set; }
}