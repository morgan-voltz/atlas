# Audit profond Atlas — Synthèse globale

> Audit **lecture seule** de toute la solution, mené couche par couche du cœur vers l'extérieur. Aucun code de production modifié. Date : 2026-05-28.
> Sévérités : 🔴 P0 (à corriger) · 🟠 P1 (recommandé) · 🟡 P2 (cosmétique / faible ROI).

## Index des rapports

| # | Partie | Rapport |
|---|---|---|
| 1 | `Atlas.Shared` | [01-shared.md](01-shared.md) |
| 2 | `Atlas.Domain` | [02-domain.md](02-domain.md) |
| 3 | `Atlas.Application` | [03-application.md](03-application.md) |
| 4a | `Infrastructure.Persistence` | [04a-persistence.md](04a-persistence.md) |
| 4b | `Infrastructure.Veille/Inpi/Security/Messaging` | [04b-infra-adapters.md](04b-infra-adapters.md) |
| 5 | `Atlas.Api` | [05-api.md](05-api.md) |
| 6 | Tests (transverse) | [06-tests.md](06-tests.md) |
| 7 | `Atlas.Maui` | [07-maui.md](07-maui.md) |

## Verdict global

**Architecture très saine, dette concentrée sur la mise en production et le filet de sécurité.**

Atlas est un projet **rigoureusement structuré** : hexagone respecté à toutes les couches (pureté du Domain exemplaire, identifiants fortement typés, encapsulation, `Result<T>` au lieu d'exceptions, conventions homogènes, value-object conversions EF, crypto correcte). **Aucune dérive d'architecture majeure.**

Les problèmes sérieux ne sont **pas** structurels : ce sont **(a)** un bug de **conformité RGPD** (effacement incomplet), **(b)** l'**absence de CI** (rien n'exécute les tests/règles d'archi), et **(c)** un ensemble cohérent de **durcissements sécurité pré-production** (clés/KMS, SSRF, garde-fous HTTP) et de **trous de tests ciblés** (crypto, sécurité, veille). Tout est corrigeable **sans refactoring de masse**.

## Les 2 P0 (à corriger en priorité)

| # | Partie | Constat | Impact |
|---|---|---|---|
| **P0-1** | 4a | Suppression de compte n'efface **pas** les données de veille (`veille_subscription`, `veille_pack_enrollment`, `feed_item_user_state` sans FK `user_id` ; `ExecuteDelete` ne cascade qu'au niveau DB). | **RGPD art. 17** : données personnelles orphelines persistées. Le commentaire du handler affirme l'inverse. |
| **P0-2** | 6 | **Aucune CI** n'exécute build + tests + tests d'architecture (seul `docs.yml` existe). | DoD « le build CI passe » **non applicable** ; aurait dû attraper P0-1 et toute régression. Méta-contrôle absent. |

> Les deux sont liés : une CI exécutant un test de cascade aurait révélé P0-1.

## P1 — par thème

**Sécurité pré-production (4b, 5)**
- **SSRF** : flux RSS fournis par l'utilisateur (F-043) sans blocage des IP privées/loopback/métadonnées cloud (seule une blocklist d'hôtes existe).
- **Clés non sûres sans garde** : fallback AES `SHA256("atlas-dev-insecure-crypto-key")` et clé RSA **éphémère** si non configurées ; **pas de KMS / rotation / JWKS**. → échouer au démarrage hors Development si clés absentes.
- **Durcissement HTTP** : pas de **rate limiting**, **HSTS**, en-têtes de sécurité, **CORS**, logging structuré.

**Conformité & correctness (5)**
- **Erreurs `veille.*` non mappées** → toutes en 400 au lieu de 404/409/403/502.

**Filet de sécurité / tests (6, 4b, 2, 3)**
- **Code crypto sans tests directs** (créer `Security.UnitTests`).
- **Tests d'architecture partiels** : ne gardent ni `Shared`, ni **`Maui`** (règle anti-décompilation critique), ni les autres `Infrastructure.*`, ni `Premium`, ni les conventions.
- **Trous de couverture sécurité** : `TwoFactorRecoveryCode` (Domain), handlers `RefreshToken`/`Logout`/`Setup/DisableTwoFactor`/`VerifyEmail` (App) ; validators jamais testés ; **veille non testée au niveau API**.

