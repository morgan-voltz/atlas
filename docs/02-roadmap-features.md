# Roadmap features

> Catalogue détaillé des fonctionnalités du projet, organisé selon la méthode **MoSCoW** (Must / Should / Could / Won't have for now) et par jalon de version.
> Chaque feature est décrite avec sa valeur utilisateur, sa complexité technique, les APIs externes requises et les dépendances vers d'autres features.
>
> **Ce document est le backlog produit (`F-NNN`).** L'implémentation du **client web** suit sa propre feuille de route, à part : `docs/15-roadmap-client-web.md` (`W-NNN`).

**Version** : 1.0
**Date de dernière mise à jour** : 31 mai 2026

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

## État d'avancement — MVP 2

Légende identique au tableau MVP 1.

**Section principale** (F-013 → F-022) :

| Feature | Statut |
|---|:--:|
| F-013 Téléchargement individuel d'actes et bilans | 🟡 |
| F-014 Téléchargement en masse de documents | ✅ |
| F-015 Recherche brevet par numéro | ✅ |
| F-016 Recherche brevet avancée (titre / inventeur / déposant) | 🟡 |
| F-017 Favoris : suivi d'une entreprise | ✅ |
| F-018 Favoris : suivi d'une marque ou d'un brevet | ✅ |
| F-019 Alerte email sur modification d'une entreprise favorite | ✅ |
| F-020 Notifications push mobiles (et desktop) | ✅ |
| F-021 Export CSV / Excel | 🟡 |
| F-022 Rapport PDF de fiche entreprise | ✅ |

**Cluster Veille** (F-041 → F-050) :

| Feature | Statut |
|---|:--:|
| F-041 Moteur d'agrégation RSS / Atom | ✅ |
| F-042 Catalogue de templates (VeillePack) | ✅ |
| F-043 Ajout libre de sources | ✅ |
| F-044 Timeline unifiée | ✅ |
| F-045 Déduplication intelligente (SimHash) | ✅ |
| F-046 Filtres et règles de surveillance | ✅ |
| F-047 Combinaison veille + favoris (3 volets : RSS, RNE, BODACC) | ✅ |
| F-048 Intégration BODACC | ✅ |
| F-049 Marketplace des templates partagés | ✅ |
| F-050 Préparation à la couche premium (architecture) | ✅ |

Les 🟡 correspondent surtout à : confirmation contre l'API INPI réelle (F-013, F-016) et
export XLSX et étendue de couverture (F-021). **Le cluster veille est complet côté backend.**
UI MAUI restante pour F-017, F-020, F-044, F-046, F-049 (chantier client cross-cutting,
suivi côté F-009/F-010 du MVP 1).

---

## Sommaire

- [Méthodologie](#méthodologie)
- [Conventions](#conventions)
- [MVP 1 — Must have (mois 1–4)](#mvp-1--must-have-mois-1-4)
- [MVP 2 — Should have (mois 4–8)](#mvp-2--should-have-mois-4-8)
- [Cluster Veille (intégré au MVP 2)](#cluster-veille-intégré-au-mvp-2)
- [Reste à faire pour clore MVP 2](#reste-à-faire-pour-clore-mvp-2)
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

> **Doctrine UX** : F-009 et F-010 suivent le **modèle UX adaptatif** posé par `docs/12-modele-ux-client-maui.md` — **un seul modèle mental, deux densités** (R1), adaptation à la **largeur** disponible et non à la plateforme (R2), **list-detail** comme épine dorsale récursive (R3), **5 destinations** plafonnées (Accueil / Recherche / Veille / Favoris / Profil). Toute nouvelle vue MAUI doit s'y conformer.

**Description** : application mobile native (Android et iOS via MAUI) permettant les fonctions F-004 à F-008 dans une UX adaptée mobile.

**Valeur user** : un utilisateur peut consulter une fiche entreprise depuis son téléphone en rendez-vous. Valeur métier énorme pour la cible "expert-comptable / avocat en déplacement".

**Complexité** : ★★★★

**APIs externes** : utilise le backend du projet.

**Dépendances** : F-001 à F-008.

**Détails techniques** :
- Authentification via JWT stocké dans le secure storage de la plateforme (Keychain iOS, Keystore Android).
- Pas de credentials INPI sur le device — uniquement le token de session.
- UI native MAUI (Shell + ContentPage) — navigation et adaptation à la largeur conformes à `docs/12`.

---

### F-010 — Client MAUI desktop (Windows/macOS)

> **Statut** : 🟡 Amorcé (MVP 1, 27 mai 2026). Le client desktop est la **même app MAUI** (projet unique multi-cible) que F-009 : tout le code (ViewModels, `AtlasApiClient`, écrans) est partagé. **Compilé Windows** (`net10.0-windows`) — a nécessité de passer les `[ObservableProperty]` en propriétés partielles (compatibilité WinRT, MVVMTK0045). Adaptation grand écran : fenêtre dimensionnée (1100×800, min 800×600) sur desktop. **Reste** : adaptations UX écran large plus poussées (panneaux multiples, raccourcis clavier), **packaging MSIX (Windows) / PKG (macOS)** et **build macCatalyst** (machine macOS requise), QA sur cible réelle.
> **Doctrine UX** : `docs/12-modele-ux-client-maui.md` impose **R4 — desktop = densité & clavier** : information visible d'un coup, raccourcis clavier, états au survol. L'adaptation est gouvernée par la **largeur** (R2, `VisualStateManager` + `AdaptiveTrigger`) et non par la plateforme — un mobile en paysage, une fenêtre desktop réduite ou un split-screen sont traités par la même mécanique.

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

> **Statut** : 🟡 Backend implémenté (MVP 2, 29 mai 2026). Extension `ICompanyDataProvider` avec `GetAttachmentsAsync` + `DownloadAttachmentAsync` (auth Bearer + retry 401 unifié dans `ExecuteAsync<T>` du `RneCompanyProvider`). Endpoints `GET /companies/{siren}/attachments` et `/attachments/{id}/download` (proxy binaire, `Results.Stream`). Entité `CompanyAttachment` (Id, Type enum `Acte`/`Bilan`/`Other`, Name, DepositedAt?, SizeBytes?, IsConfidential). Mapping JSON **défensif** : accepte tableau direct OU objet avec sous-collections `actes`/`comptesAnnuels`/`bilans`. Bilans confidentiels (champ `confidentialite`) → 403 (`companies.attachment_confidential`). **Reste** : confirmation par un **appel authentifié réel** (compte INPI requis) sur la structure JSON exacte renvoyée par l'INPI ; ajouter Bruno coverage pour les 2 endpoints.

**Description** : depuis la fiche d'une entreprise, l'utilisateur voit la liste des actes (statuts, modifications) et bilans déposés, et peut télécharger chaque document en PDF.

**Valeur user** : usage central pour les experts-comptables et avocats — accéder rapidement aux documents juridiques d'une boîte.

**Complexité** : ★★

**APIs externes** : INPI RNE (`/companies/{siren}/attachments` + endpoint de téléchargement).

**Dépendances** : F-004.

---

### F-014 — Téléchargement en masse de documents — ✅ MVP 2 (29 mai 2026)

**Description** : l'utilisateur sélectionne plusieurs entreprises (via une liste de SIREN, jusqu'à 50) et lance un téléchargement groupé de tous leurs bilans/actes, livré sous forme d'archive ZIP avec une arborescence par SIREN.

**Valeur user** : automatisation d'une tâche fastidieuse pour les cabinets traitant des dizaines d'entreprises.

**Complexité** : ★★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-013.

**Implémentation** :
- Endpoints `POST /downloads/bulk`, `GET /downloads/bulk/{id}`, `GET /downloads/bulk/{id}/archive`.
- Job Hangfire `BulkDownloadJob.RunAsync(jobId)` enfilé après la création du job en base. Idempotent : si le job est déjà finalisé, no-op.
- Limite : 50 SIREN par job, validés en amont (Luhn) avant enqueue.
- Stockage temporaire via port `IFileStorage` (adapter `LocalFileStorage` filesystem ; clé `bulk/{jobId:N}.zip`).
- TTL d'archive : 24h. Status renvoie `410 Gone` après expiration.
- Documents confidentiels filtrés à la source (best-effort par SIREN : un échec n'arrête pas les autres).
- Suivi par polling client (l'envoi push/email est repoussé à un lot ultérieur).

**Reste à traiter dans un lot futur** :
- Notification push/email à la finalisation du job.
- Job récurrent de purge des archives expirées + entités `BulkDownloadJob`.
- Adapter S3 (MinIO, Wasabi, AWS) pour la prod.
- Rate limiting INPI dédié aux téléchargements (aujourd'hui : limites par défaut du `RneCompanyProvider`).

---

### F-015 — Recherche brevet par numéro

> **Statut** : ✅ Backend implémenté (MVP 2, commit `85ae014`). Endpoint `GET /patents/{publicationNumber}` qui interroge l'INPI PI brevets via `IIntellectualPropertyProvider.GetPatentAsync`. Route paramétrée placée **après** la route de recherche (F-016) pour éviter la collision. Mapping best-effort `PiPatentMapper.MapDetail`. **Reste** : confirmation contre l'API INPI réelle (structure JSON exacte) et couverture Bruno.

**Description** : recherche d'un brevet par son numéro de publication (FR, EP, WO), affichage de la notice + image d'abrégé.

**Valeur user** : couverture du périmètre PI au-delà des marques.

**Complexité** : ★★

**APIs externes** : INPI PI brevets.

**Dépendances** : F-003.

---

### F-016 — Recherche brevet par titre / inventeur / déposant

> **Statut** : 🟡 Backend implémenté (MVP 2, 29 mai 2026). `PatentSearchQuery(Title?, Inventor?, Applicant?, Page, PageSize)` + `PatentSummary` + `IIntellectualPropertyProvider.SearchPatentsAsync`. Endpoint `GET /patents?title=&inventor=&applicant=&page=&pageSize=` (route précédant celle paramétrée pour éviter collision avec F-015). Validation : ≥ 1 critère renseigné → 400 `patents.empty_search` sinon. Pagination clampée. Adapter via `POST /services/apidiffusion/api/brevets/search`. Mapping best-effort `PiPatentMapper.MapSummary`. **Reste** : confirmation contre l'API INPI réelle (syntaxe SolR exacte, structure de la réponse paginée). PR #40.

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

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Entités `TrademarkFavorite` (UserId, DepositNumber, NameSnapshot?, AddedAt) et `PatentFavorite` (UserId, PublicationNumber, TitleSnapshot?, AddedAt) avec ports repositories + erreurs métier. 6 use cases Application (Add/Remove/GetMine × 2). 6 endpoints `POST/DELETE/GET /favorites/trademarks` et `/favorites/patents` sous le même groupe `/favorites/*`. Persistence : tables `trademark_favorites` et `patent_favorites` (index unique `(user, dépôt/publication)`, cascade FK RGPD). Tests handlers (10) + cascade RGPD étendue. PR #41.

**Description** : équivalent F-017 pour la PI.

**Valeur user** : suivi d'un portefeuille IP.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-006, F-015.

---

### F-019 — Alerte email sur modification d'une entreprise favorite

> **Statut** : ✅ Backend implémenté + **adapter Brevo livré (29 mai 2026)**. `CompanyFavoriteSnapshot` (un cliché vivant par `(UserId, SIREN)`) + `DiffWith(UniteLegale)` détecte les changements de dénomination / forme juridique / NAF / adresse / dirigeants (hash). Job Hangfire `favorite-refresh` cron `0 3 * * *` (désactivable). Notification MediatR `CompanyFavoriteChangedNotification` → 2 handlers : email (via `IEmailSender.SendFavoriteChangeAsync`) et push (via `INotificationDispatcher`, cf. F-020). Users sans compte INPI connecté passés silencieusement. **Adapter email Brevo** (provider France RGPD-compliant) : `BrevoEmailSender` (HttpClient typé, `POST /v3/smtp/email` avec header `api-key`) avec template HTML + texte, switch DI sur `Email:Brevo:ApiKey` (sans clé → fallback `LoggingEmailSender` dev). Reste : templates email enrichis (logo, branding).

> **Architecture liée** : F-019 est la **première implémentation** du patron formalisé par **ADR-013** (substrat de surveillance — stratégie `IStateMonitor<UniteLegale>` : retraits détectés, état comparé). F-048 (BODACC) est la première instance du patron `IItemStreamMonitor<TItem>` (flux append-only). L'extraction effective du runner mutualisé est planifiée pour F-057 (3ᵉ instance).

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

> **Statut** : ✅ Backend complet (29 mai 2026), client MAUI restant. **Toutes les plateformes push** sont livrées : **FCM** (Android + Web Push) JWT RS256, **APNs** (iOS + macOS) JWT ES256 HTTP/2, **WNS** (Windows desktop) OAuth2 `client_credentials` + Toast XML. Architecture : interface interne `IPlatformPushDispatcher` + `CompositeNotificationDispatcher` qui fan-out vers tous les adapters configurés en parallèle, avec isolation des erreurs. Cleanup auto des tokens morts (FCM : 404 / UNREGISTERED ; APNs : 410 / BadDeviceToken ; WNS : 410 / 404). Bascule DI : ≥ 1 plateforme configurée → composite ; aucune → `LoggingNotificationDispatcher` (dev). Ports + endpoints (28 mai) : entité `DeviceRegistration`, port `INotificationDispatcher`, API `POST/DELETE/GET /devices`. **Reste** : client MAUI (récupération du token natif par plateforme + `POST /devices` au démarrage).

**Description** : alertes envoyées en push sur l'app mobile MAUI en complément des emails. Étendu pour couvrir aussi le desktop (Windows / macOS).

**Valeur user** : immédiateté de l'information.

**Complexité** : ★★★

**APIs externes** : Firebase Cloud Messaging (Android), APNs (iOS).

**Dépendances** : F-009, F-019.

---

### F-021 — Export CSV / Excel de résultats

> **Statut** : 🟡 MVP CSV livré (MVP 2, 29 mai 2026). Helper `Atlas.Application.Common.CsvWriter` (RFC 4180, UTF-8 + BOM, CRLF, échappement quotes/virgules/newlines). 3 endpoints d'export favoris : `GET /favorites/{companies,trademarks,patents}/export` → `text/csv` avec `Content-Disposition: attachment`. Pas de dépendance NuGet ajoutée. **Reste** : export XLSX (ClosedXML), export des résultats de recherche RNE/PI (paginé, à concevoir), export de la veille (timeline). PR #42.

**Description** : depuis une liste de résultats ou un panier, l'utilisateur exporte les données en CSV ou XLSX.

**Valeur user** : intégration avec les outils existants des utilisateurs (Excel, Google Sheets, CRM).

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-005.

**Détails techniques** : bibliothèque ClosedXML ou EPPlus pour Excel.

---

### F-022 — Rapport PDF de fiche entreprise

> **Statut** : ✅ MVP implémenté (MVP 2, 29 mai 2026). Endpoint `GET /companies/{siren}/report.pdf` → PDF A4 (identité, NAF, adresse, dirigeants, table actes/bilans). Powered by **QuestPDF community edition** (licence engagée au démarrage). `CompanyReportRenderer` dans `Atlas.Api/Reports/` (couche présentation, QuestPDF référencé uniquement par Atlas.Api). Attachments best-effort (PDF généré même si la liste échoue). 3 smoke tests dans `Atlas.Api.IntegrationTests` (signature `%PDF`, payloads minimal/complet/vide). PR #43. **Reste** : enrichissement (logo, historique des modifications via snapshot F-019, bilans intégrés via F-013 download).

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

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Entité `FeedRule` (UserId + critères AND : `KeywordPattern` / `SourceId` / `MentionedSiren` (Siren) + actions : `NotifyEmail` / `NotifyPush`), invariants `Create` / `Update` (au moins un critère, au moins une action, nom ≤ 200, keyword ≤ 200), `RegisterEvaluation` / `RegisterTrigger`. Port `IFeedRuleRepository`. 5 use cases MediatR (`CreateFeedRule`, `UpdateFeedRule`, `DeleteFeedRule`, `ListMyFeedRules`, `EvaluateFeedRules`). Notification `FeedRuleMatchedNotification` + 2 handlers (`SendFeedRuleMatchedEmailHandler`, `DispatchFeedRuleMatchedPushHandler`) calqués sur F-019, gating sur `NotifyEmail` / `NotifyPush` par règle. Extension `IEmailSender.SendFeedRuleMatchedAsync` + adapters logging / capturing. 4 endpoints `/feed/rules` (POST/GET/PATCH/DELETE). Job Hangfire : `EvaluateFeedRulesCommand` chaîné dans `FeedPollingJob.PollAsync()` après `MatchFavoritesInFeedItemsCommand` (F-047) — voit donc les `FeedItemFavoriteMatch` pour évaluer `MentionedSiren`. Évaluation incrémentale par watermark `LastEvaluatedAt`. Table `feed_rule` (cascade FK user RGPD). 248 tests verts (89 domain + 155 application + 4 archi), dont 17 sur `FeedRule` (invariants + matching AND + watermark), CreateFeedRule (4), DeleteFeedRule (3), EvaluateFeedRules (4 — flux complet match/no-match/watermark). Adapter email **Brevo** livré le 29 mai 2026 (commun avec F-019) ; le `BrevoEmailSender` envoie les notifications de règle matchée via `SendFeedRuleMatchedAsync`. Reste : UI MAUI (CRUD des règles), validateurs FluentValidation câblés dans le pipeline si nécessaire.

> **Architecture liée** : `FeedRule` est le **support d'activation** des `WatchRule` futures. **F-057** étendra cette mécanique pour activer la surveillance sanctions par watchlist (« surveillance sanctions » comme `WatchRule`), réutilisant le moteur d'évaluation existant.

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

> **Architecture liée** : le volet 1 (tagging RSS → favoris) est la **première implémentation** de `INameInTextMatcher` formalisé par **ADR-014** (matching conservateur unifié — la doctrine ADR-012 incarnée dans `MatchCandidate`). À l'arrivée de F-031 et F-055/F-057, le noyau de normalisation des noms (`ICompanyNameNormalizer`) sera extrait à partir du code d'ici.

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

> **Architecture liée** : F-048 est la **première implémentation** du patron `IItemStreamMonitor<BodaccAnnouncement>` formalisé par **ADR-013** (substrat de surveillance — flux append-only, dédup par `ExternalId`). Le patron polling + dédup cross-users inventé ici est généralisé par le runner mutualisé qui sera extrait au moment de F-057.

**Description** : intégration du BODACC comme source de veille parmi d'autres, avec configuration fine (mots-clés, secteurs d'activité, tribunaux, types d'annonces).

**Valeur user** : alertes temps réel sur des événements légaux significatifs (RJ d'un client, vente d'un fonds dans son secteur, etc.).

**Complexité** : ★★★

**APIs externes** : BODACC (data.gouv.fr / Opendatasoft).

**Dépendances** : F-041.

**Détails techniques** :
- BODACC expose des flux RSS personnalisés via Opendatasoft
- Possibilité de wrap dans un adapter `BodaccSource` implémentant `IExternalContentSource`

### F-049 — Marketplace des templates partagés (V2 light, posée en MVP 2)

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). `VeillePack` étendu avec `AuthorUserId : UserId?` + `Visibility` enum (`System` / `Private` / `Public`) + compteur dénormalisé `LikesCount`. Nouvelle factory `CreateUserPack` (initialement `Private`). Méthodes `Publish` / `Unpublish` / `IncrementLikes` / `DecrementLikes` (avec plancher à 0). 2 nouvelles entités : `VeillePackLike` (index unique `(UserId, VeillePackId)`, cascade FK RGPD) et `VeillePackReport` (Pending / ReviewedNoAction / ReviewedRemoved, raison ≤ 500). Ports `IVeillePackLikeRepository`, `IVeillePackReportRepository` + extensions `GetPublicMarketplaceAsync` (paginé, trié `LikesCount desc, CreatedAt desc`) et `GetByAuthorAsync` sur `IVeillePackRepository`. 8 use cases MediatR (CreateUserVeillePack, Publish, Unpublish, Like, Unlike, Report, ListPublicMarketplace, GetMyAuthoredPacks). 8 endpoints sous `/veille/packs/*` (POST `/user`, PATCH `/user/{code}/publish|unpublish`, POST/DELETE `/{code}/like`, POST `/{code}/report`, GET `/community`, GET `/mine/authored`). Migration `AddVeillePackMarketplace` (3 colonnes sur `veille_pack` avec normalisation `System` des packs existants + 2 nouvelles tables, cascades FK RGPD). Modèle *report &amp; review* (modération a posteriori, pas de pré-modération). 21 tests (11 domaine + 10 handlers : Create / Like / Publish). **Reste** : UI MAUI (CRUD pack user + browse community + bouton like / report), workflow admin de revue des reports (endpoint et UI), recommandations / search facetté.

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

> **Statut** : ✅ Implémenté (MVP 2, 29 mai 2026). Les 3 ports premium sont déclarés côté domaine dans `Atlas.Domain.Veille.Premium` : `IFeedItemEnricher` (+ `FeedItemEnrichment` record), `IFeedRelevanceScorer` (score 0-100 par item × user), `IFeedSummarizer` (synthèse narrative d'un batch). Projet `Atlas.Application.Premium` matérialisé via `AssemblyMarker` public pour permettre aux tests d'architecture de cibler son assembly. 3 nouveaux tests dans `Atlas.Architecture.Tests` verrouillant la séparation : (a) `Atlas.Application` (cœur) ne référence pas `Atlas.Application.Premium`, (b) `Atlas.Domain` ne référence pas `Atlas.Application.Premium`, (c) `Atlas.Application.Premium` ne référence pas `Atlas.Infrastructure.*` ni `Atlas.Api`. Total : 7 tests d'archi verts (4 existants + 3 F-050). Aucune implémentation des ports en MVP 2 (cœur open source intact) ; les adapters viendront dans des projets `Atlas.Infrastructure.*Premium` dédiés. Patron réutilisable pour `IFinancialSummarizer` (F-054).

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

## Reste à faire pour clore MVP 2

> **Synthèse au 29 mai 2026** — extraite des blocs « Statut » des fiches ci-dessus. Tient lieu de punch-list MVP 2.

**🔴 Bloquant — vrais trous restants** (à livrer pour annoncer MVP 2 « fini »)

1. ~~**F-046 — Filtres et règles de surveillance personnalisées**~~ ✅ **Livré 29 mai 2026** (backend). Reste UI MAUI et adapter Brevo (commun avec F-019).
2. ~~**F-049 — Marketplace des templates partagés**~~ ✅ **Livré 29 mai 2026** (backend, périmètre V2 light gardé en MVP 2). Reste UI MAUI (création / browse / like / report) et workflow admin de revue des reports.
3. ~~**F-050 — Vérification de l'architecture premium**~~ ✅ **Livré 29 mai 2026** : 3 ports déclarés dans `Atlas.Domain.Veille.Premium` + 3 tests d'archi verrouillant la séparation cœur / premium (cf. fiche F-050).

**🟡 Important non-bloquant** (peut basculer en post-MVP 2 sans casser la promesse)

- ~~**Adapter email Brevo (F-019)**~~ ✅ **Livré 29 mai 2026** : `BrevoEmailSender` envoie les emails de F-001 / F-019 / F-046 via l'API Brevo (`POST /v3/smtp/email`), switch DI sur `Email:Brevo:ApiKey`. Sans clé renseignée, fallback `LoggingEmailSender` (mode dev). 6 tests unitaires (payload + erreurs).
- **Trame UI MAUI cross-cutting** : F-017 (onglet « Mes favoris »), F-020 (récupération du token push natif + `POST /devices`), F-044 (vue timeline veille). Backend livré pour les 3 ; côté MAUI c'est le gros chantier client restant (suivi en parallèle dans F-009/F-010 du MVP 1, encore 🟡).
- **Validation contre l'API INPI réelle** : F-013 (auth Bearer + structure JSON attachments), F-015 (structure JSON détail brevet), F-016 (syntaxe SolR + réponse paginée). Tous documentés mais non confirmés en prod. Idéalement levés via un compte INPI réel et une session Bruno.
- **F-014 — Lot ultérieur** : notification fin de job (push/email), job récurrent de purge des archives expirées + entité `BulkDownloadJob`, adapter `S3FileStorage` (MinIO/Wasabi/AWS), rate limiting INPI dédié aux téléchargements.
- **F-021 — Étendue d'export** : ClosedXML pour XLSX, export des résultats de recherche RNE/PI (paginé), export de la veille (timeline).
- **F-022 — Enrichissement PDF** : logo, historique des modifications via snapshots F-019, bilans intégrés via F-013 download.
- **F-048 — Configuration fine user** : aujourd'hui toutes les annonces BODACC sont remontées. Filtres par mots-clés / secteurs / types d'annonces à ajouter ; et validation contre l'API Opendatasoft réelle.

**🧭 Décisions à acter avant de fermer MVP 2**

- F-049 : dans MVP 2 ou bascule officielle en V2 ?
- Quels « 🟡 » assume-t-on en post-MVP 2 vs lesquels passe-t-on en ✅ avant de fermer ?
- Promotion des MVP 1 🟡 résiduels (F-004 à F-007 — confirmation INPI réelle, F-009/F-010 — exécution MAUI + QA, F-012 — contenu légal UI) : à boucler dans la même fenêtre que MVP 2 pour pouvoir parler de « MVP livré ».

---

## Campagne de tests e2e — 30 mai 2026

> Exécution locale de la collection Bruno versionnée (`bruno/`) contre l'API réelle
> (procédure : `docs/13-harness-test-local-e2e.md`).

**État** : le **cœur fonctionnel est vert** — flux complets d'inscription, 2FA TOTP,
RGPD (export / effacement), favoris (entreprise / marque / brevet), devices/push,
veille et règles de surveillance. Point de départ de la campagne : 86/138 requêtes
vertes, les écarts étant ensuite soit corrigés (ci-dessous), soit attribués aux
endpoints INPI (cf. « Validation contre l'API INPI réelle »).

**Corrections issues de la campagne**

- **F-014** — `GET /downloads/bulk/{id}/archive` renvoyait `500` (chemin de stockage
  `RootPath` non configuré écrasé à `null`) ; le job de fond plantait de même. Repli
  défensif sur le répertoire par défaut + tests de régression (#69).
- **F-001** — `GET /auth/verify-email` renvoyait `500` sur un `userId` vide ou mal
  formé (échec de binding du `Guid`). Binding permissif → `400` via le validator (#72).
- **Transversal API** — les ProblemDetails exposent désormais un champ `code` stable
  (RFC 9457) sur toutes les erreurs (métier et validation), au lieu d'un code dispersé
  entre `title`/`type`. Déduplique au passage l'invariant feed-rule (`veille.invalid_feed_rule`
  porté par le seul domaine). Contrat utile pour F-028 (#74).

**Outillage de test**

- Guide du harness e2e local ajouté (`docs/13-harness-test-local-e2e.md`, #71).
- Rate limit `auth-strict` relâché en `Development` pour permettre les runs récursifs (#70).
- Variables `depositNumber` / `publicationNumber` renseignées dans `Local.bru`, débloquant
  le dossier `18-IP-Favorites` (#73).

**Reste à valider** : les dossiers INPI (`16-Company-Attachments`, `17-Patents`,
`20-Company-Report`, `90-INPI-E2E-CI`) renvoient `409 inpi.not_connected` faute de
compte INPI habilité — couvert par la punch-list « Validation contre l'API INPI réelle ».

---

## V2 — Could have (mois 8–18)

**Objectif** : différenciation par rapport aux concurrents (Pappers, Societe.com). C'est ici qu'on construit les killer features qui rendent le projet unique.

**Storyline** — V2 s'organise en 5 grappes qui se nourrissent l'une l'autre :

1. **Approfondissement de la fiche entreprise** — au-delà du RNE / PI, on enrichit avec établissements (Sirene), cotation boursière, marchés publics et indicateurs financiers descriptifs.
2. **Organisation & capitalisation** — l'utilisateur range et emporte sa connaissance (watchlists, annotations & tags, mode offline mobile).
3. **Veille étendue & signaux** — le cluster veille du MVP 2 monte d'un cran : surveillance PI automatisée par règles utilisateur + signaux de risque descriptif.
4. **Outillage PI avancé** — killer feature pour les cabinets PI : portefeuille IP et antériorité avec matching intelligent.
5. **Exposition tiers** — Atlas devient une plateforme : API publique pour les intégrateurs, serveur MCP pour les agents IA.

**Récap V2** (13 features) :

| Grappe | # | Feature | Complexité |
|---|---|---|---|
| 1 — Fiche | F-024 | Intégration Sirene (établissements) | ★★★ |
| 1 — Fiche | F-051 | Suivi boursier des entreprises cotées | ★★★★ |
| 1 — Fiche | F-032 | Marchés publics remportés (DECP) | ★★★ |
| 1 — Fiche | F-054 | Indicateurs financiers descriptifs | ★★★ à ★★★★ |
| 2 — Orga | F-053 | Watchlists (listes d'entreprises) | ★★★ |
| 2 — Orga | F-030 | Annotations et tags utilisateur | ★★ |
| 2 — Orga | F-029 | Mode offline mobile avec sync | ★★★★ |
| 3 — Veille | F-027 | Veille PI automatisée (règles utilisateur) | ★★★★ |
| 3 — Veille | F-055 | Signaux de risque (descriptif) | ★★★ |
| 4 — PI | F-025 | Tableau de bord portefeuille IP | ★★★★★ |
| 4 — PI | F-026 | Antériorité marque (matching intelligent) | ★★★★★ |
| 5 — Exposition | F-028 | API publique du projet | ★★★★ |
| 5 — Exposition | F-052 | Serveur MCP (accès agents IA) | ★★★★ à ★★★★★ |

**Note de consolidation (29 mai 2026)** :
- **F-023 supprimée** — son périmètre (« Intégration BODACC ») est entièrement couvert par **F-048** livrée en MVP 2 (polling BODACC + `FavoriteEvent BodaccPublished` + dédup cross-users, cf. commit `50f176f`). Le numéro F-023 reste libre (non recyclé).
- **F-027 reformulée** pour la distinguer de **F-046** (filtres / règles de veille génériques, MVP 2) : F-027 = application spécifiquement PI au-dessus de F-046.

---

**Grappe 1 — Approfondissement de la fiche entreprise** *(F-024, F-051, F-032, F-054 — au-delà du RNE / PI, on enrichit la fiche entreprise avec des dimensions descriptives nouvelles)*

### F-024 — Intégration Sirene (INSEE) pour les établissements

**Description** : enrichissement des fiches entreprises avec la liste détaillée des établissements (siège + secondaires) issue du répertoire Sirene.

**Valeur user** : vision géographique d'une entreprise multi-sites.

**Complexité** : ★★★

**APIs externes** : Sirene (api.insee.fr — cf. doc 03 §2.1).

**Dépendances** : F-004.

---

### F-051 — Suivi boursier des entreprises cotées

**Description** : quand une entreprise favorite est cotée en bourse, Atlas affiche sa cotation sur sa fiche (cours, variation, mini-historique) et peut notifier d'une variation notable, sur le patron de F-019/F-048 (snapshot + diff + job Hangfire). **Fonction de suivi, pas service d'investissement** : aucune recommandation, aucun ordre, aucune donnée de portefeuille.

**Valeur user** : pour le persona Investisseur / M&A (déjà identifié dans les packs de veille), une vue unifiée « identité légale + propriété industrielle + veille + cotation » dans un seul outil.

**Complexité** : ★★★★ (phasable : pont d'identifiants → BYO-key → cours → événements).

**APIs externes** :
- **GLEIF** (Global LEI Foundation) — gratuit, sans auth. Sert au pont **SIREN → LEI → ISIN → ticker**. Le LEI étant obligatoire pour être coté, la chaîne couvre exactement le sous-ensemble pertinent.
- **Fournisseur de cours au choix de l'utilisateur** (BYO-key) : Finnhub, Twelve Data, EOD Historical Data, Financial Modeling Prep… Port `IMarketDataProvider` + adapters interchangeables.

**Dépendances** : F-017 (favoris entreprise), patron de F-019 (snapshot + diff + job Hangfire), ADR-003 (coffre de credentials chiffré), ADR-004 (architecture hexagonale).

**Détails techniques** :
- Ports : `ISecurityIdentifierResolver` (adapter `GleifIdentifierResolver`) et `IMarketDataProvider` (adapters par fournisseur).
- Modèle BYO-key : la clé du fournisseur de cours est chiffrée dans le coffre AES-256-GCM (réutilisation de `ICryptoService` / ADR-003). Avantage : zéro coût par utilisateur côté Atlas, redistribution réglée par construction, souveraineté préservée.
- Entités : `CompanySecurityLink (Siren, Isin, Ticker, Mic, ResolvedAt)`, `MarketDataCredential` (clé chiffrée + fournisseur choisi, cascade FK RGPD), `MarketEvent` (optionnel, branché sur la timeline F-047).
- Job Hangfire `market-refresh` sur le modèle de `favorite-refresh` / `bodacc-polling`.
- Endpoints (esquisse) : `POST/GET/DELETE /market/credential`, `GET /companies/{siren}/quote`.

**Cadre légal & éthique** : strictement descriptif. Pas de RGPD spécifique (données d'entreprises cotées, publiques) ; seule sensibilité = la clé API utilisateur (traitée comme secret, ADR-003).

**Accessibilité (rappel ADR-008)** : **jamais l'information par la couleur seule** sur les variations — toujours doubler d'un signe (`+`/`−`), d'une flèche, ou d'un libellé. Critique sur un écran de cotation.

**Modèle économique** : reste dans le cœur open source (BYO-key → zéro coût par utilisateur, cohérent ADR-006).

**Décisions ouvertes** :
- Fournisseur de cours recommandé par défaut dans la doc utilisateur.
- Granularité : EOD en V1, temps réel/différé en option ? La cadence du job en dépend.
- Seuil de « variation notable » (configurable par l'utilisateur ?).
- ADR dédié pour acter le pont d'identifiants et le choix BYO-key (recommandé, au même titre que l'ADR-003).

---

### F-032 — Marchés publics remportés (DECP)

> **Réactivée le 29 mai 2026** — promue de V3+ vers V2. Le motif initial de report (« trop spécifique BTP / consulting / IT public ») ne tient plus : (a) la donnée DECP est désormais **consolidée et propre** (jeu unique sur data.gouv.fr + API tabulaire, le scraping multi-sources n'est plus nécessaire), (b) elle se branche sur le **pattern existant de F-048** (coût marginal faible, purement additif).

**Description** : pour une entreprise, Atlas affiche les **marchés publics qu'elle a remportés** (en tant que titulaire) : acheteur, objet, montant, durée, date de notification, code CPV. Quand une entreprise **suivie** (favori / watchlist) remporte un nouveau marché, un **événement** apparaît dans sa timeline — sur le même principe que les annonces BODACC (F-048).

**Valeur user** : le DECP apporte un signal que ni Pappers ni Societe.com n'exploitent à fond : l'**activité réelle**. Pas « cette boîte existe et a tel bilan » mais « cette boîte **gagne effectivement** des marchés publics, pour tel montant, auprès de tel acheteur ». Croisé avec l'identité (RNE), la PI, la veille et les finances (F-054), ça complète la vue « que fait vraiment cette entreprise ». Personas : **Investisseur / M&A**, **veille concurrentielle** (qui rafle les marchés dans mon secteur ?), et tout profil travaillant avec / autour du secteur public. Contrairement à la crainte initiale, ce n'est pas « niche BTP » — la commande publique achète dans **tous** les secteurs (IT, conseil, services, fournitures, travaux).

**Complexité** : ★★★ — surtout grâce à la réutilisation du pattern F-048 (polling → `FavoriteEvent` → timeline) et à la consolidation de la source.

**APIs externes** :
- **DECP consolidé** (data.gouv.fr) : jeu unique retraité, formats **Parquet / CSV**, mis à jour quasi quotidiennement, régi par l'arrêté du 22 décembre 2022 (étendu mars 2024). Couvre les marchés notifiés depuis 2020 (7 millésimes).
- **API tabulaire data.gouv.fr** : consommation directe du jeu (≈ 100 req/s) « pour alimenter une application sans configurer de base ».

Gratuit, sans authentification. Le **scraping n'est plus nécessaire** (corrige la note « DECP via scraping » du pack Investisseur, doc 07).

**Dépendances** : F-017 (favoris), F-053 (watchlists), F-047 (timeline mixte) & patron de F-048 (polling BODACC → `FavoriteEvent`), ADR-004 (archi hexagonale), ADR-006 (open core).

**Détails techniques** :
- **Deux modes, une source** (calqués sur l'existant) :
  - **Mode événement** (pattern F-048) : job `decp-polling` repère les nouveaux marchés attribués aux SIREN suivis → crée un `FavoriteEvent` de type « marché public attribué » → fusionné dans la timeline (F-047), dédupliqué via `FavoriteEvent.ExternalId` (comme BODACC).
  - **Mode fiche** : section « Marchés publics remportés » alimentée à la demande ou depuis l'ingestion.
- **Source : ingestion vs API tabulaire (décision ouverte)** :
  - **Ingestion périodique** du jeu consolidé (filtré / indexé par SIREN titulaire) dans la base Atlas → rapide en lecture, autonome, idéal pour le mode événement (job Hangfire `decp-polling` sur le modèle de `bodacc-polling`).
  - **API tabulaire à la demande** : interroger par SIREN au moment de l'affichage de la fiche → zéro stockage, mais dépendant de la dispo de l'API.
  - Probable : ingestion pour les événements + API tabulaire pour le détail de fiche.
- **Matching titulaire → entreprise** : le titulaire est identifié en **SIRET** (établissement) ; on mappe sur le **SIREN** (9 premiers chiffres) pour matcher les entreprises favorites / watchlists. Trivial, mais à faire explicitement (une entreprise peut remporter un marché via un établissement secondaire).
- **Architecture (hexagonale, ADR-004)** : adapter `DecpSource` implémentant `IExternalContentSource` (mode événement) comme `BodaccSource` ; éventuel port `IPublicProcurementProvider` pour les requêtes de fiche (mode fiche) ; extension de `FavoriteEvent` avec un nouveau type / `Kind` (« marché public »). **Aucune modification du domaine existant.**

**Cadre légal & positionnement** : la piste facile — open data pur, données publiées par obligation légale (code de la commande publique, articles L2196-2 / L3131-1). Afficher des marchés attribués = exposer des **faits publics**. Aucune ligne sensible à tenir, aucun RGPD spécifique (données d'entreprises et de contrats publics). On reste **descriptif** : on liste, on n'évalue pas (pas de « score puissance publique » agrégé).

**Couverture & qualité (caveats honnêtes)** :
- **Seuil** : seuls les marchés **> 40 000 € HT** sont publiés. En dessous, rien.
- **Périmètre** : seulement les entreprises qui **remportent** des marchés publics → beaucoup de favoris n'en auront aucun (afficher « aucun marché public connu » proprement, pas un vide ambigu).
- **Qualité variable** : malgré l'obligation, certains acheteurs publient mal / incomplètement ; le jeu consolidé corrige beaucoup mais pas tout (champs parfois vides — déjà noté en doc 03 §3.1).

**Hors périmètre (explicite)** :
- **Avis d'appels d'offres** (avant attribution) : c'est le **BOAMP** (doc 03 §3.2), source différente — hors périmètre de cette fiche.
- Marchés **européens** (TED, doc 03 §3.3) : extension future éventuelle.

**Accessibilité (rappel ADR-008)** : montants, dates et acheteurs en tableau lisible au lecteur d'écran (`SemanticProperties.Description`). Aucune information (ex. type de marché) transmise par la couleur seule.

**Modèle économique** : aucun coût par utilisateur (open data, déterministe) → **cœur open source**, gratuit. Cohérent ADR-006.

> **Architecture liée** : à l'implémentation, F-032 émet un `FavoriteEvent PublicContractAwarded` dans la timeline F-047 (patron F-048) — alimente la **taxonomie de signaux de F-058** (volet « signal commercial »).

**Découpage / jalons** :
1. **Source & matching** : ingestion du jeu consolidé (ou accès tabulaire), mapping SIRET → SIREN. *(Fondation testable.)*
2. **Mode fiche** : section « Marchés publics remportés » sur la fiche entreprise.
3. **Mode événement** : `DecpSource` + job `decp-polling` → `FavoriteEvent` → timeline (pattern F-048).

**Décisions ouvertes** :
- **Source** : ingestion périodique vs API tabulaire à la demande (ou les deux, fiche + événements).
- **Ordre de livraison** : mode fiche d'abord (autonome) ou mode événement d'abord ?
- **Seuil de notabilité** d'un événement (tout marché, ou au-dessus d'un montant ?).
- **Extension future** : BOAMP (avis avant attribution) et TED (UE) — à garder hors périmètre pour l'instant.

---

### F-054 — Indicateurs financiers descriptifs

**Description** : sur la fiche d'une entreprise, Atlas affiche des **indicateurs financiers descriptifs** issus de ses comptes annuels — ratios (rentabilité, autonomie financière, solvabilité, liquidité) et surtout leur **évolution dans le temps**. Tout est factuel, calculé de façon transparente, jamais agrégé en une note unique. **Ligne strictement descriptive** : pas de note / score / cotation propriétaire qui mimerait une notation de crédit ou prédirait un défaut (la cotation Banque de France FIBEN, confidentielle, tient ce terrain réglementé). Nommage volontaire « indicateurs **descriptifs** », pas « scoring » ni « notation » : Atlas affiche des faits, pas un jugement de solvabilité.

**Valeur user** : pour les personas **expert-comptable** et **investisseur**, voir d'un coup d'œil la trajectoire financière d'une boîte (et de ses concurrents / partenaires suivis), sans ouvrir et déchiffrer des PDF de comptes. Combiné aux watchlists (F-053) et à la veille (F-047), ça complète la vue « identité + PI + veille + finances » dans un seul outil.

**Complexité** : ★★★ à ★★★★ selon la profondeur (tendances, postes bruts) — phasable. Le travail n'est ni l'ingestion ni le calcul (déterministes), mais la **réconciliation et la provenance** entre les deux sources : c'est là que se joue la qualité perçue.

**APIs externes** :
- **Jeu open data « Ratios Financiers BCE/INPI »** (data.gouv.fr / data.economie.gouv.fr) : ratios pré-calculés par la DNUM à partir des données RNE de l'INPI, par SIREN et date de clôture. Gratuit, en masse.
- **API INPI comptes annuels** (« bilans-saisis ») : postes financiers structurés extraits par l'INPI de ses propres PDF, récupérables par identifiant. Gratuit.

Aucun parsing de PDF nécessaire dans les deux cas.

**Dépendances** : F-013 (`FinancialStatement`, téléchargement des bilans), F-017 / F-053 (favoris & watchlists — cible de la couche « profondeur »), patron des ports premium de F-050, ADR-004 (archi hexagonale), ADR-006 (open core).

**Détails techniques** :
- **Deux sources en couches, rôles distincts et complémentaires** (jamais le même chiffre produit deux fois) :
  - **Couche « largeur » — Ratios BCE/INPI (baseline, instantané)** : ingérée en masse (job périodique `bce-ratios-ingest`). N'importe quelle entreprise ouverte affiche ses ratios immédiatement, sans appel externe. Avantage de fond : ce sont des ratios **calculés par le ministère selon une méthodologie publiée** — afficher de l'open data officiel renforce la ligne « descriptif, pas notation » (on ne fabrique pas de note maison).
  - **Couche « profondeur / fraîcheur » — Bilans-saisis INPI (à la demande)** : pour une entreprise réellement suivie (favori / watchlist), on récupère les postes du bilan en clair. Trois gains que la BCE seule ne donne pas : afficher les **postes bruts** derrière les ratios, capter le **dépôt le plus récent** avant son entrée dans le prochain millésime BCE, calculer des **ratios ou tendances** absents du jeu BCE.
- **Principe directeur** : une seule source de vérité par chiffre + **provenance toujours étiquetée** (« ratio open data BCE, millésime 2024 » vs « calculé sur le bilan déposé le 14/03/2025 »). Évite le chiffre orphelin contradictoire qui détruirait la confiance. Cohérent avec l'ADN de transparence / souveraineté du projet.
- **Résilience** : deux sources évitent d'être l'otage d'une seule (si le jeu BCE change de cadence, les bilans-saisis prennent le relais ; si l'API comptes annuels est lente, la BCE assure le fond).
- **Types de bilan C / K / S** (complet / simplifié / consolidé) : les formules de ratios en dépendent. À gérer explicitement côté calcul.
- **Modèle & ports** :
  - Réutilise `FinancialStatement` (doc 08 : `FiscalYear`, `ClosureDate`, `FilingDate`, `IsConfidential`, `DocumentUrl`).
  - Nouveau : `FinancialIndicatorSet` (par `Siren` + `ClosureDate` + `Source` + provenance), porteur des ratios.
  - Port `IFinancialDataProvider` (adapter `InpiBilanSaisiProvider`) pour la couche profondeur ; job Hangfire `bce-ratios-ingest` pour la couche largeur.
  - Service de domaine déterministe `FinancialIndicatorService` (calcul des ratios). **Pas d'IA** → cœur libre.
- **Endpoints (esquisse, à confirmer)** :
  - `GET /companies/{siren}/financials` — ratios baseline (BCE) + statut de disponibilité / confidentialité.
  - `GET /companies/{siren}/financials/detail` — postes bruts + ratios + tendances (bilans-saisis, à la demande).
  - `GET /companies/{siren}/financials/analysis` — **(Premium)** synthèse narrative générée par IA.

**Cadre légal & positionnement** : la **cotation Banque de France** (FIBEN) est la référence officielle d'appréciation de la solvabilité — une note avec probabilité de défaut, établie à dire d'expert, **confidentielle**, réservée à l'entreprise notée et aux acteurs du crédit. C'est du terrain réglementé. Atlas reste **strictement descriptif** : ratios factuels et leurs tendances, calculés de façon transparente à partir de données publiques. Jamais une note / score propriétaire qui mimerait une notation de crédit. Les quatre axes de la Banque de France (rentabilité, autonomie financière, solvabilité, liquidité) servent de **catégories de ratios descriptifs**, jamais d'ingrédients d'un verdict agrégé. Même philosophie que F-051 (suivi, pas conseil).

**Confidentialité & couverture** : seuls les comptes **non confidentiels déposés depuis 2017** sont exploitables (micro-entreprises : confidentialité totale possible ; petites entreprises : compte de résultat confidentiable ; moyennes / grandes : pas de confidentialité). Le décret de février 2024 (directive UE 2023/2775) a relevé les seuils → davantage de PME peuvent opter pour la confidentialité. Conséquence : couverture **partielle**, à afficher honnêtement via le champ `IsConfidential` (« comptes non disponibles » plutôt qu'un vide trompeur).

**RGPD** : données d'entreprises issues de l'open data public → pas de traitement de données personnelles spécifique. Rien de sensible à stocker côté utilisateur (contrairement à F-051, pas de clé à protéger).

**Accessibilité (rappel ADR-008)** : une **tendance** (hausse / baisse) n'est jamais signalée par la couleur seule — toujours doublée d'un signe (`+` / `−`), d'une flèche ou d'un libellé. Tableaux de ratios lisibles au lecteur d'écran ; provenance annoncée.

**Modèle économique** : calcul de ratios = déterministe → **cœur open source**, gratuit (aucun coût par utilisateur). Synthèse narrative IA = **premium** (coût LLM), via un port `IFinancialSummarizer` analogue à `IFeedSummarizer` (patron F-050). Aligné ADR-006 (« monétiser la commodité, pas le cœur »).

**Découpage / jalons** :
1. **Couche largeur** : ingestion du jeu BCE/INPI (job `bce-ratios-ingest`), endpoint `/financials`, ratios + statut confidentialité. *(Livrable autonome, couvre déjà l'essentiel.)*
2. **Couche profondeur** : `InpiBilanSaisiProvider`, postes bruts + tendances pour les entreprises suivies, endpoint `/financials/detail`, avec provenance étiquetée.
3. **Réconciliation** : règles de source-de-vérité-par-chiffre, gestion des types de bilan C / K / S.
4. **(Premium)** synthèse narrative IA via `IFinancialSummarizer`.

**Décisions ouvertes** :
- **Ordre de livraison** des deux couches (recommandé : largeur d'abord — autonome et utile seule).
- **Profondeur de l'historique** affiché (3 ans ? 5 ans ?).
- **Cadence** d'ingestion du jeu BCE (suivre les millésimes publiés).
- **Liste exacte des ratios** retenus par axe (rentabilité / autonomie / solvabilité / liquidité), et leur définition documentée (transparence).
- Documenter publiquement les **formules de ratios** utilisées (gage de transparence et de la ligne « descriptif »).

---

**Grappe 2 — Organisation & capitalisation** *(F-053, F-030, F-029 — l'utilisateur range et emporte sa connaissance personnelle au-dessus des données publiques)*

### F-053 — Watchlists (listes d'entreprises)

**Description** : l'utilisateur regroupe des entreprises en **listes nommées** (« Concurrence », « Portefeuille clients », « Cibles M&A »…). Une entreprise peut appartenir à plusieurs listes. Chaque liste offre une vue dédiée et, en option, **sa propre timeline de veille filtrée**. Un **import en masse de SIREN** (jusqu'à plusieurs centaines) permet de constituer une liste d'un coup. Cette feature couvre le **regroupement** ; les **notes et tags** par entreprise restent de la responsabilité de F-030.

**Valeur user** : signal marché net (Pappers a lancé en 2026 un tableau de bord de suivi de portefeuille, plébiscité par les experts-comptables et les fonds d'investissement). **Angle différenciant Atlas** : une watchlist peut avoir **sa propre timeline de veille** (au-dessus de F-047) — RSS + RNE + BODACC filtrés sur les entreprises de la liste. Pappers ne combine pas listes et veille agrégée ; c'est précisément le trou de marché du projet (ADR-009).

**Complexité** : ★★★ (1–2 semaines) en Modèle A. La couche listes est simple ; le morceau principal est l'**import en masse asynchrone**.

**APIs externes** : aucune nouvelle source. L'import en masse utilise l'API **INPI RNE** existante (résolution des dénominations à partir des SIREN), avec le rate-limiting déjà en place.

**Dépendances** : F-017 (favoris = set surveillé), F-030 (annotations & tags — dépendance à sens unique, voir « Frontière »), patron de F-014 (import asynchrone), F-047 (timeline mixte), ADR-004 (archi hexagonale).

**Détails techniques** :
- **Modèle A — les listes par-dessus les favoris (non-cassant)** : `CompanyFavorite` (F-017) reste la liste plate parcourue par F-019 (refresh quotidien) et F-047 (timeline). On garde ce rôle intact.
- Entités : `Watchlist (Id, UserId, Name, CreatedAt)` privée, cascade FK RGPD ; `WatchlistEntry (WatchlistId, Siren, AddedAt)`, index unique `(WatchlistId, Siren)`. Ajouter une entreprise à une liste ⇒ upsert du `CompanyFavorite` correspondant.
- **Import en masse** : réutilise le patron de F-014 (téléchargement de masse) — validation (Luhn), job Hangfire dédié respectant le rate-limiting INPI, notification (email + push) à la fin avec récap (N ajoutés, M doublons, K invalides).
- **Timeline par liste** : filtre `watchlistId` au-dessus de F-047, sans nouvelle mécanique de veille.
- Endpoints (esquisse) : `POST/GET /watchlists`, `PATCH/DELETE /watchlists/{id}`, `POST/DELETE /watchlists/{id}/entries[/{siren}]`, `POST /watchlists/{id}/import` (job async), `GET /watchlists/{id}/timeline`.

**Frontière avec F-030** (fait foi pour les deux fiches) :
- **F-030 possède** `UserAnnotation` (note privée par entité) **et les tags** (libellés sur la relation utilisateur ↔ entité).
- **F-053 possède** `Watchlist` + `WatchlistEntry` (le regroupement) et l'import en masse.
- **Dépendance à sens unique** : F-053 **consomme** les tags de F-030 pour le filtrage. Une watchlist sans tag fonctionne ; une annotation sans liste fonctionne. **Pas de circularité.**
- **Set surveillé** = `CompanyFavorite` (F-017). Appartenir à une liste ⇒ être favori. F-019/F-047 restent branchés sur les favoris.
- **F-030 peut être livré seul et en premier** (le plus rapide), F-053 se pose ensuite ou en parallèle.

**RGPD** : listes et appartenances = données utilisateur. Cascade FK sur le compte (patron existant), incluses dans `/account/export` (art. 20), supprimées à la suppression du compte (art. 17). Privées par défaut (pas de partage en V1 ; le partage relève de F-035 espace équipe).

**Accessibilité (rappel ADR-008)** : un tag n'est **jamais** identifié par la couleur seule (libellé + couleur optionnelle), règle portée par F-030. Listes et entrées navigables au clavier et annoncées au lecteur d'écran. L'écran d'import est accessible (rapport d'import lisible, pas seulement visuel).

**Modèle économique** : aucun coût par utilisateur → cœur open source (cohérent ADR-006). Possibilité de **quotas en hébergé** (nombre de listes, taille), sur le modèle de la limite de sources de F-043.

**Décisions ouvertes** :
- Modèle A maintenant, Modèle B (favoris = « liste par défaut ») plus tard ? Confirmer qu'on ne généralise pas les favoris en V1.
- Colonnes type CRM (statut, dernier contact, relance, à la Pappers) : hors cœur par défaut (proche de F-037). À acter : on les exclut, ou version légère portée par F-030 ?
- Quotas en hébergé (nombre de listes / taille).

---

### F-030 — Annotations et tags utilisateur

**Description** : l'utilisateur ajoute des notes privées **et des tags** sur les fiches qu'il consulte (entités annotées : entreprise, marque, brevet). Les tags appartiennent à F-030 car ce sont des attributs de l'entité annotée, indépendants de toute liste — cf. « Frontière avec F-053 » dans F-053.

**Valeur user** : capitalisation de la connaissance personnelle au-dessus des données publiques.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-017.

---

### F-029 — Mode offline mobile avec sync

**Description** : sur l'app MAUI mobile, possibilité de consulter ses favoris et son historique en mode hors-ligne, avec synchronisation automatique au retour de la connectivité.

**Valeur user** : usage en déplacement (transports, zones rurales).

**Complexité** : ★★★★

**APIs externes** : aucune.

**Dépendances** : F-009, F-017.

**Détails techniques** : SQLite local + stratégie de réplication.

---

**Grappe 3 — Veille étendue & signaux** *(F-027, F-055 — le cluster veille du MVP 2 monte d'un cran : règles utilisateur PI + signaux de risque descriptifs)*

### F-027 — Veille PI automatisée (règles utilisateur)

> **Reformulée 29 mai 2026** — précision du périmètre pour la distinguer de F-046 (cluster veille MVP 2). **F-046 = infrastructure générique** de règles de veille (filtres sur flux, mots-clés, secteurs, types d'annonces). **F-027 = application spécifiquement PI** : règles ciblées sur les nouveaux dépôts marques / brevets (concurrents nommés, classes de Nice, mots-clés). F-027 réutilise F-046 quand la sémantique tient, ou ajoute des règles dédiées PI au-dessus.

**Description** : l'utilisateur configure des règles ciblées PI — « alerte-moi à chaque nouveau dépôt de marque par mon concurrent X », « contenant le mot Y dans la classe Z ». Notifications email / push (F-019 / F-020).

**Valeur user** : surveillance concurrentielle PI proactive, impossible à faire manuellement à grande échelle. Couvre le besoin du pack « Veille concurrentielle B2B » (doc 07).

**Complexité** : ★★★★

**APIs externes** : INPI PI (+ EUIPO / OMPI quand F-039 ouvert en V3+).

**Dépendances** : F-018 (favoris marque / brevet), F-020 (push), F-046 (couche infrastructure des règles user — réutilisation).

> **Architecture liée** : à l'implémentation, F-027 et F-032 émettront des `FavoriteEvent` typés (`IpFiled`, `PublicContractAwarded`) dans la timeline F-047 — c'est ce qui alimente la **taxonomie de signaux de F-058**.

---

### F-055 — Signaux de risque (descriptif)

**Description** : Atlas regroupe, pour une entreprise, des **signaux de risque factuels** dans une vue cohérente — procédures collectives (BODACC, F-048), correspondances avec une **liste de sanctions / gel des avoirs** officielle, et **mentions presse** (timeline F-047). Strictement **descriptif** : Atlas **affiche des faits** (« en redressement judiciaire selon BODACC », « correspondance potentielle sur la liste de gel DG Trésor — à vérifier », « N articles la mentionnent »). Atlas **n'attribue aucun score de risque** et **ne qualifie jamais** une entreprise de « à risque ». L'utilisateur évalue lui-même.

**Valeur user** : pour la due diligence et la compliance légère (personas **Compliance / KYC** et **Investisseur / M&A**) — rassembler en un endroit les signaux qu'il faut aujourd'hui aller chercher dans cinq sources. Surface d'alerte, pas verdict.

**Constat qui définit le périmètre** : l'essentiel des signaux est **déjà dans le produit**. Procédures collectives = BODACC (F-048), mentions presse = timeline (F-047, volet 1). Cette feature n'est **pas un nouveau moteur** — c'est **assembler l'existant + ajouter une seule source neuve** : le screening sanctions officiel.

**Complexité** : ★★★ — assemblage (BODACC + presse déjà là) + ingestion d'une source sanctions + matching conservateur.

**APIs externes** :
- **Registre national des gels des avoirs — DG Trésor** : liste officielle des personnes/entités sanctionnées (ONU + UE + national), en **fichiers interopérables + API, mise à jour quotidienne**. Gratuit, officiel, souverain.
- **Liste consolidée des sanctions financières de l'UE**.
- **BODACC** (déjà intégré, F-048) et **presse** (veille existante, F-047).
- **Écarté** : OpenSanctions (gratuit en non-commercial seulement → licence requise en usage business). C'est aussi pourquoi le **PEP** reste hors cœur gratuit.

**Dépendances** : F-048 (BODACC — procédures collectives), F-047 (timeline + mentions presse), F-017 / F-053 (favoris & watchlists), **ADR-012** (doctrine descriptif + matching conservateur — gouverne cette fiche), ADR-004. Optionnel : F-031 Judilibre (contentieux, V3+).

**Hors périmètre (explicite)** :
- **Aucun score de risque**, aucun label « entreprise à risque », aucun verdict (ADR-012).
- **Aucun screening PEP** dans le cœur gratuit (données surtout licenciées).
- **Aucune base d'adverse media propriétaire** (World-Check, Dow Jones…).
- **Aucune certification de conformité AML** : Atlas *expose des signaux*, il ne certifie rien.
- Bénéficiaires effectifs exclus (CJUE Sovim).

**Détails techniques** :
- **Source sanctions (la seule vraie nouveauté)** : nouvel adapter (port `IExternalContentSource` ou `ISanctionsListProvider`) ingérant les listes DG Trésor + UE périodiquement, via un job Hangfire sur le modèle de `bodacc-polling`.
- **Matching conservateur** (principe d'exactitude, ADR-012 §5) : rapprocher sur nom + identifiants/date de naissance quand disponibles ; **ne jamais affirmer automatiquement** une correspondance ; afficher « correspondance potentielle, à vérifier ». Une fausse correspondance sanctions est **diffamatoire et grave**.
- **Assemblage (réutilisation pure)** : vue « signaux de risque » combinant `FavoriteEvent` BODACC (procédures collectives), correspondances sanctions, `FeedItemFavoriteMatch` (mentions presse). Possibilité de créer des `FavoriteEvent` de type « signal de risque » pour la timeline (F-047). **Aucun nouveau moteur de veille.**

**Cadre légal & positionnement** : **gouverné par ADR-012** — descriptif, matching conservateur, jamais de verdict. Les listes de sanctions sont de l'**open data officiel** (réutilisation libre). Atlas **expose** des signaux ; il ne certifie aucune conformité AML et ne qualifie aucune entité. Le matching conservateur protège l'exactitude (RGPD art. 5.1.d) et contre la diffamation.

**Accessibilité (rappel ADR-008)** :
- Un signal n'est **jamais** transmis par la couleur seule (pas de « rouge = risque ») : icône + libellé explicite.
- Signaux en liste/tableau lisibles au lecteur d'écran ; formulation « à vérifier » explicite.

**Modèle économique** : déterministe, sources gratuites → **cœur open source** (ADR-006). Le PEP (données licenciées) serait, le cas échéant, une option premium ou hors périmètre.

**Découpage / jalons** :
1. **Source sanctions** : ingestion DG Trésor + UE, matching conservateur, affichage « correspondance potentielle ». *(Seule vraie nouveauté.)*
2. **Vue assemblée** : « signaux de risque » réunissant BODACC + sanctions + presse.
3. **Alertes / timeline** (optionnel) : événements « signal de risque ».
4. **Contentieux** (futur) : Judilibre (F-031).

**Décisions ouvertes** :
- **Contentieux maintenant ou plus tard** (dépend de F-031 Judilibre).
- **Vue dédiée vs enrichissement de la fiche** existante.
- **Seuils d'alerte** sur signaux.
- **PEP** : hors périmètre tant qu'il n'y a pas de source gratuite exploitable.

---

**Grappe 4 — Outillage PI avancé** *(F-025, F-026 — killer feature pour les cabinets de propriété industrielle)*

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

> **Architecture liée** : F-026 **respecte la posture** de **ADR-014** (matching conservateur unifié — produit des `MatchCandidate`, jamais de verdict de disponibilité), mais son **moteur reste totalement séparé** des 3 matchers à base de noms (F-047, F-055, F-031) : similarité phonétique / visuelle / conceptuelle + classes de Nice = mécanique entièrement à part. Les briques IA (sémantique, résumés) vivent dans `Atlas.Application.Premium` (F-050).

---

**Grappe 5 — Exposition tiers** *(F-028, F-052 — Atlas devient une plateforme : ouverte aux intégrateurs et aux agents IA)*

### F-028 — API publique du projet

**Description** : exposition d'une API REST publique permettant à des tiers (devs, intégrations) d'utiliser les fonctions du projet via clé API.

**Valeur user** : intégration avec des CRMs, outils internes, scripts d'automatisation.

**Complexité** : ★★★★

**APIs externes** : aucune (exposition de l'existant).

**Dépendances** : MVP 1 complet.

**Détails techniques** : gestion des clés API, quotas, documentation Swagger / OpenAPI, rate limiting.

---

### F-052 — Serveur MCP (accès agents IA)

**Description** : Atlas expose ses capacités sous forme de **serveur MCP** (Model Context Protocol), pour que des **agents IA** (Claude, et tout client compatible) puissent interroger les données et la veille au nom de l'utilisateur (rechercher une entreprise, lire une fiche, consulter les marques, gérer les favoris, interroger la timeline). L'agent n'a jamais plus de droits que l'utilisateur.

**Valeur user** : pour les power users et les profils « travaillant avec l'IA », automatiser des workflows de suivi en langage naturel ; pour les intégrateurs, brancher Atlas dans leurs propres agents via une interface standard. **Différenciation** : un MCP **souverain et auto-hébergeable** est unique sur le marché français des données d'entreprise (Pappers expose déjà un MCP mais sans cet angle).

**Complexité** : ★★★★ (lecture + écriture encadrée) ; ★★★★★ avec couche OAuth 2.1 complète. L'essentiel de l'effort est l'**auth/scoping** et le **durcissement sécurité**, pas la définition des outils (le SDK la rend triviale).

**APIs externes** : aucune nouvelle source (réexpose les capacités existantes). Dépendance technique : SDK MCP officiel C# (`ModelContextProtocol`, `ModelContextProtocol.AspNetCore`), maintenu par Microsoft et Anthropic.

**Dépendances** : use cases existants (F-004/F-005, fiche entreprise, F-006/F-007, F-017, F-041→F-047, F-042) ; ADR-003 (coffre de credentials) ; ADR-004 (archi hexagonale) ; **ADR-010 (auth utilisateur) + ADR-011 (OAuth 2.1 pour la délégation)**.

**Détails techniques** :
- **Nouvel adapter entrant** par-dessus les use cases MediatR existants (exactement comme `Atlas.Api`). Projet `Atlas.Mcp` (ou module dans l'API) qui traduit des appels d'outils MCP en commandes/queries du domaine. **Zéro modification du domaine** (ADR-004).
- Le SDK C# déclare un outil en décorant une méthode (`[McpServerTool]`) ; le schéma JSON est généré automatiquement. Intégration : `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` + `MapMcp()`.
- **Outils lecture (V1)** : `search_companies`, `get_company`, `search_trademarks`, `get_trademark`, `list_favorites`, `get_veille_timeline`, `list_veille_packs`.
- **Outils écriture (encadrés, V2 du chantier)** : `add_favorite` / `remove_favorite`, `subscribe_feed_source` / `apply_veille_pack` — derrière des **approval workflows** et des **scopes** dédiés.
- **Transports** : stdio (auto-hébergement local) et HTTP/Streamable HTTP (instance distante).
- **Aucune exposition** de la gestion des secrets (credentials INPI, clés) via MCP. Aucune opération destructrice non réversible exposée sans garde forte.

**Sécurité (cf. ADR-011 + ADR-016)** : OAuth 2.1 + PKCE ; **scopes** distincts lecture vs écriture ; tokens courts + refresh rotatif révocable ; audit logging de chaque appel d'outil (Serilog déjà en place) ; rate limiting réutilisé (global + auth-strict). **Doctrine et architecture de la surface agentique formalisées par ADR-016** : surface curée lecture-d'abord (allowlist, pas d'exposition 1:1, schémas étroits), doctrine inline avec donnée (`MatchCandidate`/`SectionState` préservés intacts, jamais aplatis dans le mapping MCP), contenu externe = donnée jamais instruction (parade injection prompt indirecte), délégation utilisateur (credentials ne traversent jamais). Outil **`get_company_dossier`** ajouté pour F-056 (dossier 360 en lecture seule, descriptif).

**Modèle économique** : aucun coût par utilisateur côté Atlas (l'inférence est côté agent). Reste dans le **cœur open source** (cohérent ADR-006). Une frontière premium éventuelle (quotas en hébergé, outils agentiques avancés) pourra être posée plus tard sans toucher au socle.

**Décisions ouvertes** :
- Périmètre exact des outils d'écriture exposés en V1 (probable : favoris + abonnements ; exclure tout ce qui touche aux secrets et au compte).
- Exposition : self-host (stdio/HTTP local) d'abord, instance hébergée ensuite ? Politique de quotas en hébergé.
- Frontière premium éventuelle (à n'arbitrer qu'en phase 3-4).

---

## V3+ — Won't have (yet)

Features identifiées comme valables mais explicitement reportées hors du périmètre actuel. À reconsidérer en fonction de la traction.

### F-056 — Vue 360 / Dossier entreprise

> **Méta-feature d'assemblage figée le 29 mai 2026**. Statut **V3+** (sections sous-jacentes en V2 ou V3+). Ce **n'est pas une nouvelle source de données** : c'est une **couche de composition** par-dessus l'existant. Backbone architectural : **ADR-015** (sections auto-descriptives + résolution snapshot-first). Issue du persona Investisseur / M&A — sert aussi expert-comptable, avocat, compliance.

**Description** : une **vue consolidée d'une entreprise** qui assemble, en un seul dossier descriptif, sourcé et daté, les données déjà servies par les features par-source — identité & dirigeants (F-004), événements légaux (F-047/F-048), indicateurs financiers (F-054), marchés publics (F-032), propriété industrielle (F-018/F-025), cotation (F-051), structure de co-mandats (F-034, sous ADR-012), signaux de risque (F-055). Chaque élément reste **tracé à sa source et à sa date** — c'est la matérialisation concrète de la thèse du produit : *croiser ce que personne ne croise*.

**Valeur user** : aujourd'hui, comprendre une entreprise oblige à consulter chaque dimension séparément. Le dossier 360 fait ce croisement **à la place de l'utilisateur** : l'investisseur obtient un portrait de cible, l'expert-comptable un état de client, l'avocat un socle de due diligence — **le même assemblage, des lentilles différentes**. C'est aussi le **cas d'usage roi de l'accès agentique** : un agent monte le dossier via le serveur MCP (F-052, outil `get_company_dossier`).

**Complexité** : ★★★★ — méta-feature : peu de code « neuf » au sens données, mais une vraie ingénierie de **composition, de cache et de dégradation**, plus l'exposition MCP et la synthèse premium.

**APIs externes** : **aucune en propre**. Elle réutilise les sources des features composées (RNE, BCE/INPI, INPI PI, DECP, BODACC, DG Trésor/UE, GLEIF). C'est tout l'intérêt.

**Dépendances** : F-004 (identité), F-047/F-048 (événements/BODACC), F-019 (snapshot — patron d'**ADR-013**), **F-054** (finances), **F-032** (marchés), **F-018/F-025** (PI), **F-051** (cotation), **F-034** (structure, sous ADR-012), **F-055** (risque), **F-052** (exposition MCP, sous **ADR-016**). Backbone architectural : **ADR-015** (sections auto-descriptives + 5 états + résolution snapshot-first).

**Hors-périmètre (explicite)** :
- **Aucune nouvelle source** : si une donnée n'est pas déjà servie par une feature, elle n'apparaît pas ici.
- **Aucune synthèse-verdict** : pas de note, de valorisation, de score de risque ni de recommandation — le dossier **assemble des faits**, il ne juge pas.
- Pas de comparaison multi-entreprises ni de portefeuille (F-053 le couvre).

**Détails techniques** :
- **Read-model de composition côté Application** (ADR-015) : un `CompanyDossier` composé de `DossierSection` indépendantes (`IdentitySection`, `FinancialsSection`, `PublicContractsSection`, `IpSection`, `ListingSection`, `StructureSection`, `RiskSection`, `EventsSection`). **Pas un nouvel agrégat de domaine** — `Company`/`UniteLegale` restent les agrégats.
- **5 états par section** (`SectionState`, ADR-015) : `Available` / `Stale` / `Unavailable` / `NotApplicable` / `Restricted` — l'**absence est honnête** (la section qui échoue ou expire tombe en `Unavailable`, jamais en succès silencieux).
- **Composition extensible** : chaque section est produite par un use case existant ; ajouter une section = brancher un use case, sans toucher au cœur (hexagonal, ADR-004). Les sections **s'allument** au fil de la disponibilité des features.
- **Assemblage hybride snapshot-first** (ADR-015 + ADR-013) :
  - sources **légères / ouvertes** (BODACC, DECP, sanctions) : en direct ou cache court ;
  - appels **INPI coûteux** (identité, bilans, PI) : pour une entreprise **suivie**, le dossier **lit les snapshots** du substrat de surveillance (ADR-013) — quasi pré-assemblé et rapide ; pour une entreprise **non suivie**, assemblage **à la demande** avec cache court.
- **Résilience par section** : timeout par section ; tout échec → `Unavailable` (jamais d'exception qui casse le dossier).
- **Endpoints (esquisse)** : `GET /companies/{siren}/dossier` (réponse composée v1) ; MCP `get_company_dossier` (lecture seule, ADR-016).

**Cadre légal** : le dossier **n'introduit aucun traitement nouveau** — il compose des données déjà encadrées par leurs features respectives. Il **hérite** de leurs garde-fous. Section **structure (F-034)** sous **ADR-012** (jamais de BE) ; section **finances (F-054)** respecte la **confidentialité des comptes** ; posture d'ensemble : **dossier de faits**, pas d'avis.

**Accessibilité (ADR-008, bloquant)** : structure sémantique claire (titres de sections, navigation), chaque section autonome et lisible au lecteur d'écran, **équivalent tabulaire** de la section structure (réutilisé de F-034). L'UI doit rendre la **fraîcheur par section** et les **5 états** honnêtement (jamais afficher `Unavailable` / `Restricted` comme « rien à signaler »).

**Modèle économique (ADR-006)** :
- **Cœur / OSS** : l'assemblage déterministe des sections sourcées. Aucun coût d'inférence → gratuit.
- **Premium** : **synthèse narrative IA** optionnelle du dossier (port `IDossierSummarizer`, patron F-050) — **descriptive**, sans verdict ni valorisation.

**Découpage / jalons** :
1. **Squelette de composition** : `CompanyDossier` + contrat de section + dégradation propre, sur les sections **déjà bâties** (identité, événements).
2. **Branchements progressifs** : finances, marchés, PI, risque, cotation, structure — chaque section ajoutée quand sa feature est prête.
3. **Assemblage hybride** : intégration au snapshot ADR-013 pour les entreprises suivies + cache court pour les autres.
4. **Exposition MCP** (F-052) : outil `get_company_dossier` en lecture seule, sous ADR-016.
5. **Premium** : synthèse IA descriptive optionnelle (F-050).

**Décisions ouvertes** :
- **Périmètre de la synthèse premium** : par section ou globale ?
- **Fraîcheur affichée** : date « as of » par section (recommandé).
- **Entreprise non suivie** : profondeur de l'assemblage à la demande.
- **Nom canonique** : `CompanyDossier` / « Dossier entreprise 360 » — à arrêter pour le doc 08.

---

### F-031 — Jurisprudence rattachée à l'entité (Judilibre)

> **Réactivée & recadrée le 29 mai 2026** — remplace le stub V3+ initial (motif « matching d'entité non-trivial »). Le **corpus s'est enrichi** (Cour de cassation ~535k décisions, arrêts de CA civils/commerciaux depuis avril 2022, TJ en déploiement, API gratuite via PISTE) et le **rapprochement est faisable** : les **personnes morales y gardent leur nom** (les personnes physiques sont pseudonymisées). On **garde le numéro F-031** (comme F-032/DECP). Origine : graine du persona **Avocat / juriste d'affaires**.
> **Doctrine** : sous **ADR-012** (descriptif, matching conservateur, aucun verdict) — incarnation par **ADR-014** (`MatchCandidate`, `INameInTextMatcher`). Garde-fou avocat : **Atlas ne rend jamais d'avis juridique**.

**Description** : faire remonter les **décisions de justice** où une **entité suivie / consultée** (personne morale) **apparaît**, par rapprochement de sa **dénomination** avec Judilibre. Une **liste descriptive et sourcée** (juridiction, date, identifiant, lien), en **matching conservateur** — « **mention potentielle à vérifier** », jamais une affirmation ni une interprétation.

**Valeur user** : pour l'**avocat / juriste d'affaires**, la jurisprudence **rattachée à l'entité** (les moteurs de jurisprudence ne partent jamais de l'entreprise — Atlas, si). Alimente aussi la **section « jurisprudence » du dossier 360 (F-056)** et le flux des **signaux légaux (F-058)**.

**Complexité** : ★★★ — le délicat : l'**adapter PISTE** (OAuth2) et la **conservativité du rapprochement** sur la dénomination. Le reste réutilise les patrons existants (timeline, événements, dossier).

**APIs externes** : **Judilibre via PISTE** (OAuth2, gratuit, sandbox + production), déjà catalogué au **doc 03 §4.1**. Établit le **patron d'adapter PISTE OAuth2** réutilisable (cf. autres sources PISTE).

**Dépendances** : **F-047** (timeline/événement), **F-056** (section « jurisprudence » du dossier), **F-053/F-017** (set suivi), **F-058** (signal légal), **ADR-012** (doctrine), **ADR-013** (substrat de surveillance — stratégie `IItemStreamMonitor<JudilibreDecision>` append-only), **ADR-014** (`INameInTextMatcher` partagé avec F-047).

**Hors-périmètre (explicite)** :
- **Aucun rapprochement de personnes physiques** : pseudonymisées dans Judilibre — respecté.
- **Aucun « score de contentieux »**, aucune interprétation, aucun **avis juridique**.
- **Couverture limitée** au corpus Judilibre (Cassation + CA civil/commercial depuis 2022 + TJ en déploiement ; **hors pénal**).

**Détails techniques** :
- **`JudilibreProvider`** : client PISTE (OAuth2, cache de token), recherche **plein texte** filtrée (juridiction, date, type).
- **Rapprochement conservateur** (ADR-012 + ADR-014) : recherche par **dénomination exacte/quasi-exacte** via `INameInTextMatcher`, **biais vers le faux négatif** (une fausse association « cette boîte était dans ce litige » est **diffamatoire**) ; sortie en `MatchCandidate` formulée « **mention potentielle à vérifier** ».
- **Mode événement** (ADR-013) : `IItemStreamMonitor<JudilibreDecision>` (append-only, dédup par identifiant de décision) → nouveau type de `FavoriteEvent` `JudilibreDecision` → timeline. Polling au patron de F-048 / F-019.
- **Mode dossier** : section « jurisprudence » dans F-056, à la demande.
- **Respect de la pseudonymisation** : on n'exploite **que** les personnes morales.

**Cadre légal** : Judilibre = **open data officiel et gratuit** ; réutilisation sous **CGU**. Pseudonymisation des personnes physiques respectée. **Matching conservateur** (risque diffamatoire) ; **ADR-012** ; descriptif ; **jamais d'avis juridique**.

**Accessibilité (ADR-008)** : listes pleinement accessibles, mention « à vérifier » explicite (jamais portée par la seule couleur), navigation clavier.

**Modèle économique (ADR-006)** : recherche / rapprochement **déterministe**, API PISTE **gratuite** → **cœur / OSS**. Option **premium** : **résumé descriptif** d'une décision par IA (patron F-050).

**Découpage / jalons** :
1. **Adapter PISTE + recherche à la demande** par dénomination → section dossier F-056.
2. **Mode événement** : polling au patron `IItemStreamMonitor` → `FavoriteEvent JudilibreDecision` → timeline.
3. **Premium (option)** : résumé descriptif de décision (F-050).

**Décisions ouvertes** :
- **Strictness du rapprochement** : seuil et traitement des variantes de raison sociale.
- **Périmètre de corpus** affiché.
- **Fréquence** du mode événement.
- **Résumé premium** : dans le périmètre ou non ?

---

### F-057 — Re-screening continu (surveillance des sanctions)

> **Statut** : V3+, **candidate naturelle au tout début de V3**. Surnom de travail : « le F-019 des sanctions ». Origine : graine du persona **Compliance / KYC**.
> **Doctrine** : sous **ADR-012** (descriptif, matching conservateur, aucun verdict) — incarnation par **ADR-014** (`INameAgainstListMatcher`). Hérite des frontières dures de F-055. **Substrat technique** : **ADR-013** (`IStateMonitor<SanctionsScreeningState>`, retraits détectés).

**Description** : une **surveillance continue** des entités **déjà suivies** (favoris F-017 / watchlists F-053) contre les **listes officielles de sanctions** (DG Trésor + liste consolidée UE, sources de F-055). Quand une entité **apparaît** (ou **disparaît**) d'une liste au fil de ses mises à jour, Atlas le **détecte et l'alerte** — timeline + push — avec une **piste d'audit** complète (date, liste, entrée, base du rapprochement).

C'est le pendant **temporel** de F-055 : F-055 répond à « cette entité est-elle sur une liste *maintenant* ? » ; F-057 répond à « **préviens-moi quand ça change** ». Mécanisme de F-019 (snapshot + diff + job) appliqué à l'**état de correspondance sanctions**.

**Valeur user** : pour le persona **Compliance / KYC**, le **re-screening** est une **obligation LCB-FT cœur**. Le faire en continu, **souverainement**, **descriptivement** et avec une **piste d'audit défendable**, c'est exactement le positionnement qu'aucun fournisseur cloud à score boîte noire ne sert proprement.

**Complexité** : ★★★ — peu de neuf : réutilise le snapshot/diff de **F-019**, le patron polling + dédup cross-users de **F-048**, les sources et le matching de **F-055**, la timeline **F-047**, le push **F-020**. Le délicat n'est pas le code, c'est la **conservativité du matching** et le **cadre légal**.

**APIs externes** : celles de **F-055** — **Registre national des gels DG Trésor** + **liste consolidée UE**. Aucune nouvelle source.

**Dépendances** : **F-055** (sources + logique de rapprochement), **F-019** (patron snapshot/diff + job), **F-047** (`FavoriteEvent` + timeline), **F-048** (patron polling + dédup `ExternalId`), **F-053/F-017** (le set suivi), **F-046** (`WatchRule` d'activation), **F-020** (push), **ADR-012** (doctrine), **ADR-013** (substrat — c'est la **3ᵉ instance** qui déclenche l'extraction effective du runner), **ADR-014** (`INameAgainstListMatcher`).

**Hors-périmètre (explicite)** :
- **Pas de score, pas de verdict, pas de label « à risque »** (ADR-012). Une correspondance = **« correspondance potentielle à vérifier »**, jamais une affirmation.
- **Pas de PEP ni d'adverse-media propriétaire** ; **pas de bénéficiaires effectifs**. Le périmètre reste celui de F-055.
- **Pas de certification de conformité** — Atlas alerte, l'entité assujettie reste responsable de sa décision.
- **v1 = entités légales** uniquement (par dénomination). Personnes physiques (dirigeants) hors v1 — diffamation trop risquée.

**Détails techniques** :
- **4ᵉ volet de la timeline**, dans la lignée de F-048 ; **ADR-013** appliqué :
  - **`IStateMonitor<SanctionsScreeningState>`** (Domain) — état = ensemble des correspondances courantes (formes normalisées / hash, comme le hash dirigeants de F-019).
  - **Diff** : `DiffWith(...)` retourne les correspondances **ajoutées** et **levées** (retraits → c'est pourquoi `IStateMonitor` et non `IItemStreamMonitor`).
  - **Runner mutualisé** (Application, ADR-013) : itère le set, dédup cross-users (un seul calcul de rapprochement par SIREN partagé), isolation des échecs, idempotence.
  - **Job Hangfire** `sanctions-rescreening`, cron `0 5 * * *` (après `favorite-refresh` 03:00 et `bodacc-polling` 04:00).
  - **Événement** : nouveau type `FavoriteEvent` `SanctionsScreeningChanged` (`ExternalId` = identifiant d'entrée de liste + version).
- **Listes locales rafraîchies** : cache local des listes officielles ; le re-screening tourne contre le cache.
- **Matching conservateur** (ADR-014, `INameAgainstListMatcher`) : sortie en `MatchCandidate` toujours formulée « **à vérifier** ».
- **Audit** : les événements sont **persistés** (date, liste, entrée, base du rapprochement) — c'est la valeur compliance, à exposer comme **journal défendable**.

**Cadre légal** : listes **officielles, gratuites, publiques** (DG Trésor, UE) → réutilisation OK. Le rapprochement manipule des **données personnelles** et touche à des allégations sensibles → **ADR-012**, matching conservateur, et l'utilisateur compliance est **responsable de traitement** de son screening (DPIA probable côté usage). **Aucune certification AML**.

**Accessibilité (ADR-008)** : alertes pleinement lisibles au lecteur d'écran, mention « correspondance potentielle à vérifier » explicite (jamais portée par la seule couleur), journal d'audit consultable de façon accessible.

**Modèle économique (ADR-006)** : déterministe contre listes gratuites → **cœur / OSS**. Quotas hébergé possibles (nombre d'entités sous surveillance), patron de F-043.

**Découpage / jalons** :
1. **Snapshot + diff** : `SanctionsScreeningSnapshot` + `DiffWith`, sur le rapprochement de F-055.
2. **Job + événement** : `sanctions-rescreening` → `FavoriteEvent SanctionsScreeningChanged` (détectée / levée), dédup `ExternalId`.
3. **Activation par règle** : `WatchRule` « surveillance sanctions » au niveau favori / watchlist (F-046).
4. **Journal d'audit** : exposition consultable et exportable.

**Décisions ouvertes** :
- **Opt-in par watchlist vs automatique pour tous les favoris** (recommandé : opt-in).
- **Personnes physiques** : v1 entité seule ; cas dirigeants à examiner plus tard sous garde-fou renforcé.
- **Levées (délistage)** : alerter aussi recommandé.
- **Stockage d'audit** : `FavoriteEvent` ou journal de screening dédié ?
- **Fréquence** : quotidienne (aligne F-019/F-048) ou calée sur la mise à jour réelle des listes ?
- **`FavoriteEvent` → `EntityEvent`** : décision ouverte d'ADR-013 à trancher au moment de l'extraction du runner.

---

### F-058 — Signaux concurrentiels & digest sectoriel

> **Statut** : V3+, **s'allume progressivement** — le volet « signaux PI » dépend de F-027 (V2). Origine : graine du persona **Veille concurrentielle B2B**.
> **Doctrine** : descriptif — on **fait remonter des faits**, on ne classe pas les concurrents par « menace ». Matching presse sous **ADR-014** (`INameInTextMatcher`).

**Description** : deux volets complémentaires posés sur la timeline mixte (F-047) :

1. **Signaux concurrentiels de premier rang** — élever les événements **structurés** au rang de signaux **typés** : un **dépôt PI** (F-027), un **marché public remporté** (F-032), un **événement légal** (BODACC F-048, RNE F-019) deviennent des `FavoriteEvent` **classés par type de signal**, filtrables (« montre-moi les mouvements **produit** = PI » vs « leurs mouvements **commerciaux** = marchés »).
2. **Digest sectoriel** — une **synthèse périodique** (hebdo par défaut) « ce qui a bougé dans mon secteur » sur une **watchlist** : un récapitulatif groupé des signaux de la semaine, livré en email / push / in-app.

**Valeur user** : pour le persona **Veille concurrentielle B2B** — transformer une timeline brute en **intelligence actionnable** (signaux typés + briefing hebdomadaire) de façon **souveraine** et **descriptive**, là où les plateformes CI (Crayon, Klue, Contify) font de l'auto-résumé IA sur l'empreinte **web**. Atlas le fait sur la **donnée officielle**.

**Complexité** : ★★★ — réutilise la timeline F-047, le patron `FavoriteEvent`, la dédup F-045, les canaux de notification et les ports premium F-050. Le neuf : la **taxonomie de signaux**, le **job de digest** + le groupement, et le **branchement de la synthèse IA**.

**APIs externes** : aucune nouvelle. Réutilise les sources de **F-027** (INPI/EUIPO/OMPI PI), **F-032** (DECP), **F-048** (BODACC), F-041 (RSS).

**Dépendances** : **F-047** (timeline + `FavoriteEvent`), **F-027** (dépôts PI — V2, conditionne le volet « signal produit »), **F-032** (DECP — marchés), **F-048/F-019** (événements légaux), **F-053** (watchlist = mes concurrents), **F-050** (`IFeedSummarizer` pour la synthèse premium), **F-046** (filtres/règles), **F-020** (push), **ADR-014** (matching presse `INameInTextMatcher`).

**Hors-périmètre (explicite)** :
- **Pas de score de menace, pas de classement concurrentiel, pas de verdict stratégique** : le digest résume des faits, il ne conseille pas.
- **Pas de scraping** des sites / prix / offres d'emploi des concurrents (gap assumé face à Crayon — le modèle RSS + officiel ne scrape pas).
- **Pas de battlecards ni de win/loss** (territoire des plateformes CI) — hors sujet.

**Détails techniques** :
- **Taxonomie** : étendre les types de `FavoriteEvent` avec des **signaux concurrentiels** — `IpFiled` (depuis F-027), `PublicContractAwarded` (depuis F-032), en plus de `RneChanged` / `BodaccPublished` existants. Brancher F-027 et F-032 pour **émettre** ces events dans la timeline (patron F-048, `ExternalId` pour la dédup).
- **Filtrage** : extension des filtres de timeline / des règles F-046 — `?signalTypes=IpFiled,PublicContractAwarded`.
- **Digest** : job Hangfire récurrent (p. ex. `sector-digest-weekly`) qui, par user/watchlist **opt-in**, agrège les `FavoriteEvent` de la période pour les SIREN de la liste, les **groupe** par entité / type de signal, et produit une `Notification` digest.
  - **Cœur** : récap déterministe groupé.
  - **Premium** : mise en récit IA via `IFeedSummarizer` (patron F-050) — **descriptive** (« X a déposé 3 marques et remporté le marché Y »), jamais d'interprétation (« X est en train de gagner »).
- Réutilise la **timeline de watchlist** (F-053) comme socle d'agrégation.

**Cadre légal** : faits **publics/officiels** (dépôts, marchés, annonces légales) → descriptif. **Matching presse conservateur** sous **ADR-014** (homonymes de raison sociale, volet 1 de F-047). Données de dirigeants sous **ADR-012**. Aucun traitement sensible nouveau.

**Accessibilité (ADR-008)** : digest **lisible** (email + in-app), type de signal **jamais** porté par la seule couleur (libellé explicite), filtres de timeline accessibles au clavier et annoncés au lecteur d'écran.

**Modèle économique (ADR-006 + ADR-009)** : typage des signaux + digest **déterministe** = **cœur / OSS** (cohérent open-core). **Synthèse narrative IA** = **premium** (coût LLM, port F-050). Quotas hébergé possibles (taille de watchlist surveillée, fréquence).

**Découpage / jalons** :
1. **Taxonomie + branchements** : types de signaux, émission depuis F-032 (marchés) puis F-027 (PI) en `FavoriteEvent`.
2. **Filtrage** par type de signal sur la timeline.
3. **Digest déterministe** par watchlist (job + `Notification` groupée).
4. **Synthèse IA premium** (F-050).

**Décisions ouvertes** :
- **Cadence du digest** : hebdo par défaut, configurable (quotidien/mensuel) ?
- **Portée** : par watchlist uniquement, ou un digest global « tous mes suivis » ?
- **Signal = classification persistée** sur l'event, **ou** simple couche de présentation au-dessus des types existants ?
- **Opt-in** du digest (recommandé) et choix du canal.

---

### F-059 — Veille d'échéances PI (docketing assistif souverain)

> **Statut** : **V3+ conditionnel** — conditionnée non par une DPIA (comme F-034) mais par un **cadrage de responsabilité** explicite : c'est le prérequis bloquant ici. Origine : graine du persona **Cabinet de Propriété Industrielle** — la fonction **la plus à enjeu** du métier.
> **Frontière fondatrice** : Atlas **fait remonter** des échéances de façon **assistive** ; il n'est **pas** le registre de docketing autoritaire du cabinet et ne **garantit aucune** date. C'est l'équivalent docketing du « pas d'avis juridique ».

**Description** : une **couche de veille des échéances** posée sur le portefeuille PI (F-018/F-025) : renouvellements de marques, annuités de brevets, fenêtres d'opposition, dates de priorité. Atlas **calcule/lit** les échéances à venir et **rappelle** l'utilisateur à des délais configurables — **souverainement** (auto-hébergeable) et **ouvertement** (règles open-source).

C'est **assistif**, pas autoritaire : Atlas aide à ne rien rater, mais le **devoir de docketing reste celui du cabinet**, et aucune date n'est garantie.

**Valeur user** : pour le **Cabinet PI**, l'oubli d'une échéance est la faute cardinale. Aujourd'hui le marché est binaire — suites fermées chères grands comptes (Anaqua, PATTSY) **ou** docketing open-source isolé (phpIP) non relié au registre vivant. Un docketing **assistif, souverain, ouvert et connecté** à la donnée et à la veille n'existe pas.

**Complexité** : ★★★★ — le volume de code n'est pas énorme, mais l'**ingénierie de prudence** l'est : préférence lecture / calcul, signalement des dates calculées, moteur de règles par juridiction **gouverné**, et une **UX qui rend la non-autorité impossible à manquer**.

**APIs externes** : celles de F-018/F-027 — **INPI PI**, **EUIPO** (TMview/eSearch), **OMPI**. Aucune nouvelle — on lit les dates des notices déjà récupérées.

**Dépendances** : **F-018** (favoris PI), **F-025** (portefeuille IP), **F-007/F-016** (notices marque/brevet portant les dates), **F-019** (patron job + rappels), **F-047** (timeline), **F-020** (push), **F-046** (`WatchRule`/`Alert`).

**Hors-périmètre (le cœur de la prudence)** :
- **Pas le registre de docketing autoritaire** : Atlas ne se substitue pas au système de référence du cabinet.
- **Aucune garantie** d'exhaustivité ni d'exactitude des dates ; le cabinet **conserve son devoir** professionnel.
- **Aucune action** : pas de renouvellement automatique, pas de paiement d'annuité, pas de dépôt — Atlas **rappelle**, il n'**agit** pas.
- **Couverture honnêtement limitée** aux juridictions dont la date est lue ou la règle encodée (démarrer **FR / UE**).

**Détails techniques** :
- **Modèle** : `IpDeadline` (type : `TrademarkRenewal` / `PatentAnnuity` / `OppositionWindow` / `PriorityDeadline`), portant **la date, sa source (lue vs calculée), et la base de calcul** le cas échéant.
- **Préférence lecture > calcul** : si la notice fournit la date d'expiration, on la **lit** (pas de risque de règle erronée) ; sinon **calcul** via la règle de juridiction, **marqué « calculé — à vérifier »**.
- **Moteur de règles** : règles de renouvellement/annuité **encodées par juridiction**, comme un **artefact open-source auditable** et **extensible par la communauté** — mais **gouverné/curé**, car une règle fausse est dangereuse.
- **Job Hangfire** : calcule les échéances à venir du portefeuille, émet des **rappels** (`Alert` issue d'une `WatchRule`) aux délais configurés → timeline + email + push.
- **Auditabilité** : chaque échéance affiche **d'où elle vient** (date lue de la notice, ou règle + base de calcul + date) — défendable et vérifiable.

**Cadre légal & responsabilité (prérequis bloquant)** :
- **Disclaimer de non-autorité au niveau du design** (pas en petites lignes) : outil d'**assistance**, le cabinet reste responsable de son docketing ; aucune garantie.
- **Dates calculées toujours marquées** comme telles et « à vérifier ».
- **Couverture juridictionnelle affichée** clairement (ce qui est couvert / ce qui ne l'est pas).
- Données de titres dépendant de l'INPI PI (auth complexe, lacunes possibles) → ne jamais présenter une absence de donnée comme « aucune échéance ».

**Accessibilité (ADR-008)** : échéancier et rappels pleinement accessibles ; l'**urgence** (proche de l'échéance) **jamais** signalée par la seule couleur (libellé + date explicites) ; configuration des délais navigable au clavier.

**Modèle économique (ADR-006)** : lecture/calcul **déterministe** → **cœur / OSS**. Le **moteur de règles** est open-source (et communautaire, sous gouvernance). **Quotas hébergé** possibles (taille de portefeuille surveillé).

**Découpage / jalons** :
1. **Lecture + rappels** sur les **renouvellements de marques FR/UE** dont la date figure dans la notice. *(Le plus sûr, livrable autonome.)*
2. **Annuités de brevets** (lecture quand fournie).
3. **Calcul en repli gouverné** : règles par juridiction, dates marquées « calculées ».
4. **Fenêtres d'opposition / dates de priorité** (les plus courtes, les plus critiques — en dernier, avec le plus de prudence).

**Décisions ouvertes** :
- **Lecture vs calcul** : jusqu'où encoder des règles (risque) plutôt que se limiter aux dates **lues** ?
- **Juridictions de la v1** : FR + UE seulement ?
- **Gouvernance des règles communautaires** : qui valide une règle avant publication ?
- **Délais de rappel par défaut** et personnalisation.
- **UX de non-autorité** : comment rendre le disclaimer **impossible à ignorer** sans alourdir l'usage ?

---

### F-033 — Indicateurs environnementaux descriptifs

> **Reframé & figé le 29 mai 2026** — remplace le stub V3+ « Score ESG / Bilan carbone ». **Le mot « score » est abandonné** : le scoring ESG est une industrie controversée et non normalisée. Atlas **affiche la donnée déclarée**, il n'invente pas de note (même logique que F-054 : ratios ≠ notation). Statut **V3+ *data-gated*** : la donnée n'est pas encore là à grande échelle.

**Description** : sur la fiche d'une entreprise, Atlas affiche ses **indicateurs environnementaux déclarés** quand ils existent — au premier chef ses **émissions de gaz à effet de serre** (scopes 1, 2, 3) issues du **BEGES** publié en open data par l'**ADEME**. Descriptif, factuel, jamais agrégé en un score propriétaire.

**Valeur user** : pour les personas **investisseur** et **compliance**, et la pression ESG croissante des donneurs d'ordre — voir l'empreinte déclarée d'une entreprise sans aller fouiller des rapports. Mais c'est aujourd'hui **data-limité** (voir couverture).

**Pourquoi V3+ *data-gated*** (et pas promu comme le DECP) : le paquet **Omnibus** (en vigueur le 18 mars 2026) a **resserré la CSRD** — seuils relevés à **1000 salariés ET 450 M€** de CA, **~80 %** des entreprises initialement visées sorties du champ, PME cotées exclues, vagues suivantes repoussées à **2028** (exercice 2027). Conséquence : la donnée ESG issue de la CSRD reste **rare et repoussée**. Le motif d'origine de F-033 (« à revoir quand la CSRD sera déployée ») est **renforcé**, pas levé. **Mais** une donnée est exploitable **dès maintenant**, indépendante de la CSRD : le **BEGES**, obligatoire pour les entreprises de **plus de 500 salariés** (scopes 1, 2 et 3 significatif), publié en **open data par l'ADEME**. C'est la porte d'entrée pragmatique.

**Complexité** : ★★★, **conditionnée à la disponibilité de la donnée**. Le travail est l'ingestion ADEME + l'affichage descriptif ; le facteur limitant est la couverture, pas le code.

**APIs externes** :
- **ADEME — Bilan GES** (open data) : émissions déclarées des entreprises françaises > 500 salariés. Source réaliste **aujourd'hui** (déjà cataloguée doc 03 §8.2).
- **Futur** : rapports CSRD (vague 1 déjà publiés par les très grandes entreprises) et, à terme, l'**ESAP** (European Single Access Point) qui agrégera les données de durabilité.

**Dépendances** : F-004 (fiche entreprise), ADR-006 (open core), ADR-004 (archi hexagonale).

**Hors périmètre (explicite)** :
- **Aucun score / notation / rating ESG** propriétaire.
- **Aucune donnée inventée** ou estimée là où rien n'est déclaré.

**Détails techniques** :
- Adapter d'ingestion des données ADEME BEGES, rapprochées par **SIREN**.
- Affichage descriptif (émissions par scope, évolution si plusieurs millésimes).
- Déterministe → **cœur open source**, aucun coût par utilisateur.
- Aucune dépendance lourde ; persistance EF Core / PostgreSQL existante.

**Couverture (caveat honnête)** : seules les entreprises **assujetties au BEGES** (> 500 salariés) déclarent → **la grande majorité des entreprises n'aura aucune donnée**. À afficher proprement (« non disponible »), comme la confidentialité pour F-054 ou le caractère coté pour F-051. C'est ce qui justifie le statut **data-gated**.

**Cadre légal & positionnement** : open data public, descriptif (données déclarées) → risque faible, pas de RGPD spécifique. Même ligne que le financier : on **montre la donnée**, on ne **note** pas.

**Accessibilité (rappel ADR-008)** : émissions et évolutions en tableau lisible au lecteur d'écran ; aucune information (ex. tendance) transmise par la couleur seule.

**Modèle économique** : déterministe, source gratuite → **cœur open source** (cohérent ADR-006).

**Découpage / jalons** :
1. **ADEME BEGES** : ingestion + matching SIREN + affichage descriptif des émissions. *(Cœur de la feature, livrable seul.)*
2. **Enrichissement futur** : rapports CSRD / ESAP quand la donnée sera disponible à plus grande échelle.

**Décisions ouvertes** :
- **Quand rouvrir** : suivre la maturité de la donnée (calendrier CSRD post-Omnibus, déploiement ESAP).
- **Autres jeux ADEME** éventuels à intégrer.

---

### F-034 — Graphe de co-mandats des dirigeants (descriptif)

> **Reframé & figé le 29 mai 2026** — remplace le stub V3+ initial (« détection de sociétés écrans / conflits d'intérêt »). Ce cadrage est **abandonné** : inférence accusatoire → profilage + diffamation (cf. ADR-012). On ne garde que le **graphe de co-mandats descriptif**, **conditionnel à la DPIA**.

**Description** : autour d'une entreprise que l'utilisateur **consulte ou suit**, Atlas affiche un **graphe de co-mandats** — les dirigeants de l'entreprise, leurs autres mandats, et les entreprises qui **partagent un dirigeant** — sur une profondeur limitée (1 à 2 sauts). Strictement **descriptif** : Atlas montre des **faits du registre**, jamais une qualification (« société écran », « conflit d'intérêt ») et **n'attribue aucun score de risque** à une personne. L'utilisateur explore et tire ses propres conclusions.

**Valeur user** : pour la due diligence et la compréhension des liens entre entreprises suivies (personas **Investisseur / M&A**, **veille concurrentielle**) — voir d'un coup d'œil « qui est aussi aux commandes d'où ». Outil d'**exploration de faits publics**, pas de jugement.

**Pourquoi V3+ conditionnel** : **mise en service bloquée tant que la DPIA n'est pas réalisée** (ADR-012, gating dur). Le poids n'est pas la traversée du graphe (triviale en SQL borné) mais la couche légale (DPIA + LIA + opposition + transparence), la **résolution d'identité conservatrice** (homonymes), et l'**accessibilité** d'un graphe (ADR-008, critère bloquant).

**Complexité** : ★★★★

**APIs externes** : **INPI RNE** (dirigeants — déjà récupérés ; F-019 n'en stocke qu'un hash, cette feature nécessite de stocker les **identités** dans le voisinage borné → **nouveau traitement**, gaté par ADR-012). Éventuellement le jeu open data « dirigeants » pour constituer le voisinage.

**Dépendances** : **ADR-012** (cadre RGPD — prérequis impératif), F-004, F-019, F-017 / F-053 (favoris & watchlists bornent le périmètre), ADR-004, ADR-008.

**Hors périmètre (explicite — voir ADR-012)** :
- **Aucune qualification ni inférence** (pas de « société écran », pas de « conflit d'intérêt », pas de score de risque sur une personne).
- **Aucune agrégation massive** exposée publiquement (pas de base graphe de tous les dirigeants de France). Une version « massive » future exigerait **un nouvel ADR** et une DPIA bien plus lourde.
- **Aucun bénéficiaire effectif** (régime restreint, CJUE Sovim).

**Détails techniques** :
- **Modèle (PostgreSQL, pas Neo4j)** : deux tables **nœuds** (`Personne`, `Entreprise`) et **arêtes** (`Mandat` : qui dirige quoi, à quel titre, depuis quand). Traversée bornée par **`WITH RECURSIVE`** (1–2 sauts), performant à cette échelle, **déjà dans la stack** — aucune nouvelle infrastructure.
- **Construction à la demande** autour de l'entreprise focus (éphémère ou cache — décision ouverte).
- **Résolution d'identité conservatrice** (ADR-012 §5) : ne relier qu'avec confiance élevée (nom + **date de naissance** du RNE), afficher l'incertitude sinon. *Pas de lien vaut mieux qu'un lien erroné.*
- **Architecture (ADR-004)** : réutilise `ICompanyDataProvider` (RNE) ; service de domaine de construction du voisinage borné + service de résolution d'identité ; persistance EF Core / PostgreSQL existante.

**Cadre légal & positionnement** : entièrement régi par **ADR-012**. Descriptif uniquement ; base légale **intérêt légitime** (réutilisation open data, cadre CNIL) avec LIA ; **DPIA prérequis de mise en service** ; **droit d'opposition / effacement** + note de transparence (art. 14) ; résolution d'identité conservatrice.

**Accessibilité (ADR-008, bloquant)** : un graphe visuel est hostile au lecteur d'écran et au daltonisme. **Obligatoire** : équivalent **tabulaire/textuel** complet (« X détient des mandats dans A, B, C ») et aucune information transmise par la couleur ou la position seules. À concevoir dès le départ.

**Modèle économique** : construction déterministe, aucun coût d'inférence par utilisateur → **cœur open source** par défaut (ADR-006). Limite de profondeur/quota possible en hébergé.

**Découpage / jalons** :
1. **Couche légale (prérequis, ADR-012)** : DPIA + LIA + mécanisme d'opposition/effacement + note de transparence. **Bloquant — rien ne se livre avant.**
2. **Modèle & résolution** : tables nœuds/arêtes, résolution d'identité conservatrice (nom + date de naissance, seuil de confiance).
3. **Traversée bornée & API** : construction à la demande (1–2 sauts), endpoint dédié.
4. **Visualisation + équivalent accessible** : rendu graphe **et** vue tabulaire/textuelle.

**Décisions ouvertes** :
- **Profondeur** : 1 ou 2 sauts ?
- **Persistance** : graphe éphémère (reconstruit à chaque consultation) ou mis en cache ?
- **Source du voisinage** : RNE live vs jeu open data « dirigeants ».
- **Cœur vs premium** : déterministe donc cœur par défaut, mais feature lourde — à confirmer.

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

### F-060 — Vérificateur de présence d'un nom (multi-sources)

> **Figée le 30 mai 2026**. Statut **V3+ — Won't have (yet)**. Doctrine **ADR-012 / ADR-014** : Atlas rapporte des faits **sourcés et datés**, **jamais un verdict de disponibilité**. Cf. fiche dédiée [`feature-verificateur-nom-F060.md`](feature-verificateur-nom-F060.md).

**Description** : depuis la Recherche, un mode « Vérifier un nom » (3ᵉ intention de recherche — pas de 6ᵉ onglet) où l'utilisateur saisit un nom (marque, enseigne, dénomination envisagée) et obtient un **rapport multi-sources** : marques (INPI PI), dénominations d'entreprises (RNE), domaines web (WHOIS/DNS) — chacun avec sa provenance, sa date et son périmètre — plus une liste de **noms proches à vérifier**. Ne rend **jamais** de verdict (« disponible », « libre ») : toute conclusion passe par le contrat `MatchCandidate` (ADR-014), dont la disposition ne peut être que « à vérifier ».

**Valeur user** : les outils existants ne couvrent qu'une source (checker de domaine isolé) ou affichent un verdict irresponsable. Atlas agrège plusieurs registres et reste descriptif — utile avant un dépôt de marque ou une création d'entreprise, sans se substituer au conseil PI.

**Complexité** : ★★★.

**APIs externes** : INPI PI (déjà intégrée, F-006), INPI RNE (déjà intégrée, F-004/F-005), **WHOIS/DNS (nouvelle source à évaluer)**.

**Dépendances** : F-006, F-005, F-004, F-026/ADR-014 (`ITrademarkSimilarityMatcher` + noyau de normalisation de noms), F-061 (pendant temporel).

**Pourquoi reporté (V3+)** : (1) **source externe nouvelle** (WHOIS/DNS) hors du socle INPI/INSEE/BODACC — à évaluer (fournisseur, coût, rate-limits) ; (2) **sensibilité juridique maximale** — l'écran où un mauvais cadrage ferait le plus de dégâts, à livrer avec rigueur doctrinale. À reconsidérer selon la traction (persona Cabinet PI).

**Détails techniques** : port `INameAvailabilityProbe`, adapters `InpiTrademarkPresenceAdapter` / `RneDenominationPresenceAdapter` / `WhoisDomainAdapter` (nouveau, `Atlas.Infrastructure.Whois`), use case `CheckNamePresence` (requêtes parallèles bornées, DTO discriminé par source — pas de fusion en indicateur unique). Cf. fiche dédiée.

---

### F-061 — Surveillance continue d'un nom

> **Figée le 30 mai 2026**. Statut **V3+ — Won't have (yet)**. **Pendant temporel de F-060**. Doctrine **ADR-014** : sortie = `MatchCandidate` « à vérifier », jamais « conflit avéré ». Cf. fiche dédiée [`feature-surveillance-nom-F061.md`](feature-surveillance-nom-F061.md).

**Description** : surveillance continue d'un nom — soit un **nom libre** saisi (projet de marque non déposé : nouveau concept de « surveillance de nom »), soit une **entité déjà suivie** (via une `WatchRule`, patron F-046). Atlas détecte dans le temps l'**apparition** d'éléments proches : nouveaux dépôts de marques (INPI/BOPI), nouvelles dénominations (RNE), nouveaux domaines (WHOIS) — et **alerte** (timeline F-047 + push F-020) avec **piste d'audit** (date, source, élément, base du rapprochement). Là où F-060 répond « ce nom est-il pris *maintenant* ? », F-061 répond « **préviens-moi quand quelque chose de proche apparaît** ».

**Valeur user** : pour le persona Cabinet PI et les créateurs, une surveillance **multi-sources** (marque + société + domaine), **souveraine** et **descriptive** — « correspondance à vérifier », jamais « conflit avéré ».

**Complexité** : ★★★★ — l'essentiel est déjà fait ailleurs (snapshot/diff F-019, polling F-048, similarité F-026, timeline F-047, push F-020) ; le neuf : concept de « surveillance de nom libre », polling filtré par similarité, maîtrise du bruit.

**APIs externes** : celles de F-060 (INPI PI/BOPI, RNE, WHOIS/DNS).

**Dépendances** : F-060, F-026/ADR-014, F-019 (snapshot+diff+job), F-047 (timeline / `FavoriteEvent`), F-048 (polling+dédup `ExternalId`), F-046 (`WatchRule`), F-020 (push), F-017/F-053 (entités suivies).

**Pourquoi reporté (V3+)** : dépend de F-060 + évaluation WHOIS + maîtrise du bruit. Réutilise massivement l'existant.

**Détails techniques** : snapshot `NameWatchSnapshot` (par cible : nom libre ou `(UserId, entité)`), `DiffWith(...)` (éléments apparus / disparus), job Hangfire `name-watch` (cron décalé, ex. `0 6 * * *`), nouvel événement `FavoriteEvent` de type `NameMatchAppeared` (`ExternalId` = dépôt/dénomination/domaine + source, pour dédup). Cf. fiche dédiée.

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
