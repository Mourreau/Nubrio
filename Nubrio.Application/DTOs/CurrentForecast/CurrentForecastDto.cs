using System.ComponentModel.DataAnnotations;
using Nubrio.Domain.Models;

namespace Nubrio.Application.DTOs.CurrentForecast;

public record CurrentForecastDto
{
    [Required]
    public string City { get; init; }
    
    [Required]
    public DateTimeOffset Date { get; init; }
    
    [Required]
    public string Condition { get; init; }
    
    [Required]
    public double Temperature { get; init; }
    
    [Required]
    public DateTimeOffset FetchedAt{ get; init; }
}
