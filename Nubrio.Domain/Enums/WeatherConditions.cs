namespace Nubrio.Domain.Enums;

public enum WeatherConditions
{
    Unknown, // Если не пришел код погоды
    Clear,
    PartlyCloudy,
    Cloudy,
    Fog,
    Drizzle, // Изморозь
    LightRain,
    Rain,
    HeavyRain,
    LightSnow,
    Snow,
    HeavySnow,
    Hailstorm, // Град
    Thunderstorm // Гроза
}