using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Presentation.DTOs.Response;
using Nubrio.Presentation.DTOs.Response.WeeklyResponse;

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

    public static DailyForecastResponseDto ToDailyResponse(DailyForecastMeanDto forecastMeanDto)
    {
        return new DailyForecastResponseDto
        {
            City = forecastMeanDto.City,
            Condition = forecastMeanDto.Condition,
            Date = forecastMeanDto.Date,
            TemperatureC = forecastMeanDto.TemperatureMean,
            FetchedAt = forecastMeanDto.FetchedAt,
            IconUrl = "Blank-Text", // TODO: Добавить иконки
            Source = "Open-Meteo", // TODO: Информацию о провайдере контроллер должен получать извне
        };
    }


    public static WeeklyForecastResponseDto ToWeeklyResponse(WeeklyForecastMeanDto forecastMeanDto)
    {
        return new WeeklyForecastResponseDto
        {
            City = forecastMeanDto.City,
            Days = ForecastMapper.ToDailyResponse(forecastMeanDto),
            Source = "Open-Meteo",
            FetchedAt = forecastMeanDto.FetchedAt
        };
    }


    private static IReadOnlyList<WeeklyForecastDayResponseDto> ToDailyResponse(WeeklyForecastMeanDto forecastMeanDto)
    {
        var days = new List<WeeklyForecastDayResponseDto>();

        foreach (var day in forecastMeanDto.Days)
        {
            days.Add(new WeeklyForecastDayResponseDto
            {
                Date = day.Date,
                Condition = day.Condition,
                TemperatureC = day.TemperatureMean,
                IconUrl = "Not found" // TODO: Добавить иконки
            });
        }

        return days;
    }
}