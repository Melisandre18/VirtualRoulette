using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Auth;

namespace VirtualRouletteApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
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
