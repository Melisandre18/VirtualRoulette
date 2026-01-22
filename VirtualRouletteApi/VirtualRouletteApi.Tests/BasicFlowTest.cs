using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace VirtualRouletteApi.Tests;

public class BasicFlowTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public BasicFlowTests(TestAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent JsonBody(object body)
        => new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    [Fact]
    public async Task Balance_Requires_Auth()
    {
        var res = await _client.GetAsync("/api/balance");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_Login_Basic_Deposit_Get_Balance()
    {
        var username = "Tamriko";
        var password = "secret123";
        
        var reg = await _client.PostAsync("/api/auth/register", JsonBody(new { userName = username, password }));
        reg.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);
        
        var login = await _client.PostAsync("/api/auth/login", JsonBody(new { userName = username, password }));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        
        TestAuth.SetBasic(_client, username, password);
        
        var dep = await _client.PostAsync("/api/balance/deposit", JsonBody(new { amount = 500 }));
        dep.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var bal = await _client.GetAsync("/api/balance");
        bal.StatusCode.Should().Be(HttpStatusCode.OK);

        var balText = await bal.Content.ReadAsStringAsync();
        balText.Should().Contain("500");
    }

    [Fact]
    public async Task Deposit_Zero_400()
    {
        var username = "Tamriko";
        var password = "secret123";

        await _client.PostAsync("/api/auth/register", JsonBody(new { userName = username, password }));
        TestAuth.SetBasic(_client, username, password);

		var res = await _client.PostAsync("/api/balance/deposit", JsonBody(new { amount = 0 }));
		res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var text = await res.Content.ReadAsStringAsync();
		text.Should().Contain("Amount must be positive");
    }

    [Fact]
    public async Task Withdraw_TooMuch()
    {
        var username = "Tamriko";
        var password = "secret123";

        await _client.PostAsync("/api/auth/register", JsonBody(new { userName = username, password }));
        TestAuth.SetBasic(_client, username, password);

        var res = await _client.PostAsync("/api/balance/withdraw", JsonBody(new { amount = 1 }));
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
