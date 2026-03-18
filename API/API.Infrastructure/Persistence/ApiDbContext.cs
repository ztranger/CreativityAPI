using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence;

public sealed class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ChatEntity> Chats => Set<ChatEntity>();
    public DbSet<ChatMemberEntity> ChatMembers => Set<ChatMemberEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();
    public DbSet<MessageContentEntity> MessageContents => Set<MessageContentEntity>();
    public DbSet<MessageReadEntity> MessageReads => Set<MessageReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
