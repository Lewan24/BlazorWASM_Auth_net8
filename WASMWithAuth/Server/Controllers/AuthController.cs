using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Text.Json;
using WASMWithAuth.Server.Models;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null) return BadRequest("User does not exist");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded) return BadRequest("Invalid password");

        await _signInManager.SignInAsync(user, request.RememberMe);
        return Ok();
    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser();
        user.UserName = request.UserName;

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);

        return await Login(new LoginRequest()
        {
            UserName = request.UserName,
            Password = request.Password,
        });
    }

    [Authorize]
    [HttpPost]
    [Route("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpGet]
    [Route("GetCurrentUser")]
    public CurrentUser GetCurrentUserInfo()
    {
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        var json = JsonSerializer.Serialize(roles);
        var claims = User.Claims.Where(c => c.Type != ClaimTypes.Role).ToDictionary(c => c.Type, c => c.Value);
        claims.Add(ClaimTypes.Role, json);
        return new CurrentUser
        {
            IsAuthenticated = User.Identity.IsAuthenticated,
            UserName = User.Identity.Name,
            Claims = claims
        };
    }
}