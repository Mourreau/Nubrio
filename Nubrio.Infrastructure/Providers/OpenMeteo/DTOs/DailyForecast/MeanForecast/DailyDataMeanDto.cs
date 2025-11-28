using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;

public class DailyDataMeanDto
{
    [JsonPropertyName("time")]
    public IReadOnlyList<string> Time { get; init; }
    
    [JsonPropertyName("temperature_2m_mean")]
    public IReadOnlyList<double> Temperature2mMean { get; init; }
    
    [JsonPropertyName("weather_code")]
    public IReadOnlyList<int> WeatherCode { get; init; }
    
}