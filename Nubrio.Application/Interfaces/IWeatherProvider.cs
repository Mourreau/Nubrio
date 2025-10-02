using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherProvider
{
    Task<Result<bool>> TryGetDailyForecastByDate(Coordinates coordinates, DateOnly startDate, DateOnly endDate,
        out DailyResponseDto dailyForecast);
    
    Task<Result<bool>> TryGetCurrentForecast(string city, out CurrentResponseDto currentForecast);
    
}