# Audit profond Atlas — Partie 4a : `Atlas.Infrastructure.Persistence`

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-27.

## Contexte de la partie

Adapter de persistance (EF Core 10 + Npgsql, PostgreSQL). `AtlasDbContext` (public, implémente `IUnitOfWork`), `AtlasDbContextFactory` (design-time), 12 `IEntityTypeConfiguration`, 12 repositories `internal sealed` (1:1 avec les ports domaine), 9 migrations + snapshot. Référence Application + Domain + Shared ; **pas** l'Api. `InternalsVisibleTo` → `…Persistence.IntegrationTests`.

## Verdict global

**Santé : bonne, mais un défaut RGPD à corriger.** Le câblage est exemplaire (alignement parfait config/repo/port, conversions de value objects partout, longueurs alignées sur les constantes domaine, `InpiCredentials` chiffrés sans colonne en clair, nommage snake_case homogène, migrations additives). **Un trou de conformité RGPD (P0)** : la suppression de compte n'efface pas les données de veille. Et des axes perf/robustesse (tracking, concurrence, résilience) + des trous de tests d'intégration.

## 🔴 P0 — La suppression de compte n'efface pas les données de veille (RGPD art. 17)

Chaîne : `DeleteAccountHandler.cs:13` → `UserRepository.DeleteAsync` (`UserRepository.cs:23`) = `dbContext.Users.Where(...).ExecuteDeleteAsync(ct)`.
- `ExecuteDeleteAsync` émet un `DELETE` SQL **brut** : il **contourne le change-tracker EF** et ne déclenche **que les cascades définies au niveau base de données**.
- La migration `AddUserCascadeDeletes` a posé des FK `ON DELETE CASCADE` pour `accounts`, `refresh_tokens`, `two_factor_recovery_codes`, `inpi_credentials`, `search_history` → celles-ci sont bien purgées.
- **Mais** `veille_subscription`, `veille_pack_enrollment`, `feed_item_user_state` (ajoutées en F-041/043/042/044) **n'ont aucune FK vers `users`** (configs : seulement des index uniques `(user_id, …)`, pas de `HasOne<User>()`). → ces lignes, qui contiennent le `user_id`, **survivent à la suppression du compte**.

Conséquence : **données personnelles orphelines persistées** après exercice du droit à l'effacement, alors que le commentaire de `DeleteAccountHandler` affirme « la suppression de l'utilisateur cascade sur **toutes** ses données liées ». `UserCascadeDeleteTests` ne couvre pas les tables veille → le test passe sans détecter le trou.

**Reco** : ajouter une FK `user_id → users(id) ON DELETE CASCADE` sur `veille_subscription`, `veille_pack_enrollment`, `feed_item_user_state` (config EF `HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade)` + migration), **et** étendre `UserCascadeDeleteTests` pour asserter la purge de ces 3 tables. (Alternative : effacement explicite dans `DeleteAccount`, moins robuste.)

## Constats & recommandations

### 🟠 P1 — Aucun `AsNoTracking()` sur les lectures (overhead systématique)
Aucune méthode de lecture des 12 repositories n'utilise `AsNoTracking()` : toutes chargent les entités dans le change-tracker (snapshots, proxies) alors qu'elles sont en lecture seule (DTO-mappées ensuite). Coût mémoire/CPU sur tous les chemins de lecture (timeline, listes paginées, lookups). Ex. `FeedItemRepository.GetTimelineAsync`, `VeilleSubscriptionRepository.GetByUserAsync`, `UserRepository.GetBy*`.
**Reco** : ajouter `.AsNoTracking()` à toutes les requêtes de lecture (garder le tracking uniquement sur les chemins d'écriture qui rechargent puis mutent).

### 🟠 P1 — Trous de tests d'intégration (6/12 repositories peu ou pas couverts)
4 classes de tests, 7 méthodes. **Non couverts** : `VeillePackRepository` (0), `VeillePackEnrollmentRepository` (0), `TwoFactorRecoveryCodeRepository` (0) ; couverture seulement implicite pour `FeedSource`/`VeilleSubscription`/`FeedItemUserState`. Risques runtime spécifiques non testés :
- **Collection possédée `VeillePack.Items`** (`OwnsMany` → `veille_pack_item`) : aucun round-trip ni test de remplacement/dedup.
- **`FeedSourceRepository.GetByIdsAsync`** : traduction `ids.Contains(source.Id)` avec conversion de value object **non testée** (risque de non-traduction EF→SQL au runtime).
- **Enforcement des index uniques** : seul `users.email` est testé ; `inpi_credentials.user_id`, `(source_id, content_hash)`, `(user_id, source_id)`, `veille_pack.code` non testés.
**Reco** : compléter (round-trip VeillePack + owned items, repos 2FA/pack, violation de contraintes uniques, `GetByIdsAsync`). Cf. P0 : ajouter aussi l'assertion cascade veille.

### 🟡 P2 — Aucun jeton de concurrence optimiste
Aucune config n'a de `IsConcurrencyToken()` / `xmin` / rowversion. Risque de *lost-update* sur les écritures concurrentes : compteur d'échecs de login (`User`), `FeedSource.LastPolledAt` (jobs de polling parallèles), `FeedItemUserState`. Aujourd'hui : last-write-wins silencieux.
**Reco** : mapper `xmin` (Npgsql) comme concurrency token sur les entités à écritures concurrentes, ou l'assumer explicitement.

### 🟡 P2 — Requête timeline (F-044) : EXISTS corrélé + entités complètes matérialisées
`FeedItemRepository.GetTimelineAsync` filtre via `VeilleSubscriptions.Any(...)` (EXISTS corrélé), LEFT JOIN sur l'état, **sans `AsNoTracking`**, et matérialise les entités `FeedItem` complètes avant mapping `TimelineEntry`.
**Reco** : `AsNoTracking` (cf. P1) ; envisager un INNER JOIN sur `veille_subscription` plutôt qu'EXISTS ; projeter directement les colonnes utiles. (L'index composite `(user_id, source_id)` existe déjà côté `veille_subscription`, ce qui atténue.)

