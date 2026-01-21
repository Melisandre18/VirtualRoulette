using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VirtualRouletteApi.Services.Jackpot;

namespace VirtualRouletteApi.Hubs;

[Authorize]
public class JackpotHub(IJackpotService jackpot) : Hub
{
    public const string AuthorizedGroup = "authorized";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AuthorizedGroup);
        var current = await jackpot.GetAsync(Context.ConnectionAborted);
        await Clients.Caller.SendAsync("jackpotChanged", current.Jackpot, Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AuthorizedGroup);
        await base.OnDisconnectedAsync(exception);
    }
}
