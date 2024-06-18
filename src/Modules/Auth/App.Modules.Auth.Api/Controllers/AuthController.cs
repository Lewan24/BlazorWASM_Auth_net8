using System.Security.Claims;
using System.Text.Json;
using App.Models.Auth.Api.Entities;
using App.Models.Auth.Shared.ActionsRequests;
using App.Models.Auth.Shared.Entities.Tokens;
using App.Models.Auth.Shared.Interfaces.Token;
using App.Models.Auth.Shared.Static.Entities;
using App.Modules.Auth.Core.Entities;
using App.Modules.Auth.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Common;

namespace App.Models.Auth.Api.Controllers;

// TODO: Add and implement mediator
[Authorize]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppIdentityDbContext context,
    ITokenValidationService tokenValidationService,
    IConfiguration config,
    ILogger<AuthController> logger)
    : BaseController
{
    private readonly AuthSettings _authSettings = config.GetSection("AuthSettings").Get<AuthSettings>() ?? new();

    [HttpPost]
    [Route("Login")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest? request)
    {
        var isLoginValid = await CanLogIn(request);

        if (!isLoginValid)
            return BadRequest("Can't log user in. Check if the inputs are valid.");
        
        var user = await userManager.FindByEmailAsync(request!.Email ?? "");
        if (user is null)
            return NotFound("Can't find matched user");
        
        if (!user.EmailConfirmed)
            return BadRequest("User's email is not confirmed");

        await signInManager.SignInAsync(user, request!.RememberMe);

        var result = await GetUserToken(request);

        if (string.IsNullOrWhiteSpace(result.Token))
            await CreateNewUserToken(request.Email!, _authSettings.DefaultTokenExpirationTimeInMinutes);

        return Accepted();
    }

    [HttpPost]
    [Route("CanLogIn")]
    [Produces(typeof(bool))]
    public async Task<bool> CanLogIn(LoginRequest? request)
    {
        if (request == null)
            return false;

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return false;

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return false;

        var result = await userManager.CheckPasswordAsync(user, request.Password);

        return result;
    }

    [HttpPost]
    [Route("Register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!_authSettings.EnableRegisterModule)
            return BadRequest("Register module is disabled by Administrator");

        if (request == null)
            return BadRequest("Request can not be null or empty");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("All request's items are required");

        if (!request.Password.Equals(request.PasswordConfirm))
            return BadRequest("Passwords doesn't match!");
        
        var newUser = new AppUser();
        newUser.UserName = request.Email;
        newUser.Email = request.Email;

        try
        {
            var result = await userManager.CreateAsync(newUser, request.Password);
            
            if (!result.Succeeded)
                return BadRequest(result.Errors.First().Description);
        }
        catch(Exception ex)
        {
            return BadRequest($"Error while creating new user: {ex.Message}");
        }

        var createdUser = await userManager.FindByIdAsync(newUser.Id);
        if (createdUser == null)
            return NotFound("Can't find created user. Try again");

        await userManager.AddToRoleAsync(createdUser, AppRoles.User);

        if (createdUser.Email!.Equals(_authSettings.MainAdminEmailAddress, StringComparison.InvariantCultureIgnoreCase))
            await userManager.AddToRoleAsync(createdUser, AppRoles.Admin);
        
        if (_authSettings is not null && _authSettings.AutoConfirmAccount)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await userManager.ConfirmEmailAsync(newUser, confirmationToken);
        }

        return CreatedAtAction(nameof(Register), createdUser.Email);
    }

    [HttpPost]
    [Route("Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        var currentUser = GetCurrentUserInfo();

        await CheckAndDeleteActiveUserTokens(currentUser.UserName);

        return NoContent();
    }

    private async Task CheckAndDeleteActiveUserTokens(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return;

        var activeUserTokens = context.UsersTokens.Where(t => t.Email == userName);

        if (activeUserTokens.Any())
        {
            context.UsersTokens.RemoveRange(activeUserTokens);
            await context.SaveChangesAsync();
        }
    }

    [HttpGet]
    [Route("GetCurrentUser")]
    [Produces(typeof(CurrentUser))]
    [AllowAnonymous]
    public CurrentUser GetCurrentUserInfo()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        var json = JsonSerializer.Serialize(roles);

        var claims = User.Claims
            .Where(c => c.Type != ClaimTypes.Role)
            .ToDictionary(c => c.Type, c => c.Value);

        claims.Add(ClaimTypes.Role, json);

        return new CurrentUser
        {
            IsAuthenticated = User.Identity!.IsAuthenticated,
            UserName = User.Identity.Name,
            Claims = claims
        };
    }

    [HttpPost]
    [Route("GetUserToken")]
    [Produces(typeof(TokenModelDto))]
    public async Task<TokenModelDto> GetUserToken(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new();

        var retrievedUserToken = context.UsersTokens.FirstOrDefault(t => t.Email == request.Email);

        if (retrievedUserToken == null) 
            return new();

        var isTokenValid = tokenValidationService.IsValid(retrievedUserToken.Token, request.Email);

        return await isTokenValid.Match<Task<TokenModelDto>>(
            valid => Task.FromResult(new TokenModelDto
            {
                Token = retrievedUserToken.Token,
                Email = retrievedUserToken.Email,
                ExpirationDate = retrievedUserToken.ExpirationDate
            }),
            async invalid =>
            {
                context.UsersTokens.Remove(retrievedUserToken);
                await context.SaveChangesAsync();

                return new TokenModelDto();
            }
        );
    }

    [HttpPost]
    [Route("RefreshToken")]
    [Produces(typeof(TokenModelDto))]
    public async Task<TokenModelDto> RefreshToken(LoginRequest request)
    {
        var isValidLogin = await CanLogIn(request);

        if (!isValidLogin)
            return new() { ExpirationDate = DateTime.UtcNow.AddMinutes(-1) };

        var existingUserToken = CheckIfAnyActiveUserTokenExist(request.Email!);

        if (existingUserToken is not null)
            return existingUserToken;

        return await CreateNewUserToken(request.Email!, _authSettings.DefaultTokenExpirationTimeInMinutes);
    }

    private TokenModelDto? CheckIfAnyActiveUserTokenExist(string userEmail)
    {
        var userToken = context.UsersTokens
            .FirstOrDefault(ut => ut.Email == userEmail && ut.ExpirationDate >= DateTime.UtcNow);

        if (userToken is null)
            return null;

        return new TokenModelDto
        {
            Email = userToken.Email,
            Token = userToken.Token,
            ExpirationDate = userToken.ExpirationDate
        };
    }

    private async Task<TokenModelDto> CreateNewUserToken(string userEmail, int expirationTimeInMinutes)
    {
        var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var resultToken = new string(
            Enumerable.Repeat(allChar, 47)
                .Select(token => token[random.Next(token.Length)]).ToArray());

        TokenModel newToken = new()
        {
            Email = userEmail,
            ExpirationDate = DateTime.UtcNow.AddMinutes(expirationTimeInMinutes),
            Token = resultToken
        };

        var addNewUserTokenResult = context.UsersTokens.Add(newToken);
        await context.SaveChangesAsync();

        var createdUserToken = addNewUserTokenResult.Entity;

        return new TokenModelDto
        {
            Token = createdUserToken.Token,
            Email = createdUserToken.Email,
            ExpirationDate = createdUserToken.ExpirationDate
        };
    }
}