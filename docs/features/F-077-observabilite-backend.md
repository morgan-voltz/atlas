# F-077 — Observabilité du backend (instrumentation & santé)

> **Statut** : 🟡 À spécifier. Cible : **V1** (l'instrumentation se fait tôt ; le backend lourd attend la prod).
> **Date** : 30 mai 2026.
> **Catégorie MoSCoW** : Should have (visibilité d'exploitation ; non bloquant pour les fonctions cœur, mais quasi gratuit et très utile dès l'alpha).
> **Décision cadre** : **ADR-028** (observabilité — OTel, découplage émission/collecte, alerting externe, progressif).
> **Distinct de** : **F-076** (télémétrie *produit*, usage utilisateurs, opt-in/RGPD). Ici = santé *infrastructure*, **pas de donnée utilisateur, pas de RGPD**.
> **Dépendances** : `Atlas.Api`, PostgreSQL + Hangfire (ADR-019), logique d'état des sources amont (**ADR-018**), reverse proxy (ADR-019).
> **Documents liés** : `02-roadmap-features.md`, `ADR-028`, `ADR-019`, `ADR-018`, `09-architecture-detaillee.md`.

---

## Description

Donner à l'exploitant (toi) une vue **« ça tourne / ça plante / ça va planter »** sur le backend, via l'instrumentation **OpenTelemetry** (logs, métriques, traces), un endpoint **`/health`**, un **alerting externe** minimal, et la **notification des échecs de jobs** Hangfire. Conçu complet, déployé léger en préprod, extensible en prod (ADR-028).

## Valeur

Ne pas voler en aveugle : savoir **immédiatement** si l'API est tombée, si un job de veille échoue en boucle, si une source amont est injoignable, ou si la latence dérive — **avant** que les testeurs (puis les clients) ne le signalent. Coût quasi nul en préprod (instrumentation + uptime gratuit), bénéfice immédiat.

## Périmètre

**Dans le périmètre :**
- **Instrumentation OTel** de `Atlas.Api` : 3 piliers (logs / métriques / traces).
- **`/health`** agrégé : API, PostgreSQL, Hangfire, état des sources amont (ADR-018).
- **Uptime check externe** sur `/health` (ping depuis l'extérieur).
- **Notification d'échec de job** Hangfire (un job qui rate / rate en boucle).
- **Backend de collecte léger** en préprod (logs fichier/console + collecteur minimal ou endpoint maison).

**Hors périmètre (ici / différé en prod) :**
- **Stack complète** (Prometheus + Grafana + Loki + Tempo + Alertmanager) sur **machine dédiée** → **prod** (ADR-028).
- **Alerting fin** (seuils de latence, quotas) → prod.
- **Toute donnée utilisateur** : l'observabilité technique n'en collecte pas (≠ F-076).

## Complexité : ★★ (préprod) → ★★★ (prod)

Préprod : faible — packages OTel + Serilog, un `/health`, un compte uptime gratuit, un handler d'échec Hangfire. Prod : la machine dédiée + la stack + l'alerting fin (le gros), mais **différé**.

## Détails techniques

### Instrumentation (OpenTelemetry)
- **Logs** : Serilog, format **structuré** (JSON), corrélés au trace-id.
- **Métriques** : compteurs/histogrammes — latence API par endpoint, taux d'erreur (4xx/5xx), **durée et échec des jobs Hangfire**, appels aux sources amont (succès/échec/latence — recoupe ADR-018), pool de connexions DB.
- **Traces** : propagation à travers les couches hexagonales (API → use case → port → adapter → source externe), utile pour localiser un ralentissement.
- **Export** : via le **collecteur OTel** (ou OTLP direct) → backend léger en préprod, stack dédiée en prod. **Le code ne change pas** entre les deux (découplage ADR-028).

### `/health`
- Sous-checks : **API up**, **PostgreSQL** (requête triviale), **Hangfire** (serveur de jobs vivant), **sources amont** (réutilise la logique d'état ADR-018 — sans appel coûteux : statut connu/caché).
- Deux variantes : `/health/live` (le process répond) et `/health/ready` (dépendances OK). Exposé derrière le reverse proxy (ADR-019).
- **Aucune donnée sensible** dans la réponse (statuts, pas de détails d'infra exploitables).

### Alerting (externe — ADR-028)
- **Uptime check** (UptimeRobot / Better Stack, plan gratuit) : ping `/health` toutes les 1–5 min → mail/push si KO. Vit **hors** du serveur → survit à la chute de la box.
- **Échecs de jobs** : Hangfire expose ses échecs ; un handler notifie (mail) au-delà d'un seuil de retries (recoupe le pattern de jobs F-048/F-019).

### Hygiène (rappel ADR-028 / doc 12 §12)
- **Jamais** de credential INPI, token, ou contenu de requête dans un log/trace. Traces **scrubbées**. Codes d'erreur (`inpi.not_connected`, `veille.fetch_failed`) = libellés, pas de dump.

## Cadre RGPD

**Sans objet côté utilisateur** : on observe l'infrastructure, pas les personnes. Vigilance unique : que les logs **ne capturent pas accidentellement** de donnée personnelle (d'où le scrubbing et l'interdiction de logguer le contenu des requêtes). L'IP éventuelle dans les logs d'accès est une donnée d'exploitation à durée de rétention courte.

## Découpage / jalons

1. **`/health` + uptime externe** : visibilité « API vivante » immédiate, presque gratuite. *(Premier livrable, autonome.)*
2. **Logs structurés (Serilog)** + scrubbing.
3. **Métriques + traces OTel** (latence, erreurs, jobs, sources amont), export vers backend léger.
4. **Notification d'échec de jobs** Hangfire.
5. **(Prod, différé)** machine d'observabilité dédiée + stack complète + alerting fin (ADR-028).

## Décisions ouvertes

- **Backend léger préprod** : collecteur OTel minimal vs endpoint d'agrégation maison vs simple fichier + lecture manuelle.
- **Fournisseur uptime** : UptimeRobot vs Better Stack (plans gratuits) — révisable.
- **Rétention** des logs/traces en préprod (faible).
- **Prod** : stack exacte + hébergeur de la box observabilité — à trancher avec le choix d'hébergeur de prod (doc 01, ADR-019).

## À faire à l'intégration

- Ajouter **F-077** au catalogue `02-roadmap-features.md` (Should have).
- Instrumenter `Atlas.Api` (OTel + Serilog) ; exposer `/health` (live/ready).
- Créer le compte **uptime check** externe sur `/health` + la notif d'échec Hangfire.
- Référencer **ADR-028** comme décision cadre ; vérifier la cohérence avec **ADR-018** (`/health` réutilise l'état des sources).

---

*Fiche figée le 30 mai 2026. Mise en œuvre d'ADR-028. Observabilité technique (≠ télémétrie produit F-076, pas de RGPD). Instrumentation OTel complète + `/health` + uptime externe dès la préprod (quasi gratuit) ; stack lourde et alerting fin différés en prod sur machine dédiée. Jamais de donnée sensible dans les logs/traces.*
