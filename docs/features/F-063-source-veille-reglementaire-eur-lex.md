# F-063 — Source de veille réglementaire EUR-Lex

> **Statut** : 📋 Spécifiée — non implémentée (cluster Veille, moyen terme, 31 mai 2026). Pendant *réglementaire* de **F-041**, cadrée par **ADR-020** (le matching → NAF est porté par **F-064**, pas ici). Donnée brute déjà exploitable seule, avant tout matching.

**Description** : nouvelle source de veille = les **textes réglementaires de l'UE** (EUR-Lex / dépôt Cellar), ingérés **comme un flux de plus** dans le moteur d'agrégation (F-041), puis **enrichis de leurs métadonnées structurées** (numéro CELEX, descripteurs EuroVoc, *directory code*, dates). La source **ne traduit pas** en NAF — elle livre la classification *native* ; le rattachement sectoriel est le job de F-064.

**Valeur user** : amène le **réglementaire UE** dans la Veille. A une valeur **seule** (lire le flux réglementaire brut, ou filtré par domaine), et constitue la **fondation** de l'exposition réglementaire sectorielle (avec F-064). Premier pas vers « ton secteur bouge au niveau européen ».

**Complexité** : ★★★★ (sans authentification, mais Atom + SPARQL + pièges consolidation/langues).

**APIs externes** : EUR-Lex / **Cellar** — flux **Atom** (nouvelles publications) + **SPARQL** (`https://publications.europa.eu/webapi/rdf/sparql`, modèle **CDM**). Licence ouverte, sans inscription.

**Dépendances** : F-041 (moteur d'agrégation — pattern `IExternalContentSource`, polling Hangfire, dédup), F-001. Hérite **ADR-018** (dégradation) et **ADR-013** (stratégie flux append-only).

**Détails techniques** :
- Adapter sortant `EurLexFeedProvider` (`Atlas.Infrastructure`), façon `IExternalContentSource` ; **stratégie flux append-only** (ADR-013), **dédup par CELEX** (`ExternalId`).
- **Ingestion en deux temps** :
  1. *Découverte* — poller le **flux Atom Cellar** (cron ~15-30 min via Hangfire) → extraire l'**URI Cellar** de chaque entrée → dédup contre le déjà-vu.
  2. *Enrichissement* — pour chaque URI neuve, **SPARQL ciblé** pour récupérer CELEX, date, **URIs EuroVoc**, **directory code**, titre.
- **Prédicats CDM** (préfixe `http://publications.europa.eu/ontology/cdm#`) : `cdm:resource_legal_id_celex` (clé de dédup), `cdm:work_date_document`, `cdm:work_is_about_concept_eurovoc` (→ concept EuroVoc, URI stable + `skos:prefLabel`). Le *directory code* (filtre `directory` du package de référence `eurlex`) sert d'**épine dorsale** du futur matching ; EuroVoc en raffinement. Prédicat exact du directory code **à confirmer dans l'éditeur SPARQL** (`op.europa.eu/.../advanced-sparql-query-editor`).
- **Filtres obligatoires** : langue (`FILTER(lang = "fr")`) sinon ×24 (24 expressions linguistiques par work) ; exclure `cdm:do_not_index "true"`.
- **Piège consolidation** : 1 règlement logique = N CELEX (consolidations, rectificatifs, amendements). Regrouper via `cdm:work_related_to` / `cdm:consolidated_by` et **réutiliser la dédup F-045** (« N sources rapportent ») pour ne pas notifier 12× le même texte.
- **Sortie normalisée** : un item portant **URIs EuroVoc + directory code + CELEX** (classif *native*, **pas de NAF**). Le mapping → NAF est F-064.
- **Dégradation (ADR-018)** : Cellar/SPARQL indisponible = **erreur locale** dans la Veille (code dédié type `eurlex.unavailable`), jamais « 0 texte » trompeur.
- **Doctrine** : descriptif, daté, sourcé (ADR-012) ; le texte est de la **donnée**, jamais une consigne.
- **Hors-scope (volontaire)** : suivi du **statut de dossier** (en négociation → adopté → en vigueur) — demanderait un monitor **clé-par-dossier** absent du substrat (ADR-013) ; feature ultérieure. *Bulk* RDF hebdomadaire (triplestore maison) : hors MVP.
