using Atlas.Api.Dev;
using Atlas.Api.Endpoints;
using Atlas.Api.Veille;
using Atlas.Application;
using Atlas.Application.Common;
using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Inpi;
using Atlas.Infrastructure.Messaging;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Security;
using Atlas.Infrastructure.Security.Jwt;
using Atlas.Infrastructure.Veille;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Couches applicatives et adapters d'infrastructure.
builder.Services.AddApplication();
builder.Services.AddSecurityInfrastructure(builder.Configuration);
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddInpiInfrastructure(builder.Configuration);
builder.Services.AddVeilleInfrastructure(builder.Configuration);
builder.Services.AddScoped<FeedPollingJob>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevEndpoints();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { name = "Atlas API", status = "ok" }))
    .WithName("Root");

app.MapAuthEndpoints();
app.MapInpiEndpoints();
app.MapCompaniesEndpoints();
app.MapTrademarksEndpoints();
app.MapSearchHistoryEndpoints();
app.MapAccountEndpoints();
app.MapFeedEndpoints();
app.MapVeillePackEndpoints();

if (backgroundJobsEnabled)
{
    using (IServiceScope scope = app.Services.CreateScope())
    {
        // Amorce les sources de veille puis les VeillePacks (idempotents) ; planifie ensuite le polling récurrent.
        await FeedSourceSeeder.SeedAsync(scope.ServiceProvider);
        await VeillePackSeeder.SeedAsync(scope.ServiceProvider);
        scope.ServiceProvider.GetRequiredService<IRecurringJobManager>()
            .AddOrUpdate<FeedPollingJob>("feed-polling", job => job.PollAsync(), "*/30 * * * *");
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapHangfireDashboard();
    }
}

app.Run();

// Rend la classe Program générée accessible aux tests d'intégration (WebApplicationFactory<Program>).
public partial class Program;
