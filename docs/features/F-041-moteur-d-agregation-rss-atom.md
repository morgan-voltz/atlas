# F-041 — Moteur d'agrégation RSS / Atom

> **Statut** : ✅ Implémenté (MVP 2, 27 mai 2026). Contexte **Veille** : port `IExternalContentSource`, entités `FeedSource`/`FeedItem` (hash URL+titre pour la dédup), adapter `RssFeedProvider` (CodeHollow.FeedReader, lecture via `HttpClient` typé + parsing défensif). Polling récurrent via **Hangfire** (storage PostgreSQL, cron 30 min, désactivable par `BackgroundJobs:Enabled`), use case `PollFeedSourcesCommand` (résilient par source), endpoint `GET /feed/items`. Sources système amorcées au démarrage (cf. doc 07). Validé contre de vrais flux. **Reste** (autres features du cluster) : templates/abonnements (F-042/043), timeline enrichie (F-044), dédup intelligente (F-045).

**Description** : moteur backend capable de poller des flux RSS et Atom à intervalle régulier, de parser leurs items, et de les stocker en BDD pour exposition aux utilisateurs.

**Valeur user** : fondation technique de toute la feature veille. Invisible mais critique.

**Complexité** : ★★★★

**APIs externes** : sources RSS/Atom variées (cf. doc 07).

**Dépendances** : F-001.

**Détails techniques** :
- Bibliothèque : `CodeHollow.FeedReader` ou `System.ServiceModel.Syndication` natif .NET
- Port métier `IExternalContentSource` côté domaine (Core)
- Adapter `RssFeedSource` côté infrastructure
- Job de polling via Hangfire ou MassTransit (intervalle configurable, par défaut 30 min)
- Déduplication par hash de l'URL + titre
- Gestion des erreurs : flux mort, parsing échec, timeout
