# F-019 — Alerte email sur modification d'une entreprise favorite

> **Statut** : ✅ Backend implémenté + **adapter Brevo livré (29 mai 2026)**. `CompanyFavoriteSnapshot` (un cliché vivant par `(UserId, SIREN)`) + `DiffWith(UniteLegale)` détecte les changements de dénomination / forme juridique / NAF / adresse / dirigeants (hash). Job Hangfire `favorite-refresh` cron `0 3 * * *` (désactivable). Notification MediatR `CompanyFavoriteChangedNotification` → 2 handlers : email (via `IEmailSender.SendFavoriteChangeAsync`) et push (via `INotificationDispatcher`, cf. F-020). Users sans compte INPI connecté passés silencieusement. **Adapter email Brevo** (provider France RGPD-compliant) : `BrevoEmailSender` (HttpClient typé, `POST /v3/smtp/email` avec header `api-key`) avec template HTML + texte, switch DI sur `Email:Brevo:ApiKey` (sans clé → fallback `LoggingEmailSender` dev). Reste : templates email enrichis (logo, branding).

> **Architecture liée** : F-019 est la **première implémentation** du patron formalisé par **ADR-013** (substrat de surveillance — stratégie `IStateMonitor<UniteLegale>` : retraits détectés, état comparé). F-048 (BODACC) est la première instance du patron `IItemStreamMonitor<TItem>` (flux append-only). L'extraction effective du runner mutualisé est planifiée pour F-057 (3ᵉ instance).

**Description** : un job quotidien re-fetch les fiches favorites. Si une modification est détectée (changement d'adresse, de dirigeant, dépôt d'un bilan, etc.), l'utilisateur reçoit un email résumant les changements.

**Valeur user** : première fonction de veille — passage d'un outil de consultation à un outil pro-actif.

**Complexité** : ★★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-017.

**Détails techniques** :
- Cron job nocturne.
- Mécanisme de diff entre snapshots de fiches.
- Persistance des snapshots historiques (utile aussi pour de futures features de timeline).
- Gestion fine des erreurs (un user dont le compte INPI a expiré ne bloque pas les autres).
