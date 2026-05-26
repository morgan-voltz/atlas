# Atlas

> Atlas est un **SaaS open source** developpe en C#/.NET 10 qui agrege les donnees publiques francaises sur les entreprises (RNE) et la propriete industrielle (marques, brevets, dessins & modeles) accessibles via les APIs INPI, combinees a un moteur de veille (RSS, BODACC, BOPI, actualites sectorielles).

**Nom de code :** `Atlas` (provisoire, sera renomme lors d'un refactoring de masse).
**Licence :** AGPL v3.
**Runtime :** .NET 10 LTS — support jusqu'a novembre 2028.

---

## Demarrage rapide

```bash
# Restaurer les dependances
dotnet restore

# Compiler toute la solution
dotnet build

# Lancer les tests
dotnet test

# Lancer l'API en local
dotnet run --project src/Atlas.Api

# Compiler le client MAUI (Android)
dotnet build src/Atlas.Maui -f net10.0-android
```

## Structure de la solution

- **src/** — code de production reparti selon l'architecture hexagonale
  - `Atlas.Shared` — utilitaires transverses (`Result<T>`, etc.)
  - `Atlas.Domain` — coeur metier pur (entites, value objects, ports)
  - `Atlas.Application` — use cases CQRS (commands/queries)
  - `Atlas.Application.Premium` — use cases premium isoles
  - `Atlas.Infrastructure.*` — adapters (Inpi, Persistence, Veille, Messaging, Security, Cache, Storage)
  - `Atlas.Api` — Web API ASP.NET Core (composition root serveur)
  - `Atlas.Maui` — client multi-plateformes (Android, iOS, Windows, macOS)
- **tests/** — projets de tests (units, integration, architecture)
- **docs/** — documentation fondatrice (ADRs, roadmap, vocabulaire, etc.)
- **tools/** — scripts utilitaires

## Documentation

Toute la documentation fondatrice est dans [`docs/`](docs/). Les points d'entree :

- [`CLAUDE.md`](CLAUDE.md) — instructions pour Claude Code et contributeurs (synthese)
- [`docs/01-decisions-architecturales.md`](docs/01-decisions-architecturales.md) — les ADRs
- [`docs/02-roadmap-features.md`](docs/02-roadmap-features.md) — backlog et MVPs
- [`docs/09-architecture-detaillee.md`](docs/09-architecture-detaillee.md) — architecture hexagonale en profondeur
- [`docs/10-layout-solution-dotnet.md`](docs/10-layout-solution-dotnet.md) — layout de la solution .NET

## Securite

Voir [`SECURITY.md`](SECURITY.md) pour la procedure de signalement de vulnerabilites et [`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md) pour le detail des mesures de securite et conformite RGPD.

## Contribuer

Voir `CONTRIBUTING.md` (a venir) et [`docs/05-strategie-repos.md`](docs/05-strategie-repos.md) pour la strategie git, branches et conventional commits.
