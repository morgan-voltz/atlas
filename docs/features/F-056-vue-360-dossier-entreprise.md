# F-056 — Vue 360 / Dossier entreprise

> **Méta-feature d'assemblage figée le 29 mai 2026**. Statut **V3+** (sections sous-jacentes en V2 ou V3+). Ce **n'est pas une nouvelle source de données** : c'est une **couche de composition** par-dessus l'existant. Backbone architectural : **ADR-015** (sections auto-descriptives + résolution snapshot-first). Issue du persona Investisseur / M&A — sert aussi expert-comptable, avocat, compliance.

**Description** : une **vue consolidée d'une entreprise** qui assemble, en un seul dossier descriptif, sourcé et daté, les données déjà servies par les features par-source — identité & dirigeants (F-004), événements légaux (F-047/F-048), indicateurs financiers (F-054), marchés publics (F-032), propriété industrielle (F-018/F-025), cotation (F-051), structure de co-mandats (F-034, sous ADR-012), signaux de risque (F-055). Chaque élément reste **tracé à sa source et à sa date** — c'est la matérialisation concrète de la thèse du produit : *croiser ce que personne ne croise*.

**Valeur user** : aujourd'hui, comprendre une entreprise oblige à consulter chaque dimension séparément. Le dossier 360 fait ce croisement **à la place de l'utilisateur** : l'investisseur obtient un portrait de cible, l'expert-comptable un état de client, l'avocat un socle de due diligence — **le même assemblage, des lentilles différentes**. C'est aussi le **cas d'usage roi de l'accès agentique** : un agent monte le dossier via le serveur MCP (F-052, outil `get_company_dossier`).

**Complexité** : ★★★★ — méta-feature : peu de code « neuf » au sens données, mais une vraie ingénierie de **composition, de cache et de dégradation**, plus l'exposition MCP et la synthèse premium.

**APIs externes** : **aucune en propre**. Elle réutilise les sources des features composées (RNE, BCE/INPI, INPI PI, DECP, BODACC, DG Trésor/UE, GLEIF). C'est tout l'intérêt.

**Dépendances** : F-004 (identité), F-047/F-048 (événements/BODACC), F-019 (snapshot — patron d'**ADR-013**), **F-054** (finances), **F-032** (marchés), **F-018/F-025** (PI), **F-051** (cotation), **F-034** (structure, sous ADR-012), **F-055** (risque), **F-052** (exposition MCP, sous **ADR-016**). Backbone architectural : **ADR-015** (sections auto-descriptives + 5 états + résolution snapshot-first).

**Hors-périmètre (explicite)** :
- **Aucune nouvelle source** : si une donnée n'est pas déjà servie par une feature, elle n'apparaît pas ici.
- **Aucune synthèse-verdict** : pas de note, de valorisation, de score de risque ni de recommandation — le dossier **assemble des faits**, il ne juge pas.
- Pas de comparaison multi-entreprises ni de portefeuille (F-053 le couvre).

**Détails techniques** :
- **Read-model de composition côté Application** (ADR-015) : un `CompanyDossier` composé de `DossierSection` indépendantes (`IdentitySection`, `FinancialsSection`, `PublicContractsSection`, `IpSection`, `ListingSection`, `StructureSection`, `RiskSection`, `EventsSection`). **Pas un nouvel agrégat de domaine** — `Company`/`UniteLegale` restent les agrégats.
- **5 états par section** (`SectionState`, ADR-015) : `Available` / `Stale` / `Unavailable` / `NotApplicable` / `Restricted` — l'**absence est honnête** (la section qui échoue ou expire tombe en `Unavailable`, jamais en succès silencieux).
- **Composition extensible** : chaque section est produite par un use case existant ; ajouter une section = brancher un use case, sans toucher au cœur (hexagonal, ADR-004). Les sections **s'allument** au fil de la disponibilité des features.
- **Assemblage hybride snapshot-first** (ADR-015 + ADR-013) :
  - sources **légères / ouvertes** (BODACC, DECP, sanctions) : en direct ou cache court ;
  - appels **INPI coûteux** (identité, bilans, PI) : pour une entreprise **suivie**, le dossier **lit les snapshots** du substrat de surveillance (ADR-013) — quasi pré-assemblé et rapide ; pour une entreprise **non suivie**, assemblage **à la demande** avec cache court.
- **Résilience par section** : timeout par section ; tout échec → `Unavailable` (jamais d'exception qui casse le dossier).
- **Endpoints (esquisse)** : `GET /companies/{siren}/dossier` (réponse composée v1) ; MCP `get_company_dossier` (lecture seule, ADR-016).

**Cadre légal** : le dossier **n'introduit aucun traitement nouveau** — il compose des données déjà encadrées par leurs features respectives. Il **hérite** de leurs garde-fous. Section **structure (F-034)** sous **ADR-012** (jamais de BE) ; section **finances (F-054)** respecte la **confidentialité des comptes** ; posture d'ensemble : **dossier de faits**, pas d'avis.

**Accessibilité (ADR-008, bloquant)** : structure sémantique claire (titres de sections, navigation), chaque section autonome et lisible au lecteur d'écran, **équivalent tabulaire** de la section structure (réutilisé de F-034). L'UI doit rendre la **fraîcheur par section** et les **5 états** honnêtement (jamais afficher `Unavailable` / `Restricted` comme « rien à signaler »).

**Modèle économique (ADR-006)** :
- **Cœur / OSS** : l'assemblage déterministe des sections sourcées. Aucun coût d'inférence → gratuit.
- **Premium** : **synthèse narrative IA** optionnelle du dossier (port `IDossierSummarizer`, patron F-050) — **descriptive**, sans verdict ni valorisation.

**Découpage / jalons** :
1. **Squelette de composition** : `CompanyDossier` + contrat de section + dégradation propre, sur les sections **déjà bâties** (identité, événements).
2. **Branchements progressifs** : finances, marchés, PI, risque, cotation, structure — chaque section ajoutée quand sa feature est prête.
3. **Assemblage hybride** : intégration au snapshot ADR-013 pour les entreprises suivies + cache court pour les autres.
4. **Exposition MCP** (F-052) : outil `get_company_dossier` en lecture seule, sous ADR-016.
5. **Premium** : synthèse IA descriptive optionnelle (F-050).

**Décisions ouvertes** :
- **Périmètre de la synthèse premium** : par section ou globale ?
- **Fraîcheur affichée** : date « as of » par section (recommandé).
- **Entreprise non suivie** : profondeur de l'assemblage à la demande.
- **Nom canonique** : `CompanyDossier` / « Dossier entreprise 360 » — à arrêter pour le doc 08.
