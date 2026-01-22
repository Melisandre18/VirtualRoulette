using Microsoft.AspNetCore.SignalR;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Hubs;
using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Jackpot;

public class JackpotService(IJackpotStore store, IHubContext<JackpotHub> hub) : IJackpotService
{
    public async Task<JackpotResponse> GetAsync(CancellationToken ct)
    {
        var current = await store.GetAsync(ct);
        return new JackpotResponse(current);
    }

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
