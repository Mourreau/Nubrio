using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;

public class DailyDataDto
{
    [JsonPropertyName("time")]
    public IReadOnlyList<string> Time { get; init; }
    
    [JsonPropertyName("weather_code")]
    public IReadOnlyList<int> WeatherCode { get; init; }
    
    [JsonPropertyName("temperature_2m_max")]
    public IReadOnlyList<double> Temperature2mMax { get; init; }
    
    [JsonPropertyName("temperature_2m_min")]
    public IReadOnlyList<double> Temperature2mMin { get; init; }
}