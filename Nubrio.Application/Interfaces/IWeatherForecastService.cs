using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<Result<CurrentForecast>> GetCurrentForecastAsync(string city);
    
    Task<Result<DailyForecast>> GetDailyForecastByDateAsync(Coordinates coordinates, DateOnly date);
    
    Task<Result<DailyForecast>> GetDailyForecastByStartEndDateAsync(Coordinates coordinates, DateOnly startDate,  DateOnly endDate);
    
    
    
}