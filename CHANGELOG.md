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
- **F-013 — Téléchargement individuel d'actes et bilans (MVP 2)** : extension
  `ICompanyDataProvider` avec `GetAttachmentsAsync` + `DownloadAttachmentAsync`
  (auth Bearer + retry 401 unifié dans `ExecuteAsync<T>` du `RneCompanyProvider`).
  Endpoints `GET /companies/{siren}/attachments` et
  `GET /companies/{siren}/attachments/{id}/download` (proxy binaire via
  `Results.Stream`). Entité `CompanyAttachment` (Id, `AttachmentType` enum
  `Acte` / `Bilan` / `Other`, Name, DepositedAt?, SizeBytes?, IsConfidential).
  Mapping JSON **défensif** : accepte tableau direct OU objet avec sous-collections
  `actes` / `comptesAnnuels` / `bilans`. Bilans confidentiels (champ `confidentialite`)
  → 403 `companies.attachment_confidential`. Reste : confirmation par un appel
  authentifié réel + couverture Bruno.
- **F-014 — Téléchargement en masse de documents (MVP 2)** : un utilisateur soumet
  jusqu'à 50 SIREN via `POST /downloads/bulk` ; le backend valide chaque SIREN (Luhn),
  crée un `BulkDownloadJob` (`Pending`) et enfile un job Hangfire qui télécharge les
  actes et bilans publics de chaque entreprise puis empaquette l'ensemble dans une
  archive ZIP (`bulk/{jobId:N}.zip`). Suivi par polling sur `GET /downloads/bulk/{id}`
  (`Pending` → `Running` → `Ready` / `Failed`). Récupération via
  `GET /downloads/bulk/{id}/archive` (streaming). Nouveau port `IFileStorage` dans
  `Atlas.Domain.Storage` (`Save` / `OpenRead` / `Delete`) avec adapter
  `LocalFileStorage` filesystem (nouveau projet `Atlas.Infrastructure.Storage`).
  TTL d'archive : 24 h (renvoie `410 Gone` après expiration). Documents confidentiels
  filtrés à la source. Cascade FK RGPD sur `bulk_download_jobs`. Le push / email
  de finalisation, le job de purge automatique et l'adapter S3 sont reportés à un
  lot ultérieur.
- **F-015 — Recherche brevet par numéro de publication (MVP 2)** : endpoint
  `GET /patents/{publicationNumber}` qui interroge l'INPI PI brevets via
  `IIntellectualPropertyProvider.GetPatentAsync`. Route paramétrée placée **après**
  la route de recherche (F-016) pour éviter la collision de routing. Mapping
  best-effort `PiPatentMapper.MapDetail`. Reste : confirmation contre la structure
  JSON exacte de l'INPI réel + couverture Bruno.
- **F-016 — Recherche brevet avancée (titre / inventeur / déposant, MVP 2)** :
  `PatentSearchQuery(Title?, Inventor?, Applicant?, Page, PageSize)` +
  `PatentSummary` + `IIntellectualPropertyProvider.SearchPatentsAsync`. Endpoint
  `GET /patents?title=&inventor=&applicant=&page=&pageSize=` (route précédant la
  route paramétrée de F-015). Validation : ≥ 1 critère renseigné sinon
  `400 patents.empty_search`. Pagination clampée. Adapter via
  `POST /services/apidiffusion/api/brevets/search`. Mapping best-effort
  `PiPatentMapper.MapSummary`. Reste : confirmation contre la syntaxe SolR exacte
  + structure de réponse paginée de l'INPI réel.
- **F-018 — Favoris : suivi d'une marque ou d'un brevet (MVP 2)** : entités
  `TrademarkFavorite` (UserId, DepositNumber, NameSnapshot?, AddedAt) et
  `PatentFavorite` (UserId, PublicationNumber, TitleSnapshot?, AddedAt) avec ports
  repositories + erreurs métier. 6 use cases Application (Add / Remove / GetMine ×
  2 entités). 6 endpoints `POST/DELETE/GET /favorites/trademarks` et
  `/favorites/patents` sous le même groupe `/favorites/*`. Persistence : tables
  `trademark_favorites` et `patent_favorites` (index unique
  `(user, dépôt / publication)`, cascade FK RGPD). Tests handlers (10) + cascade
  RGPD étendue.
