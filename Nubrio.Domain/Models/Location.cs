namespace Nubrio.Domain.Models;

public class Location
{
    public Location(Guid locationId, string name, Coordinates coordinates, string timeZoneIana)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(timeZoneIana))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.",
                nameof(name));
        }
        
        LocationId = locationId;
        Name = name;
        Coordinates = coordinates ??
                          throw new ArgumentException($"'{nameof(coordinates)}' cannot be null or whitespace.", 
                          nameof(coordinates));
        TimeZoneIana = timeZoneIana;
    }
    public Guid LocationId { get; }
    public string Name { get; }
    public Coordinates Coordinates { get; } 
    public string TimeZoneIana { get; }
}