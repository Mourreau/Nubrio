using FluentResults;

namespace Nubrio.Application.Interfaces;

public interface ITimeZoneResolver
{
    Result<TimeZoneInfo> GetTimeZoneInfoById(string timeZoneId);
    // TODO: добавить определение часового пояса по координатам
}