- **F-021 — Export CSV des listes de favoris (MVP 2)** : helper
  `Atlas.Application.Common.CsvWriter` (RFC 4180, UTF-8 + BOM, CRLF, échappement
  des quotes / virgules / sauts de ligne). 3 endpoints d'export favoris :
  `GET /favorites/{companies,trademarks,patents}/export` → `text/csv` avec
  `Content-Disposition: attachment`. Pas de dépendance NuGet ajoutée. Reste :
  export XLSX (ClosedXML), export des résultats de recherche RNE / PI (paginé),
  export de la veille (timeline).
- **F-022 — Rapport PDF de fiche entreprise (MVP 2)** : endpoint
  `GET /companies/{siren}/report.pdf` → PDF A4 (identité, NAF, adresse, dirigeants,
  table actes/bilans). Powered by **QuestPDF community edition** (licence engagée
  au démarrage). `CompanyReportRenderer` dans `Atlas.Api/Reports/` (couche
  présentation, QuestPDF référencé uniquement par `Atlas.Api`). Attachments
  best-effort (PDF généré même si la liste échoue). 3 smoke tests dans
  `Atlas.Api.IntegrationTests` (signature `%PDF`, payloads minimal / complet /
  vide). Reste : enrichissement (logo, historique des modifications via snapshots
  F-019, bilans intégrés via F-013 download).
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
  déduplication (F-045). Cascades FK RGPD sur user + feed_item.
