using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VirtualRouletteApi.Auth;
using VirtualRouletteApi.Auth.Jwt;

namespace VirtualRouletteApi.Extensions;

/// <summary>
/// Authentication/authorization registration for the API
/// Modes: Basic, Jwt
/// Authorization is enabled globally and endpoints opt-in using [Authorize]
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers application authentication.
    /// With Basic auth the browser might show a username/password dialog when a 401 is returned
    /// AuthenticationHandler updates LastSeen and rejects expired sessions
    /// </summary>
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
    {
        // Bind authentication-related options from configuration
        services.Configure<AuthOptions>(config.GetSection("Auth"));
        services.Configure<JwtOptions>(config.GetSection("Auth:Jwt"));
        var jwt = config.GetSection("Auth:Jwt").Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(options =>
            {
                // "Smart" is the single entry point for authentication
                // It will delegate to Basic or JWT depending on the request
                options.DefaultScheme = "Smart";
                options.DefaultAuthenticateScheme = "Smart";
                options.DefaultChallengeScheme = "Smart";
            })
            .AddPolicyScheme("Smart", "Smart", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var auth = context.Request.Headers.Authorization.ToString();

                    if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return JwtBearerDefaults.AuthenticationScheme;

                    if (auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                        return BasicAuthenticationHandler.SchemeName;
                    
                    // If no explicit Authorization header:
                    // fall back to configured mode (Basic or Jwt).
                    var mode = config["Auth:Mode"] ?? "Basic";
                    return string.Equals(mode, "Jwt", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : BasicAuthenticationHandler.SchemeName;
                };
            })
            // Basic authentication handler:
            // username/password verification
            // inactivity timeout via IsActive + LastSeen
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                BasicAuthenticationHandler.SchemeName, _ => { })
            
            // JWT Bearer authentication handler:
            // token validation, expiration, signature verification
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(10)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
