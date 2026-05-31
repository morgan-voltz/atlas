# F-007 — Vue détaillée d'une marque

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle. `TrademarkDetail` (déposant, dates, statut, type, classes de Nice via `NiceClassification`), `TrademarkImage` ; endpoints `GET /trademarks/{depositNumber}` et `GET /trademarks/{depositNumber}/image` (proxy binaire). Réutilise l'adapter PI de F-006 (`GET /marques/notice/{id}`, `GET /marques/image/{id}`). **Limites** : mêmes réserves best-effort que F-006 (contrat PI supposé, à confronter à l'API réelle). Tests via WireMock (notice + image).

**Description** : depuis un résultat de recherche, l'utilisateur ouvre la fiche détaillée d'une marque (notice complète, classes Nice, image/logo si disponible).

**Valeur user** : la valeur d'une recherche marque est dans le détail (qui possède quoi, depuis quand, pour quels produits/services).

**Complexité** : ★★

**APIs externes** : INPI PI (`GET /marques/notice/{id}`, `GET /marques/image/{id}`).

**Dépendances** : F-006.
