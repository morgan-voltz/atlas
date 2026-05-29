using Atlas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Démarre un PostgreSQL réel (Testcontainers) et applique les migrations une fois pour toute la collection.
/// Nécessite un daemon Docker en cours d'exécution.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using AtlasDbContext context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public AtlasDbContext CreateContext()
    {
        DbContextOptions<AtlasDbContext> options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new AtlasDbContext(options);
    }

    public string GetConnectionString() => _container.GetConnectionString();
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
