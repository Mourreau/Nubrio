using System.Globalization;
using FluentResults;
using Microsoft.Extensions.Options;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.Providers.ProviderBase;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Clients.ForecastClient;

internal sealed class OpenMeteoForecastClient : ExternalApiClientBase<ForecastProviderErrorCodes>, IForecastClient
{
    public OpenMeteoForecastClient(HttpClient httpClient, IOptions<ProviderOptions> options)
        : this(httpClient, CreateProviderInfo(options))
    {
    }

    private OpenMeteoForecastClient(HttpClient httpClient, ProviderInfo info)
        : base(httpClient, info, new ForecastProviderErrorCodes(info))
    {
    }

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
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(HttpClient.BaseAddress!, path));

        var result = await SendAndDeserializeAsync<OpenMeteoDailyMeanResponseDto>(request, ct);
        if (result.IsFailed) return Result.Fail(result.Errors);

        var dto = result.Value;

        if (dto.Daily is null)
        {
            return BuildError(
                $"No forecast found",
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
            service: nameof(OpenMeteoForecastClient),
            baseUrl: cfg.ForecastBaseUrl
        );
    }
}