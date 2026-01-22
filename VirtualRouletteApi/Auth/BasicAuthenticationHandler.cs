using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Auth;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserStore users,
    IPasswordHasher<User> passwordHasher,
    IOptions<AuthOptions> authOptions
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Basic";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            return AuthenticateResult.NoResult();

        if (!AuthenticationHeaderValue.TryParse(authHeaderValues.ToString(), out var headerValue))
            return AuthenticateResult.Fail("Invalid Authorization header.");

        if (!string.Equals(headerValue.Scheme, SchemeName, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        if (string.IsNullOrWhiteSpace(headerValue.Parameter))
            return AuthenticateResult.Fail("Missing credentials.");

        string username;
        string password;

        try
        {
            var credentialBytes = Convert.FromBase64String(headerValue.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes);
            var parts = credentials.Split(':', 2);
            if (parts.Length != 2) return AuthenticateResult.Fail("Invalid credentials format.");

            username = parts[0];
            password = parts[1];
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Base64 credentials.");
        }

        var ct = Context.RequestAborted;

        var user = await users.FindByUserNameAsync(username, ct);
        if (user is null) return AuthenticateResult.Fail("Invalid username or password.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return AuthenticateResult.Fail("Invalid username or password.");
        
        var now = DateTimeOffset.UtcNow;

        if (!user.IsActive)
            return AuthenticateResult.Fail("User inactive");

        if (now - user.LastSeen > TimeSpan.FromMinutes(5))
        {
            user.IsActive = false;
            await users.SaveAsync(ct);
            return AuthenticateResult.Fail("Session expired");
        }
        
        user.LastSeen = now;
        await users.SaveAsync(ct);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var realm = authOptions.Value.Basic.Realm ?? "API";
        Response.Headers.WWWAuthenticate = $@"Basic realm=""{realm}""";
        return base.HandleChallengeAsync(properties);
    }
}
