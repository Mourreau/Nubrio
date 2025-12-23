using FluentResults;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Nubrio.Domain.Models.Weekly;

namespace Nubrio.Application.Interfaces;

public interface IForecastProvider
{
    Task<Result<DailyForecastMean>> GetDailyForecastMeanAsync(Location location, DateOnly date,
        CancellationToken cancellationToken);
    
    Task<Result<WeeklyForecastMean>> GetWeeklyForecastMeanAsync(Location location, CancellationToken cancellationToken);
}