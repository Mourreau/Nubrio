using Nubrio.Domain.Models.Daily;
using Nubrio.Domain.Models.Weekly;

namespace Nubrio.Application.Interfaces;

public interface IWeatherForecastCache
{
    Task<DailyForecastMean?> GetDailyAsync(
        string provider,
        string cityNormalized,
        DateOnly date);

    Task SetDailyAsync(
        DailyForecastMean forecast,
        string provider,
        string cityNormalized,
        DateOnly date);


    Task<WeeklyForecastMean?> GetWeeklyAsync(
        string provider,
        string cityNormalized,
        DateOnly weekStartDate);


    Task SetWeeklyAsync(
        WeeklyForecastMean forecast,
        string provider,
        string cityNormalized,
        DateOnly weekStartDate);
}