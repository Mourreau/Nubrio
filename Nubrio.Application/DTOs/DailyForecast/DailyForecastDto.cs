using System.ComponentModel.DataAnnotations;
using Nubrio.Domain.Enums;

namespace Nubrio.Application.DTOs.DailyForecast;

public record DailyForecastDto
{
    [Required]
    public string City { get; init; }
    
    [Required]
    public IReadOnlyList<DateOnly> Dates { get; init; } = [];
    
    [Required]
    public IReadOnlyList<string> Conditions { get; init; } = [];
    
    [Required]
    public IReadOnlyList<double> TemperaturesAvg { get; init; } = [];
    
    [Required]
    public DateTimeOffset FetchedAt{ get; init; }
}