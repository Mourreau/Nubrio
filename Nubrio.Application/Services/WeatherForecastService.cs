using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherProvider _weatherProvider;
    private readonly IGeocodingService _geocodingService;
    
    public WeatherForecastService(IWeatherProvider weatherProvider, IGeocodingService geocodingService)
    {
        _weatherProvider = weatherProvider;
        _geocodingService = geocodingService;
    }

    public async Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city,
        CancellationToken cancellationToken)
    {
        var coordinates = _geocodingService.Resolve(city).Coordinates;
        var fetchResult = await _weatherProvider.GetCurrentForecastAsync(coordinates, cancellationToken);

        if (fetchResult.IsSuccess)
        {
            var result = new CurrentForecastDto
            {
                City = city,
                Date = fetchResult.Value.ObservedAt,
                Condition = fetchResult.Value.Condition.ToString(),
                Temperature = fetchResult.Value.Temperature,
                FetchedAt = DateTimeOffset.Now
            };
            
            return Result.Ok(result);
        }

        return Result.Fail(fetchResult.Errors);
    }

    public Task<Result<DailyForecastDto>> GetDailyForecastByDateAsync(Coordinates coordinates, DateOnly date,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DailyForecastDto>> GetDailyForecastByStartEndDateAsync(Coordinates coordinates,
        DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
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