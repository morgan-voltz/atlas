# Audit profond Atlas — Partie 4b : `Infrastructure.Veille` / `Inpi` / `Security` / `Messaging`

> Rapport **lecture seule**. Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-27.

## Contexte de la partie

Les adapters externes et le cœur sécurité :
- **Security** : `Argon2idPasswordHasher`, `AesGcmCryptoService`, `RsaSigningKeyProvider` + `JwtIssuer`, `TwoFactorChallengeService`, `TotpProvider`, `SecureTokenGenerator`, `SystemDateTimeProvider`.
- **Inpi** : `InpiAuthenticationProvider`, `RneCompanyProvider`, `InpiPiTrademarkProvider` (+ mappers).
- **Veille** : `RssFeedProvider`, `FeedSubscriptionPolicy`.
- **Messaging** : `LoggingEmailSender` (stub dev).

Tous : `internal sealed`, file-scoped, pureté csproj OK (Application/Domain/Shared + lib tech, pas d'Api, pas de référence croisée), DI + options propres.

## Verdict global

**Santé : implémentation crypto excellente, mais durcissement opérationnel et sécurité périmétrique incomplets.** Les algorithmes sont corrects et conformes à `docs/04` (Argon2id, AES-256-GCM, RS256, TOTP, CSPRNG, stockage des tokens hashés, séparation d'audience 2FA). Les axes à traiter : **SSRF sur les flux utilisateur**, **secrets/clefs à durcir avant tout déploiement**, et **absence totale de tests directs du code de sécurité**.

## Points forts (à préserver)

- ✅ **Argon2id exemplaire** (`Argon2idPasswordHasher.cs`) : 64 Mo / 3 itérations / parallélisme 4 (conforme doc 04 §5.4.1), sel 16 o CSPRNG unique, hash 32 o, **params encodés avec le hash** (évolutivité), vérification `CryptographicOperations.FixedTimeEquals`.
- ✅ **AES-256-GCM correct** (`AesGcmCryptoService.cs`) : nonce 12 o **CSPRNG par chiffrement** (pas de réutilisation), tag 16 o vérifié au déchiffrement, clé 256 bits, format `nonce||tag||cipher`.
- ✅ **TOTP RFC 6238** (secret 160 bits, fenêtre ±1, SHA1 standard), **génération de tokens CSPRNG** + stockage **hashé** (SHA-256), **séparation d'audience 2FA** (`atlas-2fa` ≠ `atlas`).
- ✅ **INPI** : token mis en cache avec TTL (expiry − marge), ré-authentification automatique sur 401, **credentials jamais loggés / non cachés / déchiffrés en mémoire le temps de la requête**, `System.Text.Json`, mapping erreurs → `InpiErrors`/`CompanyErrors`/`TrademarkErrors`, jamais d'exception qui fuite. Bien couvert par tests WireMock.
- ✅ **RssFeedProvider** : HttpClient typé + timeout + User-Agent, ne lève jamais (catch → `Result.Fail`). Tests WireMock (parse, filtre `since`, 404, contenu non-feed).
- ✅ Aucun secret en dur (hormis la clé **DEV** explicitement étiquetée, cf. ci-dessous).

## 🟠 P1 (bloquants avant tout déploiement / exposition)

### P1-a — SSRF : flux RSS fournis par l'utilisateur non protégés contre les cibles internes
`RssFeedProvider` fait `httpClient.GetAsync(source.Url)` sur des URL **fournies par l'utilisateur** (F-043, mergé). La seule barrière, `FeedSubscriptionPolicy.IsUrlAllowed` (`FeedSubscriptionPolicy.cs:19-30`), ne teste qu'une **liste de fragments d'hôtes interdits**. Ne sont **pas** bloqués : `127.0.0.1`, `[::1]`, `10.x`/`172.16-31.x`/`192.168.x`, `169.254.169.254` (métadonnées cloud), etc. → **SSRF** : un utilisateur peut faire émettre au serveur des requêtes vers des ressources internes / endpoints de métadonnées.
**Reco** : résoudre l'hôte et **rejeter les plages privées/loopback/link-local** (et idéalement re-valider après redirection), avant fetch. Top priorité sécurité de cette partie.

### P1-b — Secrets/clefs : fallbacks « DEV » non sécurisés sans garde de prod
- `AesGcmCryptoService.cs:38` : si `Crypto:KeyBase64` est absent, la clé devient `SHA256("atlas-dev-insecure-crypto-key")` — **clé statique publique**. Aucun garde n'empêche cet usage hors-DEV. → les `InpiCredentials` seraient chiffrés avec une clé connue (y compris **en local avec tes vrais identifiants INPI** si la clé n'est pas configurée — cf. harness local).
- `RsaSigningKeyProvider.cs:21` : `RSA.Create(3072)` génère une clé **éphémère** si `Jwt:PrivateKeyPem` absent → jetons invalidés à chaque redémarrage, non validables entre instances. Pas de validation de taille de clé à l'import.
- Aucune intégration **KMS** (Azure Key Vault / Vault) alors que doc 04 §5.3 la prévoit ; pas de rotation ni d'endpoint JWKS.
**Reco** : **échouer au démarrage hors Development** si `Crypto:KeyBase64`/`Jwt:PrivateKeyPem` ne sont pas fournis ; documenter le provisioning des clés ; prévoir l'abstraction KMS (un `IKeyStore`) et la rotation/JWKS pour la suite.

### P1-c — Aucun test direct du code de sécurité
Il **n'existe pas** de projet `Atlas.Infrastructure.Security.UnitTests` (ni Messaging). Argon2id, AES-GCM, JWT, TOTP, génération de tokens ne sont testés qu'**indirectement** (mocks dans les handlers / E2E). Pour du code cryptographique, c'est le trou le plus risqué.
**Reco** : créer `Atlas.Infrastructure.Security.UnitTests` : hash/verify Argon2 (bon mdp / mauvais mdp / format corrompu), AES-GCM round-trip + **détection d'altération** (tag), JWT (claims/expiry/signature/issuer/audience), 2FA challenge (expiry, isolation d'audience), TOTP (fenêtre ±1, code invalide).

