using System.Runtime.Serialization;
using Nubrio.Domain.Enums;

namespace Nubrio.Domain.Models;

public class CurrentForecast(
    DateOnly date,
    Guid locationId,
    double temperature,
    WeatherConditions conditions)
{
    public DateOnly Date { get; } = date;
    public Guid LocationId { get; } = locationId;
    public double Temperature { get; } = temperature;
    public WeatherConditions Conditions { get; } = conditions;
}