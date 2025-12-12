using Nubrio.Domain.Models.Daily;

namespace Nubrio.Domain.Models.Weekly;

public class WeeklyForecastMean
{
    
    public DailyForecastMean[] DailyForecasts { get; }
    public DateTimeOffset FetchedAtUtc { get; }
    
    
    public WeeklyForecastMean(DailyForecastMean[] dailyForecasts, DateTimeOffset fetchedAtUtc)
    {
        DailyForecasts = dailyForecasts;
        FetchedAtUtc = fetchedAtUtc;
    }
    

}