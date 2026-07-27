using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Neshin.Infrastructure.Persistence;

public sealed class NeshinWriteDbContextFactory : IDesignTimeDbContextFactory<NeshinWriteDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=passwoord";

    public NeshinWriteDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("NESHIN_WRITE_CONNECTION_STRING") ??
            Environment.GetEnvironmentVariable("ConnectionStrings__Write") ??
            DefaultConnectionString;

        var options = new DbContextOptionsBuilder<NeshinWriteDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(NeshinWriteDbContext).Assembly.FullName))
            .Options;

        return new NeshinWriteDbContext(options);
    }
}

public sealed class NeshinReadDbContextFactory : IDesignTimeDbContextFactory<NeshinReadDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=passwoord";

    public NeshinReadDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("NESHIN_READ_CONNECTION_STRING") ??
            Environment.GetEnvironmentVariable("ConnectionStrings__Read") ??
            DefaultConnectionString;

        var options = new DbContextOptionsBuilder<NeshinReadDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NeshinReadDbContext(options);
    }
}
