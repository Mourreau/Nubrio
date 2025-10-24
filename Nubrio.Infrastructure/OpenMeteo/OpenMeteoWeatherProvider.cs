using System.Globalization;
using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Domain.Models.Daily;
using Nubrio.Infrastructure.Http;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.OpenMeteo.Validators;

namespace Nubrio.Infrastructure.OpenMeteo;

public class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly IWeatherCodeTranslator _weatherCodeTranslator;
    private readonly IOpenMeteoClient _client;


    public OpenMeteoWeatherProvider(IOpenMeteoClient client, IWeatherCodeTranslator weatherCodeTranslator)
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

        var validationResult = OpenMeteoResponseValidator.ValidateMean(openMeteoResponseDto);
        if (validationResult.IsFailed)
        {
            var error = new Error(validationResult.Message);
            if(!string.IsNullOrEmpty(validationResult.Code))
                error = error.WithMetadata("Code", validationResult.Code);
            
            return Result.Fail(error);
        }
        
        
        if (validationResult.WeatherElements != 1) 
            return Result.Fail("Weather elements out of range.Expected 1 element.");

        var result = MapToDomainModelDailyForecastMean(openMeteoResponseDto,  location);
        
        if (result.IsFailed) return Result.Fail<DailyForecastMean>(result.Errors);
        

        return Result.Ok(result.Value);
    }

    public async Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
    {
        return Result.Fail($"{nameof(GetCurrentForecastAsync)} is not implemented");
    }

    
    
    
    // -------------------------------------------------------------------------------------------------------------- //
    private Result<DailyForecastMean> MapToDomainModelDailyForecastMean(
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

        return Result.Ok(dailyForecastResult);
    }
}