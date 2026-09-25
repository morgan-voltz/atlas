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
- **Adapter email Brevo** (provider France RGPD-compliant, cf. CLAUDE.md) :
  `BrevoEmailSender` (`Atlas.Infrastructure.Messaging.Email.Brevo`) câblé via
  `AddHttpClient<IEmailSender, BrevoEmailSender>` quand `Email:Brevo:ApiKey`
  est renseigné — sans clé, fallback transparent sur le
  `LoggingEmailSender` de dev. POST `https://api.brevo.com/v3/smtp/email`
  avec header `api-key` (et non Bearer). 3 templates inline (subject +
  HTML + plain text) pour les 3 méthodes existantes de l'`IEmailSender` :
  vérification d'email (F-001), évolution favori (F-019), règle de
  surveillance matchée (F-046). Échec non-2xx ou exception réseau : logué
  en `Warning` / `Error` mais **jamais propagé** — un email perdu ne doit
  pas casser un job métier (refresh favoris, évaluation règles). Garde-fou
  amont : si `SenderEmail` n'est pas configuré, l'envoi est sauté avec
  log d'erreur (la clé sans expéditeur est une mauvaise configuration).
  6 tests unitaires (`FakeHttpMessageHandler`) couvrent le payload Brevo
  des 3 templates, l'échec HTTP non bloquant, l'exception réseau
  swallowée et le skip sans `SenderEmail`. Débloque les annonces
  « alertes email opérationnelles » pour F-019 et F-046.
- **Kit de thèmes accessibles MAUI (WCAG 2.2 AA)** : 7 thèmes
  sélectionnables × 2 modes (clair / sombre) = 14 palettes intégrées
  dans `src/Atlas.Maui/`. Thèmes : Atlas, Ocean, Forêt, Ambre,
  Améthyste, Contraste (élevé), Sépia (faible lumière bleue). Chaque
  palette est un `ResourceDictionary` XAML avec tokens sémantiques
  (`primary`, `surface`, `onBackground`, `error`, `success`, `warning`,
  `info`, etc.) à référencer via `{DynamicResource <token>}`. Service
  `ThemeManager` (`src/Atlas.Maui/Theming/`) qui persiste la préférence
  thème + mode via `Preferences` et applique le ResourceDictionary
  actif sur `Application.Current.Resources` ; trois modes pris en
  charge (Light / Dark / System). Enum `AppThemeId` + extensions de
  libellé affichable. Wiring : `App.xaml.cs` appelle
  `new ThemeManager().Initialize()` au démarrage. Source de génération
  (chaîne Python `palettes.py → generate.py`) conservée dans
  `docs/atlas-themes-kit/atlas-themes/source/` comme outillage canonique
  de régénération — le XAML n'est jamais modifié à la main. Aucun
  thème n'est ajouté tant qu'il ne passe pas le vérificateur de
  contraste WCAG. Câblage des vues existantes (LoginPage,
  CompanySearchPage, etc.) sur les tokens reporté au chantier UI MAUI
  cross-cutting (cf. doctrine UX `docs/12-modele-ux-client-maui.md`).
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
- **Documentation — doctrine architecturale étendue (29 mai 2026)** : 4 nouveaux
  ADRs (013 → 016) intégrés dans `docs/01-decisions-architecturales.md` :
  - **ADR-013** Substrat de surveillance des entités (deux stratégies
    `IStateMonitor<TState>` / `IItemStreamMonitor<TItem>` + runner mutualisé ;
    extraction à la troisième instance F-057).
  - **ADR-014** Matching conservateur unifié (le contrat `MatchCandidate`
    encode la doctrine ADR-012 dans le type — verdict irreprésentable —, noyau
    de normalisation des noms partagé par les 3 matchers à base de noms).
  - **ADR-015** Couche d'assemblage du dossier entreprise (read-model
    `CompanyDossier` à sections auto-descriptives, 5 états `SectionState`,
    résolution snapshot-first).
  - **ADR-016** Sécurité & doctrine de la surface agentique MCP (4 principes :
    surface curée lecture-d'abord, doctrine inline avec donnée, contenu externe
    = donnée jamais instruction, délégation utilisateur).
- **Documentation — 5 fiches V3+ (29 mai 2026)** : insérées dans
  `docs/02-roadmap-features.md` à partir de la 5ᵉ graine du persona avocat /
  juriste et des séances de cartographie. **F-031** réactivée et recadrée
  (Judilibre / jurisprudence rattachée à l'entité, personnes morales seulement,
  matching conservateur sous ADR-012/014). **F-056** Vue 360 / Dossier
  entreprise (méta-feature d'assemblage sous ADR-015). **F-057** Re-screening
  continu des sanctions (le « F-019 des sanctions », `IStateMonitor`
  d'ADR-013). **F-058** Signaux concurrentiels & digest sectoriel (typage des
  `FavoriteEvent`, digest hebdo via `IFeedSummarizer`). **F-059** Veille
  d'échéances PI / docketing assistif (V3+ conditionnel — cadrage de
  responsabilité explicite en prérequis). Cross-refs ajoutées sur F-019, F-026,
  F-027, F-032, F-046, F-047, F-048, F-052, F-055.
- **Documentation — vocabulaire et sécurité étendus** : 4 nouvelles sous-sections
  §12.4 → §12.7 dans `docs/08-vocabulaire-ubiquitaire.md` (~25 termes :
  `MonitoredDimension`, `MatchCandidate` / `Confidence` / `Basis`,
  `CompanyDossier` / `DossierSection` / `SectionState`, `IDossierSectionResolver`,
  scopes MCP, `McpToolDescriptor`, etc.). Nouvelle section §12bis
  « Surface agentique MCP — confinement et audit » dans
  `docs/04-securite-rgpd.md` (taxonomie des scopes, confinement du contenu
  externe, audit logging par outil, délégation utilisateur).
