# ADR-017 — Framework du client web : Blazor Web App (interactivité WASM/Auto)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026 — **amendé le 30 mai 2026** (WASM pur pour la v1 « app authentifiée seule » ; cf. § Amendement et `docs/14-modele-ux-client-web.md`)

## Contexte

`Atlas.Maui` couvre le natif (Android, iOS, Windows, macOS) mais **pas le web**. Un **client web** est nécessaire (app authentifiée + surface publique landing/marketing).

Deux décisions cadrent déjà le terrain :
- **ADR-007** fixe la stack **tout-.NET** et listait « **Blazor Server ou WebAssembly** pour le web (décision différée) ».
- **ADR-002** (topologie hybride) impose que **les clients — MAUI *et* « futur Blazor » — soient des consommateurs HTTP purs** de `Atlas.Api`, partageant le `Core`/`Domain` ; toute logique sensible et tout appel externe restent côté backend.

La table « décisions ultérieures » de `docs/01-decisions-architecturales.md` renvoyait le **choix précis (Server vs WASM vs autre)** au MVP 1 : **cet ADR le tranche** (l'entrée correspondante a été retirée). Le choix du langage n'est **pas** rouvert : il reste .NET/Blazor (ADR-007).

Trois saveurs étaient en lice : **Blazor Server** (UI sur serveur via SignalR), **Blazor WebAssembly** (client dans le navigateur), **Blazor Web App** (modèle unifié .NET 8+, render modes *Static SSR* / *Interactive Server* / *Interactive WebAssembly* / *Interactive Auto*).

## Décision

Le client web est un **Blazor Web App (modèle unifié)**, structuré ainsi :

**1. Interactivité WebAssembly pour l'application authentifiée ; SSR pour le public.** Les pages publiques (landing, marketing, pages à SEO) sont rendues en **SSR statique** ; l'app derrière login est **interactive WebAssembly**. L'option *Interactive Auto* est admise comme optimisation du premier rendu (voir Conséquences).

**2. Le client interactif reste un consommateur HTTP pur de l'API (ADR-002).** L'assembly d'interactivité (`Atlas.Web.Client`) ne référence que **`Domain` + `Shared`**, **jamais** `Infrastructure` — **même règle et même raison que `Atlas.Maui`** : le code part dans le navigateur, donc il est décompilable (zéro credential, zéro logique sensible). Un test **NetArchTest** dédié l'impose, comme pour MAUI.

**3. Pas de Blazor Server pour l'app authentifiée.** Sa **connexion permanente** (circuit SignalR) est fragile en mobilité (personas en rendez-vous), coûteuse à scaler (état serveur par utilisateur), et son modèle **couple le client au serveur** — ce qui contredit la topologie « client pur » d'ADR-002.

**4. Authentification navigateur alignée sur l'existant API.** Access token JWT court (RS256, 15 min) gardé **en mémoire** + refresh via le **cookie `HttpOnly` rotatif `atlas_refresh`** déjà émis par `Atlas.Api`. **Jamais** de token en `localStorage`/`sessionStorage` (anti-XSS). Côté MAUI le secret vit dans SecureStorage ; côté web, dans le cookie HttpOnly. Rien à inventer côté serveur.

**5. Réutilisation maximale.** `Domain` + `Shared` (value objects `Siren`, DTOs, validations) sont partagés **directement** comme pour MAUI ; le pattern `AtlasApiClient` (Refit) se réutilise. La doctrine UX (`docs/12-modele-ux-client-maui.md`, agnostique de la techno) et le kit (carte-aperçu, carte-section, états, list-detail) se mappent en **composants Razor**.

**Placement hexagonal** : `Atlas.Web` est un **adapter entrant** (primaire), parallèle à `Atlas.Api` et `Atlas.Maui`, consommant l'API HTTP. L'hôte peut référencer davantage ; seul `Atlas.Web.Client` (interactivité) est contraint à `Domain` + `Shared`.

## Rationale

- **Cohérence ADR-007** : tout-.NET, un seul stack pour un porteur solo ; réutilisation directe de `Domain`/`Shared` comme MAUI (DRY, ADR-002).
- **Le modèle unifié est le seul à tout concilier** : il **préserve la topologie** (client interactif pur) **et** offre le **SSR/SEO** sur le public. Blazor Server seul couplerait ; WASM seul perdrait le SSR.
- **Garde-fou déjà connu** : la règle « client → `Domain` + `Shared` only », vérifiée par NetArchTest, existe déjà pour MAUI — on l'étend, on ne l'invente pas.
- **Auth état de l'art, déjà en place** : cookie `HttpOnly` rotatif + bearer court est le bon pattern navigateur (anti-XSS), et l'API l'émet déjà.

## Conséquences

- **Positives** : un seul stack, réutilisation maximale du domaine, topologie et sécurité préservées, SEO sur la surface publique, doctrine UX mutualisée entre clients, discipline d'archi homogène (MAUI ↔ web).
- **Négatives** :
  - **Poids initial WASM** — atténué par le prerendering, le lazy loading ; non-sujet derrière login (pas de SEO).
  - **Complexité du modèle unifié** : deux projets (hôte + `Atlas.Web.Client`), render modes à maîtriser.
  - **WASM décompilable** → discipline « zéro secret, zéro `Infrastructure` » à tenir, exactement comme MAUI.
- **À prévoir** (état au 31 mai 2026) :
  - ~~Créer `Atlas.Web` (+ `Atlas.Web.Client`)~~ ✅ fait (cf. `docs/10`, `docs/15`).
  - ~~Test **NetArchTest** `Atlas.Web.Client → Domain + Shared`~~ ✅ fait.
  - ~~Ligne de dépendance dans `CLAUDE.md`~~ ✅ fait.
  - Intégrer l'auth cookie/bearer côté Blazor (handler HTTP + refresh silencieux) — à faire (cf. `docs/15` jalon M1).
  - Appliquer la **checklist d'accessibilité** (ADR-008 / `docs/06`) au web comme à MAUI.

## Amendement — 30 mai 2026 (WASM pur pour la v1)

`docs/14-modele-ux-client-web.md` précise le périmètre v1 du client web : **app authentifiée uniquement**, sans pages publiques. Dans ce cas, l'option *Interactive Auto* perd son objet — elle n'optimise que le premier rendu de **pages publiques** (SSR → bascule WASM) et garderait, sans public, un bref **circuit serveur** au démarrage. La v1 retient donc **WASM pur** : un seul mode de rendu, client 100 % consommateur de l'API, **zéro état serveur** — strictement comme `Atlas.Maui`. Le **modèle unifié + SSR reste la référence** si des pages publiques (landing/marketing) sont ajoutées un jour : ce serait alors un nouvel avenant.

> Résout le point laissé ouvert par ADR-007. Topologie ADR-002 préservée, UX mutualisée avec `docs/12`/`docs/14`. Implémentation suivie dans `docs/15`.
