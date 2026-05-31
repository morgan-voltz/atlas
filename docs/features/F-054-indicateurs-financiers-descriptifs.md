# F-054 — Indicateurs financiers descriptifs

**Description** : sur la fiche d'une entreprise, Atlas affiche des **indicateurs financiers descriptifs** issus de ses comptes annuels — ratios (rentabilité, autonomie financière, solvabilité, liquidité) et surtout leur **évolution dans le temps**. Tout est factuel, calculé de façon transparente, jamais agrégé en une note unique. **Ligne strictement descriptive** : pas de note / score / cotation propriétaire qui mimerait une notation de crédit ou prédirait un défaut (la cotation Banque de France FIBEN, confidentielle, tient ce terrain réglementé). Nommage volontaire « indicateurs **descriptifs** », pas « scoring » ni « notation » : Atlas affiche des faits, pas un jugement de solvabilité.

**Valeur user** : pour les personas **expert-comptable** et **investisseur**, voir d'un coup d'œil la trajectoire financière d'une boîte (et de ses concurrents / partenaires suivis), sans ouvrir et déchiffrer des PDF de comptes. Combiné aux watchlists (F-053) et à la veille (F-047), ça complète la vue « identité + PI + veille + finances » dans un seul outil.

**Complexité** : ★★★ à ★★★★ selon la profondeur (tendances, postes bruts) — phasable. Le travail n'est ni l'ingestion ni le calcul (déterministes), mais la **réconciliation et la provenance** entre les deux sources : c'est là que se joue la qualité perçue.

**APIs externes** :
- **Jeu open data « Ratios Financiers BCE/INPI »** (data.gouv.fr / data.economie.gouv.fr) : ratios pré-calculés par la DNUM à partir des données RNE de l'INPI, par SIREN et date de clôture. Gratuit, en masse.
- **API INPI comptes annuels** (« bilans-saisis ») : postes financiers structurés extraits par l'INPI de ses propres PDF, récupérables par identifiant. Gratuit.

Aucun parsing de PDF nécessaire dans les deux cas.

**Dépendances** : F-013 (`FinancialStatement`, téléchargement des bilans), F-017 / F-053 (favoris & watchlists — cible de la couche « profondeur »), patron des ports premium de F-050, ADR-004 (archi hexagonale), ADR-006 (open core).

**Détails techniques** :
- **Deux sources en couches, rôles distincts et complémentaires** (jamais le même chiffre produit deux fois) :
  - **Couche « largeur » — Ratios BCE/INPI (baseline, instantané)** : ingérée en masse (job périodique `bce-ratios-ingest`). N'importe quelle entreprise ouverte affiche ses ratios immédiatement, sans appel externe. Avantage de fond : ce sont des ratios **calculés par le ministère selon une méthodologie publiée** — afficher de l'open data officiel renforce la ligne « descriptif, pas notation » (on ne fabrique pas de note maison).
  - **Couche « profondeur / fraîcheur » — Bilans-saisis INPI (à la demande)** : pour une entreprise réellement suivie (favori / watchlist), on récupère les postes du bilan en clair. Trois gains que la BCE seule ne donne pas : afficher les **postes bruts** derrière les ratios, capter le **dépôt le plus récent** avant son entrée dans le prochain millésime BCE, calculer des **ratios ou tendances** absents du jeu BCE.
- **Principe directeur** : une seule source de vérité par chiffre + **provenance toujours étiquetée** (« ratio open data BCE, millésime 2024 » vs « calculé sur le bilan déposé le 14/03/2025 »). Évite le chiffre orphelin contradictoire qui détruirait la confiance. Cohérent avec l'ADN de transparence / souveraineté du projet.
- **Résilience** : deux sources évitent d'être l'otage d'une seule (si le jeu BCE change de cadence, les bilans-saisis prennent le relais ; si l'API comptes annuels est lente, la BCE assure le fond).
- **Types de bilan C / K / S** (complet / simplifié / consolidé) : les formules de ratios en dépendent. À gérer explicitement côté calcul.
- **Modèle & ports** :
  - Réutilise `FinancialStatement` (doc 08 : `FiscalYear`, `ClosureDate`, `FilingDate`, `IsConfidential`, `DocumentUrl`).
  - Nouveau : `FinancialIndicatorSet` (par `Siren` + `ClosureDate` + `Source` + provenance), porteur des ratios.
  - Port `IFinancialDataProvider` (adapter `InpiBilanSaisiProvider`) pour la couche profondeur ; job Hangfire `bce-ratios-ingest` pour la couche largeur.
  - Service de domaine déterministe `FinancialIndicatorService` (calcul des ratios). **Pas d'IA** → cœur libre.
