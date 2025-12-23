namespace Nubrio.Domain.Models;

public sealed class Location
{
    private Location()
    {
    }

    public Location(
        Guid id,
        string name,
        Coordinates coordinates,
        string timeZoneIana,
        ExternalLocationId externalLocationId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(
                nameof(name), $"'{nameof(name)}' cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(timeZoneIana))
        {
            throw new ArgumentNullException(
                nameof(timeZoneIana), $"'{nameof(timeZoneIana)}' cannot be null or whitespace.");
        }
        
        Coordinates = coordinates ??
                      throw new ArgumentNullException(
                          nameof(coordinates), $"'{nameof(coordinates)}' cannot be null or whitespace.");

        ExternalLocationId = externalLocationId ??
                             throw new ArgumentNullException(
                                 nameof(externalLocationId), $"'{nameof(externalLocationId)}' cannot be null or whitespace.");
        
        Id = id;
        Name = name;
        TimeZoneIana = timeZoneIana;
    }

    public Guid Id { get; private set; }
    public ExternalLocationId ExternalLocationId { get; private set; }
    public string Name { get; private set; }
    public Coordinates Coordinates { get; private set; }
    public string TimeZoneIana { get; private set; }
}