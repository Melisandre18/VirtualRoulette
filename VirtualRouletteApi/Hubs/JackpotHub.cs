using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Services.Jackpot;

namespace VirtualRouletteApi.Hubs;

[Authorize]
public class JackpotHub(IJackpotService jackpot, AppDbContext db) : Hub
{
    public const string AuthorizedGroup = "authorized";

    /// <summary>
    /// Called automatically when a client connects
    /// Marks the user as active, updates last-seen timestamp,
    /// sends the current jackpot value to the user,
    /// and adds the connection to the authorized group.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AuthorizedGroup);
        
        var raw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            var userId = Guid.Parse(raw);
            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, Context.ConnectionAborted);
            if (user is not null)
            {
                user.IsActive = true;
                user.LastSeen = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(Context.ConnectionAborted);
            }
        }
        
        var current = await jackpot.GetAsync(Context.ConnectionAborted);
        await Clients.Caller.SendAsync("jackpotChanged", current.Jackpot, Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called automatically when a client disconnects
    /// Removes the connection from the authorized group
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AuthorizedGroup);
        await base.OnDisconnectedAsync(exception);
    }
}

