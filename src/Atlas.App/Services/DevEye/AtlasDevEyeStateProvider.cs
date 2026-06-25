#if DEBUG
using Atlas.DevEye;

namespace Atlas.App.Services.DevEye;

/// <summary>
/// Expose a DevEye un instantane de l'etat observable du client (dev-only) :
/// page courante (type du contenu du frame racine) et presence de la fenetre.
/// Forme generique (dictionnaire JSON) — aucun concept metier.
/// </summary>
internal sealed class AtlasDevEyeStateProvider : IDevEyeStateProvider
{
    public IReadOnlyDictionary<string, object?> GetCurrentState()
        => new Dictionary<string, object?>
        {
            ["page"] = App.RootFrame?.Content?.GetType().Name,
            ["window_active"] = App.WindowInstance is not null,
        };
}
#endif
