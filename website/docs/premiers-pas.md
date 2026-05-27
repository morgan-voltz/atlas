# Premiers pas

## 1. Créer un compte

Inscrivez-vous avec une adresse email et un mot de passe (12 caractères minimum).
Un email de vérification vous est envoyé : ouvrez le lien pour **activer** votre compte.

## 2. (Recommandé) Activer la double authentification

Depuis vos paramètres, activez le **2FA TOTP** (Google Authenticator, Authy…). Vous obtenez
un QR code à scanner, puis **10 codes de secours** à conserver précieusement.

!!! tip "2FA et INPI"
    Le 2FA est fortement recommandé dès lors que vous connectez un compte INPI : il protège
    l'accès à des identifiants sensibles.

## 3. Connecter votre compte INPI

Fournissez vos identifiants INPI. Atlas **teste la connexion**, puis stocke vos identifiants
**chiffrés** (AES-256-GCM). Ils ne sont jamais affichés ni renvoyés par l'API.

## 4. Rechercher une entreprise

- Par **SIREN** (9 chiffres) : accès direct à la fiche.
- Par **dénomination** : liste de résultats paginée, puis ouverture de la fiche.

La fiche affiche l'identité, la forme juridique, le code NAF, l'adresse et les dirigeants.

## 5. Rechercher une marque

Saisissez un nom de marque pour obtenir les marques françaises correspondantes, puis ouvrez
la **notice détaillée** (déposant, dates, statut, classes de Nice, image si disponible).

## 6. Retrouver vos recherches

Vos recherches (entreprises et marques) sont enregistrées dans votre **historique**.

## 7. Vos données (RGPD)

- **Exporter** : récupérez l'ensemble de vos données personnelles au format JSON.
- **Supprimer** : la suppression de votre compte efface **toutes** vos données associées.
