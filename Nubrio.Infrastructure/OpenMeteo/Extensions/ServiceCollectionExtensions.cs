using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Infrastructure.Http;
using Nubrio.Infrastructure.Http.GeocodingClient;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoForecast;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding;
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
            .Validate(o => !string.IsNullOrWhiteSpace(o.ForecastBaseUrl), "ForecastBaseUrl is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.GeocodingBaseUrl), "GeocodingBaseUrl is required")
            .Validate(o => Uri.TryCreate(o.ForecastBaseUrl, UriKind.Absolute, out var u1) && u1.Scheme == Uri.UriSchemeHttps,
                "ForecastBaseUrl must be absolute https URL")
            .Validate(o => Uri.TryCreate(o.GeocodingBaseUrl, UriKind.Absolute, out var u2) && u2.Scheme == Uri.UriSchemeHttps,
                "GeocodingBaseUrl must be absolute https URL")
            .Validate(o => o.TimeoutSeconds is >= 1 and <= 30, "TimeoutSeconds must be 1..30")
            .ValidateOnStart();

        // Forecast (typed)
        services.AddHttpClient<IForecastClient, OpenMeteoForecastClient>((serviceProvider, client) =>
            {
                var opt = serviceProvider.GetRequiredService<IOptions<OpenMeteoOptions>>().Value;
                client.BaseAddress = new Uri(opt.ForecastBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddResilienceHandler(OpenMeteoProviderInfo.OpenMeteoForecast, conf =>
                ConfigureResilience(conf, clientOptions.TimeoutSeconds));

        // Geocoding (typed)
        services.AddHttpClient<IGeocodingClient, OpenMeteoGeocodingClient>((serviceProvider, client) =>
            {
                var opt = serviceProvider.GetRequiredService<IOptions<OpenMeteoOptions>>().Value;
                client.BaseAddress = new Uri(opt.GeocodingBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddResilienceHandler(OpenMeteoProviderInfo.OpenMeteoGeocoding, conf =>
                ConfigureResilience(conf, clientOptions.TimeoutSeconds));

        services.AddScoped<IGeocodingProvider, OpenMeteoGeocodingProvider>();
        services.AddScoped<IWeatherProvider, OpenMeteoWeatherProvider>();

        return services;
    }
    
    
    // Общая настройка Polly для обоих клиентов
    private static void ConfigureResilience(ResiliencePipelineBuilder<HttpResponseMessage> cfg, int timeoutSec)
    {
        cfg.AddTimeout(TimeSpan.FromSeconds(timeoutSec));

        cfg.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            DelayGenerator = _ => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromMilliseconds(200)),
            ShouldHandle = args => ValueTask.FromResult(
                (args.Outcome.Result is { StatusCode: >= (HttpStatusCode)500 })                  // 5xx
                || (args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)          // 429
                || (args.Outcome.Exception is HttpRequestException)                              // сетевые ошибки
                || (args.Outcome.Exception is TaskCanceledException
                    && !args.Context.CancellationToken.IsCancellationRequested))                // таймаут
        });

        cfg.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio      = 0.5,                         // >=50% неуспехов в окне
            MinimumThroughput = 8,                           // минимальная выборка
            SamplingDuration  = TimeSpan.FromSeconds(30),    // окно наблюдения
            BreakDuration     = TimeSpan.FromSeconds(15)     // пауза до "пробного" запроса
        });
    }
}