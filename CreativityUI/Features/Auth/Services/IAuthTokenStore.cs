namespace CreativityUI.Features.Auth.Services;

public interface IAuthTokenStore
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task SavePhoneAsync(string phone);
    Task<string?> GetPhoneAsync();
    Task ClearTokenAsync();
}
