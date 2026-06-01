# Préproduction Atlas — VPS unique tout-en-un

Déploiement de préproduction (alpha / beta) conforme à **[ADR-019](../../docs/ADR/ADR-019-serveur-preproduction.md)** :
un **seul VPS** (région **UE**, ex. Hetzner) exécute, via `docker compose`, l'API ASP.NET Core,
PostgreSQL, les jobs Hangfire in-process et l'hôte web (statiques WASM), derrière un **reverse proxy
TLS** (Caddy + Let's Encrypt). C'est un **miroir du harness local** (`docs/13`).

> ⚠️ **Préprod uniquement.** Données de test reseedables, **SPOF assumé**, **pas de KMS** (clés en
> fichiers à permissions restreintes), **pas de PITR** (`pg_dump` quotidien). Tout cela est **interdit
> en prod** — la topologie de prod fera l'objet d'un ADR dédié (base managée, KMS, HA, CDN).

## Topologie (domaine unique)

```
                 ┌──────────── Caddy (TLS, 80/443, public) ────────────┐
 https://${ATLAS_DOMAIN}/         → web:8080   (Blazor Web App + WASM)
 https://${ATLAS_DOMAIN}/api/*    → api:8080   (préfixe /api retiré → /auth, /companies, /feed…)
                 └──────────────────────────────────────────────────────┘
   postgres:5432  ·  api (Hangfire in-process)  ·  backup (pg_dump)   — réseau interne, non exposés
```

Même origine pour le web et l'API → **aucun CORS**, cookie de refresh natif. Le client WASM lit
`Api:BaseUrl = "/api/"` (relatif) via `appsettings.Production.json`.

## Prérequis

- Un VPS **2 vCPU / 4 Go / 40–80 Go SSD** (ADR-019), région UE, Docker + Docker Compose v2 installés.
- Un **domaine** dont l'enregistrement DNS A/AAAA pointe vers l'IP du VPS, ports **80 et 443** ouverts
  (nécessaire à l'émission du certificat Let's Encrypt).

## Mise en route

```bash
cd deploy/preprod

# 1) Configuration mono-ligne
cp .env.example .env
$EDITOR .env                     # ATLAS_DOMAIN, ACME_EMAIL, POSTGRES_PASSWORD

# 2) Secrets en fichiers (jamais commités — dossier secrets/ gitignoré)
mkdir -p secrets
#   Clé privée RSA (JWT, RS256) au format PEM PKCS#8 :
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out secrets/jwt-private-key.pem
#   Clé AES-256 (32 octets) en Base64, sur une seule ligne :
openssl rand -base64 32 > secrets/crypto-key.b64
chmod 600 secrets/*

# 3) Build + démarrage (always-on)
docker compose up -d --build
```

Le conteneur `api` applique les **migrations EF au démarrage** (`Database__MigrateOnStartup=true`),
seede les sources de veille / VeillePacks et planifie les jobs Hangfire (polling veille ~30 min,
refresh favoris 03:00 UTC, BODACC 04:00 UTC). Caddy obtient le certificat TLS au premier accès.

Vérifier :

```bash
docker compose ps
curl -fsS https://${ATLAS_DOMAIN}/api/        # API : { "name": "Atlas API", "status": "ok" }
# puis ouvrir https://${ATLAS_DOMAIN}/ dans un navigateur (redirige vers /connexion si non connecté)
```

## Sauvegardes & restauration

Le service `backup` écrit un `pg_dump` (format custom) quotidien dans `./backups/` (rétention 14).

```bash
# Sauvegarde manuelle immédiate
docker compose exec -e PGPASSWORD=$POSTGRES_PASSWORD postgres \
  pg_dump -U atlas -d atlas -F c -f /tmp/manual.dump && \
  docker compose cp postgres:/tmp/manual.dump ./backups/

# Restauration d'un dump
docker compose cp ./backups/atlas-AAAAMMJJ-HHMMSS.dump postgres:/tmp/restore.dump
docker compose exec -e PGPASSWORD=$POSTGRES_PASSWORD postgres \
  pg_restore -U atlas -d atlas --clean --if-exists /tmp/restore.dump
```

## Exploitation

```bash
docker compose logs -f api          # logs applicatifs (Serilog)
docker compose pull && docker compose up -d --build   # mise à jour (rebuild des images)
docker compose down                 # arrêt (conserve les volumes/données)
```

## Essai local (sans domaine public)

Pour tester la composition sans VPS ni Let's Encrypt : dans `Caddyfile`, remplacer le bloc
`{$ATLAS_DOMAIN} { … }` par `localhost { tls internal … }` (certificat auto-signé Caddy), exposer
`443:443`, puis ouvrir `https://localhost`. Les clés/secrets restent requis (env `Production`).

## Notes de conception (rappels ADR-019)

- **Pas de Redis / RabbitMQ** : Hangfire sur PostgreSQL suffit à la charge préprod.
- **Forwarded headers** activés sur l'API (`ForwardedHeaders__Enabled=true`) : l'API est derrière le
  proxy TLS, d'où le scheme/IP réels pour `UseHttpsRedirection` et le rate limiting.
- **Cookie de refresh** en `Path=/` (et non `/auth`) pour rester valide après le retrait du préfixe
  `/api` par Caddy.
- **Clé maître / JWT** en **fichiers montés** (`secrets/`), pas en variables d'environnement — le PEM
  multiligne ne passe pas proprement en `.env`, et c'est la posture « fichier à permissions
  restreintes » d'ADR-019. L'intégration **KMS** est un sujet de **prod**.
