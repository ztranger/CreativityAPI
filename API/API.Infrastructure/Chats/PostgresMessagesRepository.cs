using API.Application.Chats;
using API.Infrastructure.Persistence;
using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Chats;

public sealed class PostgresMessagesRepository : IMessagesRepository
{
    private readonly ApiDbContext _dbContext;

    public PostgresMessagesRepository(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public MessageItem AddTextMessage(
        long chatId,
        int senderUserId,
        string text,
        long? replyToMessageId,
        DateTimeOffset createdAt)
    {
        var message = new MessageEntity
        {
            ChatId = chatId,
            SenderUserId = senderUserId,
            MessageType = 1,
            ReplyToMessageId = replyToMessageId,
            CreatedAt = createdAt,
            Version = 1
        };

        _dbContext.Messages.Add(message);
        _dbContext.SaveChanges();

        var content = new MessageContentEntity
        {
            MessageId = message.Id,
            Text = text,
            Entities = "[]",
            EmojiPayload = "[]"
        };

        _dbContext.MessageContents.Add(content);
        _dbContext.SaveChanges();

        return new MessageItem(
            Id: message.Id,
            ChatId: message.ChatId,
            SenderUserId: message.SenderUserId,
            Text: content.Text ?? string.Empty,
            CreatedAt: message.CreatedAt,
            ReplyToMessageId: message.ReplyToMessageId);
    }

    public IReadOnlyCollection<MessageItem> GetMessages(long chatId, int limit, long? beforeMessageId)
    {
        var query =
            from message in _dbContext.Messages.AsNoTracking()
            join content in _dbContext.MessageContents.AsNoTracking() on message.Id equals content.MessageId
            where message.ChatId == chatId && message.DeletedAt == null && message.MessageType == 1
            select new
            {
                Message = message,
                Content = content
            };

        if (beforeMessageId.HasValue)
        {
            var beforeReference = _dbContext.Messages
                .AsNoTracking()
                .Where(x => x.Id == beforeMessageId.Value && x.ChatId == chatId)
                .Select(x => x.CreatedAt)
                .FirstOrDefault();

            if (beforeReference != default)
            {
                query = query.Where(x => x.Message.CreatedAt < beforeReference);
            }
        }

        return query
            .OrderByDescending(x => x.Message.CreatedAt)
            .ThenByDescending(x => x.Message.Id)
            .Take(limit)
            .AsEnumerable()
            .Reverse()
            .Select(x => new MessageItem(
                Id: x.Message.Id,
                ChatId: x.Message.ChatId,
                SenderUserId: x.Message.SenderUserId,
                Text: x.Content.Text ?? string.Empty,
                CreatedAt: x.Message.CreatedAt,
                ReplyToMessageId: x.Message.ReplyToMessageId))
            .ToArray();
    }
}
