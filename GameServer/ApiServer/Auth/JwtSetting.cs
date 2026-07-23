namespace ApiServer.Auth;

public record JwtSetting
{
    public string SecretKey { get; set; } = "dev-only-change-this-secret-key-before-release";

    public string Issuer { get; set; } = "ApiServer";

    public string Audience { get; set; } = "GameServer";

    public int ExpiryMinutes { get; set; } = 60;
}
