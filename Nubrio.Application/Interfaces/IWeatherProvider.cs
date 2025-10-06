using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IWeatherProvider
{
    Task<Result<DailyForecast>> GetDailyForecastByDate(Coordinates coordinates, DateOnly startDate, DateOnly endDate);
    Task<Result<CurrentForecast>> GetCurrentForecast(string city);
    
}