using FluentResults;
using Nubrio.Application.Common.Errors;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Weekly;

namespace Nubrio.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IForecastProvider _forecastProvider;
    private readonly IGeocodingProvider _geocodingProvider;
    private readonly IClock _clock;
    private readonly ITimeZoneResolver _timeZoneResolver;
    private readonly ILanguageResolver _languageResolver;

    public WeatherForecastService(
        IForecastProvider forecastProvider,
        IGeocodingProvider geocodingProvider,
        IClock clock,
        ITimeZoneResolver timeZoneResolver,
        ILanguageResolver languageResolver)
    {
        _forecastProvider = forecastProvider;
        _geocodingProvider = geocodingProvider;
        _clock = clock;
        _timeZoneResolver = timeZoneResolver;
        _languageResolver = languageResolver;
    }
    

    public async Task<Result<DailyForecastMeanDto>> GetDailyForecastByDateAsync(string city, DateOnly date,
        CancellationToken cancellationToken)
    {
        // 0. Проверка входных данных
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail(new Error("City cannot be null or whitespace")
                .WithMetadata("ServiceCode", AppErrorCode.EmptyCity)
            );

        // 0.5. Проверка на язык
        var language = _languageResolver.Resolve(city);

        var forecastDateOffset = DateOnly
            .FromDateTime(_clock.UtcNow.UtcDateTime)
            .AddMonths(3);

        if (date > forecastDateOffset)
        {
            return Result.Fail(new Error($"Date must not be later than {forecastDateOffset}")
                .WithMetadata("ServiceCode", AppErrorCode.DateOutOfRange)
            );
        }

        // 1. Геокодинг
        var geocodingResult = await _geocodingProvider.ResolveAsync(city, language, cancellationToken);

        if (geocodingResult.IsFailed)
            return Result.Fail(geocodingResult.Errors);


        // 2. Погода по дате
        var providerResult = await _forecastProvider.GetDailyForecastMeanAsync(
            geocodingResult.Value, date, cancellationToken);

        if (providerResult.IsFailed)
            return Result.Fail(providerResult.Errors);

        // 3. Получение локального часового пояса
        var timeZoneResolveResult = _timeZoneResolver.GetTimeZoneInfoById(geocodingResult.Value.TimeZoneIana);

        if (timeZoneResolveResult.IsFailed)
            return Result.Fail(timeZoneResolveResult.Errors);

        var localFetched = TimeZoneInfo.ConvertTime(
            providerResult.Value.FetchedAtUtc, timeZoneResolveResult.Value);

        var result = new DailyForecastMeanDto
        {
            City = geocodingResult.Value.Name,
            Date = date,
            Condition = providerResult.Value.Condition,
            FetchedAt = localFetched,
            TemperatureMean = providerResult.Value.TemperatureMean
        };

        return Result.Ok(result);
    }


    public async Task<Result<WeeklyForecastMeanDto>> GetWeeklyForecastAsync(string city,
        CancellationToken cancellationToken)
    {
        // 1. Геокодинг
        var geocodingResult = await ResolveLocationAsync(city, cancellationToken);
        if (geocodingResult.IsFailed) return Result.Fail(geocodingResult.Errors);

        // 2. Недельный прогноз
        var providerResult =
            await _forecastProvider.GetWeeklyForecastMeanAsync(geocodingResult.Value, cancellationToken);
        if (providerResult.IsFailed) return Result.Fail(providerResult.Errors);

        // 3. Получение локального часового пояса
        var timeZoneResolveResult = _timeZoneResolver.GetTimeZoneInfoById(geocodingResult.Value.TimeZoneIana);

        if (timeZoneResolveResult.IsFailed)
            return Result.Fail(timeZoneResolveResult.Errors);

        var localFetched = TimeZoneInfo.ConvertTime(
            providerResult.Value.FetchedAtUtc, timeZoneResolveResult.Value);

        var result = new WeeklyForecastMeanDto
        {
            City = geocodingResult.Value.Name,
            Days = GetDays(providerResult.Value),
            FetchedAt = localFetched
        };

        return Result.Ok(result);
    }

    private IReadOnlyList<DaysDto> GetDays(WeeklyForecastMean weeklyForecast)
    {
        var days = new List<DaysDto>();

        foreach (var day in weeklyForecast.DailyForecasts)
        {
            days.Add(
                new DaysDto(
                    Condition: day.Condition,
                    Date: day.Date,
                    TemperatureMean: day.TemperatureMean
                ));
        }

        return days;
    }


    private async Task<Result<Location>> ResolveLocationAsync(string city, CancellationToken cancellationToken)
    {
        // 0. Проверка входных данных
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail(new Error("City cannot be null or whitespace")
                .WithMetadata(ProviderErrorMetadataKeys.ServiceCode, AppErrorCode.EmptyCity)
            );

        // 0.5. Проверка на язык
        var language = _languageResolver.Resolve(city);

        // 1. Геокодинг
        var geocodingResult = await _geocodingProvider.ResolveAsync(city, language, cancellationToken);

        if (geocodingResult.IsFailed)
            return Result.Fail(geocodingResult.Errors); // Все остальные ошибки отдаем как internal

        return Result.Ok(geocodingResult.Value);
    }
}