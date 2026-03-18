using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace API.Infrastructure.Persistence;

public sealed class ApiDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApiDbContext>
{
    public ApiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApiDbContext>();
        const string defaultConnectionString =
            "Host=localhost;Port=5432;Database=creativityapi;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(defaultConnectionString);
        return new ApiDbContext(optionsBuilder.Options);
    }
}
