using System.Globalization;
using FluentResults;
using Microsoft.Extensions.Options;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoGeocoding.DTOs;
using Nubrio.Infrastructure.Providers.ProviderBase;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Clients.GeocodingClient;

internal sealed class OpenMeteoGeocodingClient : ExternalApiClientBase<GeocodingProviderErrorCodes>, IGeocodingClient
{

    public OpenMeteoGeocodingClient(HttpClient httpClient, IOptions<ProviderOptions> options)
        : this(httpClient, CreateProviderInfo(options))
    {
    }

    private OpenMeteoGeocodingClient(HttpClient httpClient, ProviderInfo info)
        : base(httpClient, info, new GeocodingProviderErrorCodes(info))
    {
    }


    public async Task<Result<OpenMeteoGeocodingResponse>> GeocodeAsync(
        string city, int count, string language, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(language))
            return Result.Fail(new Error("City and language are required."));

        // Экранируем строку параметра 'City'
        var encodedCity = Uri.EscapeDataString(city);

        var path = string.Create(CultureInfo.InvariantCulture,
            $"v1/search?name={encodedCity}&count={count}&language={language}&format=json");
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(HttpClient.BaseAddress!, path));

        var result = await SendAndDeserializeAsync<OpenMeteoGeocodingResponse>(request, ct);
        if (result.IsFailed) return Result.Fail(result.Errors);

        var dto = result.Value;

        if (dto.Results is null || dto.Results.Count == 0)
        {
            return BuildError(
                $"No location found for city '{city}'",
                request.RequestUri!,
                ErrorCodes.NotFound());
        }

        return Result.Ok(dto);
    }

    private static ProviderInfo CreateProviderInfo(IOptions<ProviderOptions> options)
    {
        var cfg = options.Value.OpenMeteo;

        return new ProviderInfo
        (
            providerKey: nameof(ProviderOptions.OpenMeteo),
            name: cfg.Name,
            service: nameof(OpenMeteoGeocodingClient),
            baseUrl: cfg.GeocodingBaseUrl
        );
    }
}