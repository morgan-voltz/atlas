# F-064 — Matching sectoriel NAF de la veille

> **Statut** : 📋 Spécifiée — non implémentée (cluster Veille, moyen terme, 31 mai 2026). Pendant *sectoriel* de **F-047** (qui matche par *mention d'entité*), cadrée par **ADR-020**. Consomme la classification native livrée par **F-063**.

**Description** : relier un item de veille réglementaire (ses **URIs EuroVoc / directory code**, fournis par F-063) aux **secteurs (NAF)** des entités que l'utilisateur suit, et l'afficher comme **« secteur *semble* concerné — à vérifier »**. C'est le matching nom-dans-texte de F-047, transposé du *nom d'entité* au *secteur*.

**Valeur user** : transforme le flux réglementaire **brut** (F-063) en signal **personnel** — « un texte touche *ton* secteur ». C'est le pendant réglementaire de l'argument différenciant de F-047 : Atlas relie l'actualité officielle au portefeuille, pas seulement la liste à plat.

**Complexité** : ★★★ (le DTO est simple ; la qualité perçue vient de la **table éditoriale** d'ADR-020, pas du code).

**APIs externes** : — (consomme les métadonnées de F-063 ; le NAF des entités vient du RNE, déjà intégré).

**Dépendances** : F-063 (source + classif native), F-047 (précédent matching + DTO timeline discriminé), F-044 (timeline), F-017 (favoris → secteurs suivis), **ADR-020** (crosswalk, `ISectorClassifier`, `MatchCandidate`, scope).

**Détails techniques** :
- **`ISectorClassifier`** (service de domaine, ADR-020) : `(URIs EuroVoc + directory code) → divisions NAF candidates + Scope`. Charge le **crosswalk de référence versionné** en mémoire (fichier repo seedé, cf. ADR-020).
- **Matcher** (`Atlas.Application`), **à côté du matcher de mentions de F-047** : intersecte les divisions candidates du texte avec les **divisions NAF des entités suivies** de l'utilisateur.
- **Sortie = `MatchCandidate`** (ADR-014), persistée façon **`FeedItemSectorMatch`** (analogue de `FeedItemFavoriteMatch`, F-047). Jamais un lien « confirmé ».
- **DTO timeline** : champ **`matchedSectors: [{ naf, label }]`**, **miroir de `mentionedFavorites`** (F-047). **Pas de nouveau `kind`** — l'item reste `RssItem`-shaped — tant qu'on ne suit pas le statut de dossier (cf. F-063 hors-scope).
- **Routage `Scope`** (anti-noyade ADR-020) : match `Sectoral` → signal « ton secteur » ; match `Horizontal` (RGPD, droit du travail, fiscalité…) → **canal « réglementaire transverse » opt-in**, jamais poussé par défaut, jamais masqué.
- **Rendu** (doc 12 §6/§14, doc 14) : carte **« réglementation · secteur semble concerné · à vérifier »**, même grammaire que **« mention détectée · à vérifier »** de F-047. **Dans la Veille (éditorial), pas l'Accueil** (1↔N sectoriel, pas 1↔1 entité).
- **Agrégation / règles** : alimente **F-058** (digest sectoriel) ; pilotable via **F-046** (« alerte-moi sur les secteurs X, Y »).
- **Doctrine** : descriptif, jamais verdict (ADR-012) ; « l'app montre ce qu'elle a compris, l'utilisateur décide » ; granularité **division** (ADR-020), jamais de fausse précision au 5-positions.
- **Ouvert (à trancher à l'implémentation)** : **seuil de confiance** d'un candidat avant affichage (cf. ADR-020, « à prévoir »).
