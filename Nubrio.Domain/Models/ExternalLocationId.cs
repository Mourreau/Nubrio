namespace Nubrio.Domain.Models;

public sealed record ExternalLocationId
{
    private ExternalLocationId()
    {
    }


    public string ProviderName { get; private set; }
    public string Value { get; private set; }


    public ExternalLocationId(string providerName, string value)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Provider key cannot be empty.", nameof(providerName));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Identification value cannot be empty.", nameof(value));
        
        
        ProviderName = providerName;
        Value = value.Trim();
    }
}