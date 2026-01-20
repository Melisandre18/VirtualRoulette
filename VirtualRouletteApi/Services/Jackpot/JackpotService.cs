using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Jackpot;

public class JackpotService(AppDbContext db) : IJackpotService
{
    public async Task<JackpotResponse> GetAsync(CancellationToken ct)
    {
        var state = await db.Jackpots.SingleOrDefaultAsync(x => x.Id == 1, ct);
        if (state is null)
        {
            state = new Domain.Jackpot { Id = 1, Amount = 0, UpdatedAt = DateTimeOffset.UtcNow };
            db.Jackpots.Add(state);
            await db.SaveChangesAsync(ct);
        }

        return new JackpotResponse(state.Amount);
    }
}