namespace CreativityUI.Features.Auth.Services;

public sealed class SecureAuthTokenStore : IAuthTokenStore
{
    private const string AccessTokenKey = "auth_access_token";
    private const string PhoneKey = "auth_phone";

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(AccessTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SavePhoneAsync(string phone)
    {
        await SecureStorage.Default.SetAsync(PhoneKey, phone);
    }

    public async Task<string?> GetPhoneAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(PhoneKey);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        return Task.CompletedTask;
    }
}
