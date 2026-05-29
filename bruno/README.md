# Collection Bruno — Atlas API

Tests d'API versionnés avec [Bruno](https://www.usebruno.com/) (fichiers `.bru` en clair,
git-friendly). Couvre tout le parcours backend : auth, 2FA, connexion INPI, entreprises,
marques, historique, compte/RGPD, **favoris (F-017 + F-018 marques/brevets), devices push
(F-020), veille (F-041 à F-048), packs (F-042), téléchargements en masse (F-014), actes /
bilans individuels (F-013), brevets (F-015 / F-016), exports CSV favoris (F-021) et rapport
PDF de fiche entreprise (F-022)**.

## Prérequis : démarrer le harness local

Depuis la racine du dépôt :

```bash
# 1. PostgreSQL de dev (conteneur sur le port hôte 5433, pour ne pas heurter un PG natif sur 5432)
docker compose up -d

# 2. Appliquer le schéma
dotnet ef database update --project src/Atlas.Infrastructure.Persistence

# 3. Lancer l'API (profil http → http://localhost:5023)
dotnet run --project src/Atlas.Api --launch-profile http
```

L'API démarre en environnement `Development` :
- clés JWT (RS256) et clé de chiffrement générées automatiquement ;
- emails **non envoyés** — le lien de vérification est écrit dans les logs (`LoggingEmailSender`) ;
- push **non envoyés** — `LoggingNotificationDispatcher` log à la place (sauf si `Fcm:*` / `Apns:*` / `Wns:*` configurés) ;
- cache en mémoire (pas de Redis requis).

## Utilisation dans l'app Bruno

1. Ouvrir le dossier `bruno/` dans Bruno (*Open Collection*).
2. Sélectionner l'environnement **Local** (en haut à droite).
3. Jouer les requêtes dans l'ordre des dossiers (`01-Auth` → `14-Veille-Packs`).

### Organisation des dossiers

| Dossier | Feature | Endpoints |
|---|---|---|
| `01-Auth` | F-001 | Register, Verify Email, Login, Refresh, Logout |
| `02-2FA` | F-002 | Setup, Enable, Disable, Verify Challenge |
| `03-INPI` | F-003 | Connect, Status, Disconnect |
| `04-Companies` | F-004 / F-005 | Search, Detail |
| `05-Trademarks` | F-006 / F-007 | Search, Detail, Image |
| `06-Search History` | F-008 | List |
| `07-Account` | F-012 (RGPD) | Export, Delete (destructif) |
| `08-Scenarios` | tests négatifs / sécurité | sans accès INPI |
| `09-Flows` | E2E inscription → RGPD | sans accès INPI |
| `10-2FA-Flow` | E2E cycle 2FA | sans accès INPI |
| `11-Favorites` | **F-017** | Add, List, Remove |
| `12-Devices` | **F-020** | Register, List, Unregister |
| `13-Veille` | **F-041 / F-043 / F-044** | Recent items, Add source, Subscriptions, Unsubscribe, Timeline, Set item state |
| `14-Veille-Packs` | **F-042** | Catalog, Mine, Apply, Sync |
| `15-Downloads` | **F-014** | Request bulk (empty/invalid/too-many/accepted), Get status, Not found, Download archive |
| `16-Company-Attachments` | **F-013** | List (capture `attachmentId` / `confidentialAttachmentId`), Download (PDF), Download not found (404), Download confidential (403) |
| `17-Patents` | **F-015 / F-016** | Search (capture `publicationNumber`), Search empty (400), Detail, Detail not found (404) |
| `18-IP-Favorites` | **F-018** | Trademarks Add/List/Remove (+ Add invalid 400) + Patents Add/List/Remove (+ Add invalid 400) |
| `19-Favorites-Export` | **F-021** | Companies CSV, Trademarks CSV, Patents CSV, Empty CSV (header only) |
| `20-Company-Report` | **F-022** | Report PDF, Report not found (404) |
| `21-Feed-Rules` | **F-046** | 01 Create rule, 02 List, 03 Update, 04 Delete + Create empty (400) |
| `22-Veille-Marketplace` | **F-049** | 01 List community, 02 Create user pack, 03 My authored, 04 Publish, 05 Like, 06 Unlike, 07 Report, 08 Unpublish |
| `23-Accessibility` | **Lot 5c** | 01 Get defaults, 02 Update, 03 Round-trip (GET reflects PUT) |
| `24-Rules-Flow` | E2E F-046 | 01 Create → 02 List contains → 03 Patch → 04 Delete → 05 List excludes |

### Étape manuelle : vérification de l'email

`Register` n'envoie pas d'email en dev. Après l'avoir joué, récupérer le lien dans les
logs de l'API :

