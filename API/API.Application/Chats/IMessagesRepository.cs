namespace API.Application.Chats;

public interface IMessagesRepository
{
    MessageItem AddTextMessage(
        long chatId,
        int senderUserId,
        string text,
        long? replyToMessageId,
        DateTimeOffset createdAt);

    IReadOnlyCollection<MessageItem> GetMessages(long chatId, int limit, long? beforeMessageId);
}
