# F-048 — Intégration BODACC dans la veille

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Nouveau projet `Atlas.Infrastructure.Bodacc` avec adapter `OpendatasoftBodaccProvider` interrogeant l'API publique `bodacc-datadila.opendatasoft.com` (anonyme, pas d'INPI requis). Job Hangfire `bodacc-polling` cron `0 4 * * *` (après F-019), **déduplication cross-users** (1 appel API par SIREN partagé). Chaque annonce non encore connue → `FavoriteEvent` type `BodaccPublished` avec `ExternalId = AnnouncementId BODACC`. Index unique partiel `(user_id, external_id) WHERE external_id IS NOT NULL` pour dédup efficace. Apparaît dans la timeline via la fusion F-047 volet 2. **Reste** : (a) configuration fine côté user (mots-clés, secteurs, types d'annonces) — actuellement toutes les annonces sont remontées, (b) validation contre l'API réelle (schéma documenté mais non testé en prod).

> **Architecture liée** : F-048 est la **première implémentation** du patron `IItemStreamMonitor<BodaccAnnouncement>` formalisé par **ADR-013** (substrat de surveillance — flux append-only, dédup par `ExternalId`). Le patron polling + dédup cross-users inventé ici est généralisé par le runner mutualisé qui sera extrait au moment de F-057.

**Description** : intégration du BODACC comme source de veille parmi d'autres, avec configuration fine (mots-clés, secteurs d'activité, tribunaux, types d'annonces).

**Valeur user** : alertes temps réel sur des événements légaux significatifs (RJ d'un client, vente d'un fonds dans son secteur, etc.).

**Complexité** : ★★★

**APIs externes** : BODACC (data.gouv.fr / Opendatasoft).

**Dépendances** : F-041.

**Détails techniques** :
- BODACC expose des flux RSS personnalisés via Opendatasoft
- Possibilité de wrap dans un adapter `BodaccSource` implémentant `IExternalContentSource`
