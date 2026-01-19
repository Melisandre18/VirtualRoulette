using Microsoft.AspNetCore.Authentication;
using VirtualRouletteApi.Auth;

namespace VirtualRouletteApi.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AuthOptions>(config.GetSection("Auth"));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = BasicAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = BasicAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                BasicAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorization();
        return services;
    }
}
