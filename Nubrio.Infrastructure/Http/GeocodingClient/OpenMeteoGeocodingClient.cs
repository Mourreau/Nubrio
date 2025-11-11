using System.Globalization;
using System.Net;
using System.Text.Json;
using FluentResults;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding.DTOs;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;

namespace Nubrio.Infrastructure.Http.GeocodingClient;

internal sealed class OpenMeteoGeocodingClient(HttpClient httpClient) : IGeocodingClient
{
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
                        $"Open-Meteo responded {response.StatusCode} for {response.RequestMessage!.RequestUri}")
                    .WithMetadata("Code", response.StatusCode == HttpStatusCode.TooManyRequests
                        ? OpenMeteoErrorCodes.TooManyRequests
                        : OpenMeteoErrorCodes.Http5xx));

            await using var s = await response.Content.ReadAsStreamAsync(ct);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            var dto = await JsonSerializer.DeserializeAsync<OpenMeteoGeocodingResponse>(s, options, ct);
            if (dto is null)
                return Result.Fail(new Error("Deserialization returned null")
                    .WithMetadata("Code", OpenMeteoErrorCodes.Deserialization));

            return Result.Ok(dto);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Result.Fail(new Error("Timeout").WithMetadata("Code", OpenMeteoErrorCodes.Timeout));
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail(new Error("Network error").CausedBy(ex)
                .WithMetadata("Code", OpenMeteoErrorCodes.NetworkError));
        }
        catch (JsonException ex)
        {
            return Result.Fail(new Error("Deserialization failed").CausedBy(ex)
                .WithMetadata("Code", OpenMeteoErrorCodes.Deserialization));
        }
    }
}