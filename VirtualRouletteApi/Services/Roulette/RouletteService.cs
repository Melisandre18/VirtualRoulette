using System.Security.Cryptography;
using ge.singular.roulette;

namespace VirtualRouletteApi.Services.Roulette;

public class RouletteService : IRouletteService
{
    
    /// <summary>
    /// Validates bet JSON using the library and extracts the bet amount
    /// Returns false if the JSON is invalid, unsupported, or the library rejects it
    /// </summary>
    public bool ValidateBet(string betJson, out long betAmount)
    {
        betAmount = 0;

        var result = CheckBets.IsValid(betJson);
        if (!result.getIsValid())
            return false;

        betAmount = result.getBetAmount();
        return true;
    }
    
    /// <summary>
    /// Generates a winning roulette number in range [0..36].
    /// Used RandomNumberGenerator (secure) instead of Random,
    /// </summary>
    public int GenerateWinningNumber()
    {
        return RandomNumberGenerator.GetInt32(0, 37);
    }
    
    /// <summary>
    /// Estimates the win amount
    /// returns 0 if the bet loses
    /// </summary>
    public long CalculateWin(string betJson, int winningNumber)
    {
        return CheckBets.EstimateWin(betJson, winningNumber);
    }
}