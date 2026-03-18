using API.Application.Users;
using API.Domain.Users;

namespace API.Application.Auth;

public sealed class AuthService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUserTokenService _userTokenService;

    public AuthService(IUsersRepository usersRepository, IUserTokenService userTokenService)
    {
        _usersRepository = usersRepository;
        _userTokenService = userTokenService;
    }

    public (User User, string Token) Register(string phone, string displayName, string? username)
    {
        var normalizedUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();

        var userToCreate = new User(
            Id: 0,
            Phone: phone,
            Username: normalizedUsername,
            DisplayName: displayName,
            Avatar: null,
            Bio: null,
            Settings: new UserSettings(Notifications: true, Theme: "dark"),
            LastSeen: DateTimeOffset.UtcNow
        );

        var savedUser = _usersRepository.Add(userToCreate);
        var user = string.IsNullOrWhiteSpace(savedUser.Username)
            ? savedUser with { Username = $"user{savedUser.Id}" }
            : savedUser;

        if (!string.Equals(user.Username, savedUser.Username, StringComparison.Ordinal))
        {
            _usersRepository.Update(user);
        }

        var token = _userTokenService.GenerateToken(user);
        return (user, token);
    }

    public (User User, string Token) Verify(string phone)
    {
        var user = _usersRepository.GetByPhone(phone) ?? _usersRepository.GetCurrentUser();
        var token = _userTokenService.GenerateToken(user);
        return (user, token);
    }

    public string RefreshAccessToken()
    {
        var user = _usersRepository.GetCurrentUser();
        return _userTokenService.GenerateToken(user);
    }
}
