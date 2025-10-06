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

    public WeatherForecastService(
        IWeatherProvider weatherProvider,
        IGeocodingService geocodingService,
        IClock clock,
        IConditionStringMapper conditionStringMapper)
    {
        _weatherProvider = weatherProvider;
        _geocodingService = geocodingService;
        _clock = clock;
        _conditionStringMapper = conditionStringMapper;
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

        var result = new CurrentForecastDto
        {
            City = geocodingResult.Value.Name,
            Date = fetchResult.Value.ObservedAt,
            Condition = _conditionStringMapper.From(fetchResult.Value.Condition),
            Temperature = fetchResult.Value.Temperature,
            FetchedAt = _clock.UtcNow
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