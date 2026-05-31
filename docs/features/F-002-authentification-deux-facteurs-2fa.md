# F-002 — Authentification deux facteurs (2FA)

> **Statut** : ✅ Implémenté (MVP 1, 27 mai 2026). TOTP RFC 6238 (Otp.NET), secret chiffré AES-256-GCM (`ICryptoService`), 10 codes de secours hashés, défi 2FA à la connexion. Endpoints `/auth/2fa/{setup,enable,disable,verify}`. WebAuthn/passkeys non couvert (futur).

**Description** : l'utilisateur peut activer un 2FA TOTP (Google Authenticator, Authy, etc.) pour sécuriser son compte.

**Valeur user** : protection contre les compromissions de mots de passe. Indispensable pour un service qui stocke des credentials INPI.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-001.

**Détails techniques** : RFC 6238 TOTP, génération QR code, codes de secours (10 codes one-time imprimables).
