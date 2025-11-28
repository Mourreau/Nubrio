using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast;

public class DailyUnitsDto
{
    [JsonPropertyName("time")]
    public string Time { get; init; }
    
    [JsonPropertyName("weather_code")]
    public string WeatherCode {get; init;}
    
    [JsonPropertyName("temperature_2m_max")]
    public string Temperature2mMax {get; init;}
    
    [JsonPropertyName("temperature_2m_min")]
    public string Temperature2mMin {get; init;}
}