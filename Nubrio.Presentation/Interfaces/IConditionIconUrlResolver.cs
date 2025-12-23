using Nubrio.Domain.Enums;

namespace Nubrio.Presentation.Interfaces;

public interface IConditionIconUrlResolver
{
    string Resolve(WeatherConditions condition);
}