# Audit profond Atlas — Partie 2 : `Atlas.Domain`

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-27.

## Contexte de la partie

Le cœur de l'hexagone. ~79 types publics répartis en `Common`, `Users`, `Companies`, `IntellectualProperty`, `Inpi`, `Veille`, `Search`, `Security`, `Notifications`. Ne référence que `Atlas.Shared`. `InternalsVisibleTo` → `Atlas.Infrastructure.Persistence` (réhydratation EF).

## Verdict global

**Santé : excellente — c'est la couche la plus solide de la solution.** Pureté hexagonale impeccable, identifiants fortement typés cohérents, encapsulation rigoureuse, invariants dans les entités, temps injecté (testabilité), erreurs métier via `Result<T>` (zéro exception métier). Les points d'attention sont **ciblés** : une machinerie d'événements morte, des trous de couverture sur certaines zones (IP, Companies, sécurité 2FA), et un écart mineur de structure.

## Points forts (à préserver)

- ✅ **Pureté** : aucune dépendance interdite. Pas de `HttpClient`/`DbContext`/`Console`/EF/Npgsql/ASP.NET. Aucun `DateTime.UtcNow` direct — le temps est **toujours injecté** (`DateTimeOffset now` en paramètre) → entités testables de façon déterministe.
- ✅ **Identifiants fortement typés** : 12 `readonly record struct *Id`, tous avec `New()` + `ToString()`, jamais de `Guid`/`string` nu dans les signatures.
- ✅ **Encapsulation** : toutes les entités dérivent de `Entity<TId>`, setters `private`, aucune entité anémique, collections exposées en `IReadOnlyCollection` (ex. `VeillePack.Items` muté seulement via `SetSources`). Constructeurs EF `private`.
- ✅ **Value objects validés** via factory `Result<T>` : `Siren` (Luhn), `EmailAddress` (RFC + normalisation). `PasswordHash` lève (acceptable : précondition d'infra, pas une validation métier).
- ✅ 100% file-scoped namespaces. Convention `domaine.snake_case` pour les codes d'erreur.
- ✅ `InpiCredentials` n'expose jamais le clair (aligné sécurité/RGPD).

## Constats & recommandations

### 🟠 P1 — Machinerie de domain events morte (levés, jamais dispatchés)
`Entity<TId>` expose `RaiseDomainEvent`/`DomainEvents`/`ClearDomainEvents` (`Common/Entity.cs:14-16`). Mais :
- Un **seul** événement est levé dans toute la solution : `UserRegisteredDomainEvent` (`Users/User.cs:69`).
- **Aucun dispatcher** ne le publie (pas d'`IDomainEventDispatcher`, aucun `Publish` dans Application/Infrastructure).
- `ClearDomainEvents` n'est **jamais** appelé.
- Toutes les configs EF font `builder.Ignore(... .DomainEvents)` (13 occurrences) → la collection est accumulée puis jetée.

→ Code **trompeur** : on croit qu'un effet de bord est câblé sur l'inscription, mais l'événement part à la poubelle (l'e-mail de vérification est en réalité géré impérativement dans le handler `RegisterUser`).
**Reco** : trancher — soit **câbler un dispatcher** (publier les `DomainEvents` via MediatR au `SaveChangesAsync` de l'`IUnitOfWork`, puis `ClearDomainEvents`), soit **retirer l'événement + la machinerie** tant qu'inutilisée (YAGNI). Ne pas laisser en l'état.

### 🟠 P1 — Couverture de test ~46% des types métier (trous ciblés)
16/35 types métier testés. Forte sur **Users** (machines à états `User`/2FA bien couvertes) et **Veille** (entités). Trous notables, par risque décroissant :
- 🔒 **`TwoFactorRecoveryCode` non testé** — sécurité : codes de récupération à usage unique, idempotence de `MarkUsed`, `IsUsed`. **Priorité haute.**
- **`IntellectualProperty` : 0/6** — au moins `NiceClassification` (devrait valider 1–45) et `DepositNumber` méritent des tests de format.
- **`Account` non testé** (logique faible mais zéro test).
- **Companies** : seul `Siren` testé ; `Address`/`Naf`/`Dirigeant` non couverts.
- `User.RegisterFailedLogin` : tester précisément la **borne `maxAttempts`** et la durée de lockout.
**Reco** : prioriser `TwoFactorRecoveryCode` (sécurité) → VOs IP → `Account` → VOs Companies. (Les read-models nus type `TimelineEntry`/`CompanySummary` n'ont pas besoin de tests.)

### 🟠 P1 — `IJwtIssuer.cs` déclare 2 types publics → viole « un type par fichier »
`Security/IJwtIssuer.cs:5,10` contient `AccessToken` (record) **et** `IJwtIssuer` (interface), contre `docs/08 §2.3`. Même motif que `Result.cs` (Partie 1).
**Reco** : extraire `AccessToken.cs`. Trivial.

### 🟡 P2 — Duplication de validation Domain ↔ Application
La limite 254 de l'e-mail est dans `EmailAddress.Create` **et** `RegisterUserValidator` ; la validation d'URL est dans `FeedSource.Create` **et** `AddUserFeedSourceValidator`. Acceptable (defense-in-depth / fast-fail côté API), mais **risque de divergence** si une borne change d'un seul côté.
**Reco** : exposer la constante/règle depuis le Domain (ex. `EmailAddress.MaxLength`, déjà `FeedSource.MaxUrlLength`) et la réutiliser dans les validators pour une source unique de vérité.

### 🟡 P2 — `Siret` documenté mais non implémenté
`docs/08 §3.2` et CLAUDE.md listent `Siret` comme value object, mais seul `Siren` existe dans le code. Écart vocabulaire ↔ code.
**Reco** : l'implémenter quand le besoin Siret arrive (avec Luhn 14), ou noter explicitement le report dans le glossaire pour éviter la confusion.

### 🟡 P2 — Validation absente sur quelques value objects / queries
`NiceClassification` (pas de borne 1–45), `CompanySearchQuery`/`TrademarkSearchQuery` (page/pageSize ≥ 1 non garantis au niveau du type), `TimelineFilter` (`PublishedAfter ≤ PublishedBefore` non vérifié). Aujourd'hui la validation est soit ailleurs (validators Application), soit absente.
**Reco** : décider au cas par cas — soit valider dans le type (factory `Result<T>`), soit documenter que c'est délégué à l'Application.

### 🟡 P2 — Deux idiomes de value object coexistent
Une classe de base `Common/ValueObject.cs` (égalité par `GetEqualityComponents`) coexiste avec l'usage massif de `record struct`/`record` (égalité structurelle native). À confirmer : standardiser sur un seul idiome pour la cohérence, ou documenter quand utiliser lequel.

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | Domain events morts (dispatcher absent) → câbler ou retirer | M |
| 2 | 🟠 P1 | Couverture ~46% : trous sécurité (TwoFactorRecoveryCode) + IP (0/6) + Companies | M |
| 3 | 🟠 P1 | `IJwtIssuer.cs` = 2 types publics | XS |
| 4 | 🟡 P2 | Duplication validation Domain↔Application (email 254, URL) | S |
| 5 | 🟡 P2 | `Siret` documenté mais non implémenté | S |
| 6 | 🟡 P2 | Validation absente (NiceClassification, queries, TimelineFilter) | S |
| 7 | 🟡 P2 | Deux idiomes de value object (base `ValueObject` vs `record struct`) | S |

## Note transverse (pour l'audit *Tests*)

La couverture Domain par zone : Users 75% · Veille 55% · Search 50% · Inpi 33% · Companies 17% · **IntellectualProperty 0%**. À recouper avec un rapport de couverture chiffré (coverlet) lors de l'audit de la partie *Tests*.
