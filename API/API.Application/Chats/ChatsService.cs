namespace API.Application.Chats;

public sealed class ChatsService
{
    private readonly IChatsRepository _chatsRepository;
    private readonly IMessagesRepository _messagesRepository;

    public ChatsService(
        IChatsRepository chatsRepository,
        IMessagesRepository messagesRepository)
    {
        _chatsRepository = chatsRepository;
        _messagesRepository = messagesRepository;
    }

    public ServiceResult<ChatDetails> CreateChat(int currentUserId, CreateChatCommand command)
    {
        if (command.ChatType is < 1 or > 3)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.BadRequest, "chat_type must be between 1 and 3.");
        }

        if (command.ParticipantUserIds.Any(x => x <= 0))
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.BadRequest, "participant_user_ids contains invalid user id.");
        }

        var participantIds = command.ParticipantUserIds
            .Append(currentUserId)
            .Distinct()
            .ToArray();

        var missingUsers = _chatsRepository.GetMissingUserIds(participantIds);
        if (missingUsers.Count > 0)
        {
            return ServiceResult<ChatDetails>.Fail(
                ServiceErrorCode.BadRequest,
                $"users not found: {string.Join(", ", missingUsers)}");
        }

        var participants = participantIds
            .Select(id => new ChatParticipant(
                ChatId: 0,
                UserId: id,
                Role: id == currentUserId ? (short)2 : (short)1,
                JoinedAt: DateTimeOffset.UtcNow,
                LeftAt: null))
            .ToArray();

        var chat = _chatsRepository.CreateChat(
            command.ChatType,
            command.Title?.Trim(),
            command.Description?.Trim(),
            currentUserId,
            participants);

        return ServiceResult<ChatDetails>.Success(chat);
    }

    public IReadOnlyCollection<ChatSummary> GetChats(int currentUserId) =>
        _chatsRepository.GetChatsForUser(currentUserId);

    public ServiceResult<ChatDetails> GetChat(long chatId, int currentUserId)
    {
        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var chat = _chatsRepository.GetChatById(chatId);
        if (chat is null)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.NotFound, "Chat not found.");
        }

        var participants = _chatsRepository.GetParticipants(chatId);
        return ServiceResult<ChatDetails>.Success(new ChatDetails(chat, participants));
    }

    public ServiceResult<ChatDetails> UpdateChat(long chatId, int currentUserId, UpdateChatCommand command)
    {
        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var updated = _chatsRepository.UpdateChatMetadata(
            chatId,
            command.Title?.Trim(),
            command.Description?.Trim(),
            DateTimeOffset.UtcNow);

        if (!updated)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.NotFound, "Chat not found.");
        }

        return GetChat(chatId, currentUserId);
    }

    public ServiceResult<ChatDetails> AddParticipant(long chatId, int currentUserId, AddParticipantCommand command)
    {
        if (command.UserId <= 0)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.BadRequest, "user_id must be positive.");
        }

        if (command.Role is < 1 or > 3)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.BadRequest, "role must be between 1 and 3.");
        }

        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var missing = _chatsRepository.GetMissingUserIds([command.UserId]);
        if (missing.Count > 0)
        {
            return ServiceResult<ChatDetails>.Fail(ServiceErrorCode.BadRequest, "Requested user was not found.");
        }

        var status = _chatsRepository.AddParticipant(chatId, command.UserId, command.Role, DateTimeOffset.UtcNow);
        return status switch
        {
            AddParticipantStatus.ChatNotFound => ServiceResult<ChatDetails>.Fail(ServiceErrorCode.NotFound, "Chat not found."),
            AddParticipantStatus.AlreadyMember => ServiceResult<ChatDetails>.Fail(ServiceErrorCode.Conflict, "User is already a participant."),
            _ => GetChat(chatId, currentUserId)
        };
    }

    public ServiceResult<bool> RemoveParticipant(long chatId, int currentUserId, int participantUserId)
    {
        if (participantUserId <= 0)
        {
            return ServiceResult<bool>.Fail(ServiceErrorCode.BadRequest, "participant user id must be positive.");
        }

        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<bool>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var status = _chatsRepository.RemoveParticipant(chatId, participantUserId, DateTimeOffset.UtcNow);
        return status switch
        {
            RemoveParticipantStatus.ChatNotFound => ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Chat not found."),
            RemoveParticipantStatus.ParticipantNotFound => ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Participant not found."),
            _ => ServiceResult<bool>.Success(true)
        };
    }

    public ServiceResult<MessageItem> SendTextMessage(long chatId, int currentUserId, SendMessageCommand command)
    {
        var text = command.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return ServiceResult<MessageItem>.Fail(ServiceErrorCode.BadRequest, "text must not be empty.");
        }

        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<MessageItem>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var chat = _chatsRepository.GetChatById(chatId);
        if (chat is null)
        {
            return ServiceResult<MessageItem>.Fail(ServiceErrorCode.NotFound, "Chat not found.");
        }

        var message = _messagesRepository.AddTextMessage(
            chatId,
            currentUserId,
            text,
            command.ReplyToMessageId,
            DateTimeOffset.UtcNow);

        _chatsRepository.UpdateLastMessage(chatId, message.Id, message.CreatedAt);
        return ServiceResult<MessageItem>.Success(message);
    }

    public ServiceResult<IReadOnlyCollection<MessageItem>> GetMessages(
        long chatId,
        int currentUserId,
        int? limit,
        long? beforeMessageId)
    {
        if (!_chatsRepository.IsActiveMember(chatId, currentUserId))
        {
            return ServiceResult<IReadOnlyCollection<MessageItem>>.Fail(ServiceErrorCode.Forbidden, "Current user is not a chat member.");
        }

        var chat = _chatsRepository.GetChatById(chatId);
        if (chat is null)
        {
            return ServiceResult<IReadOnlyCollection<MessageItem>>.Fail(ServiceErrorCode.NotFound, "Chat not found.");
        }

        var normalizedLimit = Math.Clamp(limit ?? 50, 1, 200);
        var messages = _messagesRepository.GetMessages(chatId, normalizedLimit, beforeMessageId);
        return ServiceResult<IReadOnlyCollection<MessageItem>>.Success(messages);
    }
}
