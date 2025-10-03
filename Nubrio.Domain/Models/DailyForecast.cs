using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models;

public class DailyForecast(
    DateOnly date,
    Guid locationId,
    WeatherConditions conditions,
    double maxTemperature,
    double minTemperature)
{
    public DateOnly Date { get;} = date;
    public Guid LocationId { get; } = locationId;
    public WeatherConditions Conditions { get; } = conditions;
    public double MaxTemperature { get; } = maxTemperature;
    public double MinTemperature { get; } = minTemperature;
}