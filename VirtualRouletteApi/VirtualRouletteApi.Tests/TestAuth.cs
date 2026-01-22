using System.Net.Http.Headers;
using System.Text;

namespace VirtualRouletteApi.Tests;

public static class TestAuth
{
    public static void SetBasic(HttpClient client, string user, string pass)
    {
        var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
    }

    public static void SetBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}