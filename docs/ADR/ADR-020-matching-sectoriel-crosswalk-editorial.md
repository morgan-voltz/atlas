# ADR-020 — Matching sectoriel de la veille par crosswalk éditorial (donnée de référence)

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

La veille va ingérer des **sources réglementaires** — EUR-Lex en premier (**F-063**), plus tard JORF/Légifrance. Ces textes portent une classification de **sujet** (URIs EuroVoc, *directory code* du Répertoire de la législation UE) mais **jamais de code NAF**. Pour faire émerger « ce texte concerne le secteur d'une entreprise que je suis », il faut un pont **sujet → NAF**.

Trois faits cadrent la décision :

1. **Il n'existe pas de table officielle EuroVoc → NACE/NAF.** NACE n'est cross-walké que vers d'autres classifications d'*activité ou de produit* (ISIC, CPA, HS, CN) ; EuroVoc est un thésaurus de *sujets* — une autre ontologie. Le pont est **irréductiblement éditorial** : on le construit, on ne le télécharge pas.
2. **Le matching est flou par construction** (ontologies hétérogènes), exactement comme le matching nom-dans-texte de **F-047** est faux-positif-prone. La sortie doit donc être un **candidat à vérifier**, jamais un fait (ADR-012/014).
3. **Tout n'est pas sectoriel.** RGPD, droit du travail, fiscalité touchent *tout le monde* : matchés « par secteur », ils matchent tous les secteurs → noyade. La ligne de partage utile n'est pas FR/UE, c'est **sectoriel vs horizontal**.

Reste à acter **comment ce pont est construit, stocké, et comment sa sortie se comporte** — avant que F-063/F-064 ne l'implémentent.

## Décision

Six principes.

**1. Crosswalk éditorial multi-schémas → NAF, conservateur.** **Une** table logique, discriminant **`Scheme`** (`EuroVoc`, `EurLexDirectory`, plus tard `JorfNor`, `FrCode`) : la clé *source* varie selon la source (URI EuroVoc, code répertoire, préfixe NOR, Code modifié), la **cible est toujours NAF**. Construite **pilotée par la demande** (uniquement les secteurs présents dans les portefeuilles), **conservatrice** (dans le doute on ne mappe pas : un mapping manquant = silence honnête, un mapping faux = bruit + faux verdict). L'IA peut *proposer* des alignements par similarité de libellés ; **un humain valide**.

**2. Granularité = division NAF (2 chiffres).** Pas la section (trop grossière), pas le 5-positions (fausse précision = violation de doctrine). Les sections se dérivent par préfixe. Repère : « agroalimentaire » n'est pas un nœud NAF — c'est section A (divisions 01-02-03) **+** divisions 10-11 (vivant dans la section C). La division est le bon grain ; un concept source y mappe une **liste**.

**3. `Scope` (Sectoral / Horizontal) = levier anti-noyade.** Un texte horizontal n'est **pas supprimé** (cacher un fait) : il est **routé** vers un canal « réglementaire transverse » que l'utilisateur active s'il veut. Le bruit se maîtrise au **routage**, jamais en masquant la donnée.

**4. Provenance de la table + sortie candidate.** Chaque règle porte une **`Curation`** (qui a validé, quand, confiance, justification) — la doctrine « tout a une source » repliée sur la table elle-même : une correspondance *est* une affirmation. La sortie du matcher est un **`MatchCandidate`** (ADR-014) — « secteur *semble* concerné — à vérifier », jamais un verdict (ADR-012). Fractal : table floue → candidat, jamais fait.

**5. Donnée de référence versionnée + service de domaine.** La **donnée** (`SectorMappingRule`) est un **fichier de référence versionné dans le repo** (JSON/CSV), seedé en base au démarrage — **comme les packs de veille (F-042)**. Bénéfice : **chaque modif de mapping = une PR relue** ; le `git diff` *est* l'audit de curation, la validation humaine et le versionnement, gratuits. Le **comportement** (texte → divisions candidates : lookup + dérivation section + routage Scope) est un **service de domaine pur `ISectorClassifier`** (`Atlas.Domain`), chargé en mémoire (crosswalk petit, pas d'I/O dans le domaine). L'**adapter EUR-Lex ne connaît pas le NAF** : il livre URIs EuroVoc + directory code, le classifier traduit.

