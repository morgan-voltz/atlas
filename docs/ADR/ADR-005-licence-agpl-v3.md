# ADR-005 — Licence open source AGPL v3

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

Le projet sera publié en open source (cf. ADR-006). Le choix de la licence détermine ce que les tiers peuvent faire du code et ce qu'ils doivent rendre à la communauté en échange.

## Décision

**AGPL v3 (GNU Affero General Public License version 3)**.

## Rationale

- **Copyleft fort** : toute modification utilisée pour offrir un service en ligne doit être ouverte. Empêche les "vampires" (AWS, Scaleway, etc.) de fork et héberger une version concurrente fermée.
- **Compatible monétisation future** : permet le **dual-licensing** (le code reste AGPL pour les particuliers, on offre une licence commerciale aux boîtes qui ne veulent pas s'engager à ouvrir leurs modifs).
- **Crédibilité** sur un projet manipulant des credentials sensibles : la communauté peut auditer.
- **Standard reconnu** par l'OSI (Open Source Initiative) et la FSF.

## Conséquences

- **Positives** : protection contre l'exploitation commerciale fermée, option dual-licensing, transparence.
- **Négatives** : adoption potentiellement plus lente que MIT/Apache (les boîtes ont parfois peur de l'AGPL). Contributions parfois freinées par des Contributor License Agreements (CLA) à signer si on veut conserver l'option dual-licensing.
- **À prévoir** :
  - Fichier `LICENSE` à la racine du repo dès la création.
  - Header AGPL dans chaque fichier source (ou au moins dans les fichiers principaux).
  - CLA pour les contributeurs externes si dual-licensing envisagé.
  - Documentation claire de la licence dans le README.
