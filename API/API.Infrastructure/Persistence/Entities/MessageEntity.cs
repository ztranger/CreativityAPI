namespace API.Infrastructure.Persistence.Entities;

public sealed class MessageEntity
{
    public long Id { get; set; }

    public long ChatId { get; set; }

    public int SenderUserId { get; set; }

    public short MessageType { get; set; }

    public long? ReplyToMessageId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? EditedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public int Version { get; set; }
}
