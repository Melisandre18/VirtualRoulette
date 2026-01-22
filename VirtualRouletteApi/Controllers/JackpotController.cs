using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Jackpot;

namespace VirtualRouletteApi.Controllers;

// Clients should connect to SignalR /jackpot-hub to receive live updates.
[ApiController]
[Route("api/jackpot")]
[Authorize]
public class JackpotController(IJackpotService jackpot) : ControllerBase
{
    /// <summary>
    /// Returns current jackpot amount
    /// </summary>
    /// <response code="200">Jackpot returned</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    public async Task<ActionResult<JackpotResponse>> Get(CancellationToken ct)
        => Ok(await jackpot.GetAsync(ct));
}