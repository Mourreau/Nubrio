using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding.DTOs;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;
using Nubrio.Infrastructure.Options;

namespace Nubrio.Infrastructure.Http.GeocodingClient;

internal sealed class OpenMeteoGeocodingClient(HttpClient httpClient) : IGeocodingClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Result<OpenMeteoGeocodingResponse>> GeocodeAsync(
        string city, int count, string language, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(language))
            return Result.Fail(new Error($"City and language are required"));

        // Экранируем строку параметра 'City'
        var encodedCity = Uri.EscapeDataString(city);

        var path = string.Create(CultureInfo.InvariantCulture,
            $"v1/search?name={encodedCity}&count={count}&language={language}&format=json");
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(httpClient.BaseAddress!, path));

        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
                return Result.Fail(new Error(
                        $"Open-Meteo responded {response.StatusCode} for {request.RequestUri}")
                    .WithMetadata("Code", response.StatusCode == HttpStatusCode.TooManyRequests
                        ? OpenMeteoErrorCodes.TooManyRequests
                        : OpenMeteoErrorCodes.Http5xx)
                    .WithMetadata("Uri", request.RequestUri!.ToString())
                    .WithMetadata("Provider", OpenMeteoProviderInfo.OpenMeteoGeocoding));

            await using var s = await response.Content.ReadAsStreamAsync(ct);

            var dto = await JsonSerializer.DeserializeAsync<OpenMeteoGeocodingResponse>(s, JsonOptions, ct);
            if (dto is null)
                return Result.Fail(new Error("Deserialization returned null")
                    .WithMetadata("Code", OpenMeteoErrorCodes.Deserialization)
                    .WithMetadata("Uri", request.RequestUri!.ToString())
                    .WithMetadata("Provider", OpenMeteoProviderInfo.OpenMeteoGeocoding));

            return Result.Ok(dto);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Result.Fail(new Error("Timeout").WithMetadata("Code", OpenMeteoErrorCodes.Timeout)
                .WithMetadata("Uri", request.RequestUri!.ToString())
                .WithMetadata("Provider", OpenMeteoProviderInfo.OpenMeteoGeocoding));
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail(new Error("Network error").CausedBy(ex)
                .WithMetadata("Code", OpenMeteoErrorCodes.NetworkError)
                .WithMetadata("Uri", request.RequestUri!.ToString())
                .WithMetadata("Provider", OpenMeteoProviderInfo.OpenMeteoGeocoding));
        }
        catch (JsonException ex)
        {
            return Result.Fail(new Error("Deserialization failed").CausedBy(ex)
                .WithMetadata("Code", OpenMeteoErrorCodes.Deserialization)
                .WithMetadata("Provider", OpenMeteoProviderInfo.OpenMeteoGeocoding)
                .WithMetadata("Uri", request.RequestUri!.ToString())
                .WithMetadata("ExceptionMessage", ex.Message)
                .WithMetadata("ExceptionPath", ex.Path)
            );
        }
    }
}