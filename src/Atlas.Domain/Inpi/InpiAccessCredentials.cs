namespace Atlas.Domain.Inpi;

/// <summary>
/// Identifiants INPI en clair, utilisés en mémoire le temps d'un appel pour s'authentifier auprès du RNE.
/// Ne JAMAIS persister ni logger (cf. CLAUDE.md, docs/04). Obtenus en déchiffrant <see cref="InpiCredentials"/>.
/// </summary>
public sealed record InpiAccessCredentials(string Username, string Password)
{
    public override string ToString() => "InpiAccessCredentials { Username = ***, Password = *** }";
}
