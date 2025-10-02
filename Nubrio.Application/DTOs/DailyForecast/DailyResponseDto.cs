namespace Nubrio.Application.DTOs.DailyForecast;

public class DailyResponseDto
{
    public List<string> Time { get; set; }
    public List<int> WeatherCode { get; set; }
    public List<double> Temperature2mMax { get; set; }
    public List<double> Temperature2mMin { get; set; }
}