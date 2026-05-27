# Audit profond Atlas — Partie 6 : Tests (transverse)

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-28.
> Synthèse transverse : consolide les constats de couverture des parties 1→5.

## Contexte de la partie

7 projets de test :
- **Unitaires** : `Atlas.Domain.UnitTests` (~65 tests), `Atlas.Application.UnitTests` (~67).
- **Architecture** : `Atlas.Architecture.Tests` (NetArchTest, 4 tests).
- **Intégration** : `Atlas.Api.IntegrationTests` (WebApplicationFactory + Testcontainers + WireMock, 2 méthodes), `Atlas.Infrastructure.Persistence.IntegrationTests` (~7), `Atlas.Infrastructure.Veille.IntegrationTests` (~4, WireMock), `Atlas.Infrastructure.Inpi.IntegrationTests` (~13, WireMock).
- **Manuel/semi-auto** : collection **Bruno** (54 scénarios : auth, 2FA, INPI, companies, trademarks, RGPD, lockout, flows e2e).

Stack : xUnit, FluentAssertions **7.0.0** (dernière version sous licence libre — choix pertinent, évite la licence commerciale de la v8), NSubstitute, Testcontainers (PostgreSQL 16), WireMock.Net, NetArchTest, coverlet.collector.

**Projets de test absents** : `Atlas.Shared.UnitTests`, `Atlas.Infrastructure.Security.*`, `Atlas.Infrastructure.Messaging.*`.

## Verdict global

**Santé : fondations de qualité, mais protection insuffisante.** L'outillage est excellent (BDD réelle via Testcontainers, mocks HTTP déterministes via WireMock, large collection Bruno, NetArchTest, conventions homogènes). Trois problèmes structurels limitent fortement la valeur de protection : **(1) aucune CI n'exécute ces tests**, **(2) le code de sécurité (crypto) n'a aucun test direct**, **(3) les tests d'architecture ne couvrent qu'une fraction des règles**.

## 🔴 P0 — Aucune CI n'exécute build + tests + tests d'architecture

