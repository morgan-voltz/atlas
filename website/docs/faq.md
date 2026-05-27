# FAQ

## Qu'est-ce qu'Atlas ?

Un SaaS open source qui agrège des données publiques françaises sur les entreprises (RNE) et la
propriété industrielle (marques INPI), avec un historique de recherches.

## Ai-je besoin d'un compte INPI ?

Oui. Atlas suit un modèle **multi-tenant** : chaque utilisateur connecte son propre compte INPI.
Atlas interroge les API publiques **en votre nom**, sans compte partagé.

## Mes identifiants INPI sont-ils en sécurité ?

Oui. Ils sont **chiffrés au repos** (AES-256-GCM), déchiffrés uniquement le temps d'une requête
vers l'INPI, et **jamais** loggés ni renvoyés par l'API.

## Mon mot de passe Atlas est-il protégé ?

Il est haché avec **Argon2id** (jamais stocké en clair). Les sessions reposent sur des JWT courts
et des jetons de rafraîchissement rotatifs et révocables.

## Puis-je exporter ou supprimer mes données ?

Oui (RGPD). Vous pouvez **exporter** vos données personnelles (JSON) et **supprimer** votre compte ;
la suppression efface toutes vos données associées.

## Quelles données puis-je consulter ?

Les entreprises françaises (via le RNE) et les marques françaises (via l'INPI PI). D'autres sources
(brevets, BODACC, veille) sont prévues dans les versions ultérieures.

## Atlas est-il open source ?

Oui, sous licence **AGPL v3**.
