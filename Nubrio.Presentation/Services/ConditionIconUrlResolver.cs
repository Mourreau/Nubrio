using Nubrio.Domain.Enums;
using Nubrio.Presentation.Interfaces;

namespace Nubrio.Presentation.Services;

public class ConditionIconUrlResolver : IConditionIconUrlResolver
{
    public string Resolve(WeatherConditions condition)
    {
        return condition switch
        {
            WeatherConditions.Clear => "/icons/airy/clear@4x.png",
            WeatherConditions.PartlyCloudy => "/icons/airy/mostly-clear@4x.png",
            WeatherConditions.Cloudy => "/icons/airy/partly-cloudy@4x.png",
            WeatherConditions.Fog => "/icons/airy/fog@4x.png",
            WeatherConditions.Drizzle => "/icons/airy/light-drizzle@4x.png",
            WeatherConditions.LightRain => "/icons/airy/light-rain@4x.png",
            WeatherConditions.Rain => "/icons/airy/moderate-rain@4x.png",
            WeatherConditions.HeavyRain => "/icons/airy/heavy-rain@4x.png",
            WeatherConditions.LightSnow => "/icons/airy/slight-snowfall@4x.png",
            WeatherConditions.Snow => "/icons/airy/moderate-snowfall@4x.png",
            WeatherConditions.HeavySnow => "/icons/airy/heavy-snowfall@4x.png",
            WeatherConditions.Hailstorm => "/icons/airy/thunderstorm-with-hail@4x.png",
            WeatherConditions.Thunderstorm => "/icons/airy/thunderstorm@4x.png",
            _ => "/icons/airy/unknown.png"
        };
    }
}