namespace Atlas.Domain.Veille;

/// <summary>
/// Statut d'instruction d'un signalement (<see cref="VeillePackReport"/>, F-049). Le marketplace fonctionne
/// en *report &amp; review* : un pack est public dès sa publication, l'équipe (ou un admin futur) ne le revoit
/// qu'après signalement. <c>Pending</c> = signalé, en attente d'examen. <c>ReviewedNoAction</c> = examiné,
/// pack laissé en l'état. <c>ReviewedRemoved</c> = examiné, pack retiré du marketplace.
/// </summary>
public enum VeillePackReportStatus
{
    Pending = 0,
    ReviewedNoAction = 1,
    ReviewedRemoved = 2,
}
