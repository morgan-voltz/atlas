# F-058 — Signaux concurrentiels & digest sectoriel

> **Statut** : V3+, **s'allume progressivement** — le volet « signaux PI » dépend de F-027 (V2). Origine : graine du persona **Veille concurrentielle B2B**.
> **Doctrine** : descriptif — on **fait remonter des faits**, on ne classe pas les concurrents par « menace ». Matching presse sous **ADR-014** (`INameInTextMatcher`).

**Description** : deux volets complémentaires posés sur la timeline mixte (F-047) :

1. **Signaux concurrentiels de premier rang** — élever les événements **structurés** au rang de signaux **typés** : un **dépôt PI** (F-027), un **marché public remporté** (F-032), un **événement légal** (BODACC F-048, RNE F-019) deviennent des `FavoriteEvent` **classés par type de signal**, filtrables (« montre-moi les mouvements **produit** = PI » vs « leurs mouvements **commerciaux** = marchés »).
2. **Digest sectoriel** — une **synthèse périodique** (hebdo par défaut) « ce qui a bougé dans mon secteur » sur une **watchlist** : un récapitulatif groupé des signaux de la semaine, livré en email / push / in-app.

**Valeur user** : pour le persona **Veille concurrentielle B2B** — transformer une timeline brute en **intelligence actionnable** (signaux typés + briefing hebdomadaire) de façon **souveraine** et **descriptive**, là où les plateformes CI (Crayon, Klue, Contify) font de l'auto-résumé IA sur l'empreinte **web**. Atlas le fait sur la **donnée officielle**.

**Complexité** : ★★★ — réutilise la timeline F-047, le patron `FavoriteEvent`, la dédup F-045, les canaux de notification et les ports premium F-050. Le neuf : la **taxonomie de signaux**, le **job de digest** + le groupement, et le **branchement de la synthèse IA**.

**APIs externes** : aucune nouvelle. Réutilise les sources de **F-027** (INPI/EUIPO/OMPI PI), **F-032** (DECP), **F-048** (BODACC), F-041 (RSS).

**Dépendances** : **F-047** (timeline + `FavoriteEvent`), **F-027** (dépôts PI — V2, conditionne le volet « signal produit »), **F-032** (DECP — marchés), **F-048/F-019** (événements légaux), **F-053** (watchlist = mes concurrents), **F-050** (`IFeedSummarizer` pour la synthèse premium), **F-046** (filtres/règles), **F-020** (push), **ADR-014** (matching presse `INameInTextMatcher`).

**Hors-périmètre (explicite)** :
- **Pas de score de menace, pas de classement concurrentiel, pas de verdict stratégique** : le digest résume des faits, il ne conseille pas.
- **Pas de scraping** des sites / prix / offres d'emploi des concurrents (gap assumé face à Crayon — le modèle RSS + officiel ne scrape pas).
- **Pas de battlecards ni de win/loss** (territoire des plateformes CI) — hors sujet.

**Détails techniques** :
- **Taxonomie** : étendre les types de `FavoriteEvent` avec des **signaux concurrentiels** — `IpFiled` (depuis F-027), `PublicContractAwarded` (depuis F-032), en plus de `RneChanged` / `BodaccPublished` existants. Brancher F-027 et F-032 pour **émettre** ces events dans la timeline (patron F-048, `ExternalId` pour la dédup).
- **Filtrage** : extension des filtres de timeline / des règles F-046 — `?signalTypes=IpFiled,PublicContractAwarded`.
- **Digest** : job Hangfire récurrent (p. ex. `sector-digest-weekly`) qui, par user/watchlist **opt-in**, agrège les `FavoriteEvent` de la période pour les SIREN de la liste, les **groupe** par entité / type de signal, et produit une `Notification` digest.
  - **Cœur** : récap déterministe groupé.
  - **Premium** : mise en récit IA via `IFeedSummarizer` (patron F-050) — **descriptive** (« X a déposé 3 marques et remporté le marché Y »), jamais d'interprétation (« X est en train de gagner »).
- Réutilise la **timeline de watchlist** (F-053) comme socle d'agrégation.

**Cadre légal** : faits **publics/officiels** (dépôts, marchés, annonces légales) → descriptif. **Matching presse conservateur** sous **ADR-014** (homonymes de raison sociale, volet 1 de F-047). Données de dirigeants sous **ADR-012**. Aucun traitement sensible nouveau.

**Accessibilité (ADR-008)** : digest **lisible** (email + in-app), type de signal **jamais** porté par la seule couleur (libellé explicite), filtres de timeline accessibles au clavier et annoncés au lecteur d'écran.

**Modèle économique (ADR-006 + ADR-009)** : typage des signaux + digest **déterministe** = **cœur / OSS** (cohérent open-core). **Synthèse narrative IA** = **premium** (coût LLM, port F-050). Quotas hébergé possibles (taille de watchlist surveillée, fréquence).

**Découpage / jalons** :
1. **Taxonomie + branchements** : types de signaux, émission depuis F-032 (marchés) puis F-027 (PI) en `FavoriteEvent`.
2. **Filtrage** par type de signal sur la timeline.
3. **Digest déterministe** par watchlist (job + `Notification` groupée).
4. **Synthèse IA premium** (F-050).

**Décisions ouvertes** :
- **Cadence du digest** : hebdo par défaut, configurable (quotidien/mensuel) ?
- **Portée** : par watchlist uniquement, ou un digest global « tous mes suivis » ?
- **Signal = classification persistée** sur l'event, **ou** simple couche de présentation au-dessus des types existants ?
- **Opt-in** du digest (recommandé) et choix du canal.
