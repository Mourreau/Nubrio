using Nubrio.Domain.Models;

namespace Nubrio.Tests.Domain.Models;

public class LocationTests
{
    [Fact]
    public void CtorLocation_WithCorrectParam_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();
        string name = "city";
        Coordinates coord = new Coordinates(50, 60);
        string tz = "Asia/Yekaterinburg";

        string providerName = "test-provider";
        string externalId = "00001";

        Location result = new Location(id, name, coord, tz, new ExternalLocationId(providerName, externalId));

        Assert.Equal(result.Id, id);
        Assert.Equal(result.Name, name);
        Assert.Equal(result.Coordinates, coord);
        Assert.Equal(result.TimeZoneIana, tz);
    }

    [Fact]
    public void CtorLocation_WithNullName_ShouldThrow()
    {
        Guid id = Guid.NewGuid();
        string nullName = null;
        string spaceName = "";
        Coordinates coord = new Coordinates(50, 60);
        string tz = "Asia/Yekaterinburg";

        string providerName = "test-provider";
        string externalId = "00001";

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Location(id, nullName, coord, tz,
                new ExternalLocationId(providerName, externalId)));

        Assert.Equal("name", ex.ParamName);
        Assert.Contains("cannot be null", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CtorLocation_WithWhiteSpaceName_ShouldThrow()
    {
        Guid id = Guid.NewGuid();
        string spaceName = "";
        Coordinates coord = new Coordinates(50, 60);
        string tz = "Asia/Yekaterinburg";

        string providerName = "test-provider";
        string externalId = "00001";

        var ex = Assert.Throws<ArgumentNullException>(() => new Location(id, spaceName, coord, tz,
            new ExternalLocationId(providerName, externalId)));

        Assert.Equal("name", ex.ParamName);
        Assert.Contains("or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CtorLocation_WithNullTimeZone_ShouldThrow()
    {
        Guid id = Guid.NewGuid();
        string name = "city";
        Coordinates coord = new Coordinates(50, 60);
        string nullTz = null;

        string providerName = "test-provider";
        string externalId = "00001";

        var ex = Assert.Throws<ArgumentNullException>(() => new Location(id, name, coord, nullTz,
            new ExternalLocationId(providerName, externalId)));

        Assert.Equal("timeZoneIana", ex.ParamName);
        Assert.Contains("cannot be null", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CtorLocation_WithWhiteSpaceTimeZone_ShouldThrow()
    {
        Guid id = Guid.NewGuid();
        string name = "city";
        Coordinates coord = new Coordinates(50, 60);
        string spaceTz = " ";


        string providerName = "test-provider";
        string externalId = "00001";


        var ex = Assert.Throws<ArgumentNullException>(() => new Location(id, name, coord, spaceTz,
            new ExternalLocationId(providerName, externalId)));


        Assert.Equal("timeZoneIana", ex.ParamName);
        Assert.Contains("or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CtorLocation_WithNullCoordinates_ShouldThrow()
    {
        Guid id = Guid.NewGuid();
        string name = "city";
        Coordinates nullCoord = null;
        string tz = "Asia/Yekaterinburg";

        string providerName = "test-provider";
        string externalId = "00001";

        var ex = Assert.Throws<ArgumentNullException>(() => new Location(id, name, nullCoord, tz,
            new ExternalLocationId(providerName, externalId)));

        Assert.Equal("coordinates", ex.ParamName);
        Assert.Contains("cannot be null", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}