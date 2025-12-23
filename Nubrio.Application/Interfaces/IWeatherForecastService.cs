using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastService
{

    Task<Result<DailyForecastMeanDto>> GetDailyForecastByDateAsync(
        string city, DateOnly date, CancellationToken cancellationToken);

    Task<Result<WeeklyForecastMeanDto>> GetWeeklyForecastAsync(
        string city, CancellationToken cancellationToken);
}