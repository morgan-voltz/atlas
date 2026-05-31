# F-043 — Ajout libre de sources par l'utilisateur

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Entité `VeilleSubscription`, port `IFeedSubscriptionPolicy` (limite par utilisateur configurable + blocklist d'hôtes + anti-SSRF Lot 2a), endpoints `POST /veille/subscriptions`, `DELETE /veille/subscriptions/{id}`, `GET /veille/subscriptions`. PR #18.

**Description** : l'utilisateur peut ajouter manuellement n'importe quel flux RSS / Atom. Le système valide la source (parsing test) et l'intègre à son abonnement.

**Valeur user** : indispensable pour les power users et les cas particuliers non couverts par les templates.

**Complexité** : ★★

**APIs externes** : la source RSS fournie par l'utilisateur.

**Dépendances** : F-041.

**Détails techniques** :
- Validation : test de fetch, parsing, vérification taille raisonnable
- Modération : bibliothèque d'URLs interdites (sources de spam, contenu manifestement illégal)
- Limite de sources par compte : configurable selon le plan (illimité en self-hosted, limite en hébergé selon offre)
