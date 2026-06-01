# Atlas

> Atlas est un **SaaS open source** développé en C#/.NET 10 qui agrège les données publiques françaises
> sur les entreprises (RNE) et la propriété industrielle (marques, brevets, dessins & modèles) accessibles
> via les API INPI, combinées à un moteur de veille (RSS, BODACC, BOPI, actualités sectorielles).

**Nom de code :** `Atlas` (provisoire, sera renommé lors d'un refactoring de masse).
**Licence :** AGPL v3.
**Runtime :** .NET 10 LTS — support jusqu'à novembre 2028.

---

## État du projet

**Backend (MVP 1 + MVP 2) complet ; client web v1 livré.** Côté **backend** : inscription / connexion,
2FA TOTP, renvoi de vérification & réinitialisation de mot de passe, connexion d'un compte INPI, recherche
entreprise (SIREN / dénomination), actes & bilans RNE, favoris, RGPD (export / suppression), et le
**cluster Veille** (agrégation RSS / Atom, BODACC, timeline unifiée, déduplication, règles de surveillance,
alertes email / push).

Le **client web Blazor (WASM pur, cf. ADR-017)** est **complet (jalons M0 → M7)** et vérifié de bout en
bout en navigateur : onboarding (inscription, vérification email, défi 2FA), connexion, recherche, fiche
entreprise, fil d'accueil, favoris (list-detail à deux panneaux), veille, profil / connexion INPI / RGPD —
suivi dans la [roadmap client web](docs/15-roadmap-client-web.md).

Les **clients natifs** suivent [ADR-026](docs/ADR/ADR-026-clients-natifs-maui-avalonia.md) : **MAUI** pour
le mobile (Android / iOS, slice amorcé), **Avalonia** pour le desktop (Windows / macOS / Linux, à construire).
La **recherche PI** (marques / brevets) reste à débloquer côté INPI (cf. statuts 🟡 dans la
[roadmap](docs/02-roadmap-features.md)). Détail des changements dans [`CHANGELOG.md`](CHANGELOG.md).

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

# Lancer le client web (hôte Blazor — sert le client WASM interactif)
dotnet run --project src/Atlas.Web

# Compiler le client mobile MAUI (Android)
dotnet build src/Atlas.Maui -f net10.0-android
```

> Pour un cycle complet en local (PostgreSQL conteneurisé, migrations, API, collection Bruno),
> voir le [harness de test e2e](docs/13-harness-test-local-e2e.md).

Configuration backend (`src/Atlas.Api/appsettings.json` ou variables d'environnement) :
`ConnectionStrings:Atlas` (PostgreSQL), `Jwt:PrivateKeyPem` (clé RSA), `Crypto:KeyBase64`
(clé AES-256). En production, fournissez ces clés via un KMS.

## Déploiement (préproduction)

La cible de préproduction (alpha / beta) est un **VPS unique tout-en-un** (région UE), conforme à
[ADR-019](docs/ADR/ADR-019-serveur-preproduction.md) : API + PostgreSQL + jobs Hangfire in-process +
hôte web (statiques WASM) orchestrés par `docker compose`, derrière un **reverse proxy TLS Caddy**
(Let's Encrypt). Topologie **domaine unique** : `https://<domaine>/` sert le client web et
`https://<domaine>/api/*` route vers l'API (même origine, donc pas de CORS).

```bash
cd deploy/preprod
cp .env.example .env          # ATLAS_DOMAIN, ACME_EMAIL, POSTGRES_PASSWORD
mkdir -p secrets              # clés JWT (PEM) + AES (Base64) — voir le README dédié
docker compose up -d --build
```

Tout est dans **[`deploy/preprod/`](deploy/preprod/)** (compose, `Caddyfile`, `.env.example`,
sauvegardes `pg_dump`) ; la procédure complète (génération des clés, migrations au démarrage,
sauvegarde / restauration, essai local sans domaine) est détaillée dans
[`deploy/preprod/README.md`](deploy/preprod/README.md). Les images sont produites par les
`Dockerfile` de `src/Atlas.Api` et `src/Atlas.Web`.

> ⚠️ **Préprod uniquement** : SPOF assumé, secrets en fichiers (pas de KMS), `pg_dump` quotidien
> (pas de PITR). La topologie de **production** (base managée, KMS, HA) fera l'objet d'un ADR dédié.

## Structure de la solution

- **src/** — code de production réparti selon l'architecture hexagonale
  - `Atlas.Shared` — utilitaires transverses (`Result<T>`, etc.)
  - `Atlas.Domain` — cœur métier pur (entités, value objects, ports)
  - `Atlas.Application` — use cases CQRS (commands / queries)
  - `Atlas.Application.Premium` — use cases premium isolés
  - `Atlas.Infrastructure.*` — adapters (Inpi, Persistence, Veille, Messaging, Security, Cache, Storage)
  - `Atlas.Api` — Web API ASP.NET Core (composition root serveur)
  - `Atlas.Maui` — client natif **mobile** (Android, iOS) — cf. ADR-026 (le desktop migre vers Avalonia)
  - `Atlas.Web` — hôte du client web Blazor Web App (composition root web)
  - `Atlas.Web.Client` — interactivité Blazor WebAssembly (mêmes règles d'archi que MAUI : `Domain` + `Shared` uniquement)
- **tests/** — projets de tests (unitaires, intégration, architecture)
- **docs/** — documentation fondatrice (ADR, roadmap, vocabulaire, etc.)
- **website/** — site de documentation utilisateur (MkDocs)
- **tools/** — scripts utilitaires

## Documentation

- [`ARCHITECTURE.md`](ARCHITECTURE.md) — vue d'ensemble de l'architecture hexagonale
- [`CHANGELOG.md`](CHANGELOG.md) — historique des changements
- [`CONTRIBUTING.md`](CONTRIBUTING.md) — comment contribuer
- [`CLAUDE.md`](CLAUDE.md) — instructions pour Claude Code et contributeurs (synthèse)
- [`docs/`](docs/) — documentation fondatrice : [index ADR](docs/01-decisions-architecturales.md) (fiches dans [`docs/ADR/`](docs/ADR/)),
  [index roadmap features](docs/02-roadmap-features.md) (fiches dans [`docs/features/`](docs/features/)),
  [architecture détaillée](docs/09-architecture-detaillee.md), [layout solution](docs/10-layout-solution-dotnet.md),
  [API endpoints](docs/11-api-endpoints.md), [UX client MAUI](docs/12-modele-ux-client-maui.md),
  [harness de test e2e](docs/13-harness-test-local-e2e.md), [UX client web](docs/14-modele-ux-client-web.md),
  [roadmap client web](docs/15-roadmap-client-web.md), [polices & lisibilité](docs/16-polices-et-lisibilite.md)
- [`docs/persona/`](docs/persona/) — personas métier (carte + 7 fiches) ; [`docs/design/`](docs/design/) — maquettes UI et polices
- [`website/`](website/) — site de documentation utilisateur (installation, premiers pas, FAQ)

## Sécurité

Voir [`SECURITY.md`](SECURITY.md) pour la procédure de signalement de vulnérabilités et
[`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md) pour le détail des mesures de sécurité et
de conformité RGPD.

## Contribuer

Voir [`CONTRIBUTING.md`](CONTRIBUTING.md) et [`docs/05-strategie-repos.md`](docs/05-strategie-repos.md)
pour la stratégie git, les branches et les Conventional Commits.
