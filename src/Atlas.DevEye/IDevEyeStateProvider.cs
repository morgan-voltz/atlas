namespace Atlas.DevEye;

/// <summary>
/// Contrat que l'application hote implemente pour exposer son etat observable
/// courant a DevEye (page active, entite selectionnee, indicateurs...). Ne
/// depend d'aucun concept metier Atlas : les valeurs sont serialisees en JSON
/// generique par <c>GET /devtools/state</c> sous la forme
/// <c>{ "entries": { ... } }</c>.
/// </summary>
public interface IDevEyeStateProvider
{
    IReadOnlyDictionary<string, object?> GetCurrentState();
}

/// <summary>Etat vide par defaut (hote sans provider specifique).</summary>
public sealed class EmptyDevEyeStateProvider : IDevEyeStateProvider
{
    public IReadOnlyDictionary<string, object?> GetCurrentState()
        => new Dictionary<string, object?>();
}
