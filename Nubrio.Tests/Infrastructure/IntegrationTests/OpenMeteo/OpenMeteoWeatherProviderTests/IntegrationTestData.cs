using System.Net;
using System.Text;
using Nubrio.Domain.Models;

namespace Nubrio.Tests.Infrastructure.IntegrationTests.OpenMeteo.OpenMeteoWeatherProviderTests;

public class IntegrationTestData
{
    public const string ValidDailyMeanForecastJson =
        """
        {
        "latitude": 56.75,
        "longitude": 60.8125,
        "generationtime_ms": 1.4,
        "utc_offset_seconds": 18000,
        "timezone": "Europe/Moscow",
        "timezone_abbreviation": "GMT+3",
        "elevation": 228,
        "daily_units": {
          "time": "iso8601",
          "weather_code": "wmo code",
          "temperature_2m_mean": "°C"
        },
        "daily": {
          "time": ["2025-10-20"],
          "weather_code": [3],
          "temperature_2m_mean": [1.5]
        }
        }
        """;
    



}
