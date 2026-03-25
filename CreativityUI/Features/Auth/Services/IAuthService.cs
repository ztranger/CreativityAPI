using API.Contracts.Auth;

namespace CreativityUI.Features.Auth.Services;

public interface IAuthService
{
    Task<AuthRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
