namespace Nubrio.Domain.Models;

public sealed record ExternalLocationId
{
    public string ProviderName { get; init; }
    public int Value { get; init; }
    
    
    public ExternalLocationId(string providerName, int value)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Provider key cannot be empty.", nameof(providerName));
        
        ProviderName = providerName;
        Value = value;
    }

}