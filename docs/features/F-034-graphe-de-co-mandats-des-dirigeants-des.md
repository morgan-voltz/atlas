# F-034 — Graphe de co-mandats des dirigeants (descriptif)

> **Reframé & figé le 29 mai 2026** — remplace le stub V3+ initial (« détection de sociétés écrans / conflits d'intérêt »). Ce cadrage est **abandonné** : inférence accusatoire → profilage + diffamation (cf. ADR-012). On ne garde que le **graphe de co-mandats descriptif**, **conditionnel à la DPIA**.

**Description** : autour d'une entreprise que l'utilisateur **consulte ou suit**, Atlas affiche un **graphe de co-mandats** — les dirigeants de l'entreprise, leurs autres mandats, et les entreprises qui **partagent un dirigeant** — sur une profondeur limitée (1 à 2 sauts). Strictement **descriptif** : Atlas montre des **faits du registre**, jamais une qualification (« société écran », « conflit d'intérêt ») et **n'attribue aucun score de risque** à une personne. L'utilisateur explore et tire ses propres conclusions.

**Valeur user** : pour la due diligence et la compréhension des liens entre entreprises suivies (personas **Investisseur / M&A**, **veille concurrentielle**) — voir d'un coup d'œil « qui est aussi aux commandes d'où ». Outil d'**exploration de faits publics**, pas de jugement.

**Pourquoi V3+ conditionnel** : **mise en service bloquée tant que la DPIA n'est pas réalisée** (ADR-012, gating dur). Le poids n'est pas la traversée du graphe (triviale en SQL borné) mais la couche légale (DPIA + LIA + opposition + transparence), la **résolution d'identité conservatrice** (homonymes), et l'**accessibilité** d'un graphe (ADR-008, critère bloquant).

**Complexité** : ★★★★

**APIs externes** : **INPI RNE** (dirigeants — déjà récupérés ; F-019 n'en stocke qu'un hash, cette feature nécessite de stocker les **identités** dans le voisinage borné → **nouveau traitement**, gaté par ADR-012). Éventuellement le jeu open data « dirigeants » pour constituer le voisinage.

**Dépendances** : **ADR-012** (cadre RGPD — prérequis impératif), F-004, F-019, F-017 / F-053 (favoris & watchlists bornent le périmètre), ADR-004, ADR-008.

**Hors périmètre (explicite — voir ADR-012)** :
- **Aucune qualification ni inférence** (pas de « société écran », pas de « conflit d'intérêt », pas de score de risque sur une personne).
- **Aucune agrégation massive** exposée publiquement (pas de base graphe de tous les dirigeants de France). Une version « massive » future exigerait **un nouvel ADR** et une DPIA bien plus lourde.
- **Aucun bénéficiaire effectif** (régime restreint, CJUE Sovim).

**Détails techniques** :
- **Modèle (PostgreSQL, pas Neo4j)** : deux tables **nœuds** (`Personne`, `Entreprise`) et **arêtes** (`Mandat` : qui dirige quoi, à quel titre, depuis quand). Traversée bornée par **`WITH RECURSIVE`** (1–2 sauts), performant à cette échelle, **déjà dans la stack** — aucune nouvelle infrastructure.
- **Construction à la demande** autour de l'entreprise focus (éphémère ou cache — décision ouverte).
- **Résolution d'identité conservatrice** (ADR-012 §5) : ne relier qu'avec confiance élevée (nom + **date de naissance** du RNE), afficher l'incertitude sinon. *Pas de lien vaut mieux qu'un lien erroné.*
- **Architecture (ADR-004)** : réutilise `ICompanyDataProvider` (RNE) ; service de domaine de construction du voisinage borné + service de résolution d'identité ; persistance EF Core / PostgreSQL existante.

**Cadre légal & positionnement** : entièrement régi par **ADR-012**. Descriptif uniquement ; base légale **intérêt légitime** (réutilisation open data, cadre CNIL) avec LIA ; **DPIA prérequis de mise en service** ; **droit d'opposition / effacement** + note de transparence (art. 14) ; résolution d'identité conservatrice.

**Accessibilité (ADR-008, bloquant)** : un graphe visuel est hostile au lecteur d'écran et au daltonisme. **Obligatoire** : équivalent **tabulaire/textuel** complet (« X détient des mandats dans A, B, C ») et aucune information transmise par la couleur ou la position seules. À concevoir dès le départ.

**Modèle économique** : construction déterministe, aucun coût d'inférence par utilisateur → **cœur open source** par défaut (ADR-006). Limite de profondeur/quota possible en hébergé.

**Découpage / jalons** :
1. **Couche légale (prérequis, ADR-012)** : DPIA + LIA + mécanisme d'opposition/effacement + note de transparence. **Bloquant — rien ne se livre avant.**
2. **Modèle & résolution** : tables nœuds/arêtes, résolution d'identité conservatrice (nom + date de naissance, seuil de confiance).
3. **Traversée bornée & API** : construction à la demande (1–2 sauts), endpoint dédié.
4. **Visualisation + équivalent accessible** : rendu graphe **et** vue tabulaire/textuelle.

**Décisions ouvertes** :
- **Profondeur** : 1 ou 2 sauts ?
- **Persistance** : graphe éphémère (reconstruit à chaque consultation) ou mis en cache ?
- **Source du voisinage** : RNE live vs jeu open data « dirigeants ».
- **Cœur vs premium** : déterministe donc cœur par défaut, mais feature lourde — à confirmer.
