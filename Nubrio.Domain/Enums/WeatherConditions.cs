namespace Nubrio.Domain.Enums;

public enum WeatherConditions
{
    Unknown, // Если не пришел код погоды
    Clear,
    PartlyCloudy,
    Cloudy,
    Fog,
    Drizzle, // Морось
    LightRain,
    Rain,
    HeavyRain,
    LightSnow,
    Snow,
    HeavySnow,
    Hailstorm, // Град
    Thunderstorm // Гроза
}