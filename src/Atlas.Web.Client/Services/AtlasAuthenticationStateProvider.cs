using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Atlas.Web.Client.Services;

/// <summary>
/// Source d'état d'authentification pour Blazor (<c>AuthorizeView</c>, <c>AuthorizeRouteView</c>).
/// Dérive l'identité du seul access token en mémoire (<see cref="ITokenStore"/>) : présent et non expiré
/// ⇒ authentifié, claims tirés du JWT. Les pages appellent <see cref="NotifyUserAuthentication"/> après un
/// login réussi et <see cref="NotifyUserLogout"/> à la déconnexion pour rafraîchir l'arbre de composants.
/// </summary>
internal sealed class AtlasAuthenticationStateProvider(ITokenStore tokenStore, TimeProvider timeProvider)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = tokenStore.GetAccessToken();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(Anonymous);
        }

        IReadOnlyList<Claim> claims = JwtPayloadReader.ReadClaims(token);
        if (claims.Count == 0 || !JwtPayloadReader.IsUnexpired(claims, timeProvider.GetUtcNow()))
        {
            return Task.FromResult(Anonymous);
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "email", roleType: "role");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyUserAuthentication() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void NotifyUserLogout()
    {
        tokenStore.Clear();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }
}
