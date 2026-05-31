# F-057 — Re-screening continu (surveillance des sanctions)

> **Statut** : V3+, **candidate naturelle au tout début de V3**. Surnom de travail : « le F-019 des sanctions ». Origine : graine du persona **Compliance / KYC**.
> **Doctrine** : sous **ADR-012** (descriptif, matching conservateur, aucun verdict) — incarnation par **ADR-014** (`INameAgainstListMatcher`). Hérite des frontières dures de F-055. **Substrat technique** : **ADR-013** (`IStateMonitor<SanctionsScreeningState>`, retraits détectés).

**Description** : une **surveillance continue** des entités **déjà suivies** (favoris F-017 / watchlists F-053) contre les **listes officielles de sanctions** (DG Trésor + liste consolidée UE, sources de F-055). Quand une entité **apparaît** (ou **disparaît**) d'une liste au fil de ses mises à jour, Atlas le **détecte et l'alerte** — timeline + push — avec une **piste d'audit** complète (date, liste, entrée, base du rapprochement).

C'est le pendant **temporel** de F-055 : F-055 répond à « cette entité est-elle sur une liste *maintenant* ? » ; F-057 répond à « **préviens-moi quand ça change** ». Mécanisme de F-019 (snapshot + diff + job) appliqué à l'**état de correspondance sanctions**.

**Valeur user** : pour le persona **Compliance / KYC**, le **re-screening** est une **obligation LCB-FT cœur**. Le faire en continu, **souverainement**, **descriptivement** et avec une **piste d'audit défendable**, c'est exactement le positionnement qu'aucun fournisseur cloud à score boîte noire ne sert proprement.

**Complexité** : ★★★ — peu de neuf : réutilise le snapshot/diff de **F-019**, le patron polling + dédup cross-users de **F-048**, les sources et le matching de **F-055**, la timeline **F-047**, le push **F-020**. Le délicat n'est pas le code, c'est la **conservativité du matching** et le **cadre légal**.

**APIs externes** : celles de **F-055** — **Registre national des gels DG Trésor** + **liste consolidée UE**. Aucune nouvelle source.

**Dépendances** : **F-055** (sources + logique de rapprochement), **F-019** (patron snapshot/diff + job), **F-047** (`FavoriteEvent` + timeline), **F-048** (patron polling + dédup `ExternalId`), **F-053/F-017** (le set suivi), **F-046** (`WatchRule` d'activation), **F-020** (push), **ADR-012** (doctrine), **ADR-013** (substrat — c'est la **3ᵉ instance** qui déclenche l'extraction effective du runner), **ADR-014** (`INameAgainstListMatcher`).

**Hors-périmètre (explicite)** :
- **Pas de score, pas de verdict, pas de label « à risque »** (ADR-012). Une correspondance = **« correspondance potentielle à vérifier »**, jamais une affirmation.
- **Pas de PEP ni d'adverse-media propriétaire** ; **pas de bénéficiaires effectifs**. Le périmètre reste celui de F-055.
- **Pas de certification de conformité** — Atlas alerte, l'entité assujettie reste responsable de sa décision.
- **v1 = entités légales** uniquement (par dénomination). Personnes physiques (dirigeants) hors v1 — diffamation trop risquée.

**Détails techniques** :
- **4ᵉ volet de la timeline**, dans la lignée de F-048 ; **ADR-013** appliqué :
  - **`IStateMonitor<SanctionsScreeningState>`** (Domain) — état = ensemble des correspondances courantes (formes normalisées / hash, comme le hash dirigeants de F-019).
  - **Diff** : `DiffWith(...)` retourne les correspondances **ajoutées** et **levées** (retraits → c'est pourquoi `IStateMonitor` et non `IItemStreamMonitor`).
  - **Runner mutualisé** (Application, ADR-013) : itère le set, dédup cross-users (un seul calcul de rapprochement par SIREN partagé), isolation des échecs, idempotence.
  - **Job Hangfire** `sanctions-rescreening`, cron `0 5 * * *` (après `favorite-refresh` 03:00 et `bodacc-polling` 04:00).
  - **Événement** : nouveau type `FavoriteEvent` `SanctionsScreeningChanged` (`ExternalId` = identifiant d'entrée de liste + version).
- **Listes locales rafraîchies** : cache local des listes officielles ; le re-screening tourne contre le cache.
- **Matching conservateur** (ADR-014, `INameAgainstListMatcher`) : sortie en `MatchCandidate` toujours formulée « **à vérifier** ».
- **Audit** : les événements sont **persistés** (date, liste, entrée, base du rapprochement) — c'est la valeur compliance, à exposer comme **journal défendable**.

**Cadre légal** : listes **officielles, gratuites, publiques** (DG Trésor, UE) → réutilisation OK. Le rapprochement manipule des **données personnelles** et touche à des allégations sensibles → **ADR-012**, matching conservateur, et l'utilisateur compliance est **responsable de traitement** de son screening (DPIA probable côté usage). **Aucune certification AML**.

**Accessibilité (ADR-008)** : alertes pleinement lisibles au lecteur d'écran, mention « correspondance potentielle à vérifier » explicite (jamais portée par la seule couleur), journal d'audit consultable de façon accessible.

**Modèle économique (ADR-006)** : déterministe contre listes gratuites → **cœur / OSS**. Quotas hébergé possibles (nombre d'entités sous surveillance), patron de F-043.

**Découpage / jalons** :
1. **Snapshot + diff** : `SanctionsScreeningSnapshot` + `DiffWith`, sur le rapprochement de F-055.
2. **Job + événement** : `sanctions-rescreening` → `FavoriteEvent SanctionsScreeningChanged` (détectée / levée), dédup `ExternalId`.
3. **Activation par règle** : `WatchRule` « surveillance sanctions » au niveau favori / watchlist (F-046).
4. **Journal d'audit** : exposition consultable et exportable.

**Décisions ouvertes** :
- **Opt-in par watchlist vs automatique pour tous les favoris** (recommandé : opt-in).
- **Personnes physiques** : v1 entité seule ; cas dirigeants à examiner plus tard sous garde-fou renforcé.
- **Levées (délistage)** : alerter aussi recommandé.
- **Stockage d'audit** : `FavoriteEvent` ou journal de screening dédié ?
- **Fréquence** : quotidienne (aligne F-019/F-048) ou calée sur la mise à jour réelle des listes ?
- **`FavoriteEvent` → `EntityEvent`** : décision ouverte d'ADR-013 à trancher au moment de l'extraction du runner.
