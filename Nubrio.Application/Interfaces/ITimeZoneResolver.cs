using FluentResults;

namespace Nubrio.Application.Interfaces;

public interface ITimeZoneResolver
{
    Result<TimeZoneInfo> GetTimeZoneInfo(string timeZoneId);
    
}