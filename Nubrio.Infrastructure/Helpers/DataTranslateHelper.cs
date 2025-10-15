using System.Globalization;
using FluentResults;

namespace Nubrio.Infrastructure.Helpers;

public static class DataTranslateHelper
{
    public static Result<DateTimeOffset> GetUtcDateTimeOffsetFromString(string dateString, string timeZoneId)
    {
        string dateFormat = "yyyy-MM-dd'T'HH:mm";

        // Переводим строку Time из результата внешнего API в DateTime
        if (!DateTime.TryParseExact(
                dateString,
                dateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dateTimeResult))
        {
            return Result.Fail($"Could not parse '{dateString}' to '{dateFormat}'.");
        }

        // Получаем TimeZoneInfo из строки TimeZone внешнего API
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZoneInfo))
            return Result.Fail($"Could not find time zone '{timeZoneId}'.");

        // Переводим полученное время в UTC
        try
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeResult, timeZoneInfo);
            return Result.Ok(new DateTimeOffset(utcTime, TimeSpan.Zero));
        }
        catch (Exception e)
        {
            return Result.Fail($"Could not convert '{dateString}' to '{timeZoneId}'.");
        }
    }
}