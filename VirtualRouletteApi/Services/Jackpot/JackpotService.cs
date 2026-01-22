using Microsoft.AspNetCore.SignalR;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Hubs;
using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Jackpot;

/// <summary>
/// Handles jackpot reads/updates and broadcasts jackpot changes to connected clients
/// </summary>
public class JackpotService(IJackpotStore store, IHubContext<JackpotHub> hub) : IJackpotService
{
    /// <summary>
    /// Returns the current jackpot amount
    /// </summary>
    public async Task<JackpotResponse> GetAsync(CancellationToken ct)
    {
        var current = await store.GetAsync(ct);
        return new JackpotResponse(current);
    }
    /// <summary>
    /// Does the jackpot change on bet and notifies all connected authorized clients.
    /// </summary>
    public async Task<long> ChangeOnBetAsync(long betAmount, CancellationToken ct)
    {
        if (betAmount <= 0)
            return await store.GetAsync(ct);
        long addition;
        checked
        {
            addition = betAmount * 100;
        }

        var newAmount = await store.AddUnitsAsync(addition, ct);
        await hub.Clients
            .Group(JackpotHub.AuthorizedGroup)
            .SendAsync("jackpotChanged", newAmount, ct);

        return newAmount;
    }

}