**6. Table NACE 2↔2.1 séparée.** La table **éditoriale** source→NAF est maintenue sur **une seule version** de NAF (celle livrée par le RNE/INSEE). La table **mécanique et officielle** NACE rév.2 ↔ 2.1 (Eurostat) reste **distincte**, appliquée *au bord* seulement si la version des codes entités diffère du crosswalk. Les fusionner remettrait de l'éditorial flou dans une correspondance officielle nette (anti-pattern « fusionner deux natures », ADR-013).

### Croquis (illustratif)

```csharp
// ── Atlas.Domain ────────────────────────────────────────
public sealed record SectorMappingRule(
    MappingRuleId Id,
    SourceScheme  Scheme,        // EuroVoc | EurLexDirectory | JorfNor | FrCode …
    string        SourceKey,     // URI EuroVoc, "03", "AGR"… — STABLE, opaque
    string        SourceLabel,   // dénormalisé : œil du curateur / audit
    IReadOnlyList<NafDivision> Divisions,   // cible 1→N, granularité DIVISION
    MappingScope  Scope,         // Sectoral | Horizontal  ← anti-noyade
    Curation      Curation);

public sealed record Curation(
    string ValidatedBy, DateOnly ValidatedAt,
    MappingConfidence Confidence, string? Rationale);

// Service de domaine : (classif native) → secteurs candidats. Pas d'I/O.
public interface ISectorClassifier
{
    SectorClassification Classify(
        IReadOnlyList<EurovocConceptUri> eurovoc,
        DirectoryCode? directory);
}
```

```jsonc
// une ligne du fichier de référence versionné (seedé en base)
{ "scheme": "EurLexDirectory", "sourceKey": "03", "sourceLabel": "Agriculture",
  "divisions": ["01","02","03","10","11"], "scope": "Sectoral",
  "curation": { "validatedBy": "…", "validatedAt": "2026-05-31",
                "confidence": "High", "rationale": "inclut l'aval agro-industriel (10-11)" } }
```

## Rationale

- **L'éditorial est inévitable** (aucune table officielle) ; conservateur + grossier le rend honnête plutôt que bavard.
- **Réutilisation, pas invention** : packs F-042 (donnée de référence versionnée), `MatchCandidate`/ADR-014 (conservateur), F-047 (matching = candidat), ADR-013 (ne pas fusionner deux natures). On hérite de patterns éprouvés.
- **Le flag `Scope` est la réponse *structurelle* à la noyade** — au routage, pas au masquage.
- **La provenance sur la table** aligne la donnée de référence sur la doctrine du reste du produit.
- **Le `git diff` comme audit** transforme la validation humaine (principe 1) en mécanique gratuite.

## Conséquences

- **Positives** : signal honnête ; bruit maîtrisé par routage (jamais par masquage) ; audit/versionnement via PR ; granularité robuste ; migration de version NAF confinée ; cohérence doctrinale (descriptif, sourcé, candidat).
- **Négatives** : **charge éditoriale** (discipline de curation, à la main) ; couverture qui ne grandit qu'à la demande ; pas de ciblage fin (par design — la division est volontairement le plancher).
- **À prévoir** :
  - **F-063** (source EUR-Lex) et **F-064** (matcher) implémentent cet ADR.
  - **doc 08** : nommer `SectorMappingRule`, `SourceScheme`, `MappingScope`, `NafDivision`, `ISectorClassifier`, `MatchCandidate` (réf. ADR-014).
  - **Seuil de confiance** (à partir de quand un candidat est montré) : ouvert, à trancher à l'implémentation de F-064.
  - **Côté FR** (`JorfNor`, `FrCode`) = des **lignes** en plus, pas du schéma neuf.
  - **Bascule NACE 2.1** : prévoir la table mécanique 2↔2.1 quand le NAF livré par l'INSEE bascule.
  - **Références croisées** : **F-063 / F-064** ; **F-041 / F-047** (précédents source/matching), **F-042** (donnée de référence), **F-058** (digest sectoriel), **F-046** (règles perso) ; **ADR-012 / 013 / 014 / 018** ; **doc 07 / 08 / 11 / 12 / 14**.
