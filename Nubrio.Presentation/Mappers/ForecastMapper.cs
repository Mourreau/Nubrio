using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Presentation.DTOs.Response;

namespace Nubrio.Presentation.Mappers;

public static class ForecastMapper
{
    public static CurrentWeatherResponseDto ToCurrentResponseDto(CurrentForecastDto currentForecast)
    {
        return new CurrentWeatherResponseDto
        (
            currentForecast.City,
            DateOnly.FromDateTime(currentForecast.Date.DateTime),
            currentForecast.Condition,
            currentForecast.Temperature,
            "OpenMeteo",
            currentForecast.FetchedAt
        );
    }
}