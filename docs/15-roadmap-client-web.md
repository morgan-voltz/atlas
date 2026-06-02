# Roadmap — client (Uno) & historique du client web transitoire

> ✅ **ADR-029 (UI unifiée Uno Platform)** — La cible est désormais **`Atlas.App` (Uno)**, une UI unique pour six surfaces (le web devient la **tête WebAssembly**). Ce document a **deux parties** : (1) l'**historique** de ce qui a été livré sur le **client web Blazor transitoire** (`Atlas.Web.Client`) — M0–M7 + kit de composants, **conservé tel quel** comme référence fonctionnelle et acquis réutilisable ; (2) la **bascule vers Uno** (§7). Le client Blazor reste l'app web **en vigueur jusqu'à un spike Uno multi-cible concluant** (garde-fou ADR-029). Doctrine UX (`docs/12`) inchangée ; delta multi-surface = `docs/14` (refondu Uno).

> Feuille de route **d'implémentation** du client. **§1–§6 = historique du client web Blazor transitoire** (`Atlas.Web` / `Atlas.Web.Client`) ; **§7 = bascule Uno** (`Atlas.App`).
> Document **distinct** du backlog produit (`docs/02-roadmap-features.md`, `F-NNN`) et de la doctrine
> UX (`docs/12` tronc commun + `docs/14` déclinaison multi-surface). Ici : **comment on bâtit le client**.

