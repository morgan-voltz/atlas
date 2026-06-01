namespace Atlas.App.Presentation;

/// <summary>
/// Synchronisation de l'URL sur la tête WebAssembly (doc 14 §4) : le segment de hash reflète la
/// destination courante (URL partageable / deep-link). No-op sur les têtes natives. Le suivi du bouton
/// Précédent du navigateur (hashchange → navigation) est une étape suivante.
/// </summary>
internal static class WasmRouting
{
#if __WASM__
    /// <summary>Lit le tag de destination depuis <c>window.location.hash</c> (« #/recherche » → « recherche »).</summary>
    public static string? GetTag()
    {
        string hash = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.hash") ?? string.Empty;
        string tag = hash.TrimStart('#').TrimStart('/').Trim();
        return string.IsNullOrEmpty(tag) ? null : tag;
    }

    /// <summary>Reflète la destination dans l'URL (sans recharger ; idempotent).</summary>
    public static void SetTag(string tag)
    {
        // tag ∈ {accueil, recherche, …} (alphanumérique) — pas d'injection.
        Uno.Foundation.WebAssemblyRuntime.InvokeJS(
            $"if(window.location.hash!=='#/{tag}'){{window.location.hash='#/{tag}';}}");
    }
#else
    public static string? GetTag() => null;

    public static void SetTag(string tag)
    {
        _ = tag;
    }
#endif
}