### 🟡 P2 — Pas de résilience Npgsql
`DependencyInjection.cs:21` : `UseNpgsql(connectionString)` sans `EnableRetryOnFailure()` ni timeout de commande. En prod, les erreurs transitoires PostgreSQL ne sont pas retentées.
**Reco** : activer une stratégie de retry transitoire + timeout raisonnable (à arbitrer).

### 🟡 P2 — Références « lâches » sans FK (intégrité)
Au-delà du P0 (FK user manquantes), `veille_subscription.source_id` et `veille_pack_enrollment.pack_id` n'ont pas de FK vers `feed_source`/`veille_pack`. Risque d'orphelins si une source/un pack est supprimé (rare car sources système). `feed_item_user_state` a bien sa FK vers `feed_item` (cascade) — donc seul le lien vers `users` manque.
**Reco** : ajouter les FK manquantes (cohérence avec le reste du schéma) en même temps que le correctif P0.

## Points forts (à préserver)

- ✅ **Alignement parfait** : 12 configs ↔ 12 entités ↔ 12 repos ↔ 12 ports.
- ✅ `AtlasDbContext` = `IUnitOfWork`, enregistré comme **même instance scoped** (`DependencyInjection.cs:23`). Repos `internal sealed`, DI complète et correcte (le « typo » signalé par un sous-agent était une hallucination — le code compile et est juste).
- ✅ **Conversions de value objects** systématiques, clés `ValueGeneratedNever`. Longueurs de colonnes **alignées** sur les constantes domaine (`FeedItem.MaxTitleLength=512`, etc.). `DateTimeOffset` → `timestamp with time zone` confirmé dans les migrations.
- ✅ **`InpiCredentials`** : seuls les blobs chiffrés sont persistés, aucune colonne en clair (aligné doc 04).
- ✅ Index uniques sur les invariants (`email`, `url`, `(source_id, content_hash)`, `(user_id, source_id)`, `code`, `token_hash`) + index de chemins chauds (`published_at`, `(user_id, is_archived)`).
- ✅ Owned type `VeillePackItem` correctement mappé (PK composite `(pack_id, source_id)`, FK cascade vers `veille_pack`).
- ✅ Migrations **additives** (pas d'opération destructrice), snapshot unique cohérent (13 tables), snake_case homogène.

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🔴 P0 | Suppression de compte n'efface pas les données veille (FK user manquantes + `ExecuteDelete`) | M |
| 2 | 🟠 P1 | Aucun `AsNoTracking()` sur les lectures | S |
| 3 | 🟠 P1 | Tests d'intégration : 6/12 repos peu/pas couverts (+ owned items, `GetByIdsAsync`, contraintes uniques) | M |
| 4 | 🟡 P2 | Aucun jeton de concurrence optimiste | M |
| 5 | 🟡 P2 | Timeline : EXISTS corrélé + entités complètes + sans AsNoTracking | S |
| 6 | 🟡 P2 | Pas de résilience Npgsql (retry/timeout) | S |
| 7 | 🟡 P2 | FK « lâches » `source_id`/`pack_id` (intégrité) | S |

## Note transverse

Le P0 + le trou de tests cascade sont liés : le correctif (FK cascade veille) **doit** s'accompagner d'un test d'intégration asserrant la purge. À recouper avec l'audit *Sécurité/RGPD* (partie 4b — credentials INPI) et l'audit *Tests* (partie 6).
