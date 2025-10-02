using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;

public class OpenMeteoDailyResponseDto
{
    // Meta data
    
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
    
    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }
    
    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }
    
    [JsonPropertyName("timezone")]
    public string Timezone  { get; set; }
    
    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }
    
    [JsonPropertyName("elevation")]
    public int Elevation { get; set; }
    
    // Weather forecast data
    
    [JsonPropertyName("daily_units")]
    public DailyUnitsDto DailyUnits { get; set; }
    
    [JsonPropertyName("daily")]
    public DailyDataDto Daily { get; set; }

}