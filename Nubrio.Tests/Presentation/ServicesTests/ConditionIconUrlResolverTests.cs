using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Nubrio.Domain.Enums;
using Nubrio.Presentation.Services;

namespace Nubrio.Tests.Presentation.ServicesTests;

public class ConditionIconUrlResolverTests
{
    private readonly ConditionIconUrlResolver _resolver;

    public ConditionIconUrlResolverTests()
    {
        _resolver = new ConditionIconUrlResolver();
    }

    [Fact]
    public void Resolve_ShouldReturnUnknowUri()
    {
        // Arrange
        var unknowIconUrl = "/icons/airy/unknown.png";

        // Act
        var url = _resolver.Resolve(WeatherConditions.Unknown);

        // Assert
        url.Should().Be(unknowIconUrl);
    }


    public static IEnumerable<object[]> AllConditionsExceptUnknown()
    {
        return Enum.GetValues<WeatherConditions>()
            .Where(x => x != WeatherConditions.Unknown)
            .Select(x => new object[] { x });
    }
    
    [Theory]
    [MemberData(nameof(AllConditionsExceptUnknown))]
    public void Resolve_ReturnsIconPath(WeatherConditions condition)
    {
        var url = _resolver.Resolve(condition);

        url.Should().NotBeNullOrWhiteSpace();
        url.Should().StartWith("/icons/airy/");
        url.Should().EndWith(".png");
        url.Should().NotBe("/icons/airy/unknown.png");
    }
}