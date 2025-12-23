using System.ComponentModel.DataAnnotations;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Presentation.DTOs.Forecast.Response;
using Nubrio.Presentation.DTOs.Forecast.Response.WeeklyResponse;
using Nubrio.Presentation.Interfaces;

namespace Nubrio.Presentation.Mappers;

public class ForecastMapper : IForecastMapper
{
    private readonly IConditionStringMapper _conditionStringMapper;
    private readonly IConditionIconUrlResolver _iconUrlResolver;

    public ForecastMapper(IConditionStringMapper conditionStringMapper, IConditionIconUrlResolver iconUrlResolver)
    {
        _conditionStringMapper = conditionStringMapper;
        _iconUrlResolver = iconUrlResolver;
    }

    public CurrentWeatherResponseDto ToCurrentResponseDto(CurrentForecastDto currentForecast)
    {
        return new CurrentWeatherResponseDto
        (
            currentForecast.City,
            DateOnly.FromDateTime(currentForecast.Date.DateTime),
            _conditionStringMapper.From(currentForecast.Condition),
            currentForecast.Temperature,
            "OpenMeteo",
            currentForecast.FetchedAt
        );
    }

    public DailyForecastResponseDto ToDailyResponse(DailyForecastMeanDto forecastMeanDto)
    {
        return new DailyForecastResponseDto
        {
            City = forecastMeanDto.City,
            Condition =  _conditionStringMapper.From(forecastMeanDto.Condition),
            Date = forecastMeanDto.Date,
            TemperatureC = forecastMeanDto.TemperatureMean,
            FetchedAt = forecastMeanDto.FetchedAt,
            IconUrl = _iconUrlResolver.Resolve(forecastMeanDto.Condition), 
            Source = "Open-Meteo", // TODO: Информацию о провайдере контроллер должен получать извне
        };
    }


    public WeeklyForecastResponseDto ToWeeklyResponse(WeeklyForecastMeanDto forecastMeanDto)
    {
        return new WeeklyForecastResponseDto
        {
            City = forecastMeanDto.City,
            Days = ToDailyResponse(forecastMeanDto),
            Source = "Open-Meteo",
            FetchedAt = forecastMeanDto.FetchedAt
        };
    }


    private IReadOnlyList<WeeklyForecastDayResponseDto> ToDailyResponse(WeeklyForecastMeanDto forecastMeanDto)
    {
        var days = new List<WeeklyForecastDayResponseDto>();

        foreach (var day in forecastMeanDto.Days)
        {
            days.Add(new WeeklyForecastDayResponseDto
            {
                Date = day.Date,
                Condition =  _conditionStringMapper.From(day.Condition),
                TemperatureC = day.TemperatureMean,
                IconUrl = _iconUrlResolver.Resolve(day.Condition)
            });
        }

        return days;
    }
}