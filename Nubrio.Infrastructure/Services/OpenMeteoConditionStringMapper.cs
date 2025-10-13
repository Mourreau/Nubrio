using System.Text.RegularExpressions;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;

namespace Nubrio.Infrastructure.Services;

public class OpenMeteoConditionStringMapper : IConditionStringMapper
{
    public string From(WeatherConditions condition)
    {
        var name = condition.ToString();

        // Разделяем большие буквы пробелами: "PartlyCloudy" → "Partly Cloudy"
        return Regex.Replace(name, "(?<!^)([A-Z])", " $1");
    }
}