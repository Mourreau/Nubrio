using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Services;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.MockProvider;
using Nubrio.Infrastructure.Providers.MockProvider.MockGeocoding;
using Nubrio.Infrastructure.Providers.OpenMeteo.Extensions;
using Nubrio.Infrastructure.Providers.OpenMeteo.WmoCodes;
using Nubrio.Infrastructure.Services;
using Nubrio.Presentation.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(o =>
{
    o.Filters.Add<ResultToActionResultFilter>();
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
});

builder.Services
    .Configure<ProviderOptions>(
        builder.Configuration.GetSection("WeatherProviders"));
builder.Services.Configure<WeatherCacheOptions>(builder.Configuration.GetSection("WeatherCache"));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClock, Clock>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastCache, MemoryWeatherForecastCache>();

builder.Services.AddSingleton<IConditionStringMapper, OpenMeteoConditionStringMapper>();
builder.Services.AddSingleton<ITimeZoneResolver, TimeZoneResolver>();

builder.Configuration.AddJsonFile(
    "Configuration/weathercode-mapping.json",
    optional: false,
    reloadOnChange: true);

builder.Services.Configure<WeatherCodeMappings>(
    builder.Configuration.GetSection("WeatherCodeMappings"));

builder.Services.Configure<LanguageResolverOptions>(
    builder.Configuration.GetSection("Localization"));

builder.Services.AddSingleton<IWeatherCodeTranslator, OpenMeteoWeatherCodeTranslator>();
builder.Services.AddSingleton<ILanguageResolver, LanguageResolver>();

var useMock = builder.Configuration.GetValue<bool>("UseMockProviders");
if (useMock)
{
    builder.Services.AddScoped<IForecastProvider, MockForecastProvider>();
    builder.Services.AddScoped<IGeocodingProvider, MockGeocodingProvider>();
}
else
{
    builder.Services.AddOpenMeteo(builder.Configuration);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }