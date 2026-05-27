using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atlas.Infrastructure.Persistence;

/// <summary>
/// Factory design-time pour les outils EF Core (<c>dotnet ef migrations</c>), afin de générer les
/// migrations sans démarrer l'API ni disposer d'une base réelle. La chaîne ci-dessous n'est jamais
/// utilisée à l'exécution.
/// </summary>
internal sealed class AtlasDbContextFactory : IDesignTimeDbContextFactory<AtlasDbContext>
{
    public AtlasDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<AtlasDbContext> options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=atlas;Username=atlas;Password=atlas")
            .Options;

        return new AtlasDbContext(options);
    }
}
