## ADR-028 — Observabilité du backend (OpenTelemetry, découplage émission/collecte, progressif)

**Statut** : ✅ Accepté
**Date** : 30 mai 2026

### Contexte

ADR-019 pose la préprod (VPS unique, 2 vCPU / 4 Go, app + PostgreSQL + Hangfire) et ADR-018 la **dégradation gracieuse** des sources amont (l'app *survit* quand une source tombe). Mais **rien ne dit comment on *observe* le système** : savoir si l'API répond, si les jobs Hangfire tournent, si les sources INPI/BODACC sont joignables, où ça plante côté serveur — et être prévenu **avant** la panne.

C'est une **observabilité technique** (santé de l'infrastructure), à ne pas confondre avec la **télémétrie produit** (F-076, usage par les utilisateurs, opt-in, RGPD). L'observabilité technique porte sur **les machines d'Atlas, pas sur ses utilisateurs** : **aucun enjeu RGPD**, pas de consentement, pas d'opt-in.

Deux contraintes cadrent la solution :
1. **Le VPS de préprod est modeste** (ADR-019 : 4 Go déjà pris ; « 2 Go = risque d'OOM »). Une stack d'observabilité complète self-hosted (Prometheus + Grafana + Loki + Tempo + Alertmanager) pèse **~2–4 Go de RAM** → elle **ferait tomber le serveur qu'elle surveille**. Contradiction physique à éviter.
2. **La prod est différée** (ADR-019). Spécifier l'infra d'observabilité de prod maintenant serait prématuré.

### Décision

**Observabilité conçue complète, déployée progressivement — via le découplage émission/collecte.**

**1. Émission : OpenTelemetry (OTel), complet et dès maintenant.** `Atlas.Api` est instrumenté pour émettre les **trois piliers** — **logs** (Serilog), **métriques** (latence, taux d'erreur, durée/échec des jobs, santé des sources amont), **traces** (requête à travers les couches hexagonales : API → use case → adapter → source). L'instrumentation vit **dans le process** (.NET), pèse quelques Mo, et est **indépendante du backend** qui la reçoit.

**2. Découplage émission ↔ backend de collecte.** OTel sépare *émettre* de *recevoir/stocker/afficher*. On **instrumente une fois (complet)** ; on **branche le backend que l'environnement permet**, sans retoucher le code :
- **Préprod (maintenant)** : **léger** — logs Serilog (fichier/console), un collecteur OTel minimal **ou** un endpoint d'agrégation maison. **Pas** de stack Prometheus+Grafana+Loki sur la box (cf. contrainte 1).
- **Prod (différée)** : **stack complète**, sur une **machine dédiée** (jamais sur la box applicative) — réalistement **~8–16 Go**, soit ~10–16 €/mois (p. ex. deux box Hetzner UE : applicative + observabilité). Justifié par le trafic réel et les SLA implicites du SaaS payant.

*Conséquence* : « complet d'emblée » est obtenu par l'**architecture** (instrumentation), pas par l'amputation. On n'abandonne aucun pilier ; on dimensionne seulement le backend.

**3. Alerting : externe au système surveillé, par nature.** Un alerteur hébergé **sur la box** plante avec elle → on n'est jamais prévenu. Donc :
- **Préprod (maintenant)** : un **uptime check externe** (ping `/health` depuis l'extérieur, ex. UptimeRobot / Better Stack, plan gratuit) + les **échecs de jobs Hangfire** qui notifient. Minimum vital « l'API est-elle vivante ? ».
- **Prod** : Alertmanager (ou équivalent) sur la machine d'observabilité dédiée, pour l'alerting fin (seuils de latence, quotas INPI proches, taux d'erreur). **L'uptime check externe reste** (il voit ce que l'infra interne ne peut pas voir : la box entière HS).
- L'alerting externe **ne voit aucune donnée utilisateur** (juste « l'URL répond ») → compatible souveraineté.

**4. Endpoint `/health`.** `Atlas.Api` expose un health check agrégé : **API up**, **PostgreSQL joignable**, **Hangfire vivant**, et l'**état des sources amont** (réutilise la logique ADR-018). Consommé par l'uptime check externe et le reverse proxy (ADR-019).

**5. Souveraineté : self-hosted pour le stockage, externe pour la seule surveillance de vie.** Le **stockage** des logs/métriques/traces (qui peuvent contenir des détails d'infra) reste **auto-hébergé** (cohérent ADN, comme F-076). Seul le **ping de disponibilité** (qui ne voit aucune donnée) est externalisé — ce n'est pas une fuite, c'est une nécessité technique.

**6. Hygiène des logs (rappel doctrine).** **Jamais de secret ni de donnée sensible** dans un log (credentials INPI, tokens, contenu de requête) — cohérent avec la règle « messages d'erreur sans détail sensible » (doc 12 §12) et le vocabulaire (`inpi.not_connected`, `veille.fetch_failed` sont des codes, pas des dumps). Les traces sont **scrubbées**.

### Rationale

- **OTel = standard .NET, pérenne, vendor-neutral** : instrumenter une fois, changer de backend sans réécrire. Évite le verrouillage et le travail jeté.
- **Découplage** : seule façon de concilier « observation complète » et « VPS modeste » sans contradiction (contrainte 1).
- **Alerting externe** : un système ne peut pas surveiller sa propre mort. Non négociable.
- **Progressif** : aligné sur ADR-019 (prod différée) — on n'investit pas en infra lourde sur un environnement de test reseedable.
- **Complète ADR-018** : la dégradation gracieuse *encaisse* la panne d'une source ; l'observabilité *la signale*. Les deux ADR sont les deux faces de la résilience.

### Conséquences

- **Positives** : visibilité « ça plante / ça va planter » dès la préprod **pour ~0 € de plus** (instrumentation + uptime gratuit) ; montée en puissance sans réécriture ; souveraineté du stockage préservée ; séparation nette d'avec la télémétrie produit (pas de confusion RGPD).
- **Négatives / coûts** : l'instrumentation OTel est un travail d'amorçage (à faire tôt) ; la prod demandera une **machine dédiée** (budget à prévoir, ~10–16 €/mois) ; discipline de scrubbing des logs à tenir.
- **À prévoir** :
  - Instrumenter `Atlas.Api` (OTel : logs Serilog + métriques + traces) — cf. **F-077**.
  - Exposer `/health` agrégé (API/DB/Hangfire/sources amont).
  - Mettre en place l'**uptime check externe** sur `/health` + la notification d'**échec de job Hangfire**.
  - **Prod (différé)** : machine d'observabilité dédiée + stack complète + Alertmanager — à trancher en phase déploiement (avec le choix d'hébergeur de prod, doc 01).
  - Références : **ADR-019** (préprod), **ADR-018** (dégradation gracieuse — `/health` réutilise sa logique), **F-076** (télémétrie produit — sujet distinct), **F-077** (mise en œuvre).

---

*ADR figé le 30 mai 2026. Observabilité technique (≠ télémétrie produit F-076, sans enjeu RGPD). Principe : instrumenter complet (OpenTelemetry, 3 piliers) dès maintenant, découpler l'émission du backend de collecte (léger en préprod sur le VPS d'ADR-019, stack complète sur machine dédiée en prod), alerting externe au système surveillé (uptime check sur `/health`). Stockage auto-hébergé (souveraineté) ; seul le ping de vie est externalisé. Complète ADR-018 : la dégradation encaisse, l'observabilité signale.*
