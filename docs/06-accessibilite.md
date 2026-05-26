# Accessibilité & Inclusivité

> Document de référence sur les engagements d'accessibilité du projet et leur mise en œuvre technique.
> L'accessibilité n'est **pas une option** ni une feature ajoutée tardivement : c'est un **critère de Definition of Done** sur chaque feature livrée.

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026

---

## Sommaire

- [1. Engagement du projet](#1-engagement-du-projet)
- [2. Cadre légal et normatif](#2-cadre-légal-et-normatif)
- [3. Les 4 axes du handicap](#3-les-4-axes-du-handicap)
- [4. Implémentation technique — MAUI](#4-implémentation-technique--maui)
- [5. Implémentation technique — Web (Blazor futur)](#5-implémentation-technique--web-blazor-futur)
- [6. Implémentation côté backend / API](#6-implémentation-côté-backend--api)
- [7. Accessibilité visuelle](#7-accessibilité-visuelle)
- [8. Accessibilité auditive](#8-accessibilité-auditive)
- [9. Accessibilité motrice](#9-accessibilité-motrice)
- [10. Accessibilité cognitive](#10-accessibilité-cognitive)
- [11. Tests d'accessibilité](#11-tests-daccessibilité)
- [12. Definition of Done — accessibilité](#12-definition-of-done--accessibilité)
- [13. Déclaration d'accessibilité](#13-déclaration-daccessibilité)
- [14. Roadmap conformité](#14-roadmap-conformité)
- [15. Ressources](#15-ressources)

---

## 1. Engagement du projet

### 1.1 Principes fondateurs

Le projet prend l'engagement suivant, signé en phase de cadrage :

1. **Accessibilité dès la conception** : aucune feature n'est livrée sans avoir été conçue pour être accessible. Pas de "on rendra ça accessible plus tard".
2. **Definition of Done bloquante** : chaque feature livrée doit passer une checklist accessibilité (cf. §12) avant d'être mergée. Ce n'est pas négociable.
3. **Niveau cible** : **WCAG 2.2 niveau AA + RGAA 4.1.2** (avec préparation à RGAA 5 attendu fin 2026).
4. **Couverture des 4 axes du handicap dès MVP1** : visuel, auditif, moteur, cognitif.
5. **Tests réels** : pas seulement des audits automatiques. Tests manuels avec lecteurs d'écran, navigation clavier, simulation daltonisme.

### 1.2 Pourquoi cet engagement

- **Éthique** : 17% de la population française vit avec au moins une forme de handicap (INSEE). Construire un outil sans penser à eux, c'est les exclure activement.
- **Légal** : depuis le 28 juin 2025, l'European Accessibility Act rend l'accessibilité **obligatoire** pour tous les services numériques B2C en UE (sauf TPE < 10 salariés ET < 2M€ CA). Notre projet finira par y être soumis. Sanctions : jusqu'à 50 000€ par manquement en France.
- **Stratégique** : intégrer l'accessibilité dès le MVP1 coûte ~15-25% de temps en plus. La rajouter après coup coûte 5 à 10 fois plus cher, et ne sera jamais aussi bonne que si elle est native.
- **Différenciant** : la majorité des SaaS français en 2026 ne sont **pas** accessibles. Notre projet sera un cas à part — argument commercial vis-à-vis de clients institutionnels (cabinets publics, associations, ESN attentives).
- **Qualité globale** : un design accessible est presque toujours un meilleur design tout court (clarté, structure, simplicité).

### 1.3 Impact sur le delivery

Le respect de cet engagement implique :
- ~15-25% de temps additionnel par feature
- Outillage spécifique en CI (axe-core, accessibility scanner)
- Tests manuels périodiques avec lecteurs d'écran
- Revue accessibilité dans les Pull Requests
- Formation continue (lectures, vidéos)

**Ce surcoût est accepté et intégré au planning.** Il n'est pas une excuse pour faire du "presque accessible" — soit on respecte les règles, soit on ne livre pas.

---

## 2. Cadre légal et normatif

### 2.1 WCAG 2.2 — Web Content Accessibility Guidelines

- **Publié par** : W3C (World Wide Web Consortium)
- **Version actuelle** : 2.2 (depuis octobre 2023)
- **Niveaux** :
  - **A** : minimum strict (insuffisant pour un produit pro)
  - **AA** : cible standard (notre niveau cible)
  - **AAA** : excellence (rarement applicable à tout)
- **Structure** : 4 principes (POUR) — Perceivable, Operable, Understandable, Robust. 13 lignes directrices. 86 critères de succès en 2.2.

### 2.2 RGAA 4.1.2 — Référentiel Général d'Amélioration de l'Accessibilité

- **Publié par** : DINUM (Direction Interministérielle du Numérique)
- **Version actuelle** : 4.1.2 (mise à jour 2023, basée sur WCAG 2.1)
- **Structure** : 106 critères techniques répartis en 13 thématiques (images, couleurs, formulaires, navigation, etc.)
- **Version 5** : prévue fin 2026, intégrera WCAG 2.2

**À retenir** : un site conforme RGAA 4.1.2 niveau AA est conforme aux exigences légales françaises.

### 2.3 EAA — European Accessibility Act

- **Directive UE 2019/882**, transposée en droit français
- **En vigueur depuis** : 28 juin 2025
- **Concerne** : tous les produits et services numériques mis sur le marché en UE
- **Exemption** : micro-entreprises < 10 salariés ET < 2M€ CA annuel
- **Sanctions** : jusqu'à 50 000€ par manquement pour le secteur privé en France, variable jusqu'à 75 000€ / 4% du CA dans d'autres pays UE

**Statut du projet** : initialement exempté (TPE en démarrage), soumis dès le dépassement des seuils.

### 2.4 EN 301 549

Norme européenne harmonisée pour l'accessibilité ICT (Information & Communication Technologies). C'est la **référence technique de l'EAA**. Couvre web, mobile, logiciel, documents, matériel. Intègre WCAG 2.1 niveau AA.

### 2.5 Hiérarchie pratique

```
EAA (loi UE)
   ↓ s'appuie sur
EN 301 549 (norme technique européenne)
   ↓ intègre
WCAG 2.1 / 2.2 (référentiel international W3C)
   ↓ adapté en France par
RGAA 4.1.2 (référentiel français, 106 critères)
```

Conséquence : **respecter le RGAA 4.1.2 niveau AA = respecter la loi française**.

---

## 3. Les 4 axes du handicap

### 3.1 Visuel

**Population concernée** : aveugles, malvoyants, daltoniens. Environ **1,7 million** de personnes en France.

**Sous-catégories** :
- Cécité totale (~ 65 000 personnes en France)
- Malvoyance (~ 1,2 million)
- Daltonisme (~ 4% des hommes, 0,5% des femmes — ~ 1,5 million)

**Outils utilisés par les personnes concernées** :
- Lecteurs d'écran : VoiceOver (iOS/macOS), TalkBack (Android), Narrator (Windows), NVDA (Windows gratuit), JAWS (Windows payant)
- Loupes d'écran, zooms système
- Modes contraste élevé
- Filtres daltonisme

### 3.2 Auditif

**Population concernée** : sourds, malentendants. Environ **5,2 millions** de personnes en France (la plus grande catégorie).

**Sous-catégories** :
- Surdité totale (~ 500 000 personnes)
- Malentendance (~ 4,7 millions, dont une majorité de seniors)

**Outils utilisés** :
- Sous-titres
- Transcriptions textuelles
- Notifications visuelles (badges, vibrations)
- Mode "vibrations seulement" sur mobile

### 3.3 Moteur

**Population concernée** : difficultés de manipulation, tremblements, paralysies, prothèses. Environ **2,3 millions** de personnes en France.

**Sous-catégories** :
- Paralysies (paraplégie, tétraplégie)
- Tremblements (Parkinson, sclérose)
- Amputations / prothèses
- Arthrites, troubles musculo-squelettiques

**Outils utilisés** :
- Navigation au clavier (Tab, Enter, Espace, flèches)
- Commandes vocales
- Switch interfaces (interrupteurs adaptés)
- Pointeurs alternatifs (eye-tracking, joystick adaptés)

### 3.4 Cognitif

**Population concernée** : dyslexie, autisme, TDA/H, déficience intellectuelle, troubles d'apprentissage. Environ **6 à 10 millions** de personnes en France selon la définition retenue (souvent sous-estimé).

**Sous-catégories** :
- Dyslexie (~ 5-10% de la population)
- Autisme et troubles du spectre (~ 1%)
- TDA/H (~ 5% des adultes)
- Déficience intellectuelle légère à modérée

**Adaptations clés** :
- Langage clair, phrases courtes
- Pas de surcharge visuelle / cognitive
- Instructions explicites étape par étape
- Pas d'information temporelle stricte (compte à rebours)
- Possibilité d'annuler / revenir en arrière

---

## 4. Implémentation technique — MAUI

### 4.1 SemanticProperties — La fondation

MAUI fournit des propriétés sémantiques pour décrire les éléments aux technologies d'assistance.

```xml
<!-- Description : ce que l'élément est -->
<Button Text="🔍"
        SemanticProperties.Description="Rechercher une entreprise par SIREN" />

<!-- Hint : ce qui se passe quand on l'active -->
<Button Text="Supprimer"
        SemanticProperties.Description="Supprimer le favori"
        SemanticProperties.Hint="Cette action est irréversible" />

<!-- HeadingLevel : structurer la page comme un document -->
<Label Text="Recherche entreprise"
       SemanticProperties.HeadingLevel="Level1" />

<Label Text="Résultats"
       SemanticProperties.HeadingLevel="Level2" />
```

**Règle** : tout élément interactif (Button, Entry, Picker, etc.) DOIT avoir une `SemanticProperties.Description` non vide (ou un `Text` parlant qui en tient lieu).

### 4.2 Annonces dynamiques

Pour informer un user de lecteur d'écran qu'un état a changé :

```csharp
// Après une recherche réussie
SemanticScreenReader.Default.Announce("3 entreprises trouvées");

// Après une erreur
SemanticScreenReader.Default.Announce("Erreur : SIREN invalide. Vérifiez le format.");
```

### 4.3 Images : décoratives vs informatives

```xml
<!-- Image décorative (background, ornement) : marquée comme telle -->
<Image Source="logo_decoratif.png"
       SemanticProperties.Description="" />

<!-- Image informative : description précise -->
<Image Source="graphique_evolution.png"
       SemanticProperties.Description="Graphique d'évolution du CA de 2020 à 2024, en hausse de 25%" />

<!-- Image fonctionnelle (utilisée comme bouton) : description = action -->
<ImageButton Source="trash.png"
             SemanticProperties.Description="Supprimer ce favori" />
```

### 4.4 Polices et tailles dynamiques

Respecter les réglages OS de taille de texte (Dynamic Type iOS, Font Scale Android).

```xml
<!-- Bon : la taille respecte les réglages OS -->
<Label Text="Bonjour"
       FontSize="Medium"
       FontAutoScalingEnabled="True" />

<!-- Mauvais : taille fixe en pixels qui ignore les réglages user -->
<Label Text="Bonjour"
       FontSize="14"
       FontAutoScalingEnabled="False" />
```

**Test obligatoire** : configurer le device avec la **taille de texte maximale** (Réglages → Accessibilité). L'app doit rester utilisable.

### 4.5 Contrastes et thèmes

Utiliser des ressources dynamiques pour les couleurs, jamais en dur dans les éléments :

```xml
<!-- App.xaml : palette accessible -->
<Color x:Key="PrimaryText">#1A1A1A</Color>      <!-- Sur fond clair -->
<Color x:Key="PrimaryTextDark">#F5F5F5</Color>  <!-- Sur fond sombre -->
<Color x:Key="ErrorRed">#D32F2F</Color>          <!-- Contraste 5.5:1 OK -->
<Color x:Key="ErrorRedDark">#FF6B6B</Color>      <!-- Adapté au dark mode -->

<!-- Dans les pages -->
<Label Text="Erreur"
       TextColor="{AppThemeBinding Light={StaticResource ErrorRed},
                                   Dark={StaticResource ErrorRedDark}}" />
```

**Outils de validation des contrastes** :
- Stark (Figma, navigateur)
- WCAG Contrast Checker
- Adobe Color (gratuit en ligne)

**Ratios minimaux** :
- Texte normal : 4.5:1
- Grand texte (≥18pt ou ≥14pt bold) : 3:1
- Éléments UI non textuels (icônes, bordures) : 3:1

### 4.6 Navigation clavier complète

Sur desktop MAUI (Windows/macOS) et MAUI sur device avec clavier externe, la navigation clavier doit être 100% fonctionnelle.

```xml
<!-- TabIndex pour contrôler l'ordre de tab -->
<Entry Placeholder="SIREN"
       TabIndex="1" />

<Button Text="Rechercher"
       TabIndex="2" />
```

```csharp
// Raccourcis clavier (KeyboardAccelerator)
var searchAccelerator = new KeyboardAccelerator
{
    Modifiers = KeyboardAcceleratorModifiers.Ctrl,
    Key = "F"
};
searchButton.KeyboardAccelerators.Add(searchAccelerator);
```

**Règle** : tout ce qui est cliquable doit être atteignable au Tab, et activable avec Enter ou Espace.

### 4.7 Indicateurs de focus visibles

Le focus visuel doit être **clairement visible** (pas un simple changement de couleur subtil).

```xml
<!-- VisualStateManager pour personnaliser l'état Focused -->
<Button Text="Rechercher">
    <VisualStateManager.VisualStateGroups>
        <VisualStateGroup x:Name="CommonStates">
            <VisualState x:Name="Normal" />
            <VisualState x:Name="Focused">
                <VisualState.Setters>
                    <Setter Property="BorderColor" Value="#0078D4" />
                    <Setter Property="BorderWidth" Value="3" />
                </VisualState.Setters>
            </VisualState>
        </VisualStateGroup>
    </VisualStateManager.VisualStateGroups>
</Button>
```

### 4.8 Tailles de zones tactiles

**Règle WCAG 2.5.5 (AAA mais à viser)** : zones cibles d'au moins **44x44 pixels** (Apple) / **48x48 dp** (Material Design).

```xml
<!-- Bon : zone de tap large -->
<ImageButton Source="trash.png"
             HeightRequest="48"
             WidthRequest="48"
             Padding="12" />

<!-- Mauvais : trop petit -->
<ImageButton Source="trash.png"
             HeightRequest="24"
             WidthRequest="24" />
```

### 4.9 Tests sur device réel — obligatoire

Aucun audit automatisé ne remplace les tests réels :

| Plateforme | Lecteur d'écran | Comment activer |
|---|---|---|
| iOS | VoiceOver | Réglages → Accessibilité → VoiceOver |
| Android | TalkBack | Paramètres → Accessibilité → TalkBack |
| Windows | Narrator | Win + Ctrl + Entrée |
| macOS | VoiceOver | Cmd + F5 |

**Process de test pour chaque feature MAUI** :
1. Activer le lecteur d'écran
2. Couper l'écran (ou couvrir avec la main)
3. Tenter de réaliser la feature **sans regarder**
4. Si impossible : la feature n'est **pas accessible**, retour à la conception

---

## 5. Implémentation technique — Web (Blazor futur)

À traiter en détail quand on lancera la version web. Principes généraux :

### 5.1 HTML sémantique

Utiliser les balises HTML5 sémantiques : `<header>`, `<nav>`, `<main>`, `<article>`, `<section>`, `<footer>`, `<aside>`. Jamais `<div>` pour des structures sémantiques.

### 5.2 ARIA (Accessible Rich Internet Applications)

Compléter le HTML sémantique avec des attributs ARIA quand nécessaire :

```html
<button aria-label="Rechercher une entreprise"
        aria-describedby="search-hint">
  🔍
</button>
<span id="search-hint" class="visually-hidden">
  Entrez un SIREN ou un nom d'entreprise
</span>
```

**Règle ARIA #1** : utiliser ARIA seulement quand le HTML natif ne suffit pas. Un `<button>` natif est meilleur qu'un `<div role="button">`.

### 5.3 Skip links

Lien "Aller au contenu principal" en début de chaque page, pour les users clavier.

```html
<a href="#main-content" class="skip-link">Aller au contenu principal</a>
...
<main id="main-content">...</main>
```

### 5.4 Focus management

Gérer le focus lors des changements de page (Blazor) : quand on navigue, le focus doit se replacer sur le titre H1 ou un container approprié.

### 5.5 Outils Blazor

- **Blazor.Accessibility** (lib communauté)
- Composants shadcn/MudBlazor configurés avec attention à l'accessibilité

---

## 6. Implémentation côté backend / API

L'accessibilité n'est pas que côté UI. Le backend doit aussi suivre certaines règles.

### 6.1 Messages d'erreur clairs

```json
// MAUVAIS : code abscons sans contexte
{ "error": "ERR_VALIDATION_001" }

// BON : message exploitable par l'UI pour l'utilisateur
{
  "error": {
    "code": "INVALID_SIREN",
    "message": "Le SIREN fourni n'est pas valide.",
    "details": "Un SIREN doit comporter exactement 9 chiffres.",
    "field": "siren"
  }
}
```

### 6.2 Codes HTTP appropriés

Respecter les codes HTTP standard pour que les apps puissent les annoncer correctement aux users :
- `400` : erreur client (input invalide)
- `401` : non authentifié
- `403` : interdit
- `404` : non trouvé
- `429` : trop de requêtes
- `500` : erreur serveur

### 6.3 Endpoints d'accessibilité

Prévoir des endpoints qui aident l'app à s'adapter :

```
GET /api/user/preferences/accessibility
→ {
  "highContrast": false,
  "reduceMotion": true,
  "fontSize": "large",
  "screenReaderActive": true  // si détectable
}

POST /api/user/preferences/accessibility
```

Permet à l'utilisateur de configurer ses préférences une fois et de les retrouver sur tous ses devices.

---

## 7. Accessibilité visuelle

### 7.1 Critères principaux à respecter

| Critère WCAG | Description | Implémentation |
|---|---|---|
| 1.1.1 | Contenu non-textuel | Alt text sur toutes les images |
| 1.3.1 | Info et relations | Structure sémantique (headings) |
| 1.4.3 | Contraste | Ratio 4.5:1 minimum |
| 1.4.4 | Redimensionnement texte | Zoom 200% sans casse |
| 1.4.10 | Reflow | Adapté petits écrans |
| 1.4.11 | Contraste non-textuel | Icônes / bordures 3:1 |

### 7.2 Daltonisme

**Règle absolue** : aucune information transmise UNIQUEMENT par la couleur.

```
MAUVAIS : "Les lignes en rouge sont les erreurs"
BON     : "Les lignes avec l'icône ⚠️ (en rouge) sont les erreurs"
```

**Test obligatoire** : simuler avec Color Oracle ou Sim Daltonism les 3 formes principales :
- Protanopie (rouge invisible) — ~1%
- Deutéranopie (vert invisible) — ~6%
- Tritanopie (bleu invisible) — rare

---

## 8. Accessibilité auditive

### 8.1 Périmètre dans le projet

L'app aura **peu de contenu audio en MVP1** (essentiellement textuel). Mais les règles s'appliquent dès qu'il y a :
- Notifications sonores
- Vidéos de tutoriel
- Alertes audio (futur)

### 8.2 Règles à respecter dès MVP1

1. **Toute notification sonore doit être doublée** d'une notification visuelle (badge, snackbar, vibration).
2. **Vidéos de tutoriel** : sous-titres synchronisés + transcription textuelle sous la vidéo.
3. **Pas d'audio auto-play** sans contrôle utilisateur.
4. **Volume contrôlable** indépendamment du système si l'app produit du son.

### 8.3 Cas concrets dans le projet

- Alerte de fin de téléchargement masse → toast visuel + email (pas de "ding" seul)
- Notification push d'évolution favori → notification système avec icône claire
- Tutoriels MAUI (futurs MVP 5/6) → sous-titres VTT intégrés

---

## 9. Accessibilité motrice

### 9.1 Règles principales

1. **Navigation clavier complète** (cf. §4.6)
2. **Pas de gestes complexes** obligatoires (pinch, double-tap répété, swipe précis)
3. **Zones de clic suffisantes** (44x44 minimum)
4. **Temps suffisant** : pas de timeouts agressifs sur les formulaires. Si timeout nécessaire (ex. session), avertir et permettre prolongation.
5. **Annulation possible** : tout geste destructif doit être annulable ou confirmé.

### 9.2 Cas concrets

- Pull-to-refresh : OK mais doit AUSSI exister comme bouton "Actualiser"
- Swipe pour supprimer : OK mais doit AUSSI exister comme bouton "Supprimer" dans un menu contextuel
- Double tap pour zoomer : OK mais doit AUSSI avoir un bouton zoom+/-

**Règle générale** : tout geste avancé doit avoir une **alternative simple au tap unique**.

---

## 10. Accessibilité cognitive

C'est l'axe le plus souvent oublié. Pourtant il bénéficie aussi à **toutes les autres personnes** (fatigue, distraction, langue non maternelle).

### 10.1 Règles principales

1. **Langage clair** :
   - Phrases courtes (< 20 mots idéalement)
   - Mots du quotidien (éviter le jargon juridique INPI quand on peut)
   - Glossaire pour les termes techniques inévitables (SIREN, NAF, etc.)
2. **Pas de surcharge visuelle** :
   - Une action principale par écran
   - Hiérarchie visuelle claire
   - Espace blanc respiré
3. **Instructions explicites** :
   - Étape par étape pour les processus complexes
   - Indicateur de progression (4 étapes, vous êtes à 2/4)
4. **Cohérence** :
   - Mêmes mots pour les mêmes concepts partout
   - Mêmes patterns UI pour les mêmes actions
5. **Pas d'information temporelle stricte** :
   - Pas de "vous avez 30 secondes pour cliquer"
   - Si timer nécessaire, possibilité de le pauser/étendre
6. **Annulation et retour en arrière** :
   - Confirmation avant toute action destructive
   - Bouton "annuler" présent

### 10.2 Test : la grand-mère

Le test informel : "**est-ce que ma grand-mère qui ne fait pas d'informatique pourrait utiliser cette feature ?**" — si non, simplifier.

### 10.3 Polices facilitantes

Polices recommandées pour la dyslexie :
- **OpenDyslexic** (gratuite, optionnelle à proposer)
- **Atkinson Hyperlegible** (Braille Institute, gratuite)
- Polices sans empattement de manière générale (Roboto, Inter, etc.)

**Proposer dans les paramètres** : option "Police adaptée à la dyslexie" qui bascule en OpenDyslexic.

---

## 11. Tests d'accessibilité

### 11.1 Tests automatisés

À intégrer dans la CI dès le premier commit MAUI :

| Outil | Plateforme | Couverture |
|---|---|---|
| **Accessibility Insights for Windows** | MAUI Windows | Audit complet |
| **Accessibility Scanner (Google)** | MAUI Android | Audit Android |
| **Accessibility Inspector (Apple)** | MAUI iOS / Mac | Audit Apple |
| **axe-core** | Web Blazor (futur) | Audit web |
| **Lighthouse** | Web Blazor (futur) | Audit Chrome |

**Important** : les outils automatisés détectent **30 à 40%** des problèmes d'accessibilité. Le reste se trouve manuellement.

### 11.2 Tests manuels

Pour chaque feature :

**Test au clavier** (5 min) :
1. Débrancher la souris / désactiver le trackpad
2. Tenter d'utiliser la feature uniquement au clavier
3. Le focus est-il toujours visible ?
4. L'ordre de tabulation est-il logique ?
5. Tous les éléments interactifs sont-ils atteignables ?

**Test au lecteur d'écran** (15 min) :
1. Activer VoiceOver / TalkBack / NVDA
2. Fermer les yeux ou couvrir l'écran
3. Tenter de réaliser la feature à l'aveugle
4. Les informations sont-elles annoncées correctement ?
5. Les actions sont-elles claires ?

**Test au zoom** (5 min) :
1. Régler l'OS sur la taille de texte maximale
2. Naviguer dans toutes les pages
3. Rien n'est coupé, tronqué, ou superposé ?

**Test daltonisme** (5 min) :
1. Activer Color Oracle ou Sim Daltonism
2. Simuler les 3 types
3. Toutes les informations sont-elles encore distinguables ?

### 11.3 Tests utilisateurs

Idéal à partir du MVP 2 :
- Recruter 1-2 testeurs avec un handicap réel via des associations (Valentin Haüy, Surdi13, APF France handicap)
- Sessions de 30-45 min avec questions ouvertes
- Compenser financièrement (50-100€ / session)

C'est l'investissement le plus rentable de toute la démarche : un test utilisateur réel révèle ce qu'aucun audit ne trouvera.

---

## 11.5 Accessibilité spécifique à la feature Veille (MVP 2)

La timeline veille (cf. cluster F-041 à F-050 dans la doc 02) présente des défis d'accessibilité **particuliers** qui méritent une attention spéciale. Un flux continu d'items qui s'incrémente est l'un des cas les plus piégeurs pour les lecteurs d'écran.

### 11.5.1 Défis spécifiques de la timeline

- **Volume** : 50 à 200 items potentiels par session — la navigation doit être fluide
- **Mise à jour temps réel** : les nouveaux items qui apparaissent en haut de la timeline doivent être annoncés sans submerger le user
- **Hiérarchie d'information** : chaque item contient titre, source, date, extrait — l'ordre de lecture doit être logique
- **Actions par item** : marquer lu/non lu, favori, archiver — tous ces gestes doivent avoir une alternative clavier
- **Filtres dynamiques** : quand on applique un filtre, l'utilisateur doit savoir que la liste change

### 11.5.2 Règles spécifiques à respecter

1. **Structure ARIA / Semantic** :
   - La timeline est un `<feed>` (HTML) ou un `<CollectionView>` (MAUI) avec semantic role approprié
   - Chaque item est un `<article>` indépendant
   - Headings clairs pour le titre de chaque item (`HeadingLevel="Level3"`)

2. **Annonces des nouveaux items** :
   - Mise à jour silencieuse par défaut (pas d'annonce automatique qui interrompt)
   - Indicateur visible "X nouveaux items" avec activation manuelle par le user
   - `aria-live="polite"` (web) ou équivalent MAUI pour les changements d'état

3. **Navigation clavier optimisée** :
   - Flèches haut/bas pour passer d'item en item
   - Espace pour marquer lu/non lu
   - F pour favori, A pour archiver (raccourcis annoncés en aide)
   - Tab uniquement pour entrer dans le détail d'un item

4. **Filtres accessibles** :
   - Les filtres actifs sont visibles (badges, état du champ de recherche)
   - Annonce après application : "Filtre appliqué : 23 résultats sur 247"
   - Possibilité de "tout effacer" facilement atteignable

5. **Notifications respectueuses** :
   - Push mobile : titre clair, pas de jargon ("Nouveau dépôt INPI : MARQUE X" ✓ vs "FR-2025-12345" ✗)
   - Possibilité de désactiver par catégorie
   - Respect du mode "Ne pas déranger" du système

6. **Mode "lecture seule" / digest** :
   - Option utilisateur : remplacer la timeline interactive par un email/résumé périodique
   - Particulièrement utile pour les users cognitifs ou en limitant la fatigue

### 11.5.3 Test obligatoire avant livraison du cluster veille

- [ ] Navigation complète clavier de la timeline (sans souris)
- [ ] Annonce correcte au lecteur d'écran de chaque item
- [ ] Annonces des actions (lu/favori/archivé)
- [ ] Annonces des changements de filtre
- [ ] Test au lecteur d'écran sur une timeline de 100+ items (performance perçue)
- [ ] Vérification des couleurs en mode daltonisme (badges de source colorés ne doivent pas être le seul indicateur)
- [ ] Mode digest fonctionnel et préféré par défaut pour les nouveaux comptes ayant déclaré une préférence d'accessibilité

---

## 12. Definition of Done — accessibilité

**Aucune feature n'est mergée en `main` sans valider cette checklist** :

### 12.1 Checklist universelle

- [ ] Tous les éléments interactifs ont un `SemanticProperties.Description` (MAUI) ou `aria-label` (Web)
- [ ] La structure de la page utilise des `HeadingLevel` cohérents
- [ ] Les images informatives ont un alt text descriptif ; les décoratives sont marquées comme telles
- [ ] La navigation au clavier est fonctionnelle (Tab, Enter, Espace, flèches)
- [ ] Le focus est visible visuellement
- [ ] Les contrastes respectent 4.5:1 (texte) et 3:1 (UI / grand texte)
- [ ] Aucune information n'est transmise uniquement par la couleur
- [ ] Les zones de tap sont ≥ 44x44 pixels
- [ ] La page fonctionne à 200% de zoom sans casse
- [ ] Les textes utilisent `FontAutoScalingEnabled="True"`
- [ ] Aucun geste complexe n'est obligatoire (toujours une alternative au tap)
- [ ] Les messages d'erreur sont clairs et explicites
- [ ] La feature a été testée au lecteur d'écran (au moins une plateforme)
- [ ] La feature a été testée en navigation clavier
- [ ] La feature a été simulée en daltonisme

### 12.2 Process de PR

Dans le template de Pull Request, une section dédiée :

```markdown
## Accessibilité

- [ ] Checklist accessibilité passée
- [ ] Lecteur d'écran testé (préciser lequel) : 
- [ ] Navigation clavier testée
- [ ] Daltonisme simulé
- [ ] Captures d'écran avant/après avec focus visible : 

Si une case n'est pas cochée, expliquer pourquoi :
```

Une PR avec accessibilité non-conforme est **bloquée**, comme une PR avec tests en échec.

---

## 13. Déclaration d'accessibilité

### 13.1 Obligation légale

Quand le projet dépassera les seuils d'exemption TPE, il devra publier une **déclaration d'accessibilité** publique.

### 13.2 Contenu obligatoire

Document accessible depuis chaque page du site et des apps. Doit contenir :

1. **Engagement** de l'éditeur à rendre le service accessible
2. **Niveau de conformité** atteint : totalement / partiellement / non conforme
3. **Résultats des tests** : audit RGAA ou tierce partie
4. **Liste des contenus non accessibles**, avec dérogations motivées
5. **Date d'établissement** de la déclaration
6. **Technologies utilisées** pour construire le service
7. **Outils de test** utilisés
8. **Coordonnées de contact** pour signaler un problème (mail dédié `accessibilite@<domaine>`)
9. **Voies de recours** : possibilité de saisir le Défenseur des droits

### 13.3 Modèle de page

À créer dans `/accessibilite` sur le site, et accessible depuis le pied de page de chaque écran de l'app.

---

## 14. Roadmap conformité

### 14.1 Avant MVP 1 (mois 0-4)

- [ ] Document d'engagement accessibilité figé (ce fichier)
- [ ] Mise en place des outils de test (Accessibility Insights, Accessibility Scanner, Color Oracle)
- [ ] Formation initiale (lectures, vidéos) du porteur de projet
- [ ] Définition de la palette de couleurs accessible (validée 4.5:1)
- [ ] Création du template de PR avec section accessibilité
- [ ] Configuration de la CI avec scans automatiques

### 14.2 Pendant MVP 1 (mois 1-4)

- [ ] Toutes les features livrées avec checklist DoD passée
- [ ] Tests réguliers au lecteur d'écran (au moins 1× par semaine)
- [ ] Première version de la page "Accessibilité" du site

### 14.3 Avant ouverture publique (entre MVP 1 et MVP 2)

- [ ] Audit RGAA complet (auto-audit ou freelance qualifié)
- [ ] Tests utilisateurs avec 2-3 personnes en situation de handicap
- [ ] Publication de la déclaration d'accessibilité
- [ ] Correction des écarts identifiés

### 14.4 Phase de croissance (mois 6+)

- [ ] Audit RGAA annuel
- [ ] Tests utilisateurs trimestriels
- [ ] Mise à jour vers RGAA 5 quand publié (fin 2026)
- [ ] Veille accessibilité (newsletters spécialisées, blogs)
- [ ] Formation continue (au moins 1 conférence ou MOOC par an)

---

## 15. Ressources

### 15.1 Documents officiels

- **WCAG 2.2** : https://www.w3.org/TR/WCAG22/
- **RGAA 4.1.2** : https://accessibilite.numerique.gouv.fr/
- **EN 301 549** : https://www.etsi.org/standards#page=1&search=301549
- **EAA (Directive)** : https://eur-lex.europa.eu/eli/dir/2019/882/oj

### 15.2 Guides et tutoriels

- **CNIL — Guide RGPD pour les développeurs** : aborde aussi l'accessibilité
- **DINUM — RGAA notes techniques** : https://accessibilite.numerique.gouv.fr/methode/
- **Microsoft — Accessibility in .NET MAUI** : https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility
- **Apple Human Interface Guidelines — Accessibility** : https://developer.apple.com/design/human-interface-guidelines/accessibility
- **Material Design — Accessibility** : https://m3.material.io/foundations/accessible-design

### 15.3 Outils

| Outil | Usage | Lien |
|---|---|---|
| Accessibility Insights | Audit Windows | https://accessibilityinsights.io/ |
| Accessibility Scanner | Audit Android | Play Store |
| Accessibility Inspector | Audit Apple | Inclus dans Xcode |
| axe DevTools | Audit web | https://www.deque.com/axe/devtools/ |
| Lighthouse | Audit Chrome | Chrome DevTools |
| Stark | Plugin Figma | https://www.getstark.co/ |
| Color Oracle | Simul daltonisme desktop | https://colororacle.org/ |
| Sim Daltonism | Simul daltonisme macOS | https://michelf.ca/projects/sim-daltonism/ |
| WAVE | Audit web en ligne | https://wave.webaim.org/ |
| NVDA | Lecteur d'écran Windows | https://www.nvaccess.org/ |

### 15.4 Communautés et associations

- **Association Valentin Haüy** (handicap visuel) : https://www.avh.asso.fr/
- **Fédération Nationale des Sourds de France** : https://www.fnsf.org/
- **APF France handicap** : https://www.apf-francehandicap.org/
- **AccessibleEU** (centre européen) : https://accessible-eu-centre.ec.europa.eu/
- **#a11y community** sur Twitter/Mastodon

### 15.5 Lectures recommandées (priorité haute)

- "**Accessibility for Everyone**" de Laura Kalbag (A Book Apart, court et clair)
- "**Inclusive Design Patterns**" de Heydon Pickering
- "**A11y Project Checklist**" (gratuit en ligne)
- Newsletter "Web Accessibility Weekly" (Aaron Cannon)

---

## Note finale

Ce document n'est pas un manifeste théorique. **C'est un contrat moral signé par le porteur du projet en phase de cadrage.** Il s'engage à :

1. Respecter la DoD accessibilité sur chaque feature.
2. Ne pas reporter "à plus tard" les efforts d'accessibilité.
3. Tester réellement avec des outils et, à terme, avec des utilisateurs concernés.
4. Faire de l'accessibilité un argument différenciant du projet, pas une case à cocher.

Si un jour la tentation est forte de "faire l'impasse" sur une feature pressée, **relire ce document**. C'est exactement à ce moment-là qu'il est utile.

---

*Document évolutif. Toute modification doit être tracée. La DoD accessibilité ne peut être assouplie qu'avec une justification écrite documentée dans le repo.*
