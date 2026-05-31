# ADR-024 — Graphe d'écosystème : généralisation multi-arêtes de F-034 (descriptif, borné, entité-niveau)

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

**F-034 a posé le moteur de graphe** : nœuds `Personne`/`Entreprise` + arêtes `Mandat` (PostgreSQL), traversée bornée `WITH RECURSIVE` (1-2 sauts, **pas de Neo4j**), résolution d'identité conservatrice, **couche légale lourde** (DPIA/LIA/opposition/transparence pour les données de personnes) et **accessibilité** (équivalent tabulaire obligatoire, ADR-008). F-034 a explicitement renvoyé toute *« version massive »* à un **nouvel ADR** — nous y sommes.

D'autres liens entre entités sont **dérivables de sources officielles** : **acheteur ↔ titulaire** (DECP, F-032, *déjà ingéré*, propre par SIREN), **co-dépôts PI** (différenciant — personne ne le calcule sur la PME française — mais déposant **nominatif**), **adresse partagée** (bruyant). Reste à décider **comment généraliser F-034 en graphe multi-arêtes sans rouvrir le risque légal ni noyer le signal**.

## Décision

Six principes.

**1. Multi-arêtes typées sur le substrat F-034.** On **ajoute des types d'arêtes** au graphe F-034 (mêmes nœuds, même traversée bornée) — pas de nouveau moteur, pas de Neo4j. Chaque arête est **typée et qualifiée descriptivement** (« co-déposants d'un brevet 2023 », « titulaire d'un marché de X 2024 »), **jamais** une qualification (« partenaires », « groupe », « dépendance »).

**2. On-demand borné, focus-centré — pas de graphe persistant massif.** On **conserve le modèle F-034** : construction à la demande autour d'une entité focus, 1-2 sauts, cache. F-034 **interdit** l'agrégation massive (DPIA bien plus lourde) ; un graphe géant **rouvrirait** cette question fermée.

**3. Séparation du poids légal : entité-niveau vs personne-niveau.** Les arêtes **entité ↔ entité** (DECP société↔acheteur, co-dépôts société↔société) sont de l'**open data sur des personnes morales → légalement légères**, **hors** du gating DPIA de F-034. Les arêtes **impliquant une personne** (co-mandats, et un éventuel déposant PI **individuel**) restent **sous le cadre F-034**. **v1 PI = co-déposants personnes morales uniquement** ; déposants individuels reportés sous le régime F-034.

**4. Résolution conservatrice, y compris déposant PI → SIREN.** Les arêtes **propres par SIREN** (DECP, via le SIRET→SIREN déjà fait en F-032) **ne demandent aucun matching flou**. Le **déposant PI est nominatif** (F-016) → **résolution nom→SIREN conservatrice** (ADR-014) : confiance élevée requise, **dans le doute pas d'arête**. *Pas de lien vaut mieux qu'un lien erroné* (F-034).

**5. Traversée consciente des hubs (anti-noyade).** Filtrer/flaguer les **nœuds à très haut degré** : adresse de domiciliation (des milliers de sociétés), acheteur public majeur (des milliers de marchés), déposant institutionnel. On montre l'**arête directe** (société ↔ acheteur X) mais on **n'infère pas** un lien société↔société **transitif via un hub**. Le bruit se maîtrise **à la traversée**, jamais en masquant la donnée.

**6. Liens, jamais conclusions.** Le graphe **montre des faits de liens**, l'utilisateur conclut (ADR-012). Une grappe **n'est pas** un verdict — F-034 a tué le cadrage « société écran » pour exactement ça. **Pas de bénéficiaires effectifs** (CJUE Sovim). Accessibilité : équivalent **tabulaire** obligatoire (ADR-008).

### Croquis (illustratif)

```csharp
// Extension du graphe F-034 : l'arête gagne un TYPE et un libellé descriptif.
public enum EdgeKind { Mandate, PublicContract, CoFiling, SharedAddress }

public sealed record GraphEdge(
    NodeRef From, NodeRef To,
    EdgeKind Kind,
    string DescriptiveLabel,   // « co-déposants d'un brevet (2023) » — jamais « partenaires »
    string Provenance,         // source + référence (n° de dépôt, ID marché…) + date
    int? AsOfYear);

// Traversée consciente des hubs : un nœud au-dessus du seuil ne propage pas de lien transitif.
public sealed record TraversalPolicy(int MaxHops, int HubDegreeThreshold);
```

## Rationale

- **F-034 est le substrat** : généraliser = ajouter des types d'arêtes, pas un moteur — réutilisation maximale.
- **La séparation entité/personne** concentre le poids légal là où il est (personnes) et **libère les arêtes différenciantes** (co-dépôts entre sociétés, DECP) du gating lourd.
- **On-demand borné** respecte la doctrine légale de F-034 (pas de masse).
- **Hub-awareness** = la réponse *structurelle* au faux-lien — le « se noyer » appliqué au graphe.
- **Descriptif** = cohérent avec tout le produit ; F-034 a déjà tranché contre l'inférence.

## Conséquences

- **Positives** : angle différenciant (co-dépôts PI) débloqué **légèrement** ; arête DECP **quasi gratuite** ; signal honnête ; réutilise le moteur F-034 ; risque légal contenu.
- **Négatives** : résolution déposant→SIREN = **travail réel** (conservateur → couverture **partielle** assumée) ; hub-filtering = seuil à **calibrer** ; chaque arête typée = un peu de mapping.
- **À prévoir** :
  - **F-071** (arête DECP) et **F-072** (arête co-dépôts PI) implémentent.
  - **doc 08** : nommer `EdgeKind`, `GraphEdge`, `TraversalPolicy` (seuil de hub).
  - **Déposants PI individuels** = report **sous le régime F-034** (DPIA) — décision ultérieure.
  - **Adresse partagée** = arête future, special-cased (hub).
  - **Références croisées** : **F-071 / F-072** ; **F-034** (substrat + cadre légal personnes), **F-032** (DECP, SIRET→SIREN), **F-016 / F-006** (PI, déposant nominatif), **F-056** (dossier « Structure »), **F-055** (risque) ; **ADR-012 / 014 / 008** ; **doc 08**.
