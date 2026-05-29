# API Atlas — inventaire des endpoints

> **Périmètre** : tous les endpoints HTTP exposés par `Atlas.Api`, groupés par domaine fonctionnel.
> Document **vivant** : à mettre à jour à chaque PR qui ajoute, modifie ou supprime un endpoint.

**Dernière mise à jour** : 29 mai 2026 — après merge F-014 (PR à venir).

---

## Conventions générales

### Authentification

| Mécanisme | Détail |
|---|---|
| **Schéma** | `Authorization: Bearer {access_token}` (JWT RS256) |
| **Émission** | `POST /auth/login` ou `POST /auth/2fa/verify` |
| **Durée de vie** | 15 minutes (`Jwt:AccessTokenMinutes`) |
| **Refresh** | `POST /auth/refresh` (cookie HttpOnly `atlas_refresh`, rotatif) |

Légende dans le tableau d'inventaire :
- 🔓 **Anonyme** — pas d'authentification requise.
- 🔐 **Authentifié** — `Authorization: Bearer …` obligatoire.
- 🚦 **Rate-limited** — politique `auth-strict` (10 req / 60 s par IP ou par `sub`).
- 🧪 **Dev only** — actif uniquement si `ASPNETCORE_ENVIRONMENT=Development`.

### Format des erreurs

Toutes les erreurs métier respectent **RFC 7807 ProblemDetails** :

```json
{
  "type": "users.invalid_credentials",
  "title": "users.invalid_credentials",
  "status": 401,
  "detail": "Identifiants invalides."
}
```

Mapping `code métier → status HTTP` centralisé dans [`ErrorHttpMapping.cs`](../src/Atlas.Api/Endpoints/ErrorHttpMapping.cs).

### Sécurité transverse (Lots 2a + 2b)

- **CORS** : seules les origines listées dans `Cors:AllowedOrigins` sont autorisées (jamais `*`).
- **Headers de sécurité** : `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-Frame-Options: DENY`, `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'`, `Permissions-Policy` restrictive.
- **HSTS** hors Development.
- **Rate limiting global** : 100 req / 60 s par `sub` (ou par IP si anonyme).

---

