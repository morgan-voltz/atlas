# Roadmap features

> **Index du backlog produit (`F-NNN`)**, organisé selon la méthode **MoSCoW** et par jalon de version.
> Chaque feature a sa **fiche détaillée** dans [`docs/features/`](features/) (valeur user, complexité, APIs, dépendances, doctrine). Ce document en donne les **tables d'avancement + une synthèse + un lien**.
> L'implémentation du **client web** suit sa propre feuille de route, à part : [`docs/15-roadmap-client-web.md`](15-roadmap-client-web.md).

**Version** : 2.0
**Date de dernière mise à jour** : 1ᵉʳ juin 2026

---

## État d'avancement — MVP 1

Légende : ✅ livré & vérifié · 🟡 livré, vérification partielle (cf. statut détaillé de la feature) · ⬜ à faire.

| Feature | Statut |
|---|:--:|
| F-001 Inscription / connexion | ✅ |
| F-002 2FA TOTP | ✅ |
| F-003 Connexion compte INPI | ✅ |
| F-004 Recherche entreprise (SIREN) | ✅ |
| F-005 Recherche entreprise (nom) | ✅ |
| F-006 Recherche marque | 🟡 |
| F-007 Fiche marque | 🟡 |
| F-008 Historique de recherches | ✅ |
| F-009 Client MAUI mobile | 🟡 |
| F-010 Client MAUI desktop | 🟡 |
| F-011 Documentation | ✅ |
| F-012 Conformité RGPD | 🟡 |

**Validation INPI réelle (31 mai 2026, compte habilité)** : le **RNE est validé** de bout en bout
(F-004 fiche SIREN, F-005 recherche nom, F-013 actes/bilans → ✅ via Bruno `90-INPI-E2E-CI`).
La **recherche PI** (marques F-006/F-007, brevets F-015/F-016) reste **🟡 / bloquée** : l'API
`api-gateway.inpi.fr` répond `405 Allow: GET` sur `search` alors que l'adapter envoie `POST`
(auth PI et droits OK ; **correctif POST→GET + contrat GET à établir**). Les autres 🟡 : clients
MAUI compilés non QA (F-009/F-010), contenu légal UI du RGPD (F-012). Détail dans chaque fiche.

> **Transition UI (ADR-029, 30 mai 2026)** : les clients livrés ou amorcés ci-dessus (MAUI mobile/desktop F-009/F-010, web Blazor) sont **en transition vers Uno Platform** — une UI unique `Atlas.App` pour 6 surfaces (WebAssembly + desktop Win/macOS/Linux + iOS/Android). Les statuts ci-dessus reflètent l'état des clients **actuels** (qui restent en vigueur jusqu'au spike Uno concluant) ; la doctrine UX (`docs/12`) est préservée, agnostique de la techno.

---

## État d'avancement — MVP 2

Légende identique au tableau MVP 1.

**Section principale** (F-013 → F-022, F-062) :

| Feature | Statut |
|---|:--:|
| F-013 Téléchargement individuel d'actes et bilans | ✅ |
| F-014 Téléchargement en masse de documents | ✅ |
| F-015 Recherche brevet par numéro | 🟡 |
| F-016 Recherche brevet avancée (titre / inventeur / déposant) | 🟡 |
| F-017 Favoris : suivi d'une entreprise | ✅ |
| F-018 Favoris : suivi d'une marque ou d'un brevet | ✅ |
| F-019 Alerte email sur modification d'une entreprise favorite | ✅ |
| F-020 Notifications push mobiles (et desktop) | ✅ |
| F-021 Export CSV / Excel | 🟡 |
| F-022 Rapport PDF de fiche entreprise | ✅ |
| F-062 Synchronisation des préférences (multi-surface) | ⬜ |

**Cluster Veille** (F-041 → F-050) :

| Feature | Statut |
|---|:--:|
| F-041 Moteur d'agrégation RSS / Atom | ✅ |
| F-042 Catalogue de templates (VeillePack) | ✅ |
| F-043 Ajout libre de sources | ✅ |
| F-044 Timeline unifiée | ✅ |
| F-045 Déduplication intelligente (SimHash) | ✅ |
| F-046 Filtres et règles de surveillance | ✅ |
| F-047 Combinaison veille + favoris (3 volets : RSS, RNE, BODACC) | ✅ |
| F-048 Intégration BODACC | ✅ |
| F-049 Marketplace des templates partagés | ✅ |
| F-050 Préparation à la couche premium (architecture) | ✅ |