`.github/workflows/` ne contient que `docs.yml`. Il **n'existe aucun pipeline** qui, sur push/PR, lance `dotnet build` + `dotnet test` + les tests d'architecture. Or :
- La **Definition of Done** (CLAUDE.md) stipule « Le build CI passe entièrement, y compris les tests d'architecture » → **exigence non applicable** aujourd'hui.
- C'est le **méta-contrôle** qui aurait dû attraper le P0 RGPD (Partie 4a) et toute violation d'archi.
- Rien n'empêche un merge qui casse les tests ou enfreint les règles de dépendance.
**Reco** : ajouter un workflow GitHub Actions (`.github/workflows/ci.yml`) : restore/build (warnings-as-errors déjà actifs), `dotnet test` sur **tous** les projets (avec un service/daemon Docker pour les tests d'intégration Testcontainers), collecte de couverture (coverlet), exécution obligatoire sur PR vers `main`. C'est le correctif à plus fort levier de tout l'audit.

## Constats & recommandations

### 🟠 P1 — Code de sécurité sans tests directs ; 2 projets sans aucun test
Aucun `Atlas.Infrastructure.Security.*` ni `Atlas.Infrastructure.Messaging.*`. Argon2id, AES-256-GCM, JWT, TOTP, génération de tokens ne sont **testés qu'indirectement** (mocks dans les handlers / E2E). Pour du code cryptographique, c'est le trou le plus risqué (cf. Partie 4b).
**Reco** : créer `Atlas.Infrastructure.Security.UnitTests` : Argon2 (hash/verify, mauvais mdp, format corrompu), AES-GCM (round-trip + **détection d'altération**), JWT (claims/expiry/signature/issuer/audience), 2FA challenge (expiry + isolation d'audience), TOTP (fenêtre ±1).

### 🟠 P1 — Tests d'architecture : couverture partielle des règles d'or
`DependencyRuleTests.cs` (4 tests) vérifie : Domain ⊀ App/Infra/Api ; Domain ⊀ EF/AspNet/MediatR/Npgsql ; Application ⊀ Infra/Api ; Persistence ⊀ Api. **Non couvert** :
- **`Atlas.Shared` ne dépend de rien** (kernel).
- **`Atlas.Maui` ne référence QUE Domain + Shared** — règle critique (CLAUDE.md « cas particulier » : risque de décompilation si l'infra est embarquée) — **non enforcée**.
- Les 4 autres `Infrastructure.*` (Veille/Inpi/Security/Messaging) ⊀ Api et ⊀ entre elles.
- `Application.Premium` → Application/Domain/Shared uniquement.
- Règles de **convention** (handlers `internal sealed`, « aucune entité domaine renvoyée par l'API »).
**Reco** : étendre NetArchTest pour couvrir Shared, Maui (prioritaire), tous les Infrastructure.*, Premium, + 1-2 règles de convention.

### 🟠 P1 — Trous de couverture consolidés (parties 2→5)
- **Domain ~46 %** : IntellectualProperty **0/6**, `TwoFactorRecoveryCode` (sécurité) et `Account` non testés, VOs Companies.
- **Application** : handlers **sécurité** non testés (`RefreshToken`, `Logout`, `Setup/DisableTwoFactor`, `VerifyEmail`) ; **validators jamais testés directement**.
- **Persistence** : 6/12 repos peu/pas couverts (VeillePack/2FA à 0), owned-collection + `GetByIdsAsync` (`Contains`) + contraintes uniques non testés ; **cascade RGPD non testée sur les tables veille** (lié au P0 Partie 4a).
- **API** : la **veille (10 endpoints) n'est testée nulle part** (ni C# ni Bruno) ; statuts d'erreur (404/409/423/502) assertés seulement en Bruno.
**Reco** : prioriser sécurité (2FA recovery, handlers auth) → cascade RGPD veille (test qui aurait attrapé le P0) → veille API → IP/Companies VOs.

### 🟡 P2 — Pas de mesure ni de cible de couverture
`coverlet.collector` est présent mais **aucune collecte n'est routinière** (pas de runsettings, pas de rapport, pas de seuil — la DoD assume « pas de seuil imposé »). On pilote la couverture « à l'œil ».
**Reco** : produire un rapport de couverture en CI (informatif, sans gate bloquant au début) pour objectiver les trous.

### 🟡 P2 — Dépendance Docker des tests d'intégration non isolée
4 projets d'intégration nécessitent **Docker** (Testcontainers). Sans daemon (cf. cette session), ils échouent à l'init et sont de facto **sautés en local**. Aucune séparation explicite « suite rapide (unit) » vs « suite Docker (intégration) ».
**Reco** : catégoriser (`[Trait("Category","Integration")]`) pour permettre `dotnet test --filter Category!=Integration` en local rapide, et lancer la suite complète en CI (avec Docker).

### 🟡 P2 — Bruno : précieux mais hors automatisation, et sans veille
54 scénarios Bruno couvrent largement auth/2FA/RGPD/lockout/INPI — mais c'est **manuel/semi-automatisé**, hors de tout gate, et **aucun scénario veille**.
**Reco** : porter les scénarios de sécurité Bruno clés (lockout, JWT invalide, RGPD export/delete) en tests d'intégration C# automatisés ; ajouter des scénarios veille (Bruno et/ou C#).

## Points forts (à préserver)

- ✅ **Fidélité** : PostgreSQL **réel** via Testcontainers (pas d'in-memory), INPI/RSS mockés via **WireMock** (déterministe, pas de réseau réel).
- ✅ **Tests domaine profonds** là où présents : Luhn (`Siren`), machines à états (`User`, 2FA, `FeedSource`), dédup (`FeedItem`).
- ✅ **NetArchTest** garde les règles de dépendance cœur (à étendre).
- ✅ Conventions homogènes (xUnit, FluentAssertions, NSubstitute, AAA, nommage `Methode_scenario_resultat`, builders).
- ✅ Collection **Bruno** riche (flows e2e, RGPD, lockout, 2FA TOTP).
- ✅ FluentAssertions **épinglé en v7** (dernière version libre) — évite la licence commerciale v8.

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🔴 P0 | Aucune CI n'exécute build/tests/archi (DoD non applicable) | M |
| 2 | 🟠 P1 | Code crypto/sécurité sans tests directs (créer Security.UnitTests) | M |
| 3 | 🟠 P1 | Tests d'archi partiels (Shared, **Maui**, Infra.*, Premium, conventions) | S |
| 4 | 🟠 P1 | Trous de couverture : sécurité Domain/App, cascade RGPD veille, veille API | L |
| 5 | 🟡 P2 | Pas de mesure/rapport de couverture | S |
| 6 | 🟡 P2 | Tests d'intégration non catégorisés (dépendance Docker) | S |
| 7 | 🟡 P2 | Bruno hors automatisation + sans veille | M |

## Note transverse

Le P0 « CI absente » est la **fondation** : il conditionne l'efficacité de tous les autres correctifs (rien n'est protégé tant qu'aucun pipeline n'exécute la suite). À traiter en premier dans tout plan de remédiation.
