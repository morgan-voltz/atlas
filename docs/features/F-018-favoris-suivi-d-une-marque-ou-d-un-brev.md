# F-018 — Favoris : suivi d'une marque ou d'un brevet

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Entités `TrademarkFavorite` (UserId, DepositNumber, NameSnapshot?, AddedAt) et `PatentFavorite` (UserId, PublicationNumber, TitleSnapshot?, AddedAt) avec ports repositories + erreurs métier. 6 use cases Application (Add/Remove/GetMine × 2). 6 endpoints `POST/DELETE/GET /favorites/trademarks` et `/favorites/patents` sous le même groupe `/favorites/*`. Persistence : tables `trademark_favorites` et `patent_favorites` (index unique `(user, dépôt/publication)`, cascade FK RGPD). Tests handlers (10) + cascade RGPD étendue. PR #41.

**Description** : équivalent F-017 pour la PI.

**Valeur user** : suivi d'un portefeuille IP.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-006, F-015.
