# Atlas

**Atlas** est un SaaS **open source** (AGPL v3) qui agrège des données publiques françaises :

- **Entreprises** — identité, dirigeants, adresse, code NAF, via le **Registre National des Entreprises (RNE)** de l'INPI.
- **Propriété industrielle** — recherche de **marques** françaises, via les API **INPI PI**.
- **Historique** de vos recherches, accessible depuis votre profil.

!!! info "Nom de code provisoire"
    « Atlas » est un nom de code. Le nom définitif sera arrêté ultérieurement.

## Comment ça marche

Atlas relie votre identité (compte Atlas) à votre **propre compte INPI** : vous fournissez une fois vos
identifiants INPI, Atlas les **chiffre** et les utilise pour interroger les API publiques **en votre nom**.

Aucune donnée d'identifiant INPI ne quitte le serveur en clair, et rien de sensible n'est stocké sur votre
téléphone (seul un jeton de session y est conservé, de façon sécurisée).

## Pour commencer

- [Installation](installation.md) — déployer Atlas (self-hosted).
- [Premiers pas](premiers-pas.md) — créer un compte et faire sa première recherche.
- [FAQ](faq.md) — les questions fréquentes.
