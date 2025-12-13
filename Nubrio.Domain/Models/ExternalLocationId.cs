namespace Nubrio.Domain.Models;

public sealed record ExternalLocationId
{
    public string GeocodingProviderName { get; }
    public string Value { get; }


    public ExternalLocationId(string geocodingProviderName, int value)
    {
        if (string.IsNullOrWhiteSpace(geocodingProviderName))
            throw new ArgumentException("Provider key cannot be empty.", nameof(geocodingProviderName));

        GeocodingProviderName = geocodingProviderName;
        Value = value.ToString();
    }
}