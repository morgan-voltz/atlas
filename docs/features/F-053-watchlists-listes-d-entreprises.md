# F-053 — Watchlists (listes d'entreprises)

**Description** : l'utilisateur regroupe des entreprises en **listes nommées** (« Concurrence », « Portefeuille clients », « Cibles M&A »…). Une entreprise peut appartenir à plusieurs listes. Chaque liste offre une vue dédiée et, en option, **sa propre timeline de veille filtrée**. Un **import en masse de SIREN** (jusqu'à plusieurs centaines) permet de constituer une liste d'un coup. Cette feature couvre le **regroupement** ; les **notes et tags** par entreprise restent de la responsabilité de F-030.

**Valeur user** : signal marché net (Pappers a lancé en 2026 un tableau de bord de suivi de portefeuille, plébiscité par les experts-comptables et les fonds d'investissement). **Angle différenciant Atlas** : une watchlist peut avoir **sa propre timeline de veille** (au-dessus de F-047) — RSS + RNE + BODACC filtrés sur les entreprises de la liste. Pappers ne combine pas listes et veille agrégée ; c'est précisément le trou de marché du projet (ADR-009).

**Complexité** : ★★★ (1–2 semaines) en Modèle A. La couche listes est simple ; le morceau principal est l'**import en masse asynchrone**.

**APIs externes** : aucune nouvelle source. L'import en masse utilise l'API **INPI RNE** existante (résolution des dénominations à partir des SIREN), avec le rate-limiting déjà en place.

**Dépendances** : F-017 (favoris = set surveillé), F-030 (annotations & tags — dépendance à sens unique, voir « Frontière »), patron de F-014 (import asynchrone), F-047 (timeline mixte), ADR-004 (archi hexagonale).

**Détails techniques** :
- **Modèle A — les listes par-dessus les favoris (non-cassant)** : `CompanyFavorite` (F-017) reste la liste plate parcourue par F-019 (refresh quotidien) et F-047 (timeline). On garde ce rôle intact.
- Entités : `Watchlist (Id, UserId, Name, CreatedAt)` privée, cascade FK RGPD ; `WatchlistEntry (WatchlistId, Siren, AddedAt)`, index unique `(WatchlistId, Siren)`. Ajouter une entreprise à une liste ⇒ upsert du `CompanyFavorite` correspondant.
- **Import en masse** : réutilise le patron de F-014 (téléchargement de masse) — validation (Luhn), job Hangfire dédié respectant le rate-limiting INPI, notification (email + push) à la fin avec récap (N ajoutés, M doublons, K invalides).
- **Timeline par liste** : filtre `watchlistId` au-dessus de F-047, sans nouvelle mécanique de veille.
- Endpoints (esquisse) : `POST/GET /watchlists`, `PATCH/DELETE /watchlists/{id}`, `POST/DELETE /watchlists/{id}/entries[/{siren}]`, `POST /watchlists/{id}/import` (job async), `GET /watchlists/{id}/timeline`.

**Frontière avec F-030** (fait foi pour les deux fiches) :
- **F-030 possède** `UserAnnotation` (note privée par entité) **et les tags** (libellés sur la relation utilisateur ↔ entité).
- **F-053 possède** `Watchlist` + `WatchlistEntry` (le regroupement) et l'import en masse.
- **Dépendance à sens unique** : F-053 **consomme** les tags de F-030 pour le filtrage. Une watchlist sans tag fonctionne ; une annotation sans liste fonctionne. **Pas de circularité.**
- **Set surveillé** = `CompanyFavorite` (F-017). Appartenir à une liste ⇒ être favori. F-019/F-047 restent branchés sur les favoris.
- **F-030 peut être livré seul et en premier** (le plus rapide), F-053 se pose ensuite ou en parallèle.

**RGPD** : listes et appartenances = données utilisateur. Cascade FK sur le compte (patron existant), incluses dans `/account/export` (art. 20), supprimées à la suppression du compte (art. 17). Privées par défaut (pas de partage en V1 ; le partage relève de F-035 espace équipe).

**Accessibilité (rappel ADR-008)** : un tag n'est **jamais** identifié par la couleur seule (libellé + couleur optionnelle), règle portée par F-030. Listes et entrées navigables au clavier et annoncées au lecteur d'écran. L'écran d'import est accessible (rapport d'import lisible, pas seulement visuel).

**Modèle économique** : aucun coût par utilisateur → cœur open source (cohérent ADR-006). Possibilité de **quotas en hébergé** (nombre de listes, taille), sur le modèle de la limite de sources de F-043.

**Décisions ouvertes** :
- Modèle A maintenant, Modèle B (favoris = « liste par défaut ») plus tard ? Confirmer qu'on ne généralise pas les favoris en V1.
- Colonnes type CRM (statut, dernier contact, relance, à la Pappers) : hors cœur par défaut (proche de F-037). À acter : on les exclut, ou version légère portée par F-030 ?
- Quotas en hébergé (nombre de listes / taille).
