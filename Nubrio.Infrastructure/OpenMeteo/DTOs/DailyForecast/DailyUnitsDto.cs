using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;

public class DailyUnitsDto
{
    [JsonPropertyName("time")]
    public string Time { get; set; }
    
    [JsonPropertyName("weather_code")]
    public string WeatherCode {get; set;}
    
    [JsonPropertyName("temperature_2m_max")]
    public string Temperature2mMax {get; set;}
    
    [JsonPropertyName("temperature_2m_min")]
    public string Temperature2mMin {get; set;}
}