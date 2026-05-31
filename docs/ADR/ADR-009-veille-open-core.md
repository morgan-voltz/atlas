# ADR-009 — Veille comme feature majeure du produit + open core

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

L'analyse du marché français des outils sur données entreprises (Pappers, Societe.com, Annuaire des Entreprises) révèle qu'**aucun acteur ne combine les données entreprises avec un système de veille (flux RSS, BODACC, BOPI, actualités sectorielles)**. À l'inverse, les lecteurs RSS / outils de veille (Feedly, Inoreader) n'intègrent pas les données entreprises.

Cette absence constitue un **trou de marché clair**, exploitable par le projet.

La feature "veille agrégée" est conceptuellement simple mais soulève plusieurs choix structurants :
- Place dans la roadmap (MVP 1, MVP 1.5, MVP 2 ?)
- Mode de découverte des sources (templates pré-curés vs ajout libre)
- Niveau d'enrichissement (basique vs IA)
- Modèle économique (gratuit vs open core vs premium)

## Décision

**Positionnement** : la veille devient une **feature majeure et différenciante** du produit, exploitant le trou de marché identifié.

**Place dans la roadmap** : **MVP 2** (mois 4–8 dans le planning). On valide d'abord le cœur RNE/PI en MVP 1, puis on ajoute la veille comme différenciateur.

**Découverte des sources** : **templates pré-curés par métier** (Cabinet PI, Expert-comptable, Compliance, Investisseur, etc.) **+ ajout libre** par l'utilisateur.

**Niveau d'enrichissement initial (MVP 2)** : **moyen** — déduplication + filtres + alertes. Pas d'IA en MVP 2.

**Modèle économique** : **open core aligné sur ADR-006**. Le moteur de veille reste open source. Les features d'enrichissement IA (résumés automatiques, scoring de pertinence, synthèse hebdomadaire) deviendront premium **uniquement en phase 3-4** (mois 12+), pas avant.

## Découpage fonctionnel précis

| Sous-feature | Statut futur |
|---|---|
| Lecteur RSS + abonnements | Gratuit / OSS définitif |
| Templates de veille par métier | Gratuit / OSS définitif |
| Ajout libre de sources | Gratuit / OSS définitif |
| Déduplication + filtres | Gratuit / OSS définitif |
| Alertes email + push | Gratuit (limites quantitatives en hébergé selon plan) |
| **IA : résumés automatiques** | Premium (phase 3+) |
| **IA : scoring de pertinence** | Premium (phase 3+) |
| **IA : synthèse hebdomadaire** | Premium (phase 3+) |
| **Collaboration / partage équipe** | Premium (phase 3+) |
| Marketplace de templates partagés | Gratuit |
| API publique pour la veille | Gratuit auto-hosted, quotas si hébergé |

## Rationale

- **Trou de marché clair** : aucun concurrent direct ne combine ces fonctions.
- **Lead magnet puissant** : la veille gratuite attire des users qui resteront ensuite pour les fonctions premium.
- **Cohérence stratégique** : aligné avec ADR-006 (OSS pur jusqu'à ~mois 12, puis SaaS hébergé + open core).
- **Faisabilité technique** : l'archi hexagonale (ADR-004) rend l'agrégation de sources externes naturelle. Chaque flux est un adapter implémentant un port `IExternalContentSource`.
- **Modèle économique propre** : les features qui coûtent de l'argent par utilisateur (IA = coût LLM) sont monétisées ; le reste est libre.

## Conséquences

- **Positives** : différenciation forte, valeur ajoutée immédiate, hook produit, modèle éco viable.
- **Négatives** : surface fonctionnelle élargie en MVP 2 (~10 features supplémentaires, cf. doc 02). Volume technique à gérer (parser RSS, dédup, scaling).
- **À prévoir** :
  - Isolation des modules premium dès l'architecture (projet `<Projet>.Application.Premium` séparé)
  - Définition du port `IExternalContentSource` côté domaine
  - Catalogue initial de templates de veille par métier (à constituer en partie avec la doc 07)
  - UI accessible de la timeline veille (cf. doc 06)
  - Mécanisme de modération des sources user-defined
- **Référentiel détaillé** : voir documents `02-roadmap-features.md` (cluster F-041 à F-050), `03-catalogue-apis-publiques.md`, `07-flux-rss-veille.md`
