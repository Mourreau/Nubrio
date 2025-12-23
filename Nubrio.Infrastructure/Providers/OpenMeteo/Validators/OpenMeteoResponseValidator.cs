using System.Globalization;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.DTOs.WeeklyForecast;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.Validators;

internal static class OpenMeteoResponseValidator
{
    public static ValidationResult ValidateMean(
        OpenMeteoMeanForecastBase dtoMean,
        ForecastProviderErrorCodes errorCodes)
    {
        if (dtoMean is null) return Fail("Response is null", errorCodes.MalformedDailyMean());
        if (dtoMean.Daily is null) return Fail("Missing 'daily' section", errorCodes.MalformedDailyMean());

        var d = dtoMean.Daily;
        if (d.Time is null || d.WeatherCode is null || d.Temperature2mMean is null)
            return Fail("Missing daily arrays: time/weather_code/temperature_2m_mean", errorCodes.MalformedDailyMean());


        var elemCount = d.Time.Count;
        if (elemCount == 0) return Fail("Daily arrays are empty", errorCodes.MalformedDailyMean());
        if (d.WeatherCode.Count != elemCount || d.Temperature2mMean.Count != elemCount)
            return Fail("Daily arrays must have equal lengths", errorCodes.MalformedDailyMean());

        // Проверка, что это действительно прогноз на 1 день
        if (dtoMean is OpenMeteoDailyMeanResponseDto && elemCount > 1)
            return Fail($"Daily arrays in daily forecast have more than one element. Count: {elemCount}",
                errorCodes.MalformedDailyMean());

        // Проверка, что это действительно прогноз на неделю
        if (dtoMean is OpenMeteoWeeklyMeanResponseDto && elemCount > 7)
            return Fail($"Daily arrays in weekly forecast have more than seven elements. Count: {elemCount}",
                errorCodes.MalformedDailyMean());

        // Единицы измерения
        var units = dtoMean.DailyUnits;
        if (units is not null)
        {
            if (!string.Equals(units.Temperature2mMean, "°C", StringComparison.OrdinalIgnoreCase))
                return Fail("Unexpected unit for temperature_2m_mean", errorCodes.UnitsMismatch());
        }

        // Температура
        for (int i = 0; i < elemCount; i++)
        {
            var mean = d.Temperature2mMean[i];
            if (mean < -90 || mean > 60)
                return Fail($"Mean temperature out of range at {i}", errorCodes.MalformedDailyMean());
        }

        // Корректный формат дат
        for (int i = 0; i < elemCount; i++)
        {
            if (!DateOnly.TryParse(d.Time[i], CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return Fail($"Invalid date format at {i} (expected yyyy-MM-dd)", errorCodes.MalformedDailyMean());
        }

        return Ok(elemCount);
    }


    #region Results

    private static ValidationResult Ok(int weatherElements)
    {
        return new ValidationResult(
            "Validation succeeded",
            true,
            weatherElements);
    }

    private static ValidationResult Fail(string message, string errorCode)
    {
        return new ValidationResult(
            message,
            false,
            code: errorCode);
    }

    #endregion
}