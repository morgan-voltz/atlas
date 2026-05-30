# Designs — maquettes UI (format mobile)

Maquettes finalisées de l'interface Atlas, au **format mobile**. Elles servent de référence
**à la fois pour le client mobile MAUI** (`Atlas.Maui`, Android/iOS) **et pour l'app web en
format mobile**. Le modèle UX et les règles d'adaptation (densité, breakpoints, list-detail)
sont décrits dans [`../12-modele-ux-client-maui.md`](../12-modele-ux-client-maui.md).

Chaque écran est fourni en **HTML** (export interactif) et **PNG** (capture). Les variantes
**Clair** / **Sombre** correspondent au thème ; les autres fichiers couvrent les **états**
définis au §9 du modèle UX (chargement, vide, erreur, fin de liste).

> Les HTML sont des exports volumineux (assets inline). Pour les visualiser, ouvrir le fichier
> dans un navigateur.

## Écrans (`phone_mode/`)

| Écran | Dossier | Contenu |
|---|---|---|
| **Accueil** (feed) | `phone_mode/page_accueil/` | Clair, Sombre + états : Chargement squelette, Vide onboarding, Erreur de source locale, Fin de liste |
| **Fiche entreprise** | `phone_mode/page_fiche_entreprise/` | Clair, Sombre (page unique, cartes-sections repliables) |
| **Recherche** | `phone_mode/page_recherche/` | A — SIREN détecté (accès direct), B — Texte (segments / liste) |
| **Login / Onboarding** | `phone_mode/page_login/` | Ouverture (choix), Connexion, Création de compte, Vérification email, Défi 2FA, Proposition INPI |
| **Favoris / Watchlists** | `phone_mode/page_favorie/` | Tous (le portefeuille), Liste Concurrence, Kebab/swipe, Vide d'une liste, Vide global (aucun favori) |
| **Préférences / Profil** | `phone_mode/page_preference/` | Hub, Connexion INPI, Données (sous-pages sensibles) |
| **Veille** | `phone_mode/page_veille/` | Lecture, Moment 1 (panneau replié), Moment 2 (panneau de filtres ouvert) |

Les sept écrans couvrent les cinq destinations de navigation (Accueil, Recherche, Veille,
Favoris, Profil — cf. §3 du modèle UX) plus le parcours d'authentification et la fiche entreprise.
