using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace VirtualRouletteApi.Tests;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureHostConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "InMemory",

                ["Auth:Mode"] = "Basic",

                ["Auth:Jwt:Issuer"] = "VirtualRouletteApi",
                ["Auth:Jwt:Audience"] = "VirtualRouletteApiClient",
                ["Auth:Jwt:SigningKey"] = "str",
                ["Auth:Jwt:ExpiresMinutes"] = "60"
            });
        });

        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "InMemory",

                ["Auth:Mode"] = "Basic",

                ["Auth:Jwt:Issuer"] = "VirtualRouletteApi",
                ["Auth:Jwt:Audience"] = "VirtualRouletteApiClient",
                ["Auth:Jwt:SigningKey"] = "str",
                ["Auth:Jwt:ExpiresMinutes"] = "60"
            });
        });

        return base.CreateHost(builder);
    }
}
