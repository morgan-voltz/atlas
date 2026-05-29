# Sécurité & Conformité RGPD

> Document de référence sur les exigences de sécurité technique et organisationnelle pour la mise en conformité du projet avec le RGPD et les bonnes pratiques de cybersécurité.
> **Disclaimer** : ce document est rédigé par un dev pour un dev. Il pose 90% du chemin technique et conceptuel. Avant la mise en production avec de vrais utilisateurs payants, **une revue par un avocat data privacy est fortement recommandée** (compter 500–1500€ pour une analyse complète + rédaction des CGU/politique de confidentialité).

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026

---

## Sommaire

- [1. Cadre légal applicable](#1-cadre-légal-applicable)
- [2. Qualification juridique](#2-qualification-juridique)
- [3. Les 7 principes RGPD](#3-les-7-principes-rgpd)
- [4. Bases légales du traitement](#4-bases-légales-du-traitement)
- [5. Mesures techniques de sécurité](#5-mesures-techniques-de-sécurité)
- [6. Mesures organisationnelles](#6-mesures-organisationnelles)
- [7. Choix d'hébergement](#7-choix-dhébergement)
- [8. Droits des personnes](#8-droits-des-personnes)
- [9. Gestion des violations de données](#9-gestion-des-violations-de-données)
- [10. Documentation obligatoire](#10-documentation-obligatoire)
- [11. Gestion des sous-traitants](#11-gestion-des-sous-traitants)
- [12. Cas particuliers](#12-cas-particuliers)
- [13. Checklist par sprint](#13-checklist-par-sprint)
- [14. Roadmap conformité](#14-roadmap-conformité)

---

## 1. Cadre légal applicable

Le projet est soumis à plusieurs corpus juridiques :

### 1.1 Niveau européen

- **RGPD (Règlement (UE) 2016/679)** : règlement général sur la protection des données. S'applique dès qu'on traite des données personnelles de résidents de l'UE.
- **Directive ePrivacy (2002/58/CE)** : régit notamment les cookies, le marketing direct, les communications électroniques.
- **NIS2 (Directive (UE) 2022/2555)** : cybersécurité pour certains opérateurs essentiels — probablement pas applicable au MVP, mais à surveiller si le projet grossit.

### 1.2 Niveau français

- **Loi Informatique et Libertés (1978, modifiée 2018)** : transpose et complète le RGPD en France. Donne pouvoirs et missions à la CNIL.
- **Code de commerce (art. L.123-50 et suivants)** : régit la diffusion des données du RNE.
- **Code de la propriété intellectuelle (art. L.411-1)** : missions de l'INPI.
- **Loi pour une République numérique (2016)** : ouverture des données publiques.
- **Loi PACTE (2019)** : création du RNE, transferts depuis le RNCS.
- **Recommandation CNIL cookies (2020)** : règles précises sur les bandeaux de consentement.

### 1.3 Spécifique aux données du RNE

Les données du RNE sont des **données publiques en open data**, distribuées sous **Licence Ouverte 2.0 (Etalab)**. Mais :

- Chaque entreprise peut avoir indiqué un statut **"autorisation d'utilisation commerciale"** = false → respecter cette préférence.
- Les **données en diffusion partielle** (`diffusionINSEE = "N"`) ne doivent pas être rediffusées publiquement.
- Les **bénéficiaires effectifs** font l'objet d'un régime distinct depuis l'arrêt CJUE Sovim (novembre 2022) : accès restreint.

---

## 2. Qualification juridique

### 2.1 Rôle dans le RGPD

En tant qu'opérateur du SaaS, tu es **responsable de traitement** (RGPD art. 4.7) pour :

| Type de donnée | Statut | Base légale |
|---|---|---|
| Comptes utilisateurs (email, hash mot de passe, profil) | Responsable de traitement | Exécution du contrat (art. 6.1.b) |
| Credentials INPI chiffrés | Responsable de traitement | Exécution du contrat |
| Historique de recherches | Responsable de traitement | Exécution du contrat |
| Favoris, annotations | Responsable de traitement | Exécution du contrat |
| Données INPI mises en cache localement | Responsable de traitement (zone grise) | Intérêt légitime |
| Newsletters / marketing | Responsable de traitement | Consentement explicite (art. 6.1.a) |
| Logs techniques (IPs, sessions) | Responsable de traitement | Intérêt légitime (sécurité) |

### 2.2 Tu N'ES PAS responsable du traitement initial des données INPI

Les données du RNE / des marques sont collectées et publiées par l'INPI dans le cadre de **ses missions de service public**. L'INPI est responsable de traitement de ces données. Tu es un **réutilisateur**, ce qui implique :

- Respecter les conditions de la licence Etalab et les CGU INPI.
- Documenter ta réutilisation dans ton registre des traitements.
- Ne pas dénaturer ou détourner les données.

---

## 3. Les 7 principes RGPD

L'article 5 du RGPD impose 7 principes que tu dois respecter **et pouvoir prouver** que tu respectes (principe d'accountability).

### 3.1 Licéité, loyauté, transparence

- **Licéité** : tout traitement doit avoir une base légale (cf. §4).
- **Loyauté** : ne pas traiter en cachette ou par tromperie.
- **Transparence** : informer les personnes (politique de confidentialité claire, lisible).

### 3.2 Limitation des finalités

Les données collectées pour une finalité X ne peuvent pas être utilisées pour une finalité Y incompatible. Exemple : tu ne peux pas utiliser l'email d'inscription pour de la prospection commerciale sans nouveau consentement.

### 3.3 Minimisation

Ne collecter que ce qui est strictement **nécessaire**. Tu n'as pas besoin de la date de naissance ou de la civilité pour qu'un user utilise un outil de recherche d'entreprise.

### 3.4 Exactitude

Les données doivent être exactes et à jour. Mécanisme de correction obligatoire (cf. droits des personnes).

### 3.5 Limitation de la conservation

Définir une **durée de conservation maximale** pour chaque type de donnée :

| Type de donnée | Durée recommandée |
|---|---|
| Compte utilisateur actif | Durée d'inscription |
| Compte inactif (>2 ans sans connexion) | Suppression ou anonymisation automatique |
| Logs techniques | 90 jours à 1 an max |
| Historique de recherches | Configurable user, défaut 12 mois |
| Données de facturation | 10 ans (obligation comptable) |
| Sauvegardes | 90 jours glissants |

### 3.6 Intégrité et confidentialité

Sécurité technique et organisationnelle. C'est l'objet des §5 et §6 de ce document.

### 3.7 Accountability

Tu dois **prouver** ta conformité. D'où la documentation obligatoire (registre, politique, etc.).

---

## 4. Bases légales du traitement

Le RGPD reconnaît **6 bases légales possibles** (art. 6). Pour ton SaaS, les bases pertinentes sont :

### 4.1 Exécution du contrat (art. 6.1.b)

S'applique à tout ce qui est **nécessaire pour fournir le service**.

Exemples :
- Stocker email et mot de passe pour permettre la connexion
- Stocker les credentials INPI pour faire les appels API
- Stocker les recherches et favoris

### 4.2 Intérêt légitime (art. 6.1.f)

S'applique pour des traitements **proportionnés** où ton intérêt (en tant que responsable) ne porte pas atteinte aux droits fondamentaux des personnes.

Exemples :
- Logs de sécurité (détection d'attaques)
- Mise en cache des données INPI pour performance
- Analyse anonymisée d'usage pour améliorer le produit

**Documentation requise** : test de mise en balance ("LIA — Legitimate Interest Assessment") à conserver.

### 4.3 Consentement (art. 6.1.a)

S'applique pour les traitements **non strictement nécessaires** au service.

Exemples :
- Newsletter / emails marketing
- Cookies de tracking analytique non-essentiel
- Partage de données à des tiers (s'il y en a)

**Le consentement doit être** : libre, spécifique, éclairé, univoque. Pas de cases pré-cochées, pas d'opt-out déguisé.

### 4.4 Obligation légale (art. 6.1.c)

Exemples :
- Conservation des données comptables (10 ans)
- Réponse à une réquisition judiciaire

---

## 5. Mesures techniques de sécurité

### 5.1 Chiffrement au repos (at-rest)

#### 5.1.1 Niveau base de données

PostgreSQL doit être chiffré au niveau du système de fichiers ou du volume :
- **LUKS** (Linux) pour le chiffrement disque complet
- **BitLocker** (Windows)
- Chez les cloud providers : option "Encryption at rest" activée (souvent par défaut chez Clever Cloud, Scaleway, etc.)

#### 5.1.2 Chiffrement applicatif des données ultra-sensibles

Pour les credentials INPI (la donnée la plus critique) : **chiffrement au niveau colonne**, en plus du chiffrement disque.

**Architecture recommandée — envelope encryption** :
1. Chaque utilisateur a sa propre **DEK (Data Encryption Key)** — clé AES-256.
2. Cette DEK est elle-même chiffrée par une **KEK (Key Encryption Key)** stockée dans un **KMS** (Azure Key Vault, AWS KMS, HashiCorp Vault).
3. La DEK chiffrée est stockée dans la base, à côté des credentials qu'elle protège.
4. Pour utiliser les credentials INPI : récupérer la DEK chiffrée → la déchiffrer via le KMS → déchiffrer les credentials → utiliser → effacer de la mémoire.

**Bibliothèque .NET recommandée** : `Microsoft.AspNetCore.DataProtection` + `Azure.Security.KeyVault.Keys` (ou équivalent).

#### 5.1.3 Algorithmes

- **Symétrique** : AES-256-GCM (recommandé en 2026)
- **Asymétrique** : RSA-4096 ou ECDH P-384
- **Hash mots de passe** : Argon2id avec coût ajustable. Plus moderne que bcrypt, recommandé par OWASP.

### 5.2 Chiffrement en transit (in-transit)

#### 5.2.1 HTTPS partout

- **TLS 1.3 minimum**. Pas de TLS 1.0/1.1/1.2 (sauf compatibilité descendante limitée).
- **HSTS** (`Strict-Transport-Security`) avec `max-age=31536000; includeSubDomains; preload`.
- Certificats Let's Encrypt automatiques (ACME) ou via le cloud provider.

#### 5.2.2 Mobile

Sur les apps MAUI :
- **Certificate pinning** : embarquer le hash du certificat serveur dans l'app pour bloquer les attaques MITM via un faux CA installé sur le device.
- Désactiver les protocoles obsolètes côté client (ne jamais accepter TLS 1.0/1.1).

#### 5.2.3 Services internes

Si plusieurs services backend communiquent (futur) : **mTLS** (mutual TLS), où chaque service présente un certificat à l'autre.

### 5.3 Gestion des secrets

**Règle absolue : aucun secret dans le code source ou les fichiers commités.**

#### 5.3.1 En développement

- `dotnet user-secrets` pour ASP.NET Core
- Fichiers `.env` dans `.gitignore`
- Idéalement, utiliser un coffre-fort de mots de passe partagé (Bitwarden, 1Password)

#### 5.3.2 En production

- Variables d'environnement injectées par l'orchestrateur (Docker secrets, Kubernetes secrets)
- Secrets sensibles dans un **KMS** récupérés au démarrage de l'application
- Rotation périodique (au moins annuelle)

#### 5.3.3 Détection automatique en CI

- **`gitleaks`** ou **`trufflehog`** en pre-commit hook et en GitHub Actions
- Bloque tout push contenant un secret détecté
- Si un secret a été commité par accident : **changer le secret immédiatement** (pas seulement le supprimer de l'historique, c'est trop tard)

### 5.4 Authentification & gestion des sessions

#### 5.4.1 Mots de passe

- **Argon2id** pour le hash. Paramètres : memory 64 MB, iterations 3, parallelism 4 (à ajuster selon le hardware).
- **Politique de complexité** : minimum 12 caractères, conformément aux dernières recommandations de l'ANSSI (la "complexité" type 1 majuscule + 1 chiffre + 1 spécial n'est plus une bonne pratique — la longueur prime).
- **Pas de rotation forcée** : la rotation périodique obligatoire est déconseillée par le NIST depuis 2017 (les users finissent par utiliser des variantes prédictibles).

#### 5.4.2 Multi-factor authentication (MFA / 2FA)

- **TOTP obligatoire** pour tout compte avec credentials INPI connectés.
- Conforme RFC 6238.
- Codes de secours (10 codes one-time imprimables).
- Possibilité future : WebAuthn / passkeys pour une UX moderne.

#### 5.4.3 Sessions

- **JWT courts** : 15 à 60 minutes max
- **Refresh tokens** stockés en httpOnly cookie, durée de vie 30 jours, rotation à chaque usage
- **Révocation** possible côté serveur (liste noire en Redis)
- Sur mobile : tokens stockés dans **Keychain (iOS)** ou **Android Keystore**, jamais dans SharedPreferences/UserDefaults

### 5.5 Sécurité applicative — OWASP Top 10 (2021/2025)

| Risque | Protection |
|---|---|
| **A01 — Broken Access Control** | Authorization handlers ASP.NET Core, vérifier "user owns the resource" dans chaque endpoint |
| **A02 — Cryptographic Failures** | Chiffrement systématique, pas d'algos obsolètes (MD5, SHA1) |
| **A03 — Injection (SQL, NoSQL, OS)** | EF Core paramétré, jamais de string concaténée. Validation stricte des inputs |
| **A04 — Insecure Design** | Threat modeling au début, revues de design |
| **A05 — Security Misconfiguration** | Templates sécurisés par défaut, headers de sécurité (CSP, X-Frame-Options, etc.) |
| **A06 — Vulnerable Components** | Dependabot, dotnet-outdated, scan régulier des CVE |
| **A07 — Auth Failures** | Argon2id, 2FA, lockout après 5 échecs, captcha si nécessaire |
| **A08 — Software Integrity Failures** | Signature des packages NuGet, supply chain verification |
| **A09 — Logging & Monitoring Failures** | Serilog structuré, alerting, audit trail |
| **A10 — SSRF** | Validation stricte des URLs externes, allowlist plutôt que blocklist |

### 5.6 Headers de sécurité HTTP

À configurer dans le middleware ASP.NET Core :

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
Content-Security-Policy: default-src 'self'; ...
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: ...
```

### 5.7 Logging sans fuite de données personnelles

**Piège classique** :

```csharp
// MAUVAIS — fuite l'email dans les logs
Logger.LogInformation("User {User} logged in", user);

// BON — log l'ID, pas la valeur
Logger.LogInformation("User {UserId} logged in", user.Id);
```

Règles :
- Logger les **IDs**, jamais les valeurs sensibles
- Masquer automatiquement les emails, téléphones, IPs (utilisable en Serilog Enricher custom)
- Aucun mot de passe, token, ou credential dans les logs (même temporaire pour debug)
- Rétention courte (90 jours par défaut)
- Logs eux-mêmes chiffrés au repos si possible

### 5.8 Rate limiting et anti-abuse

- Rate limiting **par IP** et **par utilisateur** sur tous les endpoints sensibles
- **AspNetCoreRateLimit** ou **`Microsoft.AspNetCore.RateLimiting`** (intégré .NET 7+)
- Captcha (hCaptcha plutôt que reCAPTCHA pour conformité RGPD) sur signup et login en cas de tentatives multiples

### 5.9 Tests de sécurité

| Type | Outil | Fréquence |
|---|---|---|
| **SAST** (analyse statique) | SonarQube, Security Code Scan, Roslyn analyzers | À chaque PR |
| **DAST** (analyse dynamique) | OWASP ZAP | Avant chaque release |
| **Dependency scanning** | Dependabot, Snyk | Continu |
| **Secret scanning** | gitleaks, trufflehog | À chaque commit |
| **Pentest manuel** | Cabinet externe ou freelance | Annuel, avant ouverture publique |
| **Bug bounty** | YesWeHack, HackerOne | À partir de la phase 3 |

---

## 6. Mesures organisationnelles

### 6.1 Registre des activités de traitement (art. 30 RGPD)

Document **obligatoire** dès qu'on traite des données personnelles régulièrement. Pour chaque traitement, documenter :

- Nom du traitement
- Finalité(s)
- Catégories de données traitées
- Catégories de personnes concernées
- Destinataires (qui voit les données)
- Durée de conservation
- Mesures de sécurité techniques et organisationnelles
- Transferts hors UE (le cas échéant)
- Base légale

**Outil recommandé** : Excel ou Notion pour démarrer. Outils dédiés (DASTRA, Dastra, Privacy Tools) quand le projet grossit.

### 6.2 Politique de confidentialité

Document **public**, accessible depuis chaque page du site et des apps. Doit contenir :

- Identité du responsable de traitement (toi)
- Coordonnées (email DPO si applicable)
- Finalités du traitement avec bases légales
- Catégories de données collectées
- Destinataires
- Durées de conservation
- Droits des personnes et comment les exercer
- Existence d'un transfert hors UE (le cas échéant)
- Droit de réclamation auprès de la CNIL

### 6.3 CGU / CGV

Conditions Générales d'Utilisation (et de Vente quand monétisation). Doivent encadrer :

- Description du service
- Création et gestion du compte
- Obligations de l'utilisateur (notamment sur la légalité de l'usage des données INPI auxquelles il accède via son propre compte)
- Disponibilité et niveau de service
- Limitation de responsabilité
- Propriété intellectuelle (rappel : la licence du code est AGPL, mais l'usage du service relève des CGU)
- Modification, résiliation
- Droit applicable et juridiction

### 6.4 DPO — Délégué à la Protection des Données

#### 6.4.1 Obligation légale

Obligatoire si (art. 37 RGPD) :
- Tu es une autorité publique
- Tes activités principales nécessitent un **suivi régulier et systématique à grande échelle**
- Tes activités principales consistent en un **traitement à grande échelle de données sensibles**

**Pour un MVP avec quelques centaines d'utilisateurs** : pas obligatoire. Mais c'est une bonne pratique de :
- Auto-désigner un DPO interne (toi pour démarrer)
- Avoir une adresse `dpo@tondomaine.fr` qui répond aux demandes
- Recourir à un DPO externe mutualisé (200–500€/mois) quand tu auras des clients professionnels exigeants

### 6.5 DPIA — Analyse d'impact

Obligatoire (art. 35 RGPD) si le traitement est susceptible d'engendrer **un risque élevé pour les droits et libertés**. Critères :
- Évaluation/scoring systématique
- Décision automatisée avec effet juridique
- Surveillance systématique
- Traitement à grande échelle de données sensibles
- Croisement de données
- Données concernant des personnes vulnérables
- Usage innovant ou nouvelles technologies
- Empêchement d'exercice d'un droit ou d'un contrat

**Pour ton SaaS au démarrage** : probablement pas obligatoire. **Mais à reconsidérer** si tu introduis :
- Du scoring de risque d'entreprises basé sur agrégation automatique
- Du matching automatique de marques avec décision juridique
- Du graphe relationnel des dirigeants → **ADR-012 (doc 01) érige la DPIA en prérequis de mise en service** pour F-034 ; à réaliser avant tout livrable, conformément au gating.

---

## 7. Choix d'hébergement

### 7.1 Pourquoi héberger en Europe

L'hébergement hors UE pose deux problèmes :
1. **Transferts internationaux** : encadrés par des SCC (Standard Contractual Clauses), TIA (Transfer Impact Assessment) à mener. Lourd administrativement.
2. **CLOUD Act américain** : permet aux autorités US de demander l'accès à des données chez les providers US, même hors USA. Conflit avec RGPD.

### 7.2 Hébergeurs recommandés

| Hébergeur | Pays | Type | Prix | Avantages | Inconvénients |
|---|---|---|---|---|---|
| **Clever Cloud** | 🇫🇷 France | PaaS managé | €€ | Très simple pour .NET + PostgreSQL, support FR | Coûts qui montent à l'échelle |
| **Scaleway** | 🇫🇷 France | IaaS + managé | €€ | Moderne, dev-friendly, certifications | Plus cher qu'OVH |
| **OVHcloud** | 🇫🇷 France | IaaS + managé | € | Pas cher, catalogue large | UX rugueuse |
| **Outscale** | 🇫🇷 France | IaaS | €€€ | SecNumCloud, hébergeur "qualifié" | Cher, plus complexe |
| **Hetzner** | 🇩🇪 Allemagne | IaaS + bare metal | € | Excellent rapport perf/prix | Moins de managé |
| **Infomaniak** | 🇨🇭 Suisse | Hébergement web/PaaS | € | Vert (éco), Suisse RGPD-compatible | Catalogue limité |

### 7.3 Choix recommandé pour le MVP

**Clever Cloud** :
- PaaS managé : tu pousses ton code Git, ça se déploie
- Support natif .NET 8/9 (et Java, Node, Python, etc.)
- PostgreSQL managé inclus
- Hébergement en France (Paris)
- Conformité RGPD documentée
- Free tier généreux pour démarrer (puis montée progressive)

**Quand changer** : à partir de ~1000 utilisateurs ou besoin de plus de contrôle, migrer vers Scaleway ou OVH (IaaS avec Kubernetes ou Docker Swarm).

### 7.4 Cas particulier : si tu utilises un cloud US

Si pour une raison ou une autre tu utilises Azure / AWS / GCP, **obligations supplémentaires** :
- Activer les **Standard Contractual Clauses**
- Mener une **Transfer Impact Assessment** documentée
- **Chiffrer côté toi** (le cloud provider ne voit que des bytes chiffrés)
- Choisir une région UE (West Europe, France Central, Paris)
- Documenter ce choix dans le registre

---

## 8. Droits des personnes

Le RGPD donne 7 droits aux personnes concernées. Tu dois pouvoir les **honorer techniquement**, pas seulement les mentionner sur une page.

### 8.1 Implémentation des droits

| Droit | Article | Implémentation technique |
|---|---|---|
| **Information** | 13-14 | Politique de confidentialité accessible partout |
| **Accès** | 15 | Bouton "Télécharger mes données" → ZIP contenant un JSON structuré |
| **Rectification** | 16 | Interface de modification du profil |
| **Effacement** | 17 | Bouton "Supprimer mon compte" → suppression effective sous 30 jours |
| **Limitation** | 18 | Possibilité de "geler" un compte sans le supprimer |
| **Portabilité** | 20 | Export en format JSON/CSV structuré |
| **Opposition** | 21 | Pour traitement basé sur intérêt légitime, possibilité de s'y opposer |

### 8.2 Anonymisation vs suppression

À la demande d'effacement, **tu ne peux pas toujours tout supprimer** (obligations comptables, logs de sécurité). Solution : **anonymisation**. Remplacer les données identifiantes par des valeurs neutres tout en conservant les enregistrements nécessaires.

Exemple : un user supprime son compte. Tu :
- Supprimes son email, nom, mot de passe, credentials INPI
- Anonymises son ID dans les logs (remplaces par un hash sans clé de réversion)
- Conserves les factures (obligation 10 ans) mais avec ID anonymisé

### 8.3 Délais de réponse

- **1 mois** pour répondre à une demande
- Possibilité de prolonger de 2 mois pour les demandes complexes (en motivant)
- **Gratuit**, sauf demandes manifestement infondées ou excessives

---

## 9. Gestion des violations de données

### 9.1 Définition

Toute "violation de la sécurité entraînant, de manière accidentelle ou illicite, la destruction, la perte, l'altération, la divulgation non autorisée ou l'accès non autorisé" à des données personnelles.

Exemples :
- Fuite d'une base de données
- Compromission du compte d'un admin
- Vol d'un laptop de dev avec des données
- Bug exposant des données d'un user à un autre

### 9.2 Procédure

1. **Détection** → systèmes de monitoring + alerting (Sentry, Better Stack, etc.)
2. **Confinement** → couper l'accès, révoquer les tokens
3. **Évaluation** → est-ce vraiment une violation ? quel impact pour les personnes ?
4. **Si haut risque** → notification CNIL **sous 72h** via le formulaire en ligne
5. **Si haut risque pour les personnes** → notification individuelle des utilisateurs concernés
6. **Documentation** → registre des violations, à conserver
7. **Remédiation** → corriger la faille, post-mortem

### 9.3 Préparer le plan AVANT le jour J

Un **incident response plan** rédigé à froid évite la panique. Document à avoir :
- Qui contacter (interne et externe)
- Comment évaluer la gravité
- Templates de notifications (CNIL, users)
- Procédures de confinement
- Communication publique éventuelle

---

## 10. Documentation obligatoire

Récapitulatif des documents à produire et tenir à jour :

| Document | Obligatoire ? | Destinataire | Fréquence de mise à jour |
|---|---|---|---|
| Registre des activités de traitement (RAT) | ✅ (si traitement régulier) | Interne + CNIL si demande | À chaque évolution majeure |
| Politique de confidentialité | ✅ | Public | À chaque évolution |
| CGU / CGV | ✅ | Public | À chaque évolution |
| DPA avec chaque sous-traitant | ✅ | Interne | À la signature de chaque vendor |
| Liste des sous-traitants | ✅ | Public | Continue |
| DPIA | Selon cas | Interne + CNIL si haut risque | Avant nouveau traitement à risque |
| Procédure de gestion des violations | Recommandé | Interne | Annuel |
| Bandeau cookies | ✅ (si cookies non essentiels) | Public | À chaque changement |
| Politique de sécurité interne | Recommandé | Interne | Annuel |
| Plan de continuité d'activité (BCP) | Recommandé | Interne | Annuel |

---

## 11. Gestion des sous-traitants

### 11.1 Définition

Un **sous-traitant** au sens RGPD est tout tiers qui traite des données personnelles **pour ton compte**. Exemples :

- Hébergeur (Clever Cloud, OVH...)
- Email transactionnel (Mailjet, Brevo, SendGrid...)
- Paiement (Stripe, Lemon Squeezy...)
- Analytics (Plausible, Matomo...)
- Erreurs / monitoring (Sentry, Better Stack...)
- Stockage de fichiers (S3-compatible...)

### 11.2 Obligations

Pour **chaque sous-traitant** :

1. **Vérifier sa conformité RGPD** : politique de confidentialité, DPA disponible, certifications (ISO 27001, SOC 2, HDS si santé)
2. **Signer un DPA** (Data Processing Agreement) — souvent disponible en self-service chez les vendors sérieux
3. **Le faire figurer dans la liste des sous-traitants** publique
4. **Vérifier la localisation des données** (UE de préférence)

### 11.3 Choix recommandés

| Fonction | Recommandation | Localisation |
|---|---|---|
| Email transactionnel | Brevo (ex-Sendinblue) | 🇫🇷 France |
| Email marketing | Brevo | 🇫🇷 France |
| Paiement | Stripe (avec config UE) | 🇮🇪 Irlande |
| Analytics web | Plausible self-hosted ou Matomo | UE |
| Monitoring erreurs | Sentry self-hosted ou GlitchTip | UE |
| Stockage fichiers | Scaleway Object Storage ou OVH | 🇫🇷 France |
| Notifications mobile | Firebase (Google) — sinon Pushy | Vérifier zone |
| CAPTCHA | hCaptcha plutôt que reCAPTCHA Google | 🇺🇸 mais plus respectueux |

---

## 12. Cas particuliers

### 12.1 Cookies et trackers

- Cookies **strictement nécessaires** (session, panier...) → pas de consentement requis
- Tous les autres (analytics, marketing, A/B test) → **consentement préalable explicite**
- **Bandeau cookies conforme** : possibilité de refuser aussi facile que d'accepter (pas de "dark pattern")
- Recommandation CNIL 2020 à respecter

**Pour démarrer sans bandeau** : utiliser uniquement des cookies de session essentiels + Plausible Analytics (qui ne pose pas de cookies).

### 12.2 Mineurs

Si ton service est accessible à des mineurs (-15 ans en France, -16 dans certains pays UE), **consentement parental requis**. Pour un SaaS B2B comme le tien, tu peux :
- Stipuler dans tes CGU qu'il faut être majeur pour s'inscrire
- Bloquer l'inscription des -15 ans (interface)
- Si tu détectes un mineur, supprimer le compte

### 12.3 Données sensibles

Le RGPD identifie des "catégories particulières" (art. 9) : santé, opinions politiques, religion, etc. **Tu ne devrais en avoir aucune** dans ton SaaS. Si jamais ça arrive (ex. un user qui colle un dossier client en pièce jointe contenant des données médicales) :
- Ne pas traiter
- Inviter à retirer

### 12.4 Bénéficiaires effectifs (BE)

**Régime spécial depuis l'arrêt CJUE Sovim (novembre 2022)** :
- Accès non libre
- Réservé aux "personnes habilitées" ou démontrant un "intérêt légitime"
- **Ne pas exposer dans le SaaS sans avis juridique préalable**

### 12.5 Diffusion partielle

Les entreprises ayant indiqué `diffusionINSEE = "N"` ou `est_diffusible = false` :
- Tu peux les avoir en BDD pour ton usage propre
- Tu **ne dois pas** les exposer publiquement ou aux autres utilisateurs sans leur consentement
- Implémenter un filtre systématique au niveau de l'API

---

## 13. Checklist par sprint

À utiliser comme **garde-fou** à chaque livraison de fonctionnalité.

### Avant chaque nouvelle feature

- [ ] Toute donnée nouvellement stockée est-elle vraiment **nécessaire** ?
- [ ] Est-ce une donnée personnelle ? Si oui, quelle base légale ?
- [ ] Est-elle chiffrée si sensible ?
- [ ] Y a-t-il une **durée de conservation** définie ?
- [ ] Cette feature touche-t-elle aux droits des personnes ?
- [ ] Est-ce qu'elle nécessite une mise à jour du registre des traitements ?

### Avant chaque push de code

- [ ] Aucun secret dans le commit (vérifié par scanner automatique)
- [ ] Aucun PII dans les logs ajoutés
- [ ] Les inputs sont validés (FluentValidation)
- [ ] Les permissions sont vérifiées (l'utilisateur a-t-il le droit ?)

### Avant chaque release

- [ ] Les dépendances ont été mises à jour (Dependabot)
- [ ] Les tests d'intégration passent
- [ ] Les headers de sécurité sont configurés
- [ ] La politique de confidentialité a-t-elle besoin d'être mise à jour ?
- [ ] Le registre des traitements a-t-il besoin d'être mis à jour ?

---

## 14. Roadmap conformité

### Avant MVP 1 (mois 0–4)

- [ ] Politique de confidentialité rédigée et publiée
- [ ] CGU rédigées et publiées
- [ ] Registre des activités de traitement initialisé
- [ ] Mécanismes techniques de base : chiffrement, hash mots de passe, JWT, HTTPS
- [ ] Implémentation des droits des personnes (accès, rectification, suppression)
- [ ] Bandeau cookies (ou choix Plausible pour éviter)
- [ ] DPAs signés avec les sous-traitants utilisés
- [ ] Liste des sous-traitants publiée

### Avant ouverture publique (entre MVP 1 et MVP 2)

- [ ] Audit de sécurité interne complet
- [ ] Pentest externe (cabinet ou freelance qualifié)
- [ ] Procédure de gestion des violations rédigée
- [ ] Plan de continuité d'activité minimal
- [ ] Revue juridique par un avocat data privacy
- [ ] Test des sauvegardes et de la restauration

### Phase de croissance (mois 6+)

- [ ] DPO désigné (interne ou externe mutualisé)
- [ ] Auto-évaluation annuelle de la conformité
- [ ] DPIA si nouveaux traitements à risque introduits
- [ ] Formation continue sur la sécurité (toi + futurs collaborateurs)
- [ ] Bug bounty si possible

---

## Ressources officielles

- **CNIL** : https://www.cnil.fr/ — autorité de contrôle française
- **Guide CNIL pour les développeurs** : https://www.cnil.fr/fr/la-cnil-publie-un-guide-rgpd-pour-les-developpeurs
- **CNIL — Outils et guides** : modèles de registre, MOOC RGPD gratuit
- **EDPB (European Data Protection Board)** : https://edpb.europa.eu/
- **ANSSI** : https://www.ssi.gouv.fr/ — recommandations sécurité françaises
- **OWASP** : https://owasp.org/ — Top 10, ASVS, cheat sheets

---

*Document évolutif. À mettre à jour à chaque évolution majeure du produit ou de la réglementation.*
