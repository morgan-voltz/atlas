# F-061 — Surveillance continue d'un nom

> **Figée le 30 mai 2026**. Statut **V3+ — Won't have (yet)**. **Pendant temporel de F-060**. Doctrine **ADR-014** : sortie = `MatchCandidate` « à vérifier », jamais « conflit avéré ». Jumeau structurel de **F-057** (re-screening de sanctions) : même patron snapshot/diff/job.

**Description** : surveillance continue d'un nom — soit un **nom libre** saisi (projet de marque non déposé : nouveau concept de « surveillance de nom »), soit une **entité déjà suivie** (via une `WatchRule`, patron F-046). Atlas détecte dans le temps l'**apparition** d'éléments proches : nouveaux dépôts de marques (INPI/BOPI), nouvelles dénominations (RNE), nouveaux domaines (WHOIS) — et **alerte** (timeline F-047 + push F-020) avec **piste d'audit** (date, source, élément, base du rapprochement). Là où F-060 répond « ce nom est-il pris *maintenant* ? », F-061 répond « **préviens-moi quand quelque chose de proche apparaît** ».

**Valeur user** : pour le persona Cabinet PI et les créateurs, une surveillance **multi-sources** (marque + société + domaine), **souveraine** et **descriptive** — « correspondance à vérifier », jamais « conflit avéré ».

**Complexité** : ★★★★ — l'essentiel est déjà fait ailleurs (snapshot/diff F-019, polling F-048, similarité F-026, timeline F-047, push F-020) ; le neuf : concept de « surveillance de nom libre », polling filtré par similarité, maîtrise du bruit.

**APIs externes** : celles de F-060 (INPI PI/BOPI, RNE, WHOIS/DNS).

**Dépendances** : F-060, F-026/ADR-014, F-019 (snapshot+diff+job), F-047 (timeline / `FavoriteEvent`), F-048 (polling+dédup `ExternalId`), F-046 (`WatchRule`), F-020 (push), F-017/F-053 (entités suivies).

**Pourquoi reporté (V3+)** : dépend de F-060 + évaluation WHOIS + maîtrise du bruit. Réutilise massivement l'existant.

**Détails techniques** : snapshot `NameWatchSnapshot` (par cible : nom libre ou `(UserId, entité)`, formes normalisées/hash à la F-019) ; `DiffWith(...)` (éléments apparus / disparus) ; job Hangfire `name-watch` (cron décalé, ex. `0 6 * * *`, désactivable via `BackgroundJobs:Enabled=false`) ; nouvel événement `FavoriteEvent` `NameMatchAppeared` (`ExternalId` = dépôt/dénomination/domaine + source, pour dédup) ; dédup inter-utilisateurs (un calcul de similarité par nom partagé, patron F-048) ; sortie `MatchCandidate` (« à vérifier » garanti par le type) ; audit persisté = journal défendable.

**Doctrine / RGPD / a11y** : hérite d'ADR-014 — la disposition ne peut être que « candidat / à vérifier », l'état « conflit avéré » est structurellement non représentable ; registres publics, pas de ré-identification WHOIS ; alertes lisibles au lecteur d'écran, « à vérifier » jamais porté par la seule couleur (doc 06).

**Modèle économique** : candidat **premium** (ADR-006 / ADR-009) — surveillance continue à forte valeur récurrente ; le cœur (vérification ponctuelle F-060 mono-source) peut rester open source.

**Découpage** : (1) `NameWatchSnapshot` + `DiffWith` + job `name-watch` sur une entité suivie (réutilise F-019/F-047/F-048) ; (2) surveillance d'un **nom libre** (saisie + persistance) ; (3) sources INPI/BOPI + RNE + WHOIS (selon F-060) ; (4) premium gating éventuel.
