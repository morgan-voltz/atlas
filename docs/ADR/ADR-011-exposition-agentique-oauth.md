# ADR-011 — Exposition agentique (MCP) et autorisation OAuth 2.1

**Statut** : ✅ Accepté — **mise en œuvre différée** à la phase de réalisation de F-052.
**Date** : 29 mai 2026
**Lié à** : F-052 (serveur MCP), ADR-003 (coffre de credentials), ADR-004 (architecture hexagonale), ADR-010 (authentification utilisateur custom).

## Contexte

La feature F-052 (serveur MCP) vise à exposer les capacités d'Atlas à des **agents IA** agissant **au nom de l'utilisateur**. Or l'authentification actuelle (ADR-010) est une auth **hexagonale custom** conçue pour des clients **first-party** (MAUI, web) : JWT RS256 courts, refresh tokens rotatifs, 2FA. Elle authentifie *l'utilisateur lui-même*, pas un *tiers agissant pour lui*.

Le besoin de F-052 est différent par nature : c'est de l'**autorisation déléguée**. Un agent tiers doit pouvoir obtenir un accès **limité, scopé, consenti et révocable** aux ressources de l'utilisateur — sans jamais détenir ses identifiants. C'est précisément le problème que résout **OAuth 2.1**, et c'est désormais l'**attendu de l'écosystème MCP 2026** (la spécification définit le serveur MCP comme un *resource server* OAuth ; les clients comme Claude découvrent le serveur d'autorisation et négocient des tokens scopés).

Trois options ont été pesées :

- **Étendre l'auth custom (ADR-010)** pour bricoler une délégation maison. Écarté : réinventer OAuth est une erreur de sécurité classique, et cela ne serait pas interopérable avec les clients MCP standard.
- **Adopter OAuth 2.1**, via une brique éprouvée. Retenu.
- Au sein d'OAuth, choix de l'implémentation : **OpenIddict** (bibliothèque .NET open source, native EF Core, sans contrainte de licence), **Duende IdentityServer** (excellent mais **licence commerciale** payante au-delà d'un seuil — incompatible avec la posture AGPL/coût), ou **Keycloak** (serveur externe, exploitation lourde — déjà écarté par l'ADR-010 pour cette raison).

## Décision

**Atlas adopte OAuth 2.1 comme mécanisme d'autorisation pour tout accès délégué / tiers** — d'abord les agents MCP (F-052), puis, à terme, l'API publique. La bascule est **assumée comme un chantier lourd** et sera réalisée **au moment d'implémenter F-052**, pas avant.

Principes :

1. **Coexistence, pas remplacement.** L'auth custom de l'ADR-010 **reste le socle d'identité** : comptes, hachage Argon2id, 2FA, coffre de credentials INPI (ADR-003). OAuth 2.1 se pose **au-dessus**, comme **couche d'autorisation** : le serveur d'autorisation authentifie l'utilisateur via le système d'identité existant, puis émet des **tokens délégués, scopés et courts** pour les clients agents.
2. **Périmètre borné au démarrage.** Les clients **first-party** (MAUI, web) conservent le flux JWT actuel dans un premier temps. Leur migration éventuelle vers OAuth est une étape **ultérieure et optionnelle**, pas un prérequis de F-052.
3. **Implémentation via OpenIddict.** Serveur d'autorisation **embarqué** dans l'API, persistance EF Core (cohérente avec la stack), open source, sans coût de licence, auto-hébergeable — fidèle à la philosophie « on maîtrise notre stack » de l'ADR-010 et à la souveraineté du projet.
4. **Exigences techniques minimales** : flux **Authorization Code + PKCE** (obligatoire en 2.1) ; **scopes** par groupe d'outils (lecture vs écriture) ; **resource indicators** (RFC 8707) pour lier le token au serveur MCP ; **tokens courts** + refresh rotatif révocable (réutilise les concepts de l'ADR-010) ; **écran de consentement** pour la délégation ; **découverte de métadonnées** du serveur d'autorisation. Le **DPoP** (RFC 9449, liaison du token à une clé) est à considérer en durcissement.
5. **Le serveur MCP (F-052) est un *resource server*** : il valide les tokens OAuth et applique les scopes, sans jamais accorder plus de droits que l'utilisateur (OWASP A01, doc 04).

## Rationale

- **Le bon outil pour le bon problème.** OAuth 2.1 est *fait* pour la délégation à un tiers (consentement, scopes, révocation) ; le JWT custom était *fait* pour le login first-party. On ne corrige pas l'ADR-010, on le complète.
- **Interopérabilité.** Sans OAuth 2.1, Atlas ne serait pas consommable par les clients MCP standard (Claude et autres). C'est la condition d'entrée dans l'écosystème agentique.
- **Fondation réutilisable.** La même couche servira à sécuriser l'**API publique** (F-028) pour les intégrateurs tiers — l'effort n'est pas dédié au seul MCP.
- **Cohérence open source / souveraine.** OpenIddict évite à la fois la **facture de licence** (Duende) et le **serveur externe lourd** (Keycloak, déjà écarté ADR-010). Le contrôle reste total et auto-hébergeable.

## Conséquences

- **Positives** : accès agentique conforme aux standards, avec délégation **scopée, consentie, révocable** ; interopérabilité immédiate avec les clients MCP du marché ; socle d'autorisation réutilisable pour l'API publique ; posture de sécurité défendable (auditable, standard), 100 % open source et souveraine.
- **Négatives** : **chantier significatif et critique en sécurité** (un serveur d'autorisation mal implémenté est une faille majeure) ; nouvelles pièces mobiles (endpoints OAuth, registre de clients, écran de consentement, taxonomie de scopes) ; **dualité temporaire** JWT first-party + OAuth tiers ; OpenIddict n'a **pas d'UI d'admin** prête à l'emploi (davantage de câblage à notre charge).
- **À prévoir** :
  - Définir la **taxonomie des scopes** (ex. `atlas.companies.read`, `atlas.favorites.write`, `atlas.veille.read`…), alignée sur les outils MCP de F-052.
  - **PKCE obligatoire**, resource indicators, durées de vie courtes, rotation + **révocation** des refresh (réutiliser la blacklist Redis anticipée par l'ADR-010).
  - **Enregistrement des clients** MCP (politique d'enregistrement, éventuellement dynamique) + **écran de consentement**.
  - **Découverte** du serveur d'autorisation (métadonnées) pour les clients.
  - Étudier **DPoP** pour la liaison de token.
  - **Modèle de menace** dédié (vol de token, escalade de scope, injection via contenu) + extension du projet de tests sécurité existant.
  - Trancher **si / quand** migrer les clients first-party vers OAuth.
  - Mettre à jour `docs/11-api-endpoints.md` avec les endpoints OAuth (`/connect/*`) une fois implémentés.
  - **Timing** : réalisé pendant F-052 ; aucune action avant.
