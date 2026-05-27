namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Image / logo d'une marque récupérée auprès de l'INPI (contenu binaire + type MIME).
/// </summary>
public sealed record TrademarkImage(byte[] Content, string ContentType);
