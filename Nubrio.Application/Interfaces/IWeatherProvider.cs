using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherProvider
{
    Task<Result<DailyForecast>> GetDailyForecastByDateAsync(
        Coordinates coordinates, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<Result<CurrentForecast>> GetCurrentForecastAsync(Coordinates coordinates, CancellationToken cancellationToken);
    
}