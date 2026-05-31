using Atlas.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// WASM pur (ADR-017) : l'hôte n'est qu'un serveur de shell, il n'est PAS la frontière de sécurité
// (celle-ci = l'API JWT + le routeur client via AuthorizeRouteView). Mais les pages portent
// `[Authorize]`, donc leurs endpoints serveur exposent des métadonnées d'autorisation : sans
// AuthorizationMiddleware, servir la racine `/` (page Accueil [Authorize]) lève une 500. On ajoute
// donc l'autorisation côté hôte ET on marque les endpoints de composants `AllowAnonymous` : le shell
// WASM boote pour tout le monde, puis le client redirige les non-authentifiés vers /connexion.
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Atlas.Web.Client._Imports).Assembly)
    .AllowAnonymous();

app.Run();
