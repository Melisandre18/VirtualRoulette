using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VirtualRouletteApi.Auth;
using VirtualRouletteApi.Auth.Jwt;

namespace VirtualRouletteApi.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
    {
        
        services.Configure<AuthOptions>(config.GetSection("Auth"));
        services.Configure<JwtOptions>(config.GetSection("Auth:Jwt"));
        var jwt = config.GetSection("Auth:Jwt").Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(options =>
            {
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
                    
                    var mode = config["Auth:Mode"] ?? "Basic";
                    return string.Equals(mode, "Jwt", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : BasicAuthenticationHandler.SchemeName;
                };
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                BasicAuthenticationHandler.SchemeName, _ => { })
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
