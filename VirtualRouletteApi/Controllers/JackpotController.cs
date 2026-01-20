using Microsoft.AspNetCore.Mvc;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Jackpot;

namespace VirtualRouletteApi.Controllers;

[ApiController]
[Route("api/jackpot")]
public class JackpotController(IJackpotService jackpot) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<JackpotResponse>> Get(CancellationToken ct)
        => Ok(await jackpot.GetAsync(ct));
}