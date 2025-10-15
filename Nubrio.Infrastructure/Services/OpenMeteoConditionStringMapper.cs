using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;

namespace Nubrio.Infrastructure.Services;

public partial class OpenMeteoConditionStringMapper : IConditionStringMapper
{
    private static readonly ConcurrentDictionary<WeatherConditions, string> _stringWmoCache = new();
    private readonly ILogger<OpenMeteoConditionStringMapper> _logger;

    public OpenMeteoConditionStringMapper(ILogger<OpenMeteoConditionStringMapper> logger)
    {
        _logger = logger;
    }

    public string From(WeatherConditions condition)
    {
        // Проверяем есть ли значение в кеше
        if (_stringWmoCache.TryGetValue(condition, out var cachedValue))
        {
            _logger.LogDebug("Weather condition string: {Condition} already cached", condition);
            return cachedValue;
        }

        _logger.LogDebug("Weather condition string: {Condition} is not cached", condition);

        var name = condition.ToString();

        // Разделяем большие буквы пробелами: "PartlyCloudy" → "Partly Cloudy"
        var readable = MyRegex().Replace(name, " $1").ToLowerInvariant();

        // Кладем в кеш и возвращаем строку
        _stringWmoCache[condition] = readable;
        _logger.LogDebug("Weather condition string: {Condition} is cached", condition);
        return readable;
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex MyRegex();
}