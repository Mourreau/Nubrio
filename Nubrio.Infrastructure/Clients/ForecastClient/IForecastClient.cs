using FluentResults;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.WeeklyForecast;

namespace Nubrio.Infrastructure.Clients.ForecastClient;

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

    Task<Result<OpenMeteoWeeklyMeanResponseDto>> GetOpenMeteoWeeklyMeanAsync(
        double latitude,
        double longitude,
        CancellationToken ct);
}