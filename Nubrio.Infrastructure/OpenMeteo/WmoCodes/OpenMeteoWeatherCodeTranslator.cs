using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;

namespace Nubrio.Infrastructure.OpenMeteo.WmoCodes;

public class OpenMeteoWeatherCodeTranslator : IWeatherCodeTranslator
{
    private readonly Dictionary<int, WeatherConditions> _weatherConditions = new();
    private readonly ILogger<OpenMeteoWeatherCodeTranslator> _logger;

    public OpenMeteoWeatherCodeTranslator(IOptions<WeatherCodeMappings> mappings,
        ILogger<OpenMeteoWeatherCodeTranslator> logger)
    {
        _logger = logger;
        var config = mappings.Value;
        
        var properties = typeof(WeatherCodeMappings).GetProperties();

        foreach (var prop in properties)
        {
            if (System.Enum.TryParse<WeatherConditions>(prop.Name, true, out var conditionEnum))
            {
                var codes = prop.GetValue(config) as int[];

                if (codes != null)
                {
                    foreach (var code in codes)
                    {
                        _weatherConditions[code] = conditionEnum;
                    }
                }
            }
        }
        
        _logger.LogInformation("Loaded {Count} WMO weather codes into translator.", _weatherConditions.Count);
    }
    
    public WeatherConditions Translate(int wmoCode)
    {
        return _weatherConditions.GetValueOrDefault(wmoCode, WeatherConditions.Unknown);
    }
}