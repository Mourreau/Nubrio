using System.ComponentModel.DataAnnotations;
using Nubrio.Domain.Enums;

namespace Nubrio.Application.DTOs.DailyForecast;

public record DailyForecastDto
{
    public required string City { get; init; }
    
    public required IReadOnlyList<DateOnly> Dates { get; init; } = [];
    
    public required IReadOnlyList<string> Conditions { get; init; } = [];
    
    public required IReadOnlyList<double> TemperaturesAvg { get; init; } = [];
    
    public required DateTimeOffset FetchedAt{ get; init; }
}