- **Documentation — modèle UX du client MAUI (`docs/12`)** : doctrine UX et
  navigation du client `Atlas.Maui` : 6 règles du modèle adaptatif (un seul
  modèle mental deux densités, adaptation à la largeur et non à la plateforme,
  list-detail récursif, mobile = focus & pouce / desktop = densité & clavier,
  divulgation progressive jamais d'amputation, pertinence avant exhaustivité),
  page vs carte, architecture de navigation à **5 destinations plafonnées**
  (Accueil / Recherche / Veille / Favoris / Profil) — c'est la ligne qu'on ne
  franchit plus. **Kit de composants** (atomic design, §6 → §9 du doc) :
  atomes (champ étiqueté, badge descriptif, ligne de provenance, chiffre
  clé), cartes (carte-section repliable de la fiche, carte-aperçu compacte
  réutilisée sur 5 surfaces — résultats / favoris / watchlist / flux / veille),
  états (chargement squelette, vide d'onboarding vs vide de couverture,
  erreur locale, « à jour / tout vu » qui clôt le flux contre le scroll
  infini). Specs détaillées (anatomie en slots, états, comportement,
  responsive, contrat d'accessibilité, garde-fous doctrine, hors-périmètre)
  pour la carte-aperçu (§7) et la carte-section (§8). Référencée depuis la
  table « Documents fondateurs » de `CLAUDE.md` et depuis les fiches F-009
  et F-010.
- **Lot 13 — Refactor du contrat de recherche PI conforme à la spec INPI v2** :
  l'API INPI a remplacé l'ancien contrat (`{query, page, pageSize}`) par
  `TrademarkQuery` / `PatentQuery` (cf. `docs/INPI/APIDiffusionV2.json`
  v1.1.0, local et gitignored). Atlas envoyait l'ancien format depuis
  toujours, ce qui retournait 405 Method Not Allowed côté INPI.
  - **Body marques `TrademarkQueryRequest`** (record interne) :
    - `collections: ["FMARK", "CTMARK", "TMINT"]` par défaut (marques
      françaises + EUIPO + OMPI), valeurs confirmées par
      `GET /api/marques/metadata` live.
    - `query` au format SolR INPI : `[Mark=<terme>]` au lieu du terme brut.
    - Pagination par `position` (0-based offset = `(page-1) * pageSize`)
      + `size`, au lieu de `page` / `pageSize`.
  - **Body brevets `PatentQueryRequest`** :
    - `collections: ["FR", "EP", "WO", "CCP"]` par défaut.
    - `query` SolR multi-critères joints par `AND` :
      `[TIT=<title>] AND [DEPOSANT=<applicant>] AND [INV=<inventor>]`.
    - Fallback `[TIT=*]` si aucun critère renseigné (pour ne pas envoyer
      une requête vide qui serait 500).
  - **Builder SolR + escape** : `BuildTrademarkSolrQuery`,
    `BuildPatentSolrQuery`, et `EscapeSolrValue` qui échappe `\\`, `[`,
    `]`, `:` (caractères réservés SolR INPI). Empêche un terme avec
    crochets de casser le parseur.
  - **Header `Accept: application/json`** ajouté à toutes les requêtes
    data dans `Authenticated()`. La spec INPI v2 documente
    `produces: [xml, json]` avec **XML par défaut** — sans ce header,
    Atlas recevrait du XML qu'il ne sait pas désérialiser.
  - **Robustesse JSON** : `ReadFromJsonAsync` enveloppé dans un
    `try/catch JsonException` côté search marques et brevets — si
    l'INPI renvoie 200 avec un body vide ou non-JSON (cas observé en
    live), Atlas retourne `InpiErrors.Unavailable` (502 propre) au lieu
    d'une exception fuyante qui leakait une stack trace dans la réponse
    API. Fragilité préexistante corrigée en passant.
  - **Tests d'intégration WireMock (+3)** :
    `SearchTrademarks_body_matches_v2_TrademarkQuery_contract` —
    inspecte le body envoyé et vérifie `query="[Mark=danone]"`,
    `position=10` (page 2 × pageSize 10), `size=10`,
    `collections=["FMARK","CTMARK","TMINT"]`, header
    `Accept: application/json`.
    `SearchPatents_body_matches_v2_PatentQuery_contract` — vérifie
    `query="[TIT=electric battery] AND [DEPOSANT=RENAULT]"`,
    `collections=["FR","EP","WO","CCP"]`.
    `SearchPatents_with_no_criteria_falls_back_to_TIT_wildcard` —
    cas dégénéré, fallback `[TIT=*]`.
  - **Validation manuelle curl live post-Lot 13** : trademark search →
    502 `inpi.unavailable` (INPI continue de renvoyer 500 SolR sur le
    backend ; mappage Atlas propre, le format de body est conforme spec) ;
    patent search → 502 `inpi.unavailable` (au lieu d'une 500
    `JsonException` qui leakait la stack avant la robustesse JSON). Le
    500 SolR côté INPI est tracé pour investigation séparée (peut-être
    syntaxe SolR subtile à comprendre, peut-être bug temporaire backend
    INPI).
  - **Anciens DTOs supprimés / déplacés** : `PiSearchRequest` retiré du
    fichier provider (remplacé par `TrademarkQueryRequest`).
    `PiPatentSearchRequest` retiré de `DTOs/PiPatentSearch.cs`
    (remplacé par `PatentQueryRequest`). Mapper de réponse et response
    DTO (`PiPatentSearchResponse`, `PiTrademarkSearchResponse`) inchangés
    pour l'instant — la structure réelle de la réponse v2 sera validée
    quand le 500 INPI sera résolu.
  - 20 tests d'intégration Inpi verts (17 existants + 3 nouveaux Lot 13).
- **Lot 12 — Fix cookie XSRF-TOKEN double-submit sur requêtes data PI** :
  suite directe du diagnostic curl live post-Lot 11. L'auth PI réussit
  désormais (JWT bien obtenu, avec rôles `ROLE_API_MARQUES` /
  `ROLE_API_BREVETS` / `ROLE_API_MODELES`), mais toutes les requêtes
  data (`/marques/search`, `/marques/notice/{id}`, `/marques/image/...`,
  équivalents brevets) recevaient **403 « Could not verify the provided
  CSRF token because your session was not found. »**.
  - Le serveur INPI applique le pattern **double-submit cookie** de
    Spring Security : il compare le header `X-XSRF-TOKEN` à la valeur
    du **cookie** `XSRF-TOKEN`. Atlas envoyait seulement
    `Cookie: access_token=...` + `X-XSRF-TOKEN: <token>` mais **omettait
    le cookie XSRF-TOKEN**, donc le serveur ne trouvait pas de session
    associée au header.
  - **Fix dans `Authenticated()` helper** (méthode privée partagée par
    Search / Notice / Image / Patent Search / Patent Detail) :
    ```diff
    - request.Headers.TryAddWithoutValidation(
    -     "Cookie", $"access_token={session.AccessToken}");
    + request.Headers.TryAddWithoutValidation(
    +     "Cookie",
    +     $"access_token={session.AccessToken}; XSRF-TOKEN={session.XsrfToken}");
      request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", session.XsrfToken);
    ```
  - **Test d'intégration WireMock**
    (`SearchTrademarks_sends_both_access_token_and_xsrf_token_cookies`) :
    inspecte `_server.LogEntries` sur `/marques/search` et vérifie que la
    requête contient à la fois `Cookie: access_token` ET
    `Cookie: XSRF-TOKEN`, ET le header `X-XSRF-TOKEN`, ET que la valeur
    du cookie XSRF-TOKEN est **identique** à celle du header (la
    comparaison côté serveur l'exige).
  - **Validation manuelle curl contre INPI live** : avant Lot 12 → 403
    « session not found » ; après Lot 12 → le CSRF est validé,
    l'INPI traite la requête. Nouveau point de friction immédiat :
    `POST /services/apidiffusion/api/marques/search` retourne **405
    Method Not Allowed** alors que la spec officielle
    (`docs/INPI/APIDiffusionV2.json` v1.1.0) documente bien POST. Soit
    l'API live diverge de la spec téléchargée, soit le contrat de
    requête (`PiSearchRequest` vs `TrademarkQuery` officiel : query SolR
    `[Mark=...]`, `position`/`size`, `collections`) doit être mis à jour.
    Tracé pour un Lot 13 dédié au refactor du contrat de search PI v2.
  - 17 tests d'intégration Inpi verts (+1 vs Lot 11).
- **Lot 11 — Fix CSRF primer dans l'auth INPI PI (régression INPI réel)** :
  l'API INPI PI a durci son authentification — un primer CSRF est désormais
  exigé avant le `POST /auth/login`. L'adapter `InpiPiTrademarkProvider`
  (qui couvre F-006/F-007 marques + F-015/F-016 brevets) recevait 403
  systématique avec le message *« Could not verify the provided CSRF token
  because your session was not found. »*. Détecté à la main en validation
  curl post-Lot 10 contre data.inpi.fr le 2026-05-29 22:25 UTC.
  - **Nouveau flow d'auth PI** dans `GetSessionAsync` :
    1. **Primer** : `POST auth/login` **sans body** avec `X-CSRF-TOKEN: Fetch`.
       Le serveur répond 403 (attendu) en posant un cookie
       `XSRF-TOKEN=<guid>`.
    2. **Vrai login** : `POST auth/login` avec body JSON
       `{ username, password }` + header **`X-XSRF-TOKEN: <guid>`** (double X,
       convention Spring Security) + `Cookie: XSRF-TOKEN=<guid>` (pattern
       double-submit cookie).
    3. La réponse de login pose un **nouveau** XSRF-TOKEN (rotation),
       réutilisé pour les requêtes suivantes ; fallback sur le token primer
       si absent.
  - **Helper privé `FetchCsrfTokenAsync`** : encapsule le primer + extrait
    le cookie XSRF-TOKEN + retourne `Result<string>`. Erreurs réseau /
    cookie absent → `InpiErrors.Unavailable`.
  - **Tests d'intégration WireMock (`InpiPiTrademarkProviderIntegrationTests`,
    +2 tests)** :
    - `SearchTrademarks_performs_csrf_primer_before_login` — inspecte
      `_server.LogEntries` pour vérifier que **2 requêtes** `/auth/login`
      sont émises, que la 1ère porte `X-CSRF-TOKEN: Fetch` (sans body), et
      que la 2ᵉ porte `X-XSRF-TOKEN: <token>` + cookie `XSRF-TOKEN` + body
      avec le username.
    - `SearchTrademarks_fails_invalid_credentials_when_primer_succeeds_but_real_login_returns_401`
      — reproduit le scénario où le primer pose un cookie XSRF puis le vrai
      login répond 401 (compte API sans accès au catalogue PI).
  - **Ancien test `SearchTrademarks_with_invalid_login_returns_invalid_credentials`**
    devenu ambigu (stubbait toutes les `/auth/login` à 401, dont le primer
    qui n'a alors plus de cookie XSRF) — renommé en
    `SearchTrademarks_fails_unavailable_when_csrf_primer_returns_no_cookie`
    et stub remplacé par `503` pour exprimer le cas explicitement.
  - **Validation manuelle curl** contre l'INPI live : avant le fix, 403
    « CSRF token null » sur le 1er POST ; après le fix, le flow CSRF
    passe et l'INPI traite la requête jusqu'à la vérification credentials.
- **Lot 10 — Automatisation E2E API contre l'INPI réel (workflow GitHub
  Actions + Bruno CLI)** : la couverture Bruno (100 % endpoints, Lot 8) +
  les mappings HTTP corrects (Lot 9) deviennent **exécutables en automatique**
  par un job CI dédié qui frappe l'INPI **en production**.
  - **`.github/workflows/bruno-inpi-e2e.yml`** : nouveau workflow distinct
    de `ci.yml`. Trigger `workflow_dispatch` (bouton « Run workflow ») +
    `schedule: cron '0 2 * * *'` (nightly 02:00 UTC, avant les jobs
    Hangfire `feed-polling` et `favorite-refresh`). Pas sur push/PR pour
    protéger les secrets INPI et éviter le rate-limit côté INPI à chaque
    contribution.
  - **Stack** : `services.postgres` (postgres:16-alpine) + `actions/setup-dotnet@v4`
    (10.0.x) + `actions/setup-node@v4` (20) + `npm install -g @usebruno/cli`.
    Migrations EF Core appliquées avant le start API. `Atlas.Api` démarré
    en background avec un `wait-for-ready` (poll `curl localhost:5023/` jusqu'à
    60 s). Logs API uploadés en artefact si échec.
  - **`bruno/90-INPI-E2E-CI/` (14 étapes orchestrées, séquence ordonnée)** :
    Register → Dev fetch verification token → Verify email → Login →
    INPI Connect (POST `/inpi/connection` avec username/password réels)
    → INPI Status (vérifie `isConnected=true`) → Company Detail (RENAULT
    SIREN 552032534) → Company Search (« Renault ») → Trademark Search
    (capture `depositNumber`) → Trademark Detail → Patent Search (déposant
    Renault, capture `publicationNumber`) → Company Attachments (F-013) →
    INPI Disconnect → Account Delete (cleanup RGPD cascade).
  - **`bruno/environments/CI.bru`** : nouveau profil dédié au workflow.
    `baseUrl=http://localhost:5023`, email unique par run via
    `e2e-{{process.env.GITHUB_RUN_ID}}@atlas-ci.test`, password fixe pour
    ce profil, credentials INPI injectés via `bruno/.env` créé à la volée
    depuis les secrets GitHub.
  - **Secrets repo requis** (à créer côté Settings → Secrets and variables) :
    `INPI_USERNAME`, `INPI_PASSWORD`. Le workflow `bruno-inpi-e2e` échoue
    explicitement avec un message clair si les secrets sont absents.
  - **Artefacts uploadés** : `bruno-results.xml` (JUnit), `bruno-results.json`,
    `bruno-results.html` (rapports Bruno) + `atlas-api.log` (sur échec API
    seulement). Téléchargeables depuis l'onglet « Actions » du run.
  - **Concurrency lock** : `group: bruno-inpi-e2e` + `cancel-in-progress: false`
    pour éviter qu'un nightly et un déclenchement manuel se chevauchent et
    cognent l'INPI en double.
- **Lot 9 — Mapping HTTP des 6 codes d'erreur veille manquants** :
  ferme la dette annexe documentée par Lot 8. Les codes métier
  `veille.feed_rule_not_found`, `feed_rule_forbidden`,
  `veille_pack_immutable`, `veille_pack_forbidden`,
  `veille_pack_not_public`, `veille_pack_code_already_used`
  retournent désormais leur **statut HTTP attendu** au lieu de
  retomber sur 400 par défaut (`ErrorHttpMapping.cs`) :
  | Code métier | Avant | Après |
  |---|---|---|
  | `veille.feed_rule_not_found` | 400 | **404** |
  | `veille.feed_rule_forbidden` | 400 | **403** |
  | `veille.veille_pack_immutable` | 400 | **409** |
  | `veille.veille_pack_forbidden` | 400 | **403** |
  | `veille.veille_pack_not_public` | 400 | **404** |
  | `veille.veille_pack_code_already_used` | 400 | **409** |
  4 tests d'intégration ajoutés (`VeilleErrorMappingTests`) :
  `Update_unknown_feed_rule_returns_404_not_400`,
  `Delete_unknown_feed_rule_returns_404_not_400`,
  `Create_user_pack_with_duplicate_code_returns_409_not_400`,
  `Like_unknown_pack_returns_404_not_400` (sanity-check du chemin
  `veille_pack_not_found` déjà mappé pour éviter une régression).
  Les assertions Bruno correspondantes (Lot 8) deviennent
  exécutables avec leurs statuts annoncés sans contournement.
- **Lot 8 — Couverture Bruno fermée à 100 % (rules / marketplace / a11y +
  variantes négatives + capture runtime + E2E)** : suite directe de l'audit
  Bruno profond livré post-Lot 7. Ferme les 14 endpoints non couverts +
  ajoute les variantes négatives identifiées + capture runtime sur Lot 7
  + premier E2E flow F-046.
  - **`21-Feed-Rules` (F-046, 4 endpoints + cas négatif)** :
    `01 Create rule` (201, capture `ruleId`),
    `02 List rules`, `03 Update rule`, `04 Delete rule`,
    `Create empty (400)` qui valide `veille.invalid_feed_rule`.
  - **`22-Veille-Marketplace` (F-049, 8 endpoints)** :
    `01 List community` (PagedResult),
    `02 Create user pack` (201 + capture `userPackCode`),
    `03 My authored`, `04 Publish` (`isPublic=true`),
    `05 Like`, `06 Unlike` (204 idempotent),
    `07 Report` (202 Accepted),
    `08 Unpublish` (`isPublic=false`).
  - **`23-Accessibility` (Lot 5c, 3 requêtes)** :
    `01 Get defaults` (200, vérifie `{ false, false, "Default" }`,
    contrôle la sérialisation `JsonStringEnumConverter` en chaîne),
    `02 Update` (204),
    `03 Round-trip (GET reflects PUT)` qui valide la persistance EF
    owned-type.
  - **`24-Rules-Flow` (E2E F-046, 5 étapes)** : create → list contains
    → patch → delete → list excludes. Valide le cycle complet d'une
    règle de surveillance avec capture / réutilisation de `ruleId`.
  - **Variantes négatives ajoutées aux dossiers Lot 7** :
    `16-Company-Attachments/Download confidential (403)` —
    `companies.attachment_confidential`, cas doctrinal documenté ;
    `17-Patents/Detail not found (404)` ;
    `18-IP-Favorites/Trademarks Add invalid (400)` +
    `Patents Add invalid (400)` (`favorites.invalid_deposit_number` /
    `favorites.invalid_publication_number`) ;
    `19-Favorites-Export/Empty CSV (header only)` (cas limite ETL) ;
    `20-Company-Report/Report not found (404)`.
  - **Capture runtime sur Lot 7** : `script:post-response` ajouté à
    `16-Company-Attachments/List` (capture `attachmentId` du premier
    item ouvert + `confidentialAttachmentId` du premier confidentiel)
    et à `17-Patents/Search` (capture `publicationNumber`). Le user
    n'a plus à copier-coller pour enchaîner List → Download ou
    Search → Detail.
  - **Variables d'environnement étendues** : `confidentialAttachmentId`,
    `siren404` (défaut `123456782`, SIREN Luhn-valide mais inexistant),
    `ruleId`, `userPackCode` (défaut `bruno-test-pack`),
    `communityPackCode` (défaut `pi-cabinet`).
  - **Dette annexe documentée** (non corrigée dans cette PR — pure
    Bruno) : `veille.feed_rule_not_found`,
    `veille.feed_rule_forbidden`, `veille.veille_pack_immutable`,
    `veille.veille_pack_forbidden`, `veille.veille_pack_not_public`,
    `veille.veille_pack_code_already_used` ne sont **pas** mappés
    dans `ErrorHttpMapping` et retombent en 400 par défaut. À corriger
    dans une PR backend dédiée (vers 404 / 403 / 409 selon le sens).
  - Couverture endpoints : passe de **74 %** à **100 %** (55/55).
- **Lot 7 — Couverture Bruno des features MVP 2 manquantes** : 16 requêtes
  Bruno ajoutées dans 5 nouveaux dossiers, complète la collection backend
  pour préparer F-028 (API publique) et débloquer le test manuel des
  features livrées récemment.
  - **`16-Company-Attachments` (F-013)** — `List attachments`,
    `Download` (PDF stream, signature `%PDF`, Content-Type
    `application/pdf`), `Download not found (404)` qui valide le
    `ProblemDetails.title=companies.attachment_not_found`.
  - **`17-Patents` (F-015 + F-016)** — `Search` multi-critères
    (PagedResult), `Search empty (400)` qui valide
    `patents.empty_search`, `Detail` par numéro de publication.
  - **`18-IP-Favorites` (F-018)** — 6 requêtes : Trademarks Add / List /
    Remove + Patents Add / List / Remove. Valide les codes `204` /
    `404` / `409` et la structure DTO `{ depositNumber|publicationNumber,
    name|title?, addedAt }`.
  - **`19-Favorites-Export` (F-021)** — 3 exports CSV (companies,
    trademarks, patents). Valide `text/csv; charset=utf-8`,
    `Content-Disposition: attachment; filename=favoris-*.csv`, BOM
    UTF-8 (compat Excel FR) ou en-tête CRLF/RFC 4180.
  - **`20-Company-Report` (F-022)** — `Report PDF` : valide
    `application/pdf`, `Content-Disposition` avec
    `atlas-<siren>.pdf`, signature `%PDF`.
  - **Variables d'environnement étendues** (`environments/Local.bru`) :
    `attachmentId`, `publicationNumber`, `patentTitle`,
    `patentInventor`, `patentApplicant`. README mis à jour avec les 5
    nouveaux dossiers et leur table d'endpoints.
- **Lot 6 — Tests d'architecture étendus (Lot 3 résiduel de l'audit)** :
  ferme la dette d'audit côté contrôle automatique. NetArchTest + parsing
  `.csproj` couvrent désormais tous les projets de la solution.
  - **`Atlas.Maui` : couverture par parsing `.csproj`** — multi-target
    `net10.0-{android,ios,maccatalyst,windows}` non référencable depuis un
    projet de tests classique. Le `.csproj` est lu directement via XLINQ.
    Test : `Atlas_Maui_csproj_should_only_reference_Domain_and_Shared` —
    interdit toute référence à `Atlas.Application`, `Atlas.Infrastructure.*`
    ou `Atlas.Api` (le client mobile ne doit JAMAIS embarquer ces couches —
    décompilation possible, cf. CLAUDE.md).
  - **`Atlas.Infrastructure.*` : 8 projets couverts par `[Theory]`** —
    Persistence, Inpi, Veille, Messaging, Security, Cache, Storage, Bodacc.
    Test :
    `Infrastructure_csproj_should_not_reference_other_infrastructure_or_api`
    — interdit (1) auto-référence à `Atlas.Api`, (2) cross-référence à un
    autre `Atlas.Infrastructure.*`, (3) référence à `Atlas.Maui`. La
    composition se fait uniquement dans `Atlas.Api` (composition root).
  - **Convention handlers MediatR `internal sealed`** — Tests
    `Application_handlers_must_be_internal_sealed` et
    `Application_premium_handlers_must_be_internal_sealed` (`ConventionTests`)
    parcourent tous les types implémentant `IRequestHandler<,>` /
    `IRequestHandler<>` / `INotificationHandler<>` / `IStreamRequestHandler<,>`
    et vérifient qu'ils sont bien `sealed` ET non `public`. CLAUDE.md
    « Patterns à utiliser systématiquement » — empêche un handler exposé en
    `public` de fuiter le contrat interne du module.
  - **Helper `FindRepoRoot()`** — remonte les dossiers depuis
    `AppContext.BaseDirectory` jusqu'au répertoire contenant `Atlas.slnx`
    pour rendre le path des `.csproj` robuste aux profils de build
    (Debug/Release, output dir custom, runner CI).
  - 11 nouveaux tests d'archi (20 au total dans `Atlas.Architecture.Tests`,
    vs 9 avant). 391 tests verts au total.
