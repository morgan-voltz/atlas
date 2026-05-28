# Changelog

Toutes les modifications notables de ce projet sont consignées ici.

Le format s'appuie sur [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/),
et le projet suit le [versionnage sémantique](https://semver.org/lang/fr/).

## [Non publié]

Première itération du produit (MVP 1). Le backend est complet ; les clients MAUI et
la documentation sont amorcés. Plusieurs intégrations INPI restent à confirmer par un
appel authentifié réel (cf. roadmap, statuts 🟡).

### Ajouté

- **F-001 — Inscription / connexion** : inscription email + mot de passe, vérification
  d'email, connexion, rafraîchissement et déconnexion (auth hexagonale custom, cf. ADR-010).
- **F-002 — Double authentification (2FA)** : TOTP (RFC 6238), 10 codes de secours,
  défi 2FA à la connexion.
- **F-003 — Connexion d'un compte INPI** : test de connexion `/sso/login`, stockage
  chiffré des identifiants, statut de connexion.
- **F-004 — Recherche entreprise par SIREN** : value object `Siren` (Luhn), client RNE
  authentifié avec cache de token, fiche `UniteLegale`.
- **F-005 — Recherche entreprise par dénomination** : recherche paginée.
- **F-006 — Recherche marque** : adapter INPI PI (auth XSRF + cookies), recherche paginée.
- **F-007 — Fiche détaillée d'une marque** : notice (classes de Nice) + image.
- **F-008 — Historique des recherches** : enregistrement découplé (notification MediatR),
  rétention de 200 entrées par utilisateur.
- **F-009 — Client MAUI mobile** : Login → recherche entreprise → fiche → historique
  (compilé Android).
- **F-010 — Client MAUI desktop** : même application sur cibles desktop (compilé Windows).
- **F-011 — Documentation utilisateur** : site MkDocs Material (`website/`) + déploiement
  GitHub Pages.
- **F-012 — Conformité RGPD** : export des données (JSON) et suppression de compte avec
  effacement en cascade.
- **F-041 — Moteur d'agrégation RSS/Atom (MVP 2)** : contexte Veille (port
  `IExternalContentSource`, entités `FeedSource`/`FeedItem`), provider RSS/Atom
  (CodeHollow.FeedReader), polling récurrent via **Hangfire** (stockage PostgreSQL),
  déduplication par hash (URL + titre), endpoint `GET /feed/items`. Sources système amorcées
  au démarrage (.NET Blog, CNIL, data.gouv.fr).
- **F-042 — Catalogue de `VeillePack` par métier** : entité `VeillePack` versionnée
  (re-sync au upgrade), port `IVeillePackRepository`, enrôlement / désenrôlement
  utilisateur, sources système rattachées au pack. Les sources d'un pack **ignorent
  la limite par utilisateur** des sources libres.