## Health

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/` | 🔓 | Health check minimal — `{ name: "Atlas API", status: "ok" }` |

---

## Authentification — `/auth/*`

Cf. [`AuthEndpoints.cs`](../src/Atlas.Api/Endpoints/AuthEndpoints.cs) et [`AuthContracts.cs`](../src/Atlas.Api/Endpoints/AuthContracts.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/register` | 🔓 🚦 | Inscription. Body `{ email, password }`. Envoie un email de vérification. |
| `GET` | `/auth/verify-email?userId=&token=` | 🔓 | Active le compte à partir du lien email. |
| `POST` | `/auth/login` | 🔓 🚦 | Authentification mot de passe. Body `{ email, password }`. Retourne soit `{ accessToken, expiresAt }` + cookie refresh, soit `{ twoFactorRequired: true, challengeToken }`. |
| `POST` | `/auth/refresh` | 🔓 🚦 | Rotation du refresh token (cookie `atlas_refresh`). Retourne un nouvel `accessToken`. |
| `POST` | `/auth/logout` | 🔓 | Révoque le refresh token + supprime le cookie. Idempotent. |

### 2FA TOTP — `/auth/2fa/*`

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/2fa/setup` | 🔐 | Génère un secret TOTP, retourne `{ secret, qrUrl, recoveryCodes }`. |
| `POST` | `/auth/2fa/enable` | 🔐 | Active la 2FA après vérification d'un code. Body `{ code }`. |
| `POST` | `/auth/2fa/disable` | 🔐 | Désactive la 2FA. Body `{ code }`. |
| `POST` | `/auth/2fa/verify` | 🔓 🚦 | Valide le code TOTP lors d'une connexion. Body `{ challengeToken, code }`. Retourne `{ accessToken, expiresAt }` + cookie refresh. |

### Codes d'erreur principaux

| Code | HTTP | Sens |
|---|---|---|
| `users.email_already_in_use` | 409 | Email déjà inscrit |
| `users.invalid_credentials` | 401 | Login ou mot de passe invalide |
| `users.email_not_verified` | 403 | Compte non vérifié |
| `users.account_locked` | 423 | Trop d'échecs (verrouillage temporaire) |
| `users.invalid_refresh_token` | 401 | Cookie refresh invalide/expiré |
| `users.invalid_two_factor_code` | 401 | Code TOTP invalide |
| `users.invalid_two_factor_challenge` | 401 | Challenge token expiré/invalide |
| `users.two_factor_already_enabled` | 409 | 2FA déjà actif |
| `users.two_factor_not_enabled` | 409 | 2FA non actif |

---

## Connexion compte INPI — `/inpi/connection`

Cf. [`InpiEndpoints.cs`](../src/Atlas.Api/Endpoints/InpiEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/inpi/connection` | 🔐 | Stocke les identifiants INPI chiffrés (AES-256-GCM) après test de connexion `/sso/login`. Body `{ username, password }`. |
| `GET` | `/inpi/connection` | 🔐 | Statut du compte INPI (`Active`, `Invalid`, `NotConfigured`). Jamais les credentials. |
| `DELETE` | `/inpi/connection` | 🔐 | Supprime les credentials chiffrés. |

| Code | HTTP | Sens |
|---|---|---|
| `inpi.invalid_credentials` | 400 | Test `/sso/login` refusé |
| `inpi.api_access_not_allowed` | 403 | Compte INPI sans droit API |
| `inpi.unavailable` | 502 | RNE injoignable |
| `inpi.not_connected` | 409 | Aucun compte INPI lié (pour les endpoints qui en ont besoin) |

---

## Entreprises — `/companies`

Cf. [`CompaniesEndpoints.cs`](../src/Atlas.Api/Endpoints/CompaniesEndpoints.cs). Tous 🔐 et nécessitent `inpi.not_connected` = false.

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/companies?name=&page=&pageSize=` | 🔐 | Recherche par dénomination. Pagination. |
| `GET` | `/companies/{siren}` | 🔐 | Fiche complète (identité, NAF, adresse, dirigeants…). Publie `SearchPerformedNotification`. |
| `GET` | `/companies/{siren}/attachments` | 🔐 | F-013 — liste des actes et bilans (`{ id, type, name, depositedAt?, sizeBytes?, isConfidential }`). |
| `GET` | `/companies/{siren}/attachments/{id}/download` | 🔐 | F-013 — téléchargement binaire du document (proxy INPI). |
| `GET` | `/companies/{siren}/report.pdf` | 🔐 | F-022 — rapport PDF (identité, NAF, adresse, dirigeants, documents). |

| Code | HTTP | Sens |
|---|---|---|
| `companies.invalid_siren` | 400 | SIREN mal formé (9 chiffres + Luhn) |
| `companies.not_found` | 404 | SIREN inconnu du RNE |
| `companies.attachment_not_found` | 404 | Attachment ID inconnu |
| `companies.attachment_confidential` | 403 | Bilan déclaré confidentiel par le déposant |

---

## Marques — `/trademarks`

Cf. [`TrademarksEndpoints.cs`](../src/Atlas.Api/Endpoints/TrademarksEndpoints.cs). Tous 🔐 et nécessitent INPI connecté.

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/trademarks?name=&page=&pageSize=` | 🔐 | Recherche par dénomination (INPI PI). |
| `GET` | `/trademarks/{depositNumber}` | 🔐 | Notice détaillée (classes de Nice, dates, statut). |
| `GET` | `/trademarks/{depositNumber}/image` | 🔐 | Proxy binaire de l'image/logo. |

| Code | HTTP | Sens |
|---|---|---|
| `trademarks.not_found` | 404 | Numéro de dépôt inconnu |
| `trademarks.image_not_found` | 404 | Pas d'image associée |

---

## Brevets — `/patents` (F-015 / F-016)

Cf. [`PatentsEndpoints.cs`](../src/Atlas.Api/Endpoints/PatentsEndpoints.cs). 🔐 et nécessite INPI connecté.

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/patents?title=&inventor=&applicant=&page=&pageSize=` | 🔐 | F-016 — recherche multi-critères. Au moins un critère requis. Pagination clampée à `[1, 100]`. |
| `GET` | `/patents/{publicationNumber}` | 🔐 | F-015 — notice brevet : titre, déposant, inventeurs, dates, statut, abrégé. Le numéro est normalisé (majuscules, sans espace). |

| Code | HTTP | Sens |
|---|---|---|
| `patents.invalid_publication_number` | 400 | Format invalide (longueur 4-32, lettres/chiffres/`-`/`.`/`/`) |
| `patents.not_found` | 404 | Numéro de publication inconnu |
| `patents.empty_search` | 400 | Aucun critère de recherche fourni |

---

## Historique de recherches — `/search-history`

Cf. [`SearchHistoryEndpoints.cs`](../src/Atlas.Api/Endpoints/SearchHistoryEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/search-history` | 🔐 | 200 dernières recherches de l'utilisateur (`type`, `query`, `createdAt`). |

---

## Compte utilisateur (RGPD) — `/account`

Cf. [`AccountEndpoints.cs`](../src/Atlas.Api/Endpoints/AccountEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/account/export` | 🔐 | Export RGPD (art. 20) — JSON structuré, **sans** hash de mot de passe ni credentials INPI. |
| `DELETE` | `/account` | 🔐 | Suppression du compte (art. 17). **Cascade FK** sur tout : refresh tokens, INPI credentials, codes 2FA, historique, favoris, snapshots, devices, mentions, événements. |

---

## Favoris entreprise — `/favorites/companies` (F-017)

Cf. [`FavoritesEndpoints.cs`](../src/Atlas.Api/Endpoints/FavoritesEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/favorites/companies` | 🔐 | Marquer une entreprise en favori. Body `{ siren, name? }`. |
| `DELETE` | `/favorites/companies/{siren}` | 🔐 | Retirer un favori. |
| `GET` | `/favorites/companies` | 🔐 | Liste de mes favoris (`siren`, `name?`, `addedAt`), tri AddedAt desc. |
| `GET` | `/favorites/companies/export` | 🔐 | F-021 — export CSV. Fichier `favoris-entreprises.csv`. |

### Favoris marques (F-018)

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/favorites/trademarks` | 🔐 | Marquer une marque en favori. Body `{ depositNumber, name? }`. |
| `DELETE` | `/favorites/trademarks/{depositNumber}` | 🔐 | Retirer un favori. |
| `GET` | `/favorites/trademarks` | 🔐 | Mes marques favorites (tri AddedAt desc). |
| `GET` | `/favorites/trademarks/export` | 🔐 | F-021 — export CSV (RFC 4180, UTF-8 + BOM). Fichier `favoris-marques.csv`. |

### Favoris brevets (F-018)

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/favorites/patents` | 🔐 | Marquer un brevet en favori. Body `{ publicationNumber, title? }`. Numéro normalisé. |
| `DELETE` | `/favorites/patents/{publicationNumber}` | 🔐 | Retirer un favori. |
| `GET` | `/favorites/patents` | 🔐 | Mes brevets favoris (tri AddedAt desc). |
| `GET` | `/favorites/patents/export` | 🔐 | F-021 — export CSV. Fichier `favoris-brevets.csv`. |

### Codes d'erreur favoris

| Code | HTTP | Sens |
|---|---|---|
| `favorites.company_already_favorite` | 409 | SIREN déjà en favori |
| `favorites.company_not_favorite` | 404 | Tentative de retrait sur un SIREN non favori |
| `favorites.invalid_siren` | 400 | SIREN mal formé |
| `favorites.trademark_already_favorite` | 409 | Marque déjà en favori |
| `favorites.trademark_not_favorite` | 404 | Marque non favori |
| `favorites.invalid_deposit_number` | 400 | Numéro de dépôt vide |
| `favorites.patent_already_favorite` | 409 | Brevet déjà en favori |
| `favorites.patent_not_favorite` | 404 | Brevet non favori |
| `favorites.invalid_publication_number` | 400 | Numéro de publication invalide |

---

## Devices push — `/devices` (F-020)

Cf. [`DevicesEndpoints.cs`](../src/Atlas.Api/Endpoints/DevicesEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/devices` | 🔐 | Enregistre / met à jour un token push. Body `{ platform, token, label? }`. `platform ∈ { "FcmAndroid", "ApnsIos", "WindowsWns", "MacOsApns" }`. Upsert sur token. |
| `DELETE` | `/devices/{id}` | 🔐 | Désenregistre un device. Refuse les devices d'un autre user (404). |
| `GET` | `/devices` | 🔐 | Liste de mes devices (sans le token). Tri `lastSeenAt` desc. |

| Code | HTTP | Sens |
|---|---|---|
| `devices.not_found` | 404 | Device inconnu OU appartient à un autre user |
| `devices.invalid_token` | 400 | Token vide |
| `devices.invalid_platform` | 400 | Plateforme inconnue |

**Dispatch des notifications** : `INotificationDispatcher` → `CompositeNotificationDispatcher` → fan-out vers `FcmNotificationDispatcher` (Android + Web Push), `ApnsNotificationDispatcher` (iOS + macOS) et `WnsNotificationDispatcher` (Windows desktop), selon les plateformes configurées (`Fcm:*`, `Apns:*`, `Wns:*`). Cleanup auto des tokens morts (FCM 404/UNREGISTERED, APNs 410/BadDeviceToken, WNS 410/404).

---

## Veille — `/feed/*` (F-041 à F-047)

Cf. [`FeedEndpoints.cs`](../src/Atlas.Api/Endpoints/FeedEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/feed/items?page=&pageSize=` | 🔐 | Items RSS récents (toutes sources confondues, non personnalisé). |
| `POST` | `/feed/sources` | 🔐 | Ajout libre d'une source RSS/Atom. Body `{ url, name? }`. Anti-SSRF (rejet IP privées, loopback, 169.254/16). |
| `GET` | `/feed/subscriptions` | 🔐 | Mes abonnements de veille. |
| `DELETE` | `/feed/subscriptions/{id}` | 🔐 | Désabonnement. |
| `GET` | `/feed/timeline?…` | 🔐 | Timeline mixte personnalisée (RSS + RNE + BODACC). Voir paramètres ci-dessous. |
| `PATCH` | `/feed/items/{id}/state` | 🔐 | Marque un item lu/favori/archivé. Body `{ isRead?, isFavorite?, isArchived? }`. |

### Paramètres de `GET /feed/timeline`

| Paramètre | Type | Défaut | Description |
|---|---|---|---|
| `page` | int | 1 | Page courante |
| `pageSize` | int | 20 | Taille de page (max 100 selon validator) |
| `sourceId` | guid? | — | Filtre par source RSS (F-044) |
| `after` | datetime? | — | Borne inférieure `OccurredAt` |
| `before` | datetime? | — | Borne supérieure `OccurredAt` |
| `keyword` | string? | — | Filtre titre/résumé (case-insensitive) |
| `unread` | bool | false | Seulement les items RSS non lus |
| `favorites` | bool | false | Seulement les items RSS marqués favoris |
| `includeArchived` | bool | false | Inclure les items archivés |
| `mentionsFavoritesOnly` | bool | false | F-047 — seulement les items RSS qui mentionnent un favori de l'utilisateur |

**DTO de réponse** — union discriminée :
```jsonc
{
  "kind": "RssItem" | "FavoriteEvent",
  "id": "...",
  "title": "...",
  "summary": "...",
  "occurredAt": "...",
  "isRead": false, "isFavorite": false, "isArchived": false,
  // RssItem only
  "sourceId": "...", "url": "...", "categories": [...],
  "sourceCount": 1,                    // F-045 (« N sources rapportent »)
  "mentionedFavorites": [{...}],       // F-047
  // FavoriteEvent only
  "eventType": "RneChanged" | "BodaccPublished",
  "eventSiren": "552032534"
}
```

**Fusion** : items RSS (jusqu'à 500) + `FavoriteEvent` (jusqu'à 500) mergés en mémoire, triés par `occurredAt` desc, paginés. Events exclus si filtre RSS-only actif (`sourceId`, `unread`, `favoritesOnly`, `mentionsFavoritesOnly`).

### Codes d'erreur Veille

| Code | HTTP | Sens |
|---|---|---|
| `veille.subscription_not_found` | 404 | Abonnement inconnu |
| `veille.feed_item_not_found` | 404 | Item inconnu |
| `veille.already_subscribed` | 409 | Déjà abonné à cette source |
| `veille.subscription_limit_reached` | 409 | Plafond utilisateur atteint |
| `veille.source_blocked` | 403 | URL refusée par la policy (anti-SSRF / blocklist) |
| `veille.fetch_failed` | 502 | RSS injoignable |
| `veille.feed_unreachable` / `veille.invalid_feed_source` | 400 | URL invalide |

---

## Packs de veille — `/veille/packs` (F-042)

Cf. [`VeillePackEndpoints.cs`](../src/Atlas.Api/Endpoints/VeillePackEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/veille/packs` | 🔐 | Catalogue des packs disponibles (avec version courante). |
| `GET` | `/veille/packs/mine` | 🔐 | Mes inscriptions (avec version snapshot par pack). |
| `POST` | `/veille/packs/{code}/apply` | 🔐 | S'inscrire à un pack (les sources du pack **ignorent la limite** F-043). |
| `POST` | `/veille/packs/{code}/sync` | 🔐 | Re-synchroniser un pack après upgrade de version. |

| Code | HTTP | Sens |
|---|---|---|
| `veille.veille_pack_not_found` | 404 | Code de pack inconnu |
| `veille.veille_pack_not_enrolled` | 409 | Tentative de sync sans inscription préalable |
| `veille.invalid_veille_pack` | 400 | Code mal formé |

---

## Téléchargements en masse — `/downloads` (F-014)

Cf. [`DownloadsEndpoints.cs`](../src/Atlas.Api/Endpoints/DownloadsEndpoints.cs). Tous 🔐 et nécessitent INPI connecté (pour le job d'arrière-plan).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/downloads/bulk` | 🔐 | Crée un job pour une liste de SIREN (≤ 50). Body `{ "sirens": ["…", "…"] }`. Renvoie `202` avec `Location: /downloads/bulk/{jobId}` et `{ jobId }`. |
| `GET` | `/downloads/bulk/{jobId}` | 🔐 | Statut du job : `Pending` → `Running` → `Ready` ou `Failed`. Renvoie `archiveKey`, `errorMessage`, `requestedAt`, `completedAt`, `expiresAt`. |
| `GET` | `/downloads/bulk/{jobId}/archive` | 🔐 | Stream binaire ZIP. Disponible uniquement si statut `Ready` et non expiré. |

**TTL d'archive** : 24 h à compter de la création du job. Après expiration, `GET /archive` renvoie `410 Gone`.

**Arborescence du ZIP** : un dossier par SIREN, contenant les documents publics retournés par INPI RNE (les confidentiels sont filtrés à la source). Un SIREN qui échoue n'arrête pas les autres (best-effort par entreprise).

| Code | HTTP | Sens |
|---|---|---|
| `downloads.empty_sirens` | 400 | Liste vide |
| `downloads.too_many_sirens` | 400 | Plus de 50 SIREN |
| `downloads.invalid_siren` | 400 | Un SIREN ne passe pas Luhn |
| `downloads.not_found` | 404 | Job inconnu, ou propriété d'un autre utilisateur |
| `downloads.not_ready` | 409 | Archive demandée alors que le job n'est pas `Ready` |
| `downloads.expired` | 410 | Archive expirée (TTL dépassé) |

---

## Endpoints de développement — `/dev/*` 🧪

Actifs **uniquement** si `ASPNETCORE_ENVIRONMENT=Development`. Cf. [`DevEndpoints.cs`](../src/Atlas.Api/Endpoints/DevEndpoints.cs).

| Méthode | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/dev/verification-token?email=` | 🔓 🧪 | Récupère le dernier token d'activation pour un email (utilisé par les tests Bruno). |
| `POST` | `/dev/feed/poll` | 🔓 🧪 | Force un cycle de polling RSS + clustering + matching favoris (sans attendre le cron Hangfire). |

Et `/hangfire` (dashboard) est exposé en dev également (`MapHangfireDashboard`).

---

## Jobs Hangfire (background, hors API publique)

Pour mémoire — pas des endpoints HTTP, mais des récurrents Hangfire planifiés au démarrage de l'API :

| Job ID | Cron | Description |
|---|---|---|
| `feed-polling` | `*/30 * * * *` | F-041/F-045/F-047 v1 : poll RSS → clusterise → matche favoris |
| `favorite-refresh` | `0 3 * * *` | F-019 : compare snapshots RNE des favoris, publie notifications |
| `bodacc-polling` | `0 4 * * *` | F-048 : interroge BODACC pour les SIREN favoris, crée `FavoriteEvent` `BodaccPublished` |

Plus des jobs **à la demande** enfilés depuis les endpoints :

| Job | Trigger | Description |
|---|---|---|
| `BulkDownloadJob.RunAsync(jobId)` | `POST /downloads/bulk` (F-014) | Télécharge et zippe les bilans/actes des SIREN demandés, écrit dans `IFileStorage`. Idempotent. |

Désactivables via `BackgroundJobs:Enabled=false` (utilisé par les tests d'intégration).

---

## Mémo pour la maintenance

À chaque PR qui ajoute / modifie un endpoint :

1. Mettre à jour la **table du groupe concerné** (méthode, path, auth, description).
2. Ajouter les **codes d'erreur métier** si nouveaux (table d'erreurs du groupe).
3. Référencer le **fichier source** si nouveau groupe (lien Markdown).
4. Mettre à jour le **header** « Dernière mise à jour » + n° de PR.
5. Mettre à jour les **jobs Hangfire** si un nouveau récurrent est planifié.
