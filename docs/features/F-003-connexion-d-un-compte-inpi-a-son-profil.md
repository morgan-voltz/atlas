# F-003 — Connexion d'un compte INPI à son profil

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). Test de connexion via `IInpiAuthenticationProvider` (`POST /sso/login` RNE), credentials chiffrés AES-256-GCM (`ICryptoService`, jamais loggés), statut Active/Invalid. Endpoints `/inpi/connection` (POST/GET/DELETE). Cache court du token RNE + refresh auto : non encore implémenté (sera ajouté avec F-004 qui consomme le RNE).

**Description** : depuis ses paramètres, l'utilisateur fournit ses identifiants INPI (email + mot de passe). Le système teste la connexion, stocke les credentials chiffrés, et marque le compte comme "actif".

**Valeur user** : sans ça, aucune fonction métier ne peut être utilisée. C'est le pont entre l'identité utilisateur SaaS et la capacité à appeler l'INPI.

**Complexité** : ★★★

**APIs externes** : INPI RNE (test de connexion).

**Dépendances** : F-001.

**Détails techniques** :
- Chiffrement des credentials en AES-256-GCM avec une clé dérivée du mot de passe utilisateur (ou idéalement une clé spécifique stockée dans un KMS type Azure Key Vault, AWS Secrets Manager, ou Vault).
- Test de connexion en appelant `/sso/login` du RNE.
- Stockage du token JWT obtenu en cache court (1h).
- Refresh automatique lorsque le token approche de l'expiration.
