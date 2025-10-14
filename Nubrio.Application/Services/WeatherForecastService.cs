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
    private readonly IClock _clock;
    private readonly IConditionStringMapper _conditionStringMapper;
    private readonly ITimeZoneResolver _timeZoneResolver;

    public WeatherForecastService(
        IWeatherProvider weatherProvider,
        IGeocodingService geocodingService,
        IClock clock,
        IConditionStringMapper conditionStringMapper, 
        ITimeZoneResolver timeZoneResolver)
    {
        _weatherProvider = weatherProvider;
        _geocodingService = geocodingService;
        _clock = clock;
        _conditionStringMapper = conditionStringMapper;
        _timeZoneResolver = timeZoneResolver;
    }

    public async Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail("City must not be empty or whitespace");

        var geocodingResult = await _geocodingService.ResolveAsync(city, cancellationToken);

        if (!geocodingResult.IsSuccess)
            return Result.Fail(geocodingResult.Errors);

        var fetchResult = await _weatherProvider.GetCurrentForecastAsync(geocodingResult.Value, cancellationToken);

        if (fetchResult.IsFailed)
            return Result.Fail(fetchResult.Errors);
        
        var timeZoneResolveResult = _timeZoneResolver.GetTimeZoneInfo(geocodingResult.Value.TimeZoneIana);

        if (timeZoneResolveResult.IsFailed)
            return Result.Fail(timeZoneResolveResult.Errors);
        
        
        var localDateObserved = TimeZoneInfo.ConvertTime(
            fetchResult.Value.ObservedAt,  timeZoneResolveResult.Value);
        
        var localFetched = TimeZoneInfo.ConvertTime(
            _clock.UtcNow, timeZoneResolveResult.Value);
        
        
        var result = new CurrentForecastDto
        {
            City = geocodingResult.Value.Name,
            Date = localDateObserved,
            Condition = _conditionStringMapper.From(fetchResult.Value.Condition),
            Temperature = fetchResult.Value.Temperature,
            FetchedAt = localFetched
        };

        return Result.Ok(result);
    }

    public Task<Result<DailyForecastDto>> GetDailyForecastByDateAsync(string city, DateOnly date,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DailyForecastDto>> GetDailyForecastRangeAsync(string city,
        DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}