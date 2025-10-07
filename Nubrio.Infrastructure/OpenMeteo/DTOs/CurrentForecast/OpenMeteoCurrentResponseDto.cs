using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

public class OpenMeteoCurrentResponseDto
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
    public string Timezone { get; init; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; init; }

    [JsonPropertyName("elevation")] 
    public int Elevation { get; init; }

// Weather forecast data

    [JsonPropertyName("current_units")] 
    public CurrentUnitsDto CurrentUnits { get; init; }

    [JsonPropertyName("current")] 
    public CurrentDataDto Current { get; init; }
}