# ADR-012 — Réutilisation des données de dirigeants & cadre du graphe relationnel

**Statut** : ✅ Accepté — **mise en œuvre conditionnée à la réalisation préalable d'une DPIA** (gating, voir Décision §3).
**Date** : 29 mai 2026
**Lié à** : F-034 (graphe de co-mandats), ADR-004 (architecture hexagonale), `04-securite-rgpd.md` (DPIA, réutilisation open data), F-019 (dirigeants déjà récupérés, aujourd'hui stockés en hash).

## Contexte

F-034 veut visualiser les liens entre entreprises via leurs dirigeants. Trois faits cadrent la décision :

1. **Les données de dirigeants sont des données personnelles** (nom, fonction, date de naissance…), même dans un contexte professionnel. Un graphe de dirigeants est donc un **traitement RGPD** à part entière.
2. Le **doc 04 cite explicitement** « le graphe relationnel des dirigeants » comme déclencheur d'obligation de **DPIA** (art. 35). Les sanctions CNIL 2026 frappent les « analyses d'impact absentes ».
3. La CNIL encadre la **réutilisation de l'open data** (et cite le **RNE de l'INPI** en exemple) : réutilisation possible **sans consentement ni anonymisation**, sur la base de l'**intérêt légitime**, avec information par **note publique** — **mais** dans le respect du **droit d'opposition**, qui peut être exercé dès que la diffusion **excède le cadre imposé par la loi**.

Le stub initial de F-034 prévoyait la « **détection de sociétés écrans et de conflits d'intérêt** ». C'est le point de rupture : qualifier une structure d'« écran » ou un dirigeant de « en conflit », ce n'est plus décrire un fait public, c'est émettre une **inférence accusatoire** (profilage + risque diffamatoire + dépassement manifeste du cadre légal, art. 82).

Deux problèmes liés : l'**« agrégation massive »** (tous les dirigeants de France pré-agrégés) maximise *à la fois* le risque juridique et le coût technique (base graphe dédiée type Neo4j + exploitation) ; et la **résolution d'identité** (homonymes) crée un risque d'**exactitude** (RGPD art. 5.1.d) et de diffamation si on relie par erreur deux personnes distinctes.

Options pesées :
- **(A) Produit de détection** (sociétés écrans / conflits / score de risque sur personnes) → **écarté** : profilage, diffamation, dépassement du cadre légal.
- **(B) Graphe de co-mandats descriptif et borné** → **retenu**.
- **(C) Ne rien faire** (statu quo V3+) → toujours possible, mais on perd une vraie valeur défendable.

## Décision

Atlas adopte une doctrine **descriptive, bornée et conditionnée à une DPIA** pour toute réutilisation des données de dirigeants et pour le graphe relationnel.

1. **Reframe descriptif.** Atlas expose des **faits du registre** : « cette personne détient des mandats dans A, B, C » ; « ces entreprises partagent un dirigeant ». Atlas **ne qualifie jamais** (« société écran », « conflit d'intérêt ») et **n'attribue aucun score de risque à une personne**. L'utilisateur tire ses propres conclusions.
2. **Base légale.** Intérêt légitime (réutilisation d'open data, cadre CNIL), documenté par un **test de mise en balance (LIA)**. Respect de la licence Etalab du RNE, de l'`autorisation d'utilisation commerciale = false` et de la `diffusionINSEE = N` (non rediffusion). **Jamais** de bénéficiaires effectifs (régime restreint, CJUE Sovim).
3. **DPIA = prérequis de mise en service.** La feature **ne peut pas être greenlightée** tant que la DPIA n'est pas réalisée et ses conclusions implémentées. Gating dur.
4. **Droits des personnes.** Mécanisme de **droit d'opposition / d'effacement** : une personne physique peut demander à être retirée du graphe ; information par **note publique de transparence** (art. 14).
5. **Exactitude (homonymes).** Résolution d'identité **conservatrice** : on ne relie une personne entre deux entreprises qu'avec une **confiance élevée** (ex. nom + date de naissance). En cas de doute, on **n'affiche pas le lien** plutôt que d'en afficher un faux. *Pas de lien vaut mieux qu'un lien erroné.*
6. **Périmètre borné.** Graphe construit **à la demande**, autour d'une entreprise que l'utilisateur consulte ou suit, sur une **profondeur limitée** (1 à 2 sauts). **Pas** de pré-agrégation massive exposée publiquement.
7. **Conséquence technique.** Le périmètre borné rend une base graphe dédiée **inutile** : **PostgreSQL** suffit (tables nœuds + arêtes, requêtes `WITH RECURSIVE`). Pas de Neo4j, pas de nouvelle infrastructure. Une éventuelle version « massive » future exigerait **un nouvel ADR** et une DPIA bien plus lourde.

## Rationale

- **Le bon positionnement.** Descriptif = factuel = défendable, dans la lignée de tout le produit (suivi ≠ conseil, ratios ≠ notation, liste ≠ verdict). La détection accusatoire est précisément ce qui expose juridiquement.
- **Droit et technique alignés.** Le périmètre borné minimise *à la fois* le risque juridique (finalité limitée, pas de surveillance de masse) et le coût technique (PostgreSQL, déjà dans la stack). Le chemin sûr est aussi le chemin léger.
- **La DPIA est due.** Ce n'est pas une précaution discrétionnaire : le doc 04 et la pratique CNIL l'imposent. La gater évite une mise en service non conforme.
- **L'exactitude protège deux fois.** Une résolution conservatrice protège la personne (pas de faux lien) et Atlas (pas de diffamation, conformité art. 5.1.d).

## Conséquences

- **Positives** : une feature relationnelle puissante **et** juridiquement défendable ; aucune nouvelle base / infrastructure à exploiter (PostgreSQL) ; réutilise les données RNE déjà récupérées ; doctrine cohérente, réutilisable si d'autres features touchent aux personnes physiques.
- **Négatives** : la **DPIA est un vrai chantier** et **bloque** la mise en service (pas de livraison rapide) ; Atlas renonce volontairement au « détecteur de sociétés écrans » que des concurrents pourraient afficher (choix assumé) ; la résolution conservatrice **n'affichera pas certains liens réels** (faux négatifs, compromis accepté au profit de l'exactitude) ; l'accessibilité d'un graphe visuel est exigeante (ADR-008, critère bloquant).
- **À prévoir** :
  - Réaliser la **DPIA** et le **LIA** ; les conserver au registre des traitements (doc 04 §6.1).
  - Implémenter le **mécanisme d'opposition / effacement** + la **note publique de transparence** (art. 14).
  - Concevoir la **résolution d'identité conservatrice** (nom + date de naissance, seuil de confiance, affichage de l'incertitude).
  - Fixer la **profondeur** (1 ou 2 sauts) et le caractère **éphémère ou persistant** du graphe.
  - Prévoir l'**équivalent tabulaire/textuel** accessible (ADR-008).
  - Garder **F-034 en V3+ conditionnel** jusqu'à DPIA réalisée.
