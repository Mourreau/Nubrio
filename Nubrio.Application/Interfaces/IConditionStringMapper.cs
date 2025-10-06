using Nubrio.Domain.Enums;

namespace Nubrio.Application.Interfaces;

public interface IConditionStringMapper
{
    string From(WeatherConditions condition);
}