using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.CurrentForecast;

public class CurrentDataDto
{
    /// <summary>
    /// Response date and time in format: "yyyy-MM-dd'T'HH:mm".
    /// </summary>
    [JsonPropertyName("time")]
    public string Time { get; init; }
    
    [JsonPropertyName("interval")]
    public int Interval { get; init; }
    
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; init; }
    
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; init; }
}