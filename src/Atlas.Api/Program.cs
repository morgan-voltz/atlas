using Atlas.Api.Dev;
using Atlas.Api.Endpoints;
using Atlas.Api.Favorites;
using Atlas.Api.Security;
using Atlas.Api.Veille;
using Atlas.Application;
using Atlas.Application.Common;
using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Bodacc;
using Atlas.Infrastructure.Inpi;
using Atlas.Infrastructure.Messaging;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Security;
using Atlas.Infrastructure.Security.Jwt;
using Atlas.Infrastructure.Storage;
using Atlas.Infrastructure.Veille;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// F-022 — active la licence community QuestPDF pour la génération PDF.
Atlas.Api.Reports.CompanyReportRenderer.Configure();

// TimeProvider est requis par certains endpoints (F-022). Service standard .NET 10.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);

// Logging structuré Serilog (Lot 2b audit) + masquage proactif des propriétés sensibles.
#if DEBUG
// DevEye (dev-only) : buffer alimenté par un sink Serilog, drainé par /devtools/logs/poll.
var devEyeBuffer = new Atlas.DevEye.DevEyeLogBuffer();
#endif
builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    SerilogConfiguration.Configure(loggerConfiguration, context.Configuration);
#if DEBUG
    loggerConfiguration.WriteTo.Sink(new Atlas.Api.DevEye.DevEyeSerilogSink(devEyeBuffer));
#endif
});

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// CORS strict + rate limiting (Lot 2b audit). Hors Development, exige au moins une origine CORS.
builder.Services.AddApiSecurity(builder.Configuration, builder.Environment);

// Derrière un reverse proxy TLS (préprod ADR-019, domaine unique) : honorer X-Forwarded-Proto/-For
// pour le scheme (UseHttpsRedirection) et l'IP client réelle (rate limiting). Le proxy vit dans le
// réseau Docker maîtrisé, donc on n'impose pas de liste de proxies de confiance. Activé par config.
bool behindProxy = builder.Configuration.GetValue("ForwardedHeaders:Enabled", false);
if (behindProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Couches applicatives et adapters d'infrastructure.
builder.Services.AddApplication();
builder.Services.AddSecurityInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddInpiInfrastructure(builder.Configuration);
builder.Services.AddBodaccInfrastructure(builder.Configuration);
builder.Services.AddVeilleInfrastructure(builder.Configuration);
builder.Services.AddStorageInfrastructure(builder.Configuration);
builder.Services.AddScoped<FeedPollingJob>();
builder.Services.AddScoped<FavoriteRefreshJob>();
builder.Services.AddScoped<BodaccPollingJob>();
// F-014 — job Hangfire à la demande (Enqueue depuis l'endpoint).
builder.Services.AddScoped<Atlas.Api.Downloads.BulkDownloadJob>();

// Jobs en arrière-plan (Hangfire, stockage PostgreSQL). Désactivable via BackgroundJobs:Enabled=false
// (les tests d'intégration le coupent : ils utilisent une autre base que la chaîne de connexion app).
bool backgroundJobsEnabled = builder.Configuration.GetValue("BackgroundJobs:Enabled", true);
if (backgroundJobsEnabled)
{
    string? hangfireConnection = builder.Configuration.GetConnectionString("Atlas");
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(storage => storage.UseNpgsqlConnection(hangfireConnection)));
    builder.Services.AddHangfireServer();

    // Adapter Hangfire de l'enqueueur F-014.
    builder.Services.AddScoped<Atlas.Application.Downloads.IBulkDownloadEnqueuer,
        Atlas.Api.Downloads.HangfireBulkDownloadEnqueuer>();
}
else
{
    // Fallback no-op : l'endpoint POST /downloads/bulk reste fonctionnel (le job est créé en
    // base au statut Pending), mais aucun job n'est exécuté. Utilisé par les tests d'intégration.
    builder.Services.AddSingleton<Atlas.Application.Downloads.IBulkDownloadEnqueuer,
        Atlas.Api.Downloads.LoggingBulkDownloadEnqueuer>();
}

