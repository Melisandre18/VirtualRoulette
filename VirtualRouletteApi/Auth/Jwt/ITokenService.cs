using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Auth.Jwt;

public interface ITokenService
{
    string CreateToken(User user, DateTimeOffset now);
}