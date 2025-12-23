using System.Configuration;
using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nubrio.Application.Common;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.Clients.ForecastClient;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Persistence;
using Nubrio.Infrastructure.Providers.OpenMeteo.Extensions;
using Nubrio.Infrastructure.Providers.OpenMeteo.WmoCodes;
using Nubrio.Infrastructure.Services;
using Nubrio.Presentation.Common;
using Nubrio.Tests.Infrastructure.IntegrationTests.Clients;

namespace Nubrio.Tests.Infrastructure.IntegrationTests.OpenMeteo.OpenMeteoWeatherProviderTests;

public class OpenMeteoProviderIntegrationTests
{
    private const string ForecastBaseUrl = "https://api.test.forecast/";
    private const string GeocodeBaseUrl = "https://api.test.geocode/";
    

    private static ServiceProvider BuildProvider(HttpMessageHandler handler, string forecastBaseUri,
        string? geocodingBaseUri, int timeoutSeconds = 5, int ttl = 5)
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WeatherProviders:OpenMeteo:ForecastBaseUrl"] = forecastBaseUri,
                ["WeatherProviders:OpenMeteo:GeocodingBaseUrl"] = geocodingBaseUri,
                ["WeatherProviders:OpenMeteo:TimeoutSeconds"] = timeoutSeconds.ToString(),
                ["WeatherCache:TtlMinutes"] =  ttl.ToString(),
                ["ConnectionStrings:NubrioDb"] = "Host=localhost;Port=5432;Database=nubrio;Username=nubrio_user;Password=secret"
            })
            .Build();

        var services = new ServiceCollection();
        
        

        // Зависимости домена/мапперы, как в Program.cs
        services.AddSingleton<IConfiguration>(cfg);
        services.AddOptions<WeatherCacheOptions>()
            .Bind(cfg.GetSection("WeatherCache"));
        
        
        services.AddSingleton<IConditionStringMapper, OpenMeteoConditionStringMapper>();
        services.AddSingleton<ITimeZoneResolver, TimeZoneResolver>();
        services.AddSingleton<IWeatherCodeTranslator, OpenMeteoWeatherCodeTranslator>();
        services.AddScoped<IClock, Clock>();
        services.AddScoped<IWeatherForecastCache, MemoryWeatherForecastCache>();
        services.AddMemoryCache();
        services.AddPersistence(cfg);
        services.AddHttpContextAccessor();
        services.AddScoped<ICacheHitAccessor, HttpContextCacheHitAccessor>();

        // Провайдер + клиент
        services.AddOpenMeteo(cfg);

        services.AddHttpClient<IForecastClient, OpenMeteoForecastClient>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(IntegrationTestData.ValidDailyMeanForecastJson)]
    public async Task GetDailyForecastMean_HappyPath_ReturnsMean(string validJson)
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, __) => Json200(validJson));
        var sp = BuildProvider(handler, ForecastBaseUrl, GeocodeBaseUrl);
        var provider = sp.GetRequiredService<IForecastProvider>();
        
        string providerName = "test-provider";
        string externalId = "00001";

        var location = new Location(
            Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow",
            new ExternalLocationId(providerName, externalId));
        var date = DateOnly.Parse("2025-10-20", System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var result = await provider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(string.Join(" | ", result.Errors.Select(e => e.Message)));
    }

    [Theory]
    [InlineData(IntegrationTestData.ValidDailyMeanForecastJson)]
    public async Task GetDailyForecastMean_WhenServer500_Then200_RetriesAndSucceeds(string validJson)
    {
        // Arrange 
        string providerName = "test-provider";
        string externalId = "00001";
        
        
        var handler = new StubHttpMessageHandler((_, call) =>
            call < 3
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : Json200(validJson));

        var sp = BuildProvider(handler, ForecastBaseUrl, GeocodeBaseUrl);
        var provider = sp.GetRequiredService<IForecastProvider>();

        var location = new Location(
            Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow",
            new ExternalLocationId(providerName, externalId));
        var date = DateOnly.Parse("2025-10-20", System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var result = await provider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(string.Join(" | ", result.Errors.Select(e => e.Message)));
        handler.Calls.Should().Be(3);
    }

    [Theory]
    [InlineData(IntegrationTestData.ValidDailyMeanForecastJson)]
    public async Task GetDailyForecastMean_WhenUnitsMismatch_ReturnsFailWithUnitsCode(string validJson)
    {
        // Arrange 
        string providerName = "test-provider";
        string externalId = "00001";
        
        
        var badJson = validJson.Replace("\"°C\"", "\"°F\"");

        var handler = new StubHttpMessageHandler((_, __) => Json200(badJson));
        var sp = BuildProvider(handler, ForecastBaseUrl, GeocodeBaseUrl);
        var provider = sp.GetRequiredService<IForecastProvider>();

        var location = new Location(
            Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow", 
            new ExternalLocationId(providerName, externalId));
        var date = DateOnly.Parse("2025-10-20", System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var result = await provider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);

        // Assert
        handler.Calls.Should().Be(1);
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Metadata?["ProviderCode"] as string)
            .Should().Contain("Forecast.UnitsMismatch");
    }


    // ------------------------------------------------------------------------------------------------------------- //

    private static HttpResponseMessage Json200(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage Status(HttpStatusCode code) => new(code);
}