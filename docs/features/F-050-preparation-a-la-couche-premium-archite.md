# F-050 — Préparation à la couche premium (architecture)

> **Statut** : ✅ Implémenté (MVP 2, 29 mai 2026). Les 3 ports premium sont déclarés côté domaine dans `Atlas.Domain.Veille.Premium` : `IFeedItemEnricher` (+ `FeedItemEnrichment` record), `IFeedRelevanceScorer` (score 0-100 par item × user), `IFeedSummarizer` (synthèse narrative d'un batch). Projet `Atlas.Application.Premium` matérialisé via `AssemblyMarker` public pour permettre aux tests d'architecture de cibler son assembly. 3 nouveaux tests dans `Atlas.Architecture.Tests` verrouillant la séparation : (a) `Atlas.Application` (cœur) ne référence pas `Atlas.Application.Premium`, (b) `Atlas.Domain` ne référence pas `Atlas.Application.Premium`, (c) `Atlas.Application.Premium` ne référence pas `Atlas.Infrastructure.*` ni `Atlas.Api`. Total : 7 tests d'archi verts (4 existants + 3 F-050). Aucune implémentation des ports en MVP 2 (cœur open source intact) ; les adapters viendront dans des projets `Atlas.Infrastructure.*Premium` dédiés. Patron réutilisable pour `IFinancialSummarizer` (F-054).

**Description** : pas une feature visible côté user, mais une **décision d'architecture** à respecter dès la phase de design du cluster veille : tous les use cases d'enrichissement (futurs résumés IA, scoring IA, synthèse hebdo) sont définis comme des **ports séparés** dans le domaine, implémentés en projet `.Premium` distinct (vide en MVP 2).

**Valeur user** : aucune en MVP 2. Préparation à la phase de monétisation pour ne pas refondre.

**Complexité** : ★★ (discipline d'architecture pure)

**APIs externes** : aucune.

**Dépendances** : architecture hexagonale globale (ADR-004).

**Détails techniques** :
- Projet `<Projet>.Application.Premium` créé vide ou avec stubs
- Ports `IFeedItemEnricher`, `IFeedRelevanceScorer`, `IFeedSummarizer` définis côté domaine
- Aucune implémentation en MVP 2 (les use cases premium ne sont pas appelés)
- L'ajout futur d'adapters LLM ne nécessitera **aucune modification du domaine**

---

## Reste à faire pour clore MVP 2

> **Synthèse au 29 mai 2026** — extraite des blocs « Statut » des fiches ci-dessus. Tient lieu de punch-list MVP 2.

**🔴 Bloquant — vrais trous restants** (à livrer pour annoncer MVP 2 « fini »)

1. ~~**F-046 — Filtres et règles de surveillance personnalisées**~~ ✅ **Livré 29 mai 2026** (backend). Reste UI MAUI et adapter Brevo (commun avec F-019).
2. ~~**F-049 — Marketplace des templates partagés**~~ ✅ **Livré 29 mai 2026** (backend, périmètre V2 light gardé en MVP 2). Reste UI MAUI (création / browse / like / report) et workflow admin de revue des reports.
3. ~~**F-050 — Vérification de l'architecture premium**~~ ✅ **Livré 29 mai 2026** : 3 ports déclarés dans `Atlas.Domain.Veille.Premium` + 3 tests d'archi verrouillant la séparation cœur / premium (cf. fiche F-050).

**🟡 Important non-bloquant** (peut basculer en post-MVP 2 sans casser la promesse)

- ~~**Adapter email Brevo (F-019)**~~ ✅ **Livré 29 mai 2026** : `BrevoEmailSender` envoie les emails de F-001 / F-019 / F-046 via l'API Brevo (`POST /v3/smtp/email`), switch DI sur `Email:Brevo:ApiKey`. Sans clé renseignée, fallback `LoggingEmailSender` (mode dev). 6 tests unitaires (payload + erreurs).
- **Trame UI MAUI cross-cutting** : F-017 (onglet « Mes favoris »), F-020 (récupération du token push natif + `POST /devices`), F-044 (vue timeline veille). Backend livré pour les 3 ; côté MAUI c'est le gros chantier client restant (suivi en parallèle dans F-009/F-010 du MVP 1, encore 🟡).
- **Validation contre l'API INPI réelle** : F-013 (auth Bearer + structure JSON attachments), F-015 (structure JSON détail brevet), F-016 (syntaxe SolR + réponse paginée). Tous documentés mais non confirmés en prod. Idéalement levés via un compte INPI réel et une session Bruno.
- **F-014 — Lot ultérieur** : notification fin de job (push/email), job récurrent de purge des archives expirées + entité `BulkDownloadJob`, adapter `S3FileStorage` (MinIO/Wasabi/AWS), rate limiting INPI dédié aux téléchargements.
- **F-021 — Étendue d'export** : ClosedXML pour XLSX, export des résultats de recherche RNE/PI (paginé), export de la veille (timeline).
- **F-022 — Enrichissement PDF** : logo, historique des modifications via snapshots F-019, bilans intégrés via F-013 download.
- **F-048 — Configuration fine user** : aujourd'hui toutes les annonces BODACC sont remontées. Filtres par mots-clés / secteurs / types d'annonces à ajouter ; et validation contre l'API Opendatasoft réelle.

**🧭 Décisions à acter avant de fermer MVP 2**

- F-049 : dans MVP 2 ou bascule officielle en V2 ?
- Quels « 🟡 » assume-t-on en post-MVP 2 vs lesquels passe-t-on en ✅ avant de fermer ?
- Promotion des MVP 1 🟡 résiduels (F-004 à F-007 — confirmation INPI réelle, F-009/F-010 — exécution MAUI + QA, F-012 — contenu légal UI) : à boucler dans la même fenêtre que MVP 2 pour pouvoir parler de « MVP livré ».

---

## Campagne de tests e2e — 30 mai 2026

> Exécution locale de la collection Bruno versionnée (`bruno/`) contre l'API réelle
> (procédure : `docs/13-harness-test-local-e2e.md`).

**État** : le **cœur fonctionnel est vert** — flux complets d'inscription, 2FA TOTP,
RGPD (export / effacement), favoris (entreprise / marque / brevet), devices/push,
veille et règles de surveillance. Point de départ de la campagne : 86/138 requêtes
vertes, les écarts étant ensuite soit corrigés (ci-dessous), soit attribués aux
endpoints INPI (cf. « Validation contre l'API INPI réelle »).

**Corrections issues de la campagne**

- **F-014** — `GET /downloads/bulk/{id}/archive` renvoyait `500` (chemin de stockage
  `RootPath` non configuré écrasé à `null`) ; le job de fond plantait de même. Repli
  défensif sur le répertoire par défaut + tests de régression (#69).
- **F-001** — `GET /auth/verify-email` renvoyait `500` sur un `userId` vide ou mal
  formé (échec de binding du `Guid`). Binding permissif → `400` via le validator (#72).
- **Transversal API** — les ProblemDetails exposent désormais un champ `code` stable
  (RFC 9457) sur toutes les erreurs (métier et validation), au lieu d'un code dispersé
  entre `title`/`type`. Déduplique au passage l'invariant feed-rule (`veille.invalid_feed_rule`
  porté par le seul domaine). Contrat utile pour F-028 (#74).

**Outillage de test**

- Guide du harness e2e local ajouté (`docs/13-harness-test-local-e2e.md`, #71).
- Rate limit `auth-strict` relâché en `Development` pour permettre les runs récursifs (#70).
- Variables `depositNumber` / `publicationNumber` renseignées dans `Local.bru`, débloquant
  le dossier `18-IP-Favorites` (#73).

**Reste à valider** : les dossiers INPI (`16-Company-Attachments`, `17-Patents`,
`20-Company-Report`, `90-INPI-E2E-CI`) renvoient `409 inpi.not_connected` faute de
compte INPI habilité — couvert par la punch-list « Validation contre l'API INPI réelle ».

---

## V2 — Could have (mois 8–18)

**Objectif** : différenciation par rapport aux concurrents (Pappers, Societe.com). C'est ici qu'on construit les killer features qui rendent le projet unique.

**Storyline** — V2 s'organise en 5 grappes qui se nourrissent l'une l'autre :

1. **Approfondissement de la fiche entreprise** — au-delà du RNE / PI, on enrichit avec établissements (Sirene), cotation boursière, marchés publics et indicateurs financiers descriptifs.
2. **Organisation & capitalisation** — l'utilisateur range et emporte sa connaissance (watchlists, annotations & tags, mode offline mobile).
3. **Veille étendue & signaux** — le cluster veille du MVP 2 monte d'un cran : surveillance PI automatisée par règles utilisateur + signaux de risque descriptif.
4. **Outillage PI avancé** — killer feature pour les cabinets PI : portefeuille IP et antériorité avec matching intelligent.
5. **Exposition tiers** — Atlas devient une plateforme : API publique pour les intégrateurs, serveur MCP pour les agents IA.

**Récap V2** (13 features) :

| Grappe | # | Feature | Complexité |
|---|---|---|---|
| 1 — Fiche | F-024 | Intégration Sirene (établissements) | ★★★ |
| 1 — Fiche | F-051 | Suivi boursier des entreprises cotées | ★★★★ |
| 1 — Fiche | F-032 | Marchés publics remportés (DECP) | ★★★ |
| 1 — Fiche | F-054 | Indicateurs financiers descriptifs | ★★★ à ★★★★ |
| 2 — Orga | F-053 | Watchlists (listes d'entreprises) | ★★★ |
| 2 — Orga | F-030 | Annotations et tags utilisateur | ★★ |
| 2 — Orga | F-029 | Mode offline mobile avec sync | ★★★★ |
| 3 — Veille | F-027 | Veille PI automatisée (règles utilisateur) | ★★★★ |
| 3 — Veille | F-055 | Signaux de risque (descriptif) | ★★★ |
| 4 — PI | F-025 | Tableau de bord portefeuille IP | ★★★★★ |
| 4 — PI | F-026 | Antériorité marque (matching intelligent) | ★★★★★ |
| 5 — Exposition | F-028 | API publique du projet | ★★★★ |
| 5 — Exposition | F-052 | Serveur MCP (accès agents IA) | ★★★★ à ★★★★★ |

**Note de consolidation (29 mai 2026)** :
- **F-023 supprimée** — son périmètre (« Intégration BODACC ») est entièrement couvert par **F-048** livrée en MVP 2 (polling BODACC + `FavoriteEvent BodaccPublished` + dédup cross-users, cf. commit `50f176f`). Le numéro F-023 reste libre (non recyclé).
- **F-027 reformulée** pour la distinguer de **F-046** (filtres / règles de veille génériques, MVP 2) : F-027 = application spécifiquement PI au-dessus de F-046.

---

**Grappe 1 — Approfondissement de la fiche entreprise** *(F-024, F-051, F-032, F-054 — au-delà du RNE / PI, on enrichit la fiche entreprise avec des dimensions descriptives nouvelles)*
