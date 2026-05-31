# F-009 — Client MAUI mobile (Android/iOS) avec fonctions de base

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
