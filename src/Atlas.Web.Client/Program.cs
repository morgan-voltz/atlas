using Atlas.Web.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Base de l'API : configurable par environnement (wwwroot/appsettings*.json), défaut = API locale HTTPS.
// Cross-origin (port distinct) mais same-site (localhost / même domaine racine en prod) → le cookie
// refresh SameSite=Strict est bien joint ; CORS de l'API autorise cette origine + les credentials.
string apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7201/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ITokenStore, InMemoryTokenStore>();
builder.Services.AddScoped<IAtlasApiClient>(sp => new AtlasApiClient(
    new HttpClient { BaseAddress = new Uri(apiBaseUrl) },
    sp.GetRequiredService<ITokenStore>()));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AtlasAuthenticationStateProvider>();
builder.Services.AddScoped(sp =>
    (AtlasAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

await builder.Build().RunAsync();
