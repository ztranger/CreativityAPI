namespace API.Application.Chats;

public enum AddParticipantStatus
{
    Added = 0,
    ChatNotFound = 1,
    AlreadyMember = 2
}

public enum RemoveParticipantStatus
{
    Removed = 0,
    ChatNotFound = 1,
    ParticipantNotFound = 2
}

public interface IChatsRepository
{
    ChatDetails CreateChat(
        short chatType,
        string? title,
        string? description,
        int createdBy,
        IReadOnlyCollection<ChatParticipant> participants);

    IReadOnlyCollection<ChatSummary> GetChatsForUser(int userId);

    ChatSummary? GetChatById(long chatId);

    IReadOnlyCollection<ChatParticipant> GetParticipants(long chatId);

    bool IsActiveMember(long chatId, int userId);

    IReadOnlyCollection<int> GetMissingUserIds(IReadOnlyCollection<int> userIds);

    bool UpdateChatMetadata(long chatId, string? title, string? description, DateTimeOffset updatedAt);

    AddParticipantStatus AddParticipant(long chatId, int userId, short role, DateTimeOffset joinedAt);

    RemoveParticipantStatus RemoveParticipant(long chatId, int userId, DateTimeOffset leftAt);

    void UpdateLastMessage(long chatId, long messageId, DateTimeOffset updatedAt);
}
