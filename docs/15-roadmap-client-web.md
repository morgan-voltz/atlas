# Roadmap — client web (Atlas.Web)

> Feuille de route **d'implémentation** du client web Blazor (`Atlas.Web` / `Atlas.Web.Client`).
> Document **distinct** du backlog produit (`docs/02-roadmap-features.md`, `F-NNN`) et de la doctrine
> UX (`docs/12` tronc commun + `docs/14` delta web). Ici : **comment on bâtit le client**.

**Version** : 1.0 — **Date** : 31 mai 2026 — **Portée** : `Atlas.Web` (app authentifiée, WASM pur).
**Cadre** : **ADR-017** (Blazor Web App, WASM pur), **ADR-002** (client pur de l'API), **ADR-008 / doc 06** (accessibilité, bloquante).

Légende : ✅ fait & vérifié · 🟡 partiel · ⬜ à faire.

**Principe directeur — tranches verticales, pas couches horizontales.** On livre **un écran de bout en
bout** (UI + données + auth + états + accessibilité) avant de passer au suivant : du fonctionnel tôt,
l'intégration dé-risquée au plus tôt. Le **kit de composants s'extrait au fil des écrans** (pas tout en
amont). Chaque écran est « fini » au sens de sa **Definition of Done** (§3), pas avant.

---

## 1. Cadre

Le client web est le **3ᵉ form-factor** d'un même produit (R1) : 90 % de la doctrine vient de `docs/12`
(agnostique) ; `docs/14` ne décrit que les deltas web (rail, list-detail 2 panneaux, routing/URLs).
Topologie : **consommateur HTTP pur** de `Atlas.Api` (ADR-002) ; `Atlas.Web.Client` ne référence que
`Domain` + `Shared` (verrouillé par NetArchTest). Maquettes de référence : `docs/design/phone_mode/`.

**Fondations — ✅ faites** (30-31 mai 2026, PR #81-#85) : décision framework (ADR-017, WASM pur),
modèle UX web (doc 14), scaffold `Atlas.Web`/`Atlas.Web.Client` (CPM, NetArchTest), squelette de
navigation (rail 5 destinations) + routing + page 404.

---

## 2. Jalons (tranches verticales)

Chaque jalon = une tranche verticale livrant de la valeur utilisable de bout en bout.

| Jalon | Contenu | Pourquoi en premier / dépendances | Statut |
|---|---|:--:|:--:|
| **M0 — Fondations** | scaffold + navigation/routing | socle (cf. §1) | ✅ |
| **M1 — Connexion + Recherche** | login (token mémoire + refresh cookie HttpOnly) **et** écran Recherche de bout en bout | 1ʳᵉ tranche : valide toute la chaîne — `AtlasApiClient`, auth, garde de route, premiers composants (carte-aperçu, états) | ✅ |
| **M2 — Fiche entreprise** | depuis un résultat, fiche en cartes-sections, provenance épinglée, états de couverture | réutilise carte-section + le client API de M1 | 🟡 |
| **M3 — Accueil / feed** | fil des mouvements des entités suivies (cartes-aperçu *event*) | dépend des favoris (lecture) ; garde-fous anti-« réseau social » | ⬜ |
| **M4 — Favoris / Watchlists + list-detail 2 panneaux** | gestion des favoris **et** introduction du **list-detail à 2 panneaux** (signature desktop, réutilisé ensuite) | la signature desktop (doc 14 §3) arrive ici puis se généralise | ⬜ |
| **M5 — Veille** | flux, palier de lecture, pont vers fiche « à vérifier » | réutilise list-detail (M4) | ⬜ |
| **M6 — Profil & compte** | profil, sous-pages compte / connexion INPI / données (RGPD) | — | ⬜ |
| **M7 — Onboarding complet** | création de compte, vérification email, défi 2FA, proposition INPI | complète l'auth minimale de M1 | ⬜ |

> L'ordre est indicatif et révisable ; on ne démarre un jalon que quand le précédent atteint sa DoD.

---

## 3. Écrans & parcours — Definition of Done

Un écran n'est **✅** que lorsque **toutes** ses colonnes le sont. DoD = template commun :

> **maquette** conforme à `docs/design` · **données** câblées à l'API · **états** (chargement / vide / erreur) ·
> **responsive** (reflow par largeur, doc 14 §5) · **a11y** (doc 06, bloquant) · **clavier** (focus, raccourcis) · **URL/titre** de page.

| Écran | Jalon | Maquette | Données | États | Responsive | A11y | Clavier | URL/titre | Statut |
|---|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| Recherche (cœur M1 ; « Vérifier un nom » F-060 = backlog séparé) | M1 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Fiche entreprise (Identité + Dirigeants ; Bilans/BODACC/Étabts/PI = features à venir) | M2 | 🟡 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🟡 |
| Accueil / feed | M3 | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Favoris / Watchlists | M4 | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Veille | M5 | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Profil + sous-pages | M6 | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Auth / onboarding | M1/M7 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🟡 |

Renvois doctrine par écran : Recherche doc 12 §7 ; Fiche §4 ; Accueil §5 ; Favoris §8 ; Veille §6 ; Profil §9 ; Auth §10.

---

## 4. Kit de composants (vivant)

Extrait **au fil des écrans**, pas en amont. Chaque composant couvre ses **états** (doc 12 §11-14).

| Composant | États / variantes | Extrait en | Statut |
|---|---|:--:|:--:|
| Layout **rail** (5 destinations) | actif / hover / focus | M0 | ✅ |
| Atomes (champ étiqueté, badge, **provenance**, chiffre-clé) | — (provenance jamais masquée) | M1 | ⬜ |
| **Carte-aperçu** (entity / event) | défaut · hover · pressed · focus · sélectionné · lu/non-lu · skeleton | M1 | 🟡 |
| **Carte-section** (repliable) | ouvert · replié · épinglé · vide-couverture · erreur-locale · skeleton | M2 | 🟡 |
| **États** (composants) | chargement (skeleton) · vide (onboarding/couverture) · erreur (locale) · fin de liste | M1 | 🟡 |
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

## 6. Hors périmètre (v1)

- **Pages publiques / SEO** (landing, marketing) — exclues (app authentifiée seule) ; leur ajout rouvrirait WASM-pur vs modèle unifié (avenant ADR-017). Doc 14 §1/§7.
- **PWA / hors-ligne** — à évaluer ultérieurement. Doc 14 §7.
- **Client MAUI** — suivi séparément (F-009/F-010 de `docs/02`) ; priorité au web d'abord.

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

*Document évolutif. Cocher les colonnes de DoD (§3) et les statuts (§2, §4) au fil des PR.*
