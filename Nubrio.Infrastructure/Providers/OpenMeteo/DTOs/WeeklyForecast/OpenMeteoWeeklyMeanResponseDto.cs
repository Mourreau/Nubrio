using System.Text.Json.Serialization;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.WeeklyForecast;

/// <summary>
/// Совпадает с <c>OpenMeteoDailyResponseDto</c>
/// </summary>
public record OpenMeteoWeeklyMeanResponseDto : OpenMeteoMeanForecastBase
{
}
