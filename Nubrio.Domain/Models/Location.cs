namespace Nubrio.Domain.Models;

public class Location
{
    public Location(Guid locationId, string name, Coordinates coordinates, string timeZoneIana)
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
        
        LocationId = locationId;
        Name = name;
        Coordinates = coordinates ??
                          throw new ArgumentNullException(
                              nameof(coordinates), $"'{nameof(coordinates)}' cannot be null or whitespace.");
        TimeZoneIana = timeZoneIana;
    }
    public Guid LocationId { get; }
    public string Name { get; }
    public Coordinates Coordinates { get; } 
    public string TimeZoneIana { get; }
}