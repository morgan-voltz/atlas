# F-032 — Marchés publics remportés (DECP)

> **Réactivée le 29 mai 2026** — promue de V3+ vers V2. Le motif initial de report (« trop spécifique BTP / consulting / IT public ») ne tient plus : (a) la donnée DECP est désormais **consolidée et propre** (jeu unique sur data.gouv.fr + API tabulaire, le scraping multi-sources n'est plus nécessaire), (b) elle se branche sur le **pattern existant de F-048** (coût marginal faible, purement additif).

**Description** : pour une entreprise, Atlas affiche les **marchés publics qu'elle a remportés** (en tant que titulaire) : acheteur, objet, montant, durée, date de notification, code CPV. Quand une entreprise **suivie** (favori / watchlist) remporte un nouveau marché, un **événement** apparaît dans sa timeline — sur le même principe que les annonces BODACC (F-048).

**Valeur user** : le DECP apporte un signal que ni Pappers ni Societe.com n'exploitent à fond : l'**activité réelle**. Pas « cette boîte existe et a tel bilan » mais « cette boîte **gagne effectivement** des marchés publics, pour tel montant, auprès de tel acheteur ». Croisé avec l'identité (RNE), la PI, la veille et les finances (F-054), ça complète la vue « que fait vraiment cette entreprise ». Personas : **Investisseur / M&A**, **veille concurrentielle** (qui rafle les marchés dans mon secteur ?), et tout profil travaillant avec / autour du secteur public. Contrairement à la crainte initiale, ce n'est pas « niche BTP » — la commande publique achète dans **tous** les secteurs (IT, conseil, services, fournitures, travaux).

**Complexité** : ★★★ — surtout grâce à la réutilisation du pattern F-048 (polling → `FavoriteEvent` → timeline) et à la consolidation de la source.

**APIs externes** :
- **DECP consolidé** (data.gouv.fr) : jeu unique retraité, formats **Parquet / CSV**, mis à jour quasi quotidiennement, régi par l'arrêté du 22 décembre 2022 (étendu mars 2024). Couvre les marchés notifiés depuis 2020 (7 millésimes).
- **API tabulaire data.gouv.fr** : consommation directe du jeu (≈ 100 req/s) « pour alimenter une application sans configurer de base ».

Gratuit, sans authentification. Le **scraping n'est plus nécessaire** (corrige la note « DECP via scraping » du pack Investisseur, doc 07).

**Dépendances** : F-017 (favoris), F-053 (watchlists), F-047 (timeline mixte) & patron de F-048 (polling BODACC → `FavoriteEvent`), ADR-004 (archi hexagonale), ADR-006 (open core).

**Détails techniques** :
- **Deux modes, une source** (calqués sur l'existant) :
  - **Mode événement** (pattern F-048) : job `decp-polling` repère les nouveaux marchés attribués aux SIREN suivis → crée un `FavoriteEvent` de type « marché public attribué » → fusionné dans la timeline (F-047), dédupliqué via `FavoriteEvent.ExternalId` (comme BODACC).
  - **Mode fiche** : section « Marchés publics remportés » alimentée à la demande ou depuis l'ingestion.
- **Source : ingestion vs API tabulaire (décision ouverte)** :
  - **Ingestion périodique** du jeu consolidé (filtré / indexé par SIREN titulaire) dans la base Atlas → rapide en lecture, autonome, idéal pour le mode événement (job Hangfire `decp-polling` sur le modèle de `bodacc-polling`).
  - **API tabulaire à la demande** : interroger par SIREN au moment de l'affichage de la fiche → zéro stockage, mais dépendant de la dispo de l'API.
  - Probable : ingestion pour les événements + API tabulaire pour le détail de fiche.
- **Matching titulaire → entreprise** : le titulaire est identifié en **SIRET** (établissement) ; on mappe sur le **SIREN** (9 premiers chiffres) pour matcher les entreprises favorites / watchlists. Trivial, mais à faire explicitement (une entreprise peut remporter un marché via un établissement secondaire).
- **Architecture (hexagonale, ADR-004)** : adapter `DecpSource` implémentant `IExternalContentSource` (mode événement) comme `BodaccSource` ; éventuel port `IPublicProcurementProvider` pour les requêtes de fiche (mode fiche) ; extension de `FavoriteEvent` avec un nouveau type / `Kind` (« marché public »). **Aucune modification du domaine existant.**

**Cadre légal & positionnement** : la piste facile — open data pur, données publiées par obligation légale (code de la commande publique, articles L2196-2 / L3131-1). Afficher des marchés attribués = exposer des **faits publics**. Aucune ligne sensible à tenir, aucun RGPD spécifique (données d'entreprises et de contrats publics). On reste **descriptif** : on liste, on n'évalue pas (pas de « score puissance publique » agrégé).

**Couverture & qualité (caveats honnêtes)** :
- **Seuil** : seuls les marchés **> 40 000 € HT** sont publiés. En dessous, rien.
- **Périmètre** : seulement les entreprises qui **remportent** des marchés publics → beaucoup de favoris n'en auront aucun (afficher « aucun marché public connu » proprement, pas un vide ambigu).
- **Qualité variable** : malgré l'obligation, certains acheteurs publient mal / incomplètement ; le jeu consolidé corrige beaucoup mais pas tout (champs parfois vides — déjà noté en doc 03 §3.1).

**Hors périmètre (explicite)** :
- **Avis d'appels d'offres** (avant attribution) : c'est le **BOAMP** (doc 03 §3.2), source différente — hors périmètre de cette fiche.
- Marchés **européens** (TED, doc 03 §3.3) : extension future éventuelle.

**Accessibilité (rappel ADR-008)** : montants, dates et acheteurs en tableau lisible au lecteur d'écran (`SemanticProperties.Description`). Aucune information (ex. type de marché) transmise par la couleur seule.

**Modèle économique** : aucun coût par utilisateur (open data, déterministe) → **cœur open source**, gratuit. Cohérent ADR-006.

> **Architecture liée** : à l'implémentation, F-032 émet un `FavoriteEvent PublicContractAwarded` dans la timeline F-047 (patron F-048) — alimente la **taxonomie de signaux de F-058** (volet « signal commercial »).

**Découpage / jalons** :
1. **Source & matching** : ingestion du jeu consolidé (ou accès tabulaire), mapping SIRET → SIREN. *(Fondation testable.)*
2. **Mode fiche** : section « Marchés publics remportés » sur la fiche entreprise.
3. **Mode événement** : `DecpSource` + job `decp-polling` → `FavoriteEvent` → timeline (pattern F-048).

**Décisions ouvertes** :
- **Source** : ingestion périodique vs API tabulaire à la demande (ou les deux, fiche + événements).
- **Ordre de livraison** : mode fiche d'abord (autonome) ou mode événement d'abord ?
- **Seuil de notabilité** d'un événement (tout marché, ou au-dessus d'un montant ?).
- **Extension future** : BOAMP (avis avant attribution) et TED (UE) — à garder hors périmètre pour l'instant.
