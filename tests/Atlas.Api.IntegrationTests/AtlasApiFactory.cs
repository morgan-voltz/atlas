using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using WireMock.Server;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Host de test de l'API : PostgreSQL réel (Testcontainers) + INPI simulé (WireMock), avec un
/// <see cref="IEmailSender"/> capturant. Nécessite un daemon Docker.
/// </summary>
public sealed class AtlasApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public WireMockServer Inpi { get; } = WireMockServer.Start();

    public CapturingEmailSender Emails { get; } = new();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        using IServiceScope scope = Services.CreateScope();
        AtlasDbContext context = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Inpi.Stop();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AtlasDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<AtlasDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));

            services.Configure<InpiOptions>(options => options.RneBaseUrl = Inpi.Url + "/");

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
        });
    }
}
