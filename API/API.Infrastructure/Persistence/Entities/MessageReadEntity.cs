namespace API.Infrastructure.Persistence.Entities;

public sealed class MessageReadEntity
{
    public long MessageId { get; set; }

    public int UserId { get; set; }

    public DateTimeOffset ReadAt { get; set; }
}
