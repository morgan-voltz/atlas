# F-051 — Suivi boursier des entreprises cotées

**Description** : quand une entreprise favorite est cotée en bourse, Atlas affiche sa cotation sur sa fiche (cours, variation, mini-historique) et peut notifier d'une variation notable, sur le patron de F-019/F-048 (snapshot + diff + job Hangfire). **Fonction de suivi, pas service d'investissement** : aucune recommandation, aucun ordre, aucune donnée de portefeuille.

**Valeur user** : pour le persona Investisseur / M&A (déjà identifié dans les packs de veille), une vue unifiée « identité légale + propriété industrielle + veille + cotation » dans un seul outil.

**Complexité** : ★★★★ (phasable : pont d'identifiants → BYO-key → cours → événements).

**APIs externes** :
- **GLEIF** (Global LEI Foundation) — gratuit, sans auth. Sert au pont **SIREN → LEI → ISIN → ticker**. Le LEI étant obligatoire pour être coté, la chaîne couvre exactement le sous-ensemble pertinent.
- **Fournisseur de cours au choix de l'utilisateur** (BYO-key) : Finnhub, Twelve Data, EOD Historical Data, Financial Modeling Prep… Port `IMarketDataProvider` + adapters interchangeables.

**Dépendances** : F-017 (favoris entreprise), patron de F-019 (snapshot + diff + job Hangfire), ADR-003 (coffre de credentials chiffré), ADR-004 (architecture hexagonale).

**Détails techniques** :
- Ports : `ISecurityIdentifierResolver` (adapter `GleifIdentifierResolver`) et `IMarketDataProvider` (adapters par fournisseur).
- Modèle BYO-key : la clé du fournisseur de cours est chiffrée dans le coffre AES-256-GCM (réutilisation de `ICryptoService` / ADR-003). Avantage : zéro coût par utilisateur côté Atlas, redistribution réglée par construction, souveraineté préservée.
- Entités : `CompanySecurityLink (Siren, Isin, Ticker, Mic, ResolvedAt)`, `MarketDataCredential` (clé chiffrée + fournisseur choisi, cascade FK RGPD), `MarketEvent` (optionnel, branché sur la timeline F-047).
- Job Hangfire `market-refresh` sur le modèle de `favorite-refresh` / `bodacc-polling`.
- Endpoints (esquisse) : `POST/GET/DELETE /market/credential`, `GET /companies/{siren}/quote`.

**Cadre légal & éthique** : strictement descriptif. Pas de RGPD spécifique (données d'entreprises cotées, publiques) ; seule sensibilité = la clé API utilisateur (traitée comme secret, ADR-003).

**Accessibilité (rappel ADR-008)** : **jamais l'information par la couleur seule** sur les variations — toujours doubler d'un signe (`+`/`−`), d'une flèche, ou d'un libellé. Critique sur un écran de cotation.

**Modèle économique** : reste dans le cœur open source (BYO-key → zéro coût par utilisateur, cohérent ADR-006).

**Décisions ouvertes** :
- Fournisseur de cours recommandé par défaut dans la doc utilisateur.
- Granularité : EOD en V1, temps réel/différé en option ? La cadence du job en dépend.
- Seuil de « variation notable » (configurable par l'utilisateur ?).
- ADR dédié pour acter le pont d'identifiants et le choix BYO-key (recommandé, au même titre que l'ADR-003).