## 🟡 P2

- **Pas de limite de taille** sur le téléchargement de flux (`RssFeedProvider.cs:38`, `ReadAsByteArrayAsync` non borné) → risque DoS mémoire / zip-bomb. La roadmap F-043 mentionnait « vérification taille raisonnable » non implémentée. Reco : `MaxResponseSizeBytes` dans `VeilleOptions` + contrôle `Content-Length`/stream borné.
- **XXE/DTD** de `CodeHollow.FeedReader` non vérifié (lib tierce, parse du XML non fiable). Reco : confirmer la désactivation DTD/XXE (ou la couvrir par un test avec payload XXE).
- **`TwoFactorChallengeService.cs:22`** utilise `DateTime.UtcNow` au lieu de `IDateTimeProvider` (incohérent avec le reste ; nuit à la testabilité du skew). Reco : injecter l'horloge.
- **INPI** : pas de Polly (retry transitoire 5xx, **pas de gestion 429/Retry-After**) ; seule la ré-auth 401 est gérée. Acceptable en lecture, mais à durcir. `AllowAutoRedirect=false` côté PI sans gestion manuelle de redirection visible — à documenter (l'API ne devrait pas renvoyer de 3xx).
- **Messaging** : seul `LoggingEmailSender` (stub dev) existe ; l'adapter **Brevo** (provider RGPD France imposé par CLAUDE.md) **n'est pas implémenté** → envoi d'e-mail non opérationnel hors dev. Pré-requis avant prod (clé API via config/env, jamais loggée, mêmes patterns `Result`).
- `FeedSubscriptionPolicy` **non testé** (logique simple mais sécurité-adjacente : casse, hôtes unicode).

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | SSRF : flux utilisateur sans blocage IP privées/métadonnées | M |
| 2 | 🟠 P1 | Fallbacks DEV non sûrs (clé AES statique, clé RSA éphémère) sans garde prod ; pas de KMS/rotation/JWKS | M |
| 3 | 🟠 P1 | Aucun test direct du code crypto/JWT/TOTP (créer Security.UnitTests) | M |
| 4 | 🟡 P2 | Pas de limite de taille de flux (DoS) | S |
| 5 | 🟡 P2 | XXE/DTD CodeHollow non vérifié | S |
| 6 | 🟡 P2 | `TwoFactorChallengeService` utilise `DateTime.UtcNow` | XS |
| 7 | 🟡 P2 | INPI sans Polly / sans gestion 429 | S |
| 8 | 🟡 P2 | Brevo non implémenté (stub dev) — pré-prod | M |
| 9 | 🟡 P2 | `FeedSubscriptionPolicy` non testé | S |

## Note transverse

Les items P1-b (clés) et P2 (Brevo) sont des **pré-requis de mise en production**, à recouper avec l'audit *Api* (middleware HTTPS/HSTS, rate-limiting, wiring des clés) et l'audit *Tests* (créer Security.UnitTests). Le SSRF (P1-a) est le seul risque réellement *exploitable sur du code mergé* dès qu'il y a exposition réseau multi-utilisateurs.
