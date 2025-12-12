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
        var normalizedCity = NormalizeCityName(location.Name);

        var cachedForecast = await _cache.GetDailyAsync(
            _providerKey, normalizedCity, date);

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

        await _cache.SetDailyAsync(forecast, _providerKey, normalizedCity, date);

        return Result.Ok(forecast);
    }

    public async Task<Result<WeeklyForecastMean>> GetWeeklyForecastMeanAsync(Location location,
        CancellationToken cancellationToken)
    {
        var normalizedCity = NormalizeCityName(location.Name);
        var weekStartDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime);

        var cachedForecast = await _cache.GetWeeklyAsync(_providerKey, normalizedCity, weekStartDate);

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

        await _cache.SetWeeklyAsync(forecast, _providerKey, normalizedCity, weekStartDate);

        return Result.Ok(forecast);
    }

    public Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
        => _forecastProvider.GetCurrentForecastAsync(location, cancellationToken);

    private static string NormalizeCityName(string cityName)
    {
        return cityName.Trim().ToLowerInvariant().Replace(" ", "-");
    }
}