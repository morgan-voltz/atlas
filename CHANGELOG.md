# Changelog

Toutes les modifications notables de ce projet sont consignées ici.

Le format s'appuie sur [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/),
et le projet suit le [versionnage sémantique](https://semver.org/lang/fr/).

## [Non publié]

Première itération du produit (MVP 1). Le backend est complet ; les clients MAUI et
la documentation sont amorcés. Plusieurs intégrations INPI restent à confirmer par un
appel authentifié réel (cf. roadmap, statuts 🟡).

### Ajouté

- **F-001 — Inscription / connexion** : inscription email + mot de passe, vérification
  d'email, connexion, rafraîchissement et déconnexion (auth hexagonale custom, cf. ADR-010).
- **F-002 — Double authentification (2FA)** : TOTP (RFC 6238), 10 codes de secours,
  défi 2FA à la connexion.
- **F-003 — Connexion d'un compte INPI** : test de connexion `/sso/login`, stockage
  chiffré des identifiants, statut de connexion.
- **F-004 — Recherche entreprise par SIREN** : value object `Siren` (Luhn), client RNE
  authentifié avec cache de token, fiche `UniteLegale`.
- **F-005 — Recherche entreprise par dénomination** : recherche paginée.
- **F-006 — Recherche marque** : adapter INPI PI (auth XSRF + cookies), recherche paginée.
- **F-007 — Fiche détaillée d'une marque** : notice (classes de Nice) + image.
- **F-008 — Historique des recherches** : enregistrement découplé (notification MediatR),
  rétention de 200 entrées par utilisateur.
- **F-009 — Client MAUI mobile** : Login → recherche entreprise → fiche → historique
  (compilé Android).
- **F-010 — Client MAUI desktop** : même application sur cibles desktop (compilé Windows).
- **F-011 — Documentation utilisateur** : site MkDocs Material (`website/`) + déploiement
  GitHub Pages.
- **F-012 — Conformité RGPD** : export des données (JSON) et suppression de compte avec
  effacement en cascade.
- **F-041 — Moteur d'agrégation RSS/Atom (MVP 2)** : contexte Veille (port
  `IExternalContentSource`, entités `FeedSource`/`FeedItem`), provider RSS/Atom
  (CodeHollow.FeedReader), polling récurrent via **Hangfire** (stockage PostgreSQL),
  déduplication par hash (URL + titre), endpoint `GET /feed/items`. Sources système amorcées
  au démarrage (.NET Blog, CNIL, data.gouv.fr).
- **Endpoints API** : `/auth/*`, `/inpi/connection`, `/companies`, `/trademarks`,
  `/search-history`, `/account`, `/feed/items`.
- **Tests** : suite unitaire + tests d'architecture (NetArchTest) + tests d'intégration
  (PostgreSQL via Testcontainers, INPI via WireMock, API end-to-end via WebApplicationFactory).
- **Documentation** : `ARCHITECTURE.md`, `CHANGELOG.md`, `CONTRIBUTING.md`.

### Modifié

- Alignement du mapping RNE sur la documentation technique INPI v4.0 (navigation JSON
  défensive dans `RneCompanyMapper`, mapping des dirigeants).
- `Directory.Packages.props` : correction des versions « fantômes » du squelette initial
  (Konscious, NSubstitute, NetArchTest, Testcontainers, WireMock.Net, Bogus,
  CommunityToolkit.Mvvm) et montée d'OpenTelemetry.
- Client MAUI : passage des `[ObservableProperty]` en propriétés partielles
  (compatibilité WinRT/desktop).

### Sécurité

- Mots de passe hachés avec **Argon2id**.
- Identifiants INPI chiffrés au repos en **AES-256-GCM** ; jamais loggés ni exposés.
- JWT d'accès signés **RS256**, refresh tokens rotatifs et révocables.
- Droits RGPD (export / effacement) implémentés.
- Pin de sécurité `System.Security.Cryptography.Xml` 10.0.8 (CVE GHSA-37gx-xxp4-5rgx).
