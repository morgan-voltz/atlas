# ADR-019 — Serveur de préproduction (alpha & beta) : VPS unique tout-en-un

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

La **phase 1** (MVP 1) approche : il faut une **cible de déploiement** pour les tests **alpha** (l'auteur + quelques testeurs) puis **beta** (quelques dizaines), en amont du SaaS hébergé payant prévu en **phase 3**. Ces environnements portent des **données de test reseedables**, pas des données clients.

Le **choix de l'hébergeur de production** reste explicitement **différé** (doc 01, table « décisions ultérieures » : « OVHcloud, Scaleway, Clever Cloud, Hetzner… — décision en phase déploiement »). **Cet ADR ne tranche que la préproduction** ; il ne préempte pas la topologie de prod.

Rappel de **ce qui doit réellement tourner** sur la box, car c'est ce qui dimensionne :

- **Un seul backend ASP.NET Core** (`Atlas.Api`) — process .NET unique portant auth, appels amont (RNE / INPI PI / BODACC), persistance (ADR-002, ADR-007).
- **PostgreSQL**, qui porte **à la fois les données métier et la file de jobs Hangfire** (doc 13 ; F-041, F-048). Conséquence directe : **pas de Redis ni de RabbitMQ** nécessaires à ce stade.
- **Les jobs de fond Hangfire in-process**, qui pollent **en continu** (sources de veille ~30 min ; BODACC en cron `0 4 * * *`) — ce qui découle d'ADR-001 (« fonctions asynchrones côté backend 24/7 »).
- **Les fichiers statiques du client web Blazor WASM**, servis par l'API (ADR-017) — coût CPU négligeable.

Ne tournent **pas** sur le serveur : les **clients MAUI** (installés sur les devices) et l'**exécution WASM** (dans le navigateur du testeur). Le serveur ne fait que l'API + la base + les jobs.

## Décision

Sept principes pour l'environnement de préproduction.

**1. Préprod = un seul VPS, tout-en-un.** API + PostgreSQL + Hangfire (in-process) + statiques WASM sur **la même machine**, orchestrés par **docker-compose miroir du harness local** (doc 13 : PostgreSQL conteneurisé, `BackgroundJobs:Enabled` pilotable). **Pas de PaaS, pas de base managée** à ce stade : la base de test est reseedable, la valeur d'une DB managée (PITR, réplication) appartient à la prod.

**2. Dimensionnement : 2 vCPU / 4 Go RAM / 40–80 Go SSD NVMe.** Le consommateur de RAM n'est **pas le trafic** (négligeable en alpha/beta) mais **l'empilement .NET + PostgreSQL + jobs** sur une seule box. 4 Go donnent la marge pour les **migrations EF**, les **téléchargements d'actes / bilans RNE**, et le **pic des jobs Hangfire**. 2 Go est techniquement jouable en alpha pur mais déconseillé (risque d'OOM parasite, sans rapport avec le code).

**3. Localisation UE — Hetzner pressenti, provider révisable.** Cohérence avec un produit d'**open-data français**, **RGPD-natif** (F-012), qui **stocke des credentials INPI chiffrés** (ADR-003). Fournisseur **pressenti** : **Hetzner** (compte déjà existant ; entité et datacenters UE — Allemagne / Finlande). **Garde-fou explicite** : provisionner dans une **région UE** (Allemagne ou Finlande), **jamais** une région hors-UE du fournisseur — c'est le **choix de région**, pas la marque, qui fait la conformité RGPD. Le **provider exact reste révisable** : cet ADR acte la **forme** (VPS unique UE), pas le fournisseur définitif.

