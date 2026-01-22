using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Auth;

namespace VirtualRouletteApi.Controllers;

/// <summary>
/// Authentication endpoints.
/// Register creates a new user with a hashed password
/// Login validates credentials and returns a Bearer token (Jwt)
/// Logout marks the current user as inactive
///
/// Notes:
/// In Basic mode, clients send credentials on each request.
/// In JWT mode, clients use Bearer tokens.
/// </summary>

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <response code="201">User created</response>
    /// <response code="400">Invalid input</response>
    /// <response code="409">Username already exists</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(req, ct);
        if (result.Success) return CreatedAtAction(nameof(Register), new { id = result.Value!.Id }, result.Value);

        return result.Error switch
        {
            AuthError.InvalidInput => BadRequest(result.Message),
            AuthError.UsernameTaken => Conflict(result.Message),
            _ => BadRequest("Registration failed.")
        };
    }
    
    /// <summary>
    /// Validates credentials.
    /// In JWT mode the response contains a token, in Basic mode token is null
    /// </summary>
    /// <response code="200">Valid credentials</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var result = await auth.LoginAsync(req, ct);
        if (result.Success) return Ok(result.Value);

        return result.Error switch
        {
            AuthError.InvalidInput => BadRequest(result.Message),
            AuthError.InvalidCredentials => Unauthorized(),
            _ => Unauthorized()
        };
    }
    
    /// <summary>
    /// Logs out the current user , sets IsActive=false.
    /// </summary>
    /// <response code="204">Logged out</response>
    /// <response code="401">Not authenticated</response>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(raw!);

        await auth.LogoutAsync(userId, ct);
        return Ok();
    }

}
