using FluentResults;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;

namespace Nubrio.Infrastructure.Http;

public interface IForecastClient
{
    public Task<Result<OpenMeteoDailyResponseDto>> GetOpenMeteoDailyAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken ct);

    public Task<Result<OpenMeteoDailyMeanResponseDto>> GetOpenMeteoDailyMeanAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken ct);
}