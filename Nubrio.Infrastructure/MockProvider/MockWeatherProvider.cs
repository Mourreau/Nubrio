using System.Reflection;
using System.Text.Json;
using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.Helpers;
using Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

namespace Nubrio.Infrastructure.MockProvider;

public class MockWeatherProvider : IWeatherProvider
{
    private readonly IWeatherCodeTranslator _weatherCodeTranslator;

    public MockWeatherProvider(IWeatherCodeTranslator weatherCodeTranslator)
    {
        _weatherCodeTranslator = weatherCodeTranslator;
    }

    public Task<Result<DailyForecast>> GetDailyForecastRangeAsync(
        Location location, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Fail<DailyForecast>("Weather not found"));
    }

    public Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        string resourceName = "Nubrio.Infrastructure.MockProvider.mock_weather_api_current_response.json";

        using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                return Task.FromResult(Result.Fail<CurrentForecast>($"Embedded resource '{resourceName}' not found"));
            
            using (var reader = new StreamReader(stream))
            {
                var jsonContent = reader.ReadToEnd();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                var responseDto = JsonSerializer.Deserialize<OpenMeteoCurrentResponseDto>(jsonContent, options);

                if (responseDto == null)
                {
                    return Task.FromResult(Result.Fail<CurrentForecast>("Weather not found"));
                }

                var offsetFromString = DataTranslateHelper.GetUtcDateTimeOffsetFromString(
                    responseDto.Current.Time, 
                    responseDto.Timezone);

                if (offsetFromString.IsFailed)
                    return Task.FromResult(Result.Fail<CurrentForecast>(offsetFromString.Errors));
                
                var result = new CurrentForecast
                (
                    offsetFromString.Value,
                    location.LocationId,
                    responseDto.Current.Temperature2m,
                    _weatherCodeTranslator.Translate(responseDto.Current.WeatherCode)
                );

                return Task.FromResult(Result.Ok(result));
            }
        }
    }
    
}