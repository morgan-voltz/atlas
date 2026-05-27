# Audit profond Atlas — Partie 1 : `Atlas.Shared`

> Rapport **lecture seule** (aucun code de production modifié). Audit mené du cœur vers l'extérieur : **Shared** → Domain → Application → Infrastructure.* → Api → Tests → Maui.
> Sévérités : 🔴 P0 (à corriger) · 🟠 P1 (recommandé) · 🟡 P2 (cosmétique / faible ROI).
> Date : 2026-05-27.

## Contexte de la partie

`Atlas.Shared` est le **kernel** de l'hexagone : référencé par tous les projets, ne référence rien. Surface réduite (3 fichiers utiles) :
- `Result/Result.cs` → `Error` (abstract record), `Result` (sealed record), `Result<T>` (sealed record)
- `Result/PagedResult.cs` → `PagedResult<T>` (sealed record, + `TotalPages`/`HasPreviousPage`/`HasNextPage` calculés)
- `Common/IsExternalInit.cs` → polyfill records pour `netstandard2.1`

`DomainError` (`Atlas.Domain/Common/DomainError.cs`) hérite de `Error`. Le mapping HTTP se fait dans `Atlas.Api/Endpoints/ErrorHttpMapping.cs` (`ToProblem()`).

## Verdict global

