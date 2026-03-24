namespace API.Application.Chats;

public enum ServiceErrorCode
{
    None = 0,
    BadRequest = 1,
    Forbidden = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record ServiceResult<T>(
    T? Value,
    ServiceErrorCode ErrorCode,
    string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode == ServiceErrorCode.None;

    public static ServiceResult<T> Success(T value) => new(value, ServiceErrorCode.None, null);

    public static ServiceResult<T> Fail(ServiceErrorCode code, string message) => new(default, code, message);
}

public sealed record ChatSummary(
    long Id,
    short ChatType,
    string? Title,
    string? Description,
    int CreatedBy,
    long? LastMessageId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ChatParticipant(
    long ChatId,
    int UserId,
    short Role,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt);

public sealed record ChatDetails(
    ChatSummary Chat,
    IReadOnlyCollection<ChatParticipant> Participants);

public sealed record MessageItem(
    long Id,
    long ChatId,
    int SenderUserId,
    string Text,
    DateTimeOffset CreatedAt,
    long? ReplyToMessageId);

public sealed record CreateChatCommand(
    short ChatType,
    string? Title,
    string? Description,
    IReadOnlyCollection<int> ParticipantUserIds);

public sealed record UpdateChatCommand(
    string? Title,
    string? Description);

public sealed record AddParticipantCommand(
    int UserId,
    short Role);

public sealed record SendMessageCommand(
    string Text,
    long? ReplyToMessageId);
