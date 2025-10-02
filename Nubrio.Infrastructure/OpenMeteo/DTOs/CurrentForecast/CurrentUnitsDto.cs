using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

public class CurrentUnitsDto
{
    [JsonPropertyName("time")]
    public string Time { get; set; }
    
    [JsonPropertyName("interval")]
    public string Interval {get; set;}
    
    [JsonPropertyName("temperature_2m")]
    public string Temperature2m {get; set;}
        
    [JsonPropertyName("weather_code")]
    public string WeatherCode {get; set;}
    
}