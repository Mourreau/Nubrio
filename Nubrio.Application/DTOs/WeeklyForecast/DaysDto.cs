namespace Nubrio.Application.DTOs.WeeklyForecast;

public record DaysDto
{
    public required DateOnly Date { get; init; }
    public required string Condition { get; init; }
    public required double TemperatureMean { get; init; }
}