# Roadmap features

> Catalogue détaillé des fonctionnalités du projet, organisé selon la méthode **MoSCoW** (Must / Should / Could / Won't have for now) et par jalon de version.
> Chaque feature est décrite avec sa valeur utilisateur, sa complexité technique, les APIs externes requises et les dépendances vers d'autres features.

**Version** : 1.0
**Date de dernière mise à jour** : 27 mai 2026

---

## État d'avancement — MVP 1

Légende : ✅ livré & vérifié · 🟡 livré, vérification partielle (cf. statut détaillé de la feature) · ⬜ à faire.

| Feature | Statut |
|---|:--:|
| F-001 Inscription / connexion | ✅ |
| F-002 2FA TOTP | ✅ |
| F-003 Connexion compte INPI | ✅ |
| F-004 Recherche entreprise (SIREN) | 🟡 |
| F-005 Recherche entreprise (nom) | 🟡 |
| F-006 Recherche marque | 🟡 |
| F-007 Fiche marque | 🟡 |
| F-008 Historique de recherches | ✅ |
| F-009 Client MAUI mobile | 🟡 |
| F-010 Client MAUI desktop | 🟡 |
| F-011 Documentation | ✅ |
| F-012 Conformité RGPD | 🟡 |

Les 🟡 correspondent surtout à : intégrations INPI à confirmer par un appel authentifié réel
(F-004→F-007), clients MAUI compilés mais non exécutés/QA (F-009/F-010), et contenu légal UI
restant pour le RGPD (F-012). Le détail figure dans le bloc « Statut » de chaque feature.

---

## Sommaire

- [Méthodologie](#méthodologie)
- [Conventions](#conventions)
- [MVP 1 — Must have (mois 1–4)](#mvp-1--must-have-mois-1-4)
- [MVP 2 — Should have (mois 4–8)](#mvp-2--should-have-mois-4-8)
- [Cluster Veille (intégré au MVP 2)](#cluster-veille-intégré-au-mvp-2)
- [V2 — Could have (mois 8–18)](#v2--could-have-mois-8-18)
- [V3+ — Won't have (yet)](#v3--wont-have-yet)
- [Features explicitement écartées](#features-explicitement-écartées)

---

## Méthodologie

La méthode **MoSCoW** classe les features en 4 buckets :

- **Must have** : le produit ne peut pas exister sans. Si on ne livre pas ça, on n'a pas livré.
- **Should have** : très important mais le produit existe sans. À livrer en deuxième.
- **Could have** : bien mais pas critique. À livrer si le temps le permet.
- **Won't have (yet)** : explicitement reporté, documenté pour ne pas être oublié.

**Pourquoi ce choix** : MoSCoW est plus rigoureux que la simple priorisation 1-2-3 parce qu'il force à classer les features en **catégories qualitativement différentes**. On évite l'inflation des priorités où tout devient "P1".

---

## Conventions

Pour chaque feature, on documente :

- **Description** : ce que ça fait du point de vue utilisateur.
- **Valeur user** : pourquoi c'est utile (à quel besoin ça répond).
- **Complexité** : estimation technique de l'effort.
  - ★ : 1-2 jours
  - ★★ : 3-5 jours
  - ★★★ : 1-2 semaines
  - ★★★★ : 3-4 semaines
  - ★★★★★ : 1-2 mois
- **APIs externes** : sources de données nécessaires.
- **Dépendances** : autres features prérequises.

---

## MVP 1 — Must have (mois 1–4)

**Objectif** : prouver que la stack technique fonctionne, que l'archi hex tient la route, et offrir une utilité minimale crédible. Pas de monétisation, repo encore privé. **À la fin du MVP 1, le repo passe en public.**

### F-001 — Inscription et connexion utilisateur

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Auth hexagonale custom — Argon2id, JWT RS256, refresh tokens rotatifs (cf. ADR-010). Endpoints `/auth/{register,verify-email,login,refresh,logout}`. 2FA (F-002) à suivre.

**Description** : un utilisateur peut créer un compte sur la plateforme (email + mot de passe), valider son email, se connecter et se déconnecter.

**Valeur user** : prérequis à toute personnalisation et à la connexion d'un compte INPI multi-tenant.

**Complexité** : ★★

**APIs externes** : aucune (auth interne).

**Dépendances** : aucune.

**Détails techniques** : ASP.NET Core Identity (ou alternative type Auth0 / Keycloak à décider). Hashing bcrypt / Argon2. Email transactionnel (Mailjet, Brevo, ou self-hosted Postfix pour démarrer).

---

### F-002 — Authentification deux facteurs (2FA)

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). TOTP RFC 6238 (Otp.NET), secret chiffré AES-256-GCM (`ICryptoService`), 10 codes de secours hashés, défi 2FA à la connexion. Endpoints `/auth/2fa/{setup,enable,disable,verify}`. WebAuthn/passkeys non couvert (futur).

**Description** : l'utilisateur peut activer un 2FA TOTP (Google Authenticator, Authy, etc.) pour sécuriser son compte.

**Valeur user** : protection contre les compromissions de mots de passe. Indispensable pour un service qui stocke des credentials INPI.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-001.

**Détails techniques** : RFC 6238 TOTP, génération QR code, codes de secours (10 codes one-time imprimables).

---

### F-003 — Connexion d'un compte INPI à son profil

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Test de connexion via `IInpiAuthenticationProvider` (`POST /sso/login` RNE), credentials chiffrés AES-256-GCM (`ICryptoService`, jamais loggés), statut Active/Invalid. Endpoints `/inpi/connection` (POST/GET/DELETE). Cache court du token RNE + refresh auto : non encore implémenté (sera ajouté avec F-004 qui consomme le RNE).

**Description** : depuis ses paramètres, l'utilisateur fournit ses identifiants INPI (email + mot de passe). Le système teste la connexion, stocke les credentials chiffrés, et marque le compte comme "actif".

**Valeur user** : sans ça, aucune fonction métier ne peut être utilisée. C'est le pont entre l'identité utilisateur SaaS et la capacité à appeler l'INPI.

**Complexité** : ★★★

**APIs externes** : INPI RNE (test de connexion).

**Dépendances** : F-001.

**Détails techniques** :
- Chiffrement des credentials en AES-256-GCM avec une clé dérivée du mot de passe utilisateur (ou idéalement une clé spécifique stockée dans un KMS type Azure Key Vault, AWS Secrets Manager, ou Vault).
- Test de connexion en appelant `/sso/login` du RNE.
- Stockage du token JWT obtenu en cache court (1h).
- Refresh automatique lorsque le token approche de l'expiration.

---

### F-004 — Recherche entreprise par SIREN

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026). Value object `Siren` (Luhn), port `ICompanyDataProvider`, adapter `RneCompanyProvider` (`GET /companies/{siren}`, token Bearer mis en cache + ré-auth sur 401), endpoint `GET /companies/{siren}`. **Mapping aligné sur la doc technique INPI v4.0** : navigation JSON **défensive** (`RneCompanyMapper`) — chemins documentés en primaire (`identite.denomination`, `entreprise.activitePrincipale.codeNAF`, `entreprise.adresseEntreprise`, `indicateurDiffusionINSEE`) + variantes en repli ; **dirigeants désormais mappés** (`composition.pouvoirs`). L'auth `/sso/login` est confirmée conforme. **Reste** : confirmation par un **appel authentifié réel** (compte INPI requis) — la navigation défensive dé-risque l'écart de schéma.

**Description** : l'utilisateur saisit un numéro SIREN (9 chiffres) et consulte la fiche complète de l'entreprise : identité, adresse, dirigeants, code NAF, activité, observations, établissements (le cas échéant).

**Valeur user** : c'est la fonction de base, l'équivalent du "search bar" d'un outil de recherche entreprise. Sans ça, le produit n'existe pas.

**Complexité** : ★★

**APIs externes** : INPI RNE (`GET /companies/{siren}`).

**Dépendances** : F-003.

**Détails techniques** :
- Validation SIREN côté Core (Luhn algorithm, format 9 chiffres).
- Mapping de la réponse JSON RNE vers le modèle de domaine `Company`.
- Gestion des champs absents (certaines entreprises n'ont pas tous les champs renseignés).
- Respect du flag de diffusion partielle (`diffusionINSEE = "N"` → affichage restreint).

---

### F-005 — Recherche entreprise par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026). `ICompanyDataProvider.SearchByNameAsync` → `PagedResult<CompanySummary>` (SIREN, dénomination, ville, NAF) ; endpoint `GET /companies?name=&page=&pageSize=` (réutilise le client RNE + cache token). Mapping des items aligné sur la doc INPI v4.0 (navigation défensive, cf. F-004). **Reste à valider en réel** : pagination (par page vs curseur `searchAfter`) et `TotalCount` (forme de réponse de recherche non documentée publiquement). Debouncing 300 ms côté client (MAUI, F-009).

**Description** : l'utilisateur saisit tout ou partie d'un nom d'entreprise, le système retourne une liste de résultats avec pagination.

**Valeur user** : la majorité des recherches commencent sans connaître le SIREN. Cette fonction est complémentaire de F-004.

**Complexité** : ★★

**APIs externes** : INPI RNE (`GET /companies?companyName=...`).

**Dépendances** : F-003.

**Détails techniques** :
- Debouncing côté client (300ms) pour éviter de saturer l'API.
- Pagination avec curseur (`searchAfter`).
- Affichage des résultats avec SIREN, dénomination, ville, code NAF.

---

### F-006 — Recherche marque par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle. Port `IIntellectualPropertyProvider`, adapter `InpiPiTrademarkProvider` (auth PI via cookies `access_token` + `XSRF-TOKEN`, cache par compte, retry 401), endpoint `GET /trademarks?name=&page=&pageSize=`. **Limites fortes** : l'API INPI PI (`api-gateway.inpi.fr`) a l'auth la plus complexe de l'écosystème et je n'avais pas le contrat réel — le flux d'auth, le corps de `POST /services/apidiffusion/api/marques/search` et le mapping sont **best-effort, isolés dans l'adapter PI**, à confronter à l'API réelle (compte INPI requis). Tests via WireMock contre des payloads supposés.

**Description** : l'utilisateur saisit un nom de marque, le système retourne les marques françaises correspondantes (vivantes ou non).

**Valeur user** : équivalent F-005 pour la PI. Première fonction démontrant la couverture PI du produit.

**Complexité** : ★★★

**APIs externes** : INPI PI (`POST /services/apidiffusion/api/marques/search`).

**Dépendances** : F-003.

**Détails techniques** :
- Auth INPI PI est plus complexe (XSRF + access_token + refresh_token). Cette feature force à implémenter cet adapter dès le MVP 1.
- Parsing XML ou JSON selon le format demandé.
- Affichage : nom marque, déposant, date dépôt, n° dépôt, statut juridique.

---

### F-007 — Vue détaillée d'une marque

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle. `TrademarkDetail` (déposant, dates, statut, type, classes de Nice via `NiceClassification`), `TrademarkImage` ; endpoints `GET /trademarks/{depositNumber}` et `GET /trademarks/{depositNumber}/image` (proxy binaire). Réutilise l'adapter PI de F-006 (`GET /marques/notice/{id}`, `GET /marques/image/{id}`). **Limites** : mêmes réserves best-effort que F-006 (contrat PI supposé, à confronter à l'API réelle). Tests via WireMock (notice + image).

**Description** : depuis un résultat de recherche, l'utilisateur ouvre la fiche détaillée d'une marque (notice complète, classes Nice, image/logo si disponible).

**Valeur user** : la valeur d'une recherche marque est dans le détail (qui possède quoi, depuis quand, pour quels produits/services).

**Complexité** : ★★

**APIs externes** : INPI PI (`GET /marques/notice/{id}`, `GET /marques/image/{id}`).

**Dépendances** : F-006.

---

### F-008 — Historique des recherches utilisateur

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Entité `SearchHistoryEntry` (`SearchType`, query, date), table `search_history`, rétention 200/user (prune). Enregistrement découplé via une notification MediatR `SearchPerformedNotification` publiée par les handlers de recherche (F-004/F-005/F-006) sur succès. Endpoint `GET /search-history`. Aucune API externe.

**Description** : chaque utilisateur a un historique de ses recherches (entreprises et marques), accessible depuis son profil.

**Valeur user** : retrouver rapidement une recherche précédente sans la refaire. Pose les bases des features futures (favoris, alertes).

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-001, F-004, F-006.

**Détails techniques** : table simple `SearchHistory` avec `UserId`, `SearchType`, `Query`, `Timestamp`. Limite à 200 entrées par utilisateur pour ne pas exploser la BDD.

---

### F-009 — Client MAUI mobile (Android/iOS) avec fonctions de base

> **Statut** : 🟡 Slice navigable implémenté (MVP 1, 27 mai 2026), **compilé Android** (non lancé/capturé : pas d'émulateur ici). Fondation : `AtlasApiClient` (seul point d'entrée vers l'API, refresh sur 401), `ITokenStore` via SecureStorage (Keychain/Keystore — aucun credential INPI sur le device), MVVM (CommunityToolkit). Écrans : Login → recherche entreprise (SIREN/nom) → fiche entreprise → historique ; Shell + onglets. **Reste** : écrans marques (F-006/F-007, même patron), auto-login au démarrage, bannière cookies/CGU, et **lancement/QA sur émulateur réel**. Respecte la règle d'archi (Atlas.Maui → Domain + Shared uniquement).

**Description** : application mobile native (Android et iOS via MAUI) permettant les fonctions F-004 à F-008 dans une UX adaptée mobile.

**Valeur user** : un utilisateur peut consulter une fiche entreprise depuis son téléphone en rendez-vous. Valeur métier énorme pour la cible "expert-comptable / avocat en déplacement".

**Complexité** : ★★★★

**APIs externes** : utilise le backend du projet.

**Dépendances** : F-001 à F-008.

**Détails techniques** :
- Authentification via JWT stocké dans le secure storage de la plateforme (Keychain iOS, Keystore Android).
- Pas de credentials INPI sur le device — uniquement le token de session.
- UI native MAUI (Shell + ContentPage).

---

### F-010 — Client MAUI desktop (Windows/macOS)

> **Statut** : 🟡 Amorcé (MVP 1, 27 mai 2026). Le client desktop est la **même app MAUI** (projet unique multi-cible) que F-009 : tout le code (ViewModels, `AtlasApiClient`, écrans) est partagé. **Compilé Windows** (`net10.0-windows`) — a nécessité de passer les `[ObservableProperty]` en propriétés partielles (compatibilité WinRT, MVVMTK0045). Adaptation grand écran : fenêtre dimensionnée (1100×800, min 800×600) sur desktop. **Reste** : adaptations UX écran large plus poussées (panneaux multiples, raccourcis clavier), **packaging MSIX (Windows) / PKG (macOS)** et **build macCatalyst** (machine macOS requise), QA sur cible réelle.

**Description** : application desktop reprenant les fonctions du mobile, avec une UX adaptée écran large.

**Valeur user** : utilisateurs intensifs (pro) qui préfèrent une app desktop dédiée à une interface web.

**Complexité** : ★★★

**APIs externes** : utilise le backend du projet.

**Dépendances** : F-009 (la majeure partie du code est partagée).

**Détails techniques** :
- Adaptations UI pour grand écran (panneaux multiples, raccourcis clavier).
- Packaging MSIX (Windows) et PKG (macOS).

---

### F-011 — Documentation utilisateur basique

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Générateur figé : **MkDocs Material** (léger, 100 % markdown, FR), dans `website/`. Pages : Accueil, Installation, Premiers pas, FAQ. Build `--strict` vérifié. Déploiement **GitHub Pages** via GitHub Actions (`.github/workflows/docs.yml`). À activer dans les réglages du repo (source = GitHub Actions ; Pages public nécessite le repo public).

**Description** : un site documentaire (Docusaurus, MkDocs ou similaire) hébergé sur GitHub Pages / GitLab Pages couvrant : installation, premiers pas, FAQ.

**Valeur user** : sans doc, les early adopters abandonnent à la première difficulté.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : aucune.

**Détails techniques** : choix du générateur à figer (Docusaurus si on veut une UX moderne, MkDocs si on veut quelque chose de très léger).

---

### F-012 — Conformité RGPD MVP

> **Statut** : 🟡 Backend implémenté (MVP 1, 27 mai 2026). **Export** (art. 20) : `GET /account/export` → JSON structuré sans données sensibles (pas de hash, ni credentials/secret INPI). **Effacement** (art. 17) : `DELETE /account` → suppression de l'utilisateur avec **cascade** EF (FK `ON DELETE CASCADE`) sur compte, refresh tokens, credentials INPI, codes 2FA, historique — validé sur Postgres réel. **Reste à faire (UI/contenu)** : politique de confidentialité, page CGU, bannière cookies (côté MAUI/site, F-009/F-011).

**Description** : politique de confidentialité, page CGU, mécanismes d'export et de suppression du compte utilisateur, bannière cookies.

**Valeur user** : obligation légale, mais aussi gage de sérieux pour les early adopters.

**Complexité** : ★★★

**APIs externes** : aucune.

**Dépendances** : F-001.

**Détails techniques** : voir doc dédiée sécurité/RGPD pour les détails.

---

## MVP 2 — Should have (mois 4–8)

**Objectif** : étoffer le produit avec les fonctions qui en font un vrai outil utilisable au quotidien. Le repo est public, on cherche les premiers feedbacks utilisateurs.

> **Le MVP 2 intègre également le Cluster Veille (F-041 à F-050)**, qui est documenté dans une section dédiée plus bas. Le cluster veille est une **feature majeure différenciante** (cf. ADR-009) et constitue la moitié du périmètre fonctionnel de ce jalon.

### F-013 — Téléchargement individuel d'actes et bilans

**Description** : depuis la fiche d'une entreprise, l'utilisateur voit la liste des actes (statuts, modifications) et bilans déposés, et peut télécharger chaque document en PDF.

**Valeur user** : usage central pour les experts-comptables et avocats — accéder rapidement aux documents juridiques d'une boîte.

**Complexité** : ★★

**APIs externes** : INPI RNE (`/companies/{siren}/attachments` + endpoint de téléchargement).

**Dépendances** : F-004.

---

### F-014 — Téléchargement en masse de documents

**Description** : l'utilisateur sélectionne plusieurs entreprises (via une liste de SIREN ou un panier) et lance un téléchargement groupé de tous leurs bilans/actes, livré sous forme d'archive ZIP avec une arborescence claire.

**Valeur user** : automatisation d'une tâche fastidieuse pour les cabinets traitant des dizaines/centaines d'entreprises.

**Complexité** : ★★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-013.

**Détails techniques** :
- Tâche asynchrone (background job via Hangfire ou MassTransit).
- Rate limiting respectant les contraintes INPI (espacer les downloads).
- Notification email + push quand l'archive est prête.
- Stockage temporaire (24h) puis suppression.

---

### F-015 — Recherche brevet par numéro

**Description** : recherche d'un brevet par son numéro de publication (FR, EP, WO), affichage de la notice + image d'abrégé.

**Valeur user** : couverture du périmètre PI au-delà des marques.

**Complexité** : ★★

**APIs externes** : INPI PI brevets.

**Dépendances** : F-003.

---

### F-016 — Recherche brevet par titre / inventeur / déposant

**Description** : recherche avancée multi-critères sur la base brevets.

**Valeur user** : veille technologique, recherche d'antériorité brevet.

**Complexité** : ★★★

**APIs externes** : INPI PI brevets (`/search`).

**Dépendances** : F-015.

**Détails techniques** : maîtrise de la syntaxe de requête SolR INPI (`[DENM=...]`, `[TIT=...]`, etc.) à abstraire derrière un query builder C# fluent.

---

### F-017 — Favoris : suivi d'une entreprise

> **Statut** : ✅ Backend implémenté (MVP 2, 28 mai 2026). Entité `CompanyFavorite` (UserId + Siren + NameSnapshot optionnel + AddedAt) avec index unique `(UserId, Siren)` et cascade FK RGPD sur User. Endpoints `POST /favorites/companies`, `DELETE /favorites/companies/{siren}`, `GET /favorites/companies`. Couvre la base du tableau de bord et débloque F-019 (alertes) puis F-047 (timeline mixte veille+favoris, feature phare). **Reste** : UI MAUI (onglet « Mes favoris »).

**Description** : l'utilisateur marque une entreprise en favori. La fiche est alors accessible depuis un onglet "Mes favoris" et automatiquement mise à jour à chaque consultation.

**Valeur user** : pose les bases du tableau de bord et des alertes.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-004, F-001.

---

### F-018 — Favoris : suivi d'une marque ou d'un brevet

**Description** : équivalent F-017 pour la PI.

**Valeur user** : suivi d'un portefeuille IP.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-006, F-015.

---

### F-019 — Alerte email sur modification d'une entreprise favorite

> **Statut** : ✅ Backend implémenté (MVP 2, 28 mai 2026). `CompanyFavoriteSnapshot` (un cliché vivant par `(UserId, SIREN)`) + `DiffWith(UniteLegale)` détecte les changements de dénomination / forme juridique / NAF / adresse / dirigeants (hash). Job Hangfire `favorite-refresh` cron `0 3 * * *` (désactivable). Notification MediatR `CompanyFavoriteChangedNotification` → 2 handlers : email (via `IEmailSender.SendFavoriteChangeAsync`, nouvelle méthode) et push (via `INotificationDispatcher`, cf. F-020). Users sans compte INPI connecté passés silencieusement. **Reste** : adapter email Brevo (actuellement `LoggingEmailSender` = dev only), templates email enrichis.

**Description** : un job quotidien re-fetch les fiches favorites. Si une modification est détectée (changement d'adresse, de dirigeant, dépôt d'un bilan, etc.), l'utilisateur reçoit un email résumant les changements.

**Valeur user** : première fonction de veille — passage d'un outil de consultation à un outil pro-actif.

**Complexité** : ★★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-017.

**Détails techniques** :
- Cron job nocturne.
- Mécanisme de diff entre snapshots de fiches.
- Persistance des snapshots historiques (utile aussi pour de futures features de timeline).
- Gestion fine des erreurs (un user dont le compte INPI a expiré ne bloque pas les autres).

---

### F-020 — Notifications push mobiles (et desktop)

> **Statut** : 🟡 FCM + APNs livrés, WNS restant. Adapters : **FCM** (Android + Web Push) JWT RS256 + OAuth2, **APNs** (iOS + macOS) JWT ES256 HTTP/2. Refactor (29 mai 2026) : interface interne `IPlatformPushDispatcher` + `CompositeNotificationDispatcher` qui fan-out vers tous les adapters configurés en parallèle (isolation des erreurs). Cleanup auto des tokens morts (FCM : 404 / UNREGISTERED ; APNs : 410 / BadDeviceToken). Bascule DI : au moins une plateforme configurée → composite ; aucune → `LoggingNotificationDispatcher`. Ports + endpoints (28 mai) : entité `DeviceRegistration`, port `INotificationDispatcher`, API `POST/DELETE/GET /devices`. **Reste** : adapter WNS (Windows desktop), client MAUI (récupération du token natif puis `POST /devices` au démarrage).

**Description** : alertes envoyées en push sur l'app mobile MAUI en complément des emails. Étendu pour couvrir aussi le desktop (Windows / macOS).

**Valeur user** : immédiateté de l'information.

**Complexité** : ★★★

**APIs externes** : Firebase Cloud Messaging (Android), APNs (iOS).

**Dépendances** : F-009, F-019.

---

### F-021 — Export CSV / Excel de résultats

**Description** : depuis une liste de résultats ou un panier, l'utilisateur exporte les données en CSV ou XLSX.

**Valeur user** : intégration avec les outils existants des utilisateurs (Excel, Google Sheets, CRM).

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-005.

**Détails techniques** : bibliothèque ClosedXML ou EPPlus pour Excel.

---

### F-022 — Rapport PDF de fiche entreprise

**Description** : génération d'un PDF imprimable rassemblant les informations principales d'une entreprise + historique des modifications + bilans récents.

**Valeur user** : documentation client pour les cabinets, présentation à un comité d'investissement.

**Complexité** : ★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-004, F-013.

**Détails techniques** : QuestPDF (excellente lib C# moderne et performante pour la génération PDF, sous licence MIT pour usage non-commercial).

---

## Cluster Veille (intégré au MVP 2)

> **Décision stratégique structurante** (cf. ADR-009) : la veille devient une **feature majeure et différenciante** du produit, exploitant le trou de marché identifié (aucun concurrent ne combine données entreprises et veille agrégée).
> Le cluster F-041 à F-050 forme un bloc cohérent à livrer ensemble en MVP 2 pour offrir une expérience complète dès le lancement de la veille.

### F-041 — Moteur d'agrégation RSS / Atom

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Contexte **Veille** : port `IExternalContentSource`, entités `FeedSource`/`FeedItem` (hash URL+titre pour la dédup), adapter `RssFeedProvider` (CodeHollow.FeedReader, lecture via `HttpClient` typé + parsing défensif). Polling récurrent via **Hangfire** (storage PostgreSQL, cron 30 min, désactivable par `BackgroundJobs:Enabled`), use case `PollFeedSourcesCommand` (résilient par source), endpoint `GET /feed/items`. Sources système amorcées au démarrage (cf. doc 07). Validé contre de vrais flux. **Reste** (autres features du cluster) : templates/abonnements (F-042/043), timeline enrichie (F-044), dédup intelligente (F-045).

**Description** : moteur backend capable de poller des flux RSS et Atom à intervalle régulier, de parser leurs items, et de les stocker en BDD pour exposition aux utilisateurs.

**Valeur user** : fondation technique de toute la feature veille. Invisible mais critique.

**Complexité** : ★★★★

**APIs externes** : sources RSS/Atom variées (cf. doc 07).

**Dépendances** : F-001.

**Détails techniques** :
- Bibliothèque : `CodeHollow.FeedReader` ou `System.ServiceModel.Syndication` natif .NET
- Port métier `IExternalContentSource` côté domaine (Core)
- Adapter `RssFeedSource` côté infrastructure
- Job de polling via Hangfire ou MassTransit (intervalle configurable, par défaut 30 min)
- Déduplication par hash de l'URL + titre
- Gestion des erreurs : flux mort, parsing échec, timeout

### F-042 — Catalogue de templates de veille par métier

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Entité `VeillePack` versionnée (champ `Version` incrémenté à chaque évolution), `IVeillePackRepository`, enrôlement / désenrôlement utilisateur, re-sync au upgrade via `VeillePackEnrollment.Version`. Sources système rattachées au pack ; **les sources d'un pack ignorent la limite par utilisateur** des sources libres (F-043). Endpoints `/veille/packs/*`. PR #19.

**Description** : bibliothèque de "Packs de veille" pré-curés. À l'inscription ou plus tard, l'utilisateur choisit un ou plusieurs templates correspondant à son métier (Cabinet PI, Expert-comptable, Compliance, Investisseur, Veille concurrentielle B2B, etc.). Les sources sont automatiquement abonnées.

**Valeur user** : friction zéro à l'onboarding. Un user obtient une veille pertinente en 30 secondes au lieu de configurer 2 heures.

**Complexité** : ★★★

**APIs externes** : aucune (sources internes au catalogue).

**Dépendances** : F-041.

**Détails techniques** :
- Catalogue persisté en BDD avec structure `Template > Sources > FeedDetails`
- Constitué initialement à partir du document `07-flux-rss-veille.md` (cercles de pertinence)
- Versionné (un template peut évoluer, les utilisateurs choisissent s'ils synchronisent)

### F-043 — Ajout libre de sources par l'utilisateur

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Entité `VeilleSubscription`, port `IFeedSubscriptionPolicy` (limite par utilisateur configurable + blocklist d'hôtes + anti-SSRF Lot 2a), endpoints `POST /veille/subscriptions`, `DELETE /veille/subscriptions/{id}`, `GET /veille/subscriptions`. PR #18.

**Description** : l'utilisateur peut ajouter manuellement n'importe quel flux RSS / Atom. Le système valide la source (parsing test) et l'intègre à son abonnement.

**Valeur user** : indispensable pour les power users et les cas particuliers non couverts par les templates.

**Complexité** : ★★

**APIs externes** : la source RSS fournie par l'utilisateur.

**Dépendances** : F-041.

**Détails techniques** :
- Validation : test de fetch, parsing, vérification taille raisonnable
- Modération : bibliothèque d'URLs interdites (sources de spam, contenu manifestement illégal)
- Limite de sources par compte : configurable selon le plan (illimité en self-hosted, limite en hébergé selon offre)

### F-044 — Timeline unifiée de la veille

> **Statut** : ✅ Backend implémenté (MVP 2, 27 mai 2026). Timeline par utilisateur agrégeant abonnements libres (F-043) et packs (F-042), états par item (`FeedItemUserState` : lu / non lu / favori / archivé), filtrage par source/date/mot-clé, pagination. Endpoints `/timeline/*`. **Reste** : UI MAUI (cf. doc 06 §UI veille). PR #20.

**Description** : vue principale de la feature : une timeline chronologique inversée affichant tous les items des sources abonnées de l'utilisateur, avec filtres et tri.

**Valeur user** : l'interface où le user passe son temps. Le "Twitter de sa veille".

**Complexité** : ★★★★

**APIs externes** : aucune (consomme F-041).

**Dépendances** : F-041, F-042, F-043.

**Détails techniques** :
- UI MAUI accessible (cf. doc 06 — section UI veille)
- Pagination infinie ou par "load more"
- Marquer comme lu/non lu, favori, archivé
- Filtres : par source, par date, par mot-clé

### F-045 — Déduplication intelligente des items

> **Statut** : ✅ Implémenté (MVP 2, 28 mai 2026). Empreinte **SimHash 64 bits** (FNV-1a) calculée à l'ingestion, regroupement en `FeedItemCluster` par distance de Hamming (seuil configurable), collapse dans la timeline (un seul représentant par cluster avec compteur de sources). PR #24.

**Description** : si la même information est publiée par plusieurs sources (ex. rachat d'une entreprise repris par 5 médias), regrouper ces items en un seul dans la timeline avec indication "5 sources rapportent".

**Valeur user** : réduit la fatigue informationnelle. Le user ne lit pas 5 fois la même chose.

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-044.

**Détails techniques** :
- Algorithmes de similarité : MinHash ou SimHash sur les titres + extrait
- Seuil de similarité configurable (par défaut 80%)
- Stockage en BDD du cluster d'items dédupliqués

### F-046 — Filtres et règles de surveillance personnalisées

**Description** : l'utilisateur configure des **filtres** sur sa timeline (par mot-clé, par entreprise favorite mentionnée, par source). Crée aussi des **règles** ("alerte-moi quand un nouvel item mentionne X").

**Valeur user** : transformer la veille passive en veille active et ciblée.

**Complexité** : ★★★

**APIs externes** : aucune.

**Dépendances** : F-044.

**Détails techniques** :
- Filtres : moteur de search-as-you-type sur les items en cache
- Règles : structure { critère, action } persistée
- Action : notification email, push, ou ajout à une liste dédiée

### F-047 — Combinaison veille + favoris entreprises

> **Statut** : ✅ MVP intégral livré (3 volets, MVP 2, 29 mai 2026). **Volet 1 — tagging RSS → favoris** : à l'ingestion d'un item RSS, scan titre + résumé pour les noms d'entreprises favorites (matching mot entier case-insensitive, ≥ 3 caractères). Persisté dans `FeedItemFavoriteMatch`. Timeline étendue avec `MentionedFavorites` et filtre `?mentionsFavoritesOnly=true`. **Volet 2 — événements RNE** : `FavoriteEvent` (entité + repo) créée par un 3ᵉ handler MediatR sur `CompanyFavoriteChangedNotification` (F-019). DTO timeline discriminé `Kind = "RssItem" | "FavoriteEvent"` avec fusion mémoire bornée. **Volet 3 — BODACC (cf. F-048)** : annonces légales pour les SIREN favoris, dédupliquées via `FavoriteEvent.ExternalId`. La timeline mixte est désormais opérationnelle : RSS + RNE + BODACC, tout au même endroit, trié chronologiquement.

**Description** : feature **différenciante phare** : la timeline affiche **aussi** les évolutions des entreprises favorites du user (mises à jour RNE, dépôts BODACC, articles RSS mentionnant le nom de l'entreprise). Tout au même endroit.

**Valeur user** : c'est LE feature qui justifie le projet face à un Feedly ou un Pappers seuls. **L'argument commercial central de la veille.**

**Complexité** : ★★★★

**APIs externes** : INPI RNE, BODACC.

**Dépendances** : F-017 (favoris), F-019 (alertes), F-041, F-044.

**Détails techniques** :
- Polling des favoris en parallèle des sources RSS
- Détection automatique : un item RSS mentionne le nom d'une entreprise favorite → tagging automatique
- Timeline mixte triée chronologiquement

### F-048 — Intégration BODACC dans la veille

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Nouveau projet `Atlas.Infrastructure.Bodacc` avec adapter `OpendatasoftBodaccProvider` interrogeant l'API publique `bodacc-datadila.opendatasoft.com` (anonyme, pas d'INPI requis). Job Hangfire `bodacc-polling` cron `0 4 * * *` (après F-019), **déduplication cross-users** (1 appel API par SIREN partagé). Chaque annonce non encore connue → `FavoriteEvent` type `BodaccPublished` avec `ExternalId = AnnouncementId BODACC`. Index unique partiel `(user_id, external_id) WHERE external_id IS NOT NULL` pour dédup efficace. Apparaît dans la timeline via la fusion F-047 volet 2. **Reste** : (a) configuration fine côté user (mots-clés, secteurs, types d'annonces) — actuellement toutes les annonces sont remontées, (b) validation contre l'API réelle (schéma documenté mais non testé en prod).

**Description** : intégration du BODACC comme source de veille parmi d'autres, avec configuration fine (mots-clés, secteurs d'activité, tribunaux, types d'annonces).

**Valeur user** : alertes temps réel sur des événements légaux significatifs (RJ d'un client, vente d'un fonds dans son secteur, etc.).

**Complexité** : ★★★

**APIs externes** : BODACC (data.gouv.fr / Opendatasoft).

**Dépendances** : F-041.

**Détails techniques** :
- BODACC expose des flux RSS personnalisés via Opendatasoft
- Possibilité de wrap dans un adapter `BodaccSource` implémentant `IExternalContentSource`

### F-049 — Marketplace des templates partagés (V2 light, posée en MVP 2)

**Description** : les utilisateurs peuvent partager leurs templates de veille curés. D'autres users peuvent les adopter en un clic.

**Valeur user** : effet réseau, capitalisation collective, croissance organique. Les pros aiment partager leur expertise.

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-042.

**Détails techniques** :
- Templates "publics" exposés sur une page dédiée
- Système de votes / favoris communautaires
- Modération : modèle "report & review", pas de pré-modération sauf signal
- Avantage stratégique : crée du contenu généré par les users

### F-050 — Préparation à la couche premium (architecture)

**Description** : pas une feature visible côté user, mais une **décision d'architecture** à respecter dès la phase de design du cluster veille : tous les use cases d'enrichissement (futurs résumés IA, scoring IA, synthèse hebdo) sont définis comme des **ports séparés** dans le domaine, implémentés en projet `.Premium` distinct (vide en MVP 2).

**Valeur user** : aucune en MVP 2. Préparation à la phase de monétisation pour ne pas refondre.

**Complexité** : ★★ (discipline d'architecture pure)

**APIs externes** : aucune.

**Dépendances** : architecture hexagonale globale (ADR-004).

**Détails techniques** :
- Projet `<Projet>.Application.Premium` créé vide ou avec stubs
- Ports `IFeedItemEnricher`, `IFeedRelevanceScorer`, `IFeedSummarizer` définis côté domaine
- Aucune implémentation en MVP 2 (les use cases premium ne sont pas appelés)
- L'ajout futur d'adapters LLM ne nécessitera **aucune modification du domaine**

---

## V2 — Could have (mois 8–18)

**Objectif** : différenciation par rapport aux concurrents (Pappers, Societe.com). C'est ici qu'on construit les "killer features" qui rendent le projet unique.

### F-023 — Intégration BODACC (annonces légales)

**Description** : surveillance des annonces légales (créations, modifications, RJ, LJ, ventes de fonds) en temps réel pour les entreprises favorites de l'utilisateur.

**Valeur user** : alerte précoce sur une procédure collective d'un partenaire commercial, détection d'opportunités (vente de fonds dans son secteur).

**Complexité** : ★★★★

**APIs externes** : BODACC (data.gouv.fr).

**Dépendances** : F-017.

---

### F-024 — Intégration Sirene (INSEE) pour les établissements

**Description** : enrichissement des fiches entreprises avec la liste détaillée des établissements (siège + secondaires) issue du répertoire Sirene.

**Valeur user** : vision géographique d'une entreprise multi-sites.

**Complexité** : ★★★

**APIs externes** : Sirene (api.insee.fr).

**Dépendances** : F-004.

---

### F-025 — Tableau de bord portefeuille IP

**Description** : vue agrégée des marques et brevets suivis par l'utilisateur, avec calendrier des renouvellements à venir, statut de chaque titre, alertes de risque (marques expirantes, oppositions en cours).

**Valeur user** : remplace les tableaux Excel des cabinets PI pour gérer le portefeuille de leurs clients.

**Complexité** : ★★★★★

**APIs externes** : INPI PI.

**Dépendances** : F-018.

---

### F-026 — Recherche d'antériorité marque avec matching intelligent

**Description** : recherche d'antériorité avec matching phonétique (Soundex, Metaphone) et sémantique pour détecter les marques proches d'un terme cible, pas seulement les correspondances exactes.

**Valeur user** : la vérification d'antériorité est un acte juridique précieux que les cabinets facturent. Un outil qui automatise une partie de cette analyse a une vraie valeur.

**Complexité** : ★★★★★

**APIs externes** : INPI PI + EUIPO + OMPI.

**Dépendances** : F-006.

---

### F-027 — Veille concurrentielle automatisée

**Description** : l'utilisateur configure des règles ("alerte-moi à chaque nouveau dépôt de marque par mon concurrent X, ou contenant le mot Y, dans la classe Z"). Notifications email/push.

**Valeur user** : surveillance proactive impossible à faire manuellement à grande échelle.

**Complexité** : ★★★★

**APIs externes** : INPI PI + EUIPO + OMPI.

**Dépendances** : F-018, F-020.

---

### F-028 — API publique du projet

**Description** : exposition d'une API REST publique permettant à des tiers (devs, intégrations) d'utiliser les fonctions du projet via clé API.

**Valeur user** : intégration avec des CRMs, outils internes, scripts d'automatisation.

**Complexité** : ★★★★

**APIs externes** : aucune (exposition de l'existant).

**Dépendances** : MVP 1 complet.

**Détails techniques** : gestion des clés API, quotas, documentation Swagger / OpenAPI, rate limiting.

---

### F-029 — Mode offline mobile avec sync

**Description** : sur l'app MAUI mobile, possibilité de consulter ses favoris et son historique en mode hors-ligne, avec synchronisation automatique au retour de la connectivité.

**Valeur user** : usage en déplacement (transports, zones rurales).

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-009, F-017.

**Détails techniques** : SQLite local + stratégie de réplication.

---

### F-030 — Annotations et tags utilisateur

**Description** : l'utilisateur ajoute des notes privées et des tags sur les fiches qu'il consulte.

**Valeur user** : capitalisation de la connaissance personnelle au-dessus des données publiques.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-017.

---

## V3+ — Won't have (yet)

Features identifiées comme valables mais explicitement reportées hors du périmètre actuel. À reconsidérer en fonction de la traction.

### F-031 — Intégration Judilibre (décisions de justice)

**Description** : recherche de décisions judiciaires liées à une entreprise (contentieux), à une marque (oppositions, contrefaçon), à un brevet (litiges).

**Pourquoi reporté** : l'API Judilibre est mature mais le matching entre entités (le défendeur dans une décision est-il bien "mon" entreprise ?) est non-trivial.

**APIs externes** : Judilibre (api.piste.gouv.fr).

---

### F-032 — Intégration DECP (commande publique)

**Description** : pour une entreprise donnée, afficher les marchés publics qu'elle a gagnés.

**Pourquoi reporté** : très spécifique (utile uniquement pour certains segments comme BTP, consulting, IT public).

**APIs externes** : DECP (data.gouv.fr).

---

### F-033 — Score ESG / Bilan carbone

**Description** : intégration du bilan GES (gaz à effet de serre) d'une entreprise quand disponible publiquement.

**Pourquoi reporté** : niche, peu d'entreprises publient ces données aujourd'hui. À revoir quand la CSRD sera pleinement déployée (2025–2028).

**APIs externes** : ADEME Bilan GES.

---

### F-034 — Graphe relationnel des dirigeants

**Description** : visualisation en graphe des dirigeants d'une entreprise et de leurs autres mandats. Détection de structures complexes (sociétés écrans, conflits d'intérêt).

**Pourquoi reporté** : très puissant mais complexe à construire (BDD graphe type Neo4j, algos de pathfinding, UX dédiée).

**APIs externes** : INPI RNE (mais agrégation massive nécessaire).

---

### F-035 — Mode collaboratif / espace équipe

**Description** : un utilisateur peut inviter des collègues dans un espace partagé, avec rôles et permissions.

**Pourquoi reporté** : passe le produit de B2C/B2D à B2B "vrai". Augmente massivement la complexité (multi-tenant côté users, billing par seat, etc.).

**APIs externes** : aucune.

---

### F-036 — Plugin / extension navigateur

**Description** : extension Chrome / Firefox qui détecte un SIREN sur n'importe quelle page web et propose un aperçu rapide via le projet.

**Pourquoi reporté** : valeur élevée pour les power users, mais demande un développement spécifique (manifest, build, store reviews).

**APIs externes** : utilise le backend du projet.

---

### F-037 — Connecteurs CRM (Salesforce, HubSpot, Pipedrive)

**Description** : enrichissement automatique des comptes dans un CRM tiers avec les données INPI/RNE.

**Pourquoi reporté** : forte valeur B2B mais demande un effort important par intégration. À envisager en partenariat ou via Zapier/Make.

**APIs externes** : APIs des CRMs cibles.

---

### F-038 — Webhooks pour les alertes

**Description** : au lieu de simples emails, les alertes peuvent être envoyées en POST HTTP vers une URL de l'utilisateur (intégration Slack, Teams, automations).

**Pourquoi reporté** : utile pour les power users / devs, mais cas d'usage marginal en phase 1-2.

**APIs externes** : aucune.

---

### F-039 — Couverture européenne (EUIPO, OMPI)

**Description** : recherche marques et dessins au niveau européen et international.

**Pourquoi reporté** : étape logique après stabilisation du périmètre français.

**APIs externes** : EUIPO, OMPI.

---

### F-040 — Couverture brevets mondiale (OEB Espacenet)

**Description** : extension recherche brevets au niveau européen et mondial via OPS.

**Pourquoi reporté** : idem F-039.

**APIs externes** : OEB Espacenet (epo.org).

---

## Features explicitement écartées

Pour mémoire, certaines pistes ont été évaluées et **explicitement écartées** :

| Feature écartée | Raison |
|---|---|
| Données des Bénéficiaires Effectifs (BE) | Restriction d'accès depuis l'arrêt CJUE Sovim (nov. 2022). Nécessite un statut "personne habilitée" ou la démonstration d'un intérêt légitime. Risque juridique trop élevé pour un MVP. |
| Données fiscales détaillées DGFIP | Non disponibles en open data au niveau entreprise individuelle. |
| Données URSSAF / cotisations sociales | Couvertes par le secret professionnel. |
| Données médicales (cliniques, médecins) | Hors périmètre métier, et soumis au secret médical pour partie. |
| Données bancaires (Banque de France) | API restreintes aux acteurs financiers agréés. |
| Données LinkedIn / réseaux sociaux | Pas d'API officielle ouverte, ToS très restrictives. Scraping = risque juridique. |

---

*Document évolutif. Toute nouvelle feature doit être ajoutée avec son identifiant unique (F-NNN), sa description complète selon le template, et sa catégorie MoSCoW.*
