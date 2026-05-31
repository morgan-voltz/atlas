# Roadmap — client web (Atlas.Web)

> Feuille de route **d'implémentation** du client web Blazor (`Atlas.Web` / `Atlas.Web.Client`).
> Document **distinct** du backlog produit/features (`docs/02-roadmap-features.md`, identifiants `F-NNN`)
> et de la doctrine UX (`docs/12` tronc commun + `docs/14` delta web). Ici on suit **comment on bâtit
> le client web**, pas quelles features produit existent.

**Version** : 1.0 — **Date** : 31 mai 2026 — **Portée** : `Atlas.Web` (app authentifiée, WASM pur).
**Décisions cadre** : **ADR-017** (Blazor Web App, WASM pur), **ADR-002** (client pur de l'API), **ADR-008 / doc 06** (accessibilité).

Identifiants `W-NNN` (Web). Les items qui **surfacent** une feature produit renvoient à son `F-NNN`.

Légende : ✅ livré & vérifié · 🟡 livré, vérification partielle · ⬜ à faire.

---

## État d'avancement

| Phase | Item | Statut |
|---|---|:--:|
| **0 — Fondations** | W-001 Framework & décision (ADR-017, WASM pur) | ✅ |
| | W-002 Modèle UX web (doc 14) | ✅ |
| | W-003 Scaffold `Atlas.Web` + `Atlas.Web.Client` (CPM, NetArchTest) | ✅ |
| | W-004 Squelette navigation + routing (rail 5 destinations, URLs, 404) | ✅ |
| **1 — Kit de composants Razor** | W-010 Atomes (champ étiqueté, badge, provenance, chiffre-clé) | ⬜ |
| | W-011 Carte-aperçu (entity / event) | ⬜ |
| | W-012 Carte-section (repliable, pin, provenance) | ⬜ |
| | W-013 États (chargement, vide, erreur, fin de liste) | ⬜ |
| | W-014 Thème clair/sombre + tokens (mapping maquettes) | ⬜ |
| **2 — Accès API & auth** | W-020 `AtlasApiClient` (Refit) | ⬜ |
| | W-021 Auth navigateur (token mémoire + refresh cookie HttpOnly) | ⬜ |
| | W-022 Garde de route protégée → `/login` + retour à l'URL | ⬜ |
| | W-023 Erreurs API → états (mapping `code` ProblemDetails) | ⬜ |
| **3 — Écrans par destination** | W-030 Accueil / feed | ⬜ |
| | W-031 Recherche (+ 3e mode « Vérifier un nom », F-060) | ⬜ |
| | W-032 Veille | ⬜ |
| | W-033 Favoris / Watchlists | ⬜ |
| | W-034 Profil + sous-pages (compte, INPI, données) | ⬜ |
| | W-035 Fiche entreprise (page cartes-sections) | ⬜ |
| | W-036 Auth / onboarding (ouverture → 2FA → proposition INPI) | ⬜ |
| **4 — List-detail & responsive** | W-040 List-detail 2 panneaux (Recherche/Favoris/Veille) | ⬜ |
| | W-041 Reflow par largeur (breakpoints R2 → empilement mobile) | ⬜ |
| | W-042 Conventions web (clavier, survol doublé, titre/favicon) | ⬜ |
| **5 — Qualité & déploiement** | W-050 Checklist accessibilité (doc 06 / ADR-008) par écran | ⬜ |
| | W-051 Tests (composants bUnit + smoke e2e) | ⬜ |
| | W-052 Build/CI + déploiement (hébergement statique WASM) | ⬜ |
| | W-053 PWA / hors-ligne (à évaluer) | ⬜ |

---

## Phase 0 — Fondations ✅

Socle décisionnel et technique posé (30-31 mai 2026).

- **W-001 — Framework & décision** ✅ — Client web = **Blazor Web App, WASM pur** pour l'app authentifiée (ADR-017, amendé). PR #81, #83.
- **W-002 — Modèle UX web** ✅ — `docs/14-modele-ux-client-web.md` : delta de doc 12 (rail, list-detail 2 panneaux, routing/URLs). PR #82.
- **W-003 — Scaffold** ✅ — `src/Atlas.Web` (hôte) + `src/Atlas.Web.Client` (WASM, `Domain` + `Shared` only, verrouillé par NetArchTest). CPM aligné. PR #84.
- **W-004 — Squelette navigation + routing** ✅ — Render mode WASM global, rail latéral à 5 destinations (Accueil/Recherche/Veille/Favoris/Profil), routes `/accueil…/profil`, page 404. PR #85.

## Phase 1 — Kit de composants Razor ⬜

Transposer le kit de doc 12 §6-9 et §11-14 en composants Razor réutilisables, à partir des maquettes `docs/design/phone_mode/`. **Pré-requis du reste** : les écrans (phase 3) consomment ce kit.

- **W-010 — Atomes** — champ étiqueté, badge (descriptif, jamais verdict, jamais couleur seule), **ligne de provenance** (`source · date`, jamais masquée), chiffre-clé. Doc 12 §6.
- **W-011 — Carte-aperçu** — variantes *preview-entity* (recherche/favoris) et *preview-event* (feed/veille), une seule anatomie. Doc 12 §12.
- **W-012 — Carte-section** — repliable, pin, provenance dans le corps, lien d'approfondissement. Doc 12 §13.
- **W-013 — États** — chargement (skeleton calquant l'anatomie), vide (onboarding vs couverture), erreur **locale**, « à jour / fin de liste ». Doc 12 §14.
- **W-014 — Thème & tokens** — clair/sombre, mapping des tokens depuis les maquettes (et `themes.json` si retenu).

## Phase 2 — Accès API & auth ⬜

Le client est un **consommateur HTTP pur** de `Atlas.Api` (ADR-002).

- **W-020 — `AtlasApiClient`** — contrat typé via **Refit** (`Refit.HttpClientFactory` déjà en CPM), DI côté client. Réutilise les DTO de `Atlas.Shared`/`Domain`.
- **W-021 — Auth navigateur** — access token JWT **en mémoire** + refresh via cookie **HttpOnly** rotatif `atlas_refresh` (déjà émis par l'API) ; **jamais** de token en `localStorage`. Doc 14 §1, ADR-010.
- **W-022 — Garde de route protégée** — route sans session → redirection propre vers `/login`, retour à l'URL demandée après login. Doc 14 §4.
- **W-023 — Erreurs API → états** — mapping du champ `code` des ProblemDetails (cf. PR #74) vers les états (W-013) ; deep-link dégradé honnête (INPI non connecté → bandeau, pas d'écran cassé). Doc 14 §4.

## Phase 3 — Écrans par destination ⬜

Chaque écran assemble le kit (phase 1) sur les données (phase 2), selon doc 12 + maquettes.

- **W-030 — Accueil / feed** — fil des mouvements d'entités suivies, cartes-aperçu *event*, garde-fous anti-« réseau social ». Doc 12 §5.
- **W-031 — Recherche** — champ unique intelligent, deux états guidés par l'intention ; **3e mode « Vérifier un nom »** surface **F-060** (V3). Doc 12 §7.
- **W-032 — Veille** — flux, palier de lecture, pont vers fiche « à vérifier ». Doc 12 §6.
- **W-033 — Favoris / Watchlists** — modèle « Tous + listes », divulgation R6. Doc 12 §8.
- **W-034 — Profil** — 5 entrées en 2 familles + déconnexion ; sous-pages compte / connexion INPI / données. Doc 12 §9.
- **W-035 — Fiche entreprise** — page unique de cartes-sections repliables, provenance épinglée. Doc 12 §4.
- **W-036 — Auth / onboarding** — ouverture (choix), connexion, création de compte, vérification email, défi 2FA, proposition INPI. Doc 12 §10.

## Phase 4 — List-detail & responsive ⬜

- **W-040 — List-detail 2 panneaux** — signature desktop : liste à gauche, détail à droite, sélection persistante. Recherche / Favoris / Veille. Doc 14 §3.
- **W-041 — Reflow par largeur** — sous le point de rupture, repli sur l'empilement mobile (R2, adapter à la largeur pas à l'OS). Doc 14 §5.
- **W-042 — Conventions web** — clavier de premier ordre (`/` ou `Ctrl-K`, flèches, `Esc`), survol **doublé** d'un accès permanent, `<title>`/favicon par écran. Doc 14 §5.

## Phase 5 — Qualité & déploiement ⬜

- **W-050 — Accessibilité** — checklist doc 06 / ADR-008 passée **par écran** (exigence bloquante DoD).
- **W-051 — Tests** — composants (bUnit) + smoke e2e des parcours clés.
- **W-052 — Build/CI & déploiement** — au-delà du build CI déjà en place (#84), publication WASM et hébergement statique à câbler (cf. docs/10).
- **W-053 — PWA / hors-ligne** — installable + cache, à évaluer selon le besoin. Doc 14 §7.

---

## Hors périmètre (v1)
- **Pages publiques / SEO** (landing, marketing) : exclues (app authentifiée seule). Leur ajout rouvrirait WASM-pur vs modèle unifié → avenant à ADR-017. Doc 14 §1/§7.
- **Client MAUI** : suivi séparément (F-009/F-010 de docs/02) ; priorité au web d'abord.

## Renvois
| Sujet | Référence |
|---|---|
| Backlog produit / features | `docs/02-roadmap-features.md` (`F-NNN`) |
| Doctrine UX (tronc commun) | `docs/12-modele-ux-client-maui.md` |
| Doctrine UX (delta web) | `docs/14-modele-ux-client-web.md` |
| Décision framework web | `ADR-017` |
| Maquettes UI | `docs/design/phone_mode/` |
| Accessibilité (bloquant) | `docs/06-accessibilite.md`, ADR-008 |
| Placement solution | `docs/10-layout-solution-dotnet.md` |

*Document évolutif. Mettre à jour le statut des `W-NNN` au fil des PR.*
