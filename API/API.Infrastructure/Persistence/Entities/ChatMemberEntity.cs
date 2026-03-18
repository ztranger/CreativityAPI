namespace API.Infrastructure.Persistence.Entities;

public sealed class ChatMemberEntity
{
    public long ChatId { get; set; }

    public int UserId { get; set; }

    public short Role { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public DateTimeOffset? LeftAt { get; set; }

    public DateTimeOffset? MuteUntil { get; set; }

    public long? LastReadMessageId { get; set; }

    public bool IsPinned { get; set; }
}
