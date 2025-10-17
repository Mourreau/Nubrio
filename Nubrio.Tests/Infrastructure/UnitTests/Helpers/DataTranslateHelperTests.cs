using System.Globalization;
using Nubrio.Infrastructure.Helpers;
using Xunit.Abstractions;

namespace Nubrio.Tests.Infrastructure.UnitTests.Helpers;

public class DataTranslateHelperTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DataTranslateHelperTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("2025-09-30T13:15", "Asia/Yekaterinburg", "2025-09-30T08:15:00.0000000+00:00")]
    [InlineData("2000-01-01T00:00", "Europe/Moscow", "1999-12-31T21:00:00.0000000+00:00")]
    [InlineData("1990-01-01T23:59", "Pacific/Auckland", "1990-01-01T10:59:00.0000000+00:00")]
    [InlineData("2005-07-30T13:00", "America/New_York", "2005-07-30T17:00:00.0000000+00:00")]
    public void GetUtcDateTimeOffsetFromString_WithValidData_ShouldReturnTrue(
        string dateString, string timeZoneId, string offset)
    {
        string dateFormat = "yyyy-MM-dd'T'HH:mm";

        DateTimeOffset expectedDateTimeOffset = DateTimeOffset.Parse(offset);
        
        
        var result = DataTranslateHelper.GetUtcDateTimeOffsetFromString(dateString, timeZoneId);
        
        _testOutputHelper.WriteLine(result.Value.Offset.ToString());
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDateTimeOffset, result.Value);
        Assert.Equal(TimeSpan.Zero, result.Value.Offset); // Проверка, что время действительно в UTC с offset = 00:00

    }
    
    /// <summary>
    /// Проверка несуществующего времени - когда во время перехода с летнего на зимнее время теряется час (02:00-02:59)
    /// </summary>
    /// <param name="local"></param>
    /// <param name="tz"></param>
    [Theory]
    [InlineData("2025-03-09T02:30", "America/New_York")] // не существует (прыжок на 03:00)
    public void InvalidLocalTime_ShouldFail(string local, string tz)
    {
        var result = DataTranslateHelper.GetUtcDateTimeOffsetFromString(local, tz);
        Assert.True(result.IsFailed);
    }
    
    [Theory]
    [InlineData("bad", "Europe/Moscow")]
    [InlineData("2025-01-01T00:00", "No/SuchZone")]
    public void InvalidInput_ShouldFail(string local, string tz)
    {
        var result = DataTranslateHelper.GetUtcDateTimeOffsetFromString(local, tz);
        Assert.True(result.IsFailed);
    }
    
    [Theory]
    [InlineData("2024-11-15T10:00", "Europe/Berlin")]
    public void Roundtrip_LocalUtcLocal_PreservesLocalOnUnambiguousTimes(string local, string tz)
    {
        var utc = DataTranslateHelper.GetUtcDateTimeOffsetFromString(local, tz).Value;
        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tz);
        var backLocal = TimeZoneInfo.ConvertTime(utc, tzInfo); // в локальную зону
        Assert.Equal(DateTime.ParseExact(local, "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture), backLocal.DateTime);
    }
}