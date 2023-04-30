using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WASMWithAuth.Server.Data.Interfaces;
using WASMWithAuth.Shared;

namespace WASMWithAuth.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
    
    private ITokenValidationService _tokenValidationService;

    public WeatherForecastController(ITokenValidationService tokenValidationService)
    {
        _tokenValidationService = tokenValidationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route("GetWeather")]
    public IActionResult GetWeather(string token)
    {
        if (!_tokenValidationService.CheckValidation(token, User.Identity.Name))
            return Ok("Invalid auth");

        return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
    }
}