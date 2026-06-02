# Audit sécurité, résilience & performance de l'API

> Rapport d'audit de fond de l'API Atlas (`Atlas.Api` et les couches qu'elle mobilise), couvrant la **sécurité**, la **résilience**, la **performance** et la **maintenabilité**. Ce document trace les constats, les correctifs livrés (avec leur PR), et les rares items volontairement écartés ou différés avec leur justification.
> Il sert de **référence d'état** : avant de retoucher l'un des composants cités, vérifier ici si un correctif récent s'y applique.

**Version** : 1.0
**Date de l'audit** : 1ᵉʳ juin 2026
**Date de dernière mise à jour** : 2 juin 2026

---

## Sommaire

- [1. Périmètre & méthodologie](#1-périmètre--méthodologie)
- [2. Verdict global](#2-verdict-global)
- [3. Points forts confirmés](#3-points-forts-confirmés)
- [4. Constats & correctifs par dimension](#4-constats--correctifs-par-dimension)
  - [4.1 Sécurité](#41-sécurité)
  - [4.2 Résilience des intégrations externes](#42-résilience-des-intégrations-externes)
  - [4.3 Performance (base de données & jobs)](#43-performance-base-de-données--jobs)
  - [4.4 Durcissement de l'API](#44-durcissement-de-lapi)
  - [4.5 Maintenabilité](#45-maintenabilité)
- [5. Items écartés ou différés](#5-items-écartés-ou-différés)
- [6. Faux positif notable](#6-faux-positif-notable)
- [7. Découpage en lots (PRs)](#7-découpage-en-lots-prs)
- [8. État final & recommandations](#8-état-final--recommandations)

---

## 1. Périmètre & méthodologie

L'audit a porté sur `Atlas.Api` et les couches mobilisées par ses endpoints : `Atlas.Application`, `Atlas.Infrastructure.Security`, `Atlas.Infrastructure.Persistence`, `Atlas.Infrastructure.Inpi`, `Atlas.Infrastructure.Bodacc`, `Atlas.Infrastructure.Veille`.

**Méthode** : trois explorations parallèles (persistance/EF, endpoints/autorisation, intégrations externes) puis vérification manuelle du noyau cryptographique et JWT (`AesGcmCryptoService`, `RsaSigningKeyProvider`, `JwtIssuer`, `Argon2idPasswordHasher`, rotation des refresh tokens). Chaque constat retenu a été confirmé dans le code (pas de supposition), classé par sévérité, puis corrigé et testé.

**Convention de référence** : les constats portent un code stable (`E2`, `M3`, `F1`…) utilisé dans les messages de commit et les PRs. `E` = élevé, `M` = moyen, `F` = faible (la numérotation suit l'ordre de l'audit, pas la sévérité).

---

## 2. Verdict global

L'API repose sur des **fondations saines** : architecture hexagonale respectée, modèle d'autorisation correct (**aucun IDOR exploitable**), cryptographie de base correcte, **aucune surface d'injection SQL** (tout passe par LINQ paramétré). Les corrections ont porté sur deux axes principaux — la **résilience / SSRF** des intégrations externes et la **performance des jobs de fond** — plus un volet de **durcissement** et de **maintenabilité**.

L'intégralité du périmètre a été traitée en **6 PRs** (cf. §7), à l'exception de deux items explicitement écartés/différés (cf. §5).

---

## 3. Points forts confirmés

- **IDOR maîtrisé** : le `userId` provient toujours du token (jamais d'un identifiant propriétaire fourni par le client), et chaque handler recoupe l'ownership de la ressource.
- **Cryptographie** : Argon2id conforme à `docs/04` (64 Mo / 3 itérations / parallélisme 4, comparaison à temps constant) ; AES-256-GCM correct (nonce aléatoire unique par chiffrement, tag d'authentification vérifié) ; JWT **RS256** avec validation issuer/audience/lifetime/clé et `MapInboundClaims = false`.
- **Gestion des secrets** : refresh tokens hachés au repos (jeton à haute entropie), cookie `HttpOnly + Secure + SameSite=Strict` ; secret TOTP chiffré au repos ; enrichisseur Serilog masquant les propriétés sensibles ; `InpiAccessCredentials.ToString()` masqué.
- **Hygiène** : DTOs systématiques (jamais d'entité domaine exposée), en-têtes de sécurité + CSP restrictive, CORS strict sans `*`, `IHttpClientFactory` partout, `System.Text.Json` exclusif, indexation des clés étrangères globalement excellente.

---

## 4. Constats & correctifs par dimension

> Légende statut : ✅ corrigé (PR) · ⏸️ différé/écarté (cf. §5).

### 4.1 Sécurité

| Réf. | Sévérité | Constat | Correctif | Statut |
|---|---|---|---|---|
| **E2** | Élevée | **SSRF** sur les flux RSS fournis par l'utilisateur : le filtre d'hôte ne résolvait pas les noms DNS (rebinding/TOCTOU) et le client RSS suivait les redirections sans re-valider l'hôte → accès possible aux IP internes / métadonnées cloud (`169.254.169.254`). | `ConnectCallback` validant l'**IP réellement résolue** à chaque connexion TCP (fetch initial, redirection, rebinding), via `PrivateNetworkGuard` partagé avec `FeedSubscriptionPolicy`. | ✅ #142 |
| **E3** | Élevée | **XXE** potentiel au parsing des flux : aucun durcissement explicite des DTD sur du XML hostile. | `SafeXmlGuard` (`DtdProcessing=Prohibit`, `XmlResolver=null`, `MaxCharactersFromEntities=0`) appliqué **avant** `FeedReader`. | ✅ #142 |
| **M1** | Moyenne | **Énumération de comptes par timing** au login : un email inconnu répondait sans exécuter de hachage, donc plus vite. | Vérification Argon2id **factice** quand l'email est inconnu, pour égaliser le temps de réponse. | ✅ #142 |
| **M2** | Moyenne | **Pas de détection de réutilisation** de refresh token : la rotation existait, mais rejouer un jeton révoqué n'escaladait pas. | Le rejeu d'un refresh token déjà révoqué déclenche la **révocation de toute la famille** de jetons de l'utilisateur (theft detection). | ✅ #142 |
| **M3** | Moyenne | Endpoints coûteux sous la **seule limite globale** (quota INPI / CPU exposés). | Politique de rate limiting dédiée **`expensive`** (20 req/min/compte) sur `POST /downloads/bulk` et `GET /companies/{siren}/report.pdf`. Les recherches INPI restent sous la limite globale (100/min/compte), suffisante contre l'abus. | ✅ #146 |
| **F2** | Faible | **Injection de formule CSV** : `CsvWriter` ne neutralisait pas les cellules débutant par `= + - @` (exports favoris alimentés par des champs client). | Préfixe d'une apostrophe sur les cellules à risque (`CsvWriter.NeutralizeFormula`). | ✅ #146 |

Concernant les credentials INPI : déchiffrement transitoire correct, jamais loggés, `ToString()` masqué. Voir aussi **M9** (§4.2).

### 4.2 Résilience des intégrations externes

| Réf. | Sévérité | Constat | Correctif | Statut |
|---|---|---|---|---|
| **E1** | Élevée | **Aucune résilience HTTP câblée** sur les clients INPI/BODACC/RSS, malgré l'ADR-007/`docs/09` (retry + circuit breaker annoncés). Risque de *fail-storm* si l'INPI tombe. | `AddStandardResilienceHandler` (retry exponentiel sur erreurs transitoires + circuit breaker + timeout par tentative depuis la config ; `HttpClient.Timeout=Infinite` pour laisser le pipeline gouverner). Pour le client RSS, chaque tentative repasse par le `ConnectCallback` anti-SSRF (E2). | ✅ #143 |
| **M9** | Moyenne | `InpiAccessResolver` laissait remonter une `CryptographicException` (clé KMS tournée / blob corrompu), interrompant **tout** un job batch. | Capture → erreur métier `inpi.credentials_unreadable` (409), sans exposer de valeur sensible. | ✅ #143 |

### 4.3 Performance (base de données & jobs)

| Réf. | Sévérité | Constat | Correctif | Statut |
|---|---|---|---|---|
| **E4a** | Élevée | `CompanyFavoriteRepository.GetAllAsync` chargeait toute la table en entités **trackées** (jobs en lecture seule). | `AsNoTracking()`. | ✅ #144 |
| **E4b** | Élevée | **N+1** dans `RefreshFavoritesHandler` : un `GetCurrentAsync` de snapshot par favori. | Préchargement de tous les snapshots du user en une requête (`ICompanyFavoriteSnapshotRepository.GetByUserAsync`) + lookup mémoire. | ✅ #144 |
| **E4c** | Moyenne | **N+1** dans le polling BODACC : un `GetKnownExternalIdsAsync` par favori. | Requête batch par SIREN (`GetKnownExternalIdsForUsersAsync`) puis regroupement en mémoire. | ✅ #145 |
| **E5a** | Moyenne | `GetTimelineAsync` utilisait un `ContinueWith` bloquant sur `.Result` (avalait les exceptions). | Remplacé par `await` + regroupement synchrone. | ✅ #144 |
| **E5b** | Moyenne | `GetTimelineAsync` lent en pagination profonde (page 50 ≈ 1,9 s). **Profilé** : la sous-requête `SourceCount` placée dans la projection paginée était évaluée par PostgreSQL pour toutes les lignes ordonnées **avant** l'OFFSET (~1000), pas seulement les 20 retournées. | `SourceCount` calculé dans une requête séparée restreinte aux items affichés (≤ pageSize). Mesuré : page 50 1,9 s → ~210 ms (9×). | ✅ #149 |
| **M4** | Moyenne | Archive ZIP du bulk download entièrement en `MemoryStream` (risque d'OOM sur gros volumes). | Construction sur **fichier temporaire** (`FileOptions.DeleteOnClose`), mémoire bornée. | ✅ #145 |
| **M5** | Moyenne | Appels INPI **séquentiels** dans `RefreshFavoritesHandler`. | Séparation **phase HTTP** (fetch parallèle borné, `MaxDegreeOfParallelism=4`) / **phase DB** (traitement séquentiel), pour respecter la non-thread-safety du `DbContext`. | ✅ #145 |
| **M7** | Moyenne | Compteur de likes `VeillePack` en read-modify-write → *lost update* entre likes concurrents. | Incrément/décrément **atomiques** en base (`ExecuteUpdate` ; décrément borné à 0 via `GREATEST`) pour Like **et** Unlike. | ✅ #144 |
| **M8** | Moyenne | Index manquant sur `feed_item.fetched_at` (filtré + trié par `ListFetchedSinceAsync`). | Index + migration `AddFeedItemFetchedAtIndex`. | ✅ #144 |
| **F6** | Faible | Aucun timeout de commande Npgsql explicite. | `CommandTimeout(30s)` dans `AddPersistenceInfrastructure`. | ✅ #146 |

### 4.4 Durcissement de l'API

| Réf. | Sévérité | Constat | Correctif | Statut |
|---|---|---|---|---|
| **F1** | Faible | `RegisterDevice` : un token > 4096 caractères faisait lever une `ArgumentException` dans `DeviceRegistration.Register` (→ 500). | `RegisterDeviceValidator` borne `Token` (≤ 4096) et `Label` (≤ 128) → **400** propre. | ✅ #146 |

### 4.5 Maintenabilité

| Réf. | Sévérité | Constat | Correctif | Statut |
|---|---|---|---|---|
| **F3** | Faible | ~58 répétitions de `if (!principal.TryGetUserId(out Guid userId)) return Results.Unauthorized();` à travers les endpoints. | Type lié **`CurrentUser`** (`IBindableFromHttpContext`, réutilise `ClaimsPrincipalExtensions.TryGetUserId`) : un paramètre `CurrentUser user` suffit. 13 fichiers allégés (−298 lignes nettes). | ✅ #147 |
| **F4** | Faible | Trois méthodes d'export CSV favoris quasi identiques. | Helper générique `FavoritesEndpoints.ExportAsync<TDto>`. | ✅ #147 |

> **Note `CurrentUser`** : `IBindableFromHttpContext<TSelf>` contraint `TSelf` à être un **type référence** — `CurrentUser` est donc une `class`. Les endpoints qui prennent ce paramètre doivent être derrière `RequireAuthorization()` ; le claim `sub` est alors garanti présent. `AuthEndpoints` a été traité de façon ciblée car `/verify-email` possède un paramètre `userId` qui n'est **pas** le claim.

---

## 5. Items écartés ou différés

| Réf. | Décision | Justification |
|---|---|---|
| **E5b — dénormalisation `SourceCount`** | ⏸️ Écarté (profilé) | La proposition initiale (colonne dénormalisée) est **inutile** : mesurée à ~0,2 ms, la sous-requête n'était pas le problème (cf. E5b §4.3, corrigé autrement en #149). |
| **E5b — index composite** `(cluster_id, published_at, fetched_at)` | ⏸️ Écarté (profilé) | **Aucun gain** mesuré vs l'index simple `cluster_id` existant (137/199 ms ≈ 142/210 ms). L'index simple actuel reste néanmoins essentiel (sans index cluster : 2,6–4,4 s). |
| **E5b — pagination keyset** | ✅ Fait (#151) | Implémentée : curseur `(OccurredAt, Id)` sur le flux fusionné RSS + événements, supprimant le plafond de fusion (500) et le sur-fetch ; chaque flux ne lit que `pageSize+1` entrées après le curseur. Contrat changé (`PagedResult` → `CursorPage`), clients Blazor + Uno mis à jour. |
| **M6** | ⏸️ Écarté | `SaveChanges` par utilisateur dans les jobs de fond : **choix de conception assumé** (progression incrémentale — un job interrompu ne reperd pas les users déjà traités), pas un bug. |
| **F5** | ⏸️ Écarté | AAD AES-GCM liant le ciphertext au `userId` (défense en profondeur, sévérité Faible). **Invasif** (changement de signature `ICryptoService` + threading du userId) et **casserait les `InpiCredentials` / secrets TOTP déjà chiffrés** : nécessiterait un format de chiffrement versionné + une migration de ré-chiffrement. À traiter dans une évolution dédiée si le besoin se confirme. |

---

## 6. Faux positif notable

Une exploration avait classé comme **« Critique »** un *fallback silencieux* vers une clé de chiffrement de développement en production (`AesGcmCryptoService` dérivant `SHA256("atlas-dev-insecure-crypto-key")` en l'absence de `Crypto:KeyBase64`). **C'est un faux positif** : `AddSecurityInfrastructure` impose `Crypto:KeyBase64` **et** `Jwt:PrivateKeyPem` via `ValidateOnStart()` hors environnement Development — l'application **refuse de démarrer** sans clé réelle. Le fallback n'est donc jamais atteignable en production. Aucune action requise.

---

## 7. Découpage en lots (PRs)

| PR | Lot | Thème |
|---|---|---|
| [#142](https://github.com/Morgan-Voltz/atlas/pull/142) | 1 | Sécurité — SSRF (E2), XXE (E3), timing login (M1), theft detection (M2) |
| [#143](https://github.com/Morgan-Voltz/atlas/pull/143) | 2 | Résilience — Polly INPI/BODACC/RSS (E1), garde déchiffrement INPI (M9) |
| [#144](https://github.com/Morgan-Voltz/atlas/pull/144) | 3a | Performance DB — N+1 favoris (E4a/E4b), timeline (E5a), likes atomiques (M7), index (M8) |
| [#145](https://github.com/Morgan-Voltz/atlas/pull/145) | 3b | Performance jobs — batch BODACC (E4c), ZIP temp (M4), parallélisme INPI (M5) |
| [#146](https://github.com/Morgan-Voltz/atlas/pull/146) | 4a | Durcissement — rate limiting (M3), injection CSV (F2), validation device (F1), timeout SQL (F6) |
| [#147](https://github.com/Morgan-Voltz/atlas/pull/147) | 4b | Maintenabilité — binder `CurrentUser` (F3), factorisation exports (F4) |
| [#149](https://github.com/Morgan-Voltz/atlas/pull/149) | E5b | Timeline — `SourceCount` hors projection paginée (profilé : page 50 9× plus rapide) |
| [#151](https://github.com/Morgan-Voltz/atlas/pull/151) | E5b | Timeline — pagination keyset (curseur), supprime le plafond de fusion et le sur-fetch |

Chaque lot a été livré sur une branche dédiée, mergé en *squash* après CI verte (build Release + tests unitaires + tests d'architecture + tests d'intégration Docker), conformément à la Definition of Done (`CLAUDE.md`).

---

## 8. État final & recommandations

À l'issue de l'audit, **tout le périmètre sécurité / résilience / performance / durcissement / maintenabilité est traité**. Recommandations de suivi :

1. **Avant ouverture publique (F-028)** : les prérequis sécurité (SSRF, XXE, anti-timing, theft detection) sont en place. Réévaluer alors le rate limiting des recherches INPI si le quota devient un point sensible.
2. **Timeline (suite d'E5b)** : la **pagination keyset** est désormais en place (#151), supprimant le plafond de fusion (500) et le sur-fetch. Étape ultérieure éventuelle à très grande échelle : unifier les deux flux (RSS + événements) en une vue/`UNION ALL` paginée côté SQL plutôt qu'une fusion en mémoire de deux requêtes.
3. **F5** : si un modèle de menace « accès en écriture à la base » devient pertinent, introduire un format de chiffrement versionné permettant d'ajouter l'AAD sans casser les données existantes.
4. **Régression** : les comportements ajoutés sont couverts par des tests (unitaires + intégration Docker pour les requêtes EF et la résilience). Conserver cette couverture lors des évolutions des composants cités.
