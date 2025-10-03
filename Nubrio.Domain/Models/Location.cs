namespace Nubrio.Domain.Models;

public class Location(Guid locationId, string name, Coordinates coordinates, string timeZoneIana)
{
    public Guid LocationId { get; } = locationId;
    public string Name { get; } = name;
    public Coordinates Coordinates { get; } = coordinates;
    public string TimeZoneIana { get; } = timeZoneIana;
}