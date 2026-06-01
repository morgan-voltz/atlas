# F-074 — Agent de sourcing souverain (« thèse → cibles »)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Surfaces agentiques, 31 mai 2026). Le **différenciant génératif** ; sur **F-052** ; **BYOAI** ; candidats, **jamais verdicts**. Cadrée par **ADR-025**.

**Description** : l'utilisateur décrit une **thèse** (secteur, taille, dynamique, critères) ; un agent **traverse les outils MCP** d'Atlas (matching sectoriel F-064, finances F-054, DECP F-032, graphe F-072, trajectoire F-067, dossier F-056) et **fait remonter des entreprises candidates** correspondant aux critères — chacune portant ses **faits sourcés** et les **critères qu'elle matche**. Jamais un classement par qualité ni une recommandation.

**Valeur user** : le besoin **nommé** du persona **investisseur** (sourcing de cibles sur la PME française — là où PitchBook & co. sont faibles) et de la **veille concurrentielle**. **Souverain** : la thèse (pipeline confidentiel) et la traversée restent **chez l'utilisateur**.

**Complexité** : ★★★★ (orchestration générative + évitement-de-verdict rigoureux + qualité de couverture).

**APIs externes** : — en propre (réexpose les outils existants). Inférence **côté agent de l'utilisateur** (BYOAI).

**Dépendances** : **F-052 / ADR-011 / ADR-016** (surface MCP + sécurité), **ADR-025**, **ADR-014** (candidats), et les outils traversés : **F-064 / F-054 / F-032 / F-072 / F-067 / F-056**.

**Détails techniques** :
- **BYOAI sur la surface curée (ADR-025 §4)** : l'agent est celui de l'utilisateur (Claude ou autre), branché sur le **MCP souverain** ; **pipeline jamais sorti** (*« inférence côté agent »*).
- **Traversée des outils lecture** (F-052, scope `mcp:lecture-base`) : matching sectoriel (F-064), finances (F-054), DECP (F-032), graphe (F-072), trajectoire (F-067), dossier 360 (`get_company_dossier`, F-056).
- **Sortie = candidats descriptifs (ADR-025 §1)** : « entreprises qui *correspondent* aux critères de ta thèse — à évaluer », chacune avec ses **faits sourcés** et les **critères matchés**. **Jamais** un score de qualité, un classement, une recommandation (ADR-012 ; le persona l'exige explicitement).
- **Doctrine inline (ADR-016)** : `MatchCandidate` / `SectionState` **jamais aplatis** ; **contenu externe = donnée jamais instruction** (presse, faits extraits F-070).
- **Human-in-the-loop** : l'agent **propose une liste à explorer** ; aucune action (favori, watchlist) sans **scope d'écriture + validation**.
- **Accessibilité (ADR-008)** : liste de candidats + critères + provenance en **équivalent tabulaire** lisible au lecteur d'écran.
- **Modèle économique** : inférence côté agent → **aucun coût par utilisateur** côté Atlas ; **cœur OSS**. Orchestration hébergée éventuelle = **premium opt-in** (ADR-006).
- **Hors-scope** : tout **conseil / verdict de valorisation / recommandation** ; les **bénéficiaires effectifs** (Sovim — gap honnête) ; l'exécution **autonome** ; les déposants individuels via le graphe (régime F-034).
