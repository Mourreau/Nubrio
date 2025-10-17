using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.OpenMeteo;

public class OpenMeteoWeatherProvider : IWeatherProvider
{
    public Task<Result<DailyForecast>> GetDailyForecastRangeAsync(Location location, DateOnly startDate, DateOnly endDate,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}