**Version** : 2.1 — **Date** : 2 juin 2026 — **Portée** : historique `Atlas.Web.Client` (transitoire) + cible `Atlas.App` (Uno).
**Cadre** : **ADR-029** (UI unifiée Uno, WASM pur pour la tête web ; remplace ADR-017/ADR-026), **ADR-002** (client pur de l'API), **ADR-008 / doc 06** (accessibilité, bloquante).

Légende : ✅ fait & vérifié · 🟡 partiel · ⬜ à faire.

**Principe directeur — tranches verticales, pas couches horizontales.** On livre **un écran de bout en
bout** (UI + données + auth + états + accessibilité) avant de passer au suivant : du fonctionnel tôt,
l'intégration dé-risquée au plus tôt. Le **kit de composants s'extrait au fil des écrans** (pas tout en
amont). Chaque écran est « fini » au sens de sa **Definition of Done** (§3), pas avant.

---

## 1. Cadre (historique — client Blazor transitoire)

> Cette section et les §2–§6 documentent le client web **Blazor** tel que bâti en MVP 1. Avec **ADR-029**, ce client est **transitoire** : il reste en vigueur jusqu'au spike Uno concluant, puis cède la place à `Atlas.App` (Uno — cf. §7). On le conserve ici comme **acquis** (parcours, contrats API, kit, vérifications E2E) directement réutilisable sous Uno.

Le client web est une **surface** d'un même produit (R1) : 90 % de la doctrine vient de `docs/12`
(agnostique) ; `docs/14` décrit la déclinaison multi-surface (rail, list-detail 2 panneaux, routing/URLs propre à la tête WASM).
Topologie : **consommateur HTTP pur** de `Atlas.Api` (ADR-002) ; `Atlas.Web.Client` ne référence que
`Domain` + `Shared` (verrouillé par NetArchTest) — règle reprise telle quelle par `Atlas.App`. Maquettes de référence : `docs/design/phone_mode/`.

**Fondations — ✅ faites** (30-31 mai 2026, PR #81-#85, sur le client Blazor transitoire) : décision framework d'alors (ADR-017, WASM pur — depuis remplacé par ADR-029),
modèle UX (doc 14), scaffold `Atlas.Web`/`Atlas.Web.Client` (CPM, NetArchTest), squelette de
navigation (rail 5 destinations) + routing + page 404.

---

## 2. Jalons (tranches verticales)

Chaque jalon = une tranche verticale livrant de la valeur utilisable de bout en bout.

| Jalon | Contenu | Pourquoi en premier / dépendances | Statut |
|---|---|:--:|:--:|
| **M0 — Fondations** | scaffold + navigation/routing | socle (cf. §1) | ✅ |
| **M1 — Connexion + Recherche** | login (token mémoire + refresh cookie HttpOnly) **et** écran Recherche de bout en bout | 1ʳᵉ tranche : valide toute la chaîne — `AtlasApiClient`, auth, garde de route, premiers composants (carte-aperçu, états) | ✅ |
| **M2 — Fiche entreprise** | depuis un résultat, fiche en cartes-sections, provenance épinglée, états de couverture | réutilise carte-section + le client API de M1 | ✅ |
| **M3 — Accueil / feed** | fil des mouvements des entités suivies (cartes-aperçu *event*) | dépend des favoris (lecture) ; garde-fous anti-« réseau social » | ✅ |
| **M4 — Favoris / Watchlists + list-detail 2 panneaux** | gestion des favoris **et** introduction du **list-detail à 2 panneaux** (signature desktop, réutilisé ensuite) | la signature desktop (doc 14 §3) arrive ici puis se généralise | ✅ |
| **M5 — Veille** | flux, palier de lecture, pont vers fiche « à vérifier » | réutilise list-detail (M4) | ✅ |
| **M6 — Profil & compte** | profil, sous-pages compte / connexion INPI / données (RGPD) | — | ✅ |
| **M7 — Onboarding complet** | création de compte, vérification email, défi 2FA, proposition INPI | complète l'auth minimale de M1 | ✅ |

> L'ordre est indicatif et révisable ; on ne démarre un jalon que quand le précédent atteint sa DoD.

---

## 3. Écrans & parcours — Definition of Done

Un écran n'est **✅** que lorsque **toutes** ses colonnes le sont. DoD = template commun :

> **maquette** conforme à `docs/design` · **données** câblées à l'API · **états** (chargement / vide / erreur) ·
> **responsive** (reflow par largeur, doc 14 §5) · **a11y** (doc 06, bloquant) · **clavier** (focus, raccourcis) · **URL/titre** de page.

| Écran | Jalon | Maquette | Données | États | Responsive | A11y | Clavier | URL/titre | Statut |
|---|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| Recherche (cœur M1 ; « Vérifier un nom » F-060 = backlog séparé) | M1 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Fiche entreprise (Identité + Dirigeants ; Bilans/BODACC/Étabts/PI = features à venir) | M2 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Accueil / feed | M3 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Favoris (plats + list-detail 2 panneaux ; watchlists/tags F-053/F-030 = backend différé) | M4 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Veille | M5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Profil hub + Connexion INPI + Données/RGPD (Compte/2FA/préférences = à venir) | M6 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Auth / onboarding | M1/M7 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

Renvois doctrine par écran : Recherche doc 12 §7 ; Fiche §4 ; Accueil §5 ; Favoris §8 ; Veille §6 ; Profil §9 ; Auth §10.

> **Vérification E2E M7 (31 mai 2026)** — Onboarding validé en navigateur (Playwright), parcours réels : **Inscription** (`/inscription`, mdp ≥ 12 visible) → **Vérification email** (`/verifier-email` : état d'attente honnête puis activation via lien `userId+token` → « Compte activé ✓ » annonçant le palier INPI à venir) → **Connexion** → `/accueil`. **Défi 2FA réel** : 2FA activée via l'API (TOTP calculé), puis login UI → bascule en **sous-vue « Validation en deux étapes »** (le jeton de défi reste en mémoire, hors URL) → saisie TOTP → `/accueil`. Le champ code accepte TOTP **ou** code de secours (le backend essaie l'un puis l'autre). Liens « Créer un compte » activés entre Connexion ↔ Inscription. La **proposition INPI** dédiée est repliée sur l'écran « compte activé » + le mode dégradé honnête de M6 (`/profil/inpi`).

> **Complément backend onboarding (31 mai 2026)** — les actions auparavant différées sont désormais **implémentées de bout en bout** : **« Renvoyer le lien »** (`POST /auth/resend-verification`) et **« Mot de passe oublié »** (`POST /auth/forgot-password` → email → page `/reinitialiser-mot-de-passe` → `POST /auth/reset-password`). Réponses **uniformes** (anti-énumération) ; reset à **usage unique** qui **révoque les sessions actives** ; migration EF `AddPasswordResetToken`. Vérifié E2E (Playwright) : renvoi régénère le token ; reset → login au nouveau mot de passe OK, ancien mot de passe **rejeté (401)**.

> **Vérification E2E M5 (31 mai 2026)** — Veille validée en navigateur (Playwright) : login UI → `/veille` **état vide** honnête (compte neuf, aucune source abonnée) ; **flux éditorial** (contrat `GET /feed/timeline?editorialOnly=true` mocké) en **list-detail 2 panneaux**, items lu/non-lu + dédup « N sources rapportent », **mention de favori absente de la liste** (garde-fou doc 12 §6 : la liste reste éditoriale) ; **palier de lecture** (sélection `?i=`) : vue lecture interne (titre + extrait), **pont vers fiche** « \<entité\> — voir sa fiche → » marqué *à vérifier* (uniquement dans le détail, jamais un verdict — ADR-012), **lien sortant honnête** « Lire la source ↗ (vous quittez Atlas) » ; **états par item** lu/favori/archiver (`PATCH /feed/items/{id}/state`, optimiste) + raccourcis clavier (Espace/F/A) ; archivage → sortie du flux + désélection. Backend : filtre **`editorialOnly`** ajouté à la timeline (exclut les `FavoriteEvent`, sans effet sur l'Accueil ; test unitaire).

> **Vérification E2E M3 (31 mai 2026)** — Accueil / feed validé en navigateur (Playwright) : racine `/` en déconnecté → redirection propre vers `/connexion` (corrige un 500 d'hôte : la page racine `[Authorize]` exigeait un `AuthorizationMiddleware` absent en WASM pur → `Atlas.Web` ajoute `AddAuthorization()`/`UseAuthorization()` + `.AllowAnonymous()` sur les endpoints de composants, le gating restant côté client via `AuthorizeRouteView`) ; après login UI → `/accueil` **état vide** honnête (compte neuf) ; **état chargé** (contrat `GET /feed/timeline` mocké) : cartes-event RNE/BODACC + veille, lien fiche / lien source, chips de mentions favoris, dédup « N sources rapportent », clôture « tu es à jour » ; **marquage lu optimiste** (`PATCH /feed/items/{id}/state`) avec distinction lu/non-lu non portée par la seule couleur (pastille + graisse + bordure). Données réelles côté intégration (register/verify/login), rendu chargé exercé sur payload conforme au `TimelineItemDto`.

> **Vérification E2E réelle (31 mai 2026)** — M1/M2/M4/M6 validés contre le backend + l'INPI RNE réels (harness `docs/13`) : `GET /companies?name=`, `GET /companies/{siren}` (fiche DANONE/SIREN 552032534, dirigeants réels), `POST/GET/DELETE /favorites/companies`, `/inpi/connection`, `/account/export`. Parcours navigateur (Playwright, données live) : connexion → recherche SIREN → fiche → « Suivre » → favoris list-detail à deux panneaux ; puis **M6** : **connexion INPI depuis le formulaire UI → « ✓ Connecté » → déblocage des données (fiche réelle) → export RGPD**, captures à l'appui. Hors périmètre : recherche PI (blocage INPI connu, V2). Fidélité données : forme juridique et qualité des dirigeants exposées en **codes** RNE (mapping code→libellé = amélioration future, transverse aux clients).

---

## 4. Kit de composants (vivant)

Extrait **au fil des écrans**, pas en amont. Chaque composant couvre ses **états** (doc 12 §11-14).

| Composant | États / variantes | Extrait en | Statut |
|---|---|:--:|:--:|
| Layout **rail** (5 destinations) | actif / hover / focus | M0 | ✅ |
| Atomes : **`Provenance`**, **`Chip`** (badge/statut), **`LabeledField`** (champ étiqueté) | `Chip` tons neutral/info/ok/warn (+ lien) ; `Provenance` « source · date » (variante séparée) ; provenance jamais masquée | M2 (extraits) | ✅ |
| Atome **`KeyFigure`** (chiffre-clé) | — | — | ⬜ (différé : aucun consommateur avant les indicateurs financiers **F-054** — principe « au fil des écrans ») |
| **Carte-aperçu** (entity / event) | défaut · hover · focus · sélectionné · lu/non-lu · skeleton | M1 / M3 | ✅ (entity = `CompanySummaryCard` ; event = `FeedEventCard` ; skeleton = `CardSkeleton`, chargement des listes Recherche/Favoris) |
| **Carte-section** (repliable) | ouvert · replié · épinglé · vide-couverture · erreur-locale · skeleton | M2 | ✅ (`SectionCard` ; épingle = visuel + `OnPinToggle`, réordre/persistance → F-062) |
| **États** (composants) | chargement (skeleton) · vide (onboarding/couverture) · erreur (locale) · fin de liste | M1 / M3 | ✅ (les 4 exercés sur l'Accueil) |
| **Thème** clair/sombre + tokens | clair · sombre (auto `prefers-color-scheme` ; sélecteur persisté → M6/F-062) | M1 | ✅ |

---

## 5. Exigences transverses (portes de qualité — toujours actives)

Pas des phases : vérifiées **sur chaque écran** (intégrées à la DoD §3).

- **Accessibilité** (doc 06 / ADR-008) — **bloquante** (DoD). `aria-*` natifs au web ; focus visible ; rien porté par la seule couleur.
- **Responsive** — adapter à la **largeur** (R2), pas à l'OS ; list-detail 2 panneaux → empilement sous le point de rupture.
- **Thème** clair/sombre cohérent avec les maquettes.
- **Sécurité client** — token JWT **en mémoire** (jamais `localStorage`) ; `Atlas.Web.Client` → `Domain` + `Shared` only (NetArchTest) ; zéro secret côté navigateur.
- **Performance** — poids WASM initial (lazy loading / trimming à évaluer ; non-sujet SEO car app authentifiée).
- **Langue** — français par défaut (`NeutralLanguage` fr-FR).

---

## 6. Hors périmètre (de l'historique Blazor)

- **Pages publiques / SEO** (landing, marketing) — exclues (app authentifiée seule) ; leur ajout exigerait un rendu serveur (avenant ADR-029). Doc 14 §1/§7.
- **PWA / hors-ligne** — à évaluer ultérieurement. Doc 14 §7.
- **Clients natifs** (desktop/mobile) — sous ADR-029 ils ne sont plus « suivis séparément » : ce sont des **têtes de la même base `Atlas.App`** (cf. §7), plus des projets distincts (MAUI/Avalonia abandonnés).

---

## 7. Bascule vers Uno (`Atlas.App`)

> Chantier d'intégration **bien avancé** (2 juin 2026) : U1→U4 livrés (parcours auth→INPI→recherche→fiche→favoris→veille en données réelles, validé E2E ; routing URL WASM ; **onboarding** inscription/vérif e-mail/reset mot de passe ; **Profil → Données/RGPD** export+suppression+déconnexion). Reste un **polish différé de U3** (bundling des polices TTF, animation shimmer du skeleton) et surtout **U5** (retrait du client Blazor) après un spike multi-cible (Linux/mobile) concluant.

L'**acquis Blazor (§1–§5) est directement réutilisable** : contrats `AtlasApiClient`, parcours écran par écran, états, vérifications E2E, kit de composants (à porter en XAML WinUI), breakpoints 640/880 px. La doctrine UX (`docs/12`) et la déclinaison multi-surface (`docs/14`, refondu Uno) ne changent pas.

**Garde-fou ADR-029 (ordre sûr)** — on ne retire `Atlas.Web`/`Atlas.Web.Client` (ni `Atlas.Maui`) **qu'après** un spike Uno multi-cible concluant. Séquence :

| Étape | Contenu | Statut |
|---|---|:--:|
| **U0 — Spike** | écran carte-aperçu + liste sur WASM + desktop ; valider XAML WinUI, rendu Linux, poids WASM | 🟡 partiel (1ᵉʳ juin 2026 : WASM/desktop OK, ~9,5 Mo Release ; Linux/mobile au runtime à valider) |
| **U1 — Intégration repo** | `Atlas.App` (Uno single project) dans `Atlas.slnx` : `Uno.Sdk` 6.5.36 pin dans `global.json`, **CPM isolé** (`Directory.Packages.props`/`Directory.Build.props` locaux), **job CI `uno-build`** (têtes desktop Skia + WebAssembly) | ✅ (PR #120) |
| **U2 — Preuve ADR-002** | `Atlas.App` → **Domain + Shared uniquement** (verrouillé par `CsprojDependencyTests`) ; `MainPage` consomme `Domain.Siren` ; `AtlasApiClient` = stub du point d'entrée HTTP unique. *Reste : auth (token mémoire + refresh cookie/secure storage) à câbler avec les vrais écrans (U4)* | ✅ (PR #120) |
| **U3 — Portage du kit** | thème clair/sombre (`ThemeDictionaries`) ; atomes `Chip`/`Provenance`/`LabeledField` ; cartes `CompanySummaryCard`/`SectionCard` (4 états)/`FeedEventCard` ; `CardSkeleton` ; coque `RailShell` (`NavigationView`) ; `ListDetailView` (2 panneaux, doc 14 §3) — en UserControls XAML WinUI. *Reste (polish différé) : bundling des polices (TTF, dispo dans `docs/design/fonts/`) + animation shimmer du `CardSkeleton`* | ✅ (PR #123/#124/#125) |
| **U4 — Re-livraison écran par écran** | DI léger (MS HttpClientFactory) ; **Connexion + 2FA + refresh silencieux** (PR #128/#136/#137) ; **Profil → connexion INPI** (#130) ; **Recherche** réelle + debounce (#129/#133) ; **Fiche entreprise** (#132) ; **Suivre + Favoris** (#134) ; **Accueil/Veille** timeline réelle (#135) ; **routing URL WASM** + deep-link + back/forward (#138/#139) ; **onboarding** (inscription / vérif e-mail / mot de passe oublié, deep-link pré-auth WASM — #154) ; **Profil → Données/RGPD** (export art. 20 + suppression art. 17 + déconnexion — #155). Parcours auth→INPI→recherche→fiche→favoris **validé E2E en réel** (desktop, harness docs/13) ; onboarding + RGPD vérifiés au build (2 têtes). | ✅ écrans livrés (2 juin 2026 ; reste le polish différé de U3) |
| **U5 — Retrait du Blazor** | supprimer `Atlas.Web`/`Atlas.Web.Client` (+`Atlas.Maui`) une fois U0–U4 validés multi-cible (spike Linux/mobile) | ⬜ |

## Renvois

| Sujet | Référence |
|---|---|
| Backlog produit / features | `docs/02-roadmap-features.md` (`F-NNN`) |
| Doctrine UX (tronc commun) | `docs/12-modele-ux-client-maui.md` |
| Doctrine UX (déclinaison multi-surface) | `docs/14-modele-ux-client-web.md` |
| Décision UI unifiée (Uno, 6 cibles) | `ADR-029` (remplace ADR-017/ADR-026) |
| Maquettes UI | `docs/design/phone_mode/` |
| Accessibilité (bloquant) | `docs/06-accessibilite.md`, ADR-008 |
| Placement solution (`Atlas.App`) | `docs/10-layout-solution-dotnet.md` |

*Document évolutif. §1–§6 = historique figé du client Blazor transitoire ; §7 = bascule Uno (cocher les étapes U0–U5 au fil du chantier d'intégration).*
