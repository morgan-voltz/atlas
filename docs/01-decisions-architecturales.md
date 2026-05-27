# Décisions architecturales & stratégiques

> Document de référence des décisions structurantes prises pour le projet.
> Format inspiré des **Architecture Decision Records (ADR)** de Michael Nygard — chaque décision est tracée avec son contexte, sa rationale et ses conséquences.
> Toute évolution de ces décisions doit être documentée ici (versioning, dates de modif).

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026
**Statut** : Phase de cadrage — décisions figées avant développement.

---

## Sommaire

- [Vision du projet](#vision-du-projet)
- [ADR-001 — Modèle d'usage multi-utilisateur SaaS](#adr-001--modèle-dusage-multi-utilisateur-saas)
- [ADR-002 — Topologie hybride (Core lib partagé + backend séparé)](#adr-002--topologie-hybride)
- [ADR-003 — Authentification INPI en multi-tenant credentials](#adr-003--authentification-inpi-multi-tenant)
- [ADR-004 — Architecture hexagonale "platform-ready"](#adr-004--architecture-hexagonale-platform-ready)
- [ADR-005 — Licence open source AGPL v3](#adr-005--licence-open-source-agpl-v3)
- [ADR-006 — Modèle économique : OSS pur 6 mois, puis SaaS hébergé](#adr-006--modèle-économique)
- [ADR-007 — Stack technique .NET / MAUI](#adr-007--stack-technique-net--maui)
- [ADR-008 — Engagement accessibilité WCAG 2.2 AA + RGAA 4.1.2](#adr-008--engagement-accessibilité-wcag-22-aa--rgaa-412)
- [ADR-009 — Veille comme feature majeure du produit + open core](#adr-009--veille-comme-feature-majeure-du-produit--open-core)
- [ADR-010 — Authentification utilisateur hexagonale custom](#adr-010--authentification-utilisateur-hexagonale-custom)
- [Décisions à prendre ultérieurement](#décisions-à-prendre-ultérieurement)
- [Roadmap macro](#roadmap-macro)
- [Glossaire](#glossaire)

---

## Vision du projet

**Nom de code** : à définir (suggestions : `Openpriv`, `Datahex`, `Frenchdata`, `Civicdata` — à figer avant le repo).

**Pitch en une phrase** :
> Une plateforme open source pour interagir avec l'écosystème des données publiques françaises sur les entreprises et la propriété industrielle, accessible depuis le bureau, le mobile et le web.

**Pitch étendu** :
La France possède un écosystème open data parmi les plus riches du monde (RNE, Sirene, BODACC, DECP, INPI PI, Judilibre, etc.), mais ces APIs sont **fragmentées**, **techniquement hétérogènes** (auth, formats, rate limits) et **mal intégrées** entre elles dans les outils existants. Les solutions commerciales (Pappers, Societe.com) agrègent ces sources mais en mode boîte noire, payant pour les usages avancés, et avec une API limitée.

Le projet propose une **alternative open source** qui :

1. **Unifie l'accès** à ces sources via une couche d'abstraction propre (architecture hexagonale).
2. **Respecte les flags de diffusion** et le RGPD nativement.
3. **Donne le contrôle aux utilisateurs** : ils gardent leurs credentials, peuvent auto-héberger, ou utiliser une instance SaaS gérée.
4. **Sert de référence technique** pour la communauté dev française qui veut bâtir des outils sur l'open data entreprises.

**Cible primaire à long terme** : développeurs, cabinets juridiques / d'expertise comptable, services compliance, intégrateurs B2B. **Cible primaire à court terme (phase 1)** : développeurs et communauté open source.

---

## ADR-001 — Modèle d'usage multi-utilisateur SaaS

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Le projet pouvait s'orienter en outil **mono-utilisateur** (application installée localement avec les identifiants INPI de l'utilisateur) ou en **multi-utilisateur SaaS** (plateforme hébergée gérant plusieurs comptes utilisateurs distincts).

### Décision

**Multi-utilisateur SaaS**, avec gestion centralisée des comptes utilisateurs, sessions, profils, abonnements (futurs) et données associées.

### Rationale

- Permet une **synchronisation multi-device** (un user qui utilise l'app sur son desktop ET son mobile MAUI doit retrouver ses données).
- Permet les **fonctions asynchrones** (alertes, veille, agents de surveillance) qui nécessitent un backend tournant 24/7.
- Permet une **base utilisateurs commune** facilitant le support, l'analyse d'usage, et plus tard la monétisation.
- Aligne avec la cible (multi-segments : cabinets, devs, etc.) où chaque user a un environnement personnalisé.

### Conséquences

- **Positives** : flexibilité d'évolution, modèle économique possible plus tard.
- **Négatives** : complexité accrue (auth, RGPD, sécurité, scalabilité). Coûts d'hébergement à anticiper.
- **À prévoir** : politique de confidentialité, conditions générales d'utilisation, registre des traitements RGPD.

---

## ADR-002 — Topologie hybride

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Trois topologies étaient en lice :
- **A. Tout local** : MAUI consomme une lib partagée qui tape directement les API externes.
- **B. Backend séparé strict** : ASP.NET Core héberge toute la logique, MAUI / Blazor sont des clients HTTP purs.
- **C. Hybride** : un projet `Core` partagé entre backend et clients pour le domaine pur (entités, value objects, validations), avec backend séparé pour la logique métier sensible et les appels API externes.

### Décision

**Topologie hybride (C)** :
- Un projet `Core` (ou `Domain`) contient les entités, value objects, validations métier purs.
- Cette lib est **partagée** entre le backend ASP.NET Core et le client MAUI.
- Toute la logique d'orchestration, d'appel aux APIs INPI/externes, et de persistance vit **uniquement dans le backend**.
- Les clients (MAUI, futur Blazor) consomment le backend via une API REST authentifiée.

### Rationale

- **L'option A est incompatible** avec le multi-utilisateur SaaS (ADR-001) : pas de partage de données entre devices, pas de tâches asynchrones, et credentials INPI dans le binaire MAUI = fuite assurée.
- **L'option B fonctionne** mais duplique souvent les entités entre backend (DTOs) et client (modèles UI), créant de la dette technique.
- **L'option C** offre un compromis idéal pour le contexte .NET : un seul vocabulaire métier réutilisable, validations cohérentes côté client et serveur, sans compromettre la sécurité (les use cases sensibles restent côté serveur).

### Conséquences

- **Positives** : DRY sur le domaine, validations cohérentes, un seul endroit pour les règles métier de base, possibilité d'ajouter d'autres clients (CLI, Blazor) sans refactor.
- **Négatives** : discipline nécessaire pour ne **PAS** mettre de logique dépendant d'infrastructure dans `Core`. Risque de "leak" si on n'est pas vigilant.
- **Règle stricte** : `Core` n'a **AUCUNE référence** à `Microsoft.AspNetCore.*`, `EntityFramework`, `HttpClient`, etc. C'est du C# pur, testable sans I/O.

---

## ADR-003 — Authentification INPI multi-tenant

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Le SaaS doit interagir avec les API INPI au nom de ses utilisateurs. Trois stratégies étaient possibles :
- **Compte maître** : un compte INPI partagé par tout le SaaS.
- **Multi-tenant credentials** : chaque utilisateur fournit ses propres identifiants INPI.
- **Hybride** : compte maître pour fonctions limitées, credentials user pour fonctions avancées.

### Décision

**Multi-tenant credentials** : chaque utilisateur du SaaS crée son propre compte INPI et fournit ses identifiants à la plateforme, qui les stocke chiffrés.

### Rationale

- **Conformité aux CGU INPI** : les comptes INPI sont nominatifs. Faire passer N utilisateurs par un seul compte est une violation potentielle.
- **Protection juridique** : en cas de litige, chaque utilisateur est responsable de son propre accès. Le SaaS n'est qu'un opérateur technique.
- **Scalabilité naturelle** : pas de rate limit partagé, chaque user a sa propre enveloppe.
- **Pérennité** : si l'INPI durcit ses CGU, le SaaS n'est pas impacté.

### Conséquences

- **Positives** : posture juridique safe, scalabilité, alignement avec les patterns SaaS B2B modernes (Zapier, Make).
- **Négatives** : friction d'onboarding (chaque user doit créer un compte INPI). Risque de support utilisateur sur "comment je crée mon compte INPI ?".
- **À prévoir** :
  - Wizard d'onboarding très soigné pour guider l'utilisateur dans la création de son compte INPI.
  - Stockage chiffré des credentials (per-user encryption keys, idéalement via un KMS).
  - Rotation et révocation des credentials.
  - Tests de connectivité automatiques pour détecter les mots de passe expirés.

### Possible évolution future

La piste **hybride** reste ouverte. Si la pertinence émerge, on pourra ajouter un compte maître pour des fonctions "preview" (consultation de données publiques cachées en local par le SaaS) avant que l'utilisateur ne connecte son propre compte. Cette évolution est non-bloquante avec l'architecture hexagonale prévue.

---

## ADR-004 — Architecture hexagonale platform-ready

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Trois ambitions d'architecture étaient discutées :
- **A. Focused INPI v1** : on code pour INPI uniquement, on étend plus tard.
- **B. Platform-ready** : on architecture comme si on allait brancher 10 sources, on n'en implémente qu'une au début.
- **C. Multi-sources dès la v1** : on implémente INPI + BODACC + Sirene dès le premier livrable.

### Décision

**Option B : Platform-ready, INPI seul en v1**.

L'architecture suit le pattern **Ports & Adapters / Clean Architecture** :
- Le **Core** définit des **ports** (interfaces) en termes métier (ex. `ICompanyDataProvider`, `IIntellectualPropertyProvider`).
- L'**Infrastructure** contient des **adapters** qui implémentent ces ports avec une source spécifique (ex. `InpiRneCompanyDataProvider`).
- Le **domaine ne connaît pas l'INPI**. Il connaît la notion de `Company`, `LegalEntity`, `Trademark`, etc.

### Rationale

- **Effort initial faible** : un port bien défini ne coûte pas plus cher qu'un service couplé à l'INPI.
- **Évolution massive facilitée** : ajouter BODACC, Sirene ou EUIPO = écrire un nouvel adapter, **zéro modification du domaine ou des use cases**.
- **Testabilité** : les use cases sont testables avec des mocks des ports, sans appeler l'INPI.
- **Substitution** : si l'INPI change radicalement son API, on remplace l'adapter sans toucher au reste.
- **L'option A** sous-utilise la puissance de l'archi hex. **L'option C** est trop ambitieuse pour un projet porté par une personne en parallèle des études.

### Conséquences

- **Positives** : flexibilité, testabilité, séparation claire des responsabilités, faible couplage.
- **Négatives** : courbe d'apprentissage initiale plus raide. Risque de sur-ingénierie si on perd la mesure (pas besoin de 4 niveaux d'abstraction pour des cas simples).
- **À prévoir** : règles de dépendance strictes (le domaine ne dépend de RIEN ; l'application dépend du domaine ; l'infrastructure dépend du domaine et de l'application). Outillage : tests d'architecture (NetArchTest, ArchUnitNET).

---

## ADR-005 — Licence open source AGPL v3

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Le projet sera publié en open source (cf. ADR-006). Le choix de la licence détermine ce que les tiers peuvent faire du code et ce qu'ils doivent rendre à la communauté en échange.

### Décision

**AGPL v3 (GNU Affero General Public License version 3)**.

### Rationale

- **Copyleft fort** : toute modification utilisée pour offrir un service en ligne doit être ouverte. Empêche les "vampires" (AWS, Scaleway, etc.) de fork et héberger une version concurrente fermée.
- **Compatible monétisation future** : permet le **dual-licensing** (le code reste AGPL pour les particuliers, on offre une licence commerciale aux boîtes qui ne veulent pas s'engager à ouvrir leurs modifs).
- **Crédibilité** sur un projet manipulant des credentials sensibles : la communauté peut auditer.
- **Standard reconnu** par l'OSI (Open Source Initiative) et la FSF.

### Conséquences

- **Positives** : protection contre l'exploitation commerciale fermée, option dual-licensing, transparence.
- **Négatives** : adoption potentiellement plus lente que MIT/Apache (les boîtes ont parfois peur de l'AGPL). Contributions parfois freinées par des Contributor License Agreements (CLA) à signer si on veut conserver l'option dual-licensing.
- **À prévoir** :
  - Fichier `LICENSE` à la racine du repo dès la création.
  - Header AGPL dans chaque fichier source (ou au moins dans les fichiers principaux).
  - CLA pour les contributeurs externes si dual-licensing envisagé.
  - Documentation claire de la licence dans le README.

---

## ADR-006 — Modèle économique

**Statut** : ✅ Accepté (phasage)
**Date** : 26 mai 2026

### Contexte

Plusieurs modèles économiques étaient envisageables : open core (libre + premium fermé), OSS + SaaS hébergé payant, pur libre + donations / support, ou aucun.

### Décision

**Phasage en 4 étapes** :

1. **Mois 0–6** : 100% open source, gratuit, **aucune monétisation**. Focus : adoption, visibilité, communauté.
2. **Mois 6–12** : maintien open source, premier revenu via **support / consulting** sur demande.
3. **Mois 12–24** : lancement d'une **version SaaS hébergée payante** (instance gérée, alertes, backups, support). Le code reste 100% libre, on monétise la commodité.
4. **Mois 24+** : selon traction, potentiel passage à un modèle **open core** (fonctions premium fermées) ou maintien du modèle pur SaaS-hébergé.

### Rationale

- **Pas de monétisation prématurée** : éviter de brûler des opportunités de feedback gratuit avec des early adopters payants.
- **Construire la réputation d'abord** : un projet open source bien fait est un atout de carrière, indépendamment de toute monétisation.
- **Modèle éprouvé** : c'est la trajectoire de Plausible Analytics, Posthog, Supabase, Cal.com.
- **Compatible avec la situation actuelle** : étudiant + dev indé, sans levée, peut se permettre 6 mois sans revenu direct du projet.

### Conséquences

- **Positives** : flexibilité, validation marché organique, communauté construite avant la monétisation.
- **Négatives** : aucun revenu pendant 6+ mois. Demande de la patience et de la discipline pour ne pas céder à la tentation de monétiser trop tôt.
- **Indicateurs à suivre** :
  - Mois 6 : 100+ stars GitHub, 5+ users actifs, 2+ articles techniques publiés.
  - Mois 12 : 500+ stars, 50+ users, premiers contacts pour du support payant.
  - Mois 24 : 100+ users sur l'instance hébergée payante.

---

## ADR-007 — Stack technique .NET / MAUI

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

Le choix de la stack technique est contraint par le cursus actuel (MAUI étudié) et l'envie d'un écosystème pro et stable.

### Décision

**Backend** :
- **.NET 10 LTS** (version courante stable depuis novembre 2025, supportée jusqu'à novembre 2028). Migration possible vers .NET 11 STS à sa sortie si bénéfices techniques justifient le passage à un cycle STS.
- **ASP.NET Core** (Minimal APIs pour la simplicité, ou MVC controllers selon préférence).
- **Entity Framework Core** pour la persistance, avec **PostgreSQL** comme SGBD (open source, mature, supporté partout).
- **MediatR** ou pattern handler manuel pour les use cases (à décider en phase architecture détaillée).
- **FluentValidation** pour les validations métier.
- **Polly** pour la résilience HTTP (retry, circuit breaker, timeouts) sur les appels INPI.
- **Serilog** + **OpenTelemetry** pour le logging et l'observabilité.

**Clients** :
- **.NET MAUI** pour iOS / Android / Windows / macOS.
- **Blazor Server ou WebAssembly** pour le web (décision différée).

**Outillage** :
- **xUnit** pour les tests unitaires.
- **Testcontainers** pour les tests d'intégration avec PostgreSQL réel.
- **NetArchTest** ou **ArchUnitNET** pour les tests d'architecture (vérifier que les dépendances respectent l'archi hex).
- **GitHub Actions** ou **GitLab CI** pour le CI/CD (décision : voir doc séparée sur le repo).

### Rationale

- **Cohérence** : tout .NET, une seule stack à maîtriser pour le porteur du projet.
- **Maturité** : ASP.NET Core et MAUI sont stables, supportés long terme par Microsoft.
- **Open source** : tout cet écosystème est open source et utilisable librement.
- **PostgreSQL** : meilleur SGBD open source, supporte le JSON natif (utile pour stocker les réponses brutes INPI), gratuit même en production.

### Conséquences

- **Positives** : productivité maximale, écosystème riche, pas de fragmentation langages.
- **Négatives** : moins d'attrait pour certains contributeurs open source (la communauté .NET est plus restreinte que JS/Python).
- **À prévoir** : documentation onboarding contributeurs pour faciliter l'arrivée de devs non-.NET.

---

## ADR-008 — Engagement accessibilité WCAG 2.2 AA + RGAA 4.1.2

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

L'accessibilité numérique est légalement encadrée en France et en UE depuis l'**European Accessibility Act** entré en vigueur le 28 juin 2025. Au-delà de la conformité légale, c'est une **question éthique et stratégique** : 17% de la population française vit avec au moins une forme de handicap, et l'accessibilité est trop souvent reportée à un hypothétique "plus tard" qui n'arrive jamais.

Trois niveaux étaient envisageables :
- WCAG 2.2 niveau A (minimum strict, insuffisant pour un produit pro)
- **WCAG 2.2 niveau AA** (standard pro reconnu, exigence légale française)
- WCAG 2.2 niveau AAA (excellence, rarement applicable à tout)

Trois périmètres également :
- Visuel uniquement
- Visuel + moteur + cognitif (quasi-gratuit si bien fait)
- Les 4 axes dès MVP1 (auditif inclus)

### Décision

**Niveau cible** : **WCAG 2.2 AA + RGAA 4.1.2**, avec préparation à RGAA 5 attendu fin 2026.

**Périmètre** : **les 4 axes du handicap (visuel, auditif, moteur, cognitif) dès MVP 1**.

**Discipline** : l'accessibilité est intégrée comme **critère bloquant de Definition of Done** sur chaque feature. Une PR ne peut pas être mergée si la checklist accessibilité n'est pas passée.

### Rationale

- **Conformité légale anticipée** : initialement exempté (TPE < 10 salariés et < 2M€ CA), le projet sera soumis à l'EAA dès dépassement des seuils. Mieux vaut concevoir accessible que refondre.
- **Économie à long terme** : intégrer l'accessibilité dès le MVP coûte ~15-25% de temps en plus ; la rajouter après coup coûte 5 à 10 fois plus cher.
- **Différenciation produit** : la majorité des SaaS français en 2026 ne sont pas réellement accessibles. Argument commercial vis-à-vis de clients institutionnels (cabinets, associations, ESN).
- **Qualité globale** : un design accessible est presque toujours un meilleur design.
- **Couvrir les 4 axes** : faire le visuel correctement couvre automatiquement 80% du moteur (navigation clavier) et 80% du cognitif (structure sémantique). L'auditif a peu de surface en MVP 1.

### Conséquences

- **Positives** : robustesse, conformité, différenciation, qualité produit.
- **Négatives** : ~15-25% de temps en plus par feature MVP 1. Nécessite outillage (Accessibility Insights, Color Oracle, lecteurs d'écran) et discipline.
- **À prévoir** :
  - Checklist accessibilité dans le template de PR (doc 05)
  - Outils en CI (axe-core, accessibility scanner)
  - Tests manuels périodiques (lecteurs d'écran)
  - Page "Déclaration d'accessibilité" sur le site
  - Tests utilisateurs avec personnes en situation de handicap à partir du MVP 2
- **Référentiel détaillé** : voir document `06-accessibilite.md`

---

## ADR-009 — Veille comme feature majeure du produit + open core

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

### Contexte

L'analyse du marché français des outils sur données entreprises (Pappers, Societe.com, Annuaire des Entreprises) révèle qu'**aucun acteur ne combine les données entreprises avec un système de veille (flux RSS, BODACC, BOPI, actualités sectorielles)**. À l'inverse, les lecteurs RSS / outils de veille (Feedly, Inoreader) n'intègrent pas les données entreprises.

Cette absence constitue un **trou de marché clair**, exploitable par le projet.

La feature "veille agrégée" est conceptuellement simple mais soulève plusieurs choix structurants :
- Place dans la roadmap (MVP 1, MVP 1.5, MVP 2 ?)
- Mode de découverte des sources (templates pré-curés vs ajout libre)
- Niveau d'enrichissement (basique vs IA)
- Modèle économique (gratuit vs open core vs premium)

### Décision

**Positionnement** : la veille devient une **feature majeure et différenciante** du produit, exploitant le trou de marché identifié.

**Place dans la roadmap** : **MVP 2** (mois 4–8 dans le planning). On valide d'abord le cœur RNE/PI en MVP 1, puis on ajoute la veille comme différenciateur.

**Découverte des sources** : **templates pré-curés par métier** (Cabinet PI, Expert-comptable, Compliance, Investisseur, etc.) **+ ajout libre** par l'utilisateur.

**Niveau d'enrichissement initial (MVP 2)** : **moyen** — déduplication + filtres + alertes. Pas d'IA en MVP 2.

**Modèle économique** : **open core aligné sur ADR-006**. Le moteur de veille reste open source. Les features d'enrichissement IA (résumés automatiques, scoring de pertinence, synthèse hebdomadaire) deviendront premium **uniquement en phase 3-4** (mois 12+), pas avant.

### Découpage fonctionnel précis

| Sous-feature | Statut futur |
|---|---|
| Lecteur RSS + abonnements | Gratuit / OSS définitif |
| Templates de veille par métier | Gratuit / OSS définitif |
| Ajout libre de sources | Gratuit / OSS définitif |
| Déduplication + filtres | Gratuit / OSS définitif |
| Alertes email + push | Gratuit (limites quantitatives en hébergé selon plan) |
| **IA : résumés automatiques** | Premium (phase 3+) |
| **IA : scoring de pertinence** | Premium (phase 3+) |
| **IA : synthèse hebdomadaire** | Premium (phase 3+) |
| **Collaboration / partage équipe** | Premium (phase 3+) |
| Marketplace de templates partagés | Gratuit |
| API publique pour la veille | Gratuit auto-hosted, quotas si hébergé |

### Rationale

- **Trou de marché clair** : aucun concurrent direct ne combine ces fonctions.
- **Lead magnet puissant** : la veille gratuite attire des users qui resteront ensuite pour les fonctions premium.
- **Cohérence stratégique** : aligné avec ADR-006 (OSS pur jusqu'à ~mois 12, puis SaaS hébergé + open core).
- **Faisabilité technique** : l'archi hexagonale (ADR-004) rend l'agrégation de sources externes naturelle. Chaque flux est un adapter implémentant un port `IExternalContentSource`.
- **Modèle économique propre** : les features qui coûtent de l'argent par utilisateur (IA = coût LLM) sont monétisées ; le reste est libre.

### Conséquences

- **Positives** : différenciation forte, valeur ajoutée immédiate, hook produit, modèle éco viable.
- **Négatives** : surface fonctionnelle élargie en MVP 2 (~10 features supplémentaires, cf. doc 02). Volume technique à gérer (parser RSS, dédup, scaling).
- **À prévoir** :
  - Isolation des modules premium dès l'architecture (projet `<Projet>.Application.Premium` séparé)
  - Définition du port `IExternalContentSource` côté domaine
  - Catalogue initial de templates de veille par métier (à constituer en partie avec la doc 07)
  - UI accessible de la timeline veille (cf. doc 06)
  - Mécanisme de modération des sources user-defined
- **Référentiel détaillé** : voir documents `02-roadmap-features.md` (cluster F-041 à F-050), `03-catalogue-apis-publiques.md`, `07-flux-rss-veille.md`

---

## ADR-010 — Authentification utilisateur hexagonale custom

**Statut** : ✅ Accepté
**Date** : 27 mai 2026

### Contexte

Le modèle d'authentification des **utilisateurs du SaaS** (à ne pas confondre avec l'authentification *INPI* multi-tenant tranchée par ADR-003) était listé dans les « décisions à prendre ultérieurement ». Trois options étaient sur la table (cf. F-001) :

- **ASP.NET Core Identity** : framework intégré (UserStore EF, confirmation d'email, lockout, 2FA prêts à l'emploi).
- **IdP externe** (Auth0, Keycloak self-host) : délégation complète de l'identité.
- **Auth hexagonale custom** : implémentation propre, alignée sur l'architecture du projet, via des ports.

Les contraintes de sécurité sont déjà fermes (cf. `docs/04-securite-rgpd.md` §5.4) : **Argon2id** imposé (pas de PBKDF2/bcrypt), **JWT courts** signés en asymétrique (RS256/EdDSA), **refresh tokens rotatifs** révocables côté serveur, **TOTP** obligatoire dès qu'un compte connecte des credentials INPI.

### Décision

**Authentification hexagonale custom**. L'identité, les comptes et les sessions sont modélisés dans `Atlas.Domain` ; les mécanismes techniques sont des **adapters** derrière des ports secondaires :

- `IPasswordHasher` → adapter Argon2id (Konscious) — paramètres 64 Mo / 3 itérations / parallélisme 4.
- `IJwtIssuer` + `ISigningKeyProvider` → émission JWT RS256 (clé RSA, PEM configurable / KMS en production).
- `ITokenGenerator` → jetons opaques (vérification d'email, refresh) ; seul le **hash** est persisté.
- `IUserRepository`, `IAccountRepository`, `IRefreshTokenRepository`, `IUnitOfWork` → persistance EF Core/PostgreSQL.
- `IEmailSender` → email transactionnel (adapter Brevo en production ; adapter de log en DEV).

Les use cases (`RegisterUser`, `VerifyEmail`, `Login`, `RefreshToken`, `Logout`) sont des handlers MediatR retournant `Result`/`Result<T>`. Le refresh token est déposé dans un cookie **httpOnly / Secure / SameSite=Strict**.

### Rationale

- **Cohérence architecturale** : ASP.NET Core Identity couple l'identité à EF et impose son propre hasher (PBKDF2) et son modèle ; l'intégrer aurait signifié le désactiver en grande partie pour satisfaire Argon2id + JWT custom + le pattern `Result<T>`. Friction nette avec l'hexagonal (ADR-004).
- **Souveraineté / RGPD** : un IdP externe (Auth0) ré-introduit une dépendance SaaS tierce, à rebours de la posture OSS/souveraine du projet (ADR-005/006). Keycloak self-host ajoute une lourde opération d'exploitation pour un MVP.
- **Maîtrise sécurité** : le projet stocke des credentials INPI ; il doit garder un contrôle total et auditable sur le hachage, la signature des jetons et la révocation.
- **Coût maîtrisé** : le périmètre F-001 (register/verify/login/refresh/logout) est borné et entièrement testable avec des ports mockés.

### Conséquences

- **Positives** : zéro friction architecturale, testabilité maximale (ports), aucune dépendance d'identité tierce, conformité directe aux exigences de `docs/04`.
- **Négatives** : davantage de code à écrire et à maintenir (flux email de vérification, rotation des refresh tokens, lockout). Responsabilité accrue : toute faille d'implémentation est nôtre.
- **À prévoir** :
  - **F-002 (TOTP)** : ajouter `ITotpProvider` (Otp.NET) et les codes de secours, en réutilisant le même socle.
  - Adapter **Brevo** pour `IEmailSender` (remplacer l'adapter de log DEV) avant production.
  - **Blacklist Redis** des access tokens révoqués (révocation immédiate) — prévue, non implémentée en F-001.
  - Clé RSA de signature gérée par **KMS** en production (PEM éphémère en DEV uniquement).

---

## Décisions à prendre ultérieurement

Les sujets suivants sont **identifiés** mais **non encore tranchés**. Ils seront documentés dans des ADR ultérieurs.

| Sujet | Pourquoi attendre |
|---|---|
| Nom du projet | À figer avant la création du repo |
| Choix précis du framework web côté front (Blazor Server vs WASM vs autre) | Décision en phase MVP 1 |
| Choix de l'hébergeur (OVHcloud, Scaleway, Clever Cloud, Hetzner...) | Décision en phase déploiement |
| Outil de paiement (Stripe, Lemon Squeezy, Paddle) | Décision phase 3 (mois 12–24) |
| Stratégie de cache (Redis ? In-memory ?) | Décision en phase architecture détaillée |
| Gestion de queue pour les tâches asynchrones (Hangfire, MassTransit + RabbitMQ) | Décision en phase architecture détaillée |
| Segment client primaire (cabinets compta, avocats, KYC, etc.) | À laisser émerger des early adopters |

---

## Roadmap macro

**Phase 0 — Cadrage (en cours, mai 2026)**
- ✅ Décisions stratégiques
- ✅ Documentation des APIs INPI
- ⏳ Architecture détaillée (hexagone, layout solution .NET)
- ⏳ Vocabulaire ubiquitaire
- ⏳ Choix du repo

**Phase 1 — MVP 1 (mois 1–4)**
- Backend ASP.NET Core fonctionnel
- Auth utilisateurs + connexion compte INPI
- Recherche entreprise par SIREN (RNE)
- Recherche marque par nom (INPI PI)
- Client MAUI mobile + desktop avec ces 2 fonctions
- Repo public, premiers articles techniques

**Phase 2 — MVP 2 (mois 4–8)**
- Téléchargement bilans / actes RNE
- Détail marque + image
- Recherche brevet
- Persistance des recherches / favoris
- **Veille intégrée (feature majeure différenciante)** :
  - Lecteur RSS / Atom natif
  - Templates de veille pré-curés par métier
  - Ajout libre de sources par l'utilisateur
  - Intégration BODACC pour les annonces légales
  - Déduplication et filtres
  - Alertes email et push sur évolutions des favoris
  - Timeline unifiée combinant favoris + sources veille

**Phase 3 — Croissance (mois 8–18)**
- Intégration BODACC (annonces légales)
- Intégration Sirene (compléments INSEE)
- Premiers utilisateurs réels, écoute du feedback
- Lancement de la version SaaS hébergée payante

**Phase 4 — Maturité (mois 18+)**
- Selon adoption : EUIPO/OMPI international, Judilibre, DECP
- Premium features éventuelles
- Premiers clients entreprise

---

## Glossaire

| Terme | Définition |
|---|---|
| **RNE** | Registre National des Entreprises (depuis 2023, remplace RNCS) |
| **PI** | Propriété Industrielle (brevets, marques, dessins & modèles) |
| **SIREN** | Identifiant unique d'une entreprise française (9 chiffres) |
| **SIRET** | Identifiant unique d'un établissement (SIREN + 5 chiffres) |
| **ADR** | Architecture Decision Record |
| **OSS** | Open Source Software |
| **AGPL** | Affero General Public License |
| **CLA** | Contributor License Agreement |
| **Hexagonal / Ports & Adapters** | Pattern d'architecture isolant le domaine métier des détails d'infrastructure |
| **Multi-tenant** | Système où chaque client/user a son propre espace isolé |
| **Use case** | Cas d'utilisation métier (ex. "rechercher une entreprise") |
| **Adapter sortant** | Code qui traduit une demande du domaine vers un système externe (API, BDD) |
| **Adapter entrant** | Code qui traduit une demande externe (HTTP, UI) vers une demande du domaine |
| **Port** | Interface définie par le domaine, implémentée par un adapter |

---

*Document évolutif. Toute modification doit être tracée avec date et auteur en haut de l'ADR concerné.*
