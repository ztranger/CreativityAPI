using API.Application.Chats;

namespace API.Infrastructure.Chats;

public sealed class InMemoryMessagesRepository : IMessagesRepository
{
    private readonly InMemoryMessagingStore _store;

    public InMemoryMessagesRepository(InMemoryMessagingStore store)
    {
        _store = store;
    }

    public MessageItem AddTextMessage(
        long chatId,
        int senderUserId,
        string text,
        long? replyToMessageId,
        DateTimeOffset createdAt)
    {
        lock (_store.SyncRoot)
        {
            var message = new InMemoryMessage
            {
                Id = _store.NextMessageId++,
                ChatId = chatId,
                SenderUserId = senderUserId,
                Text = text,
                ReplyToMessageId = replyToMessageId,
                CreatedAt = createdAt
            };

            _store.Messages.Add(message);
            return ToModel(message);
        }
    }

    public IReadOnlyCollection<MessageItem> GetMessages(long chatId, int limit, long? beforeMessageId)
    {
        lock (_store.SyncRoot)
        {
            var messages = _store.Messages
                .Where(x => x.ChatId == chatId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id);

            if (beforeMessageId.HasValue)
            {
                var beforeMessage = _store.Messages.FirstOrDefault(x => x.Id == beforeMessageId.Value && x.ChatId == chatId);
                if (beforeMessage is not null)
                {
                    messages = messages.Where(x => x.CreatedAt < beforeMessage.CreatedAt)
                        .OrderByDescending(x => x.CreatedAt)
                        .ThenByDescending(x => x.Id);
                }
            }

            return messages
                .Take(limit)
                .Reverse()
                .Select(ToModel)
                .ToArray();
        }
    }

    private static MessageItem ToModel(InMemoryMessage message) =>
        new(
            Id: message.Id,
            ChatId: message.ChatId,
            SenderUserId: message.SenderUserId,
            Text: message.Text,
            CreatedAt: message.CreatedAt,
            ReplyToMessageId: message.ReplyToMessageId);
}
