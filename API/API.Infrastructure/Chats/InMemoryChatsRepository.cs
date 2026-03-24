using API.Application.Chats;
using API.Application.Users;

namespace API.Infrastructure.Chats;

public sealed class InMemoryChatsRepository : IChatsRepository
{
    private readonly InMemoryMessagingStore _store;
    private readonly IUsersRepository _usersRepository;

    public InMemoryChatsRepository(InMemoryMessagingStore store, IUsersRepository usersRepository)
    {
        _store = store;
        _usersRepository = usersRepository;
    }

    public ChatDetails CreateChat(
        short chatType,
        string? title,
        string? description,
        int createdBy,
        IReadOnlyCollection<ChatParticipant> participants)
    {
        lock (_store.SyncRoot)
        {
            var now = DateTimeOffset.UtcNow;
            var chat = new InMemoryChat
            {
                Id = _store.NextChatId++,
                ChatType = chatType,
                Title = title,
                Description = description,
                CreatedBy = createdBy,
                CreatedAt = now,
                UpdatedAt = now,
                IsArchived = false
            };

            _store.Chats.Add(chat);

            var members = participants
                .Select(p => new InMemoryChatMember
                {
                    ChatId = chat.Id,
                    UserId = p.UserId,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt,
                    LeftAt = null
                })
                .ToArray();

            _store.ChatMembers.AddRange(members);

            return new ChatDetails(
                ToSummary(chat),
                members.Select(ToParticipant).ToArray());
        }
    }

    public IReadOnlyCollection<ChatSummary> GetChatsForUser(int userId)
    {
        lock (_store.SyncRoot)
        {
            var chatIds = _store.ChatMembers
                .Where(x => x.UserId == userId && x.LeftAt == null)
                .Select(x => x.ChatId)
                .ToHashSet();

            return _store.Chats
                .Where(x => chatIds.Contains(x.Id))
                .OrderByDescending(x => x.UpdatedAt)
                .Select(ToSummary)
                .ToArray();
        }
    }

    public ChatSummary? GetChatById(long chatId)
    {
        lock (_store.SyncRoot)
        {
            var chat = _store.Chats.FirstOrDefault(x => x.Id == chatId);
            return chat is null ? null : ToSummary(chat);
        }
    }

    public IReadOnlyCollection<ChatParticipant> GetParticipants(long chatId)
    {
        lock (_store.SyncRoot)
        {
            return _store.ChatMembers
                .Where(x => x.ChatId == chatId && x.LeftAt == null)
                .OrderBy(x => x.JoinedAt)
                .Select(ToParticipant)
                .ToArray();
        }
    }

    public bool IsActiveMember(long chatId, int userId)
    {
        lock (_store.SyncRoot)
        {
            return _store.ChatMembers.Any(x => x.ChatId == chatId && x.UserId == userId && x.LeftAt == null);
        }
    }

    public IReadOnlyCollection<int> GetMissingUserIds(IReadOnlyCollection<int> userIds)
    {
        var uniqueIds = userIds.Distinct().ToArray();
        return uniqueIds
            .Where(id => _usersRepository.GetById(id) is null)
            .ToArray();
    }

    public bool UpdateChatMetadata(long chatId, string? title, string? description, DateTimeOffset updatedAt)
    {
        lock (_store.SyncRoot)
        {
            var chat = _store.Chats.FirstOrDefault(x => x.Id == chatId);
            if (chat is null)
            {
                return false;
            }

            chat.Title = title;
            chat.Description = description;
            chat.UpdatedAt = updatedAt;
            return true;
        }
    }

    public AddParticipantStatus AddParticipant(long chatId, int userId, short role, DateTimeOffset joinedAt)
    {
        lock (_store.SyncRoot)
        {
            var chatExists = _store.Chats.Any(x => x.Id == chatId);
            if (!chatExists)
            {
                return AddParticipantStatus.ChatNotFound;
            }

            var existing = _store.ChatMembers.FirstOrDefault(x => x.ChatId == chatId && x.UserId == userId);
            if (existing is not null)
            {
                if (existing.LeftAt is null)
                {
                    return AddParticipantStatus.AlreadyMember;
                }

                existing.LeftAt = null;
                existing.Role = role;
                existing.JoinedAt = joinedAt;
                return AddParticipantStatus.Added;
            }

            _store.ChatMembers.Add(new InMemoryChatMember
            {
                ChatId = chatId,
                UserId = userId,
                Role = role,
                JoinedAt = joinedAt,
                LeftAt = null
            });

            return AddParticipantStatus.Added;
        }
    }

    public RemoveParticipantStatus RemoveParticipant(long chatId, int userId, DateTimeOffset leftAt)
    {
        lock (_store.SyncRoot)
        {
            var chatExists = _store.Chats.Any(x => x.Id == chatId);
            if (!chatExists)
            {
                return RemoveParticipantStatus.ChatNotFound;
            }

            var existing = _store.ChatMembers
                .FirstOrDefault(x => x.ChatId == chatId && x.UserId == userId && x.LeftAt == null);

            if (existing is null)
            {
                return RemoveParticipantStatus.ParticipantNotFound;
            }

            existing.LeftAt = leftAt;
            return RemoveParticipantStatus.Removed;
        }
    }

    public void UpdateLastMessage(long chatId, long messageId, DateTimeOffset updatedAt)
    {
        lock (_store.SyncRoot)
        {
            var chat = _store.Chats.FirstOrDefault(x => x.Id == chatId);
            if (chat is null)
            {
                return;
            }

            chat.LastMessageId = messageId;
            chat.UpdatedAt = updatedAt;
        }
    }

    private static ChatSummary ToSummary(InMemoryChat chat) =>
        new(
            Id: chat.Id,
            ChatType: chat.ChatType,
            Title: chat.Title,
            Description: chat.Description,
            CreatedBy: chat.CreatedBy,
            LastMessageId: chat.LastMessageId,
            IsArchived: chat.IsArchived,
            CreatedAt: chat.CreatedAt,
            UpdatedAt: chat.UpdatedAt);

    private static ChatParticipant ToParticipant(InMemoryChatMember member) =>
        new(
            ChatId: member.ChatId,
            UserId: member.UserId,
            Role: member.Role,
            JoinedAt: member.JoinedAt,
            LeftAt: member.LeftAt);
}
