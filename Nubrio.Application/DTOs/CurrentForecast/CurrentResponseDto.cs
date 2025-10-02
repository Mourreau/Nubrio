using Nubrio.Domain.Models;

namespace Nubrio.Application.DTOs.CurrentForecast;

public class CurrentResponseDto
{
    public string Time { get; set; }
    public int Interval {get; set;}
    public double Temperature2m { get; set; }
    public int WeatherCode { get; set; }
    
    public Coordinates Coordinates { get; set; }
    public string TimeZone { get; set; }
}
