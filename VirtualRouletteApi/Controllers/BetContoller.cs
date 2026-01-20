using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Bets;

namespace VirtualRouletteApi.Controllers;

[ApiController]
[Route("api/bet")]
[Authorize]
public class BetController(IBetService bets) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BetResponse>> MakeBet(BetRequest req, CancellationToken ct)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Bet))
            return BadRequest("Bet is required.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await bets.MakeBetAsync(userId, req.Bet, ip, ct);
        
        if (result.BetId == Guid.Empty)
            return BadRequest(result);

        return Ok(result);
    }
}
