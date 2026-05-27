namespace Atlas.Domain.Veille;

/// <summary>
/// Type d'une source de veille. RSS et Atom sont gérés par le même provider (auto-détection) ;
/// Bodacc et InpiBopi sont réservés pour des sources futures (cf. doc 08 §9.2, F-048).
/// </summary>
public enum FeedSourceType
{
    Rss = 1,
    Atom = 2,
    Bodacc = 3,
    InpiBopi = 4,
}
