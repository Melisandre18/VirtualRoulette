using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace VirtualRouletteApi.Tests;

public class BetAndJackpotTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public BetAndJackpotTests(TestAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent JsonBody(object body)
        => new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    [Fact]
    public async Task MakeBet_WritesHistory_And_ChangesJackpot()
    {
        var username = "Tamriko";
        var password = "secret123";

        await _client.PostAsync("/api/auth/register", JsonBody(new { userName = username, password }));
        TestAuth.SetBasic(_client, username, password);
        
        (await _client.PostAsync("/api/balance/deposit", JsonBody(new { amount = 10_000 })))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        
        var j0 = await _client.GetAsync("/api/jackpot");
        j0.StatusCode.Should().Be(HttpStatusCode.OK);
        var before = await j0.Content.ReadAsStringAsync();
        
        var betJson = "[{\"T\": \"v\", \"I\": 20, \"C\": 1, \"K\": 1}]";
        
        var betRes = await _client.PostAsync("/api/bet", JsonBody(new { bet = betJson }));
        betRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var betText = await betRes.Content.ReadAsStringAsync();
        betText.Should().NotBeNullOrWhiteSpace();
        
        var hist = await _client.GetAsync("/api/bet/history?take=10");
        hist.StatusCode.Should().Be(HttpStatusCode.OK);

        var histText = await hist.Content.ReadAsStringAsync();
        histText.Should().NotBeNullOrWhiteSpace();
        
        var j1 = await _client.GetAsync("/api/jackpot");
        j1.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = await j1.Content.ReadAsStringAsync();

        after.Should().NotBe(before);
    }
}
