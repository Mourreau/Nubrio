using Nubrio.Domain.Models;

namespace Nubrio.Tests.Domain.Models;


public class CoordinatesTests
{

    // Валидные значения (включая границы)
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-90, 0)]
    [InlineData(90, 0)]
    [InlineData(0, -180)]
    [InlineData(0, 180)]
    [InlineData(45.123, 120.456)]
    public void CtorCoordinatesTest_WithValidRange_ShouldReturnTrue(double latitude, double longitude)
    {
        
        var result = new Coordinates
        (
            latitude,
            longitude
        );
        
        Assert.Equal(result.Latitude, latitude);
        Assert.Equal(result.Longitude, longitude);
    }
    
    // Невалидная широта
    [Theory]
    [InlineData(-90.0000001)]
    [InlineData(90.0000001)]
    public void CtorCoordinatesTest_WithWrongLatRange_ShouldReturnException(double invalidLat)
    {
        double longitude = 0;
        
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(invalidLat, longitude));
        Assert.Equal("Latitude", ex.ParamName);
        Assert.Contains("between -90 and 90", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    
    // Невалидная долгота
    [Theory]
    [InlineData(-180.0000001)]
    [InlineData(180.0000001)]
    public void CtorCoordinatesTest_WithWrongLongRange_ShouldReturnException(double invalidLong)
    {
        double latitude = 0;
        
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(latitude, invalidLong));
        Assert.Equal("Longitude", ex.ParamName);
        Assert.Contains("between -180 and 180", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    
    // NaN/Infinity для широты
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Ctor_WithNonFiniteLatitude_ShouldThrow(double lat)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(lat, 0));
        Assert.Equal("Latitude", ex.ParamName);
    }

    // NaN/Infinity для долготы
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Ctor_WithNonFiniteLongitude_ShouldThrow(double lon)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(0, lon));
        Assert.Equal("Longitude", ex.ParamName);
    }
    
}