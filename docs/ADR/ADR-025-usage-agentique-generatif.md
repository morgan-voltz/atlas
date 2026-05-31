# ADR-025 — Usage agentique génératif : composer & faire remonter, jamais conclure

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

La **surface** agentique est posée : **F-052** (serveur MCP, lecture-d'abord, auto-hébergeable, *« inférence côté agent »*), **ADR-011** (OAuth 2.1) et **ADR-016** (sécurité & doctrine de la surface : allowlist, schémas étroits, **doctrine inline**, **contenu externe = donnée jamais instruction**, délégation). ADR-016 traite la **sécurité de la surface** consommée par un agent.

Par-dessus émergent deux usages **génératifs** : des **règles de surveillance composées en langage naturel** (étend F-046) et un **agent de sourcing « thèse → cibles »** (sur F-052). Ce ne sont **pas** de nouvelles données — de l'**orchestration** de ce qui existe.

ADR-016 a flagué le risque (un agent peut aplatir « à vérifier » en « sanctionné »). Mais des features **génératives cadrées par Atlas** soulèvent des décisions **distinctes de la sécurité de la surface** : la posture doctrinale appliquée à la *génération*, où placer le LLM, le human-in-the-loop pour des artefacts *rédigés*, et la souveraineté de l'inférence.

## Décision

Cinq principes — bâtis **sur** ADR-016, sans le dupliquer.

**1. L'orchestration compose et fait remonter — elle ne conclut jamais.** Même avec un LLM, la sortie reste **descriptive** : une **règle à confirmer**, des **candidats sourcés correspondant à des critères** — **jamais** un verdict, un classement par qualité, une note, une recommandation. ADR-012 + la doctrine inline d'ADR-016 étendus à la *génération*. L'agent de sourcing remonte « des entreprises qui *correspondent* — à évaluer », jamais « les meilleures cibles ».

**2. Le LLM hors de la boucle déterministe quand c'est possible.** Là où l'usage se réduit à produire une **structure** (une règle), le LLM est dans la **composition** (une fois), pas dans l'**exécution** : le moteur déterministe (F-046, ADR-013) exécute → **pas d'hallucination au runtime, pas de coût par événement, rejouable et auditable**. Là où l'usage est intrinsèquement **exploratoire** (sourcing), le LLM est dans la boucle, mais sa sortie reste des **candidats descriptifs**.

**3. Human-in-the-loop, lecture-d'abord (ADR-016 prolongé).** L'agent **propose** (une règle, une liste) ; l'humain **confirme / agit**. Les écritures restent derrière **scopes explicites + approbation** ; **aucune mutation autonome**. Un artefact rédigé par l'agent (une règle) est **transparent et éditable** — l'utilisateur voit les conditions structurées, pas une boîte noire.

**4. Souveraineté : BYOAI sur la surface curée.** Atlas fournit la **surface d'outils** (F-052) ; l'**agent / LLM est celui de l'utilisateur** — son infra, ses credentials, **son pipeline confidentiel ne sort jamais** (*« inférence côté agent »*). Orchestration **hébergée = premium opt-in**, jamais le défaut du cœur. Cohérent ADR-006/009 et le BYOAI d'ADR-023.

**5. Anti-injection amplifiée (ADR-016 hérité).** Le contenu externe traversé (presse, décisions, **faits extraits d'actes F-070**) reste **donnée, jamais instruction** — d'autant plus critique en usage génératif. La parade structurelle reste **l'étroitesse de la surface d'écriture** : même détourné, l'agent ne peut muter sans un scope d'écriture qu'il a le droit d'appeler.

### Croquis (illustratif)

```
Règle NL (idée 1) — LLM HORS de l'exécution :
  langage naturel ──[LLM compose]──▶ brouillon de FeedRule/WatchRule structuré
        ──[humain revoit + valide]──▶ règle créée (scope écriture + approbation)
        ──[moteur F-046 DÉTERMINISTE]──▶ évaluation (zéro LLM, rejouable, auditable)

Sourcing (idée 2) — LLM dans la boucle, sortie DESCRIPTIVE :
  thèse ──[agent BYOAI traverse les outils lecture MCP]──▶
        liste de CANDIDATS { faits sourcés + critères matchés }   ← jamais de score/verdict
```

## Rationale

- **Bâtit sur ADR-016** (sécurité de la surface) en ajoutant la doctrine du **produit agentique génératif**.
- **« Compose, jamais conclut »** = la seule façon de garder un LLM dans le produit sans trahir « descriptif, jamais verdict ».
- **LLM hors boucle** = robustesse + coût nul + auditabilité — le même réflexe que le **replay déterministe** de la machine à remonter le temps.
- **BYOAI souverain** = différenciation + respect du pipeline confidentiel et du secret professionnel.

## Conséquences

- **Positives** : déverrouille l'agentique à fort levier **sans coût par utilisateur ni trahison doctrinale** ; réutilise F-046 + F-052 + la sécurité d'ADR-016 ; différenciation souveraine.
- **Négatives** : la frontière **« candidat vs verdict »** dans une sortie *générée* demande de la discipline (prompt + post-traitement + tests) ; un artefact rédigé doit rester **éditable** (UX) ; le BYOAI rend la qualité tributaire de l'agent de l'utilisateur (assumé).
- **À prévoir** :
  - **F-073** (règles NL) et **F-074** (agent de sourcing) implémentent.
  - **doc 08** : nommer la composition NL→règle et le pattern de sourcing (sortie candidate).
  - **Frontière premium** (orchestration hébergée) = à n'arbitrer qu'en phase 3-4 (comme F-052).
  - **Composition inter-produits** (Atlas ↔ Chronos, « alvéoles d'une ruche ») : la surface agentique en est le **mécanisme**, mais elle relève d'un **futur ADR dédié** (bounded context + anti-corruption layer ; la donnée *se référence*, ne *déménage* pas) — **hors** de cet ADR.
  - **Références croisées** : **F-073 / F-074** ; **F-046** (moteur de règles), **F-052 / ADR-011 / ADR-016** (surface + sécurité), **F-064 / F-054 / F-032 / F-072 / F-067 / F-056** (outils traversés par le sourcing), **F-070** (faits extraits = donnée), **ADR-006 / 009 / 012 / 023** ; **doc 08**.
