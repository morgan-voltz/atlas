# Décisions architecturales & stratégiques

> Index des décisions structurantes du projet, au format **Architecture Decision Records (ADR)** de Michael Nygard.
> **Chaque ADR vit dans son propre fichier sous [`docs/ADR/`](ADR/)** ; ce document n'en donne qu'une **synthèse + lien**.
> Toute évolution d'une décision se fait dans le fichier ADR concerné (statut, date, amendement).

**Version** : 2.0
**Date de dernière mise à jour** : 1ᵉʳ juin 2026
**Statut** : décisions figées ; nouvelles décisions ajoutées comme fichiers ADR au fil de l'eau.

---

## Sommaire

- [Vision du projet](#vision-du-projet)
- [Décisions (ADR-001 → ADR-028)](#décisions-adr-001--adr-028)
- [Décisions à prendre ultérieurement](#décisions-à-prendre-ultérieurement)
- [Roadmap macro](#roadmap-macro)
- [Glossaire](#glossaire)

---

## Vision du projet

**Nom de code** : à définir (suggestions : `Openpriv`, `Datahex`, `Frenchdata`, `Civicdata` — à figer avant le repo).

**Pitch en une phrase** :
> Une plateforme open source pour interagir avec l'écosystème des données publiques françaises sur les entreprises et la propriété industrielle, accessible depuis le bureau, le mobile et le web.

**Pitch étendu** :
La France possède un écosystème open data parmi les plus riches du monde (RNE, Sirene, BODACC, DECP, INPI PI, Judilibre, etc.), mais ces APIs sont **fragmentées**, **techniquement hétérogènes** (auth, formats, rate limits) et **mal intégrées** entre elles dans les outils existants. Les solutions commerciales (Pappers, Societe.com) agrègent ces sources mais en mode boîte noire, payant pour les usages avancés, et avec une API limitée.

Le projet propose une **alternative open source** qui :

1. **Unifie l'accès** à ces sources via une couche d'abstraction propre (architecture hexagonale).
2. **Respecte les flags de diffusion** et le RGPD nativement.
3. **Donne le contrôle aux utilisateurs** : ils gardent leurs credentials, peuvent auto-héberger, ou utiliser une instance SaaS gérée.
4. **Sert de référence technique** pour la communauté dev française qui veut bâtir des outils sur l'open data entreprises.

**Cible primaire à long terme** : développeurs, cabinets juridiques / d'expertise comptable, services compliance, intégrateurs B2B. **Cible primaire à court terme (phase 1)** : développeurs et communauté open source.

---

## Décisions (ADR-001 → ADR-028)

> Synthèses ; le détail (contexte, rationale, conséquences) est dans chaque fichier de [`docs/ADR/`](ADR/).

### Fondations

- **[ADR-001 — Modèle d'usage multi-utilisateur SaaS](ADR/ADR-001-modele-usage-saas.md)** ✅
  Plateforme **multi-utilisateur SaaS** (et non outil mono-poste) : sync multi-device, fonctions asynchrones (veille/alertes) côté backend 24/7, base utilisateurs commune.
- **[ADR-002 — Topologie hybride](ADR/ADR-002-topologie-hybride.md)** ✅
  **Backend séparé strict** : toute la logique dans ASP.NET Core ; MAUI et (futur) Blazor sont des **clients HTTP purs** partageant `Domain`/`Shared`.
- **[ADR-003 — Authentification INPI multi-tenant](ADR/ADR-003-auth-inpi-multitenant.md)** ✅
  Chaque utilisateur fournit ses **propres credentials INPI**, chiffrés au repos (AES-256-GCM, KMS), déchiffrés le temps d'une requête, jamais loggés.
- **[ADR-004 — Architecture hexagonale platform-ready](ADR/ADR-004-architecture-hexagonale.md)** ✅
  Ports & Adapters stricts ; le domaine ne dépend de rien ; dépendances vers l'intérieur, vérifiées par NetArchTest.
- **[ADR-005 — Licence open source AGPL v3](ADR/ADR-005-licence-agpl-v3.md)** ✅
  **AGPL v3** : copyleft fort (protège contre le fork SaaS fermé), compatible dual-licensing futur.
- **[ADR-006 — Modèle économique : open core](ADR/ADR-006-modele-economique.md)** ✅
  OSS pur d'abord, puis **SaaS hébergé** ; frontière cœur / premium isolée structurellement (`Atlas.Application.Premium`).
- **[ADR-007 — Stack technique .NET / MAUI](ADR/ADR-007-stack-dotnet-maui.md)** ✅ *(amendé par ADR-026 — desktop → Avalonia)*
  **Tout-.NET** (une seule stack) : ASP.NET Core, **Blazor pour le web** (précisé par ADR-017), clients natifs **MAUI (mobile) + Avalonia (desktop)** (amendé par ADR-026).
- **[ADR-008 — Accessibilité WCAG 2.2 AA + RGAA 4.1.2](ADR/ADR-008-accessibilite-wcag.md)** ✅
  Accessibilité **bloquante** dans la Definition of Done de toute UI.
- **[ADR-009 — Veille comme feature majeure + open core](ADR/ADR-009-veille-open-core.md)** ✅
  La veille est un pilier produit différenciant ; ses extensions (IA) sont le terrain premium.

### Authentification & délégation

- **[ADR-010 — Authentification utilisateur hexagonale custom](ADR/ADR-010-auth-utilisateur-custom.md)** ✅
  Auth maison : Argon2id, **JWT RS256** courts, refresh tokens rotatifs, 2FA TOTP.
- **[ADR-011 — Exposition agentique (MCP) & OAuth 2.1](ADR/ADR-011-exposition-agentique-oauth.md)** ✅ *(mise en œuvre différée à F-052)*
  L'accès agentique (MCP) passe par une **autorisation déléguée OAuth 2.1** (scopes, consentement, révocation) — jamais les credentials utilisateur.

### Doctrine descriptive, substrat & matching

- **[ADR-012 — Réutilisation des données de dirigeants & graphe relationnel](ADR/ADR-012-reutilisation-donnees-dirigeants.md)** ✅
  **Descriptif, jamais de verdict** ; DPIA obligatoire en prérequis (F-034).
- **[ADR-013 — Substrat de surveillance des entités](ADR/ADR-013-substrat-surveillance.md)** ✅
  Deux stratégies (`IStateMonitor<TState>` retraits / `IItemStreamMonitor<TItem>` flux append-only) + runner mutualisé (extraction à la 3ᵉ instance, F-057).
- **[ADR-014 — Matching conservateur unifié](ADR/ADR-014-matching-conservateur-unifie.md)** ✅
  `MatchCandidate` encode la doctrine ADR-012 **dans le type** (état illégal irreprésentable) ; noyau de normalisation des noms partagé.
- **[ADR-015 — Couche d'assemblage du dossier entreprise](ADR/ADR-015-assemblage-dossier-360.md)** ✅
  Read-model `CompanyDossier` à sections auto-descriptives, 5 états `SectionState`, résolution snapshot-first (backbone de F-056).
- **[ADR-016 — Sécurité & doctrine de la surface agentique (MCP)](ADR/ADR-016-securite-doctrine-surface-agentique.md)** ✅
  Surface curée **lecture-d'abord**, doctrine inline avec la donnée, contenu externe = donnée jamais instruction, credentials qui ne traversent jamais.
- **[ADR-020 — Matching sectoriel de la veille par crosswalk éditorial](ADR/ADR-020-matching-sectoriel-crosswalk-editorial.md)** ✅
  Pont **sujet → NAF** irréductiblement **éditorial** (aucune table officielle EuroVoc→NACE) : crosswalk multi-schémas (`SectorMappingRule`, discriminant `Scheme`), conservateur, granularité **division NAF (2 chiffres)** ; flag **`Scope` (Sectoral / Horizontal)** comme levier anti-noyade par **routage, jamais masquage** ; donnée de référence **versionnée dans le repo** (le `git diff` *est* l'audit de curation, comme F-042) ; comportement = service de domaine pur `ISectorClassifier` ; sortie = **`MatchCandidate`** (ADR-014), jamais un verdict. Implémenté par **F-063 / F-064**.

### Clients

- **[ADR-017 — Framework du client web : Blazor Web App](ADR/ADR-017-framework-web-blazor.md)** ✅ *(amendé 30 mai 2026 — WASM pur v1)*
  Client web = **Blazor Web App**, **WASM pur** pour l'app authentifiée v1 ; `Atlas.Web.Client` = consommateur HTTP pur (`Domain` + `Shared` only, NetArchTest). Résout le point laissé ouvert par ADR-007. UX : `docs/14`.
- **[ADR-026 — Clients natifs : MAUI (mobile) + Avalonia (desktop)](ADR/ADR-026-clients-natifs-maui-avalonia.md)** ✅
  Split par force : **mobile (Android/iOS) reste MAUI**, **desktop (Windows/macOS/Linux) passe à Avalonia** (couvre Linux face à la bascule souveraine DINUM), web Blazor WASM inchangé. **Noyau partagé** (Domain + Shared + `AtlasApiClient` + ViewModels + doctrine UX), seules les Views diffèrent ; client = adapter entrant pur (NetArchTest étendu). **Amende ADR-007** ; requalifie **F-010** (desktop → Avalonia). UX agnostique : `docs/12` (delta desktop éventuel comme `docs/14` pour le web).

- **[ADR-027 — Cache client hors-ligne : fraîcheur datée & grammaire d'états honnêtes](ADR/ADR-027-cache-client-offline-fraicheur.md)** ✅
  Le cache mobile (SQLite, acté) mémorise des **read-models datés** (jamais le domaine, aucun secret — topologie pure ADR-002) ; une donnée servie du cache ne se présente **jamais** « à jour » → **`Stale`** (« hors-ligne · vu le {date} »), absente → **`Unavailable`**, **jamais un vide menteur** (grammaire d'états honnêtes ADR-015 étendue à l'axe réseau, comme ADR-022 au temps) ; bandeau global = **état système** (ADR-012), fraîcheur **par-item** ; **lecture seule** v1, **pas de TTL dur** (on date, on n'expire pas), cache **chiffré + purgé à la déconnexion** (INPI jamais caché). Offline web/desktop = avenants futurs. Implémenté par **F-029** (cache de lecture) / **F-075** (moteur de fraîcheur & sync).

### Robustesse & exploitation

- **[ADR-018 — Dégradation gracieuse des sources amont & code d'erreur dédié](ADR/ADR-018-degradation-gracieuse-sources-amont.md)** ✅
  Trois faits jamais confondus (**indisponible** ≠ **vide de couverture** ≠ **problème d'accès**) ; code métier **`inpi.pi_unavailable`** (502) sur `/trademarks*` et `/patents*` classifié dans l'adapter ; dégradation **à portée locale** (`SectionState`, ADR-015) ; rendu client honnête réutilisant le composant Erreur. Pendant *humain* de la doctrine ADR-016. UX : `docs/12 §14`, `docs/14`.
- **[ADR-019 — Serveur de préproduction (alpha & beta) : VPS unique tout-en-un](ADR/ADR-019-serveur-preproduction.md)** ✅
  Préprod = **un seul VPS auto-géré** (API + PostgreSQL + Hangfire in-process + statiques WASM) en docker-compose miroir du harness local (doc 13) ; 2 vCPU / 4 Go ; **région UE** (**Hetzner** pressenti, provider révisable — c'est la région UE, pas la marque, qui fait la conformité) ; always-on derrière reverse proxy TLS ; clé de chiffrement en posture préprod assumée ; `pg_dump` quotidien ; **maintenance par l'auteur, délégation différée** sur critère de bascule explicite. **Ne tranche que la préprod** — le choix de l'hébergeur de **prod** reste ouvert.

- **[ADR-028 — Observabilité du backend (OpenTelemetry, découplage émission/collecte, progressif)](ADR/ADR-028-observabilite-backend.md)** ✅
  Observabilité **technique** (santé infra, ≠ télémétrie produit **F-076**, **sans RGPD**) : instrumenter **complet** (OTel 3 piliers — logs Serilog / métriques / traces) **dès maintenant**, **découpler l'émission du backend de collecte** (léger en préprod sur le VPS d'ADR-019, stack complète sur **machine dédiée** en prod — jamais sur la box surveillée) ; **alerting externe** au système surveillé (uptime check sur **`/health`** agrégé API/DB/Hangfire/sources) ; stockage **auto-hébergé** (souveraineté), seul le ping de vie externalisé ; jamais de secret/donnée sensible dans les logs (scrubbing). **Complète ADR-018** (la dégradation encaisse, l'observabilité signale). Implémenté par **F-077**.

### Approfondissement de la donnée (confiance · temps · documents · graphe)

- **[ADR-021 — Export auditable : intégrité par empreinte + horodatage](ADR/ADR-021-export-auditable-integrite.md)** ✅
  Tout export auditable embarque une **empreinte SHA-256 du contenu canonicalisé + horodatage** (option RFC 3161), **auto-vérifiable** ; granularité **par fait**, états honnêtes inclus, scellement souverain côté serveur. Posture **« trace vérifiable », jamais « preuve légale »**. Prolonge ADR-012 jusqu'à l'artefact d'audit. Implémenté par **F-065 / F-066**.
- **[ADR-022 — Historisation bi-temporelle des entités](ADR/ADR-022-historisation-bitemporelle-entites.md)** ✅
  **Journal de changements append-only** (réutilise les `MonitoredChange` d'ADR-013) + snapshots baseline pour **reconstituer l'état d'une entité à une date** (`asOf`). Bi-temporalité ciblée (temps d'événement vs d'observation), nouvel état `SectionState.Unobserved` pour l'honnêteté temporelle, périmètre = entités suivies. Implémenté par **F-067 / F-068** ; donne un `asOf` à l'export (F-066).
- **[ADR-023 — Extraction documentaire souveraine (actes)](ADR/ADR-023-extraction-documentaire-souveraine.md)** ✅
  Extraction de faits depuis les actes comme **aide à la lecture** : **faits candidats à vérifier, ancrés à la page**, jamais oracle. **OCR + extraction on-infra par défaut** (cloud/BYOAI = premium opt-in), phasage index (F-069) puis extraction (F-070). Prolonge le patron open core ADR-006/009 et la doctrine ADR-012/014.
- **[ADR-024 — Graphe d'écosystème : généralisation multi-arêtes de F-034](ADR/ADR-024-graphe-ecosysteme-multiaretes.md)** ✅
  Généralise F-034 en **graphe multi-arêtes typées** (DECP, co-dépôts PI) sur le **même substrat PostgreSQL borné** (1-2 sauts, pas de Neo4j). Sépare le poids légal entité↔entité (open data léger) des arêtes impliquant une **personne** (cadre DPIA F-034) ; résolution conservatrice nom→SIREN, traversée **consciente des hubs** ; arêtes **descriptives, jamais qualifiantes**. Implémenté par **F-071 / F-072**.

### Agentique & génératif

- **[ADR-025 — Usage agentique génératif : composer & faire remonter, jamais conclure](ADR/ADR-025-usage-agentique-generatif.md)** ✅
  Encadre deux usages génératifs bâtis sur ADR-016 : **règles de surveillance en langage naturel** (étend F-046) et **agent de sourcing « thèse → cibles »** (sur F-052). Principe : l'orchestration **compose et fait remonter, jamais ne conclut** ; **LLM hors de la boucle déterministe** quand possible, **human-in-the-loop lecture-d'abord**, souveraineté **BYOAI** (inférence côté agent de l'utilisateur). Implémenté par **F-073 / F-074**. (Composition inter-produits Atlas ↔ Chronos = futur ADR dédié.)

---

## Décisions à prendre ultérieurement

Les sujets suivants sont **identifiés** mais **non encore tranchés**. Ils seront documentés dans des ADR ultérieurs.

| Sujet | Pourquoi attendre |
|---|---|
| Nom du projet | À figer avant la création du repo |
| Choix de l'hébergeur de **production** (OVHcloud, Scaleway, Clever Cloud, Hetzner...) | **Préprod tranchée par ADR-019** (VPS unique FR/UE) ; **prod reste ouverte** — décision en phase déploiement |
| Outil de paiement (Stripe, Lemon Squeezy, Paddle) | Décision phase 3 (mois 12–24) |
| Stratégie de cache (Redis ? In-memory ?) | Décision en phase architecture détaillée |
| Gestion de queue pour les tâches asynchrones (Hangfire, MassTransit + RabbitMQ) | Décision en phase architecture détaillée |
| Segment client primaire (cabinets compta, avocats, KYC, etc.) | À laisser émerger des early adopters |

---

## Roadmap macro

**Phase 0 — Cadrage (en cours, mai 2026)**
- ✅ Décisions stratégiques
- ✅ Documentation des APIs INPI
- ⏳ Architecture détaillée (hexagone, layout solution .NET)
- ⏳ Vocabulaire ubiquitaire
- ⏳ Choix du repo

**Phase 1 — MVP 1 (mois 1–4)**
- Backend ASP.NET Core fonctionnel
- Auth utilisateurs + connexion compte INPI
- Recherche entreprise par SIREN (RNE)
- Recherche marque par nom (INPI PI)
- Client MAUI mobile + desktop avec ces 2 fonctions
- Repo public, premiers articles techniques

**Phase 2 — MVP 2 (mois 4–8)**
- Téléchargement bilans / actes RNE
- Détail marque + image
- Recherche brevet
- Persistance des recherches / favoris
- **Veille intégrée (feature majeure différenciante)** :
  - Lecteur RSS / Atom natif
  - Templates de veille pré-curés par métier
  - Ajout libre de sources par l'utilisateur
  - Intégration BODACC pour les annonces légales
  - Déduplication et filtres
  - Alertes email et push sur évolutions des favoris
  - Timeline unifiée combinant favoris + sources veille

**Phase 3 — Croissance (mois 8–18)**
- Intégration BODACC (annonces légales)
- Intégration Sirene (compléments INSEE)
- Premiers utilisateurs réels, écoute du feedback
- Lancement de la version SaaS hébergée payante

**Phase 4 — Maturité (mois 18+)**
- Selon adoption : EUIPO/OMPI international, Judilibre, DECP
- Premium features éventuelles
- Premiers clients entreprise

---

## Glossaire

| Terme | Définition |
|---|---|
| **RNE** | Registre National des Entreprises (depuis 2023, remplace RNCS) |
| **PI** | Propriété Industrielle (brevets, marques, dessins & modèles) |
| **SIREN** | Identifiant unique d'une entreprise française (9 chiffres) |
| **SIRET** | Identifiant unique d'un établissement (SIREN + 5 chiffres) |
| **ADR** | Architecture Decision Record |
| **OSS** | Open Source Software |
| **AGPL** | Affero General Public License |
| **CLA** | Contributor License Agreement |
| **Hexagonal / Ports & Adapters** | Pattern d'architecture isolant le domaine métier des détails d'infrastructure |
| **Multi-tenant** | Système où chaque client/user a son propre espace isolé |
| **Use case** | Cas d'utilisation métier (ex. "rechercher une entreprise") |
| **Adapter sortant** | Code qui traduit une demande du domaine vers un système externe (API, BDD) |
| **Adapter entrant** | Code qui traduit une demande externe (HTTP, UI) vers une demande du domaine |
| **Port** | Interface définie par le domaine, implémentée par un adapter |

---

*Document évolutif. Chaque décision est tracée dans son fichier sous [`docs/ADR/`](ADR/) (statut, date, amendements en tête).*
