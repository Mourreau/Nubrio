using System.Globalization;
using FluentResults;

namespace Nubrio.Infrastructure.Helpers;

public static class DataTranslateHelper
{
    public static Result<DateTimeOffset> GetDateTimeOffsetFromString(string dateString, string timeZoneId)
    {
        string dateFormat = "yyyy-MM-dd'T'HH:mm";

        if (!DateTime.TryParseExact(
                dateString,
                dateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTime dateTimeResult))
        {
            return Result.Fail($"Could not parse '{dateString}' to '{dateFormat}'.");
        }
        
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        TimeSpan offset = timeZoneInfo.GetUtcOffset(dateTimeResult);

        return Result.Ok(new DateTimeOffset(dateTimeResult,  offset));

    }
}