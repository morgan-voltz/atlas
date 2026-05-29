# Architecture détaillée — Backend Atlas

> Architecture technique détaillée du backend Atlas, suivant le pattern **Ports & Adapters (Hexagonal Architecture)** d'Alistair Cockburn, dans sa formulation moderne dite **Clean Architecture** (Robert C. Martin).
> Ce document spécifie les couches, les ports, les adapters, les règles de dépendance et les flows typiques.

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026
**Nom de code projet** : Atlas (provisoire)

---

## Sommaire

- [1. Principes et fondations](#1-principes-et-fondations)
- [2. Vue d'ensemble — l'hexagone d'Atlas](#2-vue-densemble--lhexagone-datlas)
- [3. Les 4 couches en détail](#3-les-4-couches-en-détail)
- [4. La règle d'or des dépendances](#4-la-règle-dor-des-dépendances)
- [5. Inventaire des ports secondaires (sortants)](#5-inventaire-des-ports-secondaires-sortants)
- [6. Inventaire des ports primaires (entrants)](#6-inventaire-des-ports-primaires-entrants)
- [7. Inventaire des adapters secondaires](#7-inventaire-des-adapters-secondaires)
- [8. Inventaire des adapters primaires](#8-inventaire-des-adapters-primaires)
- [9. Flow bout en bout — Rechercher une entreprise par SIREN](#9-flow-bout-en-bout--rechercher-une-entreprise-par-siren)
- [10. Cross-cutting concerns](#10-cross-cutting-concerns)
- [11. Patterns techniques retenus](#11-patterns-techniques-retenus)
- [12. Tests d'architecture](#12-tests-darchitecture)

---

## 1. Principes et fondations

### 1.1 Pourquoi l'architecture hexagonale

Rappel des fondements (décidés dans l'ADR-004) :

L'**architecture hexagonale** isole le **domaine métier** des détails techniques (frameworks, BDD, APIs externes, UI). Le domaine ne sait rien de qui l'appelle ou de ce qu'il appelle ; il définit des **interfaces (ports)** et les autres couches **les implémentent (adapters)**.

Les bénéfices concrets pour Atlas :

- **Substituabilité** : si l'INPI change ses API en V2, on remplace l'adapter ; le domaine ne bouge pas.
- **Testabilité** : on teste les use cases avec des **mocks** des ports, sans réseau ni BDD.
- **Évolutivité** : ajouter BODACC, Sirene, EUIPO = ajouter des adapters. Zéro modification du domaine.
- **Indépendance technologique** : si on migre de PostgreSQL vers autre chose, idem.

### 1.2 Vocabulaire des couches

Quatre couches s'empilent dans Atlas, de la plus abstraite (au centre) à la plus concrète (en périphérie) :

```
                        ┌──────────────────────────┐
                        │   ADAPTERS PRIMAIRES     │  ← UI, API, CLI, MAUI
                        │  (entrants / driving)    │
                        └───────────┬──────────────┘
                                    │ appellent
                        ┌───────────▼──────────────┐
                        │      APPLICATION         │  ← Use cases, orchestration
                        │  (use cases, services)   │
                        └───────────┬──────────────┘
                                    │ utilise
                        ┌───────────▼──────────────┐
                        │         DOMAIN           │  ← Cœur, intouché
                        │ (entities, VO, ports)    │
                        └───────────┬──────────────┘
                                    │ ports implémentés par
                        ┌───────────▼──────────────┐
                        │  ADAPTERS SECONDAIRES    │  ← INPI, DB, Cache, Email
                        │  (sortants / driven)     │
                        └──────────────────────────┘
```

- **Domain** : le cœur pur. Entités (`Company`, `Trademark`...), value objects (`Siren`, `Naf`...), interfaces de ports.
- **Application** : les use cases. Implémentent la logique métier en orchestrant le domaine et les ports.
- **Adapters primaires** (entrants) : ce qui appelle l'application. Web API, MAUI, CLI futurs.
- **Adapters secondaires** (sortants) : ce que l'application appelle vers l'extérieur. APIs INPI, BDD, Email, etc.

### 1.3 Ports primaires vs secondaires

- **Port primaire (entrant)** : interface définie côté **Application**, appelée par les adapters primaires. Définit ce que l'application *expose* (ses use cases).
- **Port secondaire (sortant)** : interface définie côté **Domain**, implémentée par les adapters secondaires. Définit ce que l'application *attend* du monde extérieur.

**Mnémonique** : `Primaire = qui pilote l'app` / `Secondaire = piloté par l'app`.

---

## 2. Vue d'ensemble — l'hexagone d'Atlas

Le schéma complet de l'architecture (ASCII art, fidèle au pattern hexagonal) :

```
                              CÔTÉ ENTRANT (DRIVING)
        ┌─────────────────────────────────────────────────────────────┐
        │                                                             │
        │   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐    │
        │   │   MAUI App   │   │   Web API    │   │   Future CLI │    │
        │   │ (iOS/Android │   │  (ASP.NET    │   │   Atlas.Cli  │    │
        │   │ /Win/macOS)  │   │   Core)      │   │              │    │
        │   └──────┬───────┘   └──────┬───────┘   └──────┬───────┘    │
        │          │ HTTP/JSON        │ HTTP/JSON        │            │
        │          ▼                  ▼                  ▼            │
        │   ┌──────────────────────────────────────────────────────┐  │
        │   │           PORTS PRIMAIRES (Use cases)                │  │
        │   │   ICommand<T>, IQuery<T>, IRequestHandler<>          │  │
        │   └──────────────────────────────────────────────────────┘  │
        └─────────────────────────────┬───────────────────────────────┘
                                      │
                                      ▼
        ╔══════════════════════════════════════════════════════════════╗
        ║                       APPLICATION LAYER                       ║
        ║       (Use Cases, Handlers, Application Services)             ║
        ║                                                               ║
        ║   ┌─────────────────────────────────────────────────────┐    ║
        ║   │  GetCompanyBySirenHandler                            │    ║
        ║   │  SearchTrademarksHandler                             │    ║
        ║   │  AddCompanyToFavoritesHandler                        │    ║
        ║   │  AggregateFeedItemsHandler                           │    ║
        ║   │  ... (n use cases)                                   │    ║
        ║   └─────────────────────────────────────────────────────┘    ║
        ║                                                               ║
        ╚══════════════════════════════╤═══════════════════════════════╝
                                       │ utilise (interfaces uniquement)
                                       ▼
        ╔══════════════════════════════════════════════════════════════╗
        ║                          DOMAIN LAYER                         ║
        ║                                                               ║
        ║   ┌────────────────────────┐  ┌──────────────────────────┐   ║
        ║   │  ENTITIES              │  │  VALUE OBJECTS           │   ║
        ║   │  Company, Trademark,   │  │  Siren, Siret, Naf,      │   ║
        ║   │  Patent, User,         │  │  NiceClassification,     │   ║
        ║   │  UniteLegale,          │  │  Address, TvaNumber,     │   ║
        ║   │  FeedItem, ...         │  │  DepositNumber, ...      │   ║
        ║   └────────────────────────┘  └──────────────────────────┘   ║
        ║                                                               ║
        ║   ┌────────────────────────┐  ┌──────────────────────────┐   ║
        ║   │  DOMAIN SERVICES       │  │  PORTS SECONDAIRES       │   ║
        ║   │  SirenValidator,       │  │  ICompanyDataProvider,   │   ║
        ║   │  TrademarkMatcher,     │  │  IIntellectualProperty   │   ║
        ║   │  FeedItemDeduplicator  │  │  Provider,               │   ║
        ║   │                        │  │  IExternalContentSource, │   ║
        ║   └────────────────────────┘  │  ICompanyRepository,     │   ║
        ║                                │  IUserRepository, ...    │   ║
        ║                                └──────────────────────────┘   ║
        ╚═══════════════════════════════╤══════════════════════════════╝
                                        │ implémentés par
                                        ▼
        ┌──────────────────────────────────────────────────────────────┐
        │                  CÔTÉ SORTANT (DRIVEN)                       │
        │                                                              │
        │  ┌───────────────────────────────────────────────────────┐  │
        │  │              ADAPTERS SECONDAIRES                      │  │
        │  │                                                        │  │
        │  │  ┌─────────────────┐  ┌─────────────────────────┐    │  │
        │  │  │ Atlas.Infra     │  │  Atlas.Infra            │    │  │
        │  │  │ .Inpi           │  │  .Persistence           │    │  │
        │  │  │                 │  │                         │    │  │
        │  │  │ InpiRneCompany  │  │  CompanyRepository      │    │  │
        │  │  │ Provider        │  │  (EF Core + Postgres)   │    │  │
        │  │  │ InpiPiTrademark │  │  UserRepository         │    │  │
        │  │  │ Provider        │  │  FavoriteRepository     │    │  │
        │  │  │ InpiAuth        │  │                         │    │  │
        │  │  │ Provider        │  │                         │    │  │
        │  │  └─────────────────┘  └─────────────────────────┘    │  │
        │  │                                                        │  │
        │  │  ┌─────────────────┐  ┌─────────────────────────┐    │  │
        │  │  │ Atlas.Infra     │  │  Atlas.Infra            │    │  │
        │  │  │ .Veille         │  │  .Messaging             │    │  │
        │  │  │                 │  │                         │    │  │
        │  │  │ RssFeedProvider │  │  EmailSender (Brevo)    │    │  │
        │  │  │ BodaccProvider  │  │  PushSender (FCM/APNs)  │    │  │
        │  │  └─────────────────┘  └─────────────────────────┘    │  │
        │  │                                                        │  │
        │  │  ┌─────────────────┐  ┌─────────────────────────┐    │  │
        │  │  │ Atlas.Infra     │  │  Atlas.Infra            │    │  │
        │  │  │ .Security       │  │  .Cache                 │    │  │
        │  │  │                 │  │                         │    │  │
        │  │  │ KmsKeyProvider  │  │  RedisCache             │    │  │
        │  │  │ JwtIssuer       │  │  InMemoryCache (dev)    │    │  │
        │  │  └─────────────────┘  └─────────────────────────┘    │  │
        │  └───────────────────────────────────────────────────────┘  │
        │                                                              │
        │      ┌──────┐  ┌──────┐  ┌────────┐  ┌─────────┐            │
        │      │ INPI │  │BODACC│  │Postgres│  │  Brevo  │  ...       │
        │      │ APIs │  │ API  │  │  + EF  │  │ (email) │            │
        │      └──────┘  └──────┘  └────────┘  └─────────┘            │
        └──────────────────────────────────────────────────────────────┘
```

**Comment lire ce schéma** :

- Le **DOMAIN** au centre est le cœur. Il ne dépend de rien.
- L'**APPLICATION** dépend uniquement du domaine.
- Les **adapters primaires** (en haut) appellent les use cases via leurs ports primaires.
- Les **adapters secondaires** (en bas) implémentent les ports secondaires définis par le domaine.
- Les **systèmes externes** (INPI, BODACC, Postgres...) sont accessibles uniquement via les adapters secondaires.

---

## 3. Les 4 couches en détail

### 3.1 Domain (Atlas.Domain)

**Rôle** : représenter le métier de manière pure, indépendamment de toute technologie.

**Contenu** :

- **Entities** : objets avec une identité dans le temps. `Company`, `UniteLegale`, `User`, `Trademark`, `Patent`, `FeedItem`, etc.
- **Value objects** : objets immuables sans identité. `Siren`, `Siret`, `Naf`, `Address`, `DepositNumber`, etc.
- **Domain services** : logique métier qui ne rentre pas dans une entité. `SirenValidator`, `FeedItemDeduplicator`, `TrademarkPhoneticMatcher`.
- **Aggregates** : ensembles cohérents d'entités/VO traités comme une unité (ex. `UniteLegale` agrège ses `Etablissement`).
- **Domain events** : faits métier (ex. `CompanyAddedToFavoritesEvent`).
- **Domain errors** : erreurs typées du métier (`InvalidSirenError`, `UnauthorizedAccessError`).
- **Ports secondaires** : interfaces que le domaine définit pour ses besoins (`ICompanyDataProvider`, `ICompanyRepository`).

**Dépendances autorisées** :
- AUCUNE dépendance externe sauf : libs de base .NET (`System.*`)
- Quelques libs utilitaires pures acceptables : `MediatR.Contracts` (pour les interfaces de commands/queries), `FluentValidation.Abstractions`.
- **Interdit** : `Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore`, `HttpClient`, `System.Net.Http`, `Newtonsoft.Json` (préférer `System.Text.Json` si vraiment nécessaire mais éviter même ça).

**Test** : tout test du domaine est un test unitaire pur, sans I/O, sans mock complexe.

### 3.2 Application (Atlas.Application)

**Rôle** : orchestrer le domaine pour réaliser des use cases métier.

**Contenu** :

- **Commands** : intentions de modification (`AddCompanyToFavoritesCommand`).
- **Queries** : intentions de lecture (`GetCompanyBySirenQuery`).
- **Handlers** : implémentent les use cases (`AddCompanyToFavoritesHandler`).
- **DTOs** : objets de transfert entrée/sortie (`CompanyDto`, `TrademarkSummaryDto`).
- **Application services** : logique transverse aux use cases.
- **Validators** : règles de validation des inputs (FluentValidation).
- **Mappers** : conversion entre entités domaine et DTOs.

**Dépendances autorisées** :
- `Atlas.Domain` (obligatoire)
- `MediatR`, `FluentValidation`, `AutoMapper` (ou Mapster) — libs purement applicatives
- `Microsoft.Extensions.DependencyInjection.Abstractions`

**Interdit** :
- Dépendances vers les couches Infrastructure ou Api/MAUI
- HttpClient, EF Core, ASP.NET Core

### 3.3 Application.Premium (Atlas.Application.Premium)

**Rôle** : isolation des use cases premium (cf. ADR-009).

**Contenu** :
- Vide en MVP 2 (juste structure)
- Plus tard : use cases IA (résumés, scoring, synthèse), collaboration équipe
- Les ports correspondants (`IFeedSummarizer`, `IFeedRelevanceScorer`) sont définis dans `Atlas.Domain` ; leurs use cases dans `Atlas.Application.Premium` ; leurs adapters dans `Atlas.Infrastructure.Premium`

### 3.4 Infrastructure (Atlas.Infrastructure.*)

**Rôle** : implémenter les ports secondaires en utilisant les technologies concrètes.

**Découpage en projets séparés** (par responsabilité) :

| Projet | Contenu |
|---|---|
| `Atlas.Infrastructure.Inpi` | Adapters vers les API INPI (RNE, PI, BOPI) |
| `Atlas.Infrastructure.Persistence` | EF Core + PostgreSQL, repositories |
| `Atlas.Infrastructure.Veille` | Adapters RSS, BODACC, agrégation flux |
| `Atlas.Infrastructure.Messaging` | Email, push, webhooks |
| `Atlas.Infrastructure.Security` | KMS, JWT, hashage Argon2id |
| `Atlas.Infrastructure.Cache` | Redis, in-memory |
| `Atlas.Infrastructure.Storage` | Stockage de fichiers (S3-compatible) |
| `Atlas.Infrastructure.External` (futur) | Autres adapters externes (Sirene, EUIPO, etc.) |

**Dépendances autorisées** :
- `Atlas.Domain` et `Atlas.Application` (pour DI registration)
- Toutes les libs techniques nécessaires : HttpClient, EF Core, Polly, etc.

### 3.5 Api (Atlas.Api)

**Rôle** : adapter primaire exposant l'application en HTTP/REST.

**Contenu** :
- Endpoints (Minimal APIs ou Controllers)
- Middlewares (auth, logging, error handling)
- Configuration DI, Swagger
- Filtres de validation
- Programme d'entrée

### 3.6 Maui (Atlas.Maui)

**Rôle** : adapter primaire pour iOS, Android, Windows, macOS.

**Contenu** :
- Views (XAML)
- ViewModels (CommunityToolkit.Mvvm)
- Services HTTP clients (vers Atlas.Api)
- Storage local (SQLite pour offline)
- Notifications push

**Dépendance** :
- `Atlas.Domain` (pour les value objects et entités partagées)
- **PAS** sur `Atlas.Application` ou `Atlas.Infrastructure` (sécurité)

### 3.7 Shared (Atlas.Shared)

**Rôle** : code utilitaire partagé entre tous les projets (constantes, exceptions techniques, helpers).

**Contenu minimal** :
- Types techniques génériques (`Result<T>`, `PagedResult<T>`)
- Constantes globales
- Extensions méthodes utilitaires

**Règle** : pas de logique métier ici. Tout ce qui est métier va dans Domain.

---

## 4. La règle d'or des dépendances

C'est la règle qui rend l'architecture vivante et maintenable. Elle se résume en une phrase :

> **Les dépendances pointent toujours vers l'intérieur.**

Schéma des dépendances autorisées :

```
        Atlas.Maui ────────────────┐
                                    │
        Atlas.Api ──────────────────┤
                                    │
                                    ▼
                          ┌────────────────────┐
                          │  Atlas.Application │ ──┐
                          │  (+ .Premium)      │   │
                          └─────────┬──────────┘   │
                                    │              │
                                    ▼              │
                          ┌────────────────────┐   │
                          │   Atlas.Domain     │ ◀─┘
                          └────────────────────┘
                                    ▲
                                    │ (références uniquement)
                                    │
                          ┌─────────┴──────────┐
                          │ Atlas.Infrastructure│
                          │ .Inpi, .Persistence,│
                          │ .Veille, etc.       │
                          └────────────────────┘
```

### 4.1 Règles strictes

1. **`Atlas.Domain` ne référence rien d'autre que `Atlas.Shared`** (et libs de base .NET).
2. **`Atlas.Application` ne référence que `Atlas.Domain` et `Atlas.Shared`**. Pas d'Infrastructure, pas d'Api, pas de Maui.
3. **`Atlas.Infrastructure.*` peut référencer `Atlas.Domain` et `Atlas.Application`** (pour implémenter les ports + DI registration).
4. **`Atlas.Api` peut référencer Application + Domain + Infrastructure** (composition root, où tout est câblé).
5. **`Atlas.Maui` peut référencer Domain + Shared** uniquement (pas Application, pas Infrastructure — sécurité côté client).

### 4.2 La règle expliquée pour les débutants

Imagine que tu peux **supprimer** n'importe quel projet de la liste sans casser ceux qui sont plus à l'intérieur. Si tu supprimes `Atlas.Infrastructure.Inpi`, est-ce que `Atlas.Application` compile encore ? **Oui**, parce qu'elle ne référence que les ports (interfaces) dans `Atlas.Domain`. C'est ça, le test ultime de la règle d'or.

### 4.3 Comment on force cette règle

- **Visual Studio / Rider** te laissent ajouter une référence circulaire ou interdite — tu dois te discipliner
- **Outil obligatoire** : `NetArchTest` ou `ArchUnitNET` (cf. doc 01 ADR-007) qui écrit ces règles comme des **tests automatisés**

Exemple de test d'architecture :
```csharp
[Fact]
public void Domain_should_not_depend_on_Application()
{
    var result = Types.InAssembly(typeof(Company).Assembly)
        .Should()
        .NotHaveDependencyOn("Atlas.Application")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

---

## 5. Inventaire des ports secondaires (sortants)

Définis dans `Atlas.Domain`, implémentés par `Atlas.Infrastructure.*`. Liste exhaustive pour MVP 1 et MVP 2.

### 5.1 Persistance (CRUD entités)

```csharp
namespace Atlas.Domain.Companies;

public interface ICompanyRepository
{
    Task<Company?> GetBySirenAsync(Siren siren, CancellationToken ct = default);
    Task<Company?> GetByIdAsync(CompanyId id, CancellationToken ct = default);
    Task AddAsync(Company company, CancellationToken ct = default);
    Task UpdateAsync(Company company, CancellationToken ct = default);
    Task<PagedResult<Company>> SearchAsync(CompanySearchSpec spec, CancellationToken ct = default);
}
```

| Port | Rôle |
|---|---|
| `IUserRepository` | CRUD utilisateurs |
| `IAccountRepository` | CRUD comptes |
| `ICompanyRepository` | CRUD entreprises (cache local des fiches consultées) |
| `ITrademarkRepository` | CRUD marques |
| `IPatentRepository` | CRUD brevets |
| `IFavoriteRepository` | CRUD favoris |
| `ISearchHistoryRepository` | Historique de recherches |
| `IFeedSourceRepository` | Sources de veille suivies |
| `IFeedItemRepository` | Items de veille |
| `IVeilleSubscriptionRepository` | Abonnements à des VeillePacks |
| `IUnitOfWork` | Pattern Unit of Work pour transactions multi-repos |

### 5.2 Sources externes (lecture de données métier)

```csharp
namespace Atlas.Domain.Companies;

public interface ICompanyDataProvider
{
    Task<UniteLegale?> GetCompanyBySirenAsync(
        Siren siren, 
        InpiCredentials credentials, 
        CancellationToken ct = default);
    
    Task<PagedResult<UniteLegale>> SearchCompaniesAsync(
        CompanySearchQuery query, 
        InpiCredentials credentials, 
        CancellationToken ct = default);
}
```

| Port | Rôle |
|---|---|
| `ICompanyDataProvider` | Lecture entreprises depuis source externe (INPI RNE) |
| `IIntellectualPropertyProvider` | Lecture PI (marques, brevets, D&M) |
| `IDocumentDownloader` | Téléchargement de documents (bilans, actes, fascicules) |
| `IInpiAuthenticationProvider` | Authentification auprès des API INPI (gestion tokens) |
| `IExternalContentSource` | Lecture de flux (RSS, BODACC, BOPI) — port pour la veille |

### 5.3 Sécurité

| Port | Rôle |
|---|---|
| `IPasswordHasher` | Hash et vérification (Argon2id) |
| `ITotpProvider` | Génération et vérification TOTP (2FA) |
| `IJwtIssuer` | Émission de JWT |
| `IJwtValidator` | Validation de JWT |
| `IKmsKeyProvider` | Accès aux clés du KMS pour chiffrement des credentials INPI |
| `ICryptoService` | Chiffrement / déchiffrement (AES-256-GCM) |

### 5.4 Communication

| Port | Rôle |
|---|---|
| `IEmailSender` | Envoi email transactionnel |
| `IPushNotificationSender` | Envoi push mobile (FCM/APNs) |
| `IWebhookDispatcher` (V3+) | Envoi webhooks aux integrations user |

### 5.5 Infrastructure technique

| Port | Rôle |
|---|---|
| `ICache<T>` | Cache générique typé |
| `IFileStorage` | Stockage de fichiers (PDF documents, exports) |
| `IDateTimeProvider` | Horloge système (testable — utile pour les tests) |
| `IEventBus` | Publication d'événements de domaine (in-process en MVP, message broker plus tard) |
| `ILogger` (déjà fourni par .NET) | Logging structuré |

### 5.6 Veille (cluster F-041 à F-050)

| Port | Rôle |
|---|---|
| `IExternalContentSource` | Source de flux (RSS, BODACC, etc.) — déjà cité |
| `IFeedItemDeduplicator` | Algo de déduplication (peut avoir plusieurs impls) |
| `IVeillePackCatalog` | Catalogue des packs pré-curés (lecture en MVP, gestion plus tard) |

### 5.7 Ports premium (vides en MVP 2, prévus pour V3+)

```csharp
namespace Atlas.Domain.Premium;

public interface IFeedSummarizer
{
    Task<string> SummarizeAsync(FeedItem item, CancellationToken ct = default);
}

public interface IFeedRelevanceScorer
{
    Task<RelevanceScore> ScoreAsync(
        FeedItem item, 
        UserContext context, 
        CancellationToken ct = default);
}
```

---

## 6. Inventaire des ports primaires (entrants)

Définis dans `Atlas.Application`, appelés par les adapters primaires (`Atlas.Api`, `Atlas.Maui`).

### 6.1 Pattern adopté : CQRS léger via MediatR

On adopte une séparation **Commands / Queries** :
- **Commands** = modifications (écriture). Retournent un `Result` simple ou un ID.
- **Queries** = lectures. Retournent des DTOs.

Chaque port primaire est concrètement une **paire** `Command/Query` + `Handler`.

### 6.2 Use cases du MVP 1

**Commands** (intentions de modification) :
- `RegisterUserCommand` → handler `RegisterUserHandler`
- `LoginCommand`
- `Enable2FaCommand`, `Verify2FaCommand`
- `LinkInpiAccountCommand` (F-003)
- `AddCompanyToFavoritesCommand` (F-017)
- `RemoveCompanyFromFavoritesCommand`
- `AddTrademarkToFavoritesCommand` (F-018)
- `LogSearchHistoryCommand` (F-008)

**Queries** (intentions de lecture) :
- `GetCompanyBySirenQuery` (F-004)
- `SearchCompaniesQuery` (F-005)
- `SearchTrademarksQuery` (F-006)
- `GetTrademarkDetailsQuery` (F-007)
- `GetUserFavoritesQuery`
- `GetSearchHistoryQuery`

### 6.3 Use cases du MVP 2 (veille incluse)

**Commands** :
- `SubscribeToVeillePackCommand` (F-042)
- `AddCustomFeedSourceCommand` (F-043)
- `MarkFeedItemAsReadCommand`
- `CreateWatchRuleCommand` (F-046)
- `DownloadBulkDocumentsCommand` (F-014)

**Queries** :
- `GetTimelineQuery` (F-044)
- `GetVeillePackCatalogQuery`
- `GetFeedItemDetailsQuery`
- `SearchPatentsQuery` (F-015, F-016)

### 6.4 Structure d'un handler

Exemple complet :

```csharp
namespace Atlas.Application.Companies.Queries;

public record GetCompanyBySirenQuery(string SirenValue) : IRequest<Result<CompanyDto>>;

public class GetCompanyBySirenHandler 
    : IRequestHandler<GetCompanyBySirenQuery, Result<CompanyDto>>
{
    private readonly ICompanyDataProvider _provider;
    private readonly ICompanyRepository _cache;
    private readonly ICurrentUserService _currentUser;
    private readonly ISearchHistoryRepository _history;
    private readonly IMapper _mapper;
    
    public GetCompanyBySirenHandler(...)
    {
        // DI
    }
    
    public async Task<Result<CompanyDto>> Handle(
        GetCompanyBySirenQuery request, 
        CancellationToken ct)
    {
        // 1. Validation : créer le value object Siren
        if (!Siren.TryParse(request.SirenValue, out var siren))
            return Result<CompanyDto>.Fail(new InvalidSirenError(request.SirenValue));
        
        // 2. Vérifier le cache local
        var cached = await _cache.GetBySirenAsync(siren, ct);
        if (cached is not null && !cached.IsStale)
            return Result<CompanyDto>.Ok(_mapper.Map<CompanyDto>(cached));
        
        // 3. Récupérer les credentials INPI du user courant
        var credentials = await _currentUser.GetInpiCredentialsAsync(ct);
        if (credentials is null)
            return Result<CompanyDto>.Fail(new InpiAccountNotLinkedError());
        
        // 4. Appeler le provider externe (INPI RNE)
        var uniteLegale = await _provider.GetCompanyBySirenAsync(siren, credentials, ct);
        if (uniteLegale is null)
            return Result<CompanyDto>.Fail(new CompanyNotFoundError(siren));
        
        // 5. Cache local
        var company = Company.FromUniteLegale(uniteLegale);
        await _cache.UpsertAsync(company, ct);
        
        // 6. Historique
        await _history.AddAsync(new SearchHistoryEntry(
            _currentUser.UserId, "SIREN", request.SirenValue, ct), ct);
        
        // 7. Retour DTO
        return Result<CompanyDto>.Ok(_mapper.Map<CompanyDto>(company));
    }
}
```

---

## 7. Inventaire des adapters secondaires

Implémentations concrètes des ports, dans `Atlas.Infrastructure.*`.

### 7.1 Atlas.Infrastructure.Inpi

| Adapter | Implémente | Note |
|---|---|---|
| `InpiRneCompanyProvider` | `ICompanyDataProvider` | INPI RNE — auth JWT |
| `InpiPiTrademarkProvider` | `IIntellectualPropertyProvider` (partiel) | INPI PI marques — auth XSRF complexe |
| `InpiPiPatentProvider` | `IIntellectualPropertyProvider` (partiel) | INPI PI brevets |
| `InpiAuthenticationProvider` | `IInpiAuthenticationProvider` | Gestion des tokens INPI |
| `InpiDocumentDownloader` | `IDocumentDownloader` | Téléchargement bilans/actes/fascicules |

**Conventions** :
- Chaque adapter wrap un `HttpClient` typé (`AddHttpClient<InpiRneCompanyProvider>` en DI)
- **Polly** pour retry / circuit breaker / timeout sur tous les appels
- **Sérialisation** : `System.Text.Json` avec `JsonSerializerOptions` configurés par adapter
- **Mapping** : fonctions privées `MapFromJson(...)` retournant des entités domaine

### 7.2 Atlas.Infrastructure.Persistence

| Adapter | Implémente | Note |
|---|---|---|
| `CompanyRepository` | `ICompanyRepository` | EF Core + Postgres |
| `UserRepository` | `IUserRepository` | Idem |
| `TrademarkRepository` | `ITrademarkRepository` | Idem |
| `PatentRepository` | `IPatentRepository` | Idem |
| `FavoriteRepository` | `IFavoriteRepository` | Idem |
| `SearchHistoryRepository` | `ISearchHistoryRepository` | Idem |
| `FeedSourceRepository` | `IFeedSourceRepository` | Idem |
| `FeedItemRepository` | `IFeedItemRepository` | Idem |
| `UnitOfWork` | `IUnitOfWork` | Wrap d'un `DbContext` |
| `AtlasDbContext` | (interne) | DbContext EF Core central |

### 7.3 Atlas.Infrastructure.Veille

| Adapter | Implémente | Note |
|---|---|---|
| `RssFeedProvider` | `IExternalContentSource` | Parsing RSS/Atom via `CodeHollow.FeedReader` |
| `BodaccLegalNoticeProvider` | `IExternalContentSource` | BODACC via Opendatasoft |
| `InpiBopiProvider` | `IExternalContentSource` | BOPI (scraping ou flux personnalisé) |
| `FeedItemDeduplicator` | `IFeedItemDeduplicator` | Algo MinHash/SimHash |
| `VeillePackCatalog` | `IVeillePackCatalog` | Catalogue (lecture depuis BDD ou fichier embeded) |

### 7.4 Atlas.Infrastructure.Security

| Adapter | Implémente | Note |
|---|---|---|
| `Argon2idPasswordHasher` | `IPasswordHasher` | Konscious.Security.Cryptography |
| `TotpProvider` | `ITotpProvider` | OtpNet ou Otp.NET |
| `JwtIssuer` | `IJwtIssuer` | Microsoft.AspNetCore.Authentication.JwtBearer |
| `JwtValidator` | `IJwtValidator` | Idem |
| `AzureKeyVaultKmsProvider` (prod) | `IKmsKeyProvider` | Azure |
| `HashiCorpVaultKmsProvider` (option self-hosted) | `IKmsKeyProvider` | Vault |
| `AesGcmCryptoService` | `ICryptoService` | System.Security.Cryptography |

### 7.5 Atlas.Infrastructure.Messaging

| Adapter | Implémente | Note |
|---|---|---|
| `BrevoEmailSender` | `IEmailSender` | API Brevo (ex Sendinblue) |
| `SmtpEmailSender` (dev/fallback) | `IEmailSender` | SMTP générique |
| `FirebasePushSender` | `IPushNotificationSender` | FCM pour Android |
| `ApnsPushSender` | `IPushNotificationSender` | APNs pour iOS |

### 7.6 Atlas.Infrastructure.Cache

| Adapter | Implémente | Note |
|---|---|---|
| `RedisCache` | `ICache<T>` | StackExchange.Redis |
| `InMemoryCache` (dev) | `ICache<T>` | Microsoft.Extensions.Caching.Memory |

### 7.7 Atlas.Infrastructure.Storage

| Adapter | Implémente | Note |
|---|---|---|
| `LocalFileStorage` | `IFileStorage` | ✅ Livré (F-014) — filesystem local, sanitization du chemin (anti path-traversal). Configurable via `Storage:Local:RootPath`. |
| `S3CompatibleFileStorage` | `IFileStorage` | 🔜 À venir — Scaleway Object Storage / OVH ObjectStorage / MinIO via `AWSSDK.S3`. |

---

## 8. Inventaire des adapters primaires

### 8.1 Atlas.Api (Web API)

**Stack** : ASP.NET Core 9 + Minimal APIs

**Endpoints structurés par domaine** :

```
/api/v1/auth
  POST /register
  POST /login
  POST /2fa/enable
  POST /2fa/verify
  POST /refresh

/api/v1/users/me
  GET /
  PUT /profile
  POST /inpi-account
  DELETE /inpi-account

/api/v1/companies
  GET /{siren}
  GET /search?q=...

/api/v1/trademarks
  GET /search?q=...
  GET /{id}

/api/v1/favorites
  GET /
  POST /companies/{siren}
  DELETE /companies/{siren}
  POST /trademarks/{id}
  DELETE /trademarks/{id}

/api/v1/veille
  GET /timeline
  GET /packs
  POST /subscriptions
  POST /sources

/api/v1/documents
  GET /companies/{siren}/attachments
  GET /companies/{siren}/attachments/{type}/{id}
  POST /bulk-download
```

**Middlewares standard** :
1. **Error handling** : capture les exceptions, retourne du JSON standardisé (RFC 7807 Problem Details)
2. **CORS** : configuré strictement (origines whitelistées)
3. **Authentication** : JWT Bearer
4. **Authorization** : policies par endpoint
5. **Rate limiting** : `Microsoft.AspNetCore.RateLimiting` natif
6. **Logging** : Serilog avec corrélation d'IDs
7. **Output caching** (pour les lectures)

### 8.2 Atlas.Maui

**Stack** : .NET MAUI sur .NET 9

**Structure** :
```
Atlas.Maui/
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   └── RegisterPage.xaml
│   ├── Companies/
│   │   ├── CompanySearchPage.xaml
│   │   └── CompanyDetailsPage.xaml
│   ├── Trademarks/
│   ├── Veille/
│   │   └── TimelinePage.xaml
│   └── Profile/
├── ViewModels/
│   └── ... (un par View, MVVM strict)
├── Services/
│   ├── IAtlasApiClient.cs       ← interface HTTP client
│   └── AtlasApiClient.cs        ← implémentation Refit ou HttpClient
├── Storage/
│   └── LocalStorage.cs          ← SQLite pour offline
├── Resources/
│   └── ... (styles, images)
└── MauiProgram.cs
```

**Patterns clés MAUI** :
- **MVVM strict** via `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`, source generators)
- **Refit** pour les clients HTTP typés contre `Atlas.Api`
- **SQLite-net-pcl** pour le stockage local hors ligne
- **Polly** côté client aussi pour la résilience réseau
- **MAUI Shell** pour la navigation

### 8.3 Atlas.Cli (futur, V3+)

CLI .NET pour les power users : `atlas search siren 552032534`. Architecture identique : appelle l'API Atlas via HTTP.

---

## 9. Flow bout en bout — Rechercher une entreprise par SIREN

Pour rendre tout ce qui précède concret, voici le **flow complet** du use case le plus simple, étape par étape.

### 9.1 Scénario

Un utilisateur authentifié, ayant lié son compte INPI, ouvre l'app MAUI sur son téléphone et tape `552032534` dans la barre de recherche.

### 9.2 Séquence complète

```
┌──────────────┐
│ MAUI iPhone  │
└──────┬───────┘
       │ User tape "552032534" dans la SearchBar
       │
       ▼
┌──────────────────────────────────────┐
│ CompanySearchViewModel (MAUI)         │
│                                       │
│ - Validation client basique (longueur)│
│ - Affichage Loading state             │
│ - Appel HTTP via IAtlasApiClient      │
└──────┬───────────────────────────────┘
       │ HTTPS GET /api/v1/companies/552032534
       │ Header: Authorization: Bearer <jwt>
       │
       ▼
┌────────────────────────────────────────┐
│ Atlas.Api — Middleware Pipeline        │
│                                        │
│ 1. CORS check                          │
│ 2. RateLimiting check                  │
│ 3. JWT validation → extraction UserId  │
│ 4. Logging avec corrélation ID         │
└──────┬─────────────────────────────────┘
       │
       ▼
┌────────────────────────────────────────┐
│ Endpoint GET /api/v1/companies/{siren} │
│ (Minimal API)                          │
│                                        │
│ - Bind route param siren               │
│ - Crée GetCompanyBySirenQuery          │
│ - Envoie via MediatR.Send()            │
└──────┬─────────────────────────────────┘
       │
       ▼
┌────────────────────────────────────────┐
│ ATLAS.APPLICATION                       │
│ GetCompanyBySirenHandler.Handle()       │
│                                         │
│ Étape 1 : Validation                    │
│  - Siren.TryParse("552032534") → OK     │
│  - (le value object vérifie Luhn)       │
│                                         │
│ Étape 2 : Cache local                   │
│  - _cache.GetBySirenAsync(siren)        │
│  - Pas en cache (ou trop ancien)        │
│                                         │
│ Étape 3 : Récupérer credentials INPI    │
│  - _currentUser.GetInpiCredentials...   │
│  - (déchiffre via KMS)                  │
└──────┬──────────────────────────────────┘
       │ Appel : ICompanyDataProvider.GetCompanyBySirenAsync
       │
       ▼
┌────────────────────────────────────────┐
│ ATLAS.INFRASTRUCTURE.INPI               │
│ InpiRneCompanyProvider                  │
│                                         │
│ Étape A : Récupérer token INPI valide   │
│  - via IInpiAuthenticationProvider      │
│  - cache du token (1h) ou re-login      │
│                                         │
│ Étape B : Appel HTTP INPI RNE           │
│  - GET https://registre-national-       │
│    entreprises.inpi.fr/api/companies/   │
│    552032534                            │
│  - Header: Authorization: Bearer <inpi> │
│  - Wrapped Polly retry + circuit breaker│
│                                         │
│ Étape C : Mapping                       │
│  - JSON INPI → UniteLegale domain entity│
└──────┬──────────────────────────────────┘
       │ Retour : UniteLegale
       │
       ▼
┌────────────────────────────────────────┐
│ Handler — Étapes finales                │
│                                         │
│ Étape 4 : Map UniteLegale → Company     │
│  - Company.FromUniteLegale(...)         │
│                                         │
│ Étape 5 : Mettre en cache local         │
│  - _cache.UpsertAsync(company)          │
│  - (sauvegarde en Postgres avec TTL)    │
│                                         │
│ Étape 6 : Historique                    │
│  - _history.AddAsync(searchEntry)       │
│                                         │
│ Étape 7 : Map vers DTO                  │
│  - _mapper.Map<CompanyDto>(company)     │
│                                         │
│ Étape 8 : Retour Result.Ok(dto)         │
└──────┬──────────────────────────────────┘
       │
       ▼
┌────────────────────────────────────────┐
│ Endpoint API — Sérialisation            │
│                                         │
│ - Result.Ok → 200 OK + body JSON        │
│ - (Result.Fail → 4xx/5xx avec problème) │
│ - Logging final + métriques             │
└──────┬─────────────────────────────────┘
       │ HTTP 200 OK + Company JSON
       │
       ▼
┌────────────────────────────────────────┐
│ MAUI — ViewModel                        │
│                                         │
│ - Désérialisation JSON                  │
│ - Update du model bindé                 │
│ - Hide Loading state                    │
│ - Affichage de la fiche                 │
│                                         │
│ + Annonce au lecteur d'écran (cf. doc 06)│
│   "Entreprise Renault SA trouvée"       │
└────────────────────────────────────────┘
```

### 9.3 Points d'attention dans ce flow

- **L'utilisateur ne sait pas** que ses credentials INPI ont été utilisés. Elles sont déchiffrées en mémoire le temps de la requête, jamais loggées.
- **Le domaine ne sait pas** que la donnée vient de l'INPI. Pour lui, il appelle `ICompanyDataProvider`. Demain on remplace par `SireneCompanyProvider`, **rien d'autre ne change**.
- **Le cache local** réduit drastiquement le nombre d'appels INPI. C'est essentiel pour le rate limiting et la perf perçue.
- **Polly gère la résilience** : si l'INPI a un hoquet de 500ms, retry automatique. Si l'INPI est down, circuit breaker pour éviter le fail-storm.
- **L'historique** se fait après réussite seulement. Pas d'historique si erreur.
- **L'erreur typée** : si SIREN invalide, on retourne `400 InvalidSiren` avec le détail. Si compte INPI non lié, `409 InpiAccountNotLinked`. Si entreprise pas trouvée, `404`. Pas de "Internal Server Error" générique.

---

## 10. Cross-cutting concerns

Les préoccupations transverses : implémentées comme middleware, decorators, behaviors MediatR.

### 10.1 Validation

- **Library** : FluentValidation
- **Pattern** : validators par DTO/Command/Query (`AddCompanyToFavoritesCommandValidator`)
- **Activation** : `ValidationBehavior<TRequest, TResponse>` MediatR pipeline behavior — exécute la validation AVANT le handler

### 10.2 Logging

- **Library** : Serilog
- **Format** : structured logs JSON
- **Enrichers** : UserId, RequestId, IP (pseudonymisée), TraceId
- **Sinks** : Console + Fichier en local, Seq ou Better Stack en prod
- **Règle** : pas de PII (cf. doc 04)

### 10.3 Observabilité

- **OpenTelemetry** pour traces, metrics, logs
- **Activités** créées dans les handlers et adapters pour visualiser les appels en chaîne
- **Métriques custom** : nombre d'appels INPI, latence, taux d'erreur

### 10.4 Gestion d'erreurs

- **Pattern** : `Result<T>` (succès) / `Result<T, TError>` (succès ou erreur typée)
- **Pas d'exceptions** pour les erreurs métier ("ressource non trouvée" n'est pas une exception)
- **Exceptions** réservées aux erreurs vraiment exceptionnelles (DB down, etc.) — gérées par middleware

### 10.5 Caching

- **Niveaux** :
  1. Cache HTTP côté MAUI (cache-control)
  2. Cache applicatif Redis (clés expressives : `company:{siren}`)
  3. Cache BDD (table dédiée avec TTL)
- **Invalidation** : TTL + invalidation explicite sur événements (`CompanyUpdatedEvent`)

### 10.6 Authentification & autorisation

- **Authentification** : JWT Bearer émis par `Atlas.Api`
- **Autorisation** : policies ASP.NET Core (`UserMustOwnResourcePolicy`)
- **Multi-tenant credentials** : `ICurrentUserService` accès au UserId courant et déchiffrement on-demand des credentials INPI

### 10.7 Transactions

- **UnitOfWork** pattern via `IUnitOfWork`
- **TransactionBehavior** MediatR : ouvre la transaction sur commands, commit en fin de handler
- **Queries** : pas de transaction (lecture seule)

---

## 11. Patterns techniques retenus

### 11.1 CQRS léger

- Commands et Queries séparés
- Mais **pas de séparation des modèles** (pas d'event sourcing, pas de read model dédié en MVP)
- Apporte la lisibilité et la testabilité sans la complexité de CQRS pur

### 11.2 MediatR (pipeline behaviors)

```
Request → ValidationBehavior → LoggingBehavior → TransactionBehavior → Handler → Response
```

### 11.3 Result pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IDomainError? Error { get; }
    
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(IDomainError error) => new(false, default, error);
}
```

### 11.4 Repository + Specification

```csharp
public interface ICompanyRepository
{
    Task<PagedResult<Company>> SearchAsync(
        ICompanySpecification spec, 
        CancellationToken ct);
}

// Spec composable
var spec = new ActiveCompaniesSpec()
    .And(new InRegionSpec("Île-de-France"))
    .And(new WithNafCodeSpec("6202A"));
```

### 11.5 Strongly-typed IDs

```csharp
public readonly record struct CompanyId(Guid Value)
{
    public static CompanyId New() => new(Guid.NewGuid());
}

// Impossible de passer un UserId là où on attend un CompanyId
```

### 11.6 Domain Events (in-process)

```csharp
public class Company
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> Events => _events;
    
    public void AddToFavorites(UserId userId)
    {
        _events.Add(new CompanyAddedToFavoritesEvent(Id, userId));
    }
}

// Dispatched par MediatR après commit en transaction
```

### 11.7 Dependency Injection

- **Composition root** : `Atlas.Api/Program.cs`
- **Extension methods** par module pour rester propre :
  ```csharp
  builder.Services
      .AddDomainServices()
      .AddApplicationServices()
      .AddInpiInfrastructure(configuration)
      .AddPersistenceInfrastructure(configuration)
      .AddVeilleInfrastructure(configuration)
      .AddSecurityInfrastructure(configuration);
  ```

---

## 12. Tests d'architecture

Pour faire **vivre** ces règles, on les **automatise** en tests.

### 12.1 Library

**NetArchTest** ou **ArchUnitNET** — exécutés à chaque CI.

### 12.2 Exemples de règles à automatiser

```csharp
public class ArchitectureTests
{
    [Fact]
    public void Domain_should_not_depend_on_Application()
    {
        var result = Types.InAssembly(typeof(Company).Assembly)
            .Should().NotHaveDependencyOn("Atlas.Application")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void Domain_should_not_depend_on_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Company).Assembly)
            .Should().NotHaveDependencyOnAny("Atlas.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void Handlers_should_be_internal_sealed()
    {
        var result = Types.InAssembly(typeof(GetCompanyBySirenHandler).Assembly)
            .That().HaveNameEndingWith("Handler")
            .Should().BeSealed().And().NotBePublic()
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void Maui_should_not_depend_on_Infrastructure()
    {
        var result = Types.InAssembly(typeof(AppShell).Assembly)
            .Should().NotHaveDependencyOnAny("Atlas.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
}
```

### 12.3 Autres règles utiles

- Tous les `Handler` doivent être internes scellés (évite les usages directs)
- Les `Repository` doivent être dans Infrastructure.Persistence
- Les classes de domaine ne doivent pas avoir `public set;` (immutabilité)
- Les classes EF (DbContext, configurations) ne doivent pas être dans Domain
- Les ports (`I*Repository`, `I*Provider`) doivent être définis dans Domain

---

## Annexes

### A.1 Diagramme de dépendance des projets

```
                      ┌────────────────┐
                      │  Atlas.Maui    │
                      └───────┬────────┘
                              │ → Domain, Shared
                              │
                      ┌───────▼────────┐
                      │   Atlas.Api    │
                      └───────┬────────┘
                              │ → Application, Domain, Infrastructure.*, Shared
                              │
            ┌─────────────────┼─────────────────┐
            │                 │                 │
            ▼                 ▼                 ▼
  ┌─────────────────┐  ┌──────────────┐  ┌──────────────────┐
  │ Infrastructure.*│  │ Application  │  │  Application.    │
  │                 │  │              │  │  Premium         │
  └────────┬────────┘  └──────┬───────┘  └──────────┬───────┘
           │                  │                     │
           │                  └──────┬──────────────┘
           │                         │
           │                         ▼
           │              ┌─────────────────┐
           └──────────────►   Atlas.Domain  │
                          └─────────┬───────┘
                                    │
                                    ▼
                          ┌─────────────────┐
                          │  Atlas.Shared   │
                          └─────────────────┘
```

### A.2 Checklist de revue d'architecture (à chaque PR)

- [ ] Aucune nouvelle dépendance Domain → Application/Infrastructure
- [ ] Aucune nouvelle dépendance Application → Infrastructure
- [ ] Les nouveaux ports sont définis dans Domain
- [ ] Les nouveaux adapters sont dans le projet Infrastructure approprié
- [ ] Les handlers respectent le pattern (validation → travail → result)
- [ ] Les value objects ont leur validation
- [ ] Les tests d'architecture passent

---

*Document évolutif. Toute évolution majeure de l'architecture doit être tracée par un nouvel ADR dans la doc 01.*
