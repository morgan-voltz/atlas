# Layout de la solution .NET — projet Atlas

> ✅ **ADR-029 (UI unifiée Uno Platform)** — Ce layout décrit la couche cliente cible **`Atlas.App`** (Uno, single project, C#/XAML WinUI), qui **remplace** `Atlas.Maui` (projet réel, amorce F-009/F-010), le desktop Avalonia (ADR-026) et le client web Blazor `Atlas.Web`/`Atlas.Web.Client` (ADR-017). **`Atlas.App` est intégré au repo** (PR #120 : `Uno.Sdk` pin dans `global.json`, CPM isolé, test NetArchTest, job CI `uno-build`) et couvre désormais le parcours réel (auth/INPI/recherche/fiche/favoris/veille — cf. `docs/15` §7, U4). **Têtes ciblées actuellement : `net10.0-browserwasm` + `net10.0-desktop`** (Skia : Win/macOS/Linux) ; les têtes **iOS/Android/macCatalyst restent à ajouter**. **Transition** : le client Blazor `Atlas.Web.Client` reste l'app web en vigueur et n'est **retiré qu'après** un spike Uno multi-cible (Linux/mobile) concluant (garde-fou ADR-029, U5).

> Spécification complète de la structure physique de la solution **`Atlas.sln`** : arborescence des dossiers, liste des projets `.csproj`, frameworks cibles, références inter-projets, fichiers de configuration centralisés, et commandes `dotnet` pour reproduire la solution depuis zéro.
> Ce document traduit en structure concrète les décisions prises dans les ADRs (doc 01) et l'architecture détaillée (doc 09).

**Version** : 1.1
**Date de dernière mise à jour** : 1ᵉʳ juin 2026 (refonte client : `Atlas.App` Uno remplace `Atlas.Maui`/`Atlas.Web` — ADR-029)
**Nom de code projet** : Atlas (provisoire)
**.NET cible** : .NET 10 LTS (supporté jusqu'à novembre 2028)

---

## Sommaire

1. Pourquoi ce layout
2. Arborescence physique complète
3. Le rôle des fichiers de configuration racine
4. Inventaire détaillé des projets de production
5. Inventaire détaillé des projets de tests
6. Cas particulier — Atlas.App (Uno) et le multi-targeting
7. Gestion centralisée des versions NuGet
8. Le fichier global.json et le pinning du SDK
9. Scripts de création de la solution depuis zéro
10. Conventions de nommage et organisation interne des projets
11. Le `.editorconfig` recommandé
12. Les `.gitignore` et `.gitattributes`
13. Migration future vers .NET 11

---

## 1. Pourquoi ce layout

L'objectif d'un layout de solution n'est pas de plaire à l'œil ou de suivre une mode, mais de **rendre l'intention architecturale visible immédiatement**. Quand un nouveau contributeur clone le repository et ouvre la solution, il doit pouvoir comprendre la philosophie du projet en regardant simplement les noms et la disposition des projets.

Le layout retenu repose sur quatre principes simples qui méritent d'être explicités. Le premier principe est la **séparation physique entre code de production et code de test**. Tous les projets de production vivent sous `src/`, tous les projets de test vivent sous `tests/`. Cette séparation permet, par exemple, de générer un package NuGet en n'incluant que `src/`, ou d'exclure les tests d'un build de production. Beaucoup de projets .NET historiques mettaient tout au même niveau, et c'est aujourd'hui considéré comme une mauvaise pratique.

Le second principe est le **nommage strictement hiérarchique des projets**. Chaque projet commence par `Atlas.`, puis se spécialise. On obtient ainsi `Atlas.Domain`, `Atlas.Application`, `Atlas.Infrastructure.Inpi`, etc. Ce nommage a deux conséquences pratiques : les namespaces C# suivent exactement la même hiérarchie, ce qui élimine la dissonance cognitive entre l'organisation des fichiers et celle du code ; et les projets apparaissent triés par catégorie dans la solution, ce qui en facilite la navigation.

Le troisième principe est l'**alignement entre découpage en projets et règles de dépendances**. Si on a dit dans l'architecture hexagonale que le domaine ne doit dépendre de rien d'autre que `Atlas.Shared`, alors `Atlas.Domain.csproj` ne référence physiquement que `Atlas.Shared.csproj`. Le compilateur devient le gardien des règles d'architecture. C'est l'une des raisons profondes pour lesquelles on découpe en autant de projets : chaque frontière de projet est une frontière de dépendance qu'il devient impossible de franchir par accident.

Le quatrième principe est l'**utilisation maximale des fichiers de configuration centralisés** introduits par les versions récentes de .NET et MSBuild. Plutôt que de dupliquer les propriétés communes (langage, nullable, warnings comme erreurs, etc.) dans chaque `.csproj`, on les définit une seule fois dans `Directory.Build.props` à la racine. Cela rend les `.csproj` individuels minuscules et lisibles, et garantit que toute évolution globale (passage à C# 14 par exemple) se fait en un seul endroit.

---

## 2. Arborescence physique complète

Voici la structure complète des dossiers et fichiers à la racine du repository. J'ai annoté chaque entrée pour expliquer son rôle.

```
atlas/                                            (racine du repository git)
│
├── .git/                                         (géré par git, ne pas toucher)
├── .github/                                      (workflows CI GitHub Actions)
│   └── workflows/
│       ├── build.yml
│       ├── test.yml
│       └── deploy.yml
│
├── .vscode/                                      (config partagée VS Code, optionnel)
│   ├── settings.json
│   ├── extensions.json
│   └── launch.json
│
├── docs/                                         (les 10 docs fondateurs)
│   ├── 01-decisions-architecturales.md
│   ├── 02-roadmap-features.md
│   ├── 03-catalogue-apis-publiques.md
│   ├── 04-securite-rgpd.md
│   ├── 05-strategie-repos.md
│   ├── 06-accessibilite.md
│   ├── 07-flux-rss-veille.md
│   ├── 08-vocabulaire-ubiquitaire.md
│   ├── 09-architecture-detaillee.md
│   └── 10-layout-solution-dotnet.md
│
├── src/                                          (code de production)
│   │
│   ├── Atlas.Shared/                             (utilitaires partagés transverses)
│   │   ├── Atlas.Shared.csproj
│   │   ├── Result/
│   │   │   ├── Result.cs
│   │   │   └── PagedResult.cs
│   │   └── Common/
│   │       ├── DateTimeExtensions.cs
│   │       └── Constants.cs
│   │
│   ├── Atlas.Domain/                             (cœur métier pur)
│   │   ├── Atlas.Domain.csproj
│   │   ├── Companies/
│   │   │   ├── Company.cs
│   │   │   ├── UniteLegale.cs
│   │   │   ├── Etablissement.cs
│   │   │   ├── FormeJuridique.cs
│   │   │   ├── Dirigeant.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Siren.cs
│   │   │   │   ├── Siret.cs
│   │   │   │   ├── Naf.cs
│   │   │   │   └── CompanyId.cs
│   │   │   ├── Ports/
│   │   │   │   ├── ICompanyRepository.cs
│   │   │   │   └── ICompanyDataProvider.cs
│   │   │   └── Events/
│   │   │       └── CompanyAddedToFavoritesEvent.cs
│   │   ├── IntellectualProperty/
│   │   │   ├── Trademark.cs
│   │   │   ├── Patent.cs
│   │   │   ├── Design.cs
│   │   │   ├── ValueObjects/
│   │   │   └── Ports/
│   │   ├── Users/
│   │   │   ├── User.cs
│   │   │   ├── Account.cs
│   │   │   ├── InpiCredentials.cs
│   │   │   ├── ValueObjects/
│   │   │   │   └── UserId.cs
│   │   │   └── Ports/
│   │   │       ├── IUserRepository.cs
│   │   │       └── IPasswordHasher.cs
│   │   ├── Veille/
│   │   │   ├── FeedSource.cs
│   │   │   ├── FeedItem.cs
│   │   │   ├── VeillePack.cs
│   │   │   ├── WatchRule.cs
│   │   │   └── Ports/
│   │   │       └── IExternalContentSource.cs
│   │   └── Common/
│   │       ├── Entity.cs                         (classe de base entité)
│   │       ├── ValueObject.cs                    (classe de base VO)
│   │       ├── DomainError.cs
│   │       └── IDomainEvent.cs
│   │
│   ├── Atlas.Application/                        (use cases gratuits)
│   │   ├── Atlas.Application.csproj
│   │   ├── Companies/
│   │   │   ├── Commands/
│   │   │   │   ├── AddCompanyToFavorites/
│   │   │   │   │   ├── AddCompanyToFavoritesCommand.cs
│   │   │   │   │   ├── AddCompanyToFavoritesHandler.cs
│   │   │   │   │   └── AddCompanyToFavoritesValidator.cs
│   │   │   │   └── ...
│   │   │   └── Queries/
│   │   │       ├── GetCompanyBySiren/
│   │   │       │   ├── GetCompanyBySirenQuery.cs
│   │   │       │   ├── GetCompanyBySirenHandler.cs
│   │   │       │   └── CompanyDto.cs
│   │   │       └── ...
│   │   ├── Users/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── IntellectualProperty/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Veille/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Common/
│   │   │   ├── Behaviors/                        (pipeline behaviors MediatR)
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TransactionBehavior.cs
│   │   │   ├── Mappings/                         (profils AutoMapper / Mapster)
│   │   │   └── Services/
│   │   │       └── ICurrentUserService.cs
│   │   └── DependencyInjection.cs                (extension method d'enregistrement)
│   │
│   ├── Atlas.Application.Premium/                (use cases premium — vide en MVP 2)
│   │   ├── Atlas.Application.Premium.csproj
│   │   └── README.md                             (notice indiquant pourquoi le projet existe)
│   │
│   ├── Atlas.Infrastructure.Inpi/                (adapters INPI)
│   │   ├── Atlas.Infrastructure.Inpi.csproj
│   │   ├── Rne/
│   │   │   ├── InpiRneCompanyProvider.cs
│   │   │   └── DTOs/                             (objets de mapping JSON INPI)
│   │   ├── Pi/
│   │   │   ├── InpiPiTrademarkProvider.cs
│   │   │   ├── InpiPiPatentProvider.cs
│   │   │   └── DTOs/
│   │   ├── Authentication/
│   │   │   └── InpiAuthenticationProvider.cs
│   │   ├── Documents/
│   │   │   └── InpiDocumentDownloader.cs
│   │   ├── Common/
│   │   │   ├── PollyPolicies.cs
│   │   │   └── InpiHttpClient.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Persistence/         (EF Core + PostgreSQL)
│   │   ├── Atlas.Infrastructure.Persistence.csproj
│   │   ├── AtlasDbContext.cs
│   │   ├── Configurations/                       (EF Core IEntityTypeConfiguration)
│   │   │   ├── CompanyConfiguration.cs
│   │   │   ├── UserConfiguration.cs
│   │   │   └── ...
│   │   ├── Repositories/
│   │   │   ├── CompanyRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   └── ...
│   │   ├── Migrations/                           (généré par EF Core)
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Veille/              (RSS, BODACC, agrégation)
│   │   ├── Atlas.Infrastructure.Veille.csproj
│   │   ├── Rss/
│   │   │   └── RssFeedProvider.cs
│   │   ├── Bodacc/
│   │   │   └── BodaccLegalNoticeProvider.cs
│   │   ├── Deduplication/
│   │   │   └── MinHashDeduplicator.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Messaging/           (email + push)
│   │   ├── Atlas.Infrastructure.Messaging.csproj
│   │   ├── Email/
│   │   │   └── BrevoEmailSender.cs
│   │   ├── Push/
│   │   │   ├── FirebasePushSender.cs
│   │   │   └── ApnsPushSender.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Security/            (KMS, JWT, hashage)
│   │   ├── Atlas.Infrastructure.Security.csproj
│   │   ├── Password/
│   │   │   └── Argon2idPasswordHasher.cs
│   │   ├── Jwt/
│   │   │   ├── JwtIssuer.cs
│   │   │   └── JwtValidator.cs
│   │   ├── Kms/
│   │   │   ├── AzureKeyVaultKmsProvider.cs
│   │   │   └── HashiCorpVaultKmsProvider.cs
│   │   ├── Crypto/
│   │   │   └── AesGcmCryptoService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Cache/               (Redis + in-memory)
│   │   ├── Atlas.Infrastructure.Cache.csproj
│   │   ├── Redis/
│   │   │   └── RedisCache.cs
│   │   ├── InMemory/
│   │   │   └── InMemoryCache.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Infrastructure.Storage/             (stockage fichiers S3-compatible)
│   │   ├── Atlas.Infrastructure.Storage.csproj
│   │   ├── S3Compatible/
│   │   │   └── S3CompatibleFileStorage.cs
│   │   ├── Local/
│   │   │   └── LocalFileStorage.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Atlas.Api/                                (Web API ASP.NET Core)
│   │   ├── Atlas.Api.csproj
│   │   ├── Program.cs
│   │   ├── Endpoints/                            (Minimal APIs)
│   │   │   ├── AuthEndpoints.cs
│   │   │   ├── CompanyEndpoints.cs
│   │   │   ├── TrademarkEndpoints.cs
│   │   │   ├── VeilleEndpoints.cs
│   │   │   └── ...
│   │   ├── Middlewares/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Filters/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── appsettings.Production.json
│   │
│   └── Atlas.App/                                (client Uno — UI unique multi-cible, cf. ADR-029)
│       ├── Atlas.App.csproj                      (Uno.Sdk ; single project, TargetFrameworks browserwasm/desktop/android/ios/maccatalyst)
│       ├── App.xaml / App.xaml.cs
│       ├── GlobalUsings.cs
│       ├── Presentation/                         (pages XAML + ViewModels, MVVM)
│       │   ├── Auth/
│       │   ├── Companies/
│       │   ├── Trademarks/
│       │   └── Veille/
│       ├── Services/
│       │   ├── IAtlasApiClient.cs
│       │   └── AtlasApiClient.cs
│       ├── Storage/
│       │   └── LocalStorage.cs                   (cache offline ; secure storage natif côté desktop/mobile)
│       ├── Styles/                               (ResourceDictionary XAML : tokens, thème clair/sombre)
│       ├── Assets/                               (icônes, splash, images)
│       ├── Strings/                              (ressources de localisation)
│       ├── Package.appxmanifest / app.manifest
│       └── Platforms/                            (têtes & code spécifique par cible)
│           ├── Android/
│           ├── iOS/
│           ├── MacCatalyst/
│           ├── Desktop/                          (Skia ; couvre Windows, macOS, Linux)
│           └── WebAssembly/
│
├── tests/                                        (code de tests)
│   │
│   ├── Atlas.Domain.UnitTests/
│   │   └── Atlas.Domain.UnitTests.csproj
│   │
│   ├── Atlas.Application.UnitTests/
│   │   └── Atlas.Application.UnitTests.csproj
│   │
│   ├── Atlas.Infrastructure.Inpi.IntegrationTests/
│   │   └── Atlas.Infrastructure.Inpi.IntegrationTests.csproj
│   │
│   ├── Atlas.Infrastructure.Persistence.IntegrationTests/
│   │   └── Atlas.Infrastructure.Persistence.IntegrationTests.csproj
│   │
│   ├── Atlas.Api.IntegrationTests/
│   │   └── Atlas.Api.IntegrationTests.csproj
│   │
│   └── Atlas.Architecture.Tests/
│       └── Atlas.Architecture.Tests.csproj       (tests NetArchTest)
│
├── tools/                                        (scripts utilitaires, optionnel)
│   ├── feeds-check.csx                           (script C# vérifiant les RSS du doc 07)
│   └── ...
│
├── .editorconfig                                 (règles de formatage et style)
├── .gitignore                                    (exclusions git)
├── .gitattributes                                (normalisation fins de ligne, encodage)
├── global.json                                   (pinning de la version du SDK .NET)
├── Directory.Build.props                         (propriétés MSBuild communes à tous les projets)
├── Directory.Packages.props                      (versions NuGet centralisées)
├── nuget.config                                  (sources NuGet)
├── Atlas.sln                                     (fichier solution)
├── README.md                                     (point d'entrée du repo)
├── LICENSE                                       (AGPL v3 selon ADR-005)
├── CONTRIBUTING.md                               (guide contributeurs, à venir avec ADR-006)
├── CODE_OF_CONDUCT.md                            (charte communauté)
└── SECURITY.md                                   (procédure responsible disclosure)
```

Cette arborescence peut sembler intimidante au premier abord, mais elle se comprend en quelques minutes une fois qu'on a identifié les quatre zones principales. Le dossier `docs/` contient toute la documentation fondatrice et est destiné à finir dans le repo public dès que le code l'est aussi. Le dossier `src/` contient le code de production réparti en projets selon les couches de l'architecture hexagonale. Le dossier `tests/` reflète la structure de `src/` en ajoutant les projets de tests appropriés à chaque couche. Et les fichiers à la racine sont des fichiers de configuration globale qui s'appliquent à toute la solution.

---

## 3. Le rôle des fichiers de configuration racine

Les fichiers à la racine du repository jouent chacun un rôle spécifique dans la cohérence de la solution. Comprendre leur fonction permet d'éviter de réinventer la roue dans chaque projet.

Le fichier `global.json` épingle la version du SDK .NET utilisée pour compiler la solution. Sans ce fichier, chaque développeur compilerait avec la version qu'il a installée localement, ce qui peut produire des comportements différents. Avec `global.json`, on garantit que tout le monde utilise la même version, et que le passage à une nouvelle version se fait de manière contrôlée. Pour Atlas, on épinglera le SDK 10.0.x avec la directive `rollForward: latestFeature`, ce qui permet d'utiliser n'importe quelle version 10.0.x sans avoir à éditer le fichier à chaque patch.

Le fichier `Directory.Build.props` est un fichier MSBuild magique qui s'applique automatiquement à tous les projets de la solution. C'est lui qui va contenir les propriétés communes comme la version du langage C# (`<LangVersion>latest</LangVersion>` ou `13` explicitement), l'activation des nullable reference types (`<Nullable>enable</Nullable>`), le traitement des avertissements comme erreurs (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`), et la production de symboles de debug en mode Release. Mettre ces propriétés une seule fois ici plutôt que dans chaque `.csproj` réduit considérablement le bruit dans chaque fichier de projet.

Le fichier `Directory.Packages.props` est le pendant pour les packages NuGet. Il active la fonctionnalité **Central Package Management** introduite avec .NET 7 et désormais standard. Ce fichier liste les versions de tous les packages NuGet utilisés dans la solution, à un seul endroit. Les `.csproj` individuels ne font alors que référencer les packages par leur nom, sans préciser de version. Le bénéfice est immense en termes de cohérence : si tu utilises `Serilog` dans 8 projets, tu es certain qu'ils utilisent tous la même version, et la mise à jour se fait en un seul endroit.

Le fichier `nuget.config` configure les sources NuGet. Pour Atlas, on aura généralement la source officielle `nuget.org`, et potentiellement à terme une source privée si on publie nos propres packages internes.

Le fichier `.editorconfig` est crucial pour la cohérence du code à travers les IDE et éditeurs. Il définit les règles de formatage (indentation, fins de ligne, espaces ou tabulations) et de style C# (nommage des variables, organisation des `using`, conventions de récap). Avec un `.editorconfig` bien configuré, le code formaté par Rider sera identique à celui formaté par Visual Studio ou VS Code, ce qui évite les diffs parasites dans les commits.

Le fichier `Atlas.sln` est le fichier solution Visual Studio, qui liste tous les projets et leur organisation en dossiers logiques. C'est ce fichier qu'on ouvre dans un IDE pour travailler. Il est généré et maintenu via la CLI `dotnet sln`.

---

## 4. Inventaire détaillé des projets de production

La solution compte douze projets de production. Cette section décrit chacun d'entre eux en détail, en précisant son framework cible, son rôle, ses références autorisées, et les packages NuGet principaux qu'il utilisera.

### 4.1 Atlas.Shared

Le projet `Atlas.Shared` est le projet de plus bas niveau de la solution. Il contient des utilitaires absolument transverses qui peuvent être utilisés par n'importe quelle couche, y compris le domaine et le client Uno (`Atlas.App`). Son framework cible est `netstandard2.1` pour maximiser la portabilité, notamment vers toutes les têtes Uno (WebAssembly, desktop, mobile) qui reposent sur `net10.0` (sur-ensemble de netstandard2.1). Il ne référence aucun autre projet de la solution.

Le contenu de ce projet doit rester volontairement minimal. On y trouve typiquement le type générique `Result<T>` qui encapsule un succès ou un échec, le type `PagedResult<T>` qui représente une page de résultats avec ses métadonnées, et quelques constantes globales. Si tu hésites à mettre quelque chose dans `Atlas.Shared`, la règle est simple : si c'est du métier, va dans `Atlas.Domain`, sinon va dans `Atlas.Shared`.

### 4.2 Atlas.Domain

Le projet `Atlas.Domain` est le cœur du système. Son framework cible est `net10.0`. Il référence uniquement `Atlas.Shared`. Il contient toutes les entités métier, les value objects, les domain services, les domain events, les domain errors, et surtout les **interfaces des ports secondaires** que les couches d'infrastructure implémenteront.

Les seuls packages NuGet que ce projet devrait utiliser sont des packages purement abstraits qui ne traînent aucune dépendance technique. Tu peux y mettre `MediatR.Contracts` qui contient juste les interfaces `IRequest<T>` et `INotification` sans le moteur de dispatch, ou `FluentValidation.Abstractions` si tu veux exposer des contrats de validation. Pas de package qui implémenterait des comportements techniques.

### 4.3 Atlas.Application

Le projet `Atlas.Application` contient les use cases du produit, organisés en commands et queries selon le pattern CQRS léger. Son framework cible est `net10.0`. Il référence `Atlas.Domain` et `Atlas.Shared`.

Les packages NuGet typiques de ce projet sont `MediatR` pour le pipeline d'orchestration, `FluentValidation` pour la validation des commands et queries, et `Mapster` ou `AutoMapper` pour la conversion entre entités du domaine et DTOs. On peut y ajouter `Microsoft.Extensions.DependencyInjection.Abstractions` qui permet d'écrire une méthode d'extension `AddApplicationServices()` que la composition root appellera.

### 4.4 Atlas.Application.Premium

Le projet `Atlas.Application.Premium` est créé dès maintenant pour matérialiser la décision d'isolation des features premium prise dans l'ADR-009. En MVP 2, il est volontairement vide à part un fichier `README.md` qui explique pourquoi le projet existe. Plus tard, il contiendra les use cases premium comme les résumés IA, le scoring de pertinence, et les fonctions de collaboration en équipe. Son framework est `net10.0`, et il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Cette séparation peut sembler prématurée puisque le projet sera vide pendant des mois. Mais elle force une discipline architecturale précieuse : dès qu'un développeur, demain, voudra ajouter un use case premium, il devra le mettre ici, ce qui clarifie immédiatement qu'on touche à la zone monétisée du produit. Et le jour où on extraira les modules premium pour les compiler dans un binaire séparé, tout sera déjà en place.

### 4.5 Atlas.Infrastructure.Inpi

Ce projet contient tous les adapters vers les APIs de l'INPI : RNE pour les données entreprises, PI pour les marques et brevets, gestion de l'authentification et du téléchargement de documents. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Les packages NuGet importants sont `Microsoft.Extensions.Http` pour les `HttpClient` typés, `Polly` pour la résilience (retry, circuit breaker, timeout), `System.Text.Json` pour la sérialisation, et éventuellement `Refit` si on veut générer les clients HTTP de manière déclarative. La séparation en sous-dossiers `Rne/`, `Pi/`, `Authentication/`, `Documents/` reflète l'organisation des APIs INPI elles-mêmes, ce qui rend la maintenance plus naturelle.

### 4.6 Atlas.Infrastructure.Persistence

Ce projet contient toute la persistance via Entity Framework Core et PostgreSQL. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Les packages NuGet centraux sont `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design` pour les migrations, et `Npgsql.EntityFrameworkCore.PostgreSQL` pour le provider PostgreSQL. C'est dans ce projet que vit le `AtlasDbContext`, central, ainsi que toutes les configurations EF Core des entités via le pattern `IEntityTypeConfiguration<T>`. Les migrations générées par `dotnet ef migrations add` se déposent dans le sous-dossier `Migrations/`.

### 4.7 Atlas.Infrastructure.Veille

Ce projet contient les adapters de veille : flux RSS, BODACC, et le moteur de déduplication. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Le package NuGet principal est `CodeHollow.FeedReader` pour le parsing RSS et Atom, qui est aujourd'hui la bibliothèque la plus complète et la mieux maintenue dans l'écosystème .NET pour cet usage. On pourrait aussi utiliser `System.ServiceModel.Syndication` qui est natif mais moins riche fonctionnellement. Pour la déduplication par MinHash ou SimHash, on écrira probablement notre propre implémentation puisque les bibliothèques disponibles sont soit incomplètes soit non maintenues.

### 4.8 Atlas.Infrastructure.Messaging

Ce projet centralise l'envoi de messages : emails transactionnels via Brevo, push notifications mobiles via Firebase Cloud Messaging pour Android et Apple Push Notification service pour iOS. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

### 4.9 Atlas.Infrastructure.Security

Ce projet contient tous les composants de sécurité : hashage de mots de passe avec Argon2id, génération et validation de tokens JWT, accès au KMS pour le chiffrement des credentials INPI, et services de chiffrement symétrique AES-GCM. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Les packages NuGet incluent `Konscious.Security.Cryptography.Argon2` pour Argon2id, `OtpNet` pour TOTP, `Microsoft.AspNetCore.Authentication.JwtBearer` pour la gestion JWT, et des packages selon le KMS choisi (`Azure.Security.KeyVault.Keys` pour Azure Key Vault, ou `VaultSharp` pour HashiCorp Vault).

### 4.10 Atlas.Infrastructure.Cache

Ce projet implémente l'interface `ICache<T>` du domaine avec deux backends : Redis pour la production, et in-memory pour le développement local et les tests. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Le package NuGet principal est `StackExchange.Redis` pour le backend Redis, et `Microsoft.Extensions.Caching.Memory` pour le backend in-memory.

### 4.11 Atlas.Infrastructure.Storage

Ce projet gère le stockage de fichiers, principalement les archives ZIP de bilans / actes produites par F-014 (téléchargement en masse) et, à terme, les PDF de fascicules de brevets. Son framework cible est `net10.0`. Il référence `Atlas.Domain`, `Atlas.Application` et `Atlas.Shared`.

Il implémente le port `IFileStorage` défini dans `Atlas.Domain.Storage` (méthodes `SaveAsync` / `OpenReadAsync` / `DeleteAsync`, sans notion de TTL côté port — l'expiration est portée par les entités applicatives). À ce jour un seul adapter est livré : `LocalFileStorage` (filesystem local, configurable via `Storage:Local:RootPath`, sanitization du chemin pour éviter le path traversal). Un adapter S3-compatible (Scaleway Object Storage / MinIO / AWS S3 via `AWSSDK.S3`) est prévu pour la production et viendra dans un lot ultérieur.

### 4.12 Atlas.Api

Le projet `Atlas.Api` est la **composition root** de la solution côté serveur. C'est lui qui démarre l'application, configure l'injection de dépendances en assemblant tous les modules, et expose les endpoints HTTP. Son framework cible est `net10.0`. Il référence tous les projets précédents : `Atlas.Domain`, `Atlas.Application`, `Atlas.Application.Premium`, toutes les `Atlas.Infrastructure.*`, et `Atlas.Shared`.

C'est volontaire et nécessaire : la composition root est le seul endroit où l'on autorise une vue globale du système, parce que c'est là qu'on assemble les pièces. Le `Program.cs` y est minimaliste grâce aux méthodes d'extension `Add*Services()` exposées par chaque projet.

Les packages NuGet typiques incluent `Microsoft.AspNetCore.App` (framework reference, pas un package classique), `Swashbuckle.AspNetCore` pour OpenAPI/Swagger, et `Serilog.AspNetCore` pour le logging.

### 4.13 Atlas.App (client Uno)

Le projet `Atlas.App` est l'**application cliente unique** d'Atlas, écrite une seule fois en **C#/XAML (dialecte WinUI)** avec **Uno Platform** (cf. **ADR-029**). C'est un **single project** Uno (`<Project Sdk="Uno.Sdk">`, `UnoSingleProject`) : un seul `.csproj` multi-cible, le code spécifique à chaque tête vivant sous `Platforms/`. **Têtes activées à ce jour** : `net10.0-browserwasm` (web) et `net10.0-desktop` (Skia — couvre Windows, macOS et **Linux**). Les têtes **`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`** sont prévues (vocation 6 cibles) mais **pas encore ajoutées** (workloads mobiles / runner macOS). Structure interne réelle : `Presentation/Controls` (kit XAML), `Presentation/Pages` (5 destinations + Login), `Services` (DI + `AtlasApiClient` + handlers auth/refresh), `Models` (DTOs client + `JsonSerializerContext`).

Il référence uniquement `Atlas.Domain` et `Atlas.Shared`, conformément à la règle de sécurité énoncée dans la doc 09 et **ADR-002** : le client — y compris la tête WebAssembly, décompilable dans le navigateur — ne doit jamais accéder au code `Infrastructure` (détails techniques et secrets). Cette règle est **verrouillée par un test NetArchTest dédié**, exactement comme prévu pour l'ancien `Atlas.Maui`/`Atlas.Web.Client`.

> **Transition (ADR-029)** : `Atlas.App` (Uno) **remplace** `Atlas.Maui` (projet réel, amorce F-009/F-010), le desktop Avalonia (ADR-026) et le client web Blazor `Atlas.Web`/`Atlas.Web.Client` (ADR-017). En attendant un **spike Uno multi-cible concluant**, le client Blazor `Atlas.Web.Client` reste l'app web en vigueur (et n'est retiré qu'après ce spike, U5).

La section suivante développe les particularités de ce projet multi-cible.

---

## 5. Inventaire détaillé des projets de tests

La solution compte six projets de tests. La stratégie de test reflète directement la stratification de l'architecture. Pour les couches les plus pures, on fait des tests unitaires rapides. Pour les couches qui touchent à des dépendances externes, on fait des tests d'intégration. Et on ajoute un projet dédié aux tests d'architecture pour faire respecter les règles.

Le projet `Atlas.Domain.UnitTests` cible `net10.0`, référence `Atlas.Domain` et utilise `xUnit`, `FluentAssertions`, et éventuellement `Bogus` pour la génération de données de test. Ses tests doivent être ultra-rapides puisqu'aucun n'effectue d'entrée-sortie. Si un test du domaine prend plus de quelques millisecondes, c'est un signe que quelque chose ne va pas.

Le projet `Atlas.Application.UnitTests` cible `net10.0`, référence `Atlas.Application` et les mêmes packages de test, en ajoutant `NSubstitute` ou `Moq` pour mocker les ports utilisés par les handlers. Les tests vérifient que les handlers orchestrent correctement les ports, qu'ils retournent les bons `Result`, et qu'ils respectent les règles métier.

Le projet `Atlas.Infrastructure.Inpi.IntegrationTests` cible `net10.0`, référence `Atlas.Infrastructure.Inpi`, et utilise en plus `WireMock.Net` pour simuler les réponses des APIs INPI sans réellement les appeler. Les tests vérifient que les adapters INPI gèrent correctement les codes d'erreur, les retries Polly, la sérialisation JSON, et le mapping vers les entités domaine.

Le projet `Atlas.Infrastructure.Persistence.IntegrationTests` cible `net10.0`, référence `Atlas.Infrastructure.Persistence`, et utilise `Testcontainers.PostgreSql` pour démarrer une instance PostgreSQL réelle dans un container Docker éphémère le temps des tests. C'est aujourd'hui la méthode standard pour tester EF Core de manière fidèle, infiniment plus fiable que les anciens providers in-memory qui ne reflétaient pas le comportement réel.

Le projet `Atlas.Api.IntegrationTests` cible `net10.0`, référence `Atlas.Api`, et utilise `Microsoft.AspNetCore.Mvc.Testing` pour démarrer une instance de l'API en mémoire et tester les endpoints HTTP de bout en bout. C'est le niveau de test le plus large mais aussi le plus lent ; on en garde quelques uns pour vérifier les flows critiques, mais le gros des tests reste au niveau unitaire et intégration ciblée.

Le projet `Atlas.Architecture.Tests` cible `net10.0`, référence tous les projets de production, et utilise `NetArchTest.Rules` ou `ArchUnitNET` pour exécuter les tests d'architecture présentés dans la doc 09. Ces tests garantissent que les règles de dépendances ne sont pas violées, que les conventions de nommage sont respectées, et que les classes ont les modificateurs d'accès attendus.

---

## 6. Cas particulier — Atlas.App (Uno) et le multi-targeting

Le projet `Atlas.App` mérite une section dédiée parce qu'il est structurellement différent des autres. Là où un projet ASP.NET Core compile vers un seul binaire ciblant `net10.0`, un **single project Uno** compile vers plusieurs binaires différents simultanément, chacun adapté à une cible — depuis **une base de code unique** (C#/XAML WinUI), ce qui est précisément la motivation d'ADR-029 pour un porteur solo.

La déclaration repose sur le SDK Uno et ressemble à ceci :

```xml
<Project Sdk="Uno.Sdk">
  <PropertyGroup>
    <!-- Têtes activées à ce jour (browserwasm + desktop Skia). Ajouter android/ios/maccatalyst
         quand les workloads mobiles / un runner macOS seront en place. -->
    <TargetFrameworks>net10.0-browserwasm;net10.0-desktop</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UnoSingleProject>true</UnoSingleProject>
    <UnoFeatures>SkiaRenderer;</UnoFeatures>
  </PropertyGroup>
</Project>
```

La cible **`net10.0-desktop`** est une tête **Skia** unique qui couvre **Windows, macOS et Linux** — c'est elle qui apporte la souveraineté desktop (Linux natif) visée par ADR-029. Les têtes **`net10.0-ios`/`net10.0-maccatalyst`** ne se compilent que sur une machine macOS (workloads Apple) ; en pratique on conditionne donc la liste des `TargetFrameworks` selon l'OS de build pour qu'un poste Windows/Linux puisse travailler sur la solution sans erreur — la matrice CI multi-tête (à mettre en place) couvre les cibles manquantes.

Le code commun à toutes les cibles vit à la racine du projet et dans `Presentation/` (pages + ViewModels) et `Services/`. Le code spécifique à une tête vit dans `Platforms/WebAssembly/`, `Platforms/Desktop/`, `Platforms/Android/`, etc. ; son contenu n'est compilé que pour la cible correspondante. Le service natif par-OS (secure storage, notifications, fichiers) s'implémente derrière une abstraction commune, avec une réalisation par tête au besoin.

La gestion des dépendances Uno passe **principalement par `<UnoFeatures>`** (le SDK Uno résout les versions cohérentes du bundle) plutôt que par des `<PackageReference>` classiques ; les packages additionnels (ex. `CommunityToolkit.Mvvm`) restent gérés en CPM via `Directory.Packages.props` comme partout ailleurs.

Un point important pour Atlas : `Atlas.App` référence `Atlas.Domain` (value objects, entités) et `Atlas.Shared`. Cela impose que ces deux projets soient compatibles avec **toutes** les têtes Uno. Comme chaque tête est un `net10.0-*` reposant sur `net10.0`, et que `Atlas.Shared` cible `netstandard2.1` (sur-ensemble compatible), la contrainte est satisfaite sans effort — on cible directement `net10.0` sans détour par un netstandard intermédiaire côté `Domain`.

---

## 7. Gestion centralisée des versions NuGet

La gestion centralisée des packages NuGet via `Directory.Packages.props` est l'une des fonctionnalités les plus utiles introduites dans l'écosystème .NET récent. Plutôt que de spécifier les versions dans chaque `.csproj`, on les déclare une seule fois à la racine, et chaque projet référence simplement les packages par leur nom.

Le fichier `Directory.Packages.props` ressemble à ceci, simplifié pour montrer le pattern :

```xml
<Project>
  <!-- Active la gestion centralisée -->
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <!-- Versions des packages utilisés par les projets de la solution -->
  <ItemGroup>
    <!-- MediatR et CQRS -->
    <PackageVersion Include="MediatR" Version="13.0.0" />
    <PackageVersion Include="MediatR.Contracts" Version="2.0.1" />
    
    <!-- Validation -->
    <PackageVersion Include="FluentValidation" Version="12.0.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="12.0.0" />
    
    <!-- Mapping -->
    <PackageVersion Include="Mapster" Version="7.5.0" />
    
    <!-- Entity Framework Core -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
    
    <!-- Sécurité -->
    <PackageVersion Include="Konscious.Security.Cryptography.Argon2" Version="1.4.0" />
    <PackageVersion Include="OtpNet" Version="1.4.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    
    <!-- Résilience -->
    <PackageVersion Include="Polly" Version="8.6.0" />
    <PackageVersion Include="Microsoft.Extensions.Http.Polly" Version="10.0.0" />
    
    <!-- Logging et observabilité -->
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="2.0.0" />
    
    <!-- Veille -->
    <PackageVersion Include="CodeHollow.FeedReader" Version="1.2.6" />
    
    <!-- Cache -->
    <PackageVersion Include="StackExchange.Redis" Version="2.9.0" />
    <PackageVersion Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0" />
    
    <!-- Tests -->
    <PackageVersion Include="xunit" Version="2.10.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="NSubstitute" Version="6.0.0" />
    <PackageVersion Include="Bogus" Version="36.0.0" />
    <PackageVersion Include="WireMock.Net" Version="2.0.0" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="5.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageVersion Include="NetArchTest.Rules" Version="2.0.0" />
    
    <!-- Client Uno (Atlas.App) — le bundle Uno est piloté par <UnoFeatures>/Uno.Sdk ;
         on ne versionne en CPM que les packages additionnels hors bundle -->
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="9.0.0" />
    <PackageVersion Include="Refit.HttpClientFactory" Version="9.0.0" />
  </ItemGroup>
</Project>
```

> La version du SDK Uno (`Uno.Sdk`) se fixe dans `global.json` (propriété `msbuild-sdks`), pas en `Directory.Packages.props` — c'est le SDK qui aligne ensuite les versions des composants Uno activés via `<UnoFeatures>`.

Les numéros de version ci-dessus sont des estimations basées sur ce qui devrait exister en mai 2026 ; à toi de les vérifier au moment de la mise en place via `dotnet list package` ou directement sur nuget.org. L'idée importante est le **principe** : versions centralisées, mise à jour en un seul endroit.

Du côté des `.csproj` individuels, l'allégement est spectaculaire. Une référence de package devient simplement :

```xml
<PackageReference Include="MediatR" />
```

Sans version. La version est résolue depuis `Directory.Packages.props`. C'est plus court, plus maintenable, et impossible à divergencer.

---

## 8. Le fichier global.json et le pinning du SDK

Le fichier `global.json` à la racine du repository épingle la version du SDK .NET utilisée. Voici la version recommandée pour Atlas :

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

La propriété `version` indique la version minimale acceptable du SDK. La propriété `rollForward` avec la valeur `latestFeature` indique que tout SDK 10.0.x est acceptable, ce qui évite de devoir éditer le fichier à chaque patch release. La propriété `allowPrerelease` à `false` exclut les versions release candidate ou previews, ce qui garantit qu'on ne compile pas accidentellement avec un SDK instable.

Quand .NET 11 sortira en novembre 2026, on aura le choix de migrer. Pour un projet en production, je recommanderais d'attendre au moins six mois après une release STS pour observer la stabilité et la disponibilité des packages tiers majeurs. Une migration vers .NET 11 se fera en mettant simplement à jour le `global.json` et les références de packages NuGet, le code C# n'aura probablement aucune ligne à changer.

---

## 9. Scripts de création de la solution depuis zéro

Pour rendre tout ce qui précède concret et reproductible, voici la séquence exacte de commandes `dotnet` à exécuter pour créer la solution Atlas depuis un dossier vide. Ces commandes peuvent être enregistrées dans un script `tools/bootstrap-solution.sh` ou exécutées manuellement la première fois.

```bash
# Création du dossier racine et entrée dedans
mkdir atlas && cd atlas

# Initialisation git
git init

# Création de la solution
dotnet new sln -n Atlas

# Création de l'arborescence de base
mkdir -p src tests docs tools

# Création du projet Shared (cible netstandard2.1 pour maximiser portabilité)
cd src
dotnet new classlib -n Atlas.Shared -f netstandard2.1
cd ..
dotnet sln add src/Atlas.Shared/Atlas.Shared.csproj

# Création du projet Domain
cd src
dotnet new classlib -n Atlas.Domain -f net10.0
cd ..
dotnet sln add src/Atlas.Domain/Atlas.Domain.csproj
dotnet add src/Atlas.Domain/Atlas.Domain.csproj reference src/Atlas.Shared/Atlas.Shared.csproj

# Création du projet Application
cd src
dotnet new classlib -n Atlas.Application -f net10.0
cd ..
dotnet sln add src/Atlas.Application/Atlas.Application.csproj
dotnet add src/Atlas.Application/Atlas.Application.csproj reference src/Atlas.Domain/Atlas.Domain.csproj
dotnet add src/Atlas.Application/Atlas.Application.csproj reference src/Atlas.Shared/Atlas.Shared.csproj

# Création du projet Application.Premium
cd src
dotnet new classlib -n Atlas.Application.Premium -f net10.0
cd ..
dotnet sln add src/Atlas.Application.Premium/Atlas.Application.Premium.csproj
dotnet add src/Atlas.Application.Premium/Atlas.Application.Premium.csproj reference src/Atlas.Application/Atlas.Application.csproj
dotnet add src/Atlas.Application.Premium/Atlas.Application.Premium.csproj reference src/Atlas.Domain/Atlas.Domain.csproj

# Création des projets Infrastructure
for module in Inpi Persistence Veille Messaging Security Cache Storage; do
  cd src
  dotnet new classlib -n "Atlas.Infrastructure.$module" -f net10.0
  cd ..
  dotnet sln add "src/Atlas.Infrastructure.$module/Atlas.Infrastructure.$module.csproj"
  dotnet add "src/Atlas.Infrastructure.$module/Atlas.Infrastructure.$module.csproj" \
    reference src/Atlas.Domain/Atlas.Domain.csproj
  dotnet add "src/Atlas.Infrastructure.$module/Atlas.Infrastructure.$module.csproj" \
    reference src/Atlas.Application/Atlas.Application.csproj
  dotnet add "src/Atlas.Infrastructure.$module/Atlas.Infrastructure.$module.csproj" \
    reference src/Atlas.Shared/Atlas.Shared.csproj
done

# Création du projet Api
cd src
dotnet new webapi -n Atlas.Api -f net10.0 --use-minimal-apis
cd ..
dotnet sln add src/Atlas.Api/Atlas.Api.csproj
# Référence vers Domain, Application, Application.Premium et toutes les Infrastructure
dotnet add src/Atlas.Api/Atlas.Api.csproj reference src/Atlas.Domain/Atlas.Domain.csproj
dotnet add src/Atlas.Api/Atlas.Api.csproj reference src/Atlas.Application/Atlas.Application.csproj
dotnet add src/Atlas.Api/Atlas.Api.csproj reference src/Atlas.Application.Premium/Atlas.Application.Premium.csproj
for module in Inpi Persistence Veille Messaging Security Cache Storage; do
  dotnet add src/Atlas.Api/Atlas.Api.csproj reference "src/Atlas.Infrastructure.$module/Atlas.Infrastructure.$module.csproj"
done

# Création du client Uno (nécessite les templates : dotnet new install Uno.Templates)
cd src
dotnet new unoapp -preset recommended -platforms wasm desktop android ios -o Atlas.App -n Atlas.App
cd ..
dotnet sln add src/Atlas.App/Atlas.App.csproj
dotnet add src/Atlas.App/Atlas.App.csproj reference src/Atlas.Domain/Atlas.Domain.csproj
dotnet add src/Atlas.App/Atlas.App.csproj reference src/Atlas.Shared/Atlas.Shared.csproj

# Création des projets de tests
cd tests
dotnet new xunit -n Atlas.Domain.UnitTests -f net10.0
dotnet new xunit -n Atlas.Application.UnitTests -f net10.0
dotnet new xunit -n Atlas.Infrastructure.Inpi.IntegrationTests -f net10.0
dotnet new xunit -n Atlas.Infrastructure.Persistence.IntegrationTests -f net10.0
dotnet new xunit -n Atlas.Api.IntegrationTests -f net10.0
dotnet new xunit -n Atlas.Architecture.Tests -f net10.0
cd ..

# Ajout des projets de tests à la solution
for test in Atlas.Domain.UnitTests Atlas.Application.UnitTests \
            Atlas.Infrastructure.Inpi.IntegrationTests \
            Atlas.Infrastructure.Persistence.IntegrationTests \
            Atlas.Api.IntegrationTests Atlas.Architecture.Tests; do
  dotnet sln add "tests/$test/$test.csproj"
done

# Référencements croisés tests → src à compléter au fur et à mesure
dotnet add tests/Atlas.Domain.UnitTests/Atlas.Domain.UnitTests.csproj \
  reference src/Atlas.Domain/Atlas.Domain.csproj
dotnet add tests/Atlas.Application.UnitTests/Atlas.Application.UnitTests.csproj \
  reference src/Atlas.Application/Atlas.Application.csproj
# ... etc pour les autres tests

# Création des fichiers de configuration racine
cat > global.json << 'EOF'
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
EOF

# Directory.Build.props et Directory.Packages.props doivent être créés manuellement
# (voir sections précédentes)

# Build initial pour vérifier
dotnet restore
dotnet build
```

Ce script est volontairement verbeux pour être pédagogique. Dans la vraie vie, tu peux l'exécuter en une fois et avoir ta solution prête en quelques minutes. Tu auras ensuite à ajouter les `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitignore` manuellement à la racine.

---

## 10. Conventions de nommage et organisation interne des projets

À l'intérieur de chaque projet, on suit des conventions cohérentes qui facilitent la navigation. Les fichiers sont organisés par **bounded context** ou **feature**, pas par type technique. Cela signifie qu'on préfère un dossier `Companies/` qui contient à la fois `Company.cs`, `Siren.cs`, `ICompanyRepository.cs` plutôt que de séparer en `Entities/`, `ValueObjects/`, `Interfaces/` qui éparpillerait ce qui appartient au même concept.

Cette organisation par feature est particulièrement importante dans `Atlas.Application` où chaque use case forme un sous-dossier auto-contenu. Le use case `GetCompanyBySiren` vit dans `Companies/Queries/GetCompanyBySiren/` et contient les trois fichiers qui lui sont propres : la query, le handler, et éventuellement son validator. Cette structure dite **vertical slice** rend chaque feature complètement isolée et facile à comprendre.

Les namespaces C# suivent strictement la structure des dossiers. Le fichier `src/Atlas.Domain/Companies/ValueObjects/Siren.cs` doit avoir comme namespace `Atlas.Domain.Companies.ValueObjects`. Cette correspondance évite toute ambiguïté entre l'organisation des fichiers et celle du code. C# 10 et suivants permettent par ailleurs les *file-scoped namespaces* qui économisent un niveau d'indentation, à utiliser systématiquement.

Les classes internes au projet doivent être marquées `internal` par défaut. Seules les classes qui font partie du contrat public du projet (les ports d'interface, les entités exposées, etc.) doivent être `public`. Cette discipline rend les contrats explicites et évite les couplages accidentels.

---

## 11. Le .editorconfig recommandé

Le fichier `.editorconfig` à la racine définit les conventions de formatage et de style C# qui seront appliquées automatiquement par tous les IDE. Voici les sections les plus importantes à inclure :

```ini
# Convention pour tout le repo
root = true

# Règles universelles
[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space

# C# spécifique
[*.cs]
indent_size = 4
max_line_length = 120

# Préférences C#
csharp_style_var_for_built_in_types = false:warning
csharp_style_var_when_type_is_apparent = true:warning
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_style_pattern_matching_over_is_with_cast_check = true:warning
csharp_style_namespace_declarations = file_scoped:warning

# Organisation des using
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Nommage des champs privés
dotnet_naming_rule.private_fields_with_underscore.severity = warning
dotnet_naming_rule.private_fields_with_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_with_underscore.style = prefix_underscore

# Markdown, JSON, YAML
[*.{md,json,yml,yaml}]
indent_size = 2

# Projets .csproj
[*.{csproj,props,targets}]
indent_size = 2
```

Le `.editorconfig` complet doit aussi inclure les règles de diagnostic Roslyn (`dotnet_diagnostic.CAxxxx.severity = ...`) pour les analyseurs de code, mais c'est un sujet à part qu'on peut affiner progressivement.

---

## 12. Les .gitignore et .gitattributes

Le `.gitignore` standard pour une solution .NET est fourni par Microsoft. On peut le récupérer avec `dotnet new gitignore`. Il exclut notamment les dossiers `bin/`, `obj/`, les fichiers utilisateur de Visual Studio comme `*.user` et `.vs/`, les caches de Rider, et bien d'autres.

Le `.gitattributes` est moins connu mais utile pour normaliser les fins de ligne entre Windows et Unix. Voici un contenu standard adapté à .NET :

```
* text=auto eol=lf
*.cs text diff=csharp
*.csproj text merge=union
*.sln text eol=crlf
*.{cmd,[cC][mM][dD]} text eol=crlf
*.{bat,[bB][aA][tT]} text eol=crlf
*.png binary
*.jpg binary
*.pdf binary
*.zip binary
*.dll binary
```

La directive `text=auto eol=lf` normalise les fins de ligne en LF (style Unix) pour la majorité des fichiers, ce qui est désormais la convention même sur Windows pour les projets multi-plateformes.

---

## 13. Migration future vers .NET 11

Pour planifier sereinement la migration future vers .NET 11 quand elle deviendra pertinente, on documente ici la procédure type. La migration entre versions majeures de .NET est devenue très simple depuis .NET 5, mais elle reste une opération qui mérite préparation.

La première étape est de mettre à jour le fichier `global.json` pour pointer vers le nouveau SDK et de vérifier que toute l'équipe a installé ce SDK localement. La deuxième étape consiste à mettre à jour tous les `<TargetFramework>` ou `<TargetFrameworks>` dans les `.csproj` pour passer de `net10.0` à `net11.0` (et `net11.0-browserwasm`, `net11.0-desktop`, `net11.0-android`… pour le client Uno, etc.). La troisième étape est de mettre à jour les versions de packages NuGet dans `Directory.Packages.props` pour utiliser les versions compatibles `net11.0`, en commençant par les packages Microsoft (`Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, etc.).

Une fois ces trois étapes effectuées, on lance `dotnet restore` puis `dotnet build` pour identifier les éventuelles erreurs de compilation. Dans la grande majorité des cas, il n'y en a aucune. Les rares cas où il faut intervenir concernent généralement des APIs marquées obsolètes dans la nouvelle version, qu'on peut remplacer en suivant les avertissements du compilateur.

Enfin, on lance la suite complète de tests pour valider qu'il n'y a pas de régression de comportement. Si tout passe, la migration est terminée et on peut commiter le tout sous forme d'un seul changeset dédié.

Le choix entre rester sur .NET 10 LTS ou migrer vers .NET 11 STS dépend des bénéfices concrets apportés par .NET 11. En général, on migre quand .NET 11 apporte une fonctionnalité dont on a besoin (par exemple une amélioration Uno Platform ou Entity Framework Core spécifique), et on reste sur le LTS si rien ne motive le changement. Le support de .NET 10 jusqu'en novembre 2028 nous donne beaucoup de marge.

---

## Annexes

### A.1 Récapitulatif des références entre projets

Pour servir de référence rapide, voici le tableau récapitulatif des références autorisées entre projets de production.

| Projet | Référence |
|---|---|
| Atlas.Shared | (aucune) |
| Atlas.Domain | Atlas.Shared |
| Atlas.Application | Atlas.Domain, Atlas.Shared |
| Atlas.Application.Premium | Atlas.Application, Atlas.Domain, Atlas.Shared |
| Atlas.Infrastructure.* | Atlas.Application, Atlas.Domain, Atlas.Shared |
| Atlas.Api | tous les précédents |
| Atlas.App (Uno) | Atlas.Domain, Atlas.Shared (uniquement) |

### A.2 Récapitulatif des frameworks cibles

| Projet | Framework cible |
|---|---|
| Atlas.Shared | netstandard2.1 |
| Atlas.Domain | net10.0 |
| Atlas.Application | net10.0 |
| Atlas.Application.Premium | net10.0 |
| Atlas.Infrastructure.* | net10.0 |
| Atlas.Api | net10.0 |
| Atlas.App (Uno) | net10.0-browserwasm, net10.0-desktop *(android/ios/maccatalyst : prévues, pas encore activées)* |
| Tous les projets de tests | net10.0 |

### A.3 Checklist de mise en place

Cette checklist est destinée à servir de référence le jour où on crée effectivement la solution. Elle reprend les étapes critiques dans l'ordre.

Première étape, vérifier que le SDK .NET 10 est installé et que `dotnet --version` retourne bien une version `10.0.x`. Deuxième étape, exécuter le script de création de la solution présenté en section 9. Troisième étape, créer les fichiers `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitignore`, `.gitattributes` à la racine du repository. Quatrième étape, ajouter les packages NuGet de base à chaque projet via `dotnet add package` ou en éditant les `.csproj` à la main. Cinquième étape, lancer `dotnet restore` puis `dotnet build` pour vérifier que toute la solution compile. Sixième étape, lancer `dotnet test` qui ne fera rien pour l'instant puisque les projets de tests sont vides, mais qui valide que l'infrastructure est en place. Septième étape, écrire un premier test d'architecture dans `Atlas.Architecture.Tests` pour vérifier que les règles de dépendance sont bien respectées par le squelette qu'on vient de créer. Et enfin, faire un premier commit avec ce squelette pour fixer le point de départ.

---

*Document évolutif. Toute modification structurelle de la solution doit être documentée ici et faire l'objet d'une décision tracée dans la doc 01.*
