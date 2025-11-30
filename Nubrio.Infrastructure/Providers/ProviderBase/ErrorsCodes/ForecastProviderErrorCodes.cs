namespace Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

public class ForecastProviderErrorCodes(ProviderInfo providerInfo) : ProviderErrorCodes(providerInfo)
{
    public override string NotFound()
        => $"{Info.Service}.ForecastNotFound";
}