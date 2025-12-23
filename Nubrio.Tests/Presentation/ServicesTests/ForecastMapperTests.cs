using FluentAssertions;
using Moq;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Enums;
using Nubrio.Presentation.Interfaces;
using Nubrio.Presentation.Mappers;

namespace Nubrio.Tests.Presentation.ServicesTests;

public class ForecastMapperTests
{
    private readonly Mock<IConditionStringMapper> _conditionStringMapperMock;
    private readonly Mock<IConditionIconUrlResolver> _urlResolverMock;

    private readonly IEnumerable<WeatherConditions> Conditions = 
        Enum.GetValues<WeatherConditions>()
        .Where(x => x != WeatherConditions.Unknown);
    
    public ForecastMapperTests()
    {
        _conditionStringMapperMock = new Mock<IConditionStringMapper>();
        _urlResolverMock = new  Mock<IConditionIconUrlResolver>();
    }

    [Fact]
    public void ToDailyResponse_ShouldReturn_ValidResponse()
    {
        // Arrange
        _conditionStringMapperMock.Setup(x=> x.From(WeatherConditions.Clear)).Returns("clear");
        _urlResolverMock.Setup(x => x.Resolve(WeatherConditions.Clear)).Returns("/icons/airy/clear@4x.png");
        
        var mapper = new ForecastMapper(_conditionStringMapperMock.Object, _urlResolverMock.Object);

        var dto = new DailyForecastMeanDto
        {
            City = "Berlin",
            Condition = WeatherConditions.Clear,
            Date = new DateOnly(2025, 12, 22),
            FetchedAt = DateTimeOffset.Parse("2025-12-22T10:00:00Z"),
            TemperatureMean = 10.5
        };

        // Act
        var response = mapper.ToDailyResponse(dto);

        // Assert
        response.Condition.Should().Be("clear");
        response.IconUrl.Should().Be("/icons/airy/clear@4x.png");
        
        _conditionStringMapperMock.Verify(x => x.From(WeatherConditions.Clear), Times.Once);
        _urlResolverMock.Verify(x => x.Resolve(WeatherConditions.Clear), Times.Once);
    }
    
    
    [Fact]
    public void ToWeeklyResponse_ShouldReturn_ValidResponse()
    {
        // Arrange
        _conditionStringMapperMock.Setup(x=> x.From(It.IsAny<WeatherConditions>()))
            .Returns<WeatherConditions>(c => c.ToString());
        
        _urlResolverMock.Setup(x => x.Resolve(It.IsAny<WeatherConditions>()))
            .Returns<WeatherConditions>(c => $"/icons/{c}.png");
        
        var conditionList = Conditions.ToList();
        
        var mapper = new ForecastMapper(_conditionStringMapperMock.Object, _urlResolverMock.Object);

        var dto = new WeeklyForecastMeanDto
        {
            City = "Berlin",
            FetchedAt = DateTimeOffset.Parse("2025-12-22T10:00:00Z"),
            Days = GetDaysList()
        };

        // Act
        var response = mapper.ToWeeklyResponse(dto);

        // Assert
        response.Days.Should().HaveCount(dto.Days.Count);

        for (int i = 0; i < dto.Days.Count; i++)
        {
            response.Days[i].Condition.Should().Be(conditionList[i].ToString());
        }
        
        _conditionStringMapperMock.Verify(x => x.From(It.IsAny<WeatherConditions>()), Times.Exactly(dto.Days.Count));
        _urlResolverMock.Verify(x => x.Resolve(It.IsAny<WeatherConditions>()), Times.Exactly(dto.Days.Count));
    }

    private IReadOnlyList<DaysDto> GetDaysList()
    {
        List<DaysDto> days = [];
        

        foreach (var condition in Conditions)
        {
            days.Add(new DaysDto(
                Condition: condition,
                Date: new DateOnly(2025, 12, 22),
                TemperatureMean: 10.5));
        }

        return days;
    }
    
}