**Domain / Shared (1, 2)**
- **Domain events morts** : `UserRegisteredDomainEvent` levé mais aucun dispatcher ne le publie → câbler ou retirer.
- **`Result` sans combinateurs** (Map/Bind/Match) + `Value` silencieux sur échec → boilerplate `.Value!` dans toute la solution.
- **Couverture nulle** du kernel `Shared`.

**Persistence (4a)**
- **Aucun `AsNoTracking()`** sur les lectures ; tests d'intégration 6/12 repos.

**Maui (7)**
- **Accessibilité absente** (DoD bloquante) ; **build iOS/MacCatalyst cassé** (CA1711 + warnings-as-errors).

## P2 (sélection)

Mapping `Error→HTTP` non typé (switch string) · cible `netstandard2.1` du kernel · `Result.cs`/`IJwtIssuer.cs` = plusieurs types/fichier · `Siret` documenté non implémenté · concurrence optimiste absente · résilience Npgsql/Polly/429 · pas de limite de taille de flux · XXE CodeHollow non vérifié · `2FA challenge` utilise `DateTime.UtcNow` · **Brevo non implémenté** (stub dev) · `/feed/items` redondant · `register` 200 au lieu de 201 · OpenAPI sans métadonnées · 10 dossiers `.gitkeep` vides · base URL Maui en dur · couverture non mesurée (coverlet inutilisé) · tests d'intégration non catégorisés (Docker).

## Feuille de route de remédiation suggérée

| Lot | Objectif | Contenu | Sévérité |
|---|---|---|---|
| **0 — Fondation** | Protéger `main` | CI GitHub Actions : build (warnings-as-errors) + `dotnet test` (avec Docker pour l'intégration) + tests d'archi, obligatoire sur PR | 🔴 P0-2 |
| **1 — Conformité** | RGPD + correctness | FK `user_id ON DELETE CASCADE` sur les 3 tables veille + **test de cascade** ; mapper les codes `veille.*` dans `ErrorHttpMapping` | 🔴 P0-1, 🟠 |
| **2 — Sécurité prod** | Durcir avant exposition | Garde « fail si clés absentes hors Dev » + plan KMS ; protection SSRF (rejet IP privées) ; rate limiting + HSTS + en-têtes + CORS + logging ; `Security.UnitTests` (crypto) | 🟠 |
| **3 — Filet & archi** | Élargir la garde | Étendre NetArchTest (**Maui** d'abord, Shared, Infra.*, Premium) ; tests handlers sécurité + 2FA recovery ; `AsNoTracking` + tests persistence manquants | 🟠 |
| **4 — Dette structurée** | Ergonomie & cohérence | Combinateurs `Result` + `Value` guard ; un type/fichier (`Result.cs`, `IJwtIssuer.cs`) ; domain events (câbler ou retirer) ; build Maui iOS/Mac ; nettoyage `.gitkeep` | 🟠/🟡 |
| **5 — Pré-prod produit** | Aller en prod | Accessibilité Maui ; adapter Brevo ; base URL configurable ; concurrence optimiste ; mesure de couverture | 🟡 |

## Bilan chiffré (santé par partie)

| Partie | Verdict | P0 | P1 | P2 |
|---|---|---|---|---|
| Shared | Bonne | 0 | 4 | 5 |
| Domain | **Excellente** | 0 | 3 | 4 |
| Application | Très bonne | 0 | 2 | 5 |
| Persistence | Bonne (1 défaut RGPD) | 1 | 2 | 4 |
| Infra adapters | Crypto excellente, durcissement à finir | 0 | 3 | 6 |
| Api | Bonne structure, HTTP à durcir | 0 | 3 | 3 |
| Tests | Fondations solides, CI absente | 1 | 3 | 3 |
| Maui | Bon socle, accessibilité + build | 0 | 2 | 2 |

**Conclusion** : le socle est de grande qualité ; l'effort de remédiation est **circonscrit et séquençable** (commencer par la CI, puis RGPD, puis le durcissement sécurité), sans remise en cause de l'architecture.
