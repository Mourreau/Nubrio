using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models.Daily;
using Nubrio.Domain.Models.Weekly;
using Nubrio.Infrastructure.Options;

namespace Nubrio.Infrastructure.Services;

public class MemoryWeatherForecastCache : IWeatherForecastCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;
    private readonly ILogger<MemoryWeatherForecastCache> _logger;


    public MemoryWeatherForecastCache(IMemoryCache cache,
        IOptions<WeatherCacheOptions> options,
        ILogger<MemoryWeatherForecastCache> logger)
    {
        _cache = cache;

        if (options.Value.TtlMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Value.TtlMinutes),
                "TTL minutes must be greater then 0");
        }
        _ttl = TimeSpan.FromMinutes(options.Value.TtlMinutes);
        _logger = logger;
    }

    public Task<DailyForecastMean?> GetDailyAsync(
        string provider,
        string cityNormalized,
        DateOnly date)
    {
        var cacheKey = BuildDailyKey(provider, cityNormalized, date);
        if (_cache.TryGetValue(cacheKey, out DailyForecastMean? forecast))
        {
            _logger.LogInformation(
                "Cache hit for daily forecast. Provider={Provider}, City={City}, Date={Date}",
                provider, cityNormalized, date);
            return Task.FromResult(forecast);
        }

        _logger.LogInformation(
            "Cache miss for daily forecast. Provider={Provider}, City={City}, Date={Date}",
            provider, cityNormalized, date);
        return Task.FromResult<DailyForecastMean?>(null);
    }

    public Task SetDailyAsync(DailyForecastMean forecast,
        string provider,
        string cityNormalized,
        DateOnly date)
    {
        var cacheKey = BuildDailyKey(provider, cityNormalized, date);
        _cache.Set(cacheKey, forecast, _ttl);
        _logger.LogInformation("Daily forecast has been cached. Provider={Provider}, City={City}, Date={Date}",
            provider, cityNormalized, date);
        return Task.CompletedTask;
    }

    public Task<WeeklyForecastMean?> GetWeeklyAsync(string provider,
        string cityNormalized,
        DateOnly weekStartDate)
    {
        var cacheKey = BuildWeeklyKey(provider, cityNormalized, weekStartDate);
        if (_cache.TryGetValue(cacheKey, out WeeklyForecastMean? forecast))
        {
            _logger.LogInformation(
                "Cache hit for weekly forecast. Provider={Provider}, City={City}, Date={Date}",
                provider, cityNormalized, weekStartDate);
            return Task.FromResult(forecast);
        }

        _logger.LogInformation(
            "Cache miss for weekly forecast. Provider={Provider}, City={City}, Date={Date}",
            provider, cityNormalized, weekStartDate);
        return Task.FromResult<WeeklyForecastMean?>(null);
    }

    public Task SetWeeklyAsync(WeeklyForecastMean forecast,
        string provider,
        string cityNormalized,
        DateOnly weekStartDate)
    {
        var cacheKey = BuildWeeklyKey(provider, cityNormalized, weekStartDate);
        _cache.Set(cacheKey, forecast, _ttl);
        _logger.LogInformation("Weekly forecast has been cached. Provider={Provider}, City={City}, Date={Date}",
            provider, cityNormalized, weekStartDate);
        return Task.CompletedTask;
    }


    private static string BuildDailyKey(string provider, string cityNormalized, DateOnly date)
        => $"weather:{provider}:{cityNormalized}:{date:yyyy-MM-dd}";

    private static string BuildWeeklyKey(string provider, string cityNormalized, DateOnly weekStartDate)
        => $"weather-week:{provider}:{cityNormalized}:{weekStartDate:yyyy-MM-dd}";
}