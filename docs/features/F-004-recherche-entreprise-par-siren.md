# F-004 — Recherche entreprise par SIREN

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