- **Lot 5d — Câblage des polices facilitantes et `IMotionCoordinator`** :
  termine la couverture WCAG 2.2 AA des préférences accessibilité côté
  client MAUI initiées par les Lots 5b et 5c.
  - **Polices facilitantes câblées** : `MauiProgram` enregistre
    `OpenDyslexic-Regular.ttf` (alias `OpenDyslexicRegular`) et
    `AtkinsonHyperlegible-Regular.ttf` (alias `AtkinsonHyperlegibleRegular`)
    en plus d'OpenSans. La préférence utilisateur `FontPreference`
    (Lot 5c) résout désormais effectivement la famille de police via
    `ThemeManager.ResolveFontFamily(...)` et met à jour la ressource
    racine `AppFontFamily` à chaque `Apply()`.
  - **`Resources/Styles/Styles.xaml`** : les Setter `FontFamily="OpenSansRegular"`
    en dur sont remplacés par `{DynamicResource AppFontFamily}` — un swap
    de la ressource racine se propage immédiatement à tous les Label,
    Button, Entry, SearchBar, Picker, etc.
  - **`App.xaml`** : nouvelle ressource racine `<x:String x:Key="AppFontFamily">OpenSansRegular</x:String>`
    qui devient le point de bascule unique pour la police effective.
  - **Assets `.ttf` non commités** : les binaires sont sous licence
    SIL Open Font License 1.1, redistribution autorisée mais hors
    périmètre du repo. Un README dédié
    (`src/Atlas.Maui/Resources/Fonts/README-fonts-facilitantes.md`)
    documente noms exacts attendus, sources officielles
    (opendyslexic.org, brailleinstitute.org), licence à joindre et
    procédure de dépôt. Tant que les fichiers sont absents, MAUI
    retombe sur OpenSans au runtime sans casser l'app.
  - **`IMotionCoordinator` + `MotionCoordinator`** : nouveau port
    accessibilité dans `Atlas.Maui.Theming`. Façade des extensions
    d'animation MAUI (`FadeToAsync`, `TranslateToAsync`, `ScaleToAsync`)
    qui consulte `ThemeManager.ReduceMotion` à chaque appel et applique
    l'état final instantanément quand l'utilisateur a désactivé les
    animations. Enregistré en singleton DI. À utiliser pour toute
    animation MAUI non essentielle au lieu d'appeler directement les
    extensions `VisualElement`.
  - Build Release vert sur les 3 cibles MAUI (`net10.0-android` 1m54s,
    `net10.0-ios` 15s, `net10.0-maccatalyst` 23s).