- **F-043 — Sources de veille ajoutées par l'utilisateur** : entité
  `VeilleSubscription`, port `IFeedSubscriptionPolicy` (limite par utilisateur +
  blocklist d'hôtes), endpoints `POST /veille/subscriptions`,
  `DELETE /veille/subscriptions/{id}`, `GET /veille/subscriptions`.
- **F-044 — Timeline unifiée (backend)** : timeline par utilisateur agrégeant ses
  abonnements libres et ses packs, états par item (`FeedItemUserState` :
  lu / non lu / favori / archivé), filtrage et pagination, endpoints `/timeline/*`.
- **F-045 — Déduplication intelligente des items** : empreinte **SimHash 64 bits**
  (FNV-1a) calculée à l'ingestion, regroupement en `FeedItemCluster` par distance
  de Hamming, collapse dans la timeline (un seul représentant par cluster).
- **F-017 — Favoris entreprise** : entité `CompanyFavorite` (UserId + SIREN +
  NameSnapshot optionnel + AddedAt) avec index unique `(UserId, SIREN)` et cascade
  FK RGPD. Endpoints `POST /favorites/companies`, `DELETE /favorites/companies/{siren}`,
  `GET /favorites/companies`. Pose la base de F-019 (alertes sur favoris) puis
  F-047 (timeline mixte veille+favoris). Méthode `Siren.FromTrustedValue` ajoutée
  pour la réhydratation EF (constructeur de confiance limité à la persistance).
- **F-019 — Alerte sur modification d'une entreprise favorite (backend)** :
  - Entité `CompanyFavoriteSnapshot` (un seul cliché vivant par `(UserId, SIREN)`),
    capture dénomination / forme juridique / NAF / adresse / hash des dirigeants.
  - `CompanyFavoriteSnapshot.DiffWith(UniteLegale)` retourne la liste des
    `CompanyFavoriteChange` (champ, ancien, nouveau).
  - Use case `RefreshFavoritesCommand` (use case Application) — itère les users qui
    ont des favoris et un compte INPI connecté, re-fetch RNE, calcule le diff,
    upsert le snapshot, publie `CompanyFavoriteChangedNotification`.
  - 2 handlers de notification : `SendFavoriteChangeEmailHandler` (email via
    `IEmailSender.SendFavoriteChangeAsync`, nouvelle méthode ajoutée à
    `IEmailSender`) et `DispatchFavoriteChangePushHandler` (push via
    `INotificationDispatcher`).
  - Job Hangfire `favorite-refresh` planifié quotidiennement à **03:00 UTC**,
    désactivable via `BackgroundJobs:Enabled=false`.
- **F-047 — Combinaison veille + favoris (MVP, tagging RSS)** : LA feature
  différenciante du produit. Quand un item RSS est ingéré, le moteur scanne le
  titre et le résumé pour y détecter les noms d'entreprises favorites des
  utilisateurs (matching mot entier case-insensitive, ≥ 3 caractères). Chaque
  mention est persistée dans `feed_item_favorite_matches` (index unique
  `(user_id, item_id, siren)`). La timeline (F-044) expose un nouveau champ
  `MentionedFavorites` par item et un filtre `?mentionsFavoritesOnly=true`
  pour ne voir que les items qui parlent de **mes** entreprises favorites.
  Le matching est exécuté à chaque cycle de polling, juste après la
  déduplication (F-045). Cascades FK RGPD sur user + feed_item. Les volets
  événements RNE en timeline et BODACC (F-048) restent à venir.
- **F-020 — Notifications push (ports + endpoints, adapters à venir par plateforme)** :
  - Entité `DeviceRegistration` (UserId, `DevicePlatform` enum
    `FcmAndroid` / `ApnsIos` / `WindowsWns` / `MacOsApns`, token unique global, label),
    upsert sur token, cascade FK RGPD.
  - Port `INotificationDispatcher` (`DispatchAsync(UserId, NotificationPayload, CT)`).
  - Implémentation par défaut `LoggingNotificationDispatcher` (log les notifications
    au lieu de les pousser) — **les adapters FCM / APNs / WNS arrivent en PRs
    séparées par plateforme**, sans modification du domaine.
  - Endpoints `POST /devices`, `DELETE /devices/{id}`, `GET /devices` (le token
    n'est jamais renvoyé).
- **Pipeline CI GitHub Actions** (Lot 0 audit) : `.github/workflows/ci.yml` exécute
  build Release + tests unitaires + tests d'architecture + tests d'intégration
  (Docker / Testcontainers) sur chaque PR et chaque push sur `main`. Analyseurs
  IDE en mode `EnforceCodeStyleInBuild` (stricteté égale à CI dès le build).
- **Endpoints API** : `/auth/*`, `/inpi/connection`, `/companies`, `/trademarks`,
  `/search-history`, `/account`, `/feed/items`, `/veille/subscriptions/*`,
  `/veille/packs/*`, `/timeline/*`.
- **Tests** : suite unitaire + tests d'architecture (NetArchTest) + tests d'intégration
  (PostgreSQL via Testcontainers, INPI via WireMock, API end-to-end via WebApplicationFactory).
- **Documentation** : `ARCHITECTURE.md`, `CHANGELOG.md`, `CONTRIBUTING.md`,
  audit profond lecture seule de la solution dans `docs/audit/` (Shared → Domain →
  Application → Infrastructure.{Persistence,Veille,Inpi,Security,Messaging} → Api →
  Tests → Maui, plus synthèse priorisée).

### Modifié

- Alignement du mapping RNE sur la documentation technique INPI v4.0 (navigation JSON
  défensive dans `RneCompanyMapper`, mapping des dirigeants).
- `Directory.Packages.props` : correction des versions « fantômes » du squelette initial
  (Konscious, NSubstitute, NetArchTest, Testcontainers, WireMock.Net, Bogus,
  CommunityToolkit.Mvvm) et montée d'OpenTelemetry.
- Client MAUI : passage des `[ObservableProperty]` en propriétés partielles
  (compatibilité WinRT/desktop).
- **Effacement RGPD étendu à la veille** (Lot 1 audit) : cascade FK sur
  `VeilleSubscription`, `VeillePackEnrollment` et `FeedItemUserState` lors de la
  suppression de compte (art. 17 RGPD) — toutes les traces de veille d'un
  utilisateur sont purgées avec son compte.
- **Mapping HTTP `veille.*`** (Lot 1 audit) : codes d'erreur veille
  (`subscription_not_found`, `source_blocked`, `fetch_failed`,
  `subscription_limit_reached`, `already_subscribed`, …) explicitement traduits
  en 404 / 409 / 403 / 502 par `ErrorHttpMapping` au lieu de retomber en 400
  par défaut.

### Sécurité

- Mots de passe hachés avec **Argon2id**.
- Identifiants INPI chiffrés au repos en **AES-256-GCM** ; jamais loggés ni exposés.
- JWT d'accès signés **RS256**, refresh tokens rotatifs et révocables.
- Droits RGPD (export / effacement) implémentés.
- Pin de sécurité `System.Security.Cryptography.Xml` 10.0.8 (CVE GHSA-37gx-xxp4-5rgx).
- **Lot 2a — Durcissement crypto et anti-SSRF** :
  - **Garde-fous au démarrage sur les clés** : `Jwt:PrivateKeyPem` et `Crypto:KeyBase64`
    deviennent obligatoires hors `Development` (validation `ValidateOnStart`). Plus de
    fallback silencieux sur clé RSA éphémère (qui invalidait tous les JWT à chaque
    redémarrage) ou clé AES dérivée d'un secret de dev en production.
  - **Anti-SSRF dans `FeedSubscriptionPolicy`** : rejet des hôtes IP en plages privées
    (10/8, 172.16/12, 192.168/16), loopback (127/8, ::1, `localhost`), link-local
    (169.254/16) et IPv6 site/link-local. Couvre notamment `169.254.169.254`
    (endpoints de métadonnées AWS / GCP). Empêche un utilisateur d'ajouter une URL de
    flux qui ferait pivoter le serveur vers son réseau interne.
  - **Couverture de tests cryptographiques** : nouveau projet
    `Atlas.Infrastructure.Security.UnitTests` (37 tests) couvrant `Argon2idPasswordHasher`,
    `AesGcmCryptoService` (détection de tampering ciphertext + tag), `JwtIssuer`,
    `TwoFactorChallengeService` (isolation d'audience `atlas` vs `atlas-2fa`),
    `TotpProvider`, `SecureTokenGenerator`. 21 tests supplémentaires sur
    `FeedSubscriptionPolicy`.
- **Lot 2b — Rate limiting, en-têtes de sécurité, CORS, logging structuré** :
  - **Rate limiter ASP.NET Core** : politique globale (100 req / 60 s) + politique
    `auth-strict` (10 req / 60 s) appliquée à `POST /auth/register`, `/auth/login`,
    `/auth/refresh` et `/auth/2fa/verify`. Partition par `sub` (utilisateur
    authentifié) ou par IP. Renvoie **429 Too Many Requests** au-delà.
  - **En-têtes de sécurité** via middleware custom (`SecurityHeadersMiddleware`) :
    `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`,
    `X-Frame-Options: DENY`, `Content-Security-Policy: default-src 'none'; frame-ancestors
    'none'; base-uri 'none'; form-action 'none'` (API JSON pure), `Permissions-Policy`
    refusant l'accès aux APIs sensibles du navigateur. **HSTS** activé hors Development.
  - **CORS strict** : section `Cors:AllowedOrigins` lue depuis la configuration, **jamais
    `*`**. Liste vide tolérée en Development uniquement ; hors Development le démarrage
    échoue (`ValidateOnStart`).
  - **Logging Serilog structuré** + `UseSerilogRequestLogging` (durée / route / statut
    par requête). `SensitiveDataMaskingEnricher` masque proactivement toute propriété
    structurée dont le nom contient `password`, `token`, `secret`, `credential`,
    `privatekey`, `keybase64`, `authorization` — défense en profondeur en plus de la
    règle « ne jamais logger d'`InpiCredentials` ».
  - **Tests d'intégration** : `SecurityHeadersTests` (vérifie les 5 en-têtes sur les
    réponses), `RateLimitingTests` (4ᵉ requête `/auth/login` → 429 quand
    `PermitLimit=3`).
