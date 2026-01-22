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
    /// <summary>
    /// Places a bet for the current user
    /// The bet JSON is validated by library.
    /// </summary>
    /// <response code="200">Bet processed (accepted or rejected)</response>
    /// <response code="401">Not authenticated</response>
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
    
    /// <summary>
    /// Returns bet history for the current user (newest first)
    /// 'take' is capped server side to prevent large responses
    /// </summary>
    /// <response code="200">History returned</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<BetHistory>>> GetHistory([FromQuery] CancellationToken ct, int take = 50)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await bets.GetHistoryAsync(userId, take, ct));
    }
}
