namespace Atlas.App.Presentation;

/// <summary>
/// Synchronisation de l'URL sur la tête WebAssembly (doc 14 §4) : le segment de hash reflète la
/// destination courante (URL partageable / deep-link). No-op sur les têtes natives. Le suivi du bouton
/// Précédent du navigateur (hashchange → navigation) est une étape suivante.
/// </summary>
internal static class WasmRouting
{
    /// <summary>Vrai sur la tête WebAssembly (active la synchro d'URL). <c>static readonly</c> (pas
    /// <c>const</c>) pour éviter un CS0162 « code mort » côté natif sur les branches gardées.</summary>
#if __WASM__
    public static readonly bool IsBrowser = true;
#else
    public static readonly bool IsBrowser = false;
#endif

#if __WASM__
    /// <summary>Lit le tag de destination depuis <c>window.location.hash</c> (« #/recherche » →
    /// « recherche » ; « #/verifier-email?token=… » → « verifier-email », la query est ignorée).</summary>
    public static string? GetTag()
    {
        string hash = RawHash();
        // Le segment de chemin s'arrête au début de la query (« ? »).
        int query = hash.IndexOf('?');
        string path = query >= 0 ? hash[..query] : hash;
        string tag = path.TrimStart('#').TrimStart('/').Trim();
        return string.IsNullOrEmpty(tag) ? null : tag;
    }

    /// <summary>Lit un paramètre de la query du hash (« #/verifier-email?userId=x&amp;token=y »). Null si absent.</summary>
    public static string? GetQueryValue(string key)
    {
        string hash = RawHash();
        int start = hash.IndexOf('?');
        if (start < 0)
        {
            return null;
        }

        foreach (string pair in hash[(start + 1)..].Split('&', System.StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = pair.IndexOf('=');
            string name = eq >= 0 ? pair[..eq] : pair;
            if (string.Equals(name, key, System.StringComparison.Ordinal))
            {
                string value = eq >= 0 ? pair[(eq + 1)..] : string.Empty;
                return System.Uri.UnescapeDataString(value);
            }
        }

        return null;
    }

    /// <summary>Reflète la destination dans l'URL (sans recharger ; idempotent).</summary>
    public static void SetTag(string tag)
    {
        // tag ∈ {accueil, recherche, …} (alphanumérique) — pas d'injection.
        Uno.Foundation.WebAssemblyRuntime.InvokeJS(
            $"if(window.location.hash.split('?')[0]!=='#/{tag}'){{window.location.hash='#/{tag}';}}");
    }

    private static string RawHash() =>
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.hash") ?? string.Empty;
#else
    public static string? GetTag() => null;

    public static string? GetQueryValue(string key)
    {
        _ = key;
        return null;
    }

    public static void SetTag(string tag)
    {
        _ = tag;
    }
#endif
}
