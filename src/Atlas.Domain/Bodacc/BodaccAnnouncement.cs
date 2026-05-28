namespace Atlas.Domain.Bodacc;

/// <summary>
/// Annonce publiée au BODACC (Bulletin Officiel des Annonces Civiles et Commerciales) pour une
/// entreprise donnée. Source : data.gouv.fr / Opendatasoft. Couvre les créations, modifications,
/// procédures collectives, ventes de fonds, radiations, etc.
/// </summary>
/// <param name="AnnouncementId">Identifiant unique de l'annonce dans BODACC (stocké comme <c>ExternalId</c> dans <c>FavoriteEvent</c>).</param>
/// <param name="PublishedAt">Date de parution officielle.</param>
/// <param name="TypeLabel">Libellé du type d'annonce (« Création », « Procédure collective », « Vente », …).</param>
/// <param name="Court">Tribunal (greffe) concerné, si présent.</param>
/// <param name="Excerpt">Résumé textuel de l'annonce, déjà tronqué.</param>
public sealed record BodaccAnnouncement(
    string AnnouncementId,
    DateTimeOffset PublishedAt,
    string TypeLabel,
    string? Court,
    string Excerpt);
