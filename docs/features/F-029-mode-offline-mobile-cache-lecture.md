# F-029 — Mode offline mobile (cache de lecture)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Offline, 31 mai 2026). **Promue** de stub V3+ à spec complète. Le **cache de lecture** (ce qu'on voit hors-ligne) ; le moteur de fraîcheur/sync est **F-075**. Cadrée par **ADR-027**. **Lecture seule** en v1.

**Description** : sur l'app **MAUI mobile**, consulter **hors connexion** sa liste de favoris/watchlists et le **dernier dossier d'entreprise consulté**, en **lecture seule**, chaque donnée affichée **avec sa date** (« hors-ligne · vu le {date} ») — jamais comme « à jour ».

**Valeur user** : usage en **mobilité** (transports, zones rurales) — retrouver sa watchlist et la fiche qu'on regardait, même sans réseau. La valeur est *consulter*, pas *modifier*.

**Complexité** : ★★★ (store local + rendu honnête ; le morceau dur — sync/fraîcheur — est dans F-075).

**APIs externes** : aucune.

**Dépendances** : **F-009** (MAUI mobile), **F-017** (favoris — contenu caché), **F-056** (dossier 360 — contenu caché), **ADR-027**, ADR-002 (client pur), doc 12 §14 (états).

**Détails techniques** :
- **Store SQLite local** (SQLite-net-pcl, acté en archi) de `CachedResource<T>` (ADR-027) : **liste favoris/watchlists** + **N derniers dossiers consultés** (read-models renvoyés par l'API + `FetchedAt`). **Aucune logique métier, aucun secret** dans le cache (topologie pure, ADR-002).
- **Rendu honnête (ADR-027 §2-3)** :
  - en cache + hors-ligne → **`Stale`** « hors-ligne · vu le {date} » (jamais « À jour ») ;
  - pas en cache + hors-ligne → **`Unavailable`** « indisponible hors-ligne » ;
  - **jamais de vide** (qui laisserait croire « pas de donnée »).
- **Bandeau global** « Mode hors-ligne — données du {date} » = **état système** (doc 12 §14, traitement réseau ; **jamais** une qualification d'entité, ADR-012) ; la **fraîcheur par-item** reste sur chaque section.
- **Lecture seule (ADR-027 §4)** : aucune mutation hors-ligne (l'ajout de favori reste désactivé/différé sans réseau — pas de file d'écriture en v1).
- **Sécurité (ADR-027 §6)** : cache **chiffré au repos** + **purgé à la déconnexion** ; **identifiants INPI jamais cachés** (SecureStorage).
- **Placement (ADR-002)** : le cache vit dans `Atlas.Maui` (couche Storage), derrière un service de lecture consommé par les ViewModels ; **aucune** remontée vers `Domain`/`Infrastructure`.
- **Accessibilité (ADR-008 / doc 06)** : bandeau hors-ligne et états datés **annoncés** au lecteur d'écran ; statut jamais porté par la **seule couleur** (icône + texte).
- **Hors-scope** : la **détection retour-réseau, le rafraîchissement, l'éviction** (F-075) ; l'**écriture hors-ligne** ; le **offline web** (WASM pur — avenant futur) et **desktop Avalonia** (séparés) ; la timeline de veille complète (cache minimal v1).
