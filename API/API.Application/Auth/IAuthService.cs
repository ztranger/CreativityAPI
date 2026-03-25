using API.Domain.Users;

namespace API.Application.Auth;

public interface IAuthService
{
    (User User, string Token) Register(string phone, string displayName, string? username, string? password);

    (User User, string Token) Login(string phone, string password);

    (User User, string Token) Verify(string phone);

    string RefreshAccessToken();
}
