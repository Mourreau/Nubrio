using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Infrastructure.Http;
using Nubrio.Infrastructure.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Nubrio.Infrastructure.OpenMeteo.Extensions;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddOpenMeteo(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("WeatherProviders:OpenMeteo");
        var clientOptions = section.Get<OpenMeteoOptions>() 
                             ?? throw new InvalidOperationException("WeatherProviders:OpenMeteo is not configured.");
        
        
        services.AddOptions<OpenMeteoOptions>()
            .Bind(section)
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl is required")
            .Validate(o => o.TimeoutSeconds is >= 1 and <= 30, "TimeoutSeconds must be 1..30")
            .ValidateOnStart();
        
        
        services.Configure<OpenMeteoOptions>(section);
        
        services.AddHttpClient(PipelineName, (serviceProvider, client) =>
            {
                var opt = serviceProvider.GetRequiredService<IOptions<OpenMeteoOptions>>().Value;
                client.BaseAddress = new Uri(opt.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddResilienceHandler(PipelineName, conf =>
            {
                conf.AddTimeout(TimeSpan.FromSeconds(clientOptions.TimeoutSeconds));
                conf.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = _ => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromMilliseconds(200)),
                    ShouldHandle = args => ValueTask.FromResult(
                        // 5xx
                        (args.Outcome.Result is { StatusCode: >= (HttpStatusCode)500 })
                        // 429
                        || (args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
                        // сетевые ошибки (но не явную отмену пользователя)
                        || (args.Outcome.Exception is HttpRequestException)
                        || (args.Outcome.Exception is TaskCanceledException && !args.Context.CancellationToken.IsCancellationRequested))
                });
                conf.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = 0.5,                // если >=50% исходов за окно — считаем, что всё плохо
                    MinimumThroughput = 8,             // минимум 8 запросов, чтобы статистика была осмысленной
                    SamplingDuration = TimeSpan.FromSeconds(30), // окно наблюдения
                    BreakDuration = TimeSpan.FromSeconds(15)     // пауза перед «пробным» запросом
                });
            });

        services.AddTransient<IOpenMeteoClient, OpenMeteoClient>();
        services.AddScoped<IWeatherProvider, OpenMeteoWeatherProvider>();
        
        return services;
    }

    private const string PipelineName = "openmeteo";
}