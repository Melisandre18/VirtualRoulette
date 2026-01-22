namespace VirtualRouletteApi.Auth.Jwt;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    
    public string Audience { get; set; } = "";
    
    public string SigningKey { get; set; } = "";
    
    public int ExpiresMinutes { get; set; } = 60;
}