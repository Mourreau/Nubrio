using FluentResults;
using Nubrio.Application.DTOs.CurrentForecast;
using Nubrio.Application.DTOs.DailyForecast;
using Nubrio.Application.DTOs.WeeklyForecast;
using Nubrio.Application.Interfaces;

namespace Nubrio.Tests.Presentation.ControllersTests.IntegrationTests;

internal class FakeWeatherForecastService : IWeatherForecastService
{
    public Result<DailyForecastMeanDto>? NextDailyResult {get; set;}
    public Result<WeeklyForecastMeanDto>? NextWeeklyResult {get; set;}
    
    public Task<Result<CurrentForecastDto>> GetCurrentForecastAsync(string city, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DailyForecastMeanDto>> GetDailyForecastByDateAsync(string city, DateOnly date, CancellationToken cancellationToken)
    {
        return Task.FromResult(NextDailyResult ?? Result.Fail("Fake result not set"));
    }

    public Task<Result<WeeklyForecastMeanDto>> GetWeeklyForecastAsync(string city, CancellationToken cancellationToken)
    {
        return Task.FromResult(NextWeeklyResult ?? Result.Fail("Fake result not set"));
    }
}