- **Endpoints (esquisse, à confirmer)** :
  - `GET /companies/{siren}/financials` — ratios baseline (BCE) + statut de disponibilité / confidentialité.
  - `GET /companies/{siren}/financials/detail` — postes bruts + ratios + tendances (bilans-saisis, à la demande).
  - `GET /companies/{siren}/financials/analysis` — **(Premium)** synthèse narrative générée par IA.

**Cadre légal & positionnement** : la **cotation Banque de France** (FIBEN) est la référence officielle d'appréciation de la solvabilité — une note avec probabilité de défaut, établie à dire d'expert, **confidentielle**, réservée à l'entreprise notée et aux acteurs du crédit. C'est du terrain réglementé. Atlas reste **strictement descriptif** : ratios factuels et leurs tendances, calculés de façon transparente à partir de données publiques. Jamais une note / score propriétaire qui mimerait une notation de crédit. Les quatre axes de la Banque de France (rentabilité, autonomie financière, solvabilité, liquidité) servent de **catégories de ratios descriptifs**, jamais d'ingrédients d'un verdict agrégé. Même philosophie que F-051 (suivi, pas conseil).

**Confidentialité & couverture** : seuls les comptes **non confidentiels déposés depuis 2017** sont exploitables (micro-entreprises : confidentialité totale possible ; petites entreprises : compte de résultat confidentiable ; moyennes / grandes : pas de confidentialité). Le décret de février 2024 (directive UE 2023/2775) a relevé les seuils → davantage de PME peuvent opter pour la confidentialité. Conséquence : couverture **partielle**, à afficher honnêtement via le champ `IsConfidential` (« comptes non disponibles » plutôt qu'un vide trompeur).

**RGPD** : données d'entreprises issues de l'open data public → pas de traitement de données personnelles spécifique. Rien de sensible à stocker côté utilisateur (contrairement à F-051, pas de clé à protéger).

**Accessibilité (rappel ADR-008)** : une **tendance** (hausse / baisse) n'est jamais signalée par la couleur seule — toujours doublée d'un signe (`+` / `−`), d'une flèche ou d'un libellé. Tableaux de ratios lisibles au lecteur d'écran ; provenance annoncée.

**Modèle économique** : calcul de ratios = déterministe → **cœur open source**, gratuit (aucun coût par utilisateur). Synthèse narrative IA = **premium** (coût LLM), via un port `IFinancialSummarizer` analogue à `IFeedSummarizer` (patron F-050). Aligné ADR-006 (« monétiser la commodité, pas le cœur »).

**Découpage / jalons** :
1. **Couche largeur** : ingestion du jeu BCE/INPI (job `bce-ratios-ingest`), endpoint `/financials`, ratios + statut confidentialité. *(Livrable autonome, couvre déjà l'essentiel.)*
2. **Couche profondeur** : `InpiBilanSaisiProvider`, postes bruts + tendances pour les entreprises suivies, endpoint `/financials/detail`, avec provenance étiquetée.
3. **Réconciliation** : règles de source-de-vérité-par-chiffre, gestion des types de bilan C / K / S.
4. **(Premium)** synthèse narrative IA via `IFinancialSummarizer`.

**Décisions ouvertes** :
- **Ordre de livraison** des deux couches (recommandé : largeur d'abord — autonome et utile seule).
- **Profondeur de l'historique** affiché (3 ans ? 5 ans ?).
- **Cadence** d'ingestion du jeu BCE (suivre les millésimes publiés).
- **Liste exacte des ratios** retenus par axe (rentabilité / autonomie / solvabilité / liquidité), et leur définition documentée (transparence).
- Documenter publiquement les **formules de ratios** utilisées (gage de transparence et de la ligne « descriptif »).

---

**Grappe 2 — Organisation & capitalisation** *(F-053, F-030, F-029 — l'utilisateur range et emporte sa connaissance personnelle au-dessus des données publiques)*
