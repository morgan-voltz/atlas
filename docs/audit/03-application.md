# Audit profond Atlas — Partie 3 : `Atlas.Application` (+ `Application.Premium`)

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-27.

## Contexte de la partie

Couche use-cases (CQRS léger via MediatR). **26 slices verticales** réparties en Users (7), Veille (11), Inpi (3), IntellectualProperty (2), Companies (2), Search (1). Chaque slice = `Command|Query` + `Validator` + `Handler` (+ DTO co-localisé). ~31 handlers. Référence : Domain + Shared + MediatR + FluentValidation. `Application.Premium` = stub vide (ADR-009), référencé par l'Api mais **non wiré** dans `Program.cs` (assumé).

## Verdict global

**Santé : très bonne.** Pureté parfaite (aucune fuite EF/HttpClient/AspNetCore ; `IDateTimeProvider` partout, jamais de `DateTime.UtcNow`), conventions strictes (`internal sealed` handlers, `public sealed record … : IRequest<Result<T>>`, file-scoped), pipeline de validation, mapping DTO manuel (pas d'AutoMapper, domaine jamais exposé), `CancellationToken` propagés, zéro `throw` métier / `async void` / appel bloquant. Les axes d'amélioration sont la **couverture (handlers sécurité non testés)**, des **validators non testés**, et quelques détails de perf/robustesse.

## Points forts (à préserver)

- ✅ **Pureté** : `.csproj` = Domain + Shared + MediatR + FluentValidation uniquement. Aucun `DbContext`/`HttpClient`/`Npgsql`/`AspNetCore`/`Console`/`DateTime.UtcNow` dans le code.
- ✅ **Conventions** quasi parfaites ; les fichiers multi-types sont des **co-localisations intentionnelles** (Query+DTO, Command+Result, handlers/validators appariés) — acceptable.
- ✅ **Transactions** : 18 handlers mutateurs appellent `SaveChangesAsync` **exactement une fois** ; les queries sont en lecture seule. *(Vérifié : le `LoginHandler` qui « semblait » sauver 3× le fait en réalité sur 3 branches mutuellement exclusives → 1 save/requête.)*
- ✅ **ValidationBehavior** (seul behavior, `Common/Behaviors/ValidationBehavior.cs`) exécute les validators et convertit les échecs en `ValidationError` → `Result` (jamais d'exception). `ResultFactory` gère la création typée.
- ✅ Logique partagée bien extraite : `AuthTokenFactory`, `VeillePackEnroller`, `InpiAccessResolver`.
- ✅ Scoping utilisateur correct sur la plupart des handlers (unsubscribe, INPI, account passent le `userId` de la commande).

## Constats & recommandations

### 🟠 P1 — Couverture handlers ~68% (21/31) — trous sur des handlers **sécurité**
Non testés et **critiques** :
- 🔒 `RefreshTokenHandler` — rotation du refresh token (hash → lookup → révocation → réémission). Logique sensible, **zéro test**.
- 🔒 `LogoutHandler` — révocation de token (idempotence).
- 🔒 `SetupTwoFactorHandler` / `DisableTwoFactorHandler` — mise en place/désactivation 2FA (états, suppression des codes de récupération).
- 🔒 `VerifyEmailHandler` — vérification e-mail (token + expiration, transition d'état du compte).
- Plus bas risque : `GetRecentFeedItemsHandler`, `UnsubscribeFromFeedSourceHandler`.
**Reco** : prioriser les **5 handlers auth/2FA** (chemins d'échec : not-found, token invalide/expiré, état incohérent). Les zones Companies/Inpi/IP/Search sont à 100%, Veille à 82%.

### 🟠 P1 — Validators jamais testés directement
Les ~19 validators ne sont exercés qu'**indirectement** (via les tests de handlers, souvent avec des entrées valides). Des règles non triviales ne sont donc pas épinglées : `RegisterUserValidator` (mot de passe ≥ 12, e-mail ≤ 254), `AddUserFeedSourceValidator` (URL http(s) absolue), `GetTimelineValidator` (pageSize 1..100).
**Reco** : ajouter des tests de validators dédiés (très légers via `validator.TestValidate(...)`, fort ROI sur la régression d'input).

### 🟡 P2 — N+1 dans `GetMyVeillePacksHandler`
`GetMyVeillePacksHandler.cs:25` boucle et appelle `packRepository.GetByIdAsync` **par enrollment**, alors que `GetMySubscriptionsHandler` batch correctement via `GetByIdsAsync` + `.Distinct()`. Impact réel faible (N = nb de packs appliqués, ≈ ≤ 6) mais **incohérent** avec le bon pattern voisin.
**Reco** : ajouter `IVeillePackRepository.GetByIdsAsync` et batcher.

### 🟡 P2 — `SetFeedItemStateHandler` sans contrôle d'abonnement (robustesse, **pas** une faille)
Le handler vérifie seulement que l'item existe globalement (`ExistsAsync`) puis upsert l'état `(userId, itemId)`. **Ce n'est pas un IDOR / fuite de confidentialité** : l'état est strictement par utilisateur, aucune donnée d'autrui n'est lue ni exposée, et la timeline reste scoped aux abonnements. Au pire, un utilisateur crée une ligne d'état orpheline pour un item qu'il ne suit pas (inoffensif).
**Reco** (optionnelle) : vérifier l'abonnement à la source de l'item avant de créer l'état, pour éviter les lignes orphelines. *(Constat requalifié après vérification — un sous-agent l'avait classé « critique » à tort.)*

### 🟡 P2 — Quelques commandes/queries sans validator
11 requêtes n'ont pas de validator, mais la plupart sont **userId-only ou sans input** (le `userId` vient du principal authentifié, pas de l'utilisateur) → validator inutile. Le vrai manque : `RefreshTokenCommand` et `LogoutCommand` portent un **token string** sans garde `NotEmpty`.
**Reco** : ajouter un validator minimal pour ces deux-là.

### 🟡 P2 — Boilerplate `Result` (corrobore la Partie 1)
Motif `if (x.IsFailure) return Result<Y>.Fail(x.Error!)` + `.Value!` répété dans les handlers (ex. `GetCompanyBySirenHandler` 2×, `AddUserFeedSourceHandler`). Sera résorbé par les **combinateurs `Result`** (reco Partie 1, item 2).

### 🟡 P2 — 10 dossiers `.gitkeep` vides
`Companies/Commands`, `Companies/Queries`, `Veille/Commands`, `Veille/Queries`, `Common/Mappings`, `Common/Services`, etc. : structure anticipée jamais utilisée (les slices sont à la racine du dossier de feature).
**Reco** : supprimer ces placeholders (ou les assumer explicitement). Cosmétique.

## Non-issues vérifiés (transparence)

- ✅ `LoginHandler` : 1 save par requête (branches exclusives) — **pas** un problème transactionnel.
- ✅ `DeleteAccountHandler` / `RecordSearchHistoryHandler` (prune) utilisent `ExecuteDeleteAsync` (commit direct) — correct, simplement un idiome différent de l'UoW (cohérence mineure, non bloquant).
- ✅ `Application.Premium` : stub vide non wiré — assumé (ADR-009).

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | Handlers sécurité non testés (Refresh, Logout, Setup/Disable 2FA, VerifyEmail) | M |
| 2 | 🟠 P1 | Validators jamais testés directement | S |
| 3 | 🟡 P2 | N+1 `GetMyVeillePacks` (batcher via GetByIdsAsync) | S |
| 4 | 🟡 P2 | `SetFeedItemState` sans check d'abonnement (lignes orphelines) | S |
| 5 | 🟡 P2 | `RefreshTokenCommand`/`LogoutCommand` sans validator (token) | XS |
| 6 | 🟡 P2 | Boilerplate `Result`/`.Value!` (→ combinateurs Partie 1) | — |
| 7 | 🟡 P2 | 10 dossiers `.gitkeep` vides à nettoyer | XS |
