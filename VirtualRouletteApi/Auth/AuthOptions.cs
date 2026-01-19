namespace VirtualRouletteApi.Auth;

public class AuthOptions
{
    public string Mode { get; set; } = "Basic";
    public BasicOptions Basic { get; set; } = new();
}

public class BasicOptions
{
    public string Realm { get; set; } = "VirtualRouletteApi";
}
