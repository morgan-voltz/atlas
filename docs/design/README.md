# Designs — maquettes UI (3 formats responsive)

Maquettes finalisées de l'interface Atlas, déclinées en **trois formats de largeur**. Le modèle UX
et les règles d'adaptation sont décrits dans [`../12-modele-ux-client-maui.md`](../12-modele-ux-client-maui.md)
(tronc commun, agnostique) et [`../14-modele-ux-client-web.md`](../14-modele-ux-client-web.md) §3
(« Les 3 formats », breakpoints).

Chaque écran est fourni en **HTML** (export interactif) et **PNG** (capture). Les variantes
**Clair** / **Sombre** correspondent au thème ; les autres fichiers couvrent les **états** (chargement,
vide, erreur, fin de liste — §9/§14 du modèle UX).

> Les HTML sont des exports volumineux (assets inline) : ouvrir le fichier dans un navigateur.

## Les 3 formats (largeur × plateforme)

Les paliers sont définis par **largeur, jamais par appareil** (R2) — un même jeu de maquettes couvre
donc les trois clients :

| Format | Largeur | Dossier / préfixe | Couvre |
|---|---|---|---|
| **Téléphone** | < 640 px | `phone_mode/` | **web** étroit **+ app mobile** (MAUI — ADR-026) |
| **Intermédiaire** | 640 – 880 px | fichiers « Fenêtre réduite … » | **web** moyen + **fenêtre desktop réduite** / split-screen |
| **Full (desktop)** | ≥ 880 px | `Atlas *.html` + « Desktop … » | **web** large **+ app desktop** (Avalonia — ADR-026) |

Breakpoints **640 / 880 px** : valeurs implémentées dans le client web, faisant foi (doc 14 §3).

## Téléphone — `phone_mode/` (complet)

| Écran | Dossier | Contenu |
|---|---|---|
| **Accueil** (feed) | `phone_mode/page_accueil/` | Clair, Sombre + états : Chargement squelette, Vide onboarding, Erreur de source locale, Fin de liste |
| **Fiche entreprise** | `phone_mode/page_fiche_entreprise/` | Clair, Sombre (page unique, cartes-sections repliables) |
| **Recherche** | `phone_mode/page_recherche/` | A — SIREN détecté (accès direct), B — Texte (segments / liste) |
| **Login / Onboarding** | `phone_mode/page_login/` | Ouverture (choix), Connexion, Création de compte, Vérification email, Défi 2FA, Proposition INPI |
| **Favoris / Watchlists** | `phone_mode/page_favorie/` | Tous (le portefeuille), Liste Concurrence, Kebab/swipe, Vide d'une liste, Vide global |
| **Préférences / Profil** | `phone_mode/page_preference/` | Hub, Connexion INPI, Données (sous-pages sensibles) |
| **Veille** | `phone_mode/page_veille/` | Lecture, Moment 1 (panneau replié), Moment 2 (panneau de filtres ouvert) |

Les sept écrans couvrent les cinq destinations (Accueil, Recherche, Veille, Favoris, Profil) plus
l'authentification et la fiche entreprise.

## Full / desktop — racine `Atlas *.html` + « Desktop … »

| Écran | Fichier |
|---|---|
| Accueil | `Atlas Accueil.html` |
| Entrée (ouverture / login) | `Atlas Entrée.html` |
| Recherche | `Atlas Recherche.html`, `Atlas Recherche Web.html` |
| Fiche entreprise | `Atlas Fiche.html` |
| Favoris | `Atlas Favoris.html` |
| Veille | `Atlas Veille.html` |
| Profil | `Atlas Profil.html` |
| Démos desktop (thème) | `Desktop _ clair.html` / `.png`, `Desktop _ sombre.html` / `.png` |

## Intermédiaire — « Fenêtre réduite » (640–880 px)

| Écran | Vue | Fichier |
|---|---|---|
| **Recherche** | Liste seule | `Fenêtre réduite _ liste seule.html` / `.png` (= `RechercheWeb layout='narrow'`, vue liste) |
| **Recherche** | Liste (sombre) | `Fenêtre réduite _ liste _ sombre.html` / `.png` |
| **Fiche** | Plein écran + retour | `Fenêtre réduite _ fiche plein écran _ retour.html` / `.png` |
| **Accueil** | Feed (clair / sombre) | `Accueil — fenêtre réduite — clair.png` / `— sombre.png` |
| **Veille** | Liste éditoriale (clair / sombre) | `Veille — fenêtre réduite — clair.png` / `— sombre.png` |

> Les maquettes **Accueil** et **Veille** intermédiaires sont rendues par le harnais `render.html`
> (cf. ci-dessous) à partir des composants web responsive `AccueilWeb` / `VeilleWeb`
> (`atlas-web-screens.jsx`, `layout='narrow'`).

## Couverture & manques (audit 1ᵉʳ juin 2026)

| Écran | Téléphone | Intermédiaire | Full / desktop |
|---|:--:|:--:|:--:|
| Accueil | ✅ (+états) | ✅ (clair/sombre) | ✅ |
| Recherche (list-detail) | ✅ | ✅ (liste, clair/sombre) | ✅ |
| Veille (list-detail) | ✅ | ✅ (clair/sombre) | ✅ |
| Favoris (list-detail) | ✅ (+états) | 🟡 (liste seule) | ✅ |
| Fiche entreprise | ✅ | ✅ (plein écran + retour) | ✅ |
| Profil / préférences | ✅ | ⬜ | ✅ |
| Login / onboarding | ✅ | ⬜ | ✅ (Entrée) |

**Constat (mis à jour 1ᵉʳ juin 2026)** : l'intermédiaire couvre désormais les écrans **à reflow**
(Recherche, Veille, Accueil + Fiche). Restent, en **priorité basse** (mise en page quasi inchangée
entre intermédiaire et full) : **Favoris « détail »**, **Profil** (hub) et **Login** (centré).

**Hygiène** : harmoniser le nommage (la racine est à plat, `phone_mode/` est arborescent ; doublon
`Atlas Recherche` / `Atlas Recherche Web` à clarifier) — idéalement un dossier par format
(`phone_mode/`, `intermediate_mode/`, `desktop_mode/`) miroir l'un de l'autre.

## Régénérer les maquettes web (harnais `render.html`)

Les écrans web sont des **composants React** (`atlas-web-core.jsx` = tokens/atomes ;
`atlas-web-screens.jsx` = `RechercheWeb` / `AccueilWeb` / `VeilleWeb`, responsive `layout='desktop'|'narrow'`).
`render.html` les monte (React 18 + Babel standalone + polices Google Fonts) selon l'URL :

```
render.html?screen=accueil|veille|recherche&layout=desktop|narrow&dark=0|1
```

Ouvrir via un **petit serveur HTTP** (le `fetch` des `.jsx` ne marche pas en `file://`), p.ex. :
`npx http-server docs/design` puis `http://localhost:8080/render.html?screen=veille&layout=narrow`.
Les PNG sont produits par capture du `#frame` (Playwright @ deviceScaleFactor 2).
