using API.Domain.Users;

namespace API.Application.Auth;

public interface IUserTokenService
{
    string GenerateToken(User user);
}
