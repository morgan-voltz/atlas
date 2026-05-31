# F-073 — Règles de surveillance composées en langage naturel

> **Statut** : 📋 Spécifiée — non implémentée (cluster Surfaces agentiques, 31 mai 2026). **Fondation bornée** ; étend **F-046** ; le LLM est en **composition seulement**. Cadrée par **ADR-025**.

**Description** : l'utilisateur **décrit en langage naturel** ce qu'il veut surveiller (« préviens-moi si une boîte de mon secteur en Hauts-de-France remporte un marché > 100k€ ou subit une procédure ») ; un agent **compose** un `FeedRule` / `WatchRule` structuré, que l'utilisateur **revoit et valide** avant activation. Une fois validée, la règle s'exécute **déterministiquement** (moteur F-046).

**Valeur user** : transforme la configuration de surveillance — cliquer des filtres — en **une phrase**. Abaisse la barrière pour tous les personas (expert-comptable, veille, investisseur). C'est le « copilote de saison des comptes » côté règles.

**Complexité** : ★★★ (composition NL → structure + UX de validation ; l'exécution est déjà là).

**APIs externes** : — pour le cœur (**BYOAI** : inférence côté agent de l'utilisateur). Option **premium** : orchestration hébergée.

**Dépendances** : **F-046** (`FeedRule` / `WatchRule`, moteur déterministe), **F-052 / ADR-016** (surface MCP + outil de composition), **ADR-025**, ADR-013 (exécution).

**Détails techniques** :
- Le LLM **traduit le NL → un brouillon de `FeedRule` / `WatchRule`** (critères AND sur les **signaux existants** : secteur F-064, DECP F-032, BODACC F-048, sanctions F-057, changements RNE F-019, mot-clé / SIREN — F-046). Exposé via un **outil MCP de composition** (draft, pas d'écriture silencieuse).
- **Human-in-the-loop (ADR-025 §3)** : l'utilisateur **voit les conditions structurées**, édite, **valide** → la règle n'est créée qu'alors (scope `mcp:veille.ecriture` + approbation, ADR-016). **Jamais d'activation silencieuse.**
- **LLM hors boucle (ADR-025 §2)** : une fois créée, la règle tourne dans le **moteur déterministe F-046** (job Hangfire) — **pas de LLM au runtime, pas de coût par événement, rejouable / auditable**.
- **Transparence** : la règle reste **éditable** comme toute règle F-046 — pas une boîte noire.
- **Souverain (ADR-025 §4)** : inférence côté agent de l'utilisateur ; orchestration hébergée = premium opt-in.
- **Doctrine** : la règle décrit *quoi surveiller*, jamais un verdict ; les alertes restent descriptives (héritent des features sous-jacentes — `MatchCandidate` « à vérifier », etc.).
- **Accessibilité (ADR-008)** : l'aperçu de la règle composée, lisible et éditable au lecteur d'écran.
- **Hors-scope** : l'agent de sourcing (F-074) ; toute action **autonome** non validée ; des règles qui *concluent* (scoring).
