# ADR-002 — Topologie hybride

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

Trois topologies étaient en lice :
- **A. Tout local** : MAUI consomme une lib partagée qui tape directement les API externes.
- **B. Backend séparé strict** : ASP.NET Core héberge toute la logique, MAUI / Blazor sont des clients HTTP purs.
- **C. Hybride** : un projet `Core` partagé entre backend et clients pour le domaine pur (entités, value objects, validations), avec backend séparé pour la logique métier sensible et les appels API externes.

## Décision

**Topologie hybride (C)** :
- Un projet `Core` (ou `Domain`) contient les entités, value objects, validations métier purs.
- Cette lib est **partagée** entre le backend ASP.NET Core et le client MAUI.
- Toute la logique d'orchestration, d'appel aux APIs INPI/externes, et de persistance vit **uniquement dans le backend**.
- Les clients (MAUI, futur Blazor) consomment le backend via une API REST authentifiée.

## Rationale

- **L'option A est incompatible** avec le multi-utilisateur SaaS (ADR-001) : pas de partage de données entre devices, pas de tâches asynchrones, et credentials INPI dans le binaire MAUI = fuite assurée.
- **L'option B fonctionne** mais duplique souvent les entités entre backend (DTOs) et client (modèles UI), créant de la dette technique.
- **L'option C** offre un compromis idéal pour le contexte .NET : un seul vocabulaire métier réutilisable, validations cohérentes côté client et serveur, sans compromettre la sécurité (les use cases sensibles restent côté serveur).

## Conséquences

- **Positives** : DRY sur le domaine, validations cohérentes, un seul endroit pour les règles métier de base, possibilité d'ajouter d'autres clients (CLI, Blazor) sans refactor.
- **Négatives** : discipline nécessaire pour ne **PAS** mettre de logique dépendant d'infrastructure dans `Core`. Risque de "leak" si on n'est pas vigilant.
- **Règle stricte** : `Core` n'a **AUCUNE référence** à `Microsoft.AspNetCore.*`, `EntityFramework`, `HttpClient`, etc. C'est du C# pur, testable sans I/O.
