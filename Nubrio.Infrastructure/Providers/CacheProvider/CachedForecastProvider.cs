using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Nubrio.Domain.Models.Weekly;

namespace Nubrio.Infrastructure.Providers.CacheProvider;

public class CachedForecastProvider : IForecastProvider
{
    private readonly IWeatherForecastCache _cache;
    private readonly string _providerKey;
    private readonly IForecastProvider _forecastProvider;
    private readonly IClock _clock;
    private readonly ILogger<CachedForecastProvider> _logger;

    public CachedForecastProvider(IWeatherForecastCache cache,
        IForecastProvider forecastProvider,
        string providerKey,
        ILogger<CachedForecastProvider> logger,
        IClock clock)
    {
        _cache = cache;
        _forecastProvider = forecastProvider;
        _providerKey = providerKey;
        _logger = logger;
        _clock = clock;
    }


    public async Task<Result<DailyForecastMean>> GetDailyForecastMeanAsync(
        Location location,
        DateOnly date,
        CancellationToken cancellationToken)
    {

        var externalLocationId = location.ExternalLocationId.Value;
        var cachedForecast = await _cache.GetDailyAsync(_providerKey, externalLocationId, date);

        if (cachedForecast is not null)
            return Result.Ok(cachedForecast);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Cache miss. External forecast provider has been called. Provider - {Provider}, City - {City}, Date - {Date}",
            _providerKey, location.Name, date);

        var providerResult = await _forecastProvider.GetDailyForecastMeanAsync(location, date, cancellationToken);

        if (providerResult.IsFailed)
            return Result.Fail(providerResult.Errors);

        var forecast = providerResult.Value;

        stopwatch.Stop();
        _logger.LogInformation(
            "External forecast provider call has ended. Provider - {Provider}, City - {City}, Date - {Date}, Time elapsed - {Time}",
            _providerKey, location.Name, date, stopwatch.Elapsed);

        await _cache.SetDailyAsync(forecast, _providerKey, externalLocationId, date);

        return Result.Ok(forecast);
    }

    public async Task<Result<WeeklyForecastMean>> GetWeeklyForecastMeanAsync(Location location,
        CancellationToken cancellationToken)
    {
        var externalLocationId = location.ExternalLocationId.Value;
        var weekStartDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime);

        var cachedForecast = await _cache.GetWeeklyAsync(_providerKey, externalLocationId, weekStartDate);

        if (cachedForecast is not null)
            return Result.Ok(cachedForecast);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Cache miss. External forecast provider has been called. Provider - {Provider}, City - {City}, Date - {Date}",
            _providerKey, location.Name, weekStartDate);

        var providerResult = await _forecastProvider.GetWeeklyForecastMeanAsync(location, cancellationToken);

        if (providerResult.IsFailed)
            return Result.Fail(providerResult.Errors);

        var forecast = providerResult.Value;

        stopwatch.Stop();
        _logger.LogInformation(
            "External forecast provider call has ended. Provider - {Provider}, City - {City}, Date - {Date}, Time elapsed - {Time}",
            _providerKey, location.Name, weekStartDate, stopwatch.Elapsed);

        await _cache.SetWeeklyAsync(forecast, _providerKey, externalLocationId, weekStartDate);

        return Result.Ok(forecast);
    }

    public Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
        => _forecastProvider.GetCurrentForecastAsync(location, cancellationToken);
    
}