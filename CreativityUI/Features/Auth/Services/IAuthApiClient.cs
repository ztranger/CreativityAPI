using API.Contracts.Auth;

namespace CreativityUI.Features.Auth.Services;

public interface IAuthApiClient
{
    Task<AuthRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
