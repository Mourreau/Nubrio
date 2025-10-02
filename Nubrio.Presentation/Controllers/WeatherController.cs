using Microsoft.AspNetCore.Mvc;
using Nubrio.Application.Interfaces;
using Nubrio.Presentation.DTOs.Response;

namespace Nubrio.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : Controller
{
    private readonly IWeatherForecastService _weatherForecastService;

    public WeatherController(IWeatherForecastService weatherForecastService)
    {
        _weatherForecastService = weatherForecastService;
    }

    [HttpGet("{city}")]
    public async Task<ActionResult<CurrentWeatherResponseDto>> GetWeatherByCity(
        [FromRoute] string city)
    {
        var result = await _weatherForecastService.GetCurrentForecastAsync(city);

        if (result.IsSuccess)
        {
            var response = new CurrentWeatherResponseDto
            {
                City = result.Value.ForecastLocation.Name,
                Date = result.Value.Date,
                Condition = result.Value.Conditions.ToString(),
                Temperature = result.Value.Temperature,
                IconUrl = String.Empty,
                Source = "OpenMeteo",
                FetchedAt = DateTime.UtcNow
            };
            return Ok(response);
        }


    return NotFound();
    }
} 