- **Lot 5c — Préférences d'accessibilité utilisateur, bout-en-bout** :
  pipeline complet backend + client MAUI pour les préférences
  d'accessibilité portées par l'utilisateur, synchronisées multi-device
  (cf. `docs/06-accessibilite.md` §6.3).
  - **Domain** : record `UserAccessibilityPreferences(HighContrast,
    ReduceMotion, FontPreference)` + enum `AccessibilityFontPreference`
    (Default / DyslexiaFriendly / HighReadability). `User.AccessibilityPreferences`
    owned type, valeur initiale `UserAccessibilityPreferences.Default`,
    méthode `User.UpdateAccessibilityPreferences` idempotente.
  - **Persistence** : EF Core `OwnsOne` mapping (`a11y_high_contrast`,
    `a11y_reduce_motion`, `a11y_font_preference` colonnes sur `users`).
    Migration `AddUserAccessibilityPreferences` (3 colonnes, defaults `false` /
    `'Default'`, backfill non destructif).
  - **Application** : `GetAccessibilityPreferencesQuery` /
    `UpdateAccessibilityPreferencesCommand` MediatR, validator
    FluentValidation, DTO `AccessibilityPreferencesDto`.
  - **Api** : `AccessibilityEndpoints` — `GET /user/preferences/accessibility`
    (retourne les préférences ; défauts neutres pour un nouvel utilisateur)
    + `PUT /user/preferences/accessibility` (NoContent). `RequireAuthorization`
    + `TryGetUserId` claim. Mapping HTTP via `users.not_found` → 404 déjà en place.
  - **MAUI** : `AccessibilityPreferencesPage` accessible depuis un nouvel
    onglet « Accessibilité » du `TabBar`. `AccessibilityPreferencesViewModel`
    charge depuis l'API à `OnAppearing`, applique localement via
    `ThemeManager.SetAccessibility(...)`, pousse au `Save`.
    Toggles « Contraste élevé » et « Réduire les animations » + Picker police
    facilitante (Default / DyslexiaFriendly / HighReadability). Tous les
    contrôles équipés WCAG 2.2 AA (cf. Lot 5b — SemanticProperties, font
    auto-scaling, taille tactile 44 px).
  - **Application au runtime** : `ThemeManager` étendu —
    `HighContrast = true` force le thème « Contraste » du kit (sans écraser
    la préférence `Theme` de l'utilisateur, revert transparent au
    désactivation). `ReduceMotion` exposé comme flag global pour les
    futures animations. `FontPreference` persistée et synchronisée mais
    **application visuelle reportée** : aucun asset `.ttf` custom
    (OpenDyslexic, Atkinson Hyperlegible — SIL OFL 1.1) embarqué dans
    cette PR, à intégrer dans une mise à jour ultérieure une fois la
    licence et le poids assets validés.
  - **`AtlasApiClient`** : nouvelles méthodes `GetAccessibilityPreferencesAsync`
    + `UpdateAccessibilityPreferencesAsync` (PUT JSON via le pipeline
    `SendWithAuthAsync` qui gère le refresh JWT automatique).
  - **Tests** : 4 tests Domain (`UserAccessibilityPreferencesTests`),
    4 tests Application (`AccessibilityPreferencesHandlersTests`),
    3 tests intégration API (`AccessibilityPreferencesTests` —
    défauts pour un nouvel utilisateur, round-trip PUT/GET, accès non
    authentifié → 401).
  - Build Release vert sur les 3 cibles MAUI (`net10.0-android` 2m07s,
    `net10.0-ios` 13s, `net10.0-maccatalyst` 27s).
