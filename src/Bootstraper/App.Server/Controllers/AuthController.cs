using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using App.Server.Data;
using App.Server.Data.Interfaces;
using App.Server.Models;
using Shared.Entities;
using Shared.Entities.Models;

namespace App.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenValidationService _tokenValidationService;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, ITokenValidationService tokenValidationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenValidationService = tokenValidationService;
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

        var result = await GetUserToken(request);

        if (string.IsNullOrWhiteSpace(result.Token))
            await CreateNewUserToken(request.UserName, expirationTimeInMinutes: 20);

        return Ok();
    }

    [HttpPost]
    [Authorize]
    [Route("TryLogin")]
    public async Task<bool> TryLogin(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        var result = await _userManager.CheckPasswordAsync(user, request.Password);

        return result;
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
    public async Task<IActionResult> Logout() => Ok(await CheckActiveTokens(GetCurrentUserInfo().UserName));

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

    [HttpPost]
    [Route("GetUserToken")]
    [Authorize]
    public async Task<UserToken> GetUserToken(LoginRequest request)
    {
        var result = _context.UserTokens.FirstOrDefault(t => t.IsInActive == false && t.UserName == request.UserName);

        if (result == null) return new();

        if (!_tokenValidationService.CheckValidation(result.Token, request.UserName))
        {
            result.IsInActive = true;
            await _context.SaveChangesAsync();

            return new();
        }

        return result;
    }

    private async Task<UserToken> CreateNewUserToken(string userName, int expirationTimeInMinutes)
    {
        var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var resultToken = new string(
            Enumerable.Repeat(allChar, 47)
                .Select(token => token[random.Next(token.Length)]).ToArray());

        UserToken newToken = new()
        {
            UserName = userName,
            ExpirationDate = DateTime.UtcNow.AddMinutes(expirationTimeInMinutes),
            Token = resultToken
        };

        var result = _context.UserTokens.Add(newToken);
        await _context.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(result.Entity.Token))
            return new();

        return newToken;
    }

    private async Task<int> CheckActiveTokens(string logoutingUserName)
    {
        foreach (var userToken in _context.UserTokens.Where(t => t.UserName == logoutingUserName && t.IsInActive == false))
            userToken.IsInActive = true;
        
        await _context.SaveChangesAsync();

        await _signInManager.SignOutAsync();

        var activeTokens = _context.UserTokens.Where(t => t.IsInActive == false);
        int CheckedAndChangedTokens = 0;

        foreach (var token in activeTokens)
            if (token.ExpirationDate < DateTime.UtcNow)
            {
                token.IsInActive = true;
                CheckedAndChangedTokens++;
            }

        await _context.SaveChangesAsync();

        return CheckedAndChangedTokens;
    }
    
    [HttpPost]
    [Authorize]
    [Route("RefreshToken")]
    public async Task<UserToken> RefreshToken(LoginRequest request)
    {
        var resultOfLoginTry = await TryLogin(request);
        
        if (resultOfLoginTry)
             return await CreateNewUserToken(request.UserName, 20);

        return new(){IsInActive = true};
    }
}