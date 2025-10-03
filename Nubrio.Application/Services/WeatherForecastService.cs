using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherProvider _weatherProvider;
    private readonly IWeatherCodeTranslator _codeTranslator;

    public WeatherForecastService(IWeatherProvider weatherProvider, IWeatherCodeTranslator codeTranslator)
    {
        _weatherProvider = weatherProvider;
        _codeTranslator = codeTranslator;
    }

    public async Task<Result<CurrentForecast>> GetCurrentForecastAsync(string city)
    {
        var fetchResult = await _weatherProvider.TryGetCurrentForecast(city, out CurrentResponseDto currentForecast);

        if (fetchResult.IsSuccess)
        {
            var forecastModel = new CurrentForecast(
                DateTimeOffset.Now, 
                Guid.Empty, 
                currentForecast.Temperature2m,
                _codeTranslator.Translate(currentForecast.WeatherCode));
            
            
            return Result.Ok(forecastModel);
        }

        return Result.Fail("Current forecast could not be retrieved");
    }

    public Task<Result<DailyForecast>> GetDailyForecastByDateAsync(Coordinates coordinates, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DailyForecast>> GetDailyForecastByStartEndDateAsync(Coordinates coordinates, DateOnly startDate, DateOnly endDate)
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