# Persona — Développeur / Tech curieux

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #6). **Dernier persona de la cartographie.**
> **Statut** : déroulé le 29 mai 2026.
> **Lentille distinctive** : ne **consomme** pas Atlas comme un produit — il **construit dessus** (API, MCP, webhooks, adapters), l'**auto-héberge** et l'**étend**. C'est le **persona-plateforme**, là où convergent tous les vecteurs d'ouverture. (Et accessoirement, celui qui ressemble le plus au porteur du projet.)

---

## 1. En une phrase
Un développeur qui **bâtit par-dessus Atlas** (ou l'auto-héberge et l'étend), attiré par l'accès programmatique, la souveraineté et un code propre dont on apprend.

## 2. Contexte & enjeux métier
- Refuse le **jardin clos** : veut un accès programmatique, l'auto-hébergement, l'extensibilité.
- Valorise la **souveraineté** et le fait de **posséder sa stack** (tendance de fond du self-hosting).
- Veut **automatiser / intégrer** la donnée entreprise dans ses propres outils et agents.
- **Tech-curieux** : séduit par une archi .NET hexagonale propre, l'AGPL, l'envie d'apprendre et de contribuer.
- Accepte la contrepartie du self-hosting : il devient **son propre sysadmin**.

## 3. Jobs-to-be-done
- « Donne-moi un accès programmatique propre (API). »
- « Laisse-moi brancher Atlas dans mon agent / mes outils (MCP, webhooks). »
- « Laisse-moi ajouter une source qui m'intéresse (adapter). »
- « Laisse-moi l'auto-héberger et le faire mien. »
- « Montre-moi du code propre dont j'apprends — et que je peux auditer. »

## 4. Besoins clés
- **API publique documentée** (OpenAPI).
- **Serveur MCP (F-052)** + **OAuth (ADR-011)** pour l'accès agentique.
- **Webhooks (F-038)** pour l'événementiel.
- **Extensibilité** (hexagonal → écrire des adapters/sources).
- **Auto-hébergement** (Docker, docs, BYO credentials ADR-003).
- **Clarté de l'AGPL** (ce qu'il peut / doit faire).

## 5. Usage d'Atlas
- **API publique** (doc 01 : gratuite en auto-hébergé, quotas en hébergé).
- **Serveur MCP (F-052)** + **OAuth 2.1 (ADR-011)** : l'accès agentique.
- **Webhooks (F-038)** : intégration événementielle.
- **Auto-hébergement** (ADR-003, AGPL) : il fait tourner Atlas chez lui.
- **Ajout d'adapters/sources** (`IExternalContentSource`, hexagonal ADR-004) : il étend la portée sans toucher au cœur.
- **Packs de veille communautaires** (marketplace F-042).
- **Extension navigateur (F-036)** + **pack veille « Développeur / Tech curieux »** (doc 07 : .NET Blog, Hacker News, Lobste.rs, GitHub Trending…).

## 6. Ce qui compte pour lui (les *interfaces*, pas les données)
Pour ce persona, la « matière » n'est pas une source de données mais les **interfaces** d'Atlas : l'**API**, le **schéma des outils MCP**, les **webhooks** (événements), les **ports** (contrats d'extension hexagonaux) et le **code lui-même** (AGPL, lisible, auditable).

## 7. Ce que la concurrence ne sert pas
Les SaaS fermés (Pappers, Societe, outils CI/PE) offrent au mieux une **API vers un jardin clos** — pas d'auto-hébergement, pas de code, pas de sources custom, pas d'agent souverain. La **BI open-source** (Metabase, Superset) est **générique** : des dashboards sans couche de données entreprise (à bâtir soi-même). Les **outils OSINT** (Maltego…) visent l'**investigation/sécurité** et l'empreinte numérique, pas la donnée registre française structurée — même si le *Transform Hub* communautaire de Maltego est un **parallèle d'extensibilité** intéressant (et un avertissement : les intégrations communautaires varient en profondeur). **Aucune** plateforme d'intelligence entreprise **open-source, auto-hébergeable, ancrée registre français et native MCP** n'existe. La place est **vide**.

## 8. Champs inexplorés & game-changers (la convergence)
- **Atlas comme plateforme, pas seulement produit** `[ouverture]` — *le* méta-game-changer : les cinq autres personas **consomment** des features ; celui-ci transforme Atlas en **substrat** sur lequel d'autres construisent. Effet de **volant d'inertie** : adapters et packs communautaires améliorent Atlas pour tous.
- **Le substrat agentique souverain** `[agentique]` `[souveraineté]` : MCP (F-052) + auto-hébergement → faire tourner **son** agent contre la donnée entreprise, sur **son** infra, avec **ses** credentials. Aucun SaaS fermé ne peut offrir « ton agent, ton infra, ta donnée ». C'est la convergence des deux vecteurs les plus profonds — et c'est ce persona qui la débloque.
- **L'extensibilité par l'hexagonal** `[ouverture]` : ports/adapters → ajouter une source sans toucher au cœur ; la communauté étend la portée d'Atlas (ce qu'un produit fermé ne peut pas crowdsourcer).
- **Le code comme artefact d'apprentissage et de confiance** `[ouverture]` `[confiance]` : AGPL + code .NET hexagonal propre = une **référence** dont on apprend **et** qu'on **audite** (l'auditabilité de l'outil lui-même, pas seulement de sa donnée). C'est aussi l'entonnoir de contribution.
- **La marketplace communautaire** `[ouverture]` : packs (F-042), adapters, thèmes — un produit qui grandit **collectivement**.

---

## Implication stratégique : le moteur de l'open-core
Ce persona n'apporte pas de feature de donnée — il apporte la **dimension plateforme**, et c'est le **moteur du modèle open-core (ADR-006)**. Ce n'est pas un persona à revenu direct : c'est le **haut de l'entonnoir** et la **base de contributeurs**. L'OSS attire les devs et les auto-hébergeurs ; une partie convertit vers l'hébergé / le premium. C'est le persona de la **croissance** et du **moat**.

## Frontières & points de vigilance
- **AGPL (copyleft)** : qui auto-héberge/modifie doit partager ses changements — une **force** (l'écosystème reste ouvert), mais à **clarifier** ; les usages commerciaux préfèreront l'hébergé pour éviter ces obligations (le hook open-core).
- **API** : quotas, rate-limiting, anti-abus (l'anti-SSRF F-043 se généralise). **MCP** : OAuth, scopes, **jamais** exposer les credentials (déjà cadré ADR-011).
- **L'API/MCP expose la même donnée disciplinée** (ADR-012, descriptif, pas d'UBO) — **pas** une porte dérobée aux doctrines.
- **Adapters communautaires** : profondeur/qualité variables (leçon Maltego) → besoin de **curation/gouvernance**.

---

*Persona figé le 29 mai 2026. Persona-plateforme : il transforme Atlas en substrat et porte le modèle open-core. **Dernier persona — la cartographie est complète.***