- **Lot 5b — Accessibilité MAUI WCAG 2.2 AA sur les 4 vues actuelles** :
  première passe d'accessibilité sur l'ensemble des vues XAML
  (`LoginPage`, `CompanySearchPage`, `CompanyDetailPage`, `SearchHistoryPage`).
  Conformité à la checklist universelle de `docs/06-accessibilite.md` §12.1.
  - **`SemanticProperties.Description` et `Hint`** sur tous les éléments
    interactifs (Entry, Button, SearchBar, items CollectionView) — un
    lecteur d'écran annonce désormais le rôle et l'action de chaque
    contrôle, et chaque item de résultat est introduit par son entité
    (« Entreprise X », « Recherche Y », « Dirigeant Z »).
  - **`SemanticProperties.HeadingLevel`** sur la structure documentaire de
    chaque page (Level1 sur le titre principal, Level2 sur les sections,
    Level3 sur les items de liste). Permet la navigation par titres au
    lecteur d'écran.
  - **`FontAutoScalingEnabled="True"`** partout — les pages respectent
    désormais la taille de texte système (Dynamic Type iOS, Font Scale
    Android), test obligatoire de `docs/06` §4.4.
  - **Couleurs sémantiques dynamiques** : `TextColor="Red"` et `"Gray"`
    en dur remplacés par `{DynamicResource error}` / `{DynamicResource
    onSurfaceVariant}` / `{DynamicResource onSurface}` / `{DynamicResource
    onBackground}` / `{DynamicResource primary}` — câble les vues sur le
    kit de thèmes WCAG 2.2 AA (PR #52) et débloque le passage clair / sombre
    sans contraste cassé.
  - **`MinimumHeightRequest="44"`** sur Entry, SearchBar, items
    cliquables CollectionView — respecte le critère WCAG 2.5.5 (cible
    tactile 44×44 px Apple HIG).
  - **Annonces `SemanticScreenReader.Default.Announce`** dans les
    ViewModels (`LoginViewModel`, `CompanySearchViewModel`,
    `CompanyDetailViewModel`) après chaque action significative : succès /
    échec de connexion, nombre de résultats trouvés, succès / échec de
    chargement d'une fiche. L'utilisateur lecteur d'écran sait que l'état
    a changé sans avoir à scruter visuellement.
  - **`BaseViewModel.HasError`** propriété calculée + `NotifyPropertyChangedFor`
    sur `ErrorMessage` — permet aux vues de masquer la zone d'erreur via
    `IsVisible="{Binding HasError}"` au lieu d'afficher un label vide
    (qui serait annoncé par le lecteur d'écran).
  - Build Release vert sur les 3 cibles MAUI (`net10.0-android` 1m26s,
    `net10.0-ios` 9s, `net10.0-maccatalyst` 17s).
- **Lot 5a — Pré-prod produit, client MAUI** : 2 items résiduels de pré-prod
  produit ferment leur ticket avant la suite UI.
  - **`AtlasApiClient` base URL configurable** : `AtlasApiOptions` passe de
    constante statique à classe d'instance résolvant la base URL par deux
    couches — (1) override utilisateur via `Preferences` (clé
    `Atlas.BaseUrl`, utile staging / self-hosted), (2) défaut par plateforme
    via `#if` (Android émulateur `10.0.2.2`, iOS / MacCatalyst / Windows
    `localhost`). L'asymétrie connue de l'émulateur Android n'est plus dans
    le client par hasard.
  - **Build iOS / MacCatalyst Release débloqué** : `[SuppressMessage]` ciblé
    `CA1711` posé sur `Atlas.Maui.AppDelegate` (iOS + MacCatalyst) avec
    justification explicite (« Apple UIKit/AppKit convention requires the
    'Delegate' suffix on UIApplicationDelegate-derived types »). Build
    Release MAUI sur les 3 cibles (`net10.0-android` 1m27s,
    `net10.0-ios` 9s, `net10.0-maccatalyst` 17s) → vert. Débloque le
    packaging IPA / PKG pré-prod.
- **Lot 4 — Dette structurée résolue (audit profond)** : convergence des 4 points
  P2 / P3 du Lot 4.
  - **Combinateurs `Result` / `Result<T>`** ajoutés dans `Atlas.Shared` (purement
    additif, le boilerplate `if (x.IsFailure) return Result<Y>.Fail(x.Error!);`
    n'est plus imposé) : `Map`, `Bind` (générique et non générique), `Match`,
    `Tap`, `Ensure`, `TryGetValue`. Conversion implicite `Error` → `Result` /
    `Result<T>` pour `return error;`.
  - **`Result<T>.Value` garde-fou** : accès en état d'échec lève
    `InvalidOperationException` au lieu de retourner `default(T)`
    silencieusement. Un `result.Value!` placé après un check oublié devient
    bruyant. Migration : utiliser `TryGetValue` ou `Match`.
  - **Splits « 1 type / 1 fichier »** (docs/08 §2.3) : `Result.cs` éclaté en
    `Error.cs` + `Result.cs` + `ResultOfT.cs` ; `IJwtIssuer.cs` éclaté en
    `AccessToken.cs` + `IJwtIssuer.cs`.
  - **Domain events câblés via MediatR** : `IDomainEvent` hérite désormais de
    `MediatR.INotification` (seule `MediatR.Contracts` est tirée par
    `Atlas.Domain`, sémantique « interfaces marqueur » autorisée par CLAUDE.md).
    `AtlasDbContext.SaveChangesAsync` collecte les événements des entités
    tracked, commit, puis publie via `IPublisher` (sémantique after-commit —
    aucun event publié si la transaction échoue). `IHasDomainEvents` marqueur
    introduit pour découpler le scan du ChangeTracker du paramètre TId.
    `UserRegisteredDomainEvent` est désormais effectivement reçu par les
    `INotificationHandler<UserRegisteredDomainEvent>`.
  - **`Atlas.Shared.UnitTests` (nouveau projet, 19 tests)** : couverture
    `Result` / `Result<T>` (Map / Bind / Match / Tap / Ensure / TryGetValue /
    guard sur Value / conversion implicite depuis Error).
  - **Tests d'archi (+2)** : nouveau test
    `Domain_should_only_reference_MediatR_Contracts_not_MediatR_runtime` qui
    différencie le package marqueur (autorisé) du runtime MediatR (interdit) via
    `Assembly.GetReferencedAssemblies()`. Nouveau test
    `Shared_should_not_depend_on_any_atlas_project_or_external_lib` qui ferme la
    porte à toute fuite progressive (logging, mediator, EF, Serilog, Polly,
    FluentValidation, Npgsql) dans le noyau `Atlas.Shared`.
  - **`Atlas.Shared` retargeté `netstandard2.1` → `net10.0`** : tous les
    consommateurs réels (y compris les têtes MAUI net10.0-*) sont net10.0.
    Suppression du polyfill `IsExternalInit.cs` désormais inutile. Débloque
    l'usage natif de `ArgumentNullException.ThrowIfNull` dans les combinateurs.
  - **Test `SearchCompaniesByNameHandlerTests`** corrigé : un SIREN invalide
    Luhn (`775665019`) avait été collé en fixture — révélé par le nouveau
    guard `Value` qui throw au lieu de retourner `null` silencieusement. SIREN
    remplacé par un SIREN valide (`954506077`, Renault Trucks).
- **Audit profond — clos (29 mai 2026)** : l'audit lecture-seule de la
  solution (`docs/audit/`, daté du 2026-05-28, 9 rapports : Shared / Domain /
  Application / Persistence / Infra-adapters / Api / Tests / MAUI + synthèse)
  est **clos** côté snapshot. **Les 2 P0 sont livrés** (Lot 0 — pipeline CI
  GitHub Actions ; Lot 1 — cascades FK RGPD sur les 3 tables veille +
  mapping HTTP `veille.*`). **Lots 2a (crypto + anti-SSRF + Security.UnitTests
  37 tests) et 2b (rate limiting + en-têtes sécurité + CORS strict + Serilog
  + masquage) livrés**. Les rapports sont supprimés (récupérables via git
  history) ; les items P1/P2 résiduels sont à instruire au fil :
  - **Lot 3 (tests d'archi étendus) — livré** : 20 tests d'archi (NetArchTest
    + parsing `.csproj`) couvrent Domain, Application, Application.Premium,
    Shared, Atlas.Maui (via parsing multi-target), et les 8 projets
    `Atlas.Infrastructure.*` (Persistence, Inpi, Veille, Messaging, Security,
    Cache, Storage, Bodacc). Convention handlers MediatR `internal sealed`
    également vérifiée automatiquement.
  - **Lot 4 (dette structurée) — livré** : combinateurs `Result.Map` / `Bind` /
    `Match` / `Tap` / `Ensure` / `TryGetValue` ajoutés, garde-fou sur
    `Result<T>.Value`, `Result.cs` éclaté en `Error.cs` + `Result.cs` +
    `ResultOfT.cs`, `IJwtIssuer.cs` éclaté en `AccessToken.cs` + `IJwtIssuer.cs`,
    domain events câblés via `IPublisher` MediatR dans `SaveChangesAsync`,
    projet `Atlas.Shared.UnitTests` créé (19 tests), 2 nouveaux tests d'archi.
  - **Lot 5 (pré-prod produit) — Lots 5a, 5b, 5c et 5d livrés** :
    adapter Brevo ; base URL `AtlasApiClient` externalisée ; build iOS
    / MacCatalyst Release débloqué ; **accessibilité MAUI WCAG 2.2 AA**
    sur les 4 vues XAML ; **préférences accessibilité utilisateur
    bout-en-bout** (Domain owned type + endpoints
    `/user/preferences/accessibility` + migration + page MAUI +
    application runtime via `ThemeManager.SetAccessibility(...)`) ;
    **polices facilitantes câblées** (MauiProgram enregistre
    OpenDyslexic + Atkinson Hyperlegible, Styles globaux pointent sur
    `{DynamicResource AppFontFamily}`, swap immédiat selon
    `ThemeManager.FontPreference`) ; **`IMotionCoordinator`** branché
    sur `ReduceMotion` pour façonner les animations MAUI. Reste hors
    code : tests utilisateurs réels avec associations (Valentin Haüy,
    APF…), dépôt manuel des binaires `.ttf` SIL OFL 1.1
    (`src/Atlas.Maui/Resources/Fonts/README-fonts-facilitantes.md`).

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
- Pin de sécurité `System.Security.Cryptography.Xml` 10.0.8 (CVE GHSA-37gx-xxp4-5rgx) — porté à
  10.0.12 en septembre, voir ci-dessous.
- **Dépendances vulnérables (NuGet Audit, 25 septembre 2026)** : la CI était rouge depuis le
  10 septembre sur des advisories publiées de fin juin à mi-septembre (`NU1902`/`NU1903` en
  Warning-As-Error au restore). Montées de version, sans suppression d'advisory : pin
  `System.Security.Cryptography.Xml` 10.0.8 → 10.0.12 ; `Microsoft.AspNetCore.OpenApi`
  10.0.8 → 10.0.12 (tire `Microsoft.OpenApi` ≥ 2.12, GHSA-v5pm-xwqc-g5wc) ; `Testcontainers` +
  `Testcontainers.PostgreSql` 4.12 → 4.15 (SSH.NET 2026.0.0) ; `WireMock.Net` 2.7 → 2.18
  (Scriban.Signed 7.2.5) ; tête WASM d'`Atlas.App` : `System.Security.Cryptography.Xml` 10.0.12 en
  référence directe (transitif 10.0.7 de `Microsoft.Windows.Compatibility`) et audit NuGet promu en
  erreur (`WarningsAsErrors` NU1901–NU1904). Ajout de Dependabot (NuGet + Actions), d'un run CI
  hebdomadaire + `workflow_dispatch` ; `ci.yml` exécute désormais `Atlas.Shared.UnitTests` et
  `Atlas.Infrastructure.Storage.UnitTests` ; le nightly `bruno-inpi-e2e` expose Postgres sur 5433
  (port de la factory design-time et du harness `docs/13` — il n'avait jamais passé l'étape de
  migration). Détail : `docs/17` §9.
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
