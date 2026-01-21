using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<long> ChangeOnBetAsync(long betAmount, CancellationToken ct)
    {
        if (betAmount <= 0)
            return (await GetAsync(ct)).Jackpot;

        long addition;
        checked
        {
            addition = betAmount * 100;
        }

        var now = DateTimeOffset.UtcNow;

        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO ""Jackpots"" (""Id"", ""Amount"", ""UpdatedAt"")
        VALUES (1, @amount, @now)
        ON CONFLICT (""Id"") DO UPDATE
        SET ""Amount"" = ""Jackpots"".""Amount"" + EXCLUDED.""Amount"",
            ""UpdatedAt"" = EXCLUDED.""UpdatedAt""
        RETURNING ""Amount"";
    ";
        
        var currentTx = db.Database.CurrentTransaction;
        if (currentTx is not null)
            command.Transaction = currentTx.GetDbTransaction();

        var amountParam = command.CreateParameter();
        amountParam.ParameterName = "amount";
        amountParam.Value = addition;
        command.Parameters.Add(amountParam);

        var nowParam = command.CreateParameter();
        nowParam.ParameterName = "now";
        nowParam.Value = now;
        command.Parameters.Add(nowParam);

        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt64(result);
    }

}
