using System.Globalization;
using System.Net;
using System.Text.Json;
using FluentResults;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;

namespace Nubrio.Infrastructure.Http.ForecastClient;

internal sealed class OpenMeteoForecastClient(HttpClient httpClient) : IForecastClient
{
    public async Task<Result<OpenMeteoDailyResponseDto>> GetOpenMeteoDailyAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken ct)
    {
        return Result.Fail($"Method {nameof(GetOpenMeteoDailyAsync)} is not implemented");
    }

    public async Task<Result<OpenMeteoDailyMeanResponseDto>> GetOpenMeteoDailyMeanAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken ct)
    {
        var path = string.Create(CultureInfo.InvariantCulture,
            $"v1/forecast?latitude={latitude}" +
            $"&longitude={longitude}" +
            $"&daily=temperature_2m_mean,weather_code" +
            $"&timezone=auto&" +
            $"start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}");
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

            var dto = await JsonSerializer.DeserializeAsync<OpenMeteoDailyMeanResponseDto>(s, options, ct);
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