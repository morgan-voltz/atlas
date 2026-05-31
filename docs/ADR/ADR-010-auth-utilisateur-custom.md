# ADR-010 — Authentification utilisateur hexagonale custom

**Statut** : ✅ Accepté
**Date** : 27 mai 2026

## Contexte

Le modèle d'authentification des **utilisateurs du SaaS** (à ne pas confondre avec l'authentification *INPI* multi-tenant tranchée par ADR-003) était listé dans les « décisions à prendre ultérieurement ». Trois options étaient sur la table (cf. F-001) :

- **ASP.NET Core Identity** : framework intégré (UserStore EF, confirmation d'email, lockout, 2FA prêts à l'emploi).
- **IdP externe** (Auth0, Keycloak self-host) : délégation complète de l'identité.
- **Auth hexagonale custom** : implémentation propre, alignée sur l'architecture du projet, via des ports.

Les contraintes de sécurité sont déjà fermes (cf. `docs/04-securite-rgpd.md` §5.4) : **Argon2id** imposé (pas de PBKDF2/bcrypt), **JWT courts** signés en asymétrique (RS256/EdDSA), **refresh tokens rotatifs** révocables côté serveur, **TOTP** obligatoire dès qu'un compte connecte des credentials INPI.

## Décision

**Authentification hexagonale custom**. L'identité, les comptes et les sessions sont modélisés dans `Atlas.Domain` ; les mécanismes techniques sont des **adapters** derrière des ports secondaires :

- `IPasswordHasher` → adapter Argon2id (Konscious) — paramètres 64 Mo / 3 itérations / parallélisme 4.
- `IJwtIssuer` + `ISigningKeyProvider` → émission JWT RS256 (clé RSA, PEM configurable / KMS en production).
- `ITokenGenerator` → jetons opaques (vérification d'email, refresh) ; seul le **hash** est persisté.
- `IUserRepository`, `IAccountRepository`, `IRefreshTokenRepository`, `IUnitOfWork` → persistance EF Core/PostgreSQL.
- `IEmailSender` → email transactionnel (adapter Brevo en production ; adapter de log en DEV).

Les use cases (`RegisterUser`, `VerifyEmail`, `Login`, `RefreshToken`, `Logout`) sont des handlers MediatR retournant `Result`/`Result<T>`. Le refresh token est déposé dans un cookie **httpOnly / Secure / SameSite=Strict**.

## Rationale

- **Cohérence architecturale** : ASP.NET Core Identity couple l'identité à EF et impose son propre hasher (PBKDF2) et son modèle ; l'intégrer aurait signifié le désactiver en grande partie pour satisfaire Argon2id + JWT custom + le pattern `Result<T>`. Friction nette avec l'hexagonal (ADR-004).
- **Souveraineté / RGPD** : un IdP externe (Auth0) ré-introduit une dépendance SaaS tierce, à rebours de la posture OSS/souveraine du projet (ADR-005/006). Keycloak self-host ajoute une lourde opération d'exploitation pour un MVP.
- **Maîtrise sécurité** : le projet stocke des credentials INPI ; il doit garder un contrôle total et auditable sur le hachage, la signature des jetons et la révocation.
- **Coût maîtrisé** : le périmètre F-001 (register/verify/login/refresh/logout) est borné et entièrement testable avec des ports mockés.

## Conséquences

- **Positives** : zéro friction architecturale, testabilité maximale (ports), aucune dépendance d'identité tierce, conformité directe aux exigences de `docs/04`.
- **Négatives** : davantage de code à écrire et à maintenir (flux email de vérification, rotation des refresh tokens, lockout). Responsabilité accrue : toute faille d'implémentation est nôtre.
- **À prévoir** :
  - **F-002 (TOTP)** : ajouter `ITotpProvider` (Otp.NET) et les codes de secours, en réutilisant le même socle.
  - Adapter **Brevo** pour `IEmailSender` (remplacer l'adapter de log DEV) avant production.
  - **Blacklist Redis** des access tokens révoqués (révocation immédiate) — prévue, non implémentée en F-001.
  - Clé RSA de signature gérée par **KMS** en production (PEM éphémère en DEV uniquement).
