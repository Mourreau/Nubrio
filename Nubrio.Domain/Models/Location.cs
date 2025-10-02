namespace Nubrio.Domain.Models;

public class Location
{
    public Guid LocationId { get; set; }
    public string Name { get; set; }
    public Coordinates Coordinates { get; set; }
    public string TimeZone { get; set; }
}