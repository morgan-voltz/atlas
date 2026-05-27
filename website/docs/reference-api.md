# Référence API

API REST d'Atlas. Toutes les réponses sont en JSON. Les erreurs suivent le format
**ProblemDetails** (RFC 7807) : `title` reprend un code stable (ex. `users.invalid_credentials`),
`detail` le message lisible, `status` le code HTTP.

!!! info "Authentification"
    Les endpoints marqués 🔒 exigent un en-tête `Authorization: Bearer <accessToken>` (JWT obtenu à
    la connexion). Le **refresh token** est déposé dans un cookie httpOnly `atlas_refresh` et n'est
    jamais exposé dans le corps des réponses.

## Authentification

| Méthode | Chemin | Corps | Réponse |
|---|---|---|---|
| POST | `/auth/register` | `{ email, password }` | `200` message · `409` email déjà utilisé · `400` validation |
| GET | `/auth/verify-email?userId=&token=` | — | `200` email vérifié · `400` lien invalide/expiré |
| POST | `/auth/login` | `{ email, password }` | `200` jetons **ou** défi 2FA · `401` identifiants · `403` email non vérifié · `423` verrouillé |
| POST | `/auth/refresh` | — *(cookie)* | `200` nouveaux jetons · `401` |
| POST | `/auth/logout` | — *(cookie)* | `204` |
| POST | `/auth/2fa/verify` | `{ challengeToken, code }` | `200` jetons · `401` code/défi invalide |
| POST 🔒 | `/auth/2fa/setup` | — | `200 { secret, provisioningUri }` |
| POST 🔒 | `/auth/2fa/enable` | `{ code }` | `200 { recoveryCodes: [...] }` |
| POST 🔒 | `/auth/2fa/disable` | `{ code }` | `204` |

**`POST /auth/login`** — réponse sans 2FA :

```json
{ "accessToken": "eyJ...", "expiresAt": "2026-05-27T12:15:00+00:00" }
```

Si le 2FA est activé, la connexion renvoie un défi à confirmer via `/auth/2fa/verify` :

```json
{ "twoFactorRequired": true, "challengeToken": "eyJ..." }
```

## Compte INPI 🔒

| Méthode | Chemin | Corps | Réponse |
|---|---|---|---|
| POST | `/inpi/connection` | `{ username, password }` | `204` · `400` identifiants INPI invalides · `502` INPI indisponible |
| GET | `/inpi/connection` | — | `200 { connected, status, lastTestedAt }` |
| DELETE | `/inpi/connection` | — | `204` |

## Entreprises 🔒

| Méthode | Chemin | Réponse |
|---|---|---|
| GET | `/companies?name=&page=&pageSize=` | `200` page de résumés `{ siren, denomination, ville, nafCode }` |
| GET | `/companies/{siren}` | `200` fiche · `400` SIREN invalide · `404` introuvable · `409` compte INPI non connecté |

**`GET /companies/{siren}`** :

```json
{
  "siren": "552032534",
  "denomination": "RENAULT",
  "formeJuridique": "5710",
  "nafCode": "2910Z",
  "nafLabel": null,
  "adresse": { "line": "122 AV du General Leclerc", "postalCode": "92100", "city": "Boulogne-Billancourt", "country": "France" },
  "dateCreation": "1990-01-15",
  "isDiffusible": true,
  "dirigeants": [ { "nom": "Dupont", "qualite": "Président" } ]
}
```

## Marques 🔒

| Méthode | Chemin | Réponse |
|---|---|---|
| GET | `/trademarks?name=&page=&pageSize=` | `200` page de résumés `{ denomination, deposant, depositNumber, dateDepot, statutJuridique }` |
| GET | `/trademarks/{depositNumber}` | `200` notice (classes de Nice, `hasImage`) · `404` introuvable |
| GET | `/trademarks/{depositNumber}/image` | `200` image binaire · `404` pas d'image |

## Historique 🔒

| Méthode | Chemin | Réponse |
|---|---|---|
| GET | `/search-history` | `200` liste `{ type, query, createdAt }` (200 entrées max) |

## Compte & RGPD 🔒

| Méthode | Chemin | Réponse |
|---|---|---|
| GET | `/account/export` | `200` export JSON des données personnelles (sans données sensibles) |
| DELETE | `/account` | `204` — suppression du compte et effacement en cascade |

## Pagination

Les listes renvoient un objet paginé :

```json
{ "items": [ ... ], "page": 1, "pageSize": 20, "totalCount": 42 }
```
