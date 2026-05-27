using Atlas.Shared.Result;

namespace Atlas.Domain.Inpi;

/// <summary>
/// Authentifie auprès des API INPI (RNE : <c>POST /sso/login</c>) et fournit la session (JWT).
/// Sert aussi de test de connectivité lors de la connexion d'un compte INPI (F-003).
/// </summary>
public interface IInpiAuthenticationProvider
{
    Task<Result<InpiSession>> AuthenticateAsync(string username, string password, CancellationToken ct = default);
}
