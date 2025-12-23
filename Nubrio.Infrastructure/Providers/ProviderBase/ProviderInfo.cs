namespace Nubrio.Infrastructure.Providers.ProviderBase;

public sealed record ProviderInfo
{
    public string Name { get; }
    public string Service { get; }
    public string BaseUrl { get; }
    public string ProviderKey { get; }

    public ProviderInfo(
        string providerKey,
        string name,
        string service,
        string baseUrl)
    {
        ProviderKey = providerKey;
        Name = name;
        Service = NormalizeClientServiceName(service, providerKey);
        BaseUrl = baseUrl;
    }

    private static string NormalizeClientServiceName(string serviceNameRaw, string providerKey)
    {
        const string suffix = "Client";

        var result = serviceNameRaw;

        if (result.StartsWith((providerKey), StringComparison.Ordinal))
            result = result.Substring(providerKey.Length);

        if (result.EndsWith((suffix), StringComparison.Ordinal))
            result = result.Substring(0, result.Length - suffix.Length);


        return result;
    }
}