using API.Domain.Users;

namespace API.Application.Users;

public sealed class UsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public User GetCurrentUser() => _usersRepository.GetCurrentUser();

    public void UpdateCurrentUser(string? displayName, string? username, string? bio, string? avatar)
    {
        var user = _usersRepository.GetCurrentUser();
        var updatedUser = user with
        {
            DisplayName = displayName ?? user.DisplayName,
            Username = username ?? user.Username,
            Bio = bio ?? user.Bio,
            Avatar = avatar ?? user.Avatar
        };

        _usersRepository.Update(updatedUser);
    }

    public User? GetOtherUser(int id) => _usersRepository.GetById(id);

    public IReadOnlyCollection<User> SearchUsers(string query, int? limit)
    {
        var max = Math.Clamp(limit ?? 20, 1, 100);
        return _usersRepository.Search(query, max);
    }
}