**4. Always-on, derrière un reverse proxy TLS.** Le serveur **ne s'éteint pas** : conséquence directe d'ADR-001 (jobs 24/7). Un **reverse proxy** (terminaison TLS via Let's Encrypt) se place devant l'API ; l'API ne sert jamais en clair.

**5. Clé de chiffrement & secrets : posture préprod assumée.** ADR-003 exige AES-256-GCM **via KMS**. En préprod, sans KMS cloud, la **clé maître** est **injectée par variable d'environnement / fichier à permissions restreintes**, marquée **préprod-only**, **jamais commitée**. L'intégration **KMS réelle est un sujet de prod**. Cette dégradation est **acceptable** car la préprod ne porte que des données de test.

**6. Sauvegardes légères.** Données reseedables → **pas de PITR**. Un **`pg_dump` quotidien** suffit en préprod. PITR / base managée = prod.

**7. Maintenance par l'auteur, délégation différée (critère de bascule explicite).** L'ops de la préprod est **assurée par l'auteur** — c'est un choix **assumé et formateur** (TLS, docker, sauvegardes). **Critère de bascule** : le jour où la maintenance **empiète sur le temps de développement**, la décision de **déléguer** (managé / PaaS, ou ops externalisée) est **déjà anticipée** — elle se prend alors, pas dans l'urgence. Tant que c'est gérable, on reste sur le VPS auto-géré.

## Rationale

- **Ne pas sur-architecturer une préprod.** Une box unique **reflète le harness local** (doc 13) : continuité totale, **un seul artefact à administrer**, et un **apprentissage ops réel** (TLS, docker/systemd, sauvegardes) — aligné sur l'objectif d'apprentissage de l'auteur.
- **L'hexagonal rend le déploiement trivial** (ADR-004) : le domaine ignore l'infra, seuls les adapters changent. Le passage **préprod → prod ne touchera pas le métier**.
- **FR / UE n'est pas cosmétique.** Le produit **vend de la souveraineté** sur de l'open-data français et **stocke des credentials INPI** ; héberger hors UE contredirait le discours RGPD porté par F-012 et ADR-003.
- **Pas de file dédiée (Redis / RabbitMQ).** Hangfire sur PostgreSQL **tient largement** la charge préprod ; introduire MassTransit + RabbitMQ maintenant serait une **complexité prématurée** — ce que doc 01 différait déjà.

## Conséquences

- **Positives** : coût minimal (ordre de grandeur ~5 €/mois pour un 2 vCPU / 4 Go, à vérifier car les tarifs bougent) ; **un seul système à gérer** ; **parité préprod / local** ; apprentissage ops concret ; migration vers la prod **sans refactor métier** (hexagonal).
- **Négatives** : **point de défaillance unique (SPOF) assumé** — acceptable en préprod, **interdit en prod** ; pas de haute disponibilité ; **clé de chiffrement moins robuste** qu'un KMS (mitigée par le fait que ce sont des données de test).
- **À prévoir** :
  - **Décisions actées** (31 mai 2026 — passage en ✅) : **VPS auto-géré** retenu (pas de PaaS), maintenance par l'auteur avec **délégation différée** (principe 7) ; **localisation UE**, **Hetzner pressenti** (compte existant), **provider exact non bloquant** (principe 3).
  - **doc 01** : amender la ligne « Choix de l'hébergeur… décision en phase déploiement » → « **préprod tranchée par ADR-019** ; **prod reste ouverte** ».
  - **docker-compose de préprod** : **délégué à Claude Code** (hors périmètre ADR — l'ADR fixe la décision, pas l'implémentation).
  - **Hors-scope (volontaire) — futur ADR « infra de prod »** : base **managée + PITR**, **KMS réel** (ADR-003), **séparation API / DB**, **HA / scaling**, **CDN** pour les statiques WASM.
  - **Références croisées** : **ADR-001** (24/7 always-on), **ADR-002** (clients HTTP purs), **ADR-003** (secrets / KMS), **ADR-004** (hexagonal → déploiement trivial), **ADR-007** (tout-.NET), **ADR-017** (WASM = statiques) ; **F-012** (RGPD MVP), **F-041 / F-048 / F-057** (Hangfire / veille), **F-019** (alertes email) ; **doc 13** (harness local), **doc 01** (décision hébergeur prod).
