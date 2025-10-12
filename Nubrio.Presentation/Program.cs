using Nubrio.Application.Interfaces;
using Nubrio.Application.Services;
using Nubrio.Infrastructure.MockProvider;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding;
using Nubrio.Infrastructure.OpenMeteo.WmoCodes;
using Nubrio.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IClock, Clock>();
builder.Services.AddScoped<IConditionStringMapper, OpenMeteoConditionStringMapper>();
builder.Services.AddScoped<IGeocodingService, OpenMeteoGeocoding>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherProvider, OpenMeteoWeatherProvider>();

builder.Configuration.AddJsonFile(
    "Configuration/weathercode-mapping.json", 
    optional: false, 
    reloadOnChange: true);

builder.Services.Configure<OpenMeteoWeatherCodeTranslator>(builder.Configuration.GetSection("WeatherCodeMappings"));
builder.Services.AddSingleton<IWeatherCodeTranslator, OpenMeteoWeatherCodeTranslator>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IWeatherProvider, MockWeatherProvider>();
}
else{}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
