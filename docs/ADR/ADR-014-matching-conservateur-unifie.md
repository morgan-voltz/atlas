# ADR-014 — Matching conservateur unifié (la doctrine ADR-012 incarnée dans le type)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026

## Contexte

Quatre features ont besoin de **rapprocher** une entité connue avec un référentiel : **F-047** (mentions d'une entreprise dans la presse), **F-055** (rapprochement avec des listes de sanctions), **F-031** (jurisprudence où une entité apparaît), **F-026** (antériorité — similarité de marques).

Algorithmiquement, ce ne sont **pas** la même chose. Elles se rangent en **trois familles distinctes** :
- **Nom-dans-texte** (F-047, F-031) : chercher un nom *connu* dans un texte *libre*. Risque : homonymes, variantes de raison sociale.
- **Nom-contre-liste** (F-055) : rapprocher un nom d'une *liste structurée* (alias, translittérations) — du *record linkage*. Risque : faux positif diffamatoire.
- **Similarité de marques** (F-026) : ressemblance phonétique / visuelle / conceptuelle + recoupement des classes de Nice. Mécanique entièrement à part.

Vouloir un « matcher universel » referait l'erreur de la sur-abstraction (F-026 et F-047 ne partagent quasiment aucune mécanique). Mais ne rien partager laisserait **se réimplémenter quatre fois ce qui, lui, est réellement commun** : non pas l'algorithme, mais la **doctrine ADR-012** — produire un rapprochement avec un **niveau de confiance**, une **base explicable** (auditabilité), et **toujours** la formulation « **à vérifier** », **jamais** une affirmation ni un verdict. Réimplémentée quatre fois, cette doctrine dériverait en quatre niveaux de prudence et quatre présentations.

## Décision

Factoriser **ce qui est commun (la doctrine)** et isoler **ce qui varie (l'algorithme)**, en **trois couches** :

1. **Le contrat de doctrine** (`MatchCandidate`, dans `Atlas.Domain`) — type partagé par **les quatre** features, portant un **niveau de confiance**, une **base explicable**, et une disposition qui **ne peut être que « candidat / à vérifier »**. La doctrine ADR-012 est **encodée dans le type** : il n'existe **aucune** disposition « confirmé / avéré / verdict ». Émettre un verdict devient **structurellement impossible** — l'état illégal est *irreprésentable*.

2. **Le noyau de normalisation des noms** (`ICompanyNameNormalizer`, Domain) — partagé par les **trois matchers à base de noms** (F-047, F-055, F-031) : suffixes (SA/SAS/SARL), accents, casse, variantes de raison sociale. **Pas** F-026.

3. **Les matchers par famille** — `INameInTextMatcher`, `INameAgainstListMatcher`, `ITrademarkSimilarityMatcher` — chacun avec sa mécanique propre.

**Cas F-026** : *conforme à la posture, moteur distinct*. Il **respecte le contrat de doctrine** (couche 1 — il propose des candidats à vérifier, jamais un verdict de disponibilité), mais son **moteur reste totalement séparé** (et en partie premium). On ne le force pas dans le noyau des noms.

**Placement hexagonal** : contrat de doctrine + noyau de normalisation + ports des matchers → `Atlas.Domain` ; matchers déterministes → Domain, matchers s'appuyant sur une source externe (p.ex. similarité EUIPO) → `Atlas.Infrastructure.*` ; les briques **IA** (sémantique de F-026, résumés) → `Atlas.Application.Premium` (cohérent ADR-006).

**Timing (règle de trois)** : le **contrat de doctrine** se définit **maintenant** — ce n'est pas une extraction prématurée mais un *contrat*, avec quatre clients déjà identifiés. Le **noyau de normalisation** s'extrait à l'arrivée du **troisième** matcher à base de noms (F-047 existe ; F-055 et F-031 viendront), à partir de code éprouvé.

**Garde-fous** : ne pas construire de « matcher universel » ; ne pas forcer F-026 dans le noyau des noms ; **ne jamais ajouter** de disposition « confirmé » au contrat (son absence *est* la décision).

### Croquis du contrat (illustratif)

```csharp
// Domain — le contrat de doctrine, partagé par les 4 features.
// Encode l'ADR-012 DANS le type : un rapprochement ne peut être qu'un CANDIDAT à vérifier.
public sealed record MatchCandidate(
    EntityRef   Subject,      // l'entité suivie (ce qu'on cherchait)
    MatchTarget Target,       // ce qui a été trouvé (item presse, entrée de liste, décision, marque)
    MatchConfidence Confidence,
    MatchBasis  Basis);       // POURQUOI ça a matché — pour la piste d'audit
// Volontairement : aucun « IsConfirmed », aucune disposition « Verdict ».
// La seule sortie possible d'un matcher est un candidat à vérifier.

public enum MatchConfidence { Low, Medium, High }   // jamais « Certain »

public sealed record MatchBasis(
    string Method,    // "dénomination exacte" | "alias de liste" | "phonétique" | …
    string Evidence,  // l'élément concret trouvé
    string Source);   // source + date

// Domain — noyau partagé par les matchers à base de noms (F-047, F-055, F-031). Pas F-026.
public interface ICompanyNameNormalizer
{
    NormalizedName Normalize(string rawDenomination);
}

// Ports par famille (mécaniques distinctes, même posture)
public interface INameInTextMatcher        { /* F-047, F-031 */ }
public interface INameAgainstListMatcher   { /* F-055 */ }
public interface ITrademarkSimilarityMatcher { /* F-026 — moteur à part, même posture */ }
```

## Rationale

- **Le commun, c'est la doctrine, pas l'algorithme.** On factorise la doctrine (le type), on isole les algorithmes (les matchers) — exactement la distinction reine de l'ADR-013 : *partager ce qui est commun, isoler ce qui varie*.
- **Encoder la doctrine dans le type** rend l'état illégal irreprésentable : le compilateur fait respecter l'ADR-012, ce qui est infiniment plus robuste qu'un rappel en revue de code.
- **Évite les deux échecs** : pas de matcher universel (sur-abstraction), pas de quatre réimplémentations divergentes (sous-abstraction + dérive de prudence).
- **Auditabilité native** : `MatchBasis` est toujours présent → chaque rapprochement dit *pourquoi*, partout pareil.

## Conséquences

- **Positives** : ADR-012 garantie **structurellement** et **uniformément** ; auditabilité homogène ; ajout d'un matcher à base de noms bon marché (réutilise contrat + normalisation) ; F-026 bénéficie de la posture sans tordre le contrat.
- **Négatives** : discipline pour **ne jamais** ajouter une disposition « confirmé » (son absence est le cœur de la décision) ; trois familles à garder distinctes.
- **À prévoir** :
  - Nommer `MatchCandidate`, `MatchConfidence`, `MatchBasis`, `ICompanyNameNormalizer` dans le **doc 08**.
  - Extraire le noyau de normalisation au **3ᵉ** matcher à base de noms.
  - Garder les briques IA (sémantique F-026, résumés) dans `Application.Premium`.
  - Définir un **rendu UI homogène** du « à vérifier » (jamais présenté comme un fait).
  - Cet ADR est l'**incarnation technique de la doctrine de rapprochement d'ADR-012** : à référencer croisé.
