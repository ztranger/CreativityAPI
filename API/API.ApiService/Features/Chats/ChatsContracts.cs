namespace API.ApiService.Features.Chats;

public sealed record CreateChatRequest(
    short ChatType,
    string? Title,
    string? Description,
    IReadOnlyCollection<int>? ParticipantUserIds);

public sealed record UpdateChatRequest(
    string? Title,
    string? Description);

public sealed record AddParticipantRequest(
    int UserId,
    short Role = 1);

public sealed record SendMessageRequest(
    string Text,
    long? ReplyToMessageId = null);

public sealed record ChatSummaryResponse(
    long Id,
    short ChatType,
    string? Title,
    string? Description,
    int CreatedBy,
    long? LastMessageId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ChatParticipantResponse(
    int UserId,
    short Role,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt);

public sealed record ChatDetailsResponse(
    ChatSummaryResponse Chat,
    IReadOnlyCollection<ChatParticipantResponse> Participants);

public sealed record ChatsListResponse(
    IReadOnlyCollection<ChatSummaryResponse> Chats);

public sealed record MessageResponse(
    long Id,
    long ChatId,
    int SenderUserId,
    string Text,
    DateTimeOffset CreatedAt,
    long? ReplyToMessageId);

public sealed record MessagesListResponse(
    IReadOnlyCollection<MessageResponse> Messages);

public sealed record ErrorResponse(
    string Code,
    string Message);
