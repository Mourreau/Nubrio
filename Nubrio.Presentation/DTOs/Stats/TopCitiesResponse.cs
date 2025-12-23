namespace Nubrio.Presentation.DTOs.Stats;

public sealed record TopCitiesResponse(
    DateOnly From,
    DateOnly To,
    int Limit,
    List<TopCitieDto> Cities);

public sealed record TopCitieDto(string City, int Count);