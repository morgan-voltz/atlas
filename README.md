# Atlas

> Atlas est un **SaaS open source** développé en C#/.NET 10 qui agrège les données publiques françaises
> sur les entreprises (RNE) et la propriété industrielle (marques, brevets, dessins & modèles) accessibles
> via les API INPI, combinées à un moteur de veille (RSS, BODACC, BOPI, actualités sectorielles).

**Nom de code :** `Atlas` (provisoire, sera renommé lors d'un refactoring de masse).
**Licence :** AGPL v3.
**Runtime :** .NET 10 LTS — support jusqu'à novembre 2028.

---

## État du projet

**MVP 1 — backend complet, clients et documentation amorcés.** Fonctionnalités livrées : inscription /
connexion, 2FA, connexion d'un compte INPI, recherche entreprise (SIREN / dénomination), recherche et
fiche de marques, historique de recherches, RGPD (export / suppression), client MAUI (mobile + desktop),
site de documentation.

Certaines intégrations INPI sont alignées sur la documentation officielle mais restent à confirmer par un
appel authentifié réel (cf. statuts 🟡 dans la [roadmap](docs/02-roadmap-features.md)). Détail des
changements dans [`CHANGELOG.md`](CHANGELOG.md).

## Démarrage rapide

```bash
# Restaurer et compiler
dotnet restore
dotnet build

# Lancer les tests (Docker requis pour les tests d'intégration)
dotnet test

# Appliquer les migrations sur une base PostgreSQL configurée
dotnet ef database update \
  --project src/Atlas.Infrastructure.Persistence \
  --startup-project src/Atlas.Api

# Lancer l'API en local
dotnet run --project src/Atlas.Api

# Compiler le client MAUI (Android)
dotnet build src/Atlas.Maui -f net10.0-android
```

Configuration backend (`src/Atlas.Api/appsettings.json` ou variables d'environnement) :
`ConnectionStrings:Atlas` (PostgreSQL), `Jwt:PrivateKeyPem` (clé RSA), `Crypto:KeyBase64`
(clé AES-256). En production, fournissez ces clés via un KMS.

## Structure de la solution

- **src/** — code de production réparti selon l'architecture hexagonale
  - `Atlas.Shared` — utilitaires transverses (`Result<T>`, etc.)
  - `Atlas.Domain` — cœur métier pur (entités, value objects, ports)
  - `Atlas.Application` — use cases CQRS (commands / queries)
  - `Atlas.Application.Premium` — use cases premium isolés
  - `Atlas.Infrastructure.*` — adapters (Inpi, Persistence, Veille, Messaging, Security, Cache, Storage)
  - `Atlas.Api` — Web API ASP.NET Core (composition root serveur)
  - `Atlas.Maui` — client multi-plateformes (Android, iOS, Windows, macOS)
- **tests/** — projets de tests (unitaires, intégration, architecture)
- **docs/** — documentation fondatrice (ADR, roadmap, vocabulaire, etc.)
- **website/** — site de documentation utilisateur (MkDocs)
- **tools/** — scripts utilitaires

## Documentation

- [`ARCHITECTURE.md`](ARCHITECTURE.md) — vue d'ensemble de l'architecture hexagonale
- [`CHANGELOG.md`](CHANGELOG.md) — historique des changements
- [`CONTRIBUTING.md`](CONTRIBUTING.md) — comment contribuer
- [`CLAUDE.md`](CLAUDE.md) — instructions pour Claude Code et contributeurs (synthèse)
- [`docs/`](docs/) — documentation fondatrice : [ADR](docs/01-decisions-architecturales.md),
  [roadmap](docs/02-roadmap-features.md), [architecture détaillée](docs/09-architecture-detaillee.md),
  [layout solution](docs/10-layout-solution-dotnet.md)
- [`website/`](website/) — site de documentation utilisateur (installation, premiers pas, FAQ)

## Sécurité

Voir [`SECURITY.md`](SECURITY.md) pour la procédure de signalement de vulnérabilités et
[`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md) pour le détail des mesures de sécurité et
de conformité RGPD.

## Contribuer

Voir [`CONTRIBUTING.md`](CONTRIBUTING.md) et [`docs/05-strategie-repos.md`](docs/05-strategie-repos.md)
pour la stratégie git, les branches et les Conventional Commits.
