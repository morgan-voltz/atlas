# F-076 — Télémétrie produit (opt-in, anonyme, auto-hébergée)

> **Statut** : 🟡 À spécifier. Cible : **V1–V2**.
> **Date** : 30 mai 2026.
> **Catégorie MoSCoW** : Should have (améliore le produit ; non bloquant pour les fonctions cœur).
> **Principe non négociable** : **opt-in, anonyme/agrégé, auto-hébergé.** La télémétrie d'Atlas doit être *exemplaire*, pas « comme tout le monde » — c'est une question d'ADN (produit **souverain**, personas sensibles à la vie privée), pas seulement de conformité.
> **Dépendances** : F-001 (compte — pour le **stockage du consentement**, pas pour lier les métriques), `docs/12-modele-ux-client-maui.md` §9 (UX dans « Données & confidentialité »), `docs/04-securite-rgpd.md` (**avenant requis**, voir plus bas), ADR-001 (SaaS), ADR-005 (licence/souveraineté).
> **Documents liés** : `02-roadmap-features.md`, `04-securite-rgpd.md`.

---

## Description

Collecter des **métriques anonymes et agrégées** pour améliorer Atlas, selon deux flux distincts :
1. **Rapports de stabilité** — plantages et erreurs techniques (écran, version OS/app, trace anonymisée).
2. **Statistiques d'usage** — événements d'interface agrégés (écran ouvert, fonction utilisée, parcours), pour repérer ce qui sert et ce qui ne sert pas.

Les deux sont **désactivés par défaut** (opt-in), **non liés à l'identité**, et la collecte est **auto-hébergée** (rien ne part chez un tiers). Le contrôle vit dans **Profil → Données & confidentialité** (doc 12 §9), comme une question de vie privée — pas un réglage de confort.

## Valeur

**Pour le produit** : savoir où les utilisateurs bloquent, quels écrans sont délaissés (la Veille est-elle utilisée ?), quels plantages corriger en priorité. Indispensable pour itérer sans deviner.
**Pour l'utilisateur / la marque** : une télémétrie *exemplaire* (opt-in, anonyme, souveraine) **renforce** le positionnement au lieu de le contredire. C'est un **argument commercial** auprès des avocats/compliance, pas une concession.

## Le piège évité (pourquoi ces choix)

- **Anonyme/agrégé, pas lié au compte** : on veut des *tendances* (« la Veille est peu utilisée »), pas le comportement d'un individu (« Maître Dupont n'utilise pas la Veille »). Lier au compte entraînerait le RGPD lourd (profilage) pour un bénéfice nul.
- **Opt-in, pas opt-out** : un produit souverain qui collecterait par défaut trahirait sa promesse auprès de personas qui sont les plus allergiques au pistage. L'opt-in *prouve* la souveraineté ; c'est « l'app ne fait rien dans le dos de l'utilisateur » appliqué aux métriques.
- **Auto-hébergé, pas de tiers** : envoyer la télémétrie chez un analytics tiers (a fortiori extra-UE) serait un trou dans la coque souveraine. La collecte vit sur l'infra d'Atlas.

## Périmètre

**Dans le périmètre :**
- Deux flux **séparément activables** (stabilité / usage), OFF par défaut.
- Collecte **anonyme** : pas d'identifiant de compte, pas d'identifiant stable réidentifiant (cf. RGPD ci-dessous).
- **Auto-hébergement** de la collecte et de l'agrégation.
- Affichage honnête de **ce qui est mesuré et de ce qui ne l'est jamais**.

**Hors périmètre (explicite) :**
- **Tout lien avec l'identité** de l'utilisateur.
- **Le contenu** des recherches, fiches, watchlists, articles lus — **jamais** collecté.
- Métriques liées au compte / profilage individuel / A-B testing nominatif.
- Cookies de traçage publicitaire (sans objet — pas de pub, cohérent ADR-006).

## Complexité : ★★★

