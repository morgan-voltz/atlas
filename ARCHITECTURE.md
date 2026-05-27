# Architecture

> Vue d'ensemble de l'architecture d'Atlas. Pour le détail (inventaire des ports, flux complets,
> justification), voir [`docs/09-architecture-detaillee.md`](docs/09-architecture-detaillee.md).

## Principe

Atlas suit une **architecture hexagonale** (Ports & Adapters) dans sa formulation Clean Architecture :
le **domaine métier** est pur et ne dépend de rien ; il définit des **interfaces (ports)** que les
couches externes **implémentent (adapters)**. Les dépendances pointent toujours **vers l'intérieur**.

```
        ┌───────────────────────────────────────────────┐
        │  Adapters primaires (entrants)                 │
        │  Atlas.Api (Minimal API)   Atlas.Maui (client) │
        └───────────────┬───────────────────────────────┘
                        │ appelle les use cases
        ┌───────────────▼───────────────┐
        │  Atlas.Application (CQRS)      │  use cases MediatR, validation
        └───────────────┬───────────────┘
        ┌───────────────▼───────────────┐
        │  Atlas.Domain                  │  entités, value objects, PORTS
        │  (+ Atlas.Shared)              │  aucune dépendance technique
        └───────────────▲───────────────┘
                        │ implémente les ports
        ┌───────────────┴───────────────────────────────┐
        │  Adapters secondaires (sortants)               │
        │  Infrastructure.{Inpi, Persistence, Security,  │
        │  Messaging, Cache, Storage, Veille}            │
        └────────────────────────────────────────────────┘
```

## Projets et règles de dépendance

Les règles sont **vérifiées automatiquement** par `tests/Atlas.Architecture.Tests` (NetArchTest).

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

## Patterns clés

- **`Result<T>` / `Result`** (`Atlas.Shared`) : les erreurs métier sont des valeurs, jamais des exceptions.
- **Value objects & strongly-typed IDs** : `Siren` (Luhn), `EmailAddress`, `UserId`, `DepositNumber`,
  `NiceClassification`… encapsulent validation et invariants.
- **CQRS léger via MediatR** : chaque interaction est un `IRequest<TResponse>` ; handlers `internal sealed` ;
  `ValidationBehavior` exécute FluentValidation avant le handler (échec → `Result`, sans exception).
- **Ports & adapters** : ports secondaires définis dans `Atlas.Domain` (`IUserRepository`,
  `ICompanyDataProvider`, `IPasswordHasher`, `IJwtIssuer`, `ICryptoService`, `IEmailSender`…),
  implémentés dans `Atlas.Infrastructure.*`.

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

## Sécurité (rappel)

Argon2id (mots de passe), AES-256-GCM (identifiants INPI au repos, jamais loggés), JWT RS256 +
refresh rotatif, RGPD (export / effacement). Détail : [`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md)
et [`SECURITY.md`](SECURITY.md).

## Stack

.NET 10 · ASP.NET Core 10 (Minimal APIs) · EF Core 10 + PostgreSQL (Npgsql) · MediatR · FluentValidation ·
.NET MAUI · MkDocs Material (doc). Versions centralisées dans `Directory.Packages.props`.

## Tests

- **Unitaires** : `Atlas.Domain.UnitTests`, `Atlas.Application.UnitTests` (NSubstitute, FluentAssertions).
- **Architecture** : `Atlas.Architecture.Tests` (règles de dépendance).
- **Intégration** : `Atlas.Infrastructure.Persistence.IntegrationTests` (PostgreSQL/Testcontainers),
  `Atlas.Infrastructure.Inpi.IntegrationTests` (WireMock), `Atlas.Api.IntegrationTests`
  (WebApplicationFactory + Postgres + WireMock, flux end-to-end).
