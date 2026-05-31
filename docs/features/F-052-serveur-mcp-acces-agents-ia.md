# F-052 — Serveur MCP (accès agents IA)

**Description** : Atlas expose ses capacités sous forme de **serveur MCP** (Model Context Protocol), pour que des **agents IA** (Claude, et tout client compatible) puissent interroger les données et la veille au nom de l'utilisateur (rechercher une entreprise, lire une fiche, consulter les marques, gérer les favoris, interroger la timeline). L'agent n'a jamais plus de droits que l'utilisateur.

**Valeur user** : pour les power users et les profils « travaillant avec l'IA », automatiser des workflows de suivi en langage naturel ; pour les intégrateurs, brancher Atlas dans leurs propres agents via une interface standard. **Différenciation** : un MCP **souverain et auto-hébergeable** est unique sur le marché français des données d'entreprise (Pappers expose déjà un MCP mais sans cet angle).

**Complexité** : ★★★★ (lecture + écriture encadrée) ; ★★★★★ avec couche OAuth 2.1 complète. L'essentiel de l'effort est l'**auth/scoping** et le **durcissement sécurité**, pas la définition des outils (le SDK la rend triviale).

**APIs externes** : aucune nouvelle source (réexpose les capacités existantes). Dépendance technique : SDK MCP officiel C# (`ModelContextProtocol`, `ModelContextProtocol.AspNetCore`), maintenu par Microsoft et Anthropic.

**Dépendances** : use cases existants (F-004/F-005, fiche entreprise, F-006/F-007, F-017, F-041→F-047, F-042) ; ADR-003 (coffre de credentials) ; ADR-004 (archi hexagonale) ; **ADR-010 (auth utilisateur) + ADR-011 (OAuth 2.1 pour la délégation)**.

**Détails techniques** :
- **Nouvel adapter entrant** par-dessus les use cases MediatR existants (exactement comme `Atlas.Api`). Projet `Atlas.Mcp` (ou module dans l'API) qui traduit des appels d'outils MCP en commandes/queries du domaine. **Zéro modification du domaine** (ADR-004).
- Le SDK C# déclare un outil en décorant une méthode (`[McpServerTool]`) ; le schéma JSON est généré automatiquement. Intégration : `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` + `MapMcp()`.
- **Outils lecture (V1)** : `search_companies`, `get_company`, `search_trademarks`, `get_trademark`, `list_favorites`, `get_veille_timeline`, `list_veille_packs`.
- **Outils écriture (encadrés, V2 du chantier)** : `add_favorite` / `remove_favorite`, `subscribe_feed_source` / `apply_veille_pack` — derrière des **approval workflows** et des **scopes** dédiés.
- **Transports** : stdio (auto-hébergement local) et HTTP/Streamable HTTP (instance distante).
- **Aucune exposition** de la gestion des secrets (credentials INPI, clés) via MCP. Aucune opération destructrice non réversible exposée sans garde forte.

**Sécurité (cf. ADR-011 + ADR-016)** : OAuth 2.1 + PKCE ; **scopes** distincts lecture vs écriture ; tokens courts + refresh rotatif révocable ; audit logging de chaque appel d'outil (Serilog déjà en place) ; rate limiting réutilisé (global + auth-strict). **Doctrine et architecture de la surface agentique formalisées par ADR-016** : surface curée lecture-d'abord (allowlist, pas d'exposition 1:1, schémas étroits), doctrine inline avec donnée (`MatchCandidate`/`SectionState` préservés intacts, jamais aplatis dans le mapping MCP), contenu externe = donnée jamais instruction (parade injection prompt indirecte), délégation utilisateur (credentials ne traversent jamais). Outil **`get_company_dossier`** ajouté pour F-056 (dossier 360 en lecture seule, descriptif).

**Modèle économique** : aucun coût par utilisateur côté Atlas (l'inférence est côté agent). Reste dans le **cœur open source** (cohérent ADR-006). Une frontière premium éventuelle (quotas en hébergé, outils agentiques avancés) pourra être posée plus tard sans toucher au socle.

**Décisions ouvertes** :
- Périmètre exact des outils d'écriture exposés en V1 (probable : favoris + abonnements ; exclure tout ce qui touche aux secrets et au compte).
- Exposition : self-host (stdio/HTTP local) d'abord, instance hébergée ensuite ? Politique de quotas en hébergé.
- Frontière premium éventuelle (à n'arbitrer qu'en phase 3-4).

---

## V3+ — Won't have (yet)

Features identifiées comme valables mais explicitement reportées hors du périmètre actuel. À reconsidérer en fonction de la traction.
