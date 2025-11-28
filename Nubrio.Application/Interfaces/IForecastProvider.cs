using FluentResults;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;

namespace Nubrio.Application.Interfaces;

public interface IForecastProvider
{
    Task<Result<DailyForecastMean>> GetDailyForecastMeanAsync(Location location, DateOnly date,
        CancellationToken cancellationToken);
    Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken);
    
}