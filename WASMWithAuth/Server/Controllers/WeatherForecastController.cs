using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WASMWithAuth.Server.Data.Interfaces;
using WASMWithAuth.Shared;
using WASMWithAuth.Shared.Entities;

namespace WASMWithAuth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private ITokenValidationService _tokenValidationService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ITokenValidationService tokenValidationService)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("GetWeather")]
        public async Task<IActionResult> GetWeather(string token)
        {
            if (!await _tokenValidationService.CheckValidation(token, User.Identity.Name))
                return BadRequest("Invalid auth");

            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray());
        }
    }
}