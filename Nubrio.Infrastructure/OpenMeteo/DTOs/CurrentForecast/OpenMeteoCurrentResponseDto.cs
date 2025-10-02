using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

public class OpenMeteoCurrentResponseDto
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
    public string Timezone { get; set; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    [JsonPropertyName("elevation")] 
    public int Elevation { get; set; }

// Weather forecast data

    [JsonPropertyName("current_units")] 
    public CurrentUnitsDto CurrentUnits { get; set; }

    [JsonPropertyName("current")] 
    public CurrentDataDto Current { get; set; }
}