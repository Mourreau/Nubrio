using Nubrio.Domain.Enums;

namespace Nubrio.Application.Interfaces;

public interface IWeatherCodeTranslator
{
    WeatherConditions Translate(int wmoCode);
}