**Santé : bonne.** Kernel minimal, immuable, sans dépendance — la pureté hexagonale est respectée. Rien de critique. Les gains se concentrent sur **(1) la couverture de test (nulle aujourd'hui)**, **(2) l'ergonomie de `Result`** (génère du boilerplate dans toute la solution), et **(3) deux écarts de structure/convention**.

## Points forts (à préserver)

- ✅ **Zéro dépendance** : `.csproj` sans `ProjectReference`/`PackageReference` → règle d'or du kernel respectée.
- ✅ Types **immuables, `sealed`**, `Items` en `IReadOnlyList<T>`.
- ✅ `PagedResult.TotalPages` protège la division par zéro (`PageSize == 0 ? 0 : …`).
- ✅ Convention de code d'erreur homogène (`domaine.snake_case`, ex. `users.email_already_in_use`).
- ✅ `Nullable` activé.

## Constats & recommandations

### 🟠 P1 — Couverture de test : nulle sur le kernel
Aucun test direct de `Result`, `Result<T>`, `PagedResult<T>`. Couverture seulement **indirecte** via les handlers. Or `PagedResult` porte de l'arithmétique non triviale (arrondi `TotalPages`, bornes `HasNextPage`/`HasPreviousPage`, cas `PageSize == 0`) et `Result` a une sémantique à épingler (cf. constat sur `Value`).
**Reco** : créer `tests/Atlas.Shared.UnitTests` couvrant : math de `PagedResult` (arrondi, première/dernière page, `PageSize=0`), comportement `Ok`/`Fail`/`Value`/`Error`, égalité des records.

### 🟠 P1 — Pas de combinateurs sur `Result` → boilerplate + `.Value!` partout
`Result`/`Result<T>` n'exposent que `IsSuccess`/`IsFailure`/`Value`/`Error` + `Ok`/`Fail`. Conséquences :
- Le motif `if (x.IsFailure) return Result<Y>.Fail(x.Error!);` se répète (jusqu'à 3× dans un même handler, ex. `src/Atlas.Application/Companies/.../GetCompanyBySirenHandler.cs:20-24`).
- `.Value!` (null-forgiving) apparaît ~50× après un check `IsSuccess` — corrélation non garantie par le compilateur.
**Reco** : ajouter des combinateurs (`Map`, `Bind`, `Match`, `Ensure`, `Tap`) et des **conversions implicites `Error → Result`/`Result<T>`** (permet `return error;`). Purement additif (non cassant), réduit le boilerplate et les `.Value!` dans toute la solution. Effort moyen, fort effet de levier.

### 🟠 P1 — `Result<T>.Value` renvoie `default` silencieusement sur échec
`Fail` initialise `Value` à `default(T)` ; accéder à `.Value` sur un résultat en échec ne lève pas — footgun (un `.Value!` mal placé renvoie `null`/`0` au lieu d'échouer bruyamment).
**Reco** : soit lever sur accès à `Value` quand `IsFailure`, soit exposer `TryGetValue(out T)` / `Match`. À coupler avec les combinateurs pour supprimer les `.Value!`.

### 🟠 P1 — `Result.cs` contient 3 types publics → viole « un type par fichier »
`Error`, `Result`, `Result<T>` cohabitent dans `Result.cs`, alors que `docs/08 §2.3` impose **un type par fichier, nom de fichier = nom du type**. Auto-incohérence du référentiel.
**Reco** : extraire `Error` dans `Error.cs` (et idéalement `Result<T>` dans son fichier). Effort trivial.

### 🟠 P1 — Aucun test d'archi n'enforce la pureté du kernel
`tests/Atlas.Architecture.Tests/DependencyRuleTests.cs` couvre Domain, Application et Persistence→Api, mais **rien n'asserte que `Atlas.Shared` ne dépend de rien**. (Plus largement : `Application.Premium`, les autres `Infrastructure.*` (Veille/Inpi/Security/Messaging) et `Maui` ne sont pas couverts non plus — à traiter dans l'audit *Tests*.)
**Reco** : ajouter un test « `Atlas.Shared` n'a aucune dépendance projet/infra ». Garde-fou peu coûteux.

### 🟡 P2 — Mapping `Error → HTTP` non typé (switch de ~23 codes string)
`ErrorHttpMapping.ToProblem()` mappe via un `switch` sur `error.Code` (string) ; tout nouveau code non listé retombe en **400** silencieusement. Risque de mauvais statut pour les futures erreurs.
**Reco** (à arbitrer lors de l'audit *Api*) : porter une **catégorie générique** sur `Error`/`DomainError` (`NotFound`/`Conflict`/`Validation`/`Unauthorized`/`Unavailable`/…) pour piloter le mapping de façon polymorphe, en gardant Shared agnostique du HTTP.

### 🟡 P2 — Cible `netstandard2.1` (impose le polyfill `IsExternalInit`)
Choix volontaire (compat maximale) mais **tous** les consommateurs réels sont `net10.0` (y compris les têtes MAUI `net10.0-*`). Retarget en `net10.0` supprimerait le polyfill et débloquerait le BCL moderne dans le kernel. Trade-off : perte de consommateurs `netstandard` théoriques. Faible impact, à décider.

### 🟡 P2 — Ergonomie `PagedResult` : reconstruction manuelle répétée
Les handlers reconstruisent un `PagedResult` après mapping des items (`GetTimelineHandler.cs:29`, `SearchCompaniesByNameHandler.cs:56`, `GetRecentFeedItemsHandler.cs:30`).
**Reco** : `PagedResult<T>.Map(Func<T,U>)` + `PagedResult<T>.Empty(page,size)`. Petit confort.

### 🟡 P2 — `PagedResult` rangé dans le namespace `Atlas.Shared.Result`
`PagedResult` n'est pas un `Result` ; le namespace prête à confusion. `Atlas.Shared.Pagination` serait plus juste, mais touche beaucoup de `using` → faible ROI.

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | Couverture de test nulle (créer `Atlas.Shared.UnitTests`) | S |
| 2 | 🟠 P1 | Combinateurs `Result` (Map/Bind/Match) + conversions implicites | M |
| 3 | 🟠 P1 | `Result<T>.Value` silencieux sur échec | S |
| 4 | 🟠 P1 | `Result.cs` = 3 types (viole 1 type/fichier) | XS |
| 5 | 🟠 P1 | Test d'archi « Shared ne dépend de rien » manquant | XS |
| 6 | 🟡 P2 | Mapping `Error→HTTP` non typé (switch string) | M |
| 7 | 🟡 P2 | Cible `netstandard2.1` vs net10 partout | S |
| 8 | 🟡 P2 | Helpers `PagedResult.Map/Empty` | S |
| 9 | 🟡 P2 | Namespace de `PagedResult` | S |
