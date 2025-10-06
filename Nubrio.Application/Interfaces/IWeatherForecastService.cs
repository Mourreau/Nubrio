using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<Result<CurrentForecastResponseDto>> GetCurrentForecastAsync(string city);
    
    Task<Result<DailyForecastResponseDto>> GetDailyForecastByDateAsync(Coordinates coordinates, DateOnly date);
    
    Task<Result<DailyForecastResponseDto>> GetDailyForecastByStartEndDateAsync(
        Coordinates coordinates, DateOnly startDate,  DateOnly endDate);
    
    
    
}