# F-072 — Arête « co-dépôt PI » du graphe (co-déposants)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Graphe d'écosystème, 31 mai 2026). L'**angle différenciant** ; le morceau dur = la **résolution déposant → SIREN**. Cadrée par **ADR-024**, sur le substrat **F-034**.

**Description** : ajouter au graphe une **arête « co-dépôt »** reliant deux **entreprises** qui ont **co-déposé** un brevet ou une marque (INPI PI) — un signal de **partenariat R&D** que personne ne calcule sur le tissu des PME françaises. **v1 = co-déposants personnes morales.**

**Valeur user** : révèle **qui innove avec qui** — partenariats, alliances technologiques, liens R&D invisibles ailleurs. Personas **M&A** (alliances, actifs immatériels) et **veille concurrentielle** (alliances dans mon secteur). C'est l'**angle mort différenciant** d'Atlas.

**Complexité** : ★★★★ (résolution déposant **nominatif** → SIREN, conservatrice — le morceau dur de la piste).

**APIs externes** : **INPI PI** (déposants des brevets / marques — F-006 / F-016, **nominatif**).

**Dépendances** : **F-034** (substrat graphe), **F-006 / F-016** (PI, déposants), **ADR-024**, **ADR-014** (matching conservateur), ADR-008.

**Détails techniques** :
- Nouvelle **arête typée `CoFiling`** (Entreprise ↔ Entreprise), dérivée des titres PI ayant **≥ 2 déposants**.
- **Résolution déposant → SIREN conservatrice (ADR-014 — le crux)** : le déposant INPI PI est **nominatif** (F-016, SolR `[DENM=...]`), **pas en SIREN** → matching **nom → SIREN** avec **confiance élevée requise** ; **dans le doute, pas d'arête**. *Pas de lien vaut mieux qu'un lien erroné* (F-034). Couverture **partielle** assumée et affichée honnêtement.
- **v1 = co-déposants personnes morales** (ADR-024 §3) : déposants **individuels** = **reportés sous le régime F-034** (DPIA) — hors de cette fiche.
- Exposée dans la **traversée bornée** de F-034 ; **hub-aware** (un gros déposant institutionnel = hub à filtrer, ADR-024 §5).
- **Qualification descriptive** : « co-déposants d'un brevet / d'une marque ({année}) » — **jamais** « partenaires ». Provenance INPI + **n° de dépôt** + date.
- **Légalement léger** pour les co-déposants personnes morales (ADR-024 §3).
- **Accessibilité (ADR-008)** : équivalent **tabulaire** (« A a co-déposé avec B, C »), aucune information par la couleur ou la position seule.
- **Hors-scope** : déposants **individuels** (régime F-034) ; toute **inférence de la nature du lien** (verdict) ; l'arête adresse partagée.
