using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Balance;

namespace VirtualRouletteApi.Controllers;

[ApiController]
[Route("api/balance")]
[Authorize]
public class BalanceController(IBalanceService balance) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BalanceResponse>> Get(CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await balance.GetAsync(userId, ct));
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(raw!);
    }
}
