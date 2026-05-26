# Stratégie de gestion des repositories

> Document de référence sur l'organisation des dépôts Git du projet : forges utilisées, stratégie de mirroring, conventions de commits, gestion des secrets, branch strategy et CI/CD.
> Objectif : un setup professionnel, résilient et éthique dès le premier commit.

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026

---

## Sommaire

- [1. Vision et principes](#1-vision-et-principes)
- [2. Topologie : 3 forges, 3 rôles](#2-topologie--3-forges-3-rôles)
- [3. Détail des forges utilisées](#3-détail-des-forges-utilisées)
- [4. Stratégie de mirroring](#4-stratégie-de-mirroring)
- [5. Structure du repo principal](#5-structure-du-repo-principal)
- [6. Stratégie de branches](#6-stratégie-de-branches)
- [7. Conventions de commits](#7-conventions-de-commits)
- [8. Gestion des secrets](#8-gestion-des-secrets)
- [9. CI/CD](#9-cicd)
- [10. Lifecycle : privé → public](#10-lifecycle--privé--public)
- [11. Plan de mise en place](#11-plan-de-mise-en-place)

---

## 1. Vision et principes

### 1.1 Objectifs

- **Résilience** : la perte d'une forge ne doit pas signifier la perte du projet.
- **Visibilité** : maximiser l'exposition du projet une fois open source.
- **Souveraineté** : pouvoir continuer à travailler même sans accès aux GAFAM.
- **Confidentialité** : certains documents (stratégiques, secrets) ne doivent jamais devenir publics.
- **Simplicité** : pas de complexité inutile pour un dev solo.

### 1.2 Principes directeurs

1. **Source de vérité unique** : un seul repo "amont" où on développe activement. Les autres sont des miroirs.
2. **Séparation code / strat** : le code et la documentation publique sont séparés des notes stratégiques privées.
3. **Pull > Push pour les miroirs** : les destinations tirent depuis la source, pas l'inverse. Plus sécurisé, moins de tokens à manipuler.
4. **Open source dès que possible, pas avant** : le repo passe public à la livraison du MVP 1, pas avant.

---

## 2. Topologie : 3 forges, 3 rôles

```
┌──────────────────────────────────────────────────────┐
│  GitHub  (privé jusqu'à MVP1 → puis public)          │
│                                                       │
│  RÔLE : SOURCE DE VÉRITÉ du code                     │
│  - Code source                                        │
│  - Issues, PRs, releases                              │
│  - CI/CD via GitHub Actions                           │
│  - Discussions, contributors                          │
│  - Releases packagées                                 │
└────────────────┬─────────────────────────────────────┘
                 │ (pull mirror automatique)
                 │
       ┌─────────┴──────────┐
       ▼                    ▼
┌──────────────┐    ┌──────────────────────────┐
│  Codeberg    │    │  GitLab self-hosted      │
│  (public)    │    │  (privé, chez toi)       │
│              │    │                          │
│ RÔLE :       │    │ RÔLE :                   │
│ - Visibilité │    │ - Backup souverain       │
│   éthique    │    │ - Résilience si GitHub   │
│ - Alternative│    │   inaccessible           │
│   européenne │    │ - Migration possible     │
└──────────────┘    └──────────────────────────┘

┌──────────────────────────────────────────────────────┐
│  GitLab self-hosted (privé permanent)                │
│                                                       │
│  RÔLE : KNOWLEDGE BASE STRATÉGIQUE                   │
│  - Décisions architecturales (ADR)                    │
│  - Roadmap features                                   │
│  - Catalogue APIs                                     │
│  - Doc RGPD                                           │
│  - Cette doc de stratégie repo                        │
│  - Notes business, juridiques                         │
│  - Contacts clients, prospects                        │
│  - Configs prod, secrets envelope (chiffrés)          │
│  - Tout ce qui ne doit JAMAIS être public            │
└──────────────────────────────────────────────────────┘
```

### 2.1 Pourquoi cette séparation est puissante

- **GitHub** porte le code ET la communauté → tu maximises adoption et contributions.
- **Codeberg** est un signal politique : "ce projet a aussi sa place dans l'écosystème éthique européen". Avec un coût en temps quasi-nul.
- **GitLab self-hosted comme miroir code** : assurance résilience. Si GitHub a un blocage majeur (sanctions internationales, ban géographique, problème technique multi-jours), tu as une copie complète chez toi.
- **GitLab self-hosted comme knowledge base privée** : c'est où vit toute la matière "fondatrice" du projet. Quand le code passera open source, ces docs resteront privées et continueront d'évoluer.

---

## 3. Détail des forges utilisées

### 3.1 GitHub

**URL** : https://github.com

**Rôle** : repo principal du code

**Configuration** :
- Compte personnel ou Organisation (Organisation recommandée pour pro)
- Pack **GitHub Education** activé (gratuit pour étudiants, inclut Copilot Pro, JetBrains, Sentry credits, Heroku credits, domaine gratuit Namecheap, etc.)
- Repo en **privé** au démarrage
- Branch protection sur `main`
- Dependabot activé
- Secret scanning activé
- Code scanning à activer au passage en public

**Avantages spécifiques** :
- Écosystème .NET principal
- GitHub Actions (CI/CD)
- Marketplace d'apps
- GitHub Sponsors (futur)

### 3.2 Codeberg

**URL** : https://codeberg.org

**Rôle** : miroir public, alternative éthique européenne

**Configuration** :
- Compte gratuit
- Repo créé en **public** dès le départ (puisque c'est le miroir d'un repo qui deviendra public)
- Pull mirror configuré depuis GitHub
- Pas de CI activée (la CI tourne sur GitHub)

**Avantages** :
- Hébergé en Allemagne 🇩🇪
- Association à but non lucratif
- Basé sur **Forgejo** (fork de Gitea, libre)
- Aucune dépendance aux GAFAM
- Très utilisé par la communauté open source européenne

### 3.3 GitLab self-hosted

**URL** : ton instance privée

**Double rôle** :

#### A. Repo "mirror" : copie du code GitHub
- Pull mirror configuré depuis GitHub
- Privé (pas accessible publiquement)
- Sert de **backup chaud**

#### B. Repo "internal" : knowledge base stratégique
- **Toujours privé**
- Contient les documents fondamentaux (ADR, roadmap, RGPD, etc.)
- Contient les configs prod chiffrées
- Contient les notes business, contacts, prospects
- **Indépendant de GitHub** (pas de mirroring)

---

## 4. Stratégie de mirroring

### 4.1 Pourquoi pull plutôt que push

| Aspect | Push (depuis GitHub) | Pull (depuis Codeberg/GitLab) |
|---|---|---|
| Stockage des tokens | Tokens Codeberg/GitLab dans GitHub Secrets | Aucun token côté GitHub |
| Risque en cas de fuite | Si un secret GitHub fuite, attaquant peut écrire sur Codeberg/GitLab | Aucun secret GitHub n'expose les miroirs |
| Mélange CI / mirroring | Logique de mirroring dans GitHub Actions | Indépendant du CI |
| Direction | Unidirectionnel mais via push hooks | Unidirectionnel par design |
| Configuration | À configurer dans chaque repo | À configurer une fois dans Codeberg/GitLab |

**Conclusion** : on fait du **pull** côté Codeberg et GitLab.

### 4.2 Configuration Codeberg

Sur Codeberg :

1. Créer un nouveau repo (vide)
2. Dans **Settings → Mirror Settings** (ou directement à la création) → cocher **"Mirror this repository"**
3. Renseigner :
   - URL source : `https://github.com/<user>/<repo>.git`
   - Authentification : token d'accès GitHub avec scope `repo` (read-only)
   - Intervalle : 30 minutes par défaut

Le miroir Codeberg pull automatiquement à intervalle régulier. **Aucune action requise côté GitHub.**

### 4.3 Configuration GitLab self-hosted

Sur GitLab :

1. Créer un nouveau projet
2. Aller dans **Settings → Repository → Mirroring repositories**
3. Renseigner :
   - URL : `https://github.com/<user>/<repo>.git`
   - Direction : **Pull**
   - Authentification : token GitHub avec scope `repo`
   - Cocher "Trigger pipelines for mirror updates" si tu veux que la CI GitLab tourne aussi (optionnel)

GitLab vérifie périodiquement les modifications (toutes les 30 minutes en standard, configurable en Premium).

### 4.4 Token GitHub pour mirroring

Créer un **Fine-grained Personal Access Token** GitHub :

- Scope **repository : only the mirrored repo**
- Permissions : **Contents (read-only)**, **Metadata (read-only)**
- Expiration : 1 an (à renouveler)

Ce token est **read-only**. S'il fuite, l'attaquant ne peut que lire ton code public (ou qui le deviendra) — risque très limité.

### 4.5 Test du mirroring

Procédure de test une fois configuré :

1. Sur GitHub, faire un commit anodin (ex. modifier le README)
2. Push sur `main`
3. Attendre 5–30 minutes (selon intervalle)
4. Vérifier que le commit apparaît sur Codeberg
5. Vérifier que le commit apparaît sur GitLab self-hosted

Si ça ne fonctionne pas : vérifier les logs côté Codeberg/GitLab (généralement dans les Settings → Mirroring).

---

## 5. Structure du repo principal

### 5.1 Arborescence type pour le repo de code

```
<projet>/
├── .github/
│   ├── workflows/                    # GitHub Actions
│   │   ├── ci.yml                    # Build + tests à chaque PR
│   │   ├── codeql.yml                # Analyse de sécurité
│   │   └── release.yml               # Publication des releases
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   ├── PULL_REQUEST_TEMPLATE.md
│   └── dependabot.yml
├── docs/
│   ├── README.md                     # Index de la doc publique
│   ├── architecture.md               # Vue d'ensemble archi
│   ├── getting-started.md            # Onboarding contributeur
│   ├── api-inpi-documentation.md     # Doc des APIs INPI (la nôtre)
│   └── images/
├── src/
│   ├── <Projet>.Domain/              # Core domain
│   ├── <Projet>.Application/         # Use cases
│   ├── <Projet>.Infrastructure/      # Adapters sortants
│   │   ├── <Projet>.Infrastructure.Inpi/
│   │   └── <Projet>.Infrastructure.Persistence/
│   ├── <Projet>.Api/                 # Web API ASP.NET Core
│   └── <Projet>.Maui/                # Client MAUI
├── tests/
│   ├── <Projet>.Domain.Tests/
│   ├── <Projet>.Application.Tests/
│   ├── <Projet>.Infrastructure.Tests/
│   └── <Projet>.Api.IntegrationTests/
├── scripts/
│   ├── setup-dev.sh
│   └── ...
├── .editorconfig                     # Conventions formatage
├── .gitignore                        # Spécifique .NET + MAUI + IDE
├── .gitattributes                    # Gestion des fins de ligne
├── CHANGELOG.md                      # Suivi des changements
├── CODE_OF_CONDUCT.md                # À ajouter avant ouverture publique
├── CONTRIBUTING.md                   # À ajouter avant ouverture publique
├── LICENSE                           # AGPL v3
├── README.md                         # Présentation du projet
├── SECURITY.md                       # Politique de sécurité (reporting vulns)
├── <projet>.sln                      # Solution .NET
└── global.json                       # Pin version SDK .NET
```

### 5.2 Fichiers critiques à créer en premier

#### 5.2.1 `.gitignore`

Combinaison des templates :
- `dotnet`
- `visualstudio`
- `rider`
- `vscode`
- `maui`

Disponible sur https://www.toptal.com/developers/gitignore en cumulant ces tags.

**Lignes critiques à ajouter manuellement** :

```
# Secrets et configs locales
.env
.env.*
*.secrets.json
appsettings.Development.json
appsettings.Local.json
secrets/

# Données utilisateur de test
data/

# Outils
.tools/
.vs/
.idea/

# Build outputs
bin/
obj/
publish/
*.binlog
```

#### 5.2.2 `.editorconfig`

Standard .NET avec quelques règles strictes :

```
root = true

[*]
charset = utf-8
end_of_line = lf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csx}]
csharp_new_line_before_open_brace = all
csharp_indent_case_contents = true
dotnet_sort_system_directives_first = true

[*.{json,yml,yaml,md}]
indent_size = 2
```

#### 5.2.3 `README.md`

Structure minimale pour un projet open source :

```markdown
# <Nom du projet>

> Tagline en une phrase qui dit ce que ça fait.

## Description

Paragraphe d'intro (3-5 lignes).

## Statut

Badge build / coverage / version / license.

## Fonctionnalités

Liste à puces.

## Démarrage rapide

Code snippets pour installer et lancer.

## Documentation

Lien vers `/docs`.

## Architecture

Diagramme + lien vers `docs/architecture.md`.

## Contribuer

Lien vers `CONTRIBUTING.md`.

## Licence

AGPL v3 — voir `LICENSE`.
```

#### 5.2.4 `LICENSE`

Le texte complet de l'AGPL v3 : https://www.gnu.org/licenses/agpl-3.0.txt

#### 5.2.5 `SECURITY.md`

Politique de sécurité :

```markdown
# Politique de sécurité

## Versions supportées

| Version | Supportée |
|---------|-----------|
| 1.x     | ✅        |
| 0.x     | ❌        |

## Signaler une vulnérabilité

Merci de **ne pas** ouvrir d'issue publique pour une faille de sécurité.

Envoyez un email à **security@<domaine>** avec :
- Description de la vulnérabilité
- Étapes de reproduction
- Impact potentiel

Vous recevrez une réponse sous 48h ouvrées.
```

---

## 6. Stratégie de branches

### 6.1 Modèle : trunk-based development simplifié

Pour un projet solo en démarrage, **GitFlow est overkill**. On adopte un modèle simple :

```
main (branche principale, toujours déployable)
  ├── feature/inpi-rne-search       (feature en cours)
  ├── feature/auth-2fa
  ├── fix/login-redirect
  └── chore/update-dependencies
```

### 6.2 Règles

- **`main`** : toujours en état déployable. Branch protection : pas de push direct, PR obligatoire.
- **Branches courtes** : une branche par feature, mergée sous 1 semaine si possible. Évite les conflits massifs.
- **Naming** :
  - `feature/<nom>` pour les nouvelles fonctionnalités
  - `fix/<nom>` pour les corrections de bug
  - `chore/<nom>` pour la maintenance (déps, refactor)
  - `docs/<nom>` pour la doc
  - `test/<nom>` pour les tests

### 6.3 Pull Requests

- Description claire : **Quoi**, **Pourquoi**, **Comment tester**
- Lier l'issue concernée (`Closes #42`)
- CI doit être verte
- Self-review obligatoire (relire son propre PR avant de merger)
- Squash and merge pour garder un historique propre

### 6.4 Releases

Modèle **SemVer** (Semantic Versioning) : `MAJOR.MINOR.PATCH`

- **MAJOR** : breaking changes API
- **MINOR** : nouvelle fonctionnalité, rétrocompatible
- **PATCH** : bug fix

Tags Git : `v0.1.0`, `v0.2.0`, etc.

GitHub Releases avec notes de version structurées (générées automatiquement via Conventional Commits).

---

## 7. Conventions de commits

### 7.1 Conventional Commits

Format **standardisé** : https://www.conventionalcommits.org/

```
<type>(<scope>): <description courte>

[corps optionnel]

[footer optionnel]
```

### 7.2 Types

| Type | Usage |
|---|---|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `docs` | Documentation uniquement |
| `style` | Formatage, point-virgules manquants, etc. (pas de code) |
| `refactor` | Refactor sans changement de comportement |
| `perf` | Amélioration de performance |
| `test` | Ajout ou modification de tests |
| `chore` | Maintenance (déps, build, etc.) |
| `ci` | Modifications CI/CD |
| `build` | Modifications du système de build |
| `revert` | Annulation d'un commit précédent |

### 7.3 Exemples

```
feat(inpi-rne): add search by SIREN
fix(auth): correct JWT expiration handling
docs(api): update endpoints documentation
chore(deps): bump Microsoft.AspNetCore to 9.0.5
refactor(domain): extract Company value object
test(integration): add INPI auth tests
```

### 7.4 Bénéfices

- **Changelog automatique** : outils comme `release-please` génèrent le CHANGELOG.md
- **Versioning automatique** : détection automatique de la prochaine version SemVer
- **Lisibilité** de l'historique
- **Recherche facilitée** dans `git log`

### 7.5 Outils

- **`commitlint`** + **`husky`** : vérifie le format avant commit
- **`commitizen`** : assistant interactif pour rédiger des commits

---

## 8. Gestion des secrets

### 8.1 Aucun secret commité — jamais

Règle absolue, sans exception.

### 8.2 Niveaux de stockage

| Type de secret | Où le stocker |
|---|---|
| Secrets de dev local | `appsettings.Development.json` (gitignored) ou `dotnet user-secrets` |
| Secrets de CI | GitHub Actions Secrets |
| Secrets de production | KMS (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) |
| Secrets perso (clés API perso) | Coffre-fort de mots de passe (Bitwarden, 1Password) |

### 8.3 Scanner automatique

À mettre en place **avant le premier commit** :

- **gitleaks** en pre-commit hook local (via `lefthook` ou `husky`)
- **gitleaks** en GitHub Action (bloque le PR si secret détecté)

### 8.4 Que faire si un secret a fuité

1. **Changer immédiatement le secret** côté service (rotation)
2. Le supprimer du repo (BFG Repo-Cleaner ou git-filter-repo)
3. Force-push (à éviter sauf nécessité absolue)
4. Notifier les équipes / collaborateurs
5. Auditer les logs d'accès au service concerné

**Important** : supprimer un secret de l'historique Git **ne suffit pas**. Les services tiers (GitHub, miroirs, caches) ont déjà copié l'info. La seule vraie solution est la rotation du secret.

---

## 9. CI/CD

### 9.1 GitHub Actions

Pipeline minimal pour le MVP :

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore --configuration Release
      - name: Test
        run: dotnet test --no-build --configuration Release --verbosity normal
```

### 9.2 Pipelines à mettre en place progressivement

| Pipeline | Trigger | Action |
|---|---|---|
| **CI build + test** | Push & PR | `dotnet build` + `dotnet test` |
| **Lint** | PR | Format C# (`dotnet format`), markdown, YAML |
| **Security scan** | PR + nightly | `dotnet list package --vulnerable`, gitleaks |
| **Code quality** | PR | SonarCloud (gratuit pour OSS) |
| **CodeQL** | Push + weekly | Analyse de sécurité GitHub |
| **Release** | Tag `v*` | Build, package, publish GitHub Release |
| **Deploy preview** | PR | Déploiement éphémère pour review |
| **Deploy prod** | Push sur `main` (après validation) | Déploiement production |

### 9.3 Scaling de la CI

- **Pour démarrer** : tout sur GitHub Actions (free tier généreux)
- **Si dépassement free tier** : héberger des runners self-hosted sur ton serveur GitLab self-hosted
- **Très long terme** : déléguer CI lourde (tests d'intégration, build mobile) à des runners dédiés

---

## 10. Lifecycle : privé → public

### 10.1 Phase 1 : repo privé (mois 0–4)

**Objectif** : itérer librement, faire des erreurs sans audience.

- Repo GitHub en privé
- Miroirs Codeberg et GitLab désactivés (ou repos miroirs aussi en privé)
- CI active
- Commits réguliers
- Travail sur le MVP 1

### 10.2 Phase 2 : préparation passage public

À faire **avant** de basculer en public :

- [ ] **Auditer l'historique Git** : aucun secret, aucune donnée sensible, aucune URL interne
- [ ] **Vérifier les commit author** : pas d'info perso non souhaitée
- [ ] **Mettre à jour le README** : présentation propre, badges, screenshots
- [ ] **Ajouter `CONTRIBUTING.md`**
- [ ] **Ajouter `CODE_OF_CONDUCT.md`** (template Contributor Covenant)
- [ ] **Configurer Issue Templates et PR Template**
- [ ] **Vérifier la licence** : LICENSE complet et headers dans fichiers
- [ ] **Activer les fonctions GitHub** : Discussions, Wiki si besoin
- [ ] **Configurer GitHub Pages** pour la doc si voulu
- [ ] **Préparer l'annonce** : article de blog, post LinkedIn, Reddit r/dotnet, Hacker News

### 10.3 Phase 3 : passage public

1. Sur GitHub : Settings → Change repository visibility → Public
2. Sur Codeberg : créer le repo public (s'il n'existait pas) et activer le mirror
3. **Annoncer** ! C'est aussi du travail :
   - Article de blog sur Dev.to ou Medium
   - Post LinkedIn
   - Post Mastodon
   - Reddit r/dotnet, r/csharp, r/opendata
   - Hacker News si pertinent

### 10.4 Phase 4 : vie publique

Routines à intégrer :

- Répondre aux issues sous 7 jours
- Reviewer les PRs externes sous 14 jours
- Tagger des releases régulières (au moins mensuelles)
- Communiquer sur les évolutions (changelog, blog post)
- Modérer la communauté (Code of Conduct)

---

## 11. Plan de mise en place

### Étape 1 — GitLab self-hosted "internal" (à faire en premier)

**Objectif** : sécuriser dès maintenant les docs stratégiques.

1. Sur ton GitLab self-hosted, créer un projet **privé** nommé `<projet>-internal`
2. Initialiser avec un README simple
3. Créer une arborescence :
   ```
   <projet>-internal/
   ├── README.md
   ├── 00-decisions/          # ADRs (le fichier 01-decisions-architecturales.md)
   ├── 01-features/           # Roadmap (le fichier 02-roadmap-features.md)
   ├── 02-apis/               # Catalogue APIs (le fichier 03-catalogue-apis-publiques.md)
   ├── 03-securite/           # RGPD (le fichier 04-securite-rgpd.md)
   ├── 04-repos/              # Ce fichier (05-strategie-repos.md)
   ├── 05-business/           # Notes business futures
   ├── 06-contacts/           # Contacts clients/prospects (chiffrés)
   └── 07-configs/            # Configs prod sensibles (chiffrées)
   ```
4. Commiter les 5 docs qu'on a produits ensemble
5. Backup régulier de ce repo (snapshot du serveur GitLab)

### Étape 2 — Choix du nom + réservation des domaines

(Avant les étapes suivantes, il faut un nom — voir doc dédiée nom du projet)

- Réserver les domaines (.com, .fr, .dev au minimum)
- Réserver les handles GitHub, Codeberg
- Vérifier marques INPI / EUIPO

### Étape 3 — GitHub repo principal

1. Activer GitHub Education sur ton compte (https://education.github.com)
2. Créer une **Organisation GitHub** au nom du projet (recommandé vs compte perso)
3. Créer le repo `<projet>` en **privé**
4. Initialiser localement :
   ```bash
   git clone https://github.com/<org>/<projet>
   cd <projet>
   ```
5. Créer les fichiers de base :
   - `.gitignore` (dotnet, visualstudio, maui, jetbrains)
   - `.editorconfig`
   - `LICENSE` (AGPL v3 complète)
   - `README.md` (provisoire)
   - `SECURITY.md`
   - `global.json` (pin .NET SDK version)
6. Premier commit signé :
   ```bash
   git commit -S -m "chore: initial commit"
   ```
7. Configurer branch protection sur `main`
8. Activer Dependabot, secret scanning
9. Créer le label set initial (bug, enhancement, documentation, good first issue, etc.)

### Étape 4 — Codeberg miroir public

1. Créer un compte sur https://codeberg.org
2. Créer un repo `<projet>` en **public** (peut être public dès le début, il sera vide jusqu'au passage public de GitHub)
3. Pour l'instant, **ne pas activer le mirror** (laisser vide jusqu'à passage public GitHub)
4. Documenter le projet dans la description Codeberg : "Mirror of github.com/.../..."

**OU** alternative : activer le mirror dès maintenant mais garder le repo Codeberg **privé** jusqu'au passage public.

### Étape 5 — GitLab self-hosted miroir

1. Sur GitLab self-hosted, créer un projet `<projet>-mirror`
2. Settings → Repository → Mirroring repositories
3. Configurer le pull mirror depuis GitHub avec un token read-only
4. Tester avec un commit factice
5. Documenter dans le README du miroir : "Backup mirror of github.com/.../..."

### Étape 6 — Setup CI initial

1. Créer `.github/workflows/ci.yml` minimal (build + test)
2. Pousser, vérifier que la CI tourne
3. Ajouter Dependabot config (`.github/dependabot.yml`)
4. Ajouter CodeQL workflow

### Étape 7 — Premier commit de structure projet

À ce stade, on peut commencer à monter la solution .NET :

```bash
dotnet new sln -n <Projet>
dotnet new classlib -n <Projet>.Domain -o src/<Projet>.Domain
# ... etc.
```

C'est l'étape qui suit dans la roadmap globale (cf. doc d'architecture détaillée à venir).

---

## Annexe — Outils recommandés

| Besoin | Outil | Statut |
|---|---|---|
| Pre-commit hooks | `lefthook` ou `husky.net` | Recommandé dès le départ |
| Scan secrets | `gitleaks` | Indispensable |
| Lint commit | `commitlint` | Recommandé |
| Format code | `dotnet format` (intégré .NET) | Indispensable |
| Versioning | `release-please` ou `MinVer` | Recommandé |
| Coverage | `coverlet` + `reportgenerator` | Recommandé |
| Diagrammes | `Mermaid` (intégré GitHub) | Recommandé |
| Doc technique | `Docusaurus` ou `MkDocs` | Recommandé à terme |

---

*Document évolutif. À mettre à jour à chaque changement de stratégie repo.*
