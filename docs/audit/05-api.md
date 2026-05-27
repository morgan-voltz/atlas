# Audit profond Atlas — Partie 5 : `Atlas.Api`

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-28.

## Contexte de la partie

Composition root ASP.NET Core 10 Minimal API : `Program.cs` (DI de toutes les couches, pipeline, JWT), 9 fichiers d'endpoints (`Auth`, `Account`, `Inpi`, `Companies`, `Trademarks`, `SearchHistory`, `Feed`, `VeillePack`, `Dev`), `ErrorHttpMapping`, seeders + `FeedPollingJob` (Hangfire), `appsettings.json`. ~31 routes.

## Verdict global

**Santé : bonne structure, durcissement HTTP à compléter avant prod + un mapping d'erreurs incomplet.** Le flux d'auth est solide (JWT RS256 bien validé, refresh token en cookie httpOnly/Secure/SameSite=Strict, access 15 min / refresh 30 j, lockout), endpoints **minces** et **autorisés**, surfaces dev/Hangfire **bien gardées**, aucun secret de prod commité. Manquent les **garde-fous HTTP** (rate limiting, HSTS, en-têtes de sécurité, CORS) et le **mapping HTTP des erreurs `veille.*`**.

## Points forts (à préserver)

- ✅ **JWT** : `ValidateIssuer/Audience/Lifetime/IssuerSigningKey`, `ClockSkew 30s`, `MapInboundClaims=false`, algorithme **RS256** (pas de `alg:none`/HS) — config solide (`Program.cs:59-79`).
- ✅ **Refresh token** en cookie **httpOnly + Secure + SameSite=Strict**, `Path=/auth` (`AuthEndpoints.cs:~191`). Access 15 min / refresh 30 j (conforme doc 04 §5.4.3). Lockout 5 essais / 15 min.
- ✅ **Endpoints minces** : `TryGetUserId` → `ISender.Send` → `Result.ToProblem()`. Aucun accès repo/DbContext, **DTO uniquement** (jamais d'entité domaine). Groupes `RequireAuthorization()` corrects (account, inpi, companies, trademarks, feed, veille, search-history).
- ✅ **Surfaces dev gardées** : `MapOpenApi`, `MapDevEndpoints` (token de vérif, poll), `CapturingEmailSender` et `MapHangfireDashboard` tous **derrière `IsDevelopment()`** (`Program.cs:85-89, 116-119`). `BackgroundJobs:Enabled=false` permet de couper les jobs en test.
- ✅ **Pas de secret de prod commité** : `Jwt:PrivateKeyPem`, `Crypto:KeyBase64` à `null`, connection string = creds **dev** locales.
- ✅ DI complète ; `Application.Premium` volontairement non wiré (ADR-009).

## Constats & recommandations

### 🟠 P1 — Erreurs `veille.*` non mappées → toutes en 400
`ErrorHttpMapping.StatusCodeFor` (`ErrorHttpMapping.cs:30-52`) mappe `users.*`/`inpi.*`/`companies.*`/`trademarks.*` mais **aucun** code `veille.*` → défaut **400**. Donc, sur du code mergé (F-041→044) :
- `veille.veille_pack_not_found`, `veille.subscription_not_found`, `veille.feed_item_not_found` → devraient être **404** (sont 400).
- `veille.already_subscribed`, `veille.subscription_limit_reached`, `veille.veille_pack_not_enrolled` → devraient être **409** (sont 400).
- `veille.source_blocked` → **403** ; `veille.feed_unreachable`/`veille.fetch_failed` → **502**.
Sémantique HTTP incorrecte pour toute la veille. **Reco** : ajouter les codes `veille.*` au switch. (Manifestation concrète du constat structurel Partie 1 : un mapping par `switch` string laisse silencieusement passer les nouveaux codes en 400 → envisager une **catégorie typée** sur `Error`.)

### 🟠 P1 — Durcissement HTTP manquant (bloquant avant prod)
Pipeline (`Program.cs:85-93`) = `UseHttpsRedirection` → `UseAuthentication` → `UseAuthorization`. Absents :
- **Rate limiting** (`AddRateLimiter`/`UseRateLimiter`) : aucune limite par IP/utilisateur sur `/auth/login|register|refresh|2fa/verify`. Le lockout domaine (5 essais) protège un compte ciblé, **pas** le credential-stuffing distribué ni l'abus d'inscription. doc 04 §5.8 l'exige.
- **HSTS** (`UseHsts`) : seul `UseHttpsRedirection` est présent → pas d'en-tête `Strict-Transport-Security`.
- **En-têtes de sécurité** : `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, CSP — absents.
- **CORS** : non configuré (à restreindre aux origines front connues avant exposition).
- **Exception handler global** + **logging structuré (Serilog)** : non câblés (s'appuie sur `AddProblemDetails` + `ToProblem` par endpoint ; 500 par défaut sinon). Logging d'audit/sécurité requis par doc 04 §5.7/5.9.
**Reco** : ajouter rate limiter (politiques strictes sur les endpoints auth), `UseHsts` + middleware d'en-têtes, politique CORS, et logging structuré, **avant tout déploiement**.

### 🟠 P1 — Couverture d'intégration C# très mince (la veille n'est testée nulle part au niveau API)
2 méthodes de test C# (`AuthAndCompanyFlowTests`) pour ~31 endpoints (register→verify→login→INPI→company, + 401 sans token). La collection **Bruno** (54 scénarios) compense fortement sur auth/2FA/RGPD/INPI/companies/trademarks/lockout — **mais ne couvre aucun endpoint veille**. Donc **les 10 endpoints veille n'ont aucun test au niveau API** (ni C# ni Bruno) ; les statuts d'erreur (404/409/423/502) ne sont assertés qu'en Bruno.
**Reco** (détaillée en Partie 6) : étendre les tests d'intégration C# (groupes veille en priorité, + statuts d'erreur) ; ajouter des scénarios Bruno veille.

### 🟡 P2 — `/feed/items` global (pas une fuite, mais redondant avec la timeline)
`GET /feed/items` (F-041) renvoie **tous** les items, sans `userId`. Ce **ne sont pas des données privées** (contenu public agrégé de flux RSS) → **pas de fuite** (un sous-agent l'avait classé « data leak », à tort). Mais depuis F-044, `GET /feed/timeline` (user-scoped) est la vraie vue. **Reco** : déprécier/retirer `/feed/items` ou le réserver à un usage admin/debug pour éviter la confusion.

### 🟡 P2 — Détails REST / entrées
- `POST /auth/register` renvoie **200** au lieu de **201 Created** (`AuthEndpoints.cs:45-47`).
- Pagination/longueurs : la plupart des endpoints concernés ont un **validator** FluentValidation (`GetTimeline` pageSize 1..100, `GetRecentFeedItems`, recherches companies/trademarks) → l'entrée hors-bornes est rejetée (pas « non bornée » comme le suggérait un sous-agent). Vérifier que **chaque** query paginée a bien son validator (cf. Partie 3).
- **OpenAPI** : endpoints groupés par `WithTags` mais sans `WithName`/`Produces`/descriptions → doc auto pauvre. P2 (qualité doc).

### 🟡 P2 — Pas de `Cache-Control: no-store` explicite sur les endpoints sensibles
Aucune en-tête de cache sur auth/account/inpi → PII potentiellement mise en cache par des intermédiaires. **Reco** : `no-store` sur les réponses sensibles.

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | Erreurs `veille.*` non mappées → 400 au lieu de 404/409/403/502 | S |
| 2 | 🟠 P1 | Durcissement HTTP (rate limiting, HSTS, en-têtes, CORS, logging) avant prod | M |
| 3 | 🟠 P1 | Intégration C# minime ; veille non testée au niveau API (ni C# ni Bruno) | M |
| 4 | 🟡 P2 | `/feed/items` global redondant avec `/feed/timeline` (pas une fuite) | S |
| 5 | 🟡 P2 | `register` 200 au lieu de 201 ; OpenAPI métadonnées manquantes | S |
| 6 | 🟡 P2 | Pas de `Cache-Control: no-store` sur endpoints sensibles | S |

## Non-issues vérifiés (transparence)

- ✅ JWT : RS256 imposé, pas de `alg:none`/downgrade. Surfaces dev + dashboard Hangfire **bien gardées** (`IsDevelopment`).
- ✅ `/feed/items` n'expose **pas** de données privées (contenu public) — requalifié P2.
- ✅ Pagination : bornée par les validators (pas « non clampée »).

## Note transverse

Les P1 « durcissement HTTP » + « clés » (Partie 4b) forment le **bloc pré-prod sécurité**. Le mapping `veille.*` et la couverture d'intégration alimentent l'audit *Tests* (Partie 6).
