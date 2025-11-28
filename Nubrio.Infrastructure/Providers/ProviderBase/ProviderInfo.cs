namespace Nubrio.Infrastructure.Providers.ProviderBase;

public sealed record ProviderInfo
{
    public string Name { get; }
    public string Service { get; }
    public string BaseUrl { get; }

    public ProviderInfo(
        string name,
        string service,
        string baseUrl)
    {
        Name = name;
        Service = NormalizeServiceName(service);
        BaseUrl = baseUrl;
    }

    private static string NormalizeServiceName(string serviceNameRaw)
    {
        const string prefix = "OpenMeteo";
        const string suffix = "Client";

        var result = serviceNameRaw;
        
        if (result.StartsWith((prefix), StringComparison.Ordinal))
            result = result.Substring(prefix.Length);
        
        if (result.EndsWith((suffix), StringComparison.Ordinal))
            result = result.Substring(0, result.Length - suffix.Length);
        

        return result;
    }
}