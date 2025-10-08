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

    private const string _resourceName =
        "Nubrio.Infrastructure.MockProvider.mock_weather_api_current_response.json";

    public MockWeatherProvider(IWeatherCodeTranslator weatherCodeTranslator)
    {
        _weatherCodeTranslator = weatherCodeTranslator;
    }

    public Task<Result<DailyForecast>> GetDailyForecastRangeAsync(
        Location location, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Fail<DailyForecast>("Weather not found"));
    }

    public async Task<Result<CurrentForecast>> GetCurrentForecastAsync(Location location, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        
        await using (Stream? stream = assembly.GetManifestResourceStream(_resourceName))
        {
            if (stream == null)
                return Result.Fail<CurrentForecast>($"Embedded resource '{_resourceName}' not found");
            
            using (var reader = new StreamReader(stream))
            {
                var jsonContent = await reader.ReadToEndAsync(cancellationToken);
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;

                var responseDto = JsonSerializer.Deserialize<OpenMeteoCurrentResponseDto>(jsonContent, options);

                if (responseDto == null)
                {
                    return Result.Fail<CurrentForecast>("Weather not found");
                }

                var offsetFromString = DataTranslateHelper.GetUtcDateTimeOffsetFromString(
                    responseDto.Current.Time, 
                    responseDto.Timezone);

                if (offsetFromString.IsFailed)
                    return Result.Fail<CurrentForecast>(offsetFromString.Errors);
                
                var result = new CurrentForecast
                (
                    offsetFromString.Value,
                    location.LocationId,
                    responseDto.Current.Temperature2m,
                    _weatherCodeTranslator.Translate(responseDto.Current.WeatherCode)
                );

                return Result.Ok(result);
            }
        }
    }
    
}