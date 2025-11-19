namespace Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.TestData.OpenMeteoWeatherProviderTestData;

public class GetDailyForecastMeanAsyncTestData
{
    public static IEnumerable<object[]> NotEqualArrays() =>
        new List<object[]>
        {
            new object[]
            {
                new List<string> { "2025-10-20", "2025-10-21" },
                new List<double> { 1.5 },
                new List<int> { 47 }
            },
            new object[]
            {
                new List<string> { "2025-10-20"},
                new List<double> { 1.5, -5 },
                new List<int> { 47 }
            },
            new object[]
            {
                new List<string> { "2025-10-20" },
                new List<double> { 1.5 },
                new List<int> { 47, 10 }
            }
        };
    
    public static IEnumerable<object[]> TwoElementsInArray() =>
        new List<object[]>
        {
            new object[]
            {
                new List<string> { "2025-10-20", "2025-10-21" },
                new List<double> { 1.5, -10 },
                new List<int> { 47, 60 }
            }
        };
}