// DEV UNIQUEMENT : capture du token de vérification d'email pour l'automatisation des tests.
// Remplace l'IEmailSender de dev et expose /dev/verification-token. Jamais actif hors Development.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<DevVerificationTokenStore>();
    builder.Services.AddSingleton<IEmailSender, CapturingEmailSender>();
}

// Politique d'authentification (durées de jetons, verrouillage).
AuthSettings authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();
builder.Services.AddSingleton(authSettings);

// Validation des JWT via la clé de signature partagée avec l'émetteur.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ISigningKeyProvider>((options, keyProvider) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keyProvider.Issuer,
            ValidateAudience = true,
            ValidAudience = keyProvider.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keyProvider.SigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

#if DEBUG
// DevEye (dev-only) : sert logs Serilog + commandes du backend sur 127.0.0.1.
// Port 31338 par défaut (31337 = tête client Atlas.App). Surchargeable via DEVEYE_PORT.
var devEyeServer = new Atlas.DevEye.DevEyeServer(
    new Atlas.DevEye.EmptyDevEyeStateProvider(),
    devEyeBuffer,
    new Atlas.DevEye.DevEyeCommandRegistry(),
    Atlas.DevEye.DevEyeServer.PortFromEnv(31338));
devEyeServer.Start();
#endif

// Derrière le reverse proxy : doit être le tout premier middleware (réécrit scheme/IP en amont).
if (behindProxy)
{
    app.UseForwardedHeaders();
}

// Préprod single-VPS (ADR-019) : applique les migrations EF au démarrage si demandé (pas de CI/étape
// de migration séparée). Désactivé par défaut ; activé via Database:MigrateOnStartup=true.
if (builder.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using IServiceScope migrationScope = app.Services.CreateScope();
    AtlasDbContext database = migrationScope.ServiceProvider.GetRequiredService<AtlasDbContext>();
    await database.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevEndpoints();
}
else
{
    // HSTS hors Development (max-age 30 jours par défaut, ajustable via configuration).
    app.UseHsts();
}

app.UseHttpsRedirection();

// En-têtes de sécurité (Lot 2b) tôt dans le pipeline pour s'appliquer aussi aux réponses d'erreur.
app.UseSecurityHeaders();

// Logging structuré des requêtes HTTP (status, durée, route).
app.UseSerilogRequestLogging();

app.UseCors(CorsOptions.DefaultPolicyName);
app.UseAuthentication();
app.UseAuthorization();

// Rate limiter après authentification pour partitionner par sub quand le user est connu.
app.UseRateLimiter();

app.MapGet("/", () => Results.Ok(new { name = "Atlas API", status = "ok" }))
    .WithName("Root");

app.MapAuthEndpoints();
app.MapInpiEndpoints();
app.MapCompaniesEndpoints();
app.MapTrademarksEndpoints();
app.MapPatentsEndpoints();
app.MapSearchHistoryEndpoints();
app.MapAccountEndpoints();
app.MapAccessibilityEndpoints();
app.MapFavoritesEndpoints();
app.MapDevicesEndpoints();
app.MapFeedEndpoints();
app.MapVeillePackEndpoints();
app.MapDownloadsEndpoints();

if (backgroundJobsEnabled)
{
    using (IServiceScope scope = app.Services.CreateScope())
    {
        // Amorce les sources de veille puis les VeillePacks (idempotents) ; planifie ensuite le polling récurrent.
        await FeedSourceSeeder.SeedAsync(scope.ServiceProvider);
        await VeillePackSeeder.SeedAsync(scope.ServiceProvider);
        IRecurringJobManager recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobs.AddOrUpdate<FeedPollingJob>("feed-polling", job => job.PollAsync(), "*/30 * * * *");
        // Alerte favoris (F-019) : tous les jours à 03:00 UTC.
        recurringJobs.AddOrUpdate<FavoriteRefreshJob>("favorite-refresh", job => job.RunAsync(), "0 3 * * *");
        // Polling BODACC (F-048) : tous les jours à 04:00 UTC, après le refresh RNE.
        recurringJobs.AddOrUpdate<BodaccPollingJob>("bodacc-polling", job => job.RunAsync(), "0 4 * * *");
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapHangfireDashboard();
    }
}

app.Run();

// Rend la classe Program générée accessible aux tests d'intégration (WebApplicationFactory<Program>).
public partial class Program;