```
[DEV] Email de vérification ... ?userId=<GUID>&token=<TOKEN>
```

Copier `<GUID>` dans la variable d'env `userId` et `<TOKEN>` dans `verificationToken`,
puis jouer `Verify Email`. `Login` capture ensuite l'`accessToken` dans une variable
runtime réutilisée par toutes les requêtes protégées.

### Variables d'environnement (`environments/Local.bru`)

| Variable | Rôle |
|---|---|
| `baseUrl` | URL de l'API (`http://localhost:5023`) |
| `email` / `password` | Compte de test |
| `userId` / `verificationToken` | Vérification email (copiés depuis les logs) |
| `challengeToken` / `totpCode` | Parcours 2FA (TOTP à générer depuis le `secret` de Setup) |
| `siren` | SIREN pour la fiche entreprise (défaut RENAULT) |
| `companyName` | Nom snapshot pour le marquage favori (défaut Renault) |
| `depositNumber` | Numéro de dépôt pour la notice de marque |
| `inpiUsername` / `inpiPassword` | **Identifiants techniques API INPI** — lus depuis `bruno/.env` (cf. ci-dessous) |
| `feedSourceUrl` | URL d'un flux RSS pour `13-Veille/Add source` (défaut .NET Blog) |
| `feedItemId` / `subscriptionId` | Capturés runtime par `13-Veille/Recent items` et `Add source` |
| `packCode` | Code de pack pour `Apply`/`Sync` (défaut `pi-cabinet`) |
| `deviceToken` / `devicePlatform` / `deviceId` | Pour `12-Devices` ; `deviceId` capturé runtime par `Register` |
| `bulkJobId` | Capturé runtime par `15-Downloads/04 Request bulk (accepted)` et réutilisé par les calls suivants |
| `attachmentId` / `confidentialAttachmentId` | IDs d'attachments (F-013) — capturés runtime par `16-Company-Attachments/List` (premier item ouvert + premier confidentiel) |
| `publicationNumber` | Numéro de publication brevet (F-015 / F-018) — capturé runtime par `17-Patents/Search` |
| `patentTitle` / `patentInventor` / `patentApplicant` | Critères de recherche brevet (F-016) — au moins un requis |
| `siren404` | SIREN Luhn-valide mais inexistant côté INPI (défaut `123456782`) — utilisé pour `20-Company-Report/Report not found (404)` |
| `ruleId` | ID de règle de surveillance (F-046) — capturé runtime par `21-Feed-Rules/01 Create rule` et `24-Rules-Flow/01` |
| `userPackCode` | Code de pack utilisateur (F-049) — défaut `bruno-test-pack`, capturé par `22-Veille-Marketplace/02 Create user pack` |
| `communityPackCode` | Code d'un pack publié à liker / unliker / signaler (F-049) — défaut `pi-cabinet` (pack système) |

### Secrets : `bruno/.env` (jamais commité)

Les identifiants INPI ne vivent **pas** dans le fichier d'environnement versionné. Copier
`bruno/.env.example` en `bruno/.env` (gitignoré) et y renseigner :

```dotenv
INPI_USERNAME=...
INPI_PASSWORD='...'   # quotes simples si caractères spéciaux ($ % !)
```

`environments/Local.bru` les référence via `{{process.env.INPI_USERNAME}}` /
`{{process.env.INPI_PASSWORD}}`. Bruno (app et CLI) charge automatiquement le `.env`.

> **Accès API requis.** Un compte portail inpi.fr standard n'est **pas** habilité à l'API :
> `Connect` renvoie alors **403 `inpi.api_access_not_allowed`** (l'INPI répond
> `connection_type_not_allowed`). Activer l'accès via data.inpi.fr → « Mes accès API / SFTP »
> et utiliser les identifiants techniques fournis.

> Tant qu'aucune connexion INPI n'est configurée, `04-Companies` et `05-Trademarks`
> renvoient **409 `inpi.not_connected`** : c'est le comportement attendu. Les favoris
> (`11-Favorites`) et la veille (`13-Veille`, `14-Veille-Packs`) **n'ont pas besoin** d'INPI.

## Automatisation en ligne de commande

Avec le CLI Bruno (`@usebruno/cli`) :

```bash
# Un seul dossier
npx @usebruno/cli run 01-Auth --env Local

# Scénarios de sécurité / validation / RGPD (ne nécessitent PAS d'accès INPI)
npx @usebruno/cli run 08-Scenarios --env Local
npx @usebruno/cli run 08-Scenarios/Lockout --env Local   # verrouillage : 5 échecs → 423

# Injecter des variables (ex. token déjà obtenu) et un rapport JSON
npx @usebruno/cli run "06-Search History" --env Local \
  --env-var accessToken=<JWT> --reporter-json results.json
```

