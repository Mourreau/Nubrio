using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.Http.GeocodingClient;
using Nubrio.Infrastructure.Options;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoGeocoding;

public class OpenMeteoGeocodingProvider : IGeocodingProvider
{
    private readonly IGeocodingClient _geocodingClient;
    private const int CityCount = 1;

    public OpenMeteoGeocodingProvider(IGeocodingClient geocodingClient)
    {
        _geocodingClient = geocodingClient;
    }

    public async Task<Result<Location>> ResolveAsync(string city, string language, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail("City cannot be null or whitespace");

        var openMeteoGeoResponse = await _geocodingClient.GeocodeAsync(city, CityCount, language, cancellationToken);

        if (openMeteoGeoResponse.IsFailed)
            return Result.Fail(openMeteoGeoResponse.Errors);

        var responseDto = openMeteoGeoResponse.Value;

        if (responseDto.Results is null or { Count: 0 })
            return Result.Fail($"No location found for city '{city}'.");

        var resultDto = responseDto.Results[0];

        if (string.IsNullOrEmpty(resultDto.Timezone))
        {
            return Result.Fail($"No timezone found for city '{city}'.");
        }

        var location = new Location(
            Guid.NewGuid(),
            resultDto.Name,
            new Coordinates(resultDto.Latitude, resultDto.Longitude),
            resultDto.Timezone,
            new ExternalLocationId(
                OpenMeteoProviderInfo.OpenMeteoGeocoding,
                resultDto.Id
            ));


        return Result.Ok(location);
    }
}