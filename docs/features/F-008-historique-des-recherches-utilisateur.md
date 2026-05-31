# F-008 — Historique des recherches utilisateur

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Entité `SearchHistoryEntry` (`SearchType`, query, date), table `search_history`, rétention 200/user (prune). Enregistrement découplé via une notification MediatR `SearchPerformedNotification` publiée par les handlers de recherche (F-004/F-005/F-006) sur succès. Endpoint `GET /search-history`. Aucune API externe.

**Description** : chaque utilisateur a un historique de ses recherches (entreprises et marques), accessible depuis son profil.

**Valeur user** : retrouver rapidement une recherche précédente sans la refaire. Pose les bases des features futures (favoris, alertes).

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-001, F-004, F-006.

**Détails techniques** : table simple `SearchHistory` avec `UserId`, `SearchType`, `Query`, `Timestamp`. Limite à 200 entrées par utilisateur pour ne pas exploser la BDD.
