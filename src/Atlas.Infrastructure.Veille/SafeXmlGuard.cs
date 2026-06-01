using System.Xml;

namespace Atlas.Infrastructure.Veille;

/// <summary>
/// Durcit le parsing XML des flux RSS/Atom contre les attaques XXE et « billion laughs » (audit Lot 1).
/// Les flux proviennent d'URL fournies par l'utilisateur (F-043) : on ne se repose jamais sur les
/// réglages par défaut de la bibliothèque de parsing pour désactiver les DTD. <see cref="EnsureSafe"/>
/// parcourt le document avec un lecteur explicitement verrouillé et lève <see cref="XmlException"/>
/// dès qu'une DOCTYPE/DTD apparaît ou si le document est mal formé.
/// </summary>
internal static class SafeXmlGuard
{
    private static readonly XmlReaderSettings HardenedSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit, // toute DOCTYPE => XmlException (bloque XXE et entités récursives)
        XmlResolver = null,                      // aucune résolution d'entité/ressource externe
        MaxCharactersFromEntities = 0,
        CloseInput = true,
    };

    /// <summary>
    /// Lève <see cref="XmlException"/> si le payload contient une DTD ou n'est pas un XML bien formé.
    /// </summary>
    public static void EnsureSafe(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        using var stream = new MemoryStream(payload, writable: false);
        using var reader = XmlReader.Create(stream, HardenedSettings);
        while (reader.Read())
        {
            // Parcours complet : DtdProcessing.Prohibit fait lever XmlException sur toute DOCTYPE.
        }
    }
}
