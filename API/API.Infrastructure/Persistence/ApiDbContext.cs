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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
