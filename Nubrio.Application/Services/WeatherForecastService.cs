using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherProvider _weatherProvider;
    private readonly IGeocodingProvider _geocodingProvider;
    private readonly IClock _clock;
    private readonly IConditionStringMapper _conditionStringMapper;
    private readonly ITimeZoneResolver _timeZoneResolver;

    public WeatherForecastService(
        IWeatherProvider weatherProvider,
        IGeocodingProvider geocodingProvider,
        IClock clock,
        IConditionStringMapper conditionStringMapper, 
        ITimeZoneResolver timeZoneResolver)
    {
        _weatherProvider = weatherProvider;
        _geocodingProvider = geocodingProvider;
        _clock = clock;
        _conditionStringMapper = conditionStringMapper;
        _timeZoneResolver = timeZoneResolver;
    }

    public async Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city,
        CancellationToken cancellationToken)
    {
        // 0. Проверка входных данных
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail("City must not be empty or whitespace");

        // 1. Геокодинг
        var geocodingResult = await _geocodingProvider.ResolveAsync(city, language, cancellationToken);

        if (!geocodingResult.IsSuccess)
            return Result.Fail(geocodingResult.Errors);

        // 2. Текущая погода
        var providerResult = await _weatherProvider.GetCurrentForecastAsync(geocodingResult.Value, cancellationToken);

        if (providerResult.IsFailed)
            return Result.Fail(providerResult.Errors);
        
        // 3. Получение локального часового пояса
        var timeZoneResolveResult = _timeZoneResolver.GetTimeZoneInfo(geocodingResult.Value.TimeZoneIana);

        if (timeZoneResolveResult.IsFailed)
            return Result.Fail(timeZoneResolveResult.Errors);
        
        
        var localDateObserved = TimeZoneInfo.ConvertTime(
            providerResult.Value.ObservedAt,  timeZoneResolveResult.Value);
        
        var localFetched = TimeZoneInfo.ConvertTime(
            _clock.UtcNow, timeZoneResolveResult.Value);
        
        // 4. Перевод в DTO
        var result = new CurrentForecastDto
        {
            City = geocodingResult.Value.Name,
            Date = localDateObserved,
            Condition = _conditionStringMapper.From(providerResult.Value.Condition),
            Temperature = providerResult.Value.Temperature,
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