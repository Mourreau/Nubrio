namespace Nubrio.Application.DTOs.WeeklyForecast;

public sealed record DaysDto(string Condition, DateOnly Date, double TemperatureMean)
{
}