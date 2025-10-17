namespace Nubrio.Presentation.DTOs.Response;

public record CurrentWeatherResponseDto(
    string City,
    DateOnly Date,
    string Condition,
    double Temperature,
    string Source,
    DateTimeOffset FetchedAt,
    string IconUrl = "Icon has not found");

