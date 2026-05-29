namespace Atlas.Domain.Veille;

/// <summary>
/// Visibilité d'un <see cref="VeillePack"/> (F-049 marketplace) :
/// <list type="bullet">
/// <item><c>System</c> — pack pré-curé par l'équipe Atlas, immuable côté utilisateur, toujours actif au catalogue.</item>
/// <item><c>Private</c> — pack créé par un utilisateur, visible uniquement par lui (brouillon).</item>
/// <item><c>Public</c> — pack publié au marketplace communautaire, listable et adoptable par n'importe quel utilisateur.</item>
/// </list>
/// </summary>
public enum VeillePackVisibility
{
    System = 0,
    Private = 1,
    Public = 2,
}
