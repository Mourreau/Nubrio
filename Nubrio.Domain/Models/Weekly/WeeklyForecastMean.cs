using Nubrio.Domain.Models.Daily;

namespace Nubrio.Domain.Models.Weekly;

public class WeeklyForecastMean
{
    
    public DailyForecastMean[] DailyForecasts { get; }
    
    
    public WeeklyForecastMean(DailyForecastMean[] dailyForecasts)
    {
        DailyForecasts = dailyForecasts;
    }

}