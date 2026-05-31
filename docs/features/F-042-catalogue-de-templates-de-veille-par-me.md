# F-042 — Catalogue de templates de veille par métier

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Entité `VeillePack` versionnée (champ `Version` incrémenté à chaque évolution), `IVeillePackRepository`, enrôlement / désenrôlement utilisateur, re-sync au upgrade via `VeillePackEnrollment.Version`. Sources système rattachées au pack ; **les sources d'un pack ignorent la limite par utilisateur** des sources libres (F-043). Endpoints `/veille/packs/*`. PR #19.

**Description** : bibliothèque de "Packs de veille" pré-curés. À l'inscription ou plus tard, l'utilisateur choisit un ou plusieurs templates correspondant à son métier (Cabinet PI, Expert-comptable, Compliance, Investisseur, Veille concurrentielle B2B, etc.). Les sources sont automatiquement abonnées.

**Valeur user** : friction zéro à l'onboarding. Un user obtient une veille pertinente en 30 secondes au lieu de configurer 2 heures.

**Complexité** : ★★★

**APIs externes** : aucune (sources internes au catalogue).

**Dépendances** : F-041.

**Détails techniques** :
- Catalogue persisté en BDD avec structure `Template > Sources > FeedDetails`
- Constitué initialement à partir du document `07-flux-rss-veille.md` (cercles de pertinence)
- Versionné (un template peut évoluer, les utilisateurs choisissent s'ils synchronisent)
