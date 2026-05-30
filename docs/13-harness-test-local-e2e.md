# 13 — Harness de test local de bout en bout

> Comment lancer Atlas en local et exécuter la suite de tests d'API de bout en bout
> (PostgreSQL, API, collection Bruno, configuration INPI). Document opérationnel, à
> garder synchronisé avec `docker-compose.yml`, `bruno/` et `src/Atlas.Api`.

---

## But

Permettre à un contributeur (humain ou Claude Code) de :

1. démarrer une base PostgreSQL de dev,
2. appliquer les migrations,
3. lancer l'API,
4. exécuter la collection Bruno versionnée pour valider l'API de bout en bout.

L'objectif est un cycle reproductible : `register → verify → login → 2FA → données → RGPD`.

---

## Prérequis

| Outil | Usage |
|---|---|
| .NET 10 SDK | build / `dotnet run` / `dotnet ef` |
| Docker Desktop | conteneur PostgreSQL de dev |
| `dotnet-ef` | migrations (`dotnet tool install --global dotnet-ef` si absent) |
| Node.js + `npx` | exécuter `@usebruno/cli` |

---

## Étape 1 — PostgreSQL de dev (Docker)

```bash
docker compose up -d        # à la racine du repo
```

- Conteneur `atlas-postgres` (`postgres:17-alpine`), identifiants `atlas` / `atlas`, base `atlas`.
- **Le port hôte est `5433`, pas `5432`.** Une instance PostgreSQL native est souvent déjà
  présente sur `5432` ; le mappage `5433:5432` évite le conflit. La chaîne de connexion de dev
  (`appsettings.json` et `AtlasDbContextFactory`) pointe sur `5433`.
- Attendre que le healthcheck soit `healthy` avant les migrations :
  `docker inspect --format "{{.State.Health.Status}}" atlas-postgres`.

---

## Étape 2 — Migrations EF Core

```bash
dotnet ef database update --project src/Atlas.Infrastructure.Persistence
```

- **Sans `--startup-project`** : `Atlas.Api` ne référence pas le package EF Core Design ;
  c'est la factory design-time `AtlasDbContextFactory` qui fournit la chaîne de connexion (5433).
- La migration de veille (`AddVeille`) crée notamment le schéma utilisé par Hangfire ; elle doit
  être appliquée avant de lancer l'API si la veille est activée.

---

## Étape 3 — Lancer l'API

```bash
dotnet run --project src/Atlas.Api --launch-profile http
```

- Écoute sur `http://localhost:5023` (profil `http`). Le profil `https` ajoute `https://localhost:7201`.
- En `Development` : clés JWT / crypto auto-générées, cache en mémoire (pas de Redis requis),
  envoi d'email simulé (le lien de vérification n'est pas réellement expédié — voir
  l'endpoint de dev ci-dessous).
- **Il n'existe pas d'endpoint `/health`.** Pour vérifier que le serveur écoute, taper n'importe
  quelle vraie route : un `401`/`404`/`409` prouve déjà que l'API répond.

---

## Étape 4 — Tester avec Bruno

La suite d'API est versionnée dans `bruno/` (fichiers `.bru`), organisée en dossiers numérotés
(`01-Auth`, `02-2FA`, …, `09-Flows`, `10-2FA-Flow`, …).

```bash
# Depuis le dossier de la collection (obligatoire)
cd bruno
npx @usebruno/cli run -r --env Local
```

- **Lancer depuis la racine de la collection** (`bruno/`), sinon :
  `You can run only at the root of a collection`.
- **`-r` est obligatoire** : sans lui, le CLI ne descend pas dans les sous-dossiers.
- Le bac à sable de script Bruno **n'a pas** `require('crypto')` mais **a `crypto-js`** : les codes
  TOTP des scénarios 2FA sont calculés avec `crypto-js`.

### Secrets Bruno

- Les secrets (identifiants INPI) vont dans `bruno/.env` (gitignoré).
- **Jamais** dans `bruno/environments/Local.bru` (versionné), qui les référence via
  `{{process.env.INPI_*}}`.

---

## Configuration INPI (compte réel)

L'intégration INPI utilise un **compte réel**. Points à connaître :

- Tant qu'aucun credential valide n'est configuré (`POST /inpi/connection`), les endpoints
  `companies` / `trademarks` renvoient **`409 inpi.not_connected`** — comportement **normal**.
- Un compte **portail** `inpi.fr` standard **n'est pas** habilité à l'API : `sso/login` renvoie
  `403 connection_type_not_allowed`, que l'API mappe en `403 inpi.api_access_not_allowed`.
- Il faut demander l'accès API **« Entreprises / RNE »** sur `data.inpi.fr`
  (rubrique « Mes accès API / SFTP »), qui fournit des **identifiants techniques** (potentiellement
  différents du login portail).

Les dossiers Bruno qui dépendent d'un compte INPI habilité (`90-INPI-E2E-CI` et les détails
company/trademark/attachments) échouent donc en local sans credentials : c'est attendu.

---

## Endpoints de développement

Mappés **uniquement** si l'environnement est `Development` (cf. `Program.cs`), ils n'existent jamais
en production et servent à automatiser Bruno (`src/Atlas.Api/Endpoints/DevEndpoints.cs`) :

| Endpoint | Rôle |
|---|---|
| `GET /dev/verification-token?email=` | Restitue `{ userId, token }` de vérification email (capturé en RAM par `DevVerificationTokenStore`), pour enchaîner `register → verify` sans accès aux logs. |
| `POST /dev/feed/poll` | Déclenche immédiatement le polling des sources de veille au lieu d'attendre le job Hangfire. |

---

## Veille (F-041) et Hangfire

- Le polling des sources de veille tourne via **Hangfire** (storage PostgreSQL), planifié toutes les
  ~30 min. Sources système seedées au démarrage (`FeedSourceSeeder`).
- Désactivable via la configuration `BackgroundJobs:Enabled=false` (les tests d'intégration le
  désactivent).
- Endpoints associés : `GET /feed/items` (auth), `POST /dev/feed/poll` (dev), dashboard Hangfire
  sur `/hangfire` (dev).

---

## Pièges courants

| Symptôme | Cause / solution |
|---|---|
| `429 Too Many Requests` dès le premier `Register`, puis `401` partout | **Piège n°1 des runs e2e récursifs.** La politique `auth-strict` est à 10 req/60 s par IP en prod. Elle est **relâchée en `Development`** via `appsettings.Development.json` (`AuthStrict` 1000/60, `Global` 10000/60) ; `appsettings.json` reste strict. |
| `You can run only at the root of a collection` | Lancer le CLI Bruno depuis `bruno/`, pas depuis la racine du repo. |
| Sous-dossiers Bruno ignorés | Ajouter `-r` à la commande `run`. |
| `404` sur `/health` | Cet endpoint n'existe pas ; tester une vraie route. |
| Migrations qui ne trouvent pas la base | Vérifier que le conteneur est `healthy` et que le port est bien `5433`. |
| Conflit de port `5432` | Un PostgreSQL natif occupe `5432` ; le conteneur de dev est volontairement sur `5433`. |

---

## Arrêt et nettoyage

```bash
# Arrêter l'API : Ctrl+C dans le terminal de `dotnet run`
docker compose down        # arrête et supprime le conteneur + le réseau
```

Le PostgreSQL natif éventuel sur `5432` n'est pas concerné par `docker compose down`.
