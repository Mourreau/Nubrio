using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models.Daily;

public class DailyForecastMean(
    DateOnly date,
    Guid locationId,
    WeatherConditions condition,
    double temperatureMean)
{
    public DateOnly Date { get;} = date;
    public Guid LocationId { get; } = locationId;
    public WeatherConditions Condition { get; } = condition;
    public double TemperatureMean { get; } = temperatureMean;
}