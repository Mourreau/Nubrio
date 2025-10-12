namespace Nubrio.Presentation.DTOs.Response;

public record CurrentWeatherResponseDto
{
    public CurrentWeatherResponseDto(
        string city,
        DateOnly date,
        string condition,
        double temperature,
        string source,
        DateTimeOffset fetchedAt,
        string iconUrl = "Icon has not found")
    {
        City = city;
        Date = date;
        Condition = condition;
        Temperature = temperature;
        IconUrl = iconUrl;
        Source = source;
        FetchedAt = fetchedAt;
    }
    public string City { get;}
    public DateOnly Date { get; }
    public string Condition { get; }
    public double Temperature { get; }
    public string IconUrl { get; }
    public string Source { get; }
    public DateTimeOffset FetchedAt{ get; }
}

