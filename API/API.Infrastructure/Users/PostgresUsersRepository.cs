using API.Application.Users;
using API.Application.Common.Exceptions;
using API.Domain.Users;
using API.Infrastructure.Persistence;
using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Infrastructure.Users;

public sealed class PostgresUsersRepository : IUsersRepository
{
    private readonly ApiDbContext _dbContext;

    public PostgresUsersRepository(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User GetCurrentUser()
    {
        var user = _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefault();

        return user is null
            ? throw new InvalidOperationException("No users found in storage.")
            : ToDomain(user);
    }

    public User? GetById(int id)
    {
        var user = _dbContext.Users
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);

        return user is null ? null : ToDomain(user);
    }

    public User? GetByPhone(string phone)
    {
        var user = _dbContext.Users
            .AsNoTracking()
            .FirstOrDefault(x => x.Phone == phone);

        return user is null ? null : ToDomain(user);
    }

    public User Add(User user)
    {
        var entity = ToEntity(user, includeId: user.Id > 0);
        _dbContext.Users.Add(entity);
        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException ex) when (TryMapUniqueConstraint(ex, out var mapped))
        {
            throw mapped;
        }
        return ToDomain(entity);
    }

    public void Update(User user)
    {
        var existing = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
        if (existing is null)
        {
            return;
        }

        existing.Phone = user.Phone;
        existing.Username = user.Username;
        existing.DisplayName = user.DisplayName;
        existing.AvatarUrl = user.Avatar;
        existing.Bio = user.Bio;
        existing.Settings = user.Settings;
        existing.LastSeenAt = user.LastSeen;

        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException ex) when (TryMapUniqueConstraint(ex, out var mapped))
        {
            throw mapped;
        }
    }

    public IReadOnlyCollection<User> Search(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var pattern = $"%{query.Trim()}%";

        return _dbContext.Users
            .AsNoTracking()
            .Where(x =>
                (x.Username != null && EF.Functions.ILike(x.Username, pattern)) ||
                EF.Functions.ILike(x.DisplayName, pattern))
            .OrderBy(x => x.Id)
            .Take(limit)
            .Select(ToDomain)
            .ToList();
    }

    private static User ToDomain(UserEntity entity) =>
        new(
            Id: entity.Id,
            Phone: entity.Phone,
            Username: entity.Username,
            DisplayName: entity.DisplayName,
            Avatar: entity.AvatarUrl,
            Bio: entity.Bio,
            Settings: entity.Settings,
            LastSeen: entity.LastSeenAt);

    private static UserEntity ToEntity(User user, bool includeId)
    {
        var entity = new UserEntity
        {
            Phone = user.Phone,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.Avatar,
            Bio = user.Bio,
            Settings = user.Settings,
            LastSeenAt = user.LastSeen
        };

        if (includeId)
        {
            entity.Id = user.Id;
        }

        return entity;
    }

    private static bool TryMapUniqueConstraint(
        DbUpdateException exception,
        out UserUniqueConstraintViolationException mapped)
    {
        mapped = null!;

        if (exception.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        // 23505 = unique_violation
        if (!string.Equals(postgresException.SqlState, PostgresErrorCodes.UniqueViolation, StringComparison.Ordinal))
        {
            return false;
        }

        var field = postgresException.ConstraintName switch
        {
            "IX_users_phone" => "phone",
            "IX_users_username" => "username",
            _ => null
        };

        if (field is null)
        {
            return false;
        }

        mapped = new UserUniqueConstraintViolationException(field);
        return true;
    }
}
