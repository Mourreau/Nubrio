using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherProvider
{
    Task<Result<DailyForecast>> GetDailyForecastRangeAsync(
        Location location, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken);
    
}