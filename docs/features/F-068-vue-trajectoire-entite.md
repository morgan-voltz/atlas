# F-068 — Vue trajectoire d'une entité

> **Statut** : 📋 Spécifiée — non implémentée (cluster Trajectoire, 31 mai 2026). La **surface** ; consomme la fondation **F-067**. Cadrée par **ADR-022**.

**Description** : vue d'évolution d'une entité — son **chemin** dans le temps. Deux rendus d'une même donnée historisée : un **curseur « état à une date »** (le dossier point-in-time) et une **timeline des changements** (la suite des évolutions). Consomme F-067.

**Valeur user** : rend la donnée historisée **lisible** — « voici comment cette entreprise a évolué ». Le persona **investisseur** pense trajectoire, pas instantané ; l'**avocat** veut l'état à la date d'un litige ; le **M&A** veut la dynamique d'une cible. C'est la matérialisation visible de la machine à remonter le temps.

**Complexité** : ★★★ (UI / rendu ; la donnée est déjà fournie par F-067).

**APIs externes** : —.

**Dépendances** : F-067 (la fondation — reconstruction + journal), F-056 (dossier — base de rendu), F-044 / F-047 (timeline — grammaire de rendu réutilisable), **ADR-022**.

**Détails techniques** :
- **Deux rendus, une donnée** : **curseur `asOf`** (→ dossier point-in-time via `IPointInTimeResolver`, F-067) **+ timeline de changements** (la suite des `EntityChangeLogEntry`).
- **Sélecteur d'axe** explicite quand les deux existent : **temps d'événement** vs **temps d'observation** — l'UI **dit lequel** est affiché (cohérent avec « dernière confirmation ≠ dernier changement » de F-065). Ne jamais mélanger silencieusement les deux axes.
- **`Unobserved` rendu honnêtement** : les périodes non observées sont **marquées comme trous**, jamais comme « stable » (doctrine ADR-022 ; doc 06 — jamais la couleur seule).
- **Doctrine** : montre la **trajectoire de faits** ; **jamais** « en déclin / en croissance » comme verdict, **aucune jauge rouge/verte** (ADR-012). L'utilisateur lit la tendance.
- **Accessibilité (ADR-008, bloquant)** : un graphe temporel est hostile au lecteur d'écran et au daltonisme. **Obligatoire** — **équivalent tabulaire/textuel** complet (« au 1ᵉʳ janvier 2024 : gérant X, capital Y »), aucune information par la **couleur ou la position seules** (même garde-fou que F-034).
- **Rendu** : un mode/onglet « évolution » dans la fiche / le dossier 360 (F-056) ; sur desktop, densité accrue (R4).
- **Hors-scope** : la rétention elle-même (F-067) ; toute **analyse prédictive ou scoring** de tendance (hors doctrine descriptive).
