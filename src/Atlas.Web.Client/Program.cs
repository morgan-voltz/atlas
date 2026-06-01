using Atlas.Web.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Base de l'API : configurable par environnement (wwwroot/appsettings*.json), défaut = API locale HTTPS.
// - Dev : base **absolue** cross-origin (https://localhost:7201/) ; same-site → cookie refresh joint, CORS dev OK.
// - Préprod (ADR-019) : base **relative** same-origin "/api/" (reverse proxy domaine unique) → ni CORS, ni domaine figé.
string apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7201/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

// Base absolue → telle quelle ; base relative → résolue contre l'origine de l'app.
Uri apiBase = Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out Uri? absolute)
    ? absolute
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), apiBaseUrl);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ITokenStore, InMemoryTokenStore>();
builder.Services.AddScoped<IAtlasApiClient>(sp => new AtlasApiClient(
    new HttpClient { BaseAddress = apiBase },
    sp.GetRequiredService<ITokenStore>()));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AtlasAuthenticationStateProvider>();
builder.Services.AddScoped(sp =>
    (AtlasAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

await builder.Build().RunAsync();
