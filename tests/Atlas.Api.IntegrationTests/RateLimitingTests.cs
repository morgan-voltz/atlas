using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Persistence;
using FluentAssertions;
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
/// Vérifie le rate limiting strict (Lot 2b) sur les endpoints d'authentification :
/// une 4ème requête en moins de 60 s renvoie 429 quand PermitLimit=3.
/// </summary>
public sealed class RateLimitingTests : IAsyncLifetime, IDisposable
{
    private readonly StrictRateLimitFactory _factory = new();

    Task IAsyncLifetime.InitializeAsync() => ((IAsyncLifetime)_factory).InitializeAsync();

    Task IAsyncLifetime.DisposeAsync() => ((IAsyncLifetime)_factory).DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Login_endpoint_returns_429_after_PermitLimit_requests()
    {
        HttpClient client = _factory.CreateClient();
        var payload = new { Email = "rate-limit@example.com", Password = "irrelevant" };

        // 3 premières requêtes : pas limitées (échec auth mais pas 429).
        for (int i = 0; i < 3; i++)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/auth/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        // 4ème : doit être limitée.
        HttpResponseMessage limited = await client.PostAsJsonAsync("/auth/login", payload);
        limited.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    /// <summary>
    /// Variante d'<see cref="AtlasApiFactory"/> avec un PermitLimit AuthStrict de 3
    /// pour pouvoir tester la limitation dans un délai raisonnable.
    /// </summary>
    private sealed class StrictRateLimitFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        private readonly WireMockServer _inpi = WireMockServer.Start();

        async Task IAsyncLifetime.InitializeAsync()
        {
            await _postgres.StartAsync();
            using IServiceScope scope = Services.CreateScope();
            AtlasDbContext context = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
            await context.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            _inpi.Stop();
            await _postgres.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("BackgroundJobs:Enabled", "false");
            builder.UseSetting("RateLimit:AuthStrict:PermitLimit", "3");
            builder.UseSetting("RateLimit:AuthStrict:WindowSeconds", "60");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<AtlasDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.AddDbContext<AtlasDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
                services.Configure<InpiOptions>(options => options.RneBaseUrl = _inpi.Url + "/");
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(new CapturingEmailSender());
            });
        }
    }
}
