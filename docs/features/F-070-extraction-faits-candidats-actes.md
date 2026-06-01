# F-070 — Extraction de faits candidats des actes

> **Statut** : 📋 Spécifiée — non implémentée (cluster Document intelligence, 31 mai 2026). Par-dessus **F-069** ; la vraie « intelligence », **conservatrice**. Cadrée par **ADR-023**.

**Description** : extraire des **faits candidats structurés** des actes (cessions de parts, changements de capital, changements de dirigeants), **ancrés à la page**, présentés comme **« semble … — à vérifier »** — jamais assénés. Une aide à la lecture qui *amène* à la pièce primaire.

**Valeur user** : pour le **M&A / avocat** (due diligence : cessions, capital) et l'**expert-comptable** — le **détail que RNE/BODACC ne donnent pas**, adossé à la pièce primaire. Et ces faits événementiels **enrichissent la trajectoire** (F-067) et l'**export auditable** (F-066).

**Complexité** : ★★★★ (extraction de texte juridique + ancrage page + conservatisme — le morceau dur de la piste).

**APIs externes** : — par défaut (**modèle local**, ADR-023). Option premium : **cloud / BYOAI opt-in**.

**Dépendances** : F-069 (index / OCR / texte), **ADR-023**, ADR-014 (`MatchCandidate`), F-065 / F-066 (provenance / audit à la page), F-067 (trajectoire). Patron premium F-050.

**Détails techniques** :
- **Extraction locale par défaut** → `ExtractedFactCandidate` `{ Kind, Value, SourceDocument, PageRef, Confidence }`. Chemin **cloud / BYOAI opt-in** premium (ADR-023 §2) — jamais le défaut.
- **Sortie = candidat à vérifier** (ADR-014), **ancré à la page** (document + page) — généralise F-065. **Jamais** un fait asséné.
- **Faits événementiels → trajectoire** : un fait qui *est* un événement (cession dans un PV daté du X) → **`EntityChangeLogEntry`** (F-067) avec `EventTime` = date de l'acte, adossé à la pièce primaire.
- **Audit (F-066)** : les faits extraits entrent dans l'export avec provenance **à la page** — la trace la plus défendable.
- **Conservatisme (ADR-014)** : **dans le doute, pas de fait** (un faux positif juridique coûte cher) ; `Confidence` affichée ; toujours « à vérifier ».
- **Souverain (ADR-023)** : documents **jamais chez un tiers** dans le cœur ; compute **on-demand** (entités suivies), cache.
- **Doctrine / accessibilité** : candidat formulé « semble … à vérifier », jamais porté par la **couleur seule** (doc 06) ; descriptif, aucun verdict (ADR-012).
- **Hors-scope** : extraction des **bilans** (F-054) ; toute **qualification juridique / verdict** ; **v1 = entités légales** (même prudence que F-057 sur les personnes physiques).
