namespace Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

public class GeocodingProviderErrorCodes(ProviderInfo providerInfo) : ProviderErrorCodes(providerInfo)
{
    public override string NotFound()
        => $"{Info.Service}.LocationNotFound";
}