namespace Nubrio.Presentation.DTOs.Response;

public class CurrentWeatherResponseDto
{
    public string City { get; set; }
    public DateOnly Date { get; set; }
    public string Condition { get; set; }
    public double Temperature { get; set; }
    public string IconUrl { get; set; } 
    public string Source { get; set; }
    public DateTimeOffset FetchedAt{ get; set; }
}

