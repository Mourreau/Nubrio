using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;
using Nubrio.Infrastructure.Clients.ForecastClient;
using Nubrio.Infrastructure.Clients.GeocodingClient;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.CacheProvider;
using Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoForecast;
using Nubrio.Infrastructure.Providers.OpenMeteo.OpenMeteoGeocoding;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Nubrio.Infrastructure.Providers.OpenMeteo.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenMeteo(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("WeatherProviders");
        var clientOptions = section.Get<ProviderOptions>()
                            ?? throw new InvalidOperationException("WeatherProviders is not configured.");
        var openMeteoOptions = clientOptions.OpenMeteo
                               ?? throw new InvalidOperationException(
                                   "WeatherProviders:OpenMeteo section is required.");


        services.AddOptions<ProviderOptions>()
            .Bind(section)
            .Validate(o =>
                    o.OpenMeteo is not null,
                "WeatherProviders:OpenMeteo section is required")
            .Validate(o =>
                    o.OpenMeteo is not null &&
                    !string.IsNullOrWhiteSpace(o.OpenMeteo.ForecastBaseUrl),
                "ForecastBaseUrl is required")
            .Validate(o =>
                    o.OpenMeteo is not null &&
                    !string.IsNullOrWhiteSpace(o.OpenMeteo.GeocodingBaseUrl),
                "GeocodingBaseUrl is required")
            .Validate(o =>
                    o.OpenMeteo is not null &&
                    Uri.TryCreate(o.OpenMeteo.ForecastBaseUrl, UriKind.Absolute, out var u1) &&
                    u1.Scheme == Uri.UriSchemeHttps,
                "ForecastBaseUrl must be absolute https URL")
            .Validate(o =>
                    o.OpenMeteo is not null &&
                    Uri.TryCreate(o.OpenMeteo.GeocodingBaseUrl, UriKind.Absolute, out var u2) &&
                    u2.Scheme == Uri.UriSchemeHttps,
                "GeocodingBaseUrl must be absolute https URL")
            .Validate(o =>
                    o.OpenMeteo is not null &&
                    o.OpenMeteo.TimeoutSeconds is >= 1 and <= 30,
                "TimeoutSeconds must be 1..30")
            .ValidateOnStart();

        // Forecast (typed)
        services.AddHttpClient<IForecastClient, OpenMeteoForecastClient>((serviceProvider, client) =>
            {
                var opt = serviceProvider.GetRequiredService<IOptions<ProviderOptions>>().Value.OpenMeteo;
                client.BaseAddress = new Uri(opt.ForecastBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddResilienceHandler(nameof(OpenMeteoForecastClient), conf =>
                ConfigureResilience(conf, openMeteoOptions.TimeoutSeconds));

        // Geocoding (typed)
        services.AddHttpClient<IGeocodingClient, OpenMeteoGeocodingClient>((serviceProvider, client) =>
            {
                var opt = serviceProvider.GetRequiredService<IOptions<ProviderOptions>>().Value.OpenMeteo;
                client.BaseAddress = new Uri(opt.GeocodingBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddResilienceHandler(nameof(OpenMeteoGeocodingClient), conf =>
                ConfigureResilience(conf, openMeteoOptions.TimeoutSeconds));

        services.AddScoped<IGeocodingProvider, OpenMeteoGeocodingProvider>();

        services.AddScoped<OpenMeteoForecastProvider>();

        services.AddScoped<IForecastProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ProviderOptions>>().Value;
            var providerKey = options.OpenMeteo.Name;

            var openMeteoForecast = sp.GetRequiredService<OpenMeteoForecastProvider>();
            var cache = sp.GetRequiredService<IWeatherForecastCache>();
            var logger = sp.GetRequiredService<ILogger<CachedForecastProvider>>();
            var clock = sp.GetRequiredService<IClock>();

            return new CachedForecastProvider(cache, openMeteoForecast, providerKey, logger, clock);
        });

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
                (args.Outcome.Result is { StatusCode: >= (HttpStatusCode)500 }) // 5xx
                || (args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests) // 429
                || (args.Outcome.Exception is HttpRequestException) // сетевые ошибки
                || (args.Outcome.Exception is TaskCanceledException
                    && !args.Context.CancellationToken.IsCancellationRequested)) // таймаут
        });

        cfg.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5, // >=50% неуспехов в окне
            MinimumThroughput = 8, // минимальная выборка
            SamplingDuration = TimeSpan.FromSeconds(30), // окно наблюдения
            BreakDuration = TimeSpan.FromSeconds(15) // пауза до "пробного" запроса
        });
    }
}