# F-001 — Inscription et connexion utilisateur

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Auth hexagonale custom — Argon2id, JWT RS256, refresh tokens rotatifs (cf. ADR-010). Endpoints `/auth/{register,verify-email,login,refresh,logout}`. 2FA (F-002) à suivre.

**Description** : un utilisateur peut créer un compte sur la plateforme (email + mot de passe), valider son email, se connecter et se déconnecter.

**Valeur user** : prérequis à toute personnalisation et à la connexion d'un compte INPI multi-tenant.

**Complexité** : ★★

**APIs externes** : aucune (auth interne).

**Dépendances** : aucune.

**Détails techniques** : ASP.NET Core Identity (ou alternative type Auth0 / Keycloak à décider). Hashing bcrypt / Argon2. Email transactionnel (Mailjet, Brevo, ou self-hosted Postfix pour démarrer).
