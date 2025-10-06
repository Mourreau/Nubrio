using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherProvider _weatherProvider;
    
    public WeatherForecastService(IWeatherProvider weatherProvider)
    {
        _weatherProvider = weatherProvider;
    }

    public async Task<Result<CurrentForecastResponseDto>> GetCurrentForecastAsync(string city)
    {
        var fetchResult = await _weatherProvider.GetCurrentForecast(city);

        if (fetchResult.IsSuccess)
        {
            var result = new CurrentForecastResponseDto
            {
                City = city,
                Date = fetchResult.Value.ObservedAt,
                Condition = fetchResult.Value.Condition.ToString(),
                Temperature = fetchResult.Value.Temperature,
                IconUrl = "IconUrl",
                Source = "Open Meteo",
                FetchedAt = DateTimeOffset.Now,
            };
            
            return Result.Ok(result);
        }

        return Result.Fail("Current forecast could not be retrieved");
    }

    public Task<Result<DailyForecastResponseDto>> GetDailyForecastByDateAsync(Coordinates coordinates, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DailyForecastResponseDto>> GetDailyForecastByStartEndDateAsync(
        Coordinates coordinates, DateOnly startDate, DateOnly endDate)
    {
        throw new NotImplementedException();
    }


    private DateOnly GetDateTimeFromString(string dateString)
    {
        string format = "yyyy-MM-dd'T'HH:mm";
        
        DateTime fullDateTime = DateTime.ParseExact(dateString, format, null);

        DateOnly dateOnly = DateOnly.FromDateTime(fullDateTime);

        return dateOnly;
    }
}