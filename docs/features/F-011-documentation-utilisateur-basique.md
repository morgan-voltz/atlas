# F-011 — Documentation utilisateur basique

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Générateur figé : **MkDocs Material** (léger, 100 % markdown, FR), dans `website/`. Pages : Accueil, Installation, Premiers pas, FAQ. Build `--strict` vérifié. Déploiement **GitHub Pages** via GitHub Actions (`.github/workflows/docs.yml`). À activer dans les réglages du repo (source = GitHub Actions ; Pages public nécessite le repo public).

**Description** : un site documentaire (Docusaurus, MkDocs ou similaire) hébergé sur GitHub Pages / GitLab Pages couvrant : installation, premiers pas, FAQ.

**Valeur user** : sans doc, les early adopters abandonnent à la première difficulté.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : aucune.

**Détails techniques** : choix du générateur à figer (Docusaurus si on veut une UX moderne, MkDocs si on veut quelque chose de très léger).
