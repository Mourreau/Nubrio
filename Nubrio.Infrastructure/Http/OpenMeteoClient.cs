using System.Net;
using System.Text.Json;
using FluentResults;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;

namespace Nubrio.Infrastructure.Http;

internal sealed class OpenMeteoClient(IHttpClientFactory factory) : IOpenMeteoClient
{
    private readonly HttpClient _httpClient = factory.CreateClient("openmeteo");

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
        var path = $"v1/forecast?latitude={latitude}" +
                   $"&longitude={longitude}" +
                   $"&daily=temperature_2m_mean,weather_code" +
                   $"&timezone=auto&" +
                   $"start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}";
        try
        {
            var response = await _httpClient.GetAsync(path, ct);

            if (!response.IsSuccessStatusCode)
                return Result.Fail(new Error(
                        $"Cannot get the required path: {path}. Request has ended with status code: {response.StatusCode}")
                    .WithMetadata("Code", response.StatusCode == HttpStatusCode.TooManyRequests
                        ? OpenMeteoErrorCodes.TooManyRequests
                        : OpenMeteoErrorCodes.Http5xx));

            await using var s = await response.Content.ReadAsStreamAsync(ct);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            return await JsonSerializer.DeserializeAsync<OpenMeteoDailyMeanResponseDto>(s, options, ct)
                   ?? throw new InvalidOperationException("Deserialization failed");
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