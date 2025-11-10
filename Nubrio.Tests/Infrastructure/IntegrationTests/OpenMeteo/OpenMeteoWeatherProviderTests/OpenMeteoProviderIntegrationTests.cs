using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.OpenMeteo.Extensions;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;
using Nubrio.Infrastructure.OpenMeteo.WmoCodes;
using Nubrio.Infrastructure.Services;
using Nubrio.Tests.Infrastructure.IntegrationTests.Http;

namespace Nubrio.Tests.Infrastructure.IntegrationTests.OpenMeteo.OpenMeteoWeatherProviderTests;

public class OpenMeteoProviderIntegrationTests
{
    private static ServiceProvider BuildProvider(HttpMessageHandler handler, string baseUrl, int timeoutSeconds = 5)
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WeatherProviders:OpenMeteo:BaseUrl"] = baseUrl,
                ["WeatherProviders:OpenMeteo:TimeoutSeconds"] = timeoutSeconds.ToString()
            })
            .Build();

        var services = new ServiceCollection();
        
        // Зависимости домена/мапперы, как в Program.cs
        services.AddSingleton<IConditionStringMapper, OpenMeteoConditionStringMapper>();
        services.AddSingleton<ITimeZoneResolver, TimeZoneResolver>();
        services.AddSingleton<IWeatherCodeTranslator, OpenMeteoWeatherCodeTranslator>();
        
        // Провайдер + клиент
        services.AddOpenMeteo(cfg);

        services.AddHttpClient(PipelineName).ConfigurePrimaryHttpMessageHandler(() => handler);
        
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(IntegrationTestData.ValidDailyMeanForecastJson)]
    public async Task GetDailyForecastMean_HappyPath_ReturnsMean(string validJson)
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, __) => Json200(validJson));
        var sp = BuildProvider(handler, "https://api.test/");
        var provider = sp.GetRequiredService<IWeatherProvider>();
        
        var location = new Location(
        Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow");
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
        var handler = new StubHttpMessageHandler((_, call) =>
            call < 3 ? new  HttpResponseMessage(HttpStatusCode.InternalServerError)
                : Json200(validJson));
        
        var sp = BuildProvider(handler, "https://api.test/");
        var provider = sp.GetRequiredService<IWeatherProvider>();
        
        var location = new Location(
            Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow");
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
        var badJson = validJson.Replace("\"°C\"", "\"°F\"");
        
        var handler = new StubHttpMessageHandler((_, __) => Json200(badJson));
        var sp = BuildProvider(handler, "https://api.test/");
        var provider = sp.GetRequiredService<IWeatherProvider>();
        
        var location = new Location(
            Guid.NewGuid(), "Moscow", new Coordinates(55.75, 37.62), "Europe/Moscow");
        var date = DateOnly.Parse("2025-10-20", System.Globalization.CultureInfo.InvariantCulture);
        
        // Act
        var result = await provider.GetDailyForecastMeanAsync(location, date, CancellationToken.None);

        // Assert
        handler.Calls.Should().Be(1);
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Metadata?["Code"] as string)
            .Should().Contain(OpenMeteoErrorCodes.UnitsMismatch);
    }
    
    
    // ------------------------------------------------------------------------------------------------------------- //
    private const string PipelineName = "openmeteo";
    
    private static HttpResponseMessage Json200(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage Status(HttpStatusCode code) => new(code);
}