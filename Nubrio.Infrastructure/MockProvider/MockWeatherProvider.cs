using System.Reflection;
using System.Text.Json;
using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.OpenMeteo.DTOs.CurrentForecast;

namespace Nubrio.Infrastructure.MockProvider;

public class MockWeatherProvider : IWeatherProvider
{
    public Task<Result<bool>> TryGetDailyForecastByDate(Coordinates coordinates, DateOnly startDate, DateOnly endDate,
        out DailyResponseDto dailyForecast)
    private readonly IWeatherCodeTranslator _weatherCodeTranslator;

    public MockWeatherProvider(IWeatherCodeTranslator weatherCodeTranslator)
    {
        _weatherCodeTranslator = weatherCodeTranslator;
    }

    {
        dailyForecast = new DailyResponseDto();
        return Task.FromResult(Result.Fail<bool>("Weather not found"));
    }

    public Task<Result<bool>> TryGetCurrentForecast(string city, out CurrentResponseDto currentForecast)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        string resourceName = "Nubrio.Infrastructure.MockProvider.mock_weather_api_current_response.json";

        using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource: {resourceName} not found");

            using (var reader = new StreamReader(stream))
            {
                var jsonContent = reader.ReadToEnd();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                var responseDto = JsonSerializer.Deserialize<OpenMeteoCurrentResponseDto>(jsonContent, options);

                if (responseDto == null)
                {
                    currentForecast = new CurrentResponseDto();
                    return Task.FromResult(Result.Fail<bool>("Weather not found"));
                }

                currentForecast = new CurrentResponseDto
                {
                    Time = responseDto.Current.Time,
                    Interval = responseDto.Current.Interval,
                    Temperature2m = responseDto.Current.Temperature2m,
                    WeatherCode = responseDto.Current.WeatherCode,
                    Coordinates = new Coordinates(responseDto.Latitude, responseDto.Longitude),
                    TimeZone = responseDto.Timezone
                };

                return Task.FromResult(Result.Ok(true));
            }
        }
    }
    
    private DateTimeOffset GetDateTimeOffsetFromString(string dateString, string timeZoneId)
    {
        DateTime dateTime = DateTime.Parse(dateString);

        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        TimeSpan offset = timeZoneInfo.GetUtcOffset(dateTime);

        return new DateTimeOffset(dateTime,  offset);

    }
}