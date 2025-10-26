using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;

public class DailyUnitsMeanDto
{
    [JsonPropertyName("time")]
    public string Time { get; init; }
    
    [JsonPropertyName("temperature_2m_mean")]
    public string Temperature2mMean {get; init;}
    
    [JsonPropertyName("weather_code")]
    public string WeatherCode {get; init;}

}