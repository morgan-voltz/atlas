# F-049 — Marketplace des templates partagés (V2 light, posée en MVP 2)

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). `VeillePack` étendu avec `AuthorUserId : UserId?` + `Visibility` enum (`System` / `Private` / `Public`) + compteur dénormalisé `LikesCount`. Nouvelle factory `CreateUserPack` (initialement `Private`). Méthodes `Publish` / `Unpublish` / `IncrementLikes` / `DecrementLikes` (avec plancher à 0). 2 nouvelles entités : `VeillePackLike` (index unique `(UserId, VeillePackId)`, cascade FK RGPD) et `VeillePackReport` (Pending / ReviewedNoAction / ReviewedRemoved, raison ≤ 500). Ports `IVeillePackLikeRepository`, `IVeillePackReportRepository` + extensions `GetPublicMarketplaceAsync` (paginé, trié `LikesCount desc, CreatedAt desc`) et `GetByAuthorAsync` sur `IVeillePackRepository`. 8 use cases MediatR (CreateUserVeillePack, Publish, Unpublish, Like, Unlike, Report, ListPublicMarketplace, GetMyAuthoredPacks). 8 endpoints sous `/veille/packs/*` (POST `/user`, PATCH `/user/{code}/publish|unpublish`, POST/DELETE `/{code}/like`, POST `/{code}/report`, GET `/community`, GET `/mine/authored`). Migration `AddVeillePackMarketplace` (3 colonnes sur `veille_pack` avec normalisation `System` des packs existants + 2 nouvelles tables, cascades FK RGPD). Modèle *report &amp; review* (modération a posteriori, pas de pré-modération). 21 tests (11 domaine + 10 handlers : Create / Like / Publish). **Reste** : UI MAUI (CRUD pack user + browse community + bouton like / report), workflow admin de revue des reports (endpoint et UI), recommandations / search facetté.

**Description** : les utilisateurs peuvent partager leurs templates de veille curés. D'autres users peuvent les adopter en un clic.

**Valeur user** : effet réseau, capitalisation collective, croissance organique. Les pros aiment partager leur expertise.

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-042.

**Détails techniques** :
- Templates "publics" exposés sur une page dédiée
- Système de votes / favoris communautaires
- Modération : modèle "report & review", pas de pré-modération sauf signal
- Avantage stratégique : crée du contenu généré par les users
