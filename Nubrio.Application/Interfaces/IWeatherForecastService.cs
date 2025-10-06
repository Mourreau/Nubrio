using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city, CancellationToken cancellationToken);
    
    Task<Result<DailyForecastDto>> GetDailyForecastByDateAsync(
        Coordinates coordinates, DateOnly date, CancellationToken cancellationToken);
    
    Task<Result<DailyForecastDto>> GetDailyForecastRangeAsync(
        Coordinates coordinates, DateOnly startDate,  DateOnly endDate, CancellationToken cancellationToken);
    
    
    
}