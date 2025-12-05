using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city, CancellationToken cancellationToken);

    Task<Result<DailyForecastMeanDto>> GetDailyForecastByDateAsync(
        string city, DateOnly date, CancellationToken cancellationToken);

    Task<Result<WeeklyForecastMeanDto>> GetForecastByWeekAsync(
        string city, CancellationToken cancellationToken);
}