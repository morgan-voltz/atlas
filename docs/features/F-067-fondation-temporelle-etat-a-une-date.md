# F-067 — Fondation temporelle (journal bi-temporel + reconstruction « état à une date »)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Trajectoire, 31 mai 2026). Le **moteur** (invisible) de la machine à remonter le temps ; la vue est **F-068**. Cadrée par **ADR-022**. A une valeur **seule** : débloque l'export à une date passée (F-066) sans aucune UI de trajectoire.

**Description** : couche de rétention qui **persiste les changements** détectés par le substrat (ADR-013) dans un **journal append-only bi-temporel**, et **reconstitue l'état d'une entité à une date passée** (`asOf`). Concrètement, elle fait passer l'`AsOf` du dossier (F-056) de *propriété affichée* à *paramètre de requête*.

**Valeur user** : invisible mais structurante. Débloque immédiatement l'**export auditable à une date passée** (F-066), fonde le besoin « rejouer la trajectoire » du persona **investisseur**, et sert l'**avocat** (état à la date d'un litige). C'est le socle de toute lecture historique.

**Complexité** : ★★★★ (rétention bi-temporelle + reconstruction déterministe ; le morceau dur de la piste).

**APIs externes** : — (consomme les changements du substrat ADR-013 ; aucune source nouvelle pour l'histoire observée). Le **backfill** lit ce que les sources donnent déjà (actes RNE, archive BODACC, exercices de bilans).

**Dépendances** : ADR-013 (substrat — émet les `MonitoredChange`), ADR-015 / F-056 (dossier — devient paramétrable par le temps), **ADR-022**. Synergie : F-066 (export à une date).

**Détails techniques** :
- **Journal append-only** `EntityChangeLogEntry` : persiste chaque `MonitoredChange` (ADR-013) enrichi de **deux axes** — `EventTime?` (temps d'événement, de la source, *nullable*) + `ObservedAt` (temps d'observation) — **+ provenance + référence source**. **Hybride** permis : snapshot baseline périodique + journal entre deux (borne le coût de replay).
- **Reconstruction** `IPointInTimeResolver.ResolveAsOfAsync(siren, asOf, axis)` : **rejoue** le journal jusqu'au cutoff, **sur l'axe choisi**, par-dessus la baseline → dossier point-in-time. **Réutilise les résolveurs de section de F-056** (lecture de l'historique au lieu du courant).
- **`SectionState` étendu de `Unobserved`** (ADR-022 §3) : période antérieure au suivi = **trou explicite**, jamais « inchangé ». **Deux régimes distingués** : histoire **observée** (journal) vs histoire **fournie par la source** (backfill grossier/lacunaire). **Ne jamais combler un trou par de la supposition.**
- **Périmètre = entités suivies** (snapshot-first ADR-015 ; volume borné ADR-013). Entité non suivie = pas d'histoire observée → `Unobserved` honnête.
- **Dimensions v1** : identité / dirigeants / capital (RNE) + sanctions (F-057, état+diff). Flux append-only (BODACC, Judilibre) **déjà un historique** → intégrés quasi gratuitement. Bilans (F-054) **ensuite** (temporalité propre = exercices).
- **Cohérence (discipline ADR-022)** : replay **déterministe** — mêmes entrées ⇒ même état reconstitué (ordre stable). Prérequis du hash stable de l'export daté (F-066).
- **Placement hexagonal** : journal = persistance (Infra) derrière un port ; reconstruction = service Application réutilisant les résolveurs de section ; `SectionState` / `Unobserved` = Domain.
- **Doctrine** : chaque entrée porte provenance + les deux dates ; descriptif (ADR-012), aucun verdict.
- **Hors-scope** : la **vue trajectoire** (F-068) ; le backfill **au-delà** de ce que la source donne nativement (pas de reconstruction inventée).
