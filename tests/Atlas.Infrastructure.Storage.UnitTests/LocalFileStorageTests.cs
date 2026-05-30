using Atlas.Domain.Storage;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Storage.UnitTests;

/// <summary>
/// Régression F-014 : <c>appsettings.json</c> portait <c>"Storage:Local:RootPath": null</c>, ce qui
/// écrasait le défaut C# et faisait planter le stockage (<see cref="System.ArgumentNullException"/>
/// dans <c>Directory.CreateDirectory(null)</c>) à chaque requête de téléchargement d'archive et dans
/// le job de fond. Le stockage doit retomber sur le répertoire par défaut quand le chemin est vide.
/// </summary>
public sealed class LocalFileStorageTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolves_default_root_when_RootPath_is_blank(string? configuredRootPath)
    {
        Action resolve = () => BuildStorage(configuredRootPath);

        resolve.Should().NotThrow();
    }

    [Fact]
    public async Task Save_then_read_round_trips_when_RootPath_is_unconfigured()
    {
        IFileStorage storage = BuildStorage(configuredRootPath: null);
        string key = $"atlas-test-{Guid.NewGuid():N}.bin";
        byte[] payload = [1, 2, 3, 4, 5];

        using (var input = new MemoryStream(payload))
        {
            await storage.SaveAsync(key, input, "application/octet-stream");
        }

        using var buffer = new MemoryStream();
        await using (Stream? output = await storage.OpenReadAsync(key))
        {
            output.Should().NotBeNull();
            await output!.CopyToAsync(buffer);
        }

        buffer.ToArray().Should().Equal(payload);

        await storage.DeleteAsync(key);
    }

    private static IFileStorage BuildStorage(string? configuredRootPath)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Local:RootPath"] = configuredRootPath,
            })
            .Build();

        ServiceProvider provider = new ServiceCollection()
            .AddStorageInfrastructure(configuration)
            .BuildServiceProvider();

        return provider.GetRequiredService<IFileStorage>();
    }
}
