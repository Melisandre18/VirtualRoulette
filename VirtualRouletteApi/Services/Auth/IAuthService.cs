using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Auth;

public interface IAuthService
{
    Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest req, CancellationToken ct);
    Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct);
}