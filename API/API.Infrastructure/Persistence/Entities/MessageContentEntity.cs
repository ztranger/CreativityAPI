namespace API.Infrastructure.Persistence.Entities;

public sealed class MessageContentEntity
{
    public long MessageId { get; set; }

    public string? Text { get; set; }

    public string Entities { get; set; } = "[]";

    public string EmojiPayload { get; set; } = "[]";
}
