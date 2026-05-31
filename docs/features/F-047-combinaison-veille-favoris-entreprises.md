# F-047 — Combinaison veille + favoris entreprises

> **Statut** : ✅ MVP intégral livré (3 volets, MVP 2, 29 mai 2026). **Volet 1 — tagging RSS → favoris** : à l'ingestion d'un item RSS, scan titre + résumé pour les noms d'entreprises favorites (matching mot entier case-insensitive, ≥ 3 caractères). Persisté dans `FeedItemFavoriteMatch`. Timeline étendue avec `MentionedFavorites` et filtre `?mentionsFavoritesOnly=true`. **Volet 2 — événements RNE** : `FavoriteEvent` (entité + repo) créée par un 3ᵉ handler MediatR sur `CompanyFavoriteChangedNotification` (F-019). DTO timeline discriminé `Kind = "RssItem" | "FavoriteEvent"` avec fusion mémoire bornée. **Volet 3 — BODACC (cf. F-048)** : annonces légales pour les SIREN favoris, dédupliquées via `FavoriteEvent.ExternalId`. La timeline mixte est désormais opérationnelle : RSS + RNE + BODACC, tout au même endroit, trié chronologiquement.

> **Architecture liée** : le volet 1 (tagging RSS → favoris) est la **première implémentation** de `INameInTextMatcher` formalisé par **ADR-014** (matching conservateur unifié — la doctrine ADR-012 incarnée dans `MatchCandidate`). À l'arrivée de F-031 et F-055/F-057, le noyau de normalisation des noms (`ICompanyNameNormalizer`) sera extrait à partir du code d'ici.

**Description** : feature **différenciante phare** : la timeline affiche **aussi** les évolutions des entreprises favorites du user (mises à jour RNE, dépôts BODACC, articles RSS mentionnant le nom de l'entreprise). Tout au même endroit.

**Valeur user** : c'est LE feature qui justifie le projet face à un Feedly ou un Pappers seuls. **L'argument commercial central de la veille.**

**Complexité** : ★★★★

**APIs externes** : INPI RNE, BODACC.

**Dépendances** : F-017 (favoris), F-019 (alertes), F-041, F-044.

**Détails techniques** :
- Polling des favoris en parallèle des sources RSS
- Détection automatique : un item RSS mentionne le nom d'une entreprise favorite → tagging automatique
- Timeline mixte triée chronologiquement
