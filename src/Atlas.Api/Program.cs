using Atlas.Api.Endpoints;
using Atlas.Application;
using Atlas.Application.Common;
using Atlas.Infrastructure.Inpi;
using Atlas.Infrastructure.Messaging;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Security;
using Atlas.Infrastructure.Security.Jwt;
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
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { name = "Atlas API", status = "ok" }))
    .WithName("Root");

app.MapAuthEndpoints();
app.MapInpiEndpoints();

app.Run();
