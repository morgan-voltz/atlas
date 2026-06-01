# ADR-022 — Historisation des entités : journal bi-temporel append-only & reconstruction « état à une date »

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

Plusieurs besoins convergent vers une même capacité : **reconstituer l'état d'une entité à une date passée** et **rejouer sa trajectoire**. C'est le besoin nommé du persona **investisseur** (« rejouer la trajectoire »), la base de l'**export auditable à une date passée** (F-066 × le temps), et le prolongement naturel de l'`AsOf` du dossier (ADR-015) et du « dernière confirmation ≠ dernier changement » (F-065).

Mais le substrat ADR-013 **ne le permet pas tel quel** : sa stratégie état+diff garde *« un cliché vivant de l'état courant »* comparé *« au précédent »* — **pas un historique versionné** — et l'ADR refuse explicitement d'archiver (*« stocker et re-différer un état serait du volume non borné inutile »*). Ce qu'on possède : le **snapshot courant** + un **journal de changements émis** (les `FavoriteEvent`, le « journal défendable » de F-057). Ce qui manque : la **reconstruction à une date arbitraire**.

Il faut donc décider une **couche de rétention** — et deux subtilités la portent : la **bi-temporalité** (le temps où un fait s'est produit ≠ le temps où Atlas l'a observé) et l'**honnêteté temporelle** (une période non observée n'est pas « inchangée »).

## Décision

Six principes.

**1. Rétention par journal de changements append-only (hybride permis).** On persiste chaque **`MonitoredChange`** (déjà émis par ADR-013) dans un **journal append-only**, daté — et on **rejoue** ce journal depuis une baseline pour reconstituer n'importe quel état passé. Hybride autorisé : **snapshots baseline périodiques + journal entre deux** (borne le coût de replay). On **réutilise ce qu'on émet déjà** ; on n'invente pas une capture neuve.

**2. Bi-temporalité ciblée.** Chaque entrée porte **deux axes** : le **temps d'événement** (*valid time* — quand le fait s'est produit dans le réel : date d'acte RNE, date de publication BODACC) et le **temps d'observation** (*transaction time* — quand Atlas l'a enregistré). **Bi-temporel** pour les dimensions où la source **fournit** une date d'événement ; **mono-temporel assumé** ailleurs. C'est ce qui rend l'« état à une date » **non ambigu** : *tel que le monde était* vs *tel qu'Atlas le savait* — distinction **juridiquement porteuse** pour l'audit.

**3. `Unobserved` : l'honnêteté temporelle.** `SectionState` (ADR-015) gagne un état **`Unobserved`** : une période **antérieure au suivi** est un **trou explicite**, jamais aplatie en « inchangé ». Deux régimes distingués : **histoire observée** (le journal, fine et fiable depuis le suivi) et **histoire fournie par la source** (dates d'actes RNE, archive BODACC, exercices de bilans — disponible mais grossière et lacunaire). C'est la doctrine « indisponible ≠ vide » **étendue à l'axe temporel**.

**4. Périmètre = entités suivies, d'abord.** On historise ce qu'on observe déjà (le portefeuille), pas tout le RNE — cohérent avec le snapshot-first (ADR-015) et l'horreur du volume non borné (ADR-013). Une entité non suivie n'a **pas d'histoire observée** → `Unobserved`, honnêtement.

**5. Dimensions prioritaires.** v1 : **identité / dirigeants / capital (RNE)** et **sanctions (F-057)** — les dimensions en stratégie **état+diff**, où « ce qui a changé / disparu » compte, et que l'audit daté doit porter en premier. Les **flux append-only** (BODACC, Judilibre) **sont déjà un historique** par nature → intégrés quasi gratuitement. Les **bilans (F-054)** ont leur temporalité propre (exercices) → branchés ensuite.

**6. Le dossier 360 devient paramétrable par le temps.** La reconstruction **réutilise les résolveurs de section** de F-056 : `AsOf` passe de *propriété affichée* à *paramètre de requête*. Le snapshot-first d'ADR-013 reste le **présent** ; le journal ajoute le **passé**. Une seule machinerie, désormais trois usages (surveillance, dossier courant, dossier passé).

### Croquis (illustratif)

```csharp
// Journal append-only — une entrée = un MonitoredChange (ADR-013) horodaté sur DEUX axes.
public sealed record EntityChangeLogEntry(
    Siren Subject,
    MonitoredDimension Dimension,
    ChangeKind Kind,                 // Added / Modified / Removed
    string Delta,                    // la variation (champ → ancienne/nouvelle valeur)
    DateTimeOffset? EventTime,        // TEMPS D'ÉVÉNEMENT (valid time) — de la source, nullable
    DateTimeOffset ObservedAt,        // TEMPS D'OBSERVATION (transaction time) — Atlas
    string Provenance, string SourceRef);

// Reconstruction : rejoue le journal jusqu'au cutoff, sur l'axe choisi, par-dessus la baseline.
public interface IPointInTimeResolver
{
    Task<CompanyDossier> ResolveAsOfAsync(
        Siren siren, DateTimeOffset asOf, TimeAxis axis, CancellationToken ct);
}

// SectionState (ADR-015) gagne un état temporel.
public enum SectionState { Available, Stale, Unavailable, NotApplicable, Restricted, Unobserved }
```

## Rationale

- **On prolonge, on n'invente pas** : le journal persiste des `MonitoredChange` déjà produits ; le snapshot+diff *est* la brique d'une reconstruction temporelle.
- **Append-only + baseline** respecte l'horreur du volume non borné d'ADR-013 (on borne le replay), tout en débloquant le passé.
- **Bi-temporalité ciblée** = l'audit daté devient honnête sans surcoût là où la source ne donne pas de date d'événement.
- **`Unobserved`** = la seule façon honnête de représenter un trou ; même garde-fou doctrinal que `SectionState`.
- **Réutilisation de F-056** = le dossier passé et le dossier présent partagent les résolveurs ; zéro duplication.

## Conséquences

- **Positives** : débloque l'export à une date passée (F-066) et la trajectoire (persona investisseur) ; honnêteté temporelle native ; réutilise substrat + dossier ; coût borné.
- **Négatives** : **discipline de cohérence** (replay déterministe → mêmes entrées = même état reconstitué) ; la bi-temporalité **double** la rigueur sur les dates (toujours dire *quel* axe) ; stockage du journal à surveiller (mitigé par baseline + périmètre suivi).
- **À prévoir** :
  - **F-067** (fondation temporelle : journal + reconstruction) et **F-068** (vue trajectoire) implémentent.
  - **doc 08** : nommer `EntityChangeLogEntry`, `IPointInTimeResolver`, `TimeAxis`, l'état `Unobserved` ; documenter les deux régimes (observé / source).
  - **Backfill source** (actes RNE, archive BODACC, bilans passés) : périmètre et limites à cadrer en F-067 — ne jamais combler un trou par de la supposition.
  - **Synergie F-066** : l'export auditable gagne un paramètre `asOf` (dossier scellé *à une date*).
  - **Références croisées** : **F-067 / F-068** ; **ADR-013** (substrat, source des changements), **ADR-015 / F-056** (dossier paramétrable), **F-065 / F-066** (provenance / audit), **F-057** (journal sanctions), **F-054** (bilans), **ADR-012** ; **doc 08 / 11 / 12**.
