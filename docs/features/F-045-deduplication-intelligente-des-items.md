# F-045 — Déduplication intelligente des items

> **Statut** : ✅ Implémenté (MVP 2, 28 mai 2026). Empreinte **SimHash 64 bits** (FNV-1a) calculée à l'ingestion, regroupement en `FeedItemCluster` par distance de Hamming (seuil configurable), collapse dans la timeline (un seul représentant par cluster avec compteur de sources). PR #24.

**Description** : si la même information est publiée par plusieurs sources (ex. rachat d'une entreprise repris par 5 médias), regrouper ces items en un seul dans la timeline avec indication "5 sources rapportent".

**Valeur user** : réduit la fatigue informationnelle. Le user ne lit pas 5 fois la même chose.

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-044.

**Détails techniques** :
- Algorithmes de similarité : MinHash ou SimHash sur les titres + extrait
- Seuil de similarité configurable (par défaut 80%)
- Stockage en BDD du cluster d'items dédupliqués
