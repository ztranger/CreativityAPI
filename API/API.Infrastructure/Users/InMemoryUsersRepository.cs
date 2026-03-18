using API.Application.Users;
using API.Domain.Users;

namespace API.Infrastructure.Users;

public sealed class InMemoryUsersRepository : IUsersRepository
{
    private readonly List<User> _users =
    [
        new(
            Id: 123,
            Phone: "+79001234567",
            Username: "johndoe",
            DisplayName: "John Doe",
            Avatar: "https://cdn.example.com/avatars/123.jpg",
            Bio: "Hello there!",
            Settings: new UserSettings(Notifications: true, Theme: "dark"),
            LastSeen: DateTimeOffset.Parse("2024-01-01T12:00:00Z")
        ),
        new(
            Id: 456,
            Phone: "+79009876543",
            Username: "friend",
            DisplayName: "Friend Name",
            Avatar: "https://cdn.example.com/avatars/456.jpg",
            Bio: "Their bio",
            Settings: new UserSettings(Notifications: true, Theme: "light"),
            LastSeen: null
        )
    ];

    public User GetCurrentUser() => _users.First();

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public User? GetByPhone(string phone) =>
        _users.FirstOrDefault(u => string.Equals(u.Phone, phone, StringComparison.Ordinal));

    public int GetNextId() => _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;

    public void Add(User user) => _users.Add(user);

    public void Update(User user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            _users[index] = user;
        }
    }

    public IReadOnlyCollection<User> Search(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return _users
            .Where(u =>
                (!string.IsNullOrEmpty(u.Username) && u.Username.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(u.DisplayName) && u.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Take(limit)
            .ToList();
    }
}
