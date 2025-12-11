using Microsoft.Extensions.Options;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.ProviderBase;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Tests.Infrastructure.Helpers;

public static class TestHelpers
{
    #region Helpers

    public static IOptions<ProviderOptions> CreateProviderOptions()
    {
        var options = new ProviderOptions
        {
            OpenMeteo = new ProviderSettings
            {
                Name = "Open-Meteo",
                ForecastBaseUrl = "https://api.open-meteo.com/",
                GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/",
                TimeoutSeconds = 5,
                CacheTtlSeconds = 120
            }
        };

        return Options.Create(options);
    }

    public static GeocodingProviderErrorCodes CreateGeocodingProviderErrorCodes(IOptions<ProviderOptions>  options)
    {
        return new GeocodingProviderErrorCodes(new ProviderInfo(
            nameof(ProviderOptions.OpenMeteo),
            options.Value.OpenMeteo.Name,
            "OpenMeteoGeocodingTest",
            options.Value.OpenMeteo.GeocodingBaseUrl
        ));
    }
    
    public static ForecastProviderErrorCodes CreateForecastProviderErrorCodes(IOptions<ProviderOptions>  options)
    {
        return new ForecastProviderErrorCodes(new ProviderInfo(
            nameof(ProviderOptions.OpenMeteo),
            options.Value.OpenMeteo.Name,
            "OpenMeteoForecastTest",
            options.Value.OpenMeteo.GeocodingBaseUrl
        ));
    }
    #endregion
}