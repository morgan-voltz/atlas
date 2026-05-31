# F-031 — Jurisprudence rattachée à l'entité (Judilibre)

> **Réactivée & recadrée le 29 mai 2026** — remplace le stub V3+ initial (motif « matching d'entité non-trivial »). Le **corpus s'est enrichi** (Cour de cassation ~535k décisions, arrêts de CA civils/commerciaux depuis avril 2022, TJ en déploiement, API gratuite via PISTE) et le **rapprochement est faisable** : les **personnes morales y gardent leur nom** (les personnes physiques sont pseudonymisées). On **garde le numéro F-031** (comme F-032/DECP). Origine : graine du persona **Avocat / juriste d'affaires**.
> **Doctrine** : sous **ADR-012** (descriptif, matching conservateur, aucun verdict) — incarnation par **ADR-014** (`MatchCandidate`, `INameInTextMatcher`). Garde-fou avocat : **Atlas ne rend jamais d'avis juridique**.

**Description** : faire remonter les **décisions de justice** où une **entité suivie / consultée** (personne morale) **apparaît**, par rapprochement de sa **dénomination** avec Judilibre. Une **liste descriptive et sourcée** (juridiction, date, identifiant, lien), en **matching conservateur** — « **mention potentielle à vérifier** », jamais une affirmation ni une interprétation.

**Valeur user** : pour l'**avocat / juriste d'affaires**, la jurisprudence **rattachée à l'entité** (les moteurs de jurisprudence ne partent jamais de l'entreprise — Atlas, si). Alimente aussi la **section « jurisprudence » du dossier 360 (F-056)** et le flux des **signaux légaux (F-058)**.

**Complexité** : ★★★ — le délicat : l'**adapter PISTE** (OAuth2) et la **conservativité du rapprochement** sur la dénomination. Le reste réutilise les patrons existants (timeline, événements, dossier).

**APIs externes** : **Judilibre via PISTE** (OAuth2, gratuit, sandbox + production), déjà catalogué au **doc 03 §4.1**. Établit le **patron d'adapter PISTE OAuth2** réutilisable (cf. autres sources PISTE).

**Dépendances** : **F-047** (timeline/événement), **F-056** (section « jurisprudence » du dossier), **F-053/F-017** (set suivi), **F-058** (signal légal), **ADR-012** (doctrine), **ADR-013** (substrat de surveillance — stratégie `IItemStreamMonitor<JudilibreDecision>` append-only), **ADR-014** (`INameInTextMatcher` partagé avec F-047).

**Hors-périmètre (explicite)** :
- **Aucun rapprochement de personnes physiques** : pseudonymisées dans Judilibre — respecté.
- **Aucun « score de contentieux »**, aucune interprétation, aucun **avis juridique**.
- **Couverture limitée** au corpus Judilibre (Cassation + CA civil/commercial depuis 2022 + TJ en déploiement ; **hors pénal**).

**Détails techniques** :
- **`JudilibreProvider`** : client PISTE (OAuth2, cache de token), recherche **plein texte** filtrée (juridiction, date, type).
- **Rapprochement conservateur** (ADR-012 + ADR-014) : recherche par **dénomination exacte/quasi-exacte** via `INameInTextMatcher`, **biais vers le faux négatif** (une fausse association « cette boîte était dans ce litige » est **diffamatoire**) ; sortie en `MatchCandidate` formulée « **mention potentielle à vérifier** ».
- **Mode événement** (ADR-013) : `IItemStreamMonitor<JudilibreDecision>` (append-only, dédup par identifiant de décision) → nouveau type de `FavoriteEvent` `JudilibreDecision` → timeline. Polling au patron de F-048 / F-019.
- **Mode dossier** : section « jurisprudence » dans F-056, à la demande.
- **Respect de la pseudonymisation** : on n'exploite **que** les personnes morales.

**Cadre légal** : Judilibre = **open data officiel et gratuit** ; réutilisation sous **CGU**. Pseudonymisation des personnes physiques respectée. **Matching conservateur** (risque diffamatoire) ; **ADR-012** ; descriptif ; **jamais d'avis juridique**.

**Accessibilité (ADR-008)** : listes pleinement accessibles, mention « à vérifier » explicite (jamais portée par la seule couleur), navigation clavier.

**Modèle économique (ADR-006)** : recherche / rapprochement **déterministe**, API PISTE **gratuite** → **cœur / OSS**. Option **premium** : **résumé descriptif** d'une décision par IA (patron F-050).

**Découpage / jalons** :
1. **Adapter PISTE + recherche à la demande** par dénomination → section dossier F-056.
2. **Mode événement** : polling au patron `IItemStreamMonitor` → `FavoriteEvent JudilibreDecision` → timeline.
3. **Premium (option)** : résumé descriptif de décision (F-050).

**Décisions ouvertes** :
- **Strictness du rapprochement** : seuil et traitement des variantes de raison sociale.
- **Périmètre de corpus** affiché.
- **Fréquence** du mode événement.
- **Résumé premium** : dans le périmètre ou non ?
