using Nubrio.Domain.Enums;

namespace Nubrio.Application.DTOs.WeeklyForecast;

public sealed record DaysDto(WeatherConditions Condition, DateOnly Date, double TemperatureMean)
{
}