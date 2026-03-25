using API.Contracts.Auth;

namespace CreativityUI.Features.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IAuthTokenStore _authTokenStore;

    public AuthService(IAuthApiClient authApiClient, IAuthTokenStore authTokenStore)
    {
        _authApiClient = authApiClient;
        _authTokenStore = authTokenStore;
    }

    public async Task<AuthRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authApiClient.RegisterAsync(request, cancellationToken);
        await _authTokenStore.SaveTokenAsync(response.Token);
        await _authTokenStore.SavePhoneAsync(request.Phone);
        return response;
    }
}
