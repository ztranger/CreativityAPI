using API.Domain.Users;

namespace API.Application.Users;

public interface IUsersRepository
{
    User GetCurrentUser();

    User? GetById(int id);

    User? GetByPhone(string phone);

    int GetNextId();

    void Add(User user);

    void Update(User user);

    IReadOnlyCollection<User> Search(string query, int limit);
}