Les 🟡 correspondent surtout à : confirmation contre l'API INPI réelle (F-013, F-016) et
export XLSX et étendue de couverture (F-021). **Le cluster veille est complet côté backend.**
UI MAUI restante pour F-017, F-020, F-044, F-046, F-049 (chantier client cross-cutting,
suivi côté F-009/F-010 du MVP 1).

---

## Sommaire

- [Méthodologie](#méthodologie)
- [Conventions](#conventions)
- [MVP 1 — Must have (mois 1–4)](#mvp-1--must-have-mois-1-4)
- [MVP 2 — Should have (mois 4–8)](#mvp-2--should-have-mois-4-8)
- [Cluster Veille (intégré au MVP 2)](#cluster-veille-intégré-au-mvp-2)
- [Reste à faire pour clore MVP 2](#reste-à-faire-pour-clore-mvp-2)
- [V2 — Could have (mois 8–18)](#v2--could-have-mois-8-18)
- [V3+ — Won't have (yet)](#v3--wont-have-yet)
- [Features explicitement écartées](#features-explicitement-écartées)

---

## Méthodologie

La méthode **MoSCoW** classe les features en 4 buckets :

- **Must have** : le produit ne peut pas exister sans. Si on ne livre pas ça, on n'a pas livré.
- **Should have** : très important mais le produit existe sans. À livrer en deuxième.
- **Could have** : bien mais pas critique. À livrer si le temps le permet.
- **Won't have (yet)** : explicitement reporté, documenté pour ne pas être oublié.

**Pourquoi ce choix** : MoSCoW est plus rigoureux que la simple priorisation 1-2-3 parce qu'il force à classer les features en **catégories qualitativement différentes**. On évite l'inflation des priorités où tout devient "P1".

---

## Conventions

Pour chaque feature, on documente :

- **Description** : ce que ça fait du point de vue utilisateur.
- **Valeur user** : pourquoi c'est utile (à quel besoin ça répond).
- **Complexité** : estimation technique de l'effort.
  - ★ : 1-2 jours
  - ★★ : 3-5 jours
  - ★★★ : 1-2 semaines
  - ★★★★ : 3-4 semaines
  - ★★★★★ : 1-2 mois
- **APIs externes** : sources de données nécessaires.
- **Dépendances** : autres features prérequises.

---

## MVP 1 — Must have (mois 1–4)

**Objectif** : prouver que la stack technique fonctionne, que l'archi hex tient la route, et offrir une utilité minimale crédible. Pas de monétisation, repo encore privé. **À la fin du MVP 1, le repo passe en public.**

- [**F-001 — Inscription et connexion utilisateur**](features/F-001-inscription-et-connexion-utilisateur.md) — Statut : ✅ Implémenté (MVP 1, 27 mai 2026).
- [**F-002 — Authentification deux facteurs (2FA)**](features/F-002-authentification-deux-facteurs-2fa.md) — Statut : ✅ Implémenté (MVP 1, 27 mai 2026).
- [**F-003 — Connexion d'un compte INPI à son profil**](features/F-003-connexion-d-un-compte-inpi-a-son-profil.md) — Statut : ✅ Implémenté (MVP 1, 27 mai 2026).
- [**F-004 — Recherche entreprise par SIREN**](features/F-004-recherche-entreprise-par-siren.md) — Statut : 🟡 Implémenté (MVP 1, 27 mai 2026).
- [**F-005 — Recherche entreprise par dénomination**](features/F-005-recherche-entreprise-par-denomination.md) — Statut : 🟡 Implémenté (MVP 1, 27 mai 2026).
- [**F-006 — Recherche marque par dénomination**](features/F-006-recherche-marque-par-denomination.md) — Statut : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle.
- [**F-007 — Vue détaillée d'une marque**](features/F-007-vue-detaillee-d-une-marque.md) — Statut : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle.
- [**F-008 — Historique des recherches utilisateur**](features/F-008-historique-des-recherches-utilisateur.md) — Statut : ✅ Implémenté (MVP 1, 27 mai 2026).
- [**F-009 — Client MAUI mobile (Android/iOS) avec fonctions de base**](features/F-009-client-maui-mobile-android-ios-avec-fon.md) — Statut : 🟡 Slice navigable implémenté (MVP 1, 27 mai 2026), compilé Android (non lancé/capturé : pas d'émulateur ici).
- [**F-010 — Client desktop (Windows/macOS/Linux)**](features/F-010-client-maui-desktop-windows-macos.md) — Statut : 🟡 Amorcé (MVP 1, 27 mai 2026). **Requalifié par ADR-029 (UI unifiée Uno, 30 mai 2026)** : desktop (Win/macOS/Linux), mobile et web sont désormais une **cible unique `Atlas.App` (Uno)** — ADR-029 remplace l'ADR-026 (MAUI+Avalonia) et l'ADR-017 (Blazor web). Le travail MAUI-Windows déjà fait sert de référence d'écrans ; les ViewModels (noyau partagé) se transposent à Uno.
- [**F-011 — Documentation utilisateur basique**](features/F-011-documentation-utilisateur-basique.md) — Statut : ✅ Implémenté (MVP 1, 27 mai 2026).
- [**F-012 — Conformité RGPD MVP**](features/F-012-conformite-rgpd-mvp.md) — Statut : 🟡 Backend implémenté (MVP 1, 27 mai 2026).

---

## MVP 2 — Should have (mois 4–8)

**Objectif** : étoffer le produit avec les fonctions qui en font un vrai outil utilisable au quotidien. Le repo est public, on cherche les premiers feedbacks utilisateurs.

> **Le MVP 2 intègre également le Cluster Veille (F-041 à F-050)**, qui est documenté dans une section dédiée plus bas. Le cluster veille est une **feature majeure différenciante** (cf. ADR-009) et constitue la moitié du périmètre fonctionnel de ce jalon.

- [**F-013 — Téléchargement individuel d'actes et bilans**](features/F-013-telechargement-individuel-d-actes-et-bi.md) — Statut : 🟡 Backend implémenté (MVP 2, 29 mai 2026).
- [**F-014 — Téléchargement en masse de documents — ✅ MVP 2 (29 mai 2026)**](features/F-014-telechargement-en-masse-de-documents-mv.md) — 
- [**F-015 — Recherche brevet par numéro**](features/F-015-recherche-brevet-par-numero.md) — Statut : ✅ Backend implémenté (MVP 2, commit `85ae014`).
- [**F-016 — Recherche brevet par titre / inventeur / déposant**](features/F-016-recherche-brevet-par-titre-inventeur-de.md) — Statut : 🟡 Backend implémenté (MVP 2, 29 mai 2026).
- [**F-017 — Favoris : suivi d'une entreprise**](features/F-017-favoris-suivi-d-une-entreprise.md) — Statut : ✅ Backend implémenté (MVP 2, 28 mai 2026).
- [**F-018 — Favoris : suivi d'une marque ou d'un brevet**](features/F-018-favoris-suivi-d-une-marque-ou-d-un-brev.md) — Statut : ✅ Backend implémenté (MVP 2, 29 mai 2026).
- [**F-019 — Alerte email sur modification d'une entreprise favorite**](features/F-019-alerte-email-sur-modification-d-une-ent.md) — Statut : ✅ Backend implémenté + adapter Brevo livré (29 mai 2026).
- [**F-020 — Notifications push mobiles (et desktop)**](features/F-020-notifications-push-mobiles-et-desktop.md) — Statut : ✅ Backend complet (29 mai 2026), client MAUI restant.
- [**F-021 — Export CSV / Excel de résultats**](features/F-021-export-csv-excel-de-resultats.md) — Statut : 🟡 MVP CSV livré (MVP 2, 29 mai 2026).
- [**F-022 — Rapport PDF de fiche entreprise**](features/F-022-rapport-pdf-de-fiche-entreprise.md) — Statut : ✅ MVP implémenté (MVP 2, 29 mai 2026).
- [**F-062 — Synchronisation des préférences utilisateur (multi-surface)**](features/F-062-synchronisation-preferences-multi-surface.md) — Statut : ⬜ Spécifiée, **au périmètre V2 / MVP 2** — à implémenter (entité `UserPreference` + `GET/PUT /me/preferences`). Réalise la sync multi-device promise par ADR-001 (préférences « compte » qui suivent l'utilisateur vs « appareil » locales ; last-write-wins par clé).

---

## Cluster Veille (intégré au MVP 2)

> **Décision stratégique structurante** (cf. ADR-009) : la veille devient une **feature majeure et différenciante** du produit, exploitant le trou de marché identifié (aucun concurrent ne combine données entreprises et veille agrégée).
> Le cluster F-041 à F-050 forme un bloc cohérent à livrer ensemble en MVP 2 pour offrir une expérience complète dès le lancement de la veille.

- [**F-041 — Moteur d'agrégation RSS / Atom**](features/F-041-moteur-d-agregation-rss-atom.md) — Statut : ✅ Implémenté (MVP 2, 27 mai 2026).
- [**F-042 — Catalogue de templates de veille par métier**](features/F-042-catalogue-de-templates-de-veille-par-me.md) — Statut : ✅ Implémenté (MVP 2, 27 mai 2026).
- [**F-043 — Ajout libre de sources par l'utilisateur**](features/F-043-ajout-libre-de-sources-par-l-utilisateu.md) — Statut : ✅ Implémenté (MVP 2, 27 mai 2026).
- [**F-044 — Timeline unifiée de la veille**](features/F-044-timeline-unifiee-de-la-veille.md) — Statut : ✅ Backend implémenté (MVP 2, 27 mai 2026).
- [**F-045 — Déduplication intelligente des items**](features/F-045-deduplication-intelligente-des-items.md) — Statut : ✅ Implémenté (MVP 2, 28 mai 2026).
- [**F-046 — Filtres et règles de surveillance personnalisées**](features/F-046-filtres-et-regles-de-surveillance-perso.md) — Statut : ✅ Backend implémenté (MVP 2, 29 mai 2026).
- [**F-047 — Combinaison veille + favoris entreprises**](features/F-047-combinaison-veille-favoris-entreprises.md) — Statut : ✅ MVP intégral livré (3 volets, MVP 2, 29 mai 2026).
- [**F-048 — Intégration BODACC dans la veille**](features/F-048-integration-bodacc-dans-la-veille.md) — Statut : ✅ Backend implémenté (MVP 2, 29 mai 2026).
- [**F-049 — Marketplace des templates partagés (V2 light, posée en MVP 2)**](features/F-049-marketplace-des-templates-partages-v2-l.md) — Statut : ✅ Backend implémenté (MVP 2, 29 mai 2026).
- [**F-050 — Préparation à la couche premium (architecture)**](features/F-050-preparation-a-la-couche-premium-archite.md) — Statut : ✅ Implémenté (MVP 2, 29 mai 2026).

---

## Exploitation & amélioration produit (transverse — Should have)

> Deux features **transverses** (hors cœur fonctionnel et hors grappes V2), cadrées le 30 mai 2026. L'**observabilité** (santé infra) s'instrumente dès la V1 (quasi gratuite) ; la **télémétrie produit** (usage, opt-in) s'active V1–V2. **À ne pas confondre** : l'une observe les *machines* (aucun RGPD), l'autre mesure l'*usage* (opt-in, anonyme, RGPD).

- [**F-076 — Télémétrie produit (opt-in, anonyme, auto-hébergée)**](features/F-076-telemetrie-produit.md) — Spécifiée le 30 mai 2026. Métriques **anonymes/agrégées**, **OFF par défaut** (opt-in), **auto-hébergées** (rien chez un tiers) ; le contenu des recherches/fiches n'est **jamais** collecté ; deux flux (stabilité / usage). **Avenant RGPD** (`docs/04-securite-rgpd.md`) requis avant activation en prod. UX : doc 12 §9.
- [**F-077 — Observabilité du backend (instrumentation & santé)**](features/F-077-observabilite-backend.md) — Spécifiée le 30 mai 2026, met en œuvre **ADR-028**. Instrumentation **OpenTelemetry** (logs/métriques/traces) + **`/health`** agrégé + **uptime check externe** + notification d'échec de jobs Hangfire ; léger en préprod (ADR-019), stack lourde différée en prod sur machine dédiée. **Santé infra, aucune donnée utilisateur** (≠ F-076).

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
3. **Veille étendue & signaux** — le cluster veille du MVP 2 monte d'un cran : surveillance PI automatisée par règles utilisateur, signaux de risque descriptif, et **veille réglementaire UE (EUR-Lex) rattachée au secteur** des entités suivies.
4. **Outillage PI avancé** — killer feature pour les cabinets PI : portefeuille IP et antériorité avec matching intelligent.
5. **Exposition tiers** — Atlas devient une plateforme : API publique pour les intégrateurs, serveur MCP pour les agents IA.

**Récap V2** (16 features) :

| Grappe | # | Feature | Complexité |
|---|---|---|---|
| 1 — Fiche | F-024 | Intégration Sirene (établissements) | ★★★ |
| 1 — Fiche | F-051 | Suivi boursier des entreprises cotées | ★★★★ |
| 1 — Fiche | F-032 | Marchés publics remportés (DECP) | ★★★ |
| 1 — Fiche | F-054 | Indicateurs financiers descriptifs | ★★★ à ★★★★ |
| 2 — Orga | F-053 | Watchlists (listes d'entreprises) | ★★★ |
| 2 — Orga | F-030 | Annotations et tags utilisateur | ★★ |
| 2 — Orga | F-029 | Mode offline mobile (cache de lecture) | ★★★ |
| 2 — Orga | F-075 | Moteur de fraîcheur & sync du cache offline | ★★★★ |
| 3 — Veille | F-027 | Veille PI automatisée (règles utilisateur) | ★★★★ |
| 3 — Veille | F-055 | Signaux de risque (descriptif) | ★★★ |
| 3 — Veille | F-063 | Source de veille réglementaire EUR-Lex | ★★★★ |
| 3 — Veille | F-064 | Matching sectoriel NAF de la veille | ★★★ |
| 4 — PI | F-025 | Tableau de bord portefeuille IP | ★★★★★ |
| 4 — PI | F-026 | Antériorité marque (matching intelligent) | ★★★★★ |
| 5 — Exposition | F-028 | API publique du projet | ★★★★ |
| 5 — Exposition | F-052 | Serveur MCP (accès agents IA) | ★★★★ à ★★★★★ |

**Note de consolidation (29 mai 2026)** :
- **F-023 supprimée** — son périmètre (« Intégration BODACC ») est entièrement couvert par **F-048** livrée en MVP 2 (polling BODACC + `FavoriteEvent BodaccPublished` + dédup cross-users, cf. commit `50f176f`). Le numéro F-023 reste libre (non recyclé).
- **F-027 reformulée** pour la distinguer de **F-046** (filtres / règles de veille génériques, MVP 2) : F-027 = application spécifiquement PI au-dessus de F-046.

---

**Grappe 1 — Approfondissement de la fiche entreprise** *(F-024, F-051, F-032, F-054 — au-delà du RNE / PI, on enrichit la fiche entreprise avec des dimensions descriptives nouvelles)*

- [**F-024 — Intégration Sirene (INSEE) pour les établissements**](features/F-024-integration-sirene-insee-pour-les-etabl.md) — 
- [**F-051 — Suivi boursier des entreprises cotées**](features/F-051-suivi-boursier-des-entreprises-cotees.md) — 
- [**F-032 — Marchés publics remportés (DECP)**](features/F-032-marches-publics-remportes-decp.md) — Réactivée le 29 mai 2026 — promue de V3+ vers V2.
- [**F-054 — Indicateurs financiers descriptifs**](features/F-054-indicateurs-financiers-descriptifs.md) — 
- [**F-053 — Watchlists (listes d'entreprises)**](features/F-053-watchlists-listes-d-entreprises.md) — 
- [**F-030 — Annotations et tags utilisateur**](features/F-030-annotations-et-tags-utilisateur.md) — 
- [**F-029 — Mode offline mobile (cache de lecture)**](features/F-029-mode-offline-mobile-cache-lecture.md) — Réécrite le 31 mai 2026, **promue de stub à spec** et **scindée** : F-029 = le **cache de lecture** (rendu honnête) ; **F-075** = le moteur de fraîcheur/sync. Cadrée par **ADR-027**, **lecture seule** v1.
- [**F-075 — Moteur de fraîcheur & synchronisation du cache offline**](features/F-075-moteur-fraicheur-sync-offline.md) — Spécifiée le 31 mai 2026 : détection réseau, rafraîchissement au retour réseau, éviction par l'espace (favoris gardés) ; alimente le cache de F-029. Cadrée par **ADR-027**, descendant seul (pas d'écriture hors-ligne v1).
- [**F-027 — Veille PI automatisée (règles utilisateur)**](features/F-027-veille-pi-automatisee-regles-utilisateu.md) — Reformulée 29 mai 2026 — précision du périmètre pour la distinguer de F-046 (cluster veille MVP 2).
- [**F-055 — Signaux de risque (descriptif)**](features/F-055-signaux-de-risque-descriptif.md) — 
- [**F-063 — Source de veille réglementaire EUR-Lex**](features/F-063-source-veille-reglementaire-eur-lex.md) — Spécifiée le 31 mai 2026, cadrée par ADR-020. Pendant *réglementaire* de F-041 ; livre la classification native EUR-Lex (EuroVoc + directory code), sans NAF. Fondation de F-064.
- [**F-064 — Matching sectoriel NAF de la veille**](features/F-064-matching-sectoriel-naf-veille.md) — Spécifiée le 31 mai 2026, cadrée par ADR-020. Pendant *sectoriel* de F-047 (du nom d'entité au secteur) ; sortie `MatchCandidate` « secteur semble concerné — à vérifier », routage anti-noyade par `Scope`.
- [**F-025 — Tableau de bord portefeuille IP**](features/F-025-tableau-de-bord-portefeuille-ip.md) — 
- [**F-026 — Recherche d'antériorité marque avec matching intelligent**](features/F-026-recherche-d-anteriorite-marque-avec-mat.md) — Architecture liée : F-026 respecte la posture de ADR-014 (matching conservateur unifié — produit des `MatchCandidate`, jamais de verdict de disponibilité), mais son moteur reste totalement séparé des 3 matchers à base de noms (F-047, F-055, F-031) : similarité phonétique / visuelle / conceptuelle + classes de Nice = mécanique entièrement à part.
- [**F-028 — API publique du projet**](features/F-028-api-publique-du-projet.md) — 
- [**F-052 — Serveur MCP (accès agents IA)**](features/F-052-serveur-mcp-acces-agents-ia.md) — 

---

## V3+ — Won't have (yet)

Features identifiées comme valables mais explicitement reportées hors du périmètre actuel. À reconsidérer en fonction de la traction.

- [**F-056 — Vue 360 / Dossier entreprise**](features/F-056-vue-360-dossier-entreprise.md) — Méta-feature d'assemblage figée le 29 mai 2026.
- [**F-031 — Jurisprudence rattachée à l'entité (Judilibre)**](features/F-031-jurisprudence-rattachee-a-l-entite-judi.md) — Réactivée & recadrée le 29 mai 2026 — remplace le stub V3+ initial (motif « matching d'entité non-trivial »).
- [**F-057 — Re-screening continu (surveillance des sanctions)**](features/F-057-re-screening-continu-surveillance-des-s.md) — Statut : V3+, candidate naturelle au tout début de V3.
- [**F-058 — Signaux concurrentiels & digest sectoriel**](features/F-058-signaux-concurrentiels-digest-sectoriel.md) — Statut : V3+, s'allume progressivement — le volet « signaux PI » dépend de F-027 (V2).
- [**F-059 — Veille d'échéances PI (docketing assistif souverain)**](features/F-059-veille-d-echeances-pi-docketing-assisti.md) — Statut : V3+ conditionnel — conditionnée non par une DPIA (comme F-034) mais par un cadrage de responsabilité explicite : c'est le prérequis bloquant ici.
- [**F-033 — Indicateurs environnementaux descriptifs**](features/F-033-indicateurs-environnementaux-descriptif.md) — Reframé & figé le 29 mai 2026 — remplace le stub V3+ « Score ESG / Bilan carbone ».
- [**F-034 — Graphe de co-mandats des dirigeants (descriptif)**](features/F-034-graphe-de-co-mandats-des-dirigeants-des.md) — Reframé & figé le 29 mai 2026 — remplace le stub V3+ initial (« détection de sociétés écrans / conflits d'intérêt »).
- [**F-035 — Mode collaboratif / espace équipe**](features/F-035-mode-collaboratif-espace-equipe.md) — 
- [**F-036 — Plugin / extension navigateur**](features/F-036-plugin-extension-navigateur.md) — 
- [**F-037 — Connecteurs CRM (Salesforce, HubSpot, Pipedrive)**](features/F-037-connecteurs-crm-salesforce-hubspot-pipe.md) — 
- [**F-038 — Webhooks pour les alertes**](features/F-038-webhooks-pour-les-alertes.md) — 
- [**F-039 — Couverture européenne (EUIPO, OMPI)**](features/F-039-couverture-europeenne-euipo-ompi.md) — 
- [**F-040 — Couverture brevets mondiale (OEB Espacenet)**](features/F-040-couverture-brevets-mondiale-oeb-espacen.md) — 
- [**F-060 — Vérificateur de présence d'un nom (multi-sources)**](features/F-060-verificateur-de-presence-d-un-nom-multi.md) — Figée le 30 mai 2026. Statut V3+ — Won't have (yet). Doctrine ADR-012 / ADR-014 : Atlas rapporte des faits sourcés et datés, jamais un verdict de disponibilité.
- [**F-061 — Surveillance continue d'un nom**](features/F-061-surveillance-continue-d-un-nom.md) — Figée le 30 mai 2026. Statut V3+ — Won't have (yet). Pendant temporel de F-060. Doctrine ADR-014 : sortie = `MatchCandidate` « à vérifier », jamais « conflit avéré ». Jumeau structurel de F…

### Clusters d'approfondissement (figés le 31 mai 2026, cadrés par ADR-021 → ADR-025)

> Cinq clusters qui **approfondissent la donnée et l'usage** par-dessus le dossier 360 (F-056) et le graphe (F-034). Rangés en V3+ car leurs **prérequis** (F-056, F-034) y sont aussi ; chaque cluster s'allume dès que son socle est posé. Tous respectent la doctrine **descriptif/candidat, jamais de verdict** (ADR-012/014) et la souveraineté (on-infra / BYOAI).
>
> **Candidats à remontée V2** (prérequis légers, indépendants de F-056/F-034) : **F-065** (n'expose que des champs *déjà capturés* — provenance/AsOf/état) et **F-069** (s'appuie sur **F-013 déjà livré**). À remonter si jugés utiles avant que F-056/F-034 ne soient posés ; les autres features restent gouvernées par leurs prérequis V3+.

**Confiance** *(ADR-021)* — rendre la provenance et l'audit tangibles.
- [**F-065 — Surface de confiance navigable**](features/F-065-surface-de-confiance-navigable.md) — ★★ — Rend la ligne de provenance **traversable** : un tap ouvre « d'où vient cette donnée » (source, référence, horodatage, dernière confirmation ≠ dernier changement, état). Dépend de F-056. Livrable seul.
- [**F-066 — Export auditable d'un dossier**](features/F-066-export-auditable-dossier.md) — ★★★ — Export de dossier **scellé** (SHA-256 sur contenu canonicalisé + horodatage, auto-vérifiable) + traçabilité par fait + manifeste. Le livrable de due diligence souverain (KYC/compliance, avocat). Cadrée par ADR-021 ; dépend de F-056, F-022. Distincte de F-012 (RGPD) et F-021 (export résultats).

**Trajectoire** *(ADR-022)* — l'axe temporel.
- [**F-067 — Fondation temporelle (journal bi-temporel + « état à une date »)**](features/F-067-fondation-temporelle-etat-a-une-date.md) — ★★★★ — Journal append-only bi-temporel (réutilise les `MonitoredChange` d'ADR-013) + reconstruction `asOf` ; l'`AsOf` du dossier devient un **paramètre de requête**. Socle invisible ; débloque l'export à une date (F-066). Cadrée par ADR-022 ; consomme ADR-013, F-056.
- [**F-068 — Vue trajectoire d'une entité**](features/F-068-vue-trajectoire-entite.md) — ★★★ — Deux rendus d'une même donnée historisée : **curseur « état à une date »** + **timeline des changements**. Surface de F-067 ; réutilise la grammaire F-044/F-047. Cadrée par ADR-022.

**Document intelligence** *(ADR-023)* — exploiter les actes.
- [**F-069 — Index documentaire intelligent (actes)**](features/F-069-index-documentaire-intelligent-actes.md) — ★★★ — Actes **trouvables et lisibles** : classification légère (type/date/objet) + OCR local plein-texte cherchable + saut à la page. Fondation faible risque, utile seule. Cadrée par ADR-023 ; dépend de F-013.
- [**F-070 — Extraction de faits candidats des actes**](features/F-070-extraction-faits-candidats-actes.md) — ★★★★ — Extrait des faits structurés (cessions de parts, capital, dirigeants) **ancrés à la page**, « semble … — à vérifier ». Le morceau dur (extraction de texte juridique, conservatrice). Cadrée par ADR-023 ; s'appuie sur ADR-014, F-069, et enrichit F-067/F-066.

**Graphe d'écosystème** *(ADR-024)* — arêtes typées sur le substrat F-034.
- [**F-071 — Arête « marché public » du graphe**](features/F-071-arete-marche-public-graphe.md) — ★★ — Arête acheteur public ↔ titulaire (DECP, F-032) dans la traversée bornée hub-aware. Fondation quasi gratuite (donnée déjà ingérée par F-032). Cadrée par ADR-024 ; sur le substrat F-034.
- [**F-072 — Arête « co-dépôt PI » du graphe**](features/F-072-arete-co-depot-pi-graphe.md) — ★★★★ — Arête co-dépôt brevet/marque (INPI PI) = signal de partenariat R&D, avec **résolution déposant → SIREN conservatrice** (v1 = personnes morales). Cadrée par ADR-024 ; sur F-034, ADR-014.

**Surfaces agentiques** *(ADR-025)* — génératif, sur la surface MCP (F-052).
- [**F-073 — Règles de surveillance composées en langage naturel**](features/F-073-regles-surveillance-langage-naturel.md) — ★★★ — L'utilisateur décrit en NL ce qu'il veut surveiller ; un agent **compose un brouillon** de `FeedRule`/`WatchRule` qu'il valide, puis exécution déterministe dans F-046 (**LLM hors runtime**). Cadrée par ADR-025 ; étend F-046, sur F-052/ADR-016.
- [**F-074 — Agent de sourcing souverain (« thèse → cibles »)**](features/F-074-agent-sourcing-souverain.md) — ★★★★ — Agent **BYOAI** qui traverse les outils MCP lecture (F-064/F-054/F-032/F-072/F-067/F-056) et fait remonter des **entreprises candidates** (faits sourcés + critères matchés) — **jamais un classement, un score ni une recommandation**. Le différenciant génératif le plus lourd, à livrer en dernier. Cadrée par ADR-025 ; sur F-052/ADR-011/ADR-016/ADR-014.

---

## Features explicitement écartées

Pour mémoire, certaines pistes ont été évaluées et **explicitement écartées** :

| Feature écartée | Raison |
|---|---|
| Données des Bénéficiaires Effectifs (BE) | Restriction d'accès depuis l'arrêt CJUE Sovim (nov. 2022). Nécessite un statut "personne habilitée" ou la démonstration d'un intérêt légitime. Risque juridique trop élevé pour un MVP. |
| Données fiscales détaillées DGFIP | Non disponibles en open data au niveau entreprise individuelle. |
| Données URSSAF / cotisations sociales | Couvertes par le secret professionnel. |
| Données médicales (cliniques, médecins) | Hors périmètre métier, et soumis au secret médical pour partie. |
| Données bancaires (Banque de France) | API restreintes aux acteurs financiers agréés. |
| Données LinkedIn / réseaux sociaux | Pas d'API officielle ouverte, ToS très restrictives. Scraping = risque juridique. |

---

*Document évolutif. Toute nouvelle feature doit être ajoutée avec son identifiant unique (F-NNN), sa description complète selon le template, et sa catégorie MoSCoW.*
