using API.Application.Users;
using API.Application.Common.Exceptions;
using API.Domain.Users;

namespace API.Application.Auth;

public sealed class AuthService : IAuthService
{
    public const string LegacyDefaultPassword = "    ";

    private readonly IUsersRepository _usersRepository;
    private readonly IUserTokenService _userTokenService;
    private readonly IPasswordHashService _passwordHashService;

    public AuthService(
        IUsersRepository usersRepository,
        IUserTokenService userTokenService,
        IPasswordHashService passwordHashService)
    {
        _usersRepository = usersRepository;
        _userTokenService = userTokenService;
        _passwordHashService = passwordHashService;
    }

    public (User User, string Token) Register(string phone, string displayName, string? username, string? password)
    {
        var normalizedUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();
        var normalizedPassword = string.IsNullOrWhiteSpace(password) ? LegacyDefaultPassword : password;
        var passwordHash = _passwordHashService.HashPassword(normalizedPassword);

        var userToCreate = new User(
            Id: 0,
            Phone: phone,
            Username: normalizedUsername,
            DisplayName: displayName,
            Avatar: "https://testingbot.com/free-online-tools/random-avatar/128",
            Bio: null,
            Settings: new UserSettings(Notifications: true, Theme: "dark"),
            LastSeen: DateTimeOffset.UtcNow,
            PasswordHash: passwordHash
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

    public (User User, string Token) Login(string phone, string password)
    {
        var user = _usersRepository.GetByPhone(phone);
        if (user is null || !_passwordHashService.VerifyPassword(password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
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
