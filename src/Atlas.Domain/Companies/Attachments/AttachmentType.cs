namespace Atlas.Domain.Companies.Attachments;

/// <summary>
/// Catégorie d'un document déposé au RNE (F-013). Les libellés correspondent aux deux familles
/// principales documentées par l'INPI ; tout autre type tombe en <see cref="Other"/>.
/// </summary>
public enum AttachmentType
{
    /// <summary>Catégorie inconnue ou non mappée.</summary>
    Other = 0,

    /// <summary>Actes juridiques (statuts, modifications, dissolutions, …).</summary>
    Acte = 1,

    /// <summary>Comptes annuels / bilan (déclaration de confidentialité possible).</summary>
    Bilan = 2,
}
