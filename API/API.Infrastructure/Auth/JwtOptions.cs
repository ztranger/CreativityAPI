namespace API.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Key { get; init; } = "dev_super_secret_key_123!dev_super_secret_key_123!";

    public string Issuer { get; init; } = "CreativityApi";

    public string Audience { get; init; } = "CreativityApiClient";
}
