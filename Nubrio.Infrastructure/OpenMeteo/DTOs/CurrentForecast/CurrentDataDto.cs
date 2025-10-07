using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

public class CurrentDataDto
{
    /// <summary>
    /// Response date and time in format: "yyyy-MM-dd'T'HH:mm".
    /// </summary>
    [JsonPropertyName("time")]
    public string Time { get; set; }
    
    [JsonPropertyName("interval")]
    public int Interval { get; set; }
    
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }
    
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}