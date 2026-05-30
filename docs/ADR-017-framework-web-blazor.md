## ADR-017 — Framework du client web : Blazor Web App (interactivité WASM/Auto)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026 — **amendé le 30 mai 2026** (WASM pur pour la v1 « app authentifiée seule » ; cf. § Amendement et `docs/14-modele-ux-client-web.md`)

### Contexte

`Atlas.Maui` couvre le natif (Android, iOS, Windows, macOS) mais **pas le web**. Un **client web** est nécessaire (app authentifiée + surface publique landing/marketing).

Deux décisions cadrent déjà le terrain :
- **ADR-007** fixe la stack **tout-.NET** (« une seule stack à maîtriser », « pas de fragmentation des langages ») et liste explicitement « **Blazor Server ou WebAssembly** pour le web (décision différée) ».
- **ADR-002** (topologie hybride) impose que **les clients — MAUI *et* « futur Blazor » — soient des consommateurs HTTP purs** de `Atlas.Api`, partageant le `Core`/`Domain` ; toute logique sensible et tout appel externe restent côté backend.

La table « décisions ultérieures » de `docs/01-decisions-architecturales.md` renvoyait le **choix précis (Server vs WASM vs autre)** au MVP 1. C'est ce point que cet ADR tranche. Le choix du langage n'est **pas** rouvert : il reste .NET/Blazor (ADR-007).

Trois saveurs étaient en lice : **Blazor Server** (UI sur serveur via SignalR), **Blazor WebAssembly** (client dans le navigateur), **Blazor Web App** (modèle unifié .NET 8+, render modes *Static SSR* / *Interactive Server* / *Interactive WebAssembly* / *Interactive Auto*).

### Décision

Le client web est un **Blazor Web App (modèle unifié)**, structuré ainsi :

**1. Interactivité WebAssembly pour l'application authentifiée ; SSR pour le public.** Les pages publiques (landing, marketing, pages à SEO) sont rendues en **SSR statique** ; l'app derrière login est **interactive WebAssembly**. L'option *Interactive Auto* est admise comme optimisation du premier rendu (voir Conséquences).

**2. Le client interactif reste un consommateur HTTP pur de l'API (ADR-002).** L'assembly d'interactivité (`Atlas.Web.Client`) ne référence que **`Domain` + `Shared`**, **jamais** `Infrastructure` — **même règle et même raison que `Atlas.Maui`** : le code part dans le navigateur, donc il est décompilable (zéro credential, zéro logique sensible). Un test **NetArchTest** dédié l'impose, comme pour MAUI.

**3. Pas de Blazor Server pour l'app authentifiée.** Sa **connexion permanente** (circuit SignalR) est fragile en mobilité (personas en rendez-vous), coûteuse à scaler (état serveur par utilisateur), et son modèle **couple le client au serveur** — ce qui contredit la topologie « client pur » d'ADR-002.

**4. Authentification navigateur alignée sur l'existant API.** Access token JWT court (RS256, 15 min) gardé **en mémoire** + refresh via le **cookie `HttpOnly` rotatif `atlas_refresh`** déjà émis par `Atlas.Api`. **Jamais** de token en `localStorage`/`sessionStorage` (anti-XSS). Le CSP est déjà `default-src 'none'`. Côté MAUI le secret vit dans SecureStorage ; côté web, dans le cookie HttpOnly. Rien à inventer côté serveur.

**5. Réutilisation maximale.** `Domain` + `Shared` (value objects `Siren`, DTOs, validations) sont partagés **directement** comme pour MAUI ; le pattern `AtlasApiClient` (Refit) se réutilise. La doctrine UX (`docs/12-modele-ux-client-maui.md`, agnostique de la techno) et le kit (carte-aperçu, carte-section, états, list-detail) se mappent en **composants Razor**.

**Placement hexagonal** : `Atlas.Web` est un **adapter entrant** (primaire), parallèle à `Atlas.Api` et `Atlas.Maui`, consommant l'API HTTP. L'hôte peut référencer davantage ; seul `Atlas.Web.Client` (interactivité) est contraint à `Domain` + `Shared`.

#### Croquis (illustratif)

```razor
@* Atlas.Web — hôte Blazor Web App. Public en SSR ; app authentifiée en WASM (ou Auto). *@
@* L'assembly d'interactivité Atlas.Web.Client ne référence QUE Domain + Shared (client pur de l'API). *@
@rendermode InteractiveWebAssembly

@* Auth : access token EN MÉMOIRE ; refresh via cookie HttpOnly rotatif (atlas_refresh). *@
@* JAMAIS de token en localStorage (anti-XSS). Tout transite par AtlasApiClient (Refit) → Atlas.Api. *@
```

