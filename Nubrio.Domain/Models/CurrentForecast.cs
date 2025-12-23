using System.Runtime.Serialization;
using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models;

public class CurrentForecast(
    DateTimeOffset observedAt,
    Guid locationId,
    double temperature,
    WeatherConditions condition)
{
    public DateTimeOffset ObservedAt { get; } = observedAt;
    public Guid LocationId { get; } = locationId;
    public double Temperature { get; } = temperature;
    public WeatherConditions Condition { get; } = condition;
}