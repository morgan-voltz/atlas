# F-060 — Vérificateur de présence d'un nom (multi-sources)

> **Figée le 30 mai 2026**. Statut **V3+ — Won't have (yet)**. Doctrine **ADR-012 / ADR-014** : Atlas rapporte des faits **sourcés et datés**, **jamais un verdict de disponibilité**.

**Description** : depuis la Recherche, un mode « Vérifier un nom » (3ᵉ intention de recherche — pas de 6ᵉ onglet) où l'utilisateur saisit un nom (marque, enseigne, dénomination envisagée) et obtient un **rapport multi-sources** : marques (INPI PI), dénominations d'entreprises (RNE), domaines web (WHOIS/DNS) — chacun avec sa provenance, sa date et son périmètre — plus une liste de **noms proches à vérifier**. Ne rend **jamais** de verdict (« disponible », « libre ») : toute conclusion passe par le contrat `MatchCandidate` (ADR-014), dont la disposition ne peut être que « à vérifier ».

**Valeur user** : les outils existants ne couvrent qu'une source (checker de domaine isolé) ou affichent un verdict irresponsable. Atlas agrège plusieurs registres et reste descriptif — utile avant un dépôt de marque ou une création d'entreprise, sans se substituer au conseil PI.

**Complexité** : ★★★.

**APIs externes** : INPI PI (déjà intégrée, F-006), INPI RNE (déjà intégrée, F-004/F-005), **WHOIS/DNS (nouvelle source à évaluer)**.

**Dépendances** : F-006, F-005, F-004, F-026/ADR-014 (`ITrademarkSimilarityMatcher` + noyau de normalisation de noms), F-061 (pendant temporel).

**Pourquoi reporté (V3+)** : (1) **source externe nouvelle** (WHOIS/DNS) hors du socle INPI/INSEE/BODACC — à évaluer (fournisseur, coût, rate-limits) ; (2) **sensibilité juridique maximale** — l'écran où un mauvais cadrage ferait le plus de dégâts, à livrer avec rigueur doctrinale. À reconsidérer selon la traction (persona Cabinet PI).

**Détails techniques** : port `INameAvailabilityProbe` (`Atlas.Domain`, renvoie un agrégat **sourcé**, jamais un booléen) ; adapters `InpiTrademarkPresenceAdapter` (F-006) / `RneDenominationPresenceAdapter` (F-005) / **`WhoisDomainAdapter`** (nouveau, `Atlas.Infrastructure.Whois`) ; use case `CheckNamePresence` (requêtes parallèles bornées, DTO **discriminé par source**, aucune fusion en indicateur unique) ; sortie `MatchCandidate` (ADR-014) par élément ; endpoints esquissés `GET /names/check?q=` et `GET /names/similar?q=`.

**Doctrine (garde-fous)** : faits, jamais verdict (« disponible »/« libre »/« vous pouvez déposer » interdits) ; sources séparées, jamais fusionnées ; similarité = suggestion à vérifier, jamais affirmation de risque de confusion ; couleurs neutres ; note de cadrage permanente (« Atlas rapporte ce qu'il a trouvé, sourcé et daté ; il ne conclut pas sur la disponibilité juridique d'un nom — cette appréciation relève du conseil en PI »). Accessibilité ADR-008 / doc 06 : chaque source = section avec provenance, « à vérifier » jamais porté par la seule couleur.

**RGPD** : registres publics (pas de données personnelles au-delà de ce que F-005/F-006 traitent déjà) ; WHOIS = uniquement le public (statut d'enregistrement, dates), aucune ré-identification des titulaires.

**Modèle économique** : candidat **premium** (ADR-006 / ADR-009) — vérification multi-sources à forte valeur pour les cabinets ; le cœur (recherche mono-source) reste open source. À arbitrer.

**Découpage** : (1) port + use case `CheckNamePresence` agrégeant F-005/F-006 (sans domaines) ; (2) mode UI « Vérifier un nom » (doc 12 §7) + rapport sourcé + noms proches (réutilise F-026) ; (3) adapter WHOIS/DNS (`Atlas.Infrastructure.Whois`) ; (4) premium gating éventuel.

**À l'intégration** : référencer la source WHOIS dans `docs/03-catalogue-apis-publiques.md` ; à la création d'`Atlas.Infrastructure.Whois`, ajouter sa règle de dépendance (CLAUDE.md) et ses tests d'archi.
