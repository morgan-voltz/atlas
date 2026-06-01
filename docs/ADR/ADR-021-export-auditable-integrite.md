# ADR-021 — Export auditable : intégrité par empreinte + horodatage, posture « trace vérifiable »

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

L'auditabilité est le fil rouge d'Atlas, et le substrat existe déjà : `DossierSection` (ADR-015) porte `AsOf` (fraîcheur) + `Provenance` (source + base) **par section**, `SectionState` distingue `Available`/`Stale`/`Unavailable`/`NotApplicable`/`Restricted`, et l'UI affiche partout l'atome « ligne de provenance » + le bouclier « À jour ».

Ce qui manque, c'est le **livrable** : un **export de dossier portant la traçabilité complète**, réclamé par les personas **KYC/compliance** et **avocat** (dossier de due diligence, état traçable à une date). F-022 fournit un *rapport PDF* de fiche — pas un artefact de traçabilité scellé. F-066 portera ce livrable.

Deux questions transverses se posent dessus et **survivront aux fiches** — donc se décident ici :

1. **L'intégrité.** Un export non scellé est trivialement contestable (« qui dit que ce PDF n'a pas été retouché ? »). Faut-il une garantie d'inaltérabilité ?
2. **La posture juridique.** Le risque inverse : **surclamer** « preuve opposable » et sortir de la doctrine descriptive d'Atlas.

## Décision

Six principes.

**1. Intégrité par empreinte + horodatage.** Tout export auditable embarque une **empreinte cryptographique** (SHA-256) de son contenu **canonicalisé** et un **horodatage de génération**. v1 : hash + horodatage serveur, **auto-vérifiable** (re-hasher le contenu et comparer). Évolution non bloquante : **horodatage qualifié RFC 3161** (autorité tierce) pour une opposabilité renforcée, par-dessus le même socle.

**2. Posture « trace vérifiable », jamais « preuve légale ».** Atlas fournit *exactement ce qu'il a vu, daté, sourcé, et inaltéré depuis l'export*. Il ne **certifie pas** la recevabilité juridique ni la véracité de la source amont — l'utilisateur (son process) qualifie. C'est **ADR-012** (descriptif, jamais verdict) prolongé jusqu'à l'artefact d'audit. L'export **le dit explicitement** : « trace vérifiable des données telles que consultées le {date} ; ni acte de certification, ni avis juridique ».

**3. Granularité par fait (extension du par-section).** L'audit pousse la provenance du *par-section* (ADR-015) vers le *par-fait* quand c'est nécessaire : chaque valeur exportée porte sa **source + sa référence exacte** (id d'acte INPI, annonce BODACC, numéro CELEX, dataset + date) **+ son `AsOf`**. On **étend** `Provenance`/`AsOf`, on ne les remplace pas.

**4. L'export reflète l'état au moment T — états honnêtes inclus.** `Stale`, `Unavailable`, `Restricted`, `NotApplicable` apparaissent **dans** l'export, jamais aplatis en « rien ». Un audit honnête montre aussi ce qu'on **n'a pas pu** voir (cohérent ADR-018/015/016 : « indisponible ≠ vide »).

**5. Distinct de l'export RGPD.** L'export auditable porte sur le **dossier d'une entité tierce** (la matière consultée) ; l'export RGPD (art. 20 — le « Exporter mes données », JSON : favoris/listes/historique) porte sur **les données personnelles de l'utilisateur**. Deux artefacts, deux finalités — à ne jamais confondre.

**6. Souverain / auto-hébergeable.** Génération **et** scellement **côté serveur Atlas** (y compris auto-hébergé), **sans dépendance cloud tierce obligatoire** — l'horodatage RFC 3161 qualifié, *s'il est activé*, est la seule dépendance externe optionnelle. C'est la différenciation : un export d'audit où la donnée (souvent sensible, KYC) **ne transite jamais par un tiers**.

### Croquis (illustratif)

```csharp
// Application — artefact d'audit, sérialisé puis scellé.
public sealed record AuditExport(
    Siren Subject,
    DateTimeOffset GeneratedAt,
    string ProductVersion,
    IReadOnlyList<AuditedFact> Facts,         // granularité PAR FAIT
    IReadOnlyList<SectionStateEntry> States,  // états honnêtes inclus (Stale/Unavailable/…)
    Integrity Integrity);

public sealed record AuditedFact(
    string Path, string Value,
    string Provenance, string SourceRef,      // id acte INPI / annonce BODACC / CELEX / dataset
    DateTimeOffset? AsOf);

public sealed record Integrity(
    string HashAlgo,        // "SHA-256"
    string ContentHash,     // sur le contenu CANONICALISÉ (ordre déterministe)
    DateTimeOffset SealedAt,
    string? Rfc3161Token);  // null en v1 ; horodatage qualifié en évolution

// Mention portée par l'export :
// « Trace vérifiable des données telles que consultées le {GeneratedAt}.
//   Ni acte de certification, ni avis juridique. »
```

## Rationale

- **Sans intégrité, l'export est contestable** : hash + horodatage = le minimum qui transforme « un PDF » en « trace vérifiable », et c'est **auto-vérifiable** (pas besoin d'Atlas pour re-vérifier).
- **La posture descriptive protège juridiquement Atlas *et* respecte la doctrine** : on ne se met pas à certifier ce qu'on ne peut pas certifier (ADR-012 jusqu'au bout).
- **Granularité par fait** = ce que l'audit exige, en *extension* du modèle par section (pas une refonte).
- **États honnêtes dans l'export** = cohérence avec toute la doctrine « indisponible ≠ vide ».
- **Auto-hébergeable** = la différenciation souveraine ; le dossier KYC sensible ne sort pas chez un tiers — là où l'équivalent fermé est cloud.

## Conséquences

- **Positives** : livrable à forte valeur KYC/avocat ; différenciation **souverain + tamper-evident** ; cohérence doctrinale ; réutilise `Provenance`/`AsOf` existants.
- **Négatives** : **canonicalisation** du contenu pour un hash stable = discipline (même dossier → même hash : ordre déterministe, normalisation) ; plus de provenance à porter (par-fait) ; **wording juridique** à soigner (ne pas surclamer).
- **À prévoir** :
  - **F-065** (surface navigable) et **F-066** (export) implémentent.
  - **doc 08** : nommer `AuditExport`, `AuditedFact`, `Integrity` ; documenter l'extension de `Provenance` vers le par-fait.
  - **Horodatage qualifié RFC 3161** : évolution, à évaluer si un persona l'exige.
  - **Mention légale** de l'export à faire valider (posture « trace vérifiable »).
  - **Références croisées** : **F-065 / F-066** ; **F-022** (rapport PDF — base de rendu), **F-021** (export), **F-012** (RGPD — à ne pas confondre), **F-056** (dossier 360 — source de la matière) ; **ADR-012 / 015 / 018** ; **doc 08 / 11 / 12**.
