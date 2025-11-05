using System.Text.Json.Serialization;

namespace Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding.DTOs;

/// <summary>
/// Ответ внешнего API Open Meteo Geocoding сериализованный из JSON
/// </summary>
public sealed class OpenMeteoGeocodingResponse
{
    [JsonPropertyName("results")]
    public List<GeocodingResult>? Results { get; init; }
}