# ADR-007 — Stack technique .NET / MAUI

**Statut** : ✅ Accepté **— amendé par [ADR-026](ADR-026-clients-natifs-maui-avalonia.md) (31 mai 2026)**
**Date** : 26 mai 2026

> **Amendement (ADR-026, 31 mai 2026)** — Le client natif n'est plus « MAUI pour iOS/Android/Windows/macOS ». Désormais : **MAUI pour le mobile (Android/iOS)** ; **Avalonia pour le desktop (Windows/macOS/Linux)** — pour couvrir Linux face à la bascule souveraine DINUM. Le web (Blazor WASM, ADR-017) est inchangé. Cf. ADR-026 pour le détail et la requalification de F-010.

## Contexte

Le choix de la stack technique est contraint par le cursus actuel (MAUI étudié) et l'envie d'un écosystème pro et stable.

## Décision

**Backend** :
- **.NET 10 LTS** (version courante stable depuis novembre 2025, supportée jusqu'à novembre 2028). Migration possible vers .NET 11 STS à sa sortie si bénéfices techniques justifient le passage à un cycle STS.
- **ASP.NET Core** (Minimal APIs pour la simplicité, ou MVC controllers selon préférence).
- **Entity Framework Core** pour la persistance, avec **PostgreSQL** comme SGBD (open source, mature, supporté partout).
- **MediatR** ou pattern handler manuel pour les use cases (à décider en phase architecture détaillée).
- **FluentValidation** pour les validations métier.
- **Polly** pour la résilience HTTP (retry, circuit breaker, timeouts) sur les appels INPI.
- **Serilog** + **OpenTelemetry** pour le logging et l'observabilité.

**Clients** :
- **.NET MAUI** pour iOS / Android / Windows / macOS.
- **Blazor Web App** (WASM pur pour l'app authentifiée) pour le web — *tranché par ADR-017*.

**Outillage** :
- **xUnit** pour les tests unitaires.
- **Testcontainers** pour les tests d'intégration avec PostgreSQL réel.
- **NetArchTest** ou **ArchUnitNET** pour les tests d'architecture (vérifier que les dépendances respectent l'archi hex).
- **GitHub Actions** ou **GitLab CI** pour le CI/CD (décision : voir doc séparée sur le repo).

## Rationale

- **Cohérence** : tout .NET, une seule stack à maîtriser pour le porteur du projet.
- **Maturité** : ASP.NET Core et MAUI sont stables, supportés long terme par Microsoft.
- **Open source** : tout cet écosystème est open source et utilisable librement.
- **PostgreSQL** : meilleur SGBD open source, supporte le JSON natif (utile pour stocker les réponses brutes INPI), gratuit même en production.

## Conséquences

- **Positives** : productivité maximale, écosystème riche, pas de fragmentation langages.
- **Négatives** : moins d'attrait pour certains contributeurs open source (la communauté .NET est plus restreinte que JS/Python).
- **À prévoir** : documentation onboarding contributeurs pour faciliter l'arrivée de devs non-.NET.
