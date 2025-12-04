using System.Globalization;
using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Nubrio.Domain.Models.Weekly;
using Nubrio.Infrastructure.Clients.ForecastClient;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.WeeklyForecast;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoForecast;

public class OpenMeteoForecastProvider : IForecastProvider
{
    private readonly IWeatherCodeTranslator _weatherCodeTranslator;
    private readonly IForecastClient _client;


    public OpenMeteoForecastProvider(IForecastClient client, IWeatherCodeTranslator weatherCodeTranslator)
    {
        _weatherCodeTranslator = weatherCodeTranslator;
        _client = client;
    }

    public async Task<Result<DailyForecastMean>> GetDailyForecastMeanAsync(Location location, DateOnly date,
        CancellationToken cancellationToken)
    {
        var clientResponse = await _client.GetOpenMeteoDailyMeanAsync(
            location.Coordinates.Latitude,
            location.Coordinates.Longitude,
            date,
            cancellationToken);


        if (clientResponse.IsFailed)
            return Result.Fail(clientResponse.Errors);

        var openMeteoResponseDto = clientResponse.Value;


        var result = MapToDomainModelDailyForecastMean(openMeteoResponseDto, location);

        return Result.Ok(result);
    }

    public async Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location,
        CancellationToken cancellationToken)
    {
        return Result.Fail($"{nameof(GetCurrentForecastAsync)} is not implemented");
    }

    public async Task<Result<WeeklyForecastMean>> GetWeeklyForecastMeanAsync(Location location,
        CancellationToken cancellationToken)
    {
        var clientResponse = await _client.GetOpenMeteoWeeklyMeanAsync(
            location.Coordinates.Latitude,
            location.Coordinates.Longitude,
            cancellationToken);

        if (clientResponse.IsFailed)
            return Result.Fail(clientResponse.Errors);

        var openMeteoResponseDto = clientResponse.Value;

        var result = MapToDomainModelWeeklyForecastMean(openMeteoResponseDto, location);

        return Result.Ok(result);
    }


    // -------------------------------------------------------------------------------------------------------------- //

    #region Mappers

    private DailyForecastMean MapToDomainModelDailyForecastMean(
        OpenMeteoDailyMeanResponseDto openMeteoResponseDto, Location location)
    {
        // Здесь конкретно указан [0] элемент листа т.к. в данном контексте элемент будет только один.
        const int index = 0;

        var dateTranslate = DateOnly.Parse(
            openMeteoResponseDto.Daily.Time[index],
            CultureInfo.InvariantCulture);


        var dailyForecastResult = new DailyForecastMean
        (
            dateTranslate,
            location.LocationId,
            _weatherCodeTranslator.Translate(openMeteoResponseDto.Daily.WeatherCode[index]),
            openMeteoResponseDto.Daily.Temperature2mMean[index]
            // Берем [0] элемент листа. Прогноз на одну дату и значение в листе тоже будет одно.
        );

        return dailyForecastResult;
    }


    private WeeklyForecastMean MapToDomainModelWeeklyForecastMean(
        OpenMeteoWeeklyMeanResponseDto openMeteoResponseDto, Location location)
    {
        var count = openMeteoResponseDto.Daily.Temperature2mMean.Count;
        var daily = new DailyForecastMean[count + 1];

        for (int i = 0; i < count; i++)
        {
            var dateTranslate = DateOnly.Parse(
                openMeteoResponseDto.Daily.Time[i],
                CultureInfo.InvariantCulture);


            daily[i] = new DailyForecastMean
            (
                dateTranslate,
                location.LocationId,
                _weatherCodeTranslator.Translate(openMeteoResponseDto.Daily.WeatherCode[i]),
                openMeteoResponseDto.Daily.Temperature2mMean[i]
            );
        }

        return new WeeklyForecastMean(daily);
    }

    #endregion
}