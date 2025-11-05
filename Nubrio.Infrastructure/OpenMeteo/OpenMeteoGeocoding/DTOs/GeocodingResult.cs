using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding.DTOs;

/// <summary>
/// Поля результата геокодинга
/// </summary>
public sealed class GeocodingResult
{ 
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("latitude")]
    public required double Latitude { get; init; }
    
    [JsonPropertyName("longitude")]
    public required double Longitude { get; init; }
    
    [JsonPropertyName("timezone")]
    public required string Timezone { get; init; }
    
    [JsonExtensionData]
    public Dictionary<string, object?>? Extra { get; init; }
}