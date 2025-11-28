using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;

public record OpenMeteoDailyMeanResponseDto
{
    // Meta data
    
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }
    
    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; init; }
    
    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; init; }
    
    [JsonPropertyName("timezone")]
    public string Timezone  { get; init; }
    
    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; init; }
    
    [JsonPropertyName("elevation")]
    public double Elevation { get; init; }
    
    // Weather forecast data (mean)
    
    [JsonPropertyName("daily_units")]
    public DailyUnitsMeanDto DailyUnits { get; init; }
    
    [JsonPropertyName("daily")]
    public DailyDataMeanDto Daily { get; init; }

}