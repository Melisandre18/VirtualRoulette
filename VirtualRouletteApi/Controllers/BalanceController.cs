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
    /// <summary>
    /// Returns current user's balance
    /// </summary>
    /// <response code="200">Balance returned</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    public async Task<ActionResult<BalanceResponse>> Get(CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await balance.GetAsync(userId, ct));
    }
    
    /// <summary>
    /// Deposits amount into the current user's balance
    /// </summary>
    /// <response code="200">Deposit applied</response>
    /// <response code="400">Amount invalid</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("deposit")]
    public async Task<ActionResult<BalanceResponse>> Deposit(
        BalanceChangeRequest req,
        CancellationToken ct)
    {
        try
        {
            return Ok(await balance.DepositAsync(GetUserId(), req.Amount, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Withdraws amount from the current user's balance
    /// </summary>
    /// <response code="200">Withdraw applied</response>
    /// <response code="400">Amount invalid</response>
    /// <response code="409">Insufficient balance</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("withdraw")]
    public async Task<ActionResult<BalanceResponse>> Withdraw(
        BalanceChangeRequest req,
        CancellationToken ct)
    {
        try
        {
            return Ok(await balance.WithdrawAsync(GetUserId(), req.Amount, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException)
        {
            return Conflict("Insufficient balance.");
        }
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(raw!);
    }
}
