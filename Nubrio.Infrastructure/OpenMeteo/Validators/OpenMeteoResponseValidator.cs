using System.Globalization;
using Nubrio.Infrastructure.OpenMeteo.DTOs.DailyForecast.MeanForecast;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;

namespace Nubrio.Infrastructure.OpenMeteo.Validators;

internal static class OpenMeteoResponseValidator
{

    public static ValidationResult ValidateMean(OpenMeteoDailyMeanResponseDto dtoMean)
    {
        if (dtoMean is null) return Fail("Response is null", OpenMeteoErrorCodes.MalformedDailyMean);
        if (dtoMean.Daily is null) return Fail("Missing 'daily' section", OpenMeteoErrorCodes.MalformedDailyMean);

        var d = dtoMean.Daily;
        if (d.Time is null || d.WeatherCode is null || d.Temperature2mMean is null)
            return Fail("Missing daily arrays: time/weather_code/temperature_2m_mean", OpenMeteoErrorCodes.MalformedDailyMean);


        var elemCount = d.Time.Count;
        if (elemCount == 0) return Fail("Daily arrays are empty", OpenMeteoErrorCodes.MalformedDailyMean);
        if (d.WeatherCode.Count != elemCount || d.Temperature2mMean.Count != elemCount)
            return Fail($"Daily arrays have to be equal", OpenMeteoErrorCodes.MalformedDailyMean);

        // Проверка, что это действительно прогноз на 1 день
        if (elemCount > 1) return Fail($"Daily arrays have more than one element. Count: {elemCount}", OpenMeteoErrorCodes.MalformedDailyMean);

        // Единицы измерения
        var units = dtoMean.DailyUnits;
        if (units is not null)
        {
            if (!string.Equals(units.Temperature2mMean, "°C", StringComparison.OrdinalIgnoreCase))
                return Fail("Unexpected unit for temperature_2m_mean", OpenMeteoErrorCodes.UnitsMismatch);
        }

        // Температура
        for (int i = 0; i < elemCount; i++)
        {
            var mean = d.Temperature2mMean[i];
            if (mean < -90 || mean > 60) return Fail($"Mean temperature out of range at {i}", OpenMeteoErrorCodes.MalformedDailyMean);
        }

        // Корректный формат дат
        for (int i = 0; i < elemCount; i++)
        {
            if (!DateOnly.TryParse(d.Time[i], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return Fail($"Invalid date format at {i} (expected yyyy-MM-dd)", OpenMeteoErrorCodes.MalformedDailyMean);
        }

        return Ok(elemCount);
    }

    private static ValidationResult Ok(int weatherElements)
    {
        return new ValidationResult(
            "Validation succeeded",
            true,
            false,
            weatherElements);
    }

    private static ValidationResult Fail(string message, string errorCode)
    {
        return new ValidationResult(
            message,
            false,
            true,
            code: errorCode);
    }
}