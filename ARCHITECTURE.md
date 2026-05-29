# Architecture

> Vue d'ensemble de l'architecture d'Atlas. Pour le détail (inventaire des ports, flux complets,
> justification), voir [`docs/09-architecture-detaillee.md`](docs/09-architecture-detaillee.md).
> Pour l'UX du client MAUI : [`docs/12-modele-ux-client-maui.md`](docs/12-modele-ux-client-maui.md).

## Principe

Atlas suit une **architecture hexagonale** (Ports & Adapters) dans sa formulation Clean
Architecture : le **domaine métier** est pur et ne dépend de rien ; il définit des **interfaces
(ports)** que les couches externes **implémentent (adapters)**. Les dépendances pointent toujours
**vers l'intérieur**.

```
       ┌────────────────────────────────────────────────────────────┐
       │  Adapters primaires (entrants)                              │
       │  Atlas.Api (Minimal API)   Atlas.Maui (client)   Atlas.Mcp* │
       └─────────────────┬──────────────────────────────────────────┘
                         │ appelle les use cases
       ┌─────────────────▼──────────────────────────────────┐
       │  Atlas.Application (CQRS, MediatR)                  │
       │  Atlas.Application.Premium (use cases premium)      │
       └─────────────────┬──────────────────────────────────┘
       ┌─────────────────▼──────────────────────────────────┐
       │  Atlas.Domain                                       │
       │  (+ Atlas.Shared : Result<T>, PagedResult<T>)       │
       │  entités, value objects, PORTS, doctrine            │
       └─────────────────▲──────────────────────────────────┘
                         │ implémente les ports
       ┌─────────────────┴──────────────────────────────────────────┐
       │  Adapters secondaires (sortants)                            │
       │  Infrastructure.{Bodacc, Cache, Inpi, Messaging,            │
       │  Persistence, Security, Storage, Veille}                    │
       └────────────────────────────────────────────────────────────┘
```

\* `Atlas.Mcp` est un adapter entrant prévu pour F-052 (serveur MCP) — non encore matérialisé.
Doctrine et architecture de sécurité posées par **ADR-016**.

## Projets et règles de dépendance

Les règles sont **vérifiées automatiquement** par `tests/Atlas.Architecture.Tests` (NetArchTest).
La suite (7 tests) verrouille notamment la séparation cœur open source / premium et l'isolation
hexagonale.

| Projet | Peut référencer |
|---|---|
| `Atlas.Shared` | (rien) |
| `Atlas.Domain` | `Atlas.Shared` |
| `Atlas.Application` | `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Application.Premium` | `Atlas.Application`, `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Infrastructure.*` | `Atlas.Application`, `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Api` | tous les précédents |
| `Atlas.Maui` | **uniquement** `Atlas.Domain` et `Atlas.Shared` |

`Atlas.Maui` ne référence jamais l'infrastructure : le client passe exclusivement par l'API HTTP
via `Atlas.Maui/Services/AtlasApiClient.cs` (le code MAUI est livré sur les terminaux et décompilable).

**Verrous d'archi (NetArchTest)** :

- Le domaine ne dépend ni de l'application, ni de l'infra, ni de l'API, ni de librairies externes
  (EF Core, ASP.NET, MediatR, Npgsql).
- L'application ne dépend ni de l'infra ni de l'API.
- L'infrastructure persistence ne dépend pas de l'API.
- `Atlas.Application` (cœur open source) ne référence **jamais** `Atlas.Application.Premium` —
  ADR-006 / ADR-009 verrouillés structurellement (F-050).
- `Atlas.Application.Premium` ne dépend ni de l'infra ni de l'API.

## Patterns clés

- **`Result<T>` / `Result`** (`Atlas.Shared`) : les erreurs métier sont des valeurs, jamais des
  exceptions. Combinateurs `Map` / `Bind` / `Match` / `Tap` / `Ensure` / `TryGetValue` pour
  enchaîner sans boilerplate, plus une conversion implicite `Error` → `Result` /
  `Result<T>`. `Result<T>.Value` lève `InvalidOperationException` si appelé en état d'échec
  (anti-footgun — préférer `TryGetValue` ou `Match`).
- **Value objects & strongly-typed IDs** : `Siren` (Luhn), `Siret`, `EmailAddress`, `UserId`,
  `DepositNumber`, `NiceClassification`, `FeedRuleId`, `VeillePackId`… encapsulent validation et
  invariants.
- **CQRS léger via MediatR** : chaque interaction est un `IRequest<TResponse>` ; handlers
  `internal sealed` ; `ValidationBehavior` exécute FluentValidation avant le handler (échec →
  `Result`, sans exception).
- **Ports & adapters** : ports secondaires définis dans `Atlas.Domain`, implémentés dans
  `Atlas.Infrastructure.*`.
- **Notifications MediatR** : `INotification` + multi-handlers pour les flux *fire-and-forget*
  (alertes email + push F-019, règle de surveillance matchée F-046, signalement social F-049).
- **Domain events** : `IDomainEvent` (hérite de `MediatR.INotification` — seule
  `MediatR.Contracts` est tirée par `Atlas.Domain`). Les entités lèvent via
  `RaiseDomainEvent(...)` ; `AtlasDbContext.SaveChangesAsync` collecte les events depuis le
  ChangeTracker, commit la transaction, puis publie via `IPublisher` (sémantique
  after-commit — aucun event publié si la transaction échoue).

## Doctrine architecturale (ADR-001 → ADR-016)

Les décisions architecturales sont énumérées dans [`docs/01-decisions-architecturales.md`](docs/01-decisions-architecturales.md).

**Fondations** : ADR-001 SaaS multi-utilisateur, ADR-002 topologie hybride, ADR-003 auth INPI
multi-tenant, ADR-004 hexagonale platform-ready, ADR-005 AGPL v3, ADR-006 modèle économique
*open core*, ADR-007 stack .NET / MAUI, ADR-008 accessibilité WCAG 2.2 AA bloquante (Definition
of Done), ADR-009 veille comme feature majeure différenciante.

**Authentification et délégation** : ADR-010 auth utilisateur hexagonale custom (JWT RS256 +
refresh rotatif + 2FA TOTP), ADR-011 OAuth 2.1 / OpenIddict pour la délégation agentique (mise
en œuvre différée à F-052).

**Doctrine descriptive** : ADR-012 cadre de réutilisation des données de dirigeants et du graphe
relationnel (descriptif, jamais de verdict, DPIA obligatoire en prérequis de F-034).

**Substrat et matching** : ADR-013 substrat de surveillance des entités (deux stratégies —
`IStateMonitor<TState>` pour les retraits détectés et `IItemStreamMonitor<TItem>` pour les flux
append-only — + runner mutualisé, extraction à la troisième instance F-057). ADR-014 matching
conservateur unifié (`MatchCandidate` encode la doctrine ADR-012 dans le type — état illégal
*irreprésentable* —, noyau de normalisation des noms partagé par les 3 matchers à base de noms,
F-026 hors moteur).

**Assemblage et surface agentique** : ADR-015 couche d'assemblage du dossier entreprise
(read-model `CompanyDossier` à sections auto-descriptives, 5 états `SectionState` porteurs de
doctrine, résolution snapshot-first réutilisant ADR-013, backbone de F-056). ADR-016 sécurité &
doctrine de la surface agentique MCP (surface curée lecture-d'abord, doctrine inline avec
donnée, contenu externe = donnée jamais instruction, délégation utilisateur, credentials ne
traversent jamais).

## Sous-systèmes par sous-domaine

### Identité & sécurité

- `Atlas.Infrastructure.Security` — Argon2id (mots de passe), AES-256-GCM (`ICryptoService`),
  JWT RS256 (`JwtIssuer`), TOTP (`TotpProvider`), challenge 2FA (audience séparée `atlas-2fa`).
- Anti-SSRF dans `FeedSubscriptionPolicy` (Lot 2a) — bloque les IP privées, loopback,
  link-local et endpoints de métadonnées cloud.
- Rate limiting ASP.NET Core (global 100/60 s + `auth-strict` 10/60 s).
- En-têtes de sécurité (`SecurityHeadersMiddleware`) + CORS strict (jamais `*`).
- Logging Serilog + `SensitiveDataMaskingEnricher`.

### Données entreprises et PI

- `Atlas.Infrastructure.Inpi` — RNE (token Bearer + retry 401 unifié), PI marques (XSRF +
  cookies), PI brevets (recherche SolR + détail par numéro de publication).
- Téléchargement individuel d'actes / bilans (F-013), téléchargement en masse asynchrone
  (F-014) via `IFileStorage` et `Atlas.Infrastructure.Storage` (adapter `LocalFileStorage`).
- Favoris (`CompanyFavorite`, `TrademarkFavorite`, `PatentFavorite`) avec cascade FK RGPD.
- Surveillance favoris (F-019) : `CompanyFavoriteSnapshot` + `DiffWith` + job Hangfire
  `favorite-refresh` → `CompanyFavoriteChangedNotification` → 2 handlers (email + push). C'est
  la première instance du patron `IStateMonitor<UniteLegale>` formalisé par ADR-013.

### Veille et signaux

- `Atlas.Infrastructure.Veille` — provider RSS/Atom (CodeHollow.FeedReader), policy
  d'abonnement anti-SSRF (`IFeedSubscriptionPolicy`), policy de déduplication
  (`IDeduplicationPolicy`, SimHash 64 bits).
- `Atlas.Infrastructure.Bodacc` — `OpendatasoftBodaccProvider` (anonyme, pas d'INPI requis).
  Job Hangfire `bodacc-polling` avec dédup cross-users (1 appel API par SIREN partagé).
  Première instance du patron `IItemStreamMonitor<BodaccAnnouncement>` formalisé par ADR-013.
- Timeline mixte unifiée (F-044 + F-047) : RSS + RNE (F-019) + BODACC (F-048). Tag des items
  RSS sur favoris via `INameInTextMatcher` (volet 1 de F-047, première instance d'ADR-014).
- Catalogue de packs de veille curés (F-042, `VeillePack` versionné), abonnements libres
  (F-043, `VeilleSubscription`), états utilisateur (`FeedItemUserState`).
- **F-049 marketplace** : `VeillePack` étendu avec `AuthorUserId` + `Visibility` (System /
  Private / Public) + `LikesCount`. `VeillePackLike` (composite unique user × pack) et
  `VeillePackReport` (workflow *report &amp; review*). 8 endpoints sous `/veille/packs/*`.
- **F-046 règles de surveillance** : entité `FeedRule` (critères AND keyword / source / SIREN
  mentionné + actions email / push), évaluation incrémentale par watermark `LastEvaluatedAt`.
  `EvaluateFeedRulesCommand` chaîné dans `FeedPollingJob.PollAsync()` après le matching favoris.

### Notifications

- `Atlas.Infrastructure.Messaging` — port `IEmailSender` et port `INotificationDispatcher`.
- **Adapter email Brevo** (`BrevoEmailSender`, F-001 / F-019 / F-046) : HttpClient typé
  `POST https://api.brevo.com/v3/smtp/email` avec header `api-key`, switch DI sur
  `Email:Brevo:ApiKey`, fallback `LoggingEmailSender` (dev).
- **Push multi-plateformes** (F-020) : `CompositeNotificationDispatcher` qui fan-out vers
  `FcmNotificationDispatcher` (Android + Web Push, OAuth2 service account),
  `ApnsNotificationDispatcher` (iOS + macOS, JWT ES256 HTTP/2), `WnsNotificationDispatcher`
  (Windows desktop, OAuth2 `client_credentials` + Toast XML). Cleanup auto des tokens morts
  (404/410 selon plateforme). Bascule DI conditionnelle : sans plateforme configurée →
  fallback `LoggingNotificationDispatcher` (dev).

### Cache et stockage

- `Atlas.Infrastructure.Cache` — `IMemoryCache` (dev) / Redis (prod) selon DI.
- `Atlas.Infrastructure.Storage` — `IFileStorage` (`LocalFileStorage` actuel, adapter S3
  prévu pour F-014 v2).

### Persistance

- `Atlas.Infrastructure.Persistence` — `AtlasDbContext` (EF Core 10 + Npgsql), migrations,
  `IUnitOfWork = AtlasDbContext`. Cascades FK RGPD systématiques (art. 17) sur toutes les
  données utilisateur. Hangfire utilise le même PostgreSQL.

## Couche premium et substrat ADR-013 (en attente d'implémentation)

`Atlas.Application.Premium` est matérialisé via une classe `AssemblyMarker` publique. Les
**3 ports premium** sont déclarés côté domaine dans `Atlas.Domain.Veille.Premium` :
`IFeedItemEnricher` (+ record `FeedItemEnrichment`), `IFeedRelevanceScorer` (score 0-100 par
item × user), `IFeedSummarizer` (synthèse narrative — patron pour `IFinancialSummarizer` de
F-054).

Le **runner mutualisé** d'ADR-013 et le contrat de doctrine `MatchCandidate` d'ADR-014 ne
sont pas encore extraits ; cela arrivera lors de la construction de F-057 (3ᵉ instance) en
refactorant F-019 et F-048 sur le runner partagé.

## Flux type — recherche entreprise

```
Atlas.Api  GET /companies/{siren}
   └─► MediatR  GetCompanyBySirenQuery
          └─► GetCompanyBySirenHandler (Application)
                 ├─ Siren.Create (validation Luhn, domaine)
                 ├─ IInpiCredentialsRepository → ICryptoService (déchiffre les identifiants INPI)
                 ├─ ICompanyDataProvider.GetBySirenAsync (port domaine)
                 │     └─► RneCompanyProvider (Infrastructure.Inpi) → INPI RNE (token Bearer caché)
                 └─ mappe UniteLegale → CompanyDto, publie SearchPerformedNotification (historique)
```

## Flux type — règle de surveillance déclenchée (F-046)

```
Hangfire  FeedPollingJob (cron 30 min)
   └─► PollFeedSourcesCommand           (Application.Veille.PollFeedSources)
   └─► ClusterPendingFeedItemsCommand   (F-045, déduplication SimHash)
   └─► MatchFavoritesInFeedItemsCommand (F-047 volet 1, INameInTextMatcher)
   └─► EvaluateFeedRulesCommand         (F-046, watermark incrémental)
          └─► EvaluateFeedRulesHandler (Application)
                 ├─ IFeedRuleRepository.ListActiveAsync
                 ├─ IFeedItemRepository.ListFetchedSinceAsync(rule.EvaluationWatermark)
                 ├─ IFeedItemFavoriteMatchRepository (mentions par item × user)
                 ├─ rule.Matches(item, mentionedSirens)  ← Domain (FeedRule.Matches)
                 ├─ rule.RegisterEvaluation / RegisterTrigger
                 └─ publish FeedRuleMatchedNotification
                       ├─► SendFeedRuleMatchedEmailHandler → IEmailSender.SendFeedRuleMatchedAsync
                       │                                      → BrevoEmailSender (POST /v3/smtp/email)
                       └─► DispatchFeedRuleMatchedPushHandler → INotificationDispatcher
                                                                 → CompositeNotificationDispatcher
                                                                    → FCM + APNs + WNS (selon config)
```

## Sécurité (rappel)

Argon2id (mots de passe), AES-256-GCM (identifiants INPI au repos, jamais loggés), JWT RS256 +
refresh rotatif, 2FA TOTP avec audience séparée, RGPD (export / effacement avec cascade FK
systématique). Surface agentique MCP couverte par ADR-016 (à matérialiser dans `Atlas.Mcp` lors
de F-052). Détail : [`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md) et
[`SECURITY.md`](SECURITY.md).

## Client MAUI

`Atlas.Maui` est un projet unique multi-cible (Android, iOS, Windows, macOS) qui consomme
exclusivement l'API HTTP via `AtlasApiClient`. La doctrine UX et de navigation est posée par
[`docs/12-modele-ux-client-maui.md`](docs/12-modele-ux-client-maui.md) — 6 règles du modèle
adaptatif, 5 destinations plafonnées (Accueil / Recherche / Veille / Favoris / Profil),
list-detail récursif, divulgation progressive jamais d'amputation, pertinence avant exhaustivité.

**Kit de thèmes** : 7 thèmes × 2 modes = 14 palettes WCAG 2.2 AA dans
`src/Atlas.Maui/Resources/Themes/`, service `ThemeManager` dans `src/Atlas.Maui/Theming/`,
initialisé au démarrage depuis `App.xaml.cs`. Source de génération conservée dans
`docs/atlas-themes-kit/atlas-themes/source/` (chaîne `palettes.py → generate.py`).

## Stack

.NET 10 (C# 13, file-scoped namespaces, primary constructors) · ASP.NET Core 10 (Minimal APIs) ·
EF Core 10 + PostgreSQL (Npgsql) · MediatR 13 · FluentValidation 12 · Polly 8 · Serilog 9 +
OpenTelemetry · Hangfire (PostgreSQL storage) · QuestPDF (rapports F-022) · .NET MAUI 10 ·
MkDocs Material (doc). Versions centralisées dans `Directory.Packages.props`.

## Tests

- **Unitaires** : `Atlas.Domain.UnitTests` (100 tests), `Atlas.Application.UnitTests` (166 tests),
  `Atlas.Infrastructure.Security.UnitTests`, `Atlas.Infrastructure.Messaging.UnitTests` (41 tests
  dont 6 Brevo) — NSubstitute, FluentAssertions.
- **Architecture** : `Atlas.Architecture.Tests` (7 tests — règles de dépendance hexagonale +
  séparation cœur / premium).
- **Intégration** : `Atlas.Infrastructure.Persistence.IntegrationTests` (PostgreSQL via
  Testcontainers), `Atlas.Infrastructure.Veille.IntegrationTests`,
  `Atlas.Infrastructure.Inpi.IntegrationTests` (WireMock), `Atlas.Api.IntegrationTests`
  (`WebApplicationFactory` + Postgres + WireMock, flux end-to-end).
