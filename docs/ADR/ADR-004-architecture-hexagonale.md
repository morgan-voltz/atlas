# ADR-004 — Architecture hexagonale platform-ready

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

Trois ambitions d'architecture étaient discutées :
- **A. Focused INPI v1** : on code pour INPI uniquement, on étend plus tard.
- **B. Platform-ready** : on architecture comme si on allait brancher 10 sources, on n'en implémente qu'une au début.
- **C. Multi-sources dès la v1** : on implémente INPI + BODACC + Sirene dès le premier livrable.

## Décision

**Option B : Platform-ready, INPI seul en v1**.

L'architecture suit le pattern **Ports & Adapters / Clean Architecture** :
- Le **Core** définit des **ports** (interfaces) en termes métier (ex. `ICompanyDataProvider`, `IIntellectualPropertyProvider`).
- L'**Infrastructure** contient des **adapters** qui implémentent ces ports avec une source spécifique (ex. `InpiRneCompanyDataProvider`).
- Le **domaine ne connaît pas l'INPI**. Il connaît la notion de `Company`, `LegalEntity`, `Trademark`, etc.

## Rationale

- **Effort initial faible** : un port bien défini ne coûte pas plus cher qu'un service couplé à l'INPI.
- **Évolution massive facilitée** : ajouter BODACC, Sirene ou EUIPO = écrire un nouvel adapter, **zéro modification du domaine ou des use cases**.
- **Testabilité** : les use cases sont testables avec des mocks des ports, sans appeler l'INPI.
- **Substitution** : si l'INPI change radicalement son API, on remplace l'adapter sans toucher au reste.
- **L'option A** sous-utilise la puissance de l'archi hex. **L'option C** est trop ambitieuse pour un projet porté par une personne en parallèle des études.

## Conséquences

- **Positives** : flexibilité, testabilité, séparation claire des responsabilités, faible couplage.
- **Négatives** : courbe d'apprentissage initiale plus raide. Risque de sur-ingénierie si on perd la mesure (pas besoin de 4 niveaux d'abstraction pour des cas simples).
- **À prévoir** : règles de dépendance strictes (le domaine ne dépend de RIEN ; l'application dépend du domaine ; l'infrastructure dépend du domaine et de l'application). Outillage : tests d'architecture (NetArchTest, ArchUnitNET).
