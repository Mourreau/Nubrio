using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nubrio.Application.Interfaces;

namespace Nubrio.Tests.Presentation.ControllersTests.IntegrationTests;

public class WeatherApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWeatherForecastService>();
            
            services.AddSingleton<FakeWeatherForecastService>();
            services.AddSingleton<IWeatherForecastService>(sp =>
                sp.GetRequiredService<FakeWeatherForecastService>());
        });
    }
}