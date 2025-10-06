using Nubrio.Domain.Models;

namespace Nubrio.Application.DTOs.CurrentForecast;

public record CurrentForecastDto
{
    public string City { get; init; }
    public DateTimeOffset Date { get; init; }
    public string Condition { get; init; }
    public double Temperature { get; init; }
    public DateTimeOffset FetchedAt{ get; init; }
}
