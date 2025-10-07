using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

public class CurrentUnitsDto
{
    [JsonPropertyName("time")]
    public string Time { get; init; }
    
    [JsonPropertyName("interval")]
    public string Interval {get; init;}
    
    [JsonPropertyName("temperature_2m")]
    public string Temperature2m {get; init;}
        
    [JsonPropertyName("weather_code")]
    public string WeatherCode {get; init;}
    
}