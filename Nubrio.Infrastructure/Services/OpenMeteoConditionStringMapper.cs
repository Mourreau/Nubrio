using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;

namespace Nubrio.Infrastructure.Services;

public class OpenMeteoConditionStringMapper : IConditionStringMapper
{
    private static readonly ConcurrentDictionary<WeatherConditions, string> _stringWmoCache = new();
        
    public string From(WeatherConditions condition)
    {
        // Проверяем есть ли значение в кеше
        if (_stringWmoCache.TryGetValue(condition, out var cachedValue))
            return cachedValue;
        
        var name = condition.ToString();

        // Разделяем большие буквы пробелами: "PartlyCloudy" → "Partly Cloudy"
        var readable =  Regex.Replace(name, "(?<!^)([A-Z])", " $1").ToLower();
        
        // Кладем в кеш и возвращаем строку
        _stringWmoCache[condition] = readable;
        return readable;
    }
}