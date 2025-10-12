using Microsoft.AspNetCore.Mvc;
using Nubrio.Application.Interfaces;
using Nubrio.Presentation.DTOs.Response;
using Nubrio.Presentation.Mappers;

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
    public async Task<ActionResult<CurrentWeatherResponseDto>> GetCurrentForecastByCity(
        [FromRoute] string city,
        CancellationToken cancellationToken)
    {
        var currentForecast = await _weatherForecastService.GetCurrentForecastAsync(city, cancellationToken);

        if (currentForecast.IsFailed)
            return BadRequest(currentForecast.Errors);


        return Ok(ForecastMapper.ToCurrentResponseDto(currentForecast.Value));
    }
}