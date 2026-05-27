# Installation (self-hosted)

Atlas est composé d'un backend **ASP.NET Core 10** (API) et d'un client **.NET MAUI**.

## Prérequis

- **.NET 10 SDK**
- **PostgreSQL** (une base accessible)
- Un **compte INPI** (pour interroger les API publiques en votre nom)
- *(optionnel)* **Docker**, pour lancer PostgreSQL rapidement via le `docker-compose.yml` fourni

## 1. Récupérer le code

```bash
git clone git@github.com:<org>/atlas.git
cd atlas
```

## 2. Configurer le backend

Dans `src/Atlas.Api/appsettings.json` (ou via des variables d'environnement / secrets) :

```json
{
  "ConnectionStrings": { "Atlas": "Host=localhost;Port=5433;Database=atlas;Username=atlas;Password=…" },
  "Jwt": { "Issuer": "atlas", "Audience": "atlas", "PrivateKeyPem": "<clé RSA PEM>" },
  "Crypto": { "KeyBase64": "<clé AES-256 base64 (32 octets)>" }
}
```

!!! warning "Clés en production"
    En production, fournissez impérativement `Jwt:PrivateKeyPem` et `Crypto:KeyBase64`
    (idéalement via un KMS). À défaut, des clés de **développement** non sécurisées sont utilisées.

!!! tip "PostgreSQL via Docker"
    `docker compose up -d` à la racine démarre un PostgreSQL prêt à l'emploi, exposé sur le
    port hôte **5433** (et non 5432, pour ne pas heurter une instance native), avec les
    identifiants attendus par la configuration de développement.

## 3. Appliquer les migrations de base de données

```bash
dotnet ef database update --project src/Atlas.Infrastructure.Persistence
```

> La factory design-time (`AtlasDbContextFactory`) fournit la chaîne de connexion ;
> inutile de préciser `--startup-project`.

## 4. Lancer l'API

```bash
dotnet run --project src/Atlas.Api
```

L'API expose notamment : `/auth/*`, `/inpi/connection`, `/companies`, `/trademarks`,
`/search-history`, `/account`.

## 5. Client mobile (MAUI)

```bash
dotnet build src/Atlas.Maui -f net10.0-android
```

Configurez l'URL de l'API dans le client selon votre environnement (l'émulateur Android
joint l'hôte via `10.0.2.2`).