- **F-048 — BODACC dans la timeline (F-047 volet 3)** : nouvelle source
  d'événements légaux pour les SIREN favoris. Adapter `OpendatasoftBodaccProvider`
  (nouveau projet `Atlas.Infrastructure.Bodacc`) interroge l'API publique
  data.gouv.fr/Opendatasoft (anonyme, pas d'INPI requis). Job Hangfire
  `bodacc-polling` cron `0 4 * * *` (1 h après le refresh RNE de F-019),
  déduplication cross-users (un seul appel API par SIREN partagé entre N users).
  Chaque annonce non encore connue devient un `FavoriteEvent` type
  `BodaccPublished` avec `ExternalId = AnnouncementId BODACC`. Nouveau
  champ `FavoriteEvent.ExternalId` + index unique partiel
  `(user_id, external_id) WHERE external_id IS NOT NULL` pour la dédup
  efficace. **Clôture F-047 dans son MVP** : la timeline mixe désormais
  items RSS (F-041/045), événements RNE (F-019/F-047 v2) et annonces
  BODACC (F-048) — tout au même endroit.
- **F-047 volet 2 — Événements RNE dans la timeline** : les
  `CompanyFavoriteChangedNotification` publiées par F-019 deviennent désormais
  des entrées de timeline aux côtés des items RSS, triées chronologiquement.
  Nouvelle entité `FavoriteEvent` (UserId, Siren, type `RneChanged` / réservé
  `BodaccPublished`, Title, Summary, OccurredAt), cascade FK RGPD. 3ᵉ handler
  MediatR `RecordFavoriteEventOnChangeHandler` sur la notification de F-019.
  **Breaking change léger** côté `GET /feed/timeline` : le DTO devient un
  union discriminée — `Kind = "RssItem" | "FavoriteEvent"`, `OccurredAt`
  remplace `PublishedAt`, champs spécifiques `EventType` / `EventSiren`
  pour les events, `SourceId` / `Categories` désormais optionnels. Fusion
  bornée à 500 entrées de chaque source en mémoire pour MVP ; les events
  sont exclus si un filtre RSS-only est actif (`sourceId`, `unread`,
  `favoritesOnly`, `mentionsFavoritesOnly`). Reste : BODACC (F-048).
- **F-046 — Filtres et règles de surveillance personnalisées (MVP 2)** : entité
  `FeedRule` (`UserId` + critères évalués en **AND** : `KeywordPattern` /
  `SourceId` / `MentionedSiren` + actions : `NotifyEmail` / `NotifyPush`).
  Invariants `Create` / `Update` : nom 1-200, keyword ≤ 200, au moins un critère
  et au moins une action. Watermark `LastEvaluatedAt` pour l'évaluation
  incrémentale ; compteur monotone `TimesTriggered` + `LastTriggeredAt`. Port
  `IFeedRuleRepository`. 5 use cases MediatR (`CreateFeedRule`, `UpdateFeedRule`,
  `DeleteFeedRule`, `ListMyFeedRules`, `EvaluateFeedRules`). Notification
  `FeedRuleMatchedNotification` + 2 handlers email / push calqués sur F-019,
  gating sur `NotifyEmail` / `NotifyPush` par règle. Extension
  `IEmailSender.SendFeedRuleMatchedAsync` + adapters logging / capturing
  (l'adapter Brevo restera commun avec F-019). 4 endpoints `/feed/rules` (POST /
  GET / PATCH / DELETE) sécurisés via auth utilisateur. Table `feed_rule`
  (cascade FK user RGPD, 2 index : `user_id`, `is_active`). Job Hangfire :
  `EvaluateFeedRulesCommand` chaîné dans `FeedPollingJob.PollAsync()` après
  `MatchFavoritesInFeedItemsCommand` (F-047) — voit donc les
  `FeedItemFavoriteMatch` nécessaires au critère `MentionedSiren`. 28 tests
  couvrent l'entité (17), `CreateFeedRule` (4), `DeleteFeedRule` (3) et le flux
  complet `EvaluateFeedRules` (4). Reste : adapter Brevo email, UI MAUI (CRUD
  des règles).
- **F-050 — Préparation à la couche premium (architecture)** : matérialise la
  frontière open core / premium prévue par ADR-006. 3 ports premium déclarés
  côté domaine dans `Atlas.Domain.Veille.Premium` (interfaces uniquement,
  aucune implémentation) : `IFeedItemEnricher` (+ record `FeedItemEnrichment`),
  `IFeedRelevanceScorer` (score 0-100 par item × user) et `IFeedSummarizer`
  (synthèse narrative d'un batch — patron pour `IFinancialSummarizer` de F-054).
  Projet `Atlas.Application.Premium` matérialisé via une classe `AssemblyMarker`
  publique pour permettre aux tests d'architecture de cibler son assembly.
  +3 tests dans `Atlas.Architecture.Tests` verrouillant la séparation : (a)
  `Atlas.Application` (cœur) ne référence pas `Atlas.Application.Premium`, (b)
  `Atlas.Domain` ne référence pas `Atlas.Application.Premium`, (c)
  `Atlas.Application.Premium` ne référence pas `Atlas.Infrastructure.*` ni
  `Atlas.Api`. Le cœur open source reste intact ; les adapters viendront dans
  des projets `Atlas.Infrastructure.*Premium` dédiés, câblés en composition
  root dans `Atlas.Api`.
- **F-049 — Marketplace des templates de veille partagés (MVP 2)** : entité
  `VeillePack` étendue avec auteur (`AuthorUserId : UserId?` nullable pour les
  packs système F-042) + visibilité (`Visibility` enum `System` / `Private` /
  `Public`) + compteur dénormalisé `LikesCount`. Nouvelle factory
  `VeillePack.CreateUserPack` qui crée en brouillon (`Private`). Méthodes
  `Publish` / `Unpublish` (refusées sur les packs système),
  `IncrementLikes` / `DecrementLikes` (plancher à 0). 2 nouvelles entités :
  `VeillePackLike` (index unique `(UserId, VeillePackId)`, cascades FK RGPD
  user + pack) et `VeillePackReport` (workflow *report &amp; review* :
  `Pending` → `ReviewedNoAction` / `ReviewedRemoved`, raison ≤ 500 chars).
  Ports `IVeillePackLikeRepository` + `IVeillePackReportRepository`. Extensions
  `IVeillePackRepository.GetPublicMarketplaceAsync` (paginé, tri
  `LikesCount desc, CreatedAt desc`) et `GetByAuthorAsync`. 8 use cases
  MediatR : `CreateUserVeillePack`, `PublishVeillePack`, `UnpublishVeillePack`,
  `LikeVeillePack`, `UnlikeVeillePack`, `ReportVeillePack`,
  `ListPublicMarketplace`, `GetMyAuthoredPacks`. 8 endpoints sous
  `/veille/packs/*` : `POST /user`, `PATCH /user/{code}/publish`,
  `PATCH /user/{code}/unpublish`, `POST/DELETE /{code}/like`,
  `POST /{code}/report`, `GET /community`, `GET /mine/authored`. Migration
  `AddVeillePackMarketplace` (3 colonnes sur `veille_pack` + normalisation
  `System` des packs existants + 2 nouvelles tables, cascades FK RGPD).
  21 tests (11 entité — invariants Create system/user, transitions Publish /
  Unpublish, idempotence, plancher likes, workflow Report ; 10 handlers —
  Create avec filtrage des subs non-possédées, Like idempotent, Publish
  forbidden si caller ≠ auteur). Reste : UI MAUI (CRUD + browse + like /
  report), workflow admin de revue des reports, recommandations / search
  facetté.
- **F-020 — Adapter Firebase Cloud Messaging (Android + Web Push)** : OAuth2
  par service account (JWT RS256 signé avec la clé privée du service account
  Firebase, échange contre un access token, cache 55 min thread-safe). POST
  FCM HTTP v1 (`https://fcm.googleapis.com/v1/projects/{ProjectId}/messages:send`)
  avec Bearer token. **Cleanup auto** des tokens morts : 404 (`NOT_FOUND`)
  ou 400 (`UNREGISTERED` / `INVALID_ARGUMENT`) → suppression silencieuse du
  `DeviceRegistration` + `SaveChanges`.
- **F-020 — Adapter Windows Notification Service (Windows desktop)** :
  - **OAuth2 `client_credentials`** sur `https://login.live.com/accesstoken.srf`
    (pas de JWT signé, contrairement à FCM et APNs). Body form-encoded :
    `grant_type=client_credentials`, `client_id={PackageSid}`,
    `client_secret={ClientSecret}`, `scope=notify.windows.com`. Cache jusqu'à
    5 min avant expiration (TTL Microsoft = ~24 h). Refresh thread-safe.
  - **POST sur l'`ChannelUri`** stocké dans `DeviceRegistration.Token` —
    chaque device a son propre endpoint (`db5p.notify.windows.com/?token=…`),
    pas de `BaseAddress` partagée.
  - **Payload XML ToastGeneric** : `<toast><visual><binding template="ToastGeneric">
    <text>Title</text><text>Body</text></binding></visual></toast>`. Données
    utilisateur sérialisées en JSON dans l'attribut `launch` (récupérable
    côté app via `ToastNotificationActivatedEventArgs.Argument`).
  - Headers : `Authorization: Bearer …`, `X-WNS-Type: wns/toast`,
    `X-WNS-RequestForStatus: true`, `Content-Type: text/xml; charset=utf-8`.
  - **Cleanup auto** : 410 Gone ou 404 NotFound → suppression silencieuse du
    device + `SaveChanges`. Bonus : si `ChannelUri` n'est pas une URL absolue
    parseable, suppression immédiate (donnée corrompue).
- **F-020 — Plateformes push toutes livrées** : avec WNS, le push couvre
  maintenant **toutes les plateformes cibles** du backend — Android (FCM),
  Web (FCM Web Push), iOS (APNs), macOS (APNs) et Windows desktop (WNS).
  `CompositeNotificationDispatcher` fan-out vers les 3 adapters configurés
  selon `Fcm:*`, `Apns:*` et `Wns:*`. Reste : client MAUI (récupération du
  token natif par plateforme + `POST /devices` au démarrage).
- **F-020 — Adapter Apple Push Notification service (iOS + macOS)** : JWT
  **ES256** (ECDSA P-256) signé avec la clé privée `.p8` Apple Developer
  (header `kid` = Key ID, claim `iss` = Team ID). Cache 30 min, refresh
  thread-safe. POST HTTP/2 sur `api.push.apple.com` (ou sandbox selon
  `Apns:UseSandbox`), `/3/device/{token}`, headers `authorization: bearer …`,
  `apns-topic` (= BundleId), `apns-push-type=alert`, `apns-priority=10`.
  Payload `aps` (`{alert.title, alert.body, sound}`) + data utilisateur à plat
  à la racine. **Cleanup auto** : 410 Gone (Unregistered) ou 400 avec
  `reason: BadDeviceToken`/`DeviceTokenNotForTopic`.
- **Refactor push multi-plateformes (F-020)** : nouvelle interface interne
  `IPlatformPushDispatcher` (FCM, APNs, futurs WNS…), nouveau
  `CompositeNotificationDispatcher` qui implémente `INotificationDispatcher`
  et **fan-out vers tous les dispatchers enregistrés**. Une exception dans un
  dispatcher n'invalide pas les autres (isolation try/catch + log).
  DI bascule automatiquement : ≥ 1 plateforme configurée → composite ;
  aucune → fallback `LoggingNotificationDispatcher` (comportement dev).
  Aucun changement Program.cs requis pour activer FCM ou APNs.
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