Modérée : SDK/handler de crash (par plateforme MAUI + web), émission d'événements d'usage, **backend de collecte auto-hébergé** (Matomo self-hosted ou collecte maison), agrégation, et surtout le **respect strict du toggle** (rien n'est émis tant que l'opt-in n'est pas donné — y compris au premier lancement). Le soin réel est l'**anonymisation effective**, pas la tuyauterie.

## Détails techniques

- **Consentement** : un `UserConsent` (ou clé de préférence, cf. F-062) stocke l'état des deux toggles. **Tant que OFF → aucune émission** (pas de « buffer en attente » qui partirait après-coup).
- **Anonymisation** : pas d'`UserId` dans les événements ; pas d'identifiant d'appareil persistant réidentifiant ; troncature/suppression d'IP côté collecte ; pas de fingerprinting. L'anonymisation doit être **réelle** (irréversible), pas un pseudonymat.
- **Stabilité** : capture d'exception → message + écran + version, trace **scrubbée** de toute donnée potentiellement personnelle (pas de contenu de requête dans les logs — cohérent avec la règle « jamais de secret/donnée sensible dans un message d'erreur », doc 12 §12).
- **Usage** : événements typés (`screen_view`, `feature_used`…) **sans payload de contenu** (on enregistre « recherche lancée », jamais *quoi* a été recherché).
- **Auto-hébergement** : collecteur sur l'infra Atlas (ex. Matomo self-hosted, ou endpoint maison `/telemetry` agrégeant côté serveur). Aucun appel sortant vers un tiers.
- **Clients (ADR-002)** : MAUI et web n'émettent que si opt-in ; la doctrine « client pur de l'API » est respectée (la télémétrie passe par l'API Atlas, pas par un SDK tiers branché en direct).

## Cadre RGPD — **avenant à `docs/04-securite-rgpd.md`** (à rédiger)

La télémétrie n'est probablement pas couverte par la doc RGPD actuelle. À ajouter :
- **Base légale** : **consentement** (art. 6-1-a) — d'où l'opt-in. Le consentement est *libre* (l'app fonctionne identiquement sans), *spécifique* (deux flux distincts), *éclairé* (texte honnête), *révocable* (toggle à tout moment).
- **Registre des traitements** : ajouter le traitement « amélioration produit » avec finalité, durée de conservation (agrégats anonymes → pas de limite ; données brutes éventuelles → courte rétention puis agrégation/suppression).
- **Anonymisation vs pseudonymisation** : viser l'**anonymisation réelle** (hors champ RGPD une fois anonymisé) ; si une étape pseudonyme est techniquement nécessaire, elle reste soumise au RGPD et doit être minimale et brève.
- **Transparence** : la politique de confidentialité décrit ce qui est mesuré et ce qui ne l'est jamais ; l'écran (doc 12 §9) le résume.
- **Export/suppression** : l'état du **consentement** est une donnée de compte (export art. 20, suppression art. 17) ; les métriques anonymes, par définition, ne sont pas réidentifiables donc pas « personnelles » au sens RGPD.

## Accessibilité

Les toggles portent un libellé clair + état annoncé (jamais distingués par la seule couleur) ; le texte explicatif est lisible par lecteur d'écran ; cibles ≥ 44 px (doc 12 / doc 06).

## Modèle économique (ADR-006)

Coût d'infra de la collecte auto-hébergée (faible). Aucune monétisation des données (interdit par l'ADN). La télémétrie sert **uniquement** l'amélioration produit — jamais revendue, jamais publicitaire.

## Découpage / jalons

1. **Consentement + UI** (doc 12 §9) : les deux toggles, OFF par défaut, texte honnête, stockage du consentement — **sans collecte encore active**. *(Livrable autonome : on peut poser le contrôle avant la tuyauterie.)*
2. **Stabilité** : crash reporting anonyme auto-hébergé, respect du toggle.
3. **Usage** : événements d'interface agrégés, respect du toggle.
4. **Avenant RGPD** (`docs/04-securite-rgpd.md`) + mise à jour de la politique de confidentialité — **prérequis légal avant d'activer la collecte en production**.

## Décisions ouvertes

- **Outil** : Matomo self-hosted vs collecte maison (`/telemetry`) — arbitrage effort/contrôle.
- **Granularité des événements d'usage** : jusqu'où descendre sans risquer la réidentification.
- **Rétention** des données brutes éventuelles avant agrégation.
- **Premier lancement** : comment présenter l'opt-in sans dark pattern (proposer une fois, sobrement, sans culpabiliser le refus).

## À faire à l'intégration

- Ajouter **F-076** au catalogue `02-roadmap-features.md` (Should have).
- **Rédiger l'avenant** `docs/04-securite-rgpd.md` (base légale consentement, registre, anonymisation) — **bloquant avant prod**.
- Mettre à jour la **politique de confidentialité** (ce qui est mesuré / jamais mesuré).
- Cohérence doc 12 §9 (déjà figée) : la zone « Aider à améliorer Atlas » référence cette fiche.

---

*Fiche figée le 30 mai 2026. Télémétrie opt-in, anonyme/agrégée, auto-hébergée — exemplaire par cohérence d'ADN (souveraineté). Deux flux : stabilité + usage, OFF par défaut. Le contenu des recherches/fiches n'est jamais collecté ; rien n'est lié à l'identité ; rien ne part chez un tiers. Avenant RGPD requis avant activation en production.*
