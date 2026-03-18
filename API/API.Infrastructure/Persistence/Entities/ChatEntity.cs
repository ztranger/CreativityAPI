namespace API.Infrastructure.Persistence.Entities;

public sealed class ChatEntity
{
    public long Id { get; set; }

    public short ChatType { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public int CreatedBy { get; set; }

    public long? LastMessageId { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
