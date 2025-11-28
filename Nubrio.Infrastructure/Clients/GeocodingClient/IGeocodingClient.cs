using FluentResults;
using Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoGeocoding.DTOs;

namespace Nubrio.Infrastructure.Clients.GeocodingClient;

public interface IGeocodingClient
{
    Task<Result<OpenMeteoGeocodingResponse>> GeocodeAsync(
        string city, int count, string language, CancellationToken ct);
}

// https://geocoding-api.open-meteo.com/v1/search?name=Berlin&count=1&language=en&format=json