> **Flag `-r`** : `bru run <dossier>` ne descend PAS dans les sous-dossiers par défaut.
> Ajouter `-r` pour inclure les sous-dossiers (ex. `08-Scenarios/Lockout`). Éviter de jouer
> toute la collection d'un coup avec `-r` : elle contient `07-Account/Delete (RGPD)` (destructif)
> et `03-INPI/Connect` (qui appelle l'INPI réel).

### Le dossier `08-Scenarios`

Tests négatifs et de sécurité **jouables sans accès INPI** (idéal en attendant l'habilitation) :
validation (mot de passe faible, email invalide), login non vérifié (403) / mauvais mot de passe
(401), JWT absent/trafiqué (401), SIREN mal formé ou clé de Luhn invalide (400), blocage INPI
(`connected:false`, 409), et sous-dossier `Lockout` (verrouillage après 5 échecs → 423). Chaque
scénario crée ses propres comptes jetables (emails horodatés), sans toucher au compte `e2e`.

> ⚠️ `07-Account/Delete (RGPD)` est **destructif** (supprime le compte de test). Il est
> exclu d'un run complet automatisé sauf intention explicite : le rejouer impose de
> recréer le compte via `Register` + `Verify Email`.

### Parcours complets : `09-Flows` et `10-2FA-Flow`

Ces dossiers s'appuient sur un endpoint de **développement** `GET /dev/verification-token?email=…`,
actif **uniquement en environnement Development** (absent en production), qui renvoie le token de
vérification d'email — ce qui automatise entièrement un compte vérifié sans lire les logs.

- **`09-Flows`** : parcours de bout en bout sur un compte jetable — register → vérif email → login →
  export RGPD (assertions : bon compte, aucun hash de mot de passe) → logout → refresh refusé (401) →
  suppression du compte → login impossible (401). Prouve l'invalidation du refresh au logout et
  l'effacement RGPD.
- **`10-2FA-Flow`** : cycle 2FA complet — setup → enable → login renvoie un défi → verify → disable.
  Les codes **TOTP** sont calculés dans les pré-scripts via `crypto-js` (RFC 6238, SHA1 / 6 chiffres / 30 s).

```bash
npx @usebruno/cli run 09-Flows --env Local
npx @usebruno/cli run 10-2FA-Flow --env Local
```

### Nouveaux dossiers Favoris / Devices / Veille / Packs

Les dossiers `11-Favorites`, `12-Devices`, `13-Veille` et `14-Veille-Packs` testent les
endpoints individuels. Les requêtes nécessitent une variable `accessToken` valide —
exécuter d'abord `01-Auth/Login` (capture `accessToken`) ou un flow complet
(`09-Flows`).

```bash
# Test ciblé d'un domaine après login
npx @usebruno/cli run 11-Favorites --env Local --env-var accessToken=<JWT>
npx @usebruno/cli run 12-Devices --env Local --env-var accessToken=<JWT>
npx @usebruno/cli run 13-Veille --env Local --env-var accessToken=<JWT>
npx @usebruno/cli run 14-Veille-Packs --env Local --env-var accessToken=<JWT>
```

Notes :
- `13-Veille/Add source` valide l'URL (parsing + anti-SSRF) ; le polling effectif des
  items se fait en background via le job Hangfire `feed-polling` (cron `*/30 * * * *`).
  Pour forcer un polling immédiat en dev : `POST /dev/feed/poll`.
- `12-Devices/Register` n'envoie **pas vraiment de push** en dev : sans `Fcm:*` / `Apns:*` /
  `Wns:*` configurés, le `LoggingNotificationDispatcher` se contente de logger les
  notifications dans la console de l'API.
- `13-Veille/Timeline` renvoie un DTO union discriminé (`kind = "RssItem" | "FavoriteEvent"`) ;
  les `FavoriteEvent` apparaissent uniquement après un cycle de `favorite-refresh`
  (cron `0 3 * * *`) ou `bodacc-polling` (cron `0 4 * * *`) qui détecte un changement.
- `15-Downloads/04 Request bulk (accepted)` enqueue un job Hangfire qui appelle l'INPI
  pour récupérer les actes et bilans des SIREN demandés. Sans compte INPI connecté,
  le job passe rapidement en `Failed` avec `inpi.not_connected` — c'est visible dans le
  call `05 Get status`. Avec INPI connecté, prévoir quelques secondes avant que le
  statut ne passe à `Ready`, puis lancer `07 Download archive`.
