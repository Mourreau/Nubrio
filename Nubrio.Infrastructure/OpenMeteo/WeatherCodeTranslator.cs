using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;

namespace Nubrio.Infrastructure.OpenMeteo;

public class WeatherCodeTranslator : IWeatherCodeTranslator
{
    private readonly Dictionary<int, WeatherConditions> _weatherConditions = new();

    public WeatherCodeTranslator(IOptions<WeatherCodeMappings> mappings)
    {
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
    }
    
    public WeatherConditions Translate(int wmoCode)
    {
        return _weatherConditions.TryGetValue(wmoCode, out var condition) 
            ? condition 
            : WeatherConditions.Unknown;
    }
}