namespace Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

public class ForecastProviderErrorCodes(ProviderInfo providerInfo) : ProviderErrorCodes(providerInfo)
{
    public override string NotFound()
        => $"{Info.Service}.ForecastNotFound";

    public string MalformedDailyMean()
        => $"{Info.Service}.MalformedDailyMean";

    public string TemperatureOutOfRange()
        => $"{Info.Service}.TemperatureOutOfRange";

    public string InvalidDateFormat()
        => $"{Info.Service}.InvalidDateFormat";
}