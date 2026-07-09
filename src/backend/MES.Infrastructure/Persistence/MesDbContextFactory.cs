using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MES.Infrastructure.Persistence;

public sealed class MesDbContextFactory : IDesignTimeDbContextFactory<MesDbContext>
{
    public MesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MES_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=mes_thinking_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<MesDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new MesDbContext(optionsBuilder.Options);
    }
}
