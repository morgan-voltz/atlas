# ADR-023 — Extraction documentaire souveraine (actes) : faits candidats ancrés à la page

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

Atlas **télécharge** déjà les actes et bilans (F-013, en masse F-014) mais n'en **extrait** rien — ce sont des binaires servis en proxy. Première précision de périmètre, décisive : le **côté financier est déjà couvert sans parsing** par **F-054** (ratios BCE/INPI + **bilans-saisis** : *« postes financiers structurés extraits par l'INPI »*, *« aucun parsing de PDF nécessaire »*). On ne re-parse pas ce que l'INPI structure déjà.

Le territoire vierge, c'est donc **les actes** : statuts, modifications, **PV**, **cessions de parts** — que personne ne pré-structure. Leur valeur marginale, **par rapport à RNE (état courant) et BODACC (événements, F-048)** qui captent déjà beaucoup, tient à trois choses qu'eux ne donnent pas : le **détail structuré** (qui cède combien de parts à qui), les **clauses précises**, et la **pièce primaire comme preuve**.

Mais deux faits cadrent la décision : l'extraction de texte juridique est **error-prone** (dangereux pour avocat/KYC), et ces documents sont **sensibles** (secret professionnel) — ils ne doivent pas partir chez un tiers. Deux questions transverses survivront aux fiches : la **posture d'exécution** (souveraineté) et le **statut des faits extraits** (oracle vs aide).

## Décision

Six principes.

**1. L'extraction est une aide à la lecture, pas un oracle.** Un fait extrait est un **candidat à vérifier** (`MatchCandidate`, ADR-014), **ancré à la page** (document + page), qui *amène* à la source — jamais un fait asséné. Formulation produit : « ce qui *semble* être une cession, page 4 — à vérifier », jamais « X a cédé Y ». Ce cadrage **dé-risque** *et* **autorise des modèles plus faibles** (donc locaux), puisque tout renvoie à la page pour vérification humaine.

**2. Souveraineté : extraction on-infra par défaut, cloud/BYOAI opt-in premium.** Statuts, cessions — secret professionnel : ces documents **ne quittent pas l'infra** dans le cœur AGPL/auto-hébergé. **OCR + extraction locaux** par défaut ; un chemin **cloud/BYOAI opt-in** (jamais le défaut, jamais imposé), patron de F-054/F-050 (cœur déterministe gratuit, IA premium via port ; ADR-006/009). C'est la différenciation.

**3. Provenance à la page = la forme la plus forte.** Chaque candidat porte sa provenance **jusqu'à la page** — généralisation de F-065. Synergie **F-066** : un fait extrait dans l'export auditable pointe vers *la page source*, la trace la plus défendable qui soit.

**4. Périmètre = actes, pas bilans.** Les bilans sont couverts sans parsing par F-054. On se concentre sur les actes, en v1 phasé (§5).

**5. Phasage : index d'abord, extraction ensuite.** (a) **Index documentaire** — classer chaque acte (type/date/objet) + **OCR → plein-texte cherchable** + **saut à la page**. Utile **seul**, **zéro fait asséné**, faible risque. (b) **Extraction de faits candidats** (cessions de parts, changements de capital/dirigeants), **par-dessus** l'index. On ne tente le dur qu'une fois l'index en place.

**6. Convergence : un fait extrait événementiel devient une entrée de trajectoire.** Une cession lue dans un PV daté du X → **`EntityChangeLogEntry`** (F-067) avec `EventTime` = date de l'acte, **adossée à la pièce primaire**. Et un acte non lisible / non océrisable = **état honnête** (`Unavailable`/`Restricted`), jamais « rien » (doctrine « indisponible ≠ vide »).

### Croquis (illustratif)

```csharp
// Application — un fait candidat extrait, TOUJOURS à vérifier, ancré à la page.
public sealed record ExtractedFactCandidate(
    Siren Subject,
    ExtractedFactKind Kind,        // ShareTransfer | CapitalChange | OfficerChange …
    string Value,                  // structuré : parties, quantités…
    AttachmentId SourceDocument,   // l'acte d'origine (F-013)
    int PageRef,                   // LA page — provenance à la page (généralise F-065)
    ExtractionConfidence Confidence);  // affichée ; jamais « confirmé »

// Port d'extraction : implémentation LOCALE par défaut ; cloud/BYOAI = adapter premium opt-in.
public interface IDocumentFactExtractor
{
    Task<IReadOnlyList<ExtractedFactCandidate>> ExtractAsync(
        AttachmentId document, ExtractionContext ctx, CancellationToken ct);
}
```

## Rationale

- **« Aide à la lecture »** est la **seule posture sûre** pour du juridique error-prone — et c'est elle qui rend le **local viable** (un modèle faible qui *propose* et *renvoie à la page* est acceptable ; un modèle qui *affirme* ne le serait pas).
- **Souveraineté** = différenciation + secret professionnel ; le cœur reste local, le cloud est un **choix de l'utilisateur**, pas un défaut.
- **Provenance à la page** = l'auditabilité poussée au maximum ; synergie directe F-066.
- **Phasage** = valeur livrée tôt (l'index) sans le risque de l'extraction ; on construit le dur sur de l'éprouvé.

## Conséquences

- **Positives** : déverrouille les actes (avocat / M&A / expert-comptable) ; différenciation souveraine ; faits **adossés à la pièce primaire** (force pour l'audit *et* la trajectoire) ; valeur livrée par étapes.
- **Négatives** : modèles locaux = extraction **plus faible** (absorbée par « candidat ») ; OCR/extraction = **compute** (à cadrer : on-demand entités suivies, cache) ; discipline de **provenance à la page**.
- **À prévoir** :
  - **F-069** (index documentaire) et **F-070** (extraction de faits) implémentent.
  - **doc 08** : nommer `ExtractedFactCandidate`, `IDocumentFactExtractor`, l'index documentaire.
  - **Modèles locaux** (OCR type Tesseract/PaddleOCR ; extracteur local) à choisir à l'implémentation ; chemin **cloud BYOAI = premium opt-in**.
  - **Compute** : on-demand (entité suivie / ouverture d'un document), cache.
  - **Références croisées** : **F-069 / F-070** ; **F-013 / F-014** (téléchargement), **F-054** (bilans déjà couverts — **ne pas re-parser**), **F-065 / F-066** (provenance / audit à la page), **F-067** (trajectoire, `EventTime`) ; **ADR-006 / 009** (open core / BYOAI), **ADR-012 / 014** ; **doc 08 / 11 / 12**.
