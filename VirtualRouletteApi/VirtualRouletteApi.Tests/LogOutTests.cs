using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace VirtualRouletteApi.Tests;

public class LogoutTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public LogoutTests(TestAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent JsonBody(object body)
        => new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    [Fact]
    public async Task Logout_Makes_User_Unauthorized()
    {
        var username = "Tamriko";
        var password = "secret123";

        await _client.PostAsync("/api/auth/register", JsonBody(new { userName = username, password }));
        TestAuth.SetBasic(_client, username, password);

        (await _client.GetAsync("/api/balance")).StatusCode.Should().Be(HttpStatusCode.OK);

        var logout = await _client.PostAsync("/api/auth/logout", content: null);
        logout.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        (await _client.GetAsync("/api/balance")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}