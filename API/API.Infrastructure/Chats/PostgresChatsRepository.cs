using API.Application.Chats;
using API.Infrastructure.Persistence;
using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Chats;

public sealed class PostgresChatsRepository : IChatsRepository
{
    private readonly ApiDbContext _dbContext;

    public PostgresChatsRepository(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ChatDetails CreateChat(
        short chatType,
        string? title,
        string? description,
        int createdBy,
        IReadOnlyCollection<ChatParticipant> participants)
    {
        var now = DateTimeOffset.UtcNow;
        var chatEntity = new ChatEntity
        {
            ChatType = chatType,
            Title = title,
            Description = description,
            CreatedBy = createdBy,
            IsArchived = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Chats.Add(chatEntity);
        _dbContext.SaveChanges();

        var memberEntities = participants
            .Select(p => new ChatMemberEntity
            {
                ChatId = chatEntity.Id,
                UserId = p.UserId,
                Role = p.Role,
                JoinedAt = p.JoinedAt,
                LeftAt = null
            })
            .ToArray();

        _dbContext.ChatMembers.AddRange(memberEntities);
        _dbContext.SaveChanges();

        var summary = ToSummary(chatEntity);
        var memberModels = memberEntities
            .Select(ToParticipant)
            .ToArray();
        return new ChatDetails(summary, memberModels);
    }

    public IReadOnlyCollection<ChatSummary> GetChatsForUser(int userId)
    {
        return (
            from member in _dbContext.ChatMembers.AsNoTracking()
            join chat in _dbContext.Chats.AsNoTracking() on member.ChatId equals chat.Id
            where member.UserId == userId && member.LeftAt == null
            orderby chat.UpdatedAt descending
            select ToSummary(chat))
            .ToList();
    }

    public ChatSummary? GetChatById(long chatId)
    {
        var chat = _dbContext.Chats
            .AsNoTracking()
            .FirstOrDefault(c => c.Id == chatId);

        return chat is null ? null : ToSummary(chat);
    }

    public IReadOnlyCollection<ChatParticipant> GetParticipants(long chatId)
    {
        return _dbContext.ChatMembers
            .AsNoTracking()
            .Where(x => x.ChatId == chatId && x.LeftAt == null)
            .OrderBy(x => x.JoinedAt)
            .Select(ToParticipant)
            .ToList();
    }

    public bool IsActiveMember(long chatId, int userId)
    {
        return _dbContext.ChatMembers
            .AsNoTracking()
            .Any(x => x.ChatId == chatId && x.UserId == userId && x.LeftAt == null);
    }

    public IReadOnlyCollection<int> GetMissingUserIds(IReadOnlyCollection<int> userIds)
    {
        var uniqueIds = userIds.Distinct().ToArray();
        var existingIds = _dbContext.Users
            .AsNoTracking()
            .Where(u => uniqueIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToHashSet();

        return uniqueIds.Where(x => !existingIds.Contains(x)).ToArray();
    }

    public bool UpdateChatMetadata(long chatId, string? title, string? description, DateTimeOffset updatedAt)
    {
        var chat = _dbContext.Chats.FirstOrDefault(x => x.Id == chatId);
        if (chat is null)
        {
            return false;
        }

        chat.Title = title;
        chat.Description = description;
        chat.UpdatedAt = updatedAt;
        _dbContext.SaveChanges();
        return true;
    }

    public AddParticipantStatus AddParticipant(long chatId, int userId, short role, DateTimeOffset joinedAt)
    {
        var chatExists = _dbContext.Chats.Any(x => x.Id == chatId);
        if (!chatExists)
        {
            return AddParticipantStatus.ChatNotFound;
        }

        var existing = _dbContext.ChatMembers.FirstOrDefault(x => x.ChatId == chatId && x.UserId == userId);
        if (existing is not null)
        {
            if (existing.LeftAt is null)
            {
                return AddParticipantStatus.AlreadyMember;
            }

            existing.LeftAt = null;
            existing.JoinedAt = joinedAt;
            existing.Role = role;
            _dbContext.SaveChanges();
            return AddParticipantStatus.Added;
        }

        _dbContext.ChatMembers.Add(new ChatMemberEntity
        {
            ChatId = chatId,
            UserId = userId,
            Role = role,
            JoinedAt = joinedAt,
            LeftAt = null
        });

        _dbContext.SaveChanges();
        return AddParticipantStatus.Added;
    }

    public RemoveParticipantStatus RemoveParticipant(long chatId, int userId, DateTimeOffset leftAt)
    {
        var chatExists = _dbContext.Chats.Any(x => x.Id == chatId);
        if (!chatExists)
        {
            return RemoveParticipantStatus.ChatNotFound;
        }

        var member = _dbContext.ChatMembers
            .FirstOrDefault(x => x.ChatId == chatId && x.UserId == userId && x.LeftAt == null);

        if (member is null)
        {
            return RemoveParticipantStatus.ParticipantNotFound;
        }

        member.LeftAt = leftAt;
        _dbContext.SaveChanges();
        return RemoveParticipantStatus.Removed;
    }

    public void UpdateLastMessage(long chatId, long messageId, DateTimeOffset updatedAt)
    {
        var chat = _dbContext.Chats.FirstOrDefault(x => x.Id == chatId);
        if (chat is null)
        {
            return;
        }

        chat.LastMessageId = messageId;
        chat.UpdatedAt = updatedAt;
        _dbContext.SaveChanges();
    }

    private static ChatSummary ToSummary(ChatEntity entity) =>
        new(
            Id: entity.Id,
            ChatType: entity.ChatType,
            Title: entity.Title,
            Description: entity.Description,
            CreatedBy: entity.CreatedBy,
            LastMessageId: entity.LastMessageId,
            IsArchived: entity.IsArchived,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);

    private static ChatParticipant ToParticipant(ChatMemberEntity entity) =>
        new(
            ChatId: entity.ChatId,
            UserId: entity.UserId,
            Role: entity.Role,
            JoinedAt: entity.JoinedAt,
            LeftAt: entity.LeftAt);
}
