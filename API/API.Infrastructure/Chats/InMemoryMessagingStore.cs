namespace API.Infrastructure.Chats;

public sealed class InMemoryMessagingStore
{
    public object SyncRoot { get; } = new();

    public List<InMemoryChat> Chats { get; } = [];

    public List<InMemoryChatMember> ChatMembers { get; } = [];

    public List<InMemoryMessage> Messages { get; } = [];

    public long NextChatId { get; set; } = 1;

    public long NextMessageId { get; set; } = 1;
}

public sealed class InMemoryChat
{
    public long Id { get; set; }
    public short ChatType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int CreatedBy { get; set; }
    public long? LastMessageId { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class InMemoryChatMember
{
    public long ChatId { get; set; }
    public int UserId { get; set; }
    public short Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
}

public sealed class InMemoryMessage
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public int SenderUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public long? ReplyToMessageId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
