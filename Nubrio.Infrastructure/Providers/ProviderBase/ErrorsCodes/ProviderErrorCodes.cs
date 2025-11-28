namespace Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

public abstract class ProviderErrorCodes
{
    protected ProviderInfo Info { get; }

    protected ProviderErrorCodes(ProviderInfo providerInfo)
    {
       Info = providerInfo;
    }

    public abstract string NotFound();
    public string DeserializationNull()
        => $"{Info.Service}.JsonDeserializationReturnedNull";
    public string DeserializationException()
        => $"{Info.Service}.JsonDeserializationFailedWithException";
    
    public string TooManyRequests()
        => $"{Info.Service}.TooManyRequests";
    
    public string InternalError()
        => $"{Info.Service}.InternalError";
    
    public string NetworkError()
    => $"{Info.Service}.NetworkError";
    
    public string Timeout()
    => $"{Info.Service}.Timeout";

}