using System.Reflection;

namespace Atlas.Api.Endpoints;

/// <summary>
/// Identifiant de l'utilisateur authentifié, lié automatiquement depuis le claim <c>sub</c> du JWT
/// (audit Lot 4b — F3). Remplace la répétition de <c>principal.TryGetUserId(...)</c> dans chaque endpoint :
/// un paramètre <see cref="CurrentUser"/> suffit. À n'utiliser que sur des endpoints derrière
/// <c>RequireAuthorization()</c> (l'authentification garantit alors la présence du claim).
/// </summary>
internal sealed class CurrentUser(Guid id) : IBindableFromHttpContext<CurrentUser>
{
    public Guid Id { get; } = id;

    public static ValueTask<CurrentUser?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Réutilise la logique d'extraction (claim sub → Guid). Derrière RequireAuthorization, le claim
        // est toujours présent ; le cas null (non authentifié) est intercepté en amont par le middleware.
        CurrentUser? result = context.User.TryGetUserId(out Guid userId) ? new CurrentUser(userId) : null;
        return ValueTask.FromResult(result);
    }
}