### Rationale

- **Cohérence ADR-007** : tout-.NET, un seul stack pour un porteur solo ; réutilisation directe de `Domain`/`Shared` comme MAUI (DRY, ADR-002).
- **Le modèle unifié est le seul à tout concilier** : il **préserve la topologie** (client interactif pur) **et** offre le **SSR/SEO** sur le public. Blazor Server seul couplerait ; WASM seul perdrait le SSR.
- **Garde-fou déjà connu** : la règle « client → `Domain` + `Shared` only », vérifiée par NetArchTest, existe déjà pour MAUI — on l'étend, on ne l'invente pas.
- **Auth état de l'art, déjà en place** : cookie `HttpOnly` rotatif + bearer court est précisément le bon pattern navigateur (anti-XSS), et l'API l'émet déjà.
- **Effort UX mutualisé** : un seul document de doctrine (doc 12) sert MAUI **et** web.

### Conséquences

- **Positives** : un seul stack, réutilisation maximale du domaine, topologie et sécurité préservées, SEO sur la surface publique, doctrine UX mutualisée entre clients, discipline d'archi homogène (MAUI ↔ web).
- **Négatives** :
  - **Poids initial WASM** — atténué par le prerendering, le lazy loading, et *Auto* ; non-sujet derrière login (pas de SEO).
  - **Complexité du modèle unifié** : deux projets (hôte + `Atlas.Web.Client`), render modes à maîtriser.
  - **Entorse de *Auto*** : *Interactive Auto* utilise un **circuit Server au tout premier rendu** avant de basculer WASM — légère entorse à la pureté « client only ». À arbitrer : WASM pur si l'on veut la pureté stricte, *Auto* si l'on privilégie le temps de première interaction.
  - **WASM décompilable** → discipline « zéro secret, zéro `Infrastructure` » à tenir, exactement comme MAUI.
- **À prévoir** :
  - Créer `Atlas.Web` (+ `Atlas.Web.Client`) dans la solution (cf. `docs/10-layout-solution-dotnet.md`).
  - Test **NetArchTest** : `Atlas.Web.Client` → `Domain` + `Shared` uniquement.
  - Ajouter la ligne `Atlas.Web.Client → Domain + Shared` au tableau des règles d'or de dépendance de `CLAUDE.md`.
  - Intégrer l'auth cookie/bearer côté Blazor (handler HTTP + refresh silencieux).
  - Appliquer la **checklist d'accessibilité** (ADR-008 / `docs/06-accessibilite.md`) au web comme à MAUI — les `aria-*` y sont natifs.
  - Références croisées : **ADR-002** (topologie), **ADR-007** (stack — décision différée ici résolue), **ADR-008** (accessibilité), `docs/06-accessibilite.md`, `docs/12-modele-ux-client-maui.md`, **F-009/F-010** (clients MAUI, patron partagé).

### Amendement — 30 mai 2026 (WASM pur pour la v1)

`docs/14-modele-ux-client-web.md` précise le périmètre v1 du client web : **app
authentifiée uniquement**, sans pages publiques. Dans ce cas, l'option *Interactive
Auto* perd son objet — elle n'optimise que le premier rendu de **pages publiques**
(SSR → bascule WASM) et garderait, sans public, le bref **circuit serveur** au
démarrage (l'« entorse » signalée plus haut en Conséquences). La v1 retient donc
**WASM pur** : un seul mode de rendu, client 100 % consommateur de l'API, **zéro
état serveur** — strictement comme `Atlas.Maui`. Le **modèle unifié + SSR reste la
référence** si des pages publiques (landing/marketing) sont ajoutées un jour : ce
serait alors un nouvel avenant rouvrant le SSR.

---

*ADR figé le 29 mai 2026, amendé le 30 mai 2026. Client web = Blazor Web App ; v1 « app authentifiée seule » → **WASM pur** (modèle unifié + SSR réservé à d'éventuelles pages publiques) ; client pur de l'API (`Domain` + `Shared` only, NetArchTest) ; auth access-token-en-mémoire + refresh cookie HttpOnly. Résout le point laissé ouvert par ADR-007 ; tout-.NET, topologie ADR-002 préservée, UX mutualisée avec doc 12.*
