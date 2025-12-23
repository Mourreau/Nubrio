using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Presentation.DTOs.Forecast.Response;
using Nubrio.Presentation.DTOs.Forecast.Response.WeeklyResponse;

namespace Nubrio.Presentation.Mappers;

public interface IForecastMapper
{
    CurrentWeatherResponseDto ToCurrentResponseDto(CurrentForecastDto currentForecast);
    DailyForecastResponseDto ToDailyResponse(DailyForecastMeanDto forecastMeanDto);
    WeeklyForecastResponseDto ToWeeklyResponse(WeeklyForecastMeanDto forecastMeanDto);
}