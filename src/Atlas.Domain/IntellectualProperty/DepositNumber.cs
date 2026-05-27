namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Numéro de dépôt d'un titre de propriété industrielle (marque, brevet, dessin &amp; modèle).
/// </summary>
public readonly record struct DepositNumber(string Value)
{
    public override string ToString() => Value;
}
