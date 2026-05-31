# Vocabulaire ubiquitaire — projet Atlas

> Glossaire de référence définissant le **vocabulaire ubiquitaire** du projet (au sens du Domain-Driven Design d'Eric Evans).
> Ce document est **prescriptif** : tout code, toute documentation, toute interface utilisateur doit utiliser exclusivement les termes définis ici. Aucun synonyme ou variante ne doit être introduit sans avoir été ajouté ici en amont.

**Version** : 1.0
**Date de dernière mise à jour** : 26 mai 2026
**Nom de code projet** : Atlas (provisoire, sera remplacé)

---

## Sommaire

- [1. Principes et règles linguistiques](#1-principes-et-règles-linguistiques)
- [2. Conventions de nommage C#](#2-conventions-de-nommage-c)
- [3. Identifiants et value objects](#3-identifiants-et-value-objects)
- [4. Utilisateurs et comptes](#4-utilisateurs-et-comptes)
- [5. Entités légales et entreprises](#5-entités-légales-et-entreprises)
- [6. Propriété intellectuelle](#6-propriété-intellectuelle)
- [7. Documents](#7-documents)
- [8. Recherche et favoris](#8-recherche-et-favoris)
- [9. Veille et agrégation de contenu](#9-veille-et-agrégation-de-contenu)
- [10. Notifications et alertes](#10-notifications-et-alertes)
- [11. Sources de données externes (ports et adapters)](#11-sources-de-données-externes-ports-et-adapters)
- [12. Concepts techniques transverses](#12-concepts-techniques-transverses)
- [13. Faux-amis et pièges fréquents](#13-faux-amis-et-pièges-fréquents)
- [14. Maintenance du vocabulaire](#14-maintenance-du-vocabulaire)

---

## 1. Principes et règles linguistiques

### 1.1 Approche : hybride pragmatique

Le projet adopte une **approche linguistique hybride** entre anglais et français, suivant des règles strictes documentées ci-dessous. Ce choix résulte de l'analyse de l'ADR-007 (stack .NET) et du fait que le projet manipule des concepts spécifiquement français (RNE, INPI, droit administratif) tout en visant une communauté open source potentiellement internationale.

### 1.2 Règles de choix de langue

| Type de concept | Langue | Exemples |
|---|---|---|
| Concepts techniques génériques | **EN** | `Repository`, `Service`, `Handler`, `Query`, `Command`, `Event`, `Result` |
| Concepts métier ayant une traduction propre et standard | **EN** | `Company`, `Trademark`, `Patent`, `Address`, `User`, `Account` |
| Concepts spécifiquement français sans équivalent net | **FR** | `UniteLegale`, `Etablissement`, `FormeJuridique`, `Mandataire`, `Dirigeant` |
| Acronymes officiels français | **FR** | `Siren`, `Siret`, `Naf`, `Apet`, `Rne`, `Bopi`, `Bodacc`, `Inpi` |
| Acronymes officiels européens / internationaux | **selon usage** | `Eutm` (EU Trademark), `Eori`, `Cpc` |

### 1.3 Règle de composition

Les mots composés mélangent souvent les deux langues : `BopiPublication`, `RneCompanyData`, `InpiCredentials`, `EtablissementAddress`. **C'est volontaire et accepté.** L'objectif est la clarté, pas la pureté linguistique.

### 1.4 Règle de stabilité

Une fois un terme adopté, il **ne change pas** sans procédure explicite (voir §14). Ne jamais introduire un synonyme silencieusement.

---

## 2. Conventions de nommage C#

### 2.1 Conventions Microsoft standard

- **PascalCase** pour : types (`class`, `interface`, `struct`, `enum`), propriétés, méthodes, constantes publiques
- **camelCase** pour : paramètres, variables locales, champs privés (sans préfixe `_` selon goût d'équipe)
- **`I`** préfixe pour les **interfaces** : `ICompanyRepository`, `ITrademarkProvider`
- **`Async`** suffixe pour les méthodes asynchrones : `GetByIdAsync`, `SaveAsync`
- **Pas de préfixe de visibilité** ni notation hongroise : interdit `m_field`, `_field` discutable

### 2.2 Suffixes sémantiques (standards du projet)

| Suffixe | Signification | Exemple |
|---|---|---|
| `Repository` | Accès persistance d'une entité | `ICompanyRepository` |
| `Service` | Service métier (orchestration) | `CompanySearchService` |
| `Provider` | Adapter sortant vers une source externe | `InpiRneCompanyProvider` |
| `Handler` | Gestionnaire de commande / requête (MediatR pattern) | `GetCompanyByIdHandler` |
| `Query` | Requête métier (lecture) | `GetCompanyBySirenQuery` |
| `Command` | Commande métier (écriture) | `AddCompanyToFavoritesCommand` |
| `Event` | Événement de domaine | `CompanyAddedToFavoritesEvent` |
| `Dto` | Objet de transfert (entrée/sortie API) | `CompanyDto`, `CreateUserDto` |
| `ViewModel` | Vue côté MAUI / UI | `CompanyDetailsViewModel` |
| `Spec` | Spécification (pattern Specification) | `ActiveCompaniesSpec` |
| `Policy` | Règle métier | `UserAccessPolicy` |
| `Factory` | Construction complexe | `CompanyFactory` |
| `Result` | Résultat d'une opération | `SearchResult<Company>` |

### 2.3 Conventions de fichiers

- **Un type par fichier** : `Company.cs` contient `class Company`
- **Nom de fichier = nom du type**
- **Namespaces** alignés sur l'arborescence : `Atlas.Domain.Companies/Company.cs` → `namespace Atlas.Domain.Companies`

---

## 3. Identifiants et value objects

Les **value objects** sont des types immuables sans identité propre, qui encapsulent une valeur et ses règles métier (validation, formatage, comparaison). En C# 9+, on les implémente en `record struct` ou `record class` selon le besoin.

### 3.1 Siren

| | |
|---|---|
| Nom canonique | `Siren` |
| Type | Value Object (record struct) |
| Langue | FR (acronyme officiel) |
| Définition | Identifiant unique d'une **unité légale** française (entreprise ou personne morale). 9 chiffres. |
| Format | 9 chiffres décimaux, avec contrôle Luhn |
| Exemple | `552032534` (Renault SA) |
| Validation | Vérification longueur 9, tous chiffres, contrôle clé de Luhn |
| Exemple C# | `public readonly record struct Siren(string Value)` |
| Pièges | Ne pas confondre avec **Siret** (qui contient le Siren + un suffixe d'établissement) |

### 3.2 Siret

| | |
|---|---|
| Nom canonique | `Siret` |
| Type | Value Object (record struct) |
| Langue | FR |
| Définition | Identifiant unique d'un **établissement** d'une unité légale. Siren + 5 chiffres (NIC). 14 chiffres total. |
| Format | 14 chiffres décimaux, avec contrôle Luhn sur l'ensemble |
| Exemple | `55203253400646` (Renault SA, siège social) |
| Validation | Idem Siren, longueur 14, Luhn |
| Composition | `Siret = Siren + Nic` |
| Exemple C# | `public readonly record struct Siret(string Value) { public Siren ToSiren() => new(Value[..9]); }` |

### 3.3 Nic

| | |
|---|---|
| Nom canonique | `Nic` |
| Type | Value Object |
| Langue | FR |
| Définition | Numéro Interne de Classement. Suffixe de 5 chiffres dans le Siret identifiant l'établissement au sein de l'unité légale. |
| Format | 5 chiffres |
| Note | Rarement utilisé seul ; généralement intégré dans `Siret` |

### 3.4 Naf

| | |
|---|---|
| Nom canonique | `Naf` |
| Type | Value Object |
| Langue | FR |
| Définition | Code Nomenclature des Activités Françaises. Identifie l'activité principale d'une entreprise. |
| Format | 5 caractères : 4 chiffres + 1 lettre (ex. `6202A`) |
| Synonyme à éviter | "NAF rev 2", "APE" (code APE est dérivé du NAF) |
| Exemple C# | `public readonly record struct Naf(string Code, string Label)` |

### 3.5 Ape

| | |
|---|---|
| Nom canonique | `Ape` |
| Type | Value Object |
| Langue | FR |
| Définition | Activité Principale Exercée. Désignation d'activité attribuée par l'INSEE, basée sur la nomenclature NAF. |
| Note | Souvent confondu avec Naf ; en pratique, l'`Ape` est le code NAF appliqué à une entité |
| Recommandation | **Utiliser `Naf` plutôt qu'`Ape`** sauf besoin métier spécifique |

### 3.6 Tva (Numéro de TVA intracommunautaire)

| | |
|---|---|
| Nom canonique | `TvaNumber` ou `IntracommunityVatNumber` |
| Type | Value Object |
| Langue | EN (concept européen) |
| Définition | Numéro de TVA intracommunautaire d'une entreprise française. `FR` + 2 chiffres + Siren |
| Format | 13 caractères : `FR XX XXXXXXXXX` |
| Exemple | `FR40552032534` |

### 3.7 Identifiants PI

| Concept | Nom canonique | Type | Notes |
|---|---|---|---|
| Numéro de dépôt marque/brevet/D&M | `DepositNumber` | Value Object | EN — universel |
| Numéro de publication brevet | `PublicationNumber` | Value Object | Format : préfixe pays + numéro (ex. `EP3813503`) |
| Numéro de dépôt INPI brevet français | `FrPatentDepositNumber` | Value Object | Préfixe `FR` |
| Numéro d'enregistrement marque française | `FrTrademarkRegistrationNumber` | Value Object | |
| Numéro d'enregistrement marque UE | `EutmRegistrationNumber` | Value Object | EUIPO |
| Numéro WIPO | `WipoRegistrationNumber` | Value Object | OMPI |

### 3.8 Identifiants techniques

| Concept | Nom canonique | Type | Notes |
|---|---|---|---|
| Identifiant unique technique | `Id` ou typé : `CompanyId`, `UserId` | Value Object typé | Préférer les IDs typés (strong typing) plutôt qu'un `Guid` partout |
| GUID | `Guid` | type natif .NET | Format standard |
| Identifiant numérique externe | `ExternalId` | Value Object | Pour les IDs provenant d'APIs externes |

### 3.9 Classifications PI

| Concept | Nom canonique | Type | Notes |
|---|---|---|---|
| Classification de Nice (marques) | `NiceClassification` | Value Object | 45 classes |
| Classification de Vienne (marques figuratives) | `ViennaClassification` | Value Object | |
| Classification CPC (brevets) | `CpcClassification` | Value Object | Cooperative Patent Classification |
| Classification IPC (brevets) | `IpcClassification` | Value Object | International Patent Classification |
| Classification de Locarno (D&M) | `LocarnoClassification` | Value Object | |

---

## 4. Utilisateurs et comptes

### 4.1 User

| | |
|---|---|
| Nom canonique | `User` |
| Type | Entité (Aggregate Root) |
| Langue | EN |
| Définition | Utilisateur du SaaS Atlas. Personne physique inscrite avec un email et capable de se connecter. |
| Identité | `UserId` (Guid) |
| Propriétés clés | `Email`, `PasswordHash`, `CreatedAt`, `Status` |
| Pièges | Ne pas confondre avec `Account` (qui regroupe la facturation, les souscriptions) |

### 4.2 Account

| | |
|---|---|
| Nom canonique | `Account` |
| Type | Entité |
| Langue | EN |
| Définition | Compte d'abonnement / de facturation. Peut être lié à un ou plusieurs users (futur : équipes). |
| Note | En MVP 1, **1 User = 1 Account** (relation 1-1). Cette séparation prépare le multi-user d'équipe en V3+ |

### 4.3 UserProfile

| | |
|---|---|
| Nom canonique | `UserProfile` |
| Type | Entité (rattachée à User) |
| Langue | EN |
| Définition | Profil affiché et préférences de l'utilisateur (langue, fuseau horaire, préférences d'accessibilité, etc.) |

### 4.4 InpiCredentials

| | |
|---|---|
| Nom canonique | `InpiCredentials` |
| Type | Value Object (chiffré en BDD) |
| Langue | mix : Inpi (FR acronyme) + Credentials (EN) |
| Définition | Identifiants INPI personnels de l'utilisateur, chiffrés via le KMS (cf. doc 04). Utilisés pour authentifier les requêtes INPI au nom du user (modèle multi-tenant, ADR-003). |
| Propriétés | `Username` (chiffré), `Password` (chiffré), `LastTested`, `Status` |
| Pièges | Ne JAMAIS logger en clair. Ne JAMAIS retourner via une API. Accès strictement limité au service d'orchestration INPI. |
| À distinguer de | `InpiAccessCredentials` : identifiants **en clair**, transients (en mémoire le temps d'un appel RNE), obtenus en déchiffrant `InpiCredentials`. Jamais persistés ni loggés. |

### 4.5 Session

| | |
|---|---|
| Nom canonique | `Session` |
| Type | Entité |
| Langue | EN |
| Définition | Session active d'un utilisateur (JWT + refresh token associé). |

### 4.6 Concepts liés

| Concept | Nom canonique | Notes |
|---|---|---|
| Adresse email | `EmailAddress` | Value object validé et normalisé (minuscules, trim). Identifiant de connexion, unique par user |
| État du compte user | `UserStatus` | Énum : `PendingEmailVerification`, `Active`, `Suspended` |
| Mot de passe | `Password` (en input), `PasswordHash` (en stockage) | Jamais en clair. Hash Argon2id (ADR-010) |
| Token JWT | `AccessToken` | Émis en RS256 (ADR-010) |
| Token de rafraîchissement | `RefreshToken` | Rotatif, révocable, seul le hash est persisté |
| Code 2FA TOTP | `TotpCode` | Vérifié via `ITotpProvider` (RFC 6238) |
| Code de récupération 2FA | `RecoveryCode` / `TwoFactorRecoveryCode` | 10 codes à usage unique, seul le hash est persisté |
| Secret TOTP partagé | `TwoFactorSecret` | Chiffré au repos (AES-256-GCM via `ICryptoService`) |
| Jeton de défi 2FA | `TwoFactorChallengeToken` | JWT court (audience `atlas-2fa`) émis après mot de passe valide, à échanger contre les jetons d'accès via /auth/2fa/verify |
| Session INPI | `InpiSession` | JWT Bearer RNE (+ expiration) obtenu via `IInpiAuthenticationProvider` ; jamais persisté |

---

## 5. Entités légales et entreprises

### 5.1 Company (générique)

| | |
|---|---|
| Nom canonique | `Company` |
| Type | Entité (Aggregate Root) |
| Langue | EN |
| Définition | Représentation **générique** d'une entité du domaine entreprises. Utilisée comme racine d'agrégat dans la couche Application pour les use cases courants. |
| Pièges | Ne contient pas toutes les nuances administratives françaises (forme juridique, statuts détaillés). Pour ces nuances, utiliser `UniteLegale`. |

### 5.2 UniteLegale

| | |
|---|---|
| Nom canonique | `UniteLegale` |
| Type | Entité (Aggregate Root) |
| Langue | FR (concept administratif français) |
| Définition | Représentation **fidèle** d'une unité légale au sens du RNE / INSEE. Une entité juridique unique (société ou personne physique exerçant une activité). |
| Identité | `Siren` |
| Note | Modèle riche utilisé dans l'adapter INPI/RNE et dans la persistance détaillée. La couche Application travaille souvent avec `Company` (vue simplifiée) ou `UniteLegale` (vue complète) selon le use case. |

### 5.3 Etablissement

| | |
|---|---|
| Nom canonique | `Etablissement` |
| Type | Entité |
| Langue | FR |
| Définition | Lieu d'activité d'une unité légale. Une unité légale peut avoir plusieurs établissements (siège + secondaires). |
| Identité | `Siret` |
| Rattachement | Référence `UniteLegale` parent via son `Siren` |

### 5.4 FormeJuridique

| | |
|---|---|
| Nom canonique | `FormeJuridique` |
| Type | Value Object / Enum |
| Langue | FR |
| Définition | Forme juridique de l'unité légale : SAS, SARL, EURL, Auto-entrepreneur, Association, etc. |
| Note | Liste référentielle INSEE (code numérique + label). À gérer comme une table de référence en BDD. |
| Exemple C# | `public class FormeJuridique { public string Code { get; } public string Label { get; } }` |

### 5.5 Dirigeant

| | |
|---|---|
| Nom canonique | `Dirigeant` |
| Type | Entité |
| Langue | FR |
| Définition | Personne physique ou morale exerçant une fonction de direction au sein d'une unité légale. |
| Propriétés clés | `Nom`, `Prenoms`, `DateNaissance`, `Nationalite`, `Fonction`, `DateDebutFonction` |
| Note | Synonyme métier : "mandataire social". On utilise `Dirigeant` qui est plus courant. |
| Pièges RGPD | Les données de dirigeants personnes physiques sont des **données personnelles**. Traitement encadré par le RGPD (cf. doc 04). |

### 5.6 Mandataire

| | |
|---|---|
| Nom canonique | `Mandataire` |
| Type | Entité |
| Langue | FR |
| Définition | Personne ou cabinet (avocat, expert-comptable) mandaté pour agir au nom d'une entité (dépôt INPI, etc.). |
| Distinction | Différent de `Dirigeant` : un mandataire agit POUR le compte de quelqu'un, un dirigeant EST décideur |

### 5.7 Address

| | |
|---|---|
| Nom canonique | `Address` |
| Type | Value Object |
| Langue | EN |
| Définition | Adresse postale française normalisée (au format BAN). |
| Propriétés | `Street`, `PostalCode`, `City`, `Country`, `Latitude`, `Longitude` |
| Pièges | Une adresse peut changer dans le temps. La modéliser comme immutable ; les changements créent une nouvelle instance. |

### 5.8 Activity / Activite

| | |
|---|---|
| Nom canonique | `Activity` |
| Type | Value Object |
| Langue | EN (concept générique) |
| Définition | Activité économique de l'entité, codifiée par un `Naf` et libellée. |
| Distinction | Une entité a une **activité principale** (`MainActivity`) et peut avoir des activités secondaires. |

### 5.9 CompanyStatus

| | |
|---|---|
| Nom canonique | `CompanyStatus` |
| Type | Enum |
| Langue | EN |
| Valeurs | `Active`, `Ceased`, `InRedressment` (redressement judiciaire), `InLiquidation`, `Dissolved`, `Suspended` |
| Note | Mapping depuis les codes de statut INSEE / RNE |

### 5.10 BeneficiaireEffectif (BE)

| | |
|---|---|
| Nom canonique | `BeneficiaireEffectif` |
| Type | Entité |
| Langue | FR |
| Définition | Personne physique exerçant un contrôle effectif sur une entité (≥25% des parts ou droits de vote, ou contrôle par tout autre moyen). |
| Pièges | **Accès restreint depuis arrêt CJUE Sovim (nov. 2022)**. À ne pas exposer publiquement dans le SaaS sans avis juridique préalable. Voir doc 04. |

### 5.11 Concepts annexes

| Concept | Nom canonique | Langue | Notes |
|---|---|---|---|
| Date de création | `CreationDate` | EN | Date d'immatriculation |
| Date de cessation | `CessationDate` | EN | Date de radiation / cessation |
| Capital social | `ShareCapital` | EN | Montant en euros |
| Nombre de salariés | `EmployeeCount` ou `Workforce` | EN | Souvent en tranche (TEFEN) |

---

## 6. Propriété intellectuelle

### 6.1 IntellectualPropertyAsset

| | |
|---|---|
| Nom canonique | `IntellectualPropertyAsset` (alias `IpAsset`) |
| Type | Classe abstraite |
| Langue | EN |
| Définition | Concept générique englobant les marques, brevets, dessins & modèles. Permet une vue unifiée d'un portefeuille IP. |
| Sous-types | `Trademark`, `Patent`, `Design` |

### 6.2 Trademark

| | |
|---|---|
| Nom canonique | `Trademark` |
| Type | Entité (Aggregate Root) |
| Langue | EN |
| Définition | Marque de fabrique, de commerce ou de service, déposée auprès d'un office (INPI, EUIPO, OMPI). |
| Identité | `RegistrationNumber` (préfixé par origine : FR, EU, WO) |
| Propriétés clés | `Name`, `Owner` (déposant), `FilingDate`, `RegistrationDate`, `ExpirationDate`, `NiceClasses`, `Status` |
| Pièges | Une marque peut être verbale, figurative, semi-figurative, 3D, sonore, etc. → `TrademarkType` |

### 6.3 Patent

| | |
|---|---|
| Nom canonique | `Patent` |
| Type | Entité (Aggregate Root) |
| Langue | EN |
| Définition | Brevet d'invention ou certificat d'utilité, déposé auprès d'un office (INPI, OEB, OMPI). |
| Identité | `PublicationNumber` |
| Propriétés clés | `Title`, `Abstract`, `Inventors`, `Applicants`, `FilingDate`, `PublicationDate`, `IpcClassifications` |

### 6.4 Design

| | |
|---|---|
| Nom canonique | `Design` |
| Type | Entité |
| Langue | EN |
| Définition | Dessin & Modèle protégé (apparence d'un produit). En français : "dessin ou modèle". |
| Identité | `DepositNumber` + `SequenceNumber` |
| Note | Un même dépôt peut contenir plusieurs designs distincts |

### 6.5 Reproduction

| | |
|---|---|
| Nom canonique | `DesignReproduction` |
| Type | Value Object |
| Langue | EN |
| Définition | Image / représentation visuelle d'un design. Un design peut avoir plusieurs reproductions (vues différentes). |

### 6.6 BopiPublication

| | |
|---|---|
| Nom canonique | `BopiPublication` |
| Type | Entité |
| Langue | Mix : Bopi (FR acronyme) + Publication (EN) |
| Définition | Publication au Bulletin Officiel de la Propriété Industrielle (BOPI). Outil de publicité légale de l'INPI. |
| Note | Source de données importante pour la veille (cf. doc 02 F-048) |

### 6.7 Opposition

| | |
|---|---|
| Nom canonique | `TrademarkOpposition` |
| Type | Entité |
| Langue | EN |
| Définition | Procédure d'opposition à l'enregistrement d'une marque, déposée par un tiers titulaire d'une marque antérieure. |
| Phase MVP | V3+ |

### 6.8 Termes spécifiques à la PI

| Terme français | Nom canonique | Langue | Notes |
|---|---|---|---|
| Titulaire | `Owner` ou `Holder` | EN | Personne possédant le droit |
| Déposant | `Applicant` | EN | Personne ayant fait le dépôt |
| Inventeur (brevet) | `Inventor` | EN | Personne physique ayant inventé |
| Renouvellement | `Renewal` | EN | Action de prolonger une marque |
| Annuité (brevet) | `PatentAnnuity` | EN | Taxe annuelle pour maintenir un brevet |
| Antériorité | `PriorArt` (brevet) / `EarlierTrademark` (marque) | EN | |
| Fascicule | `PatentFascicle` | mix | Document détaillé d'un brevet |
| Notice | `Notice` | EN | Données bibliographiques d'un titre PI |

---

## 7. Documents

### 7.1 Document

| | |
|---|---|
| Nom canonique | `Document` |
| Type | Entité abstraite |
| Langue | EN |
| Définition | Document juridique ou financier rattaché à une entité (entreprise ou titre PI). |
| Sous-types | `LegalAct`, `FinancialStatement`, `PatentFascicle`, etc. |

### 7.2 LegalAct (Acte)

| | |
|---|---|
| Nom canonique | `LegalAct` |
| Type | Entité |
| Langue | EN (avec alias `Acte` autorisé dans les contextes franco-français) |
| Définition | Acte juridique d'une entreprise : statuts, modifications, procès-verbaux, cessions. |
| Source | INPI RNE — endpoint `/companies/{siren}/attachments/actes` |
| Format de stockage | PDF |

### 7.3 FinancialStatement (Bilan)

| | |
|---|---|
| Nom canonique | `FinancialStatement` |
| Type | Entité |
| Langue | EN |
| Définition | Comptes annuels déposés par une entreprise : bilan, compte de résultat, annexes. |
| Propriétés | `FiscalYear`, `ClosureDate`, `FilingDate`, `IsConfidential`, `DocumentUrl` |
| Source | INPI RNE — endpoint `/companies/{siren}/attachments/bilans` |

### 7.4 PatentFascicle

| | |
|---|---|
| Nom canonique | `PatentFascicle` |
| Type | Entité |
| Langue | EN |
| Définition | Document original d'un brevet contenant la description complète de l'invention, les revendications, dessins. |
| Source | INPI PI — endpoint `/brevets/document/{id}` |

### 7.5 Types spécifiques

| Concept | Nom canonique | Notes |
|---|---|---|
| Statuts d'entreprise | `CompanyByLaws` | Document fondateur |
| Procès-verbal | `MinutesDocument` | PV d'assemblée |
| Cession de parts | `ShareTransferAct` | |

---

## 8. Recherche et favoris

### 8.1 SearchQuery

| | |
|---|---|
| Nom canonique | `SearchQuery` |
| Type | Value Object |
| Langue | EN |
| Définition | Requête de recherche structurée (terme, filtres, tri). |
| Polymorphisme | `CompanySearchQuery`, `TrademarkSearchQuery`, `PatentSearchQuery` |

### 8.2 SearchResult

| | |
|---|---|
| Nom canonique | `SearchResult<T>` |
| Type | DTO générique |
| Langue | EN |
| Définition | Conteneur de résultats paginés avec métadonnées (total, page, taille). |

### 8.3 Favorite

| | |
|---|---|
| Nom canonique | `Favorite` |
| Type | Entité |
| Langue | EN |
| Définition | Référence sauvegardée par un user vers une entité (Company, Trademark, Patent). |
| Polymorphisme | `CompanyFavorite`, `TrademarkFavorite`, `PatentFavorite` |
| Note | Permet ensuite la veille et les alertes sur cette entité (cf. F-019, F-047) |

### 8.4 SearchHistory

| | |
|---|---|
| Nom canonique | `SearchHistoryEntry` |
| Type | Entité |
| Langue | EN |
| Définition | Entrée dans l'historique de recherche d'un user. |
| Conservation | Configurable, défaut 12 mois (cf. doc 04) |

### 8.5 Annotation

| | |
|---|---|
| Nom canonique | `UserAnnotation` |
| Type | Entité |
| Langue | EN |
| Définition | Note personnelle d'un user sur une entité (Company, Trademark, etc.). Privé par défaut. |
| Phase | V2 (F-030) |

---

## 9. Veille et agrégation de contenu

### 9.1 ExternalContentSource (Port)

| | |
|---|---|
| Nom canonique | `IExternalContentSource` |
| Type | Interface (Port) |
| Langue | EN |
| Définition | **Port de domaine** abstrait représentant une source externe de contenu daté (RSS, BODACC, BOPI, etc.). Toute source externe implémente cette interface. |
| Méthodes clés | `FetchItemsAsync(DateTimeOffset since)`, `GetMetadata()` |

### 9.2 FeedSource (entité)

| | |
|---|---|
| Nom canonique | `FeedSource` |
| Type | Entité |
| Langue | EN |
| Définition | Représentation persistante d'une source de contenu suivie par un user : URL, type, fréquence de polling, dernier fetch. |
| Sous-types possibles | `RssFeedSource`, `BodaccFeedSource`, `InpiBopiFeedSource` |

### 9.3 FeedItem

| | |
|---|---|
| Nom canonique | `FeedItem` |
| Type | Entité |
| Langue | EN |
| Définition | Élément unitaire récupéré d'une source : un article RSS, une annonce BODACC, une publication BOPI. |
| Propriétés clés | `Title`, `Url`, `Summary`, `PublishedAt`, `SourceId`, `Categories`, `Hash` (pour dédup) |

### 9.4 VeillePack (Template de veille)

| | |
|---|---|
| Nom canonique | `VeillePack` |
| Type | Entité |
| Langue | FR (concept produit spécifique à Atlas) |
| Définition | Pack pré-curé de sources de veille destiné à un segment professionnel (Cabinet PI, Expert-comptable, etc.). L'utilisateur peut s'abonner en un clic. |
| Note | Cf. doc 02 F-042 et doc 07 (catalogue des packs) |
| Choix linguistique | "Veille" est un terme français riche difficilement traduisible (les anglais utilisent "monitoring", "watch", "intelligence" qui couvrent chacun une partie du sens). On garde le français. |

### 9.5 VeilleSubscription

| | |
|---|---|
| Nom canonique | `VeilleSubscription` |
| Type | Entité |
| Langue | mix |
| Définition | Abonnement d'un user à un VeillePack ou à un FeedSource individuel. |

### 9.6 Timeline

| | |
|---|---|
| Nom canonique | `Timeline` |
| Type | Value Object / Projection |
| Langue | EN |
| Définition | Vue unifiée chronologique des FeedItems d'un user, mélangée avec les évolutions de ses Favorites. |
| Cf. | F-044, F-047 |

### 9.7 ItemCluster (Déduplication)

| | |
|---|---|
| Nom canonique | `FeedItemCluster` |
| Type | Entité |
| Langue | EN |
| Définition | Regroupement d'items similaires détectés par déduplication (même info reportée par plusieurs sources). |
| Cf. | F-045 |

### 9.7.1 SimHash (empreinte de déduplication)

| | |
|---|---|
| Nom canonique | `SimHash` |
| Type | Concept technique (value/algorithme) |
| Langue | EN |
| Définition | Empreinte floue 64 bits d'un texte (titre + extrait). Deux textes proches ont une faible distance de Hamming, base de l'appariement des `FeedItemCluster`. |
| Cf. | F-045 |

### 9.8 WatchRule (règle de surveillance)

| | |
|---|---|
| Nom canonique | `WatchRule` |
| Type | Entité |
| Langue | EN |
| Définition | Règle définie par un user pour générer des alertes (mots-clés, sources, entreprises favorites). |
| Cf. | F-046 |

### 9.9 VeillePackItem

| | |
|---|---|
| Nom canonique | `VeillePackItem` |
| Type | Type possédé (owned) |
| Langue | mix |
| Définition | Lien entre un `VeillePack` et une `FeedSource` partagée. Compose l'agrégat `VeillePack`. |
| Cf. | F-042 |

### 9.10 VeillePackEnrollment

| | |
|---|---|
| Nom canonique | `VeillePackEnrollment` |
| Type | Entité |
| Langue | mix |
| Définition | Inscription d'un user à un `VeillePack`, avec la version du pack appliquée. Permet de détecter qu'une nouvelle version est disponible et de re-synchroniser les abonnements. |
| Note | On évite `Application` (collision avec la couche `Atlas.Application`) ; « Enrollment » désigne l'acte d'appliquer un pack à un compte. |
| Cf. | F-042 |

### 9.11 FeedItemUserState

| | |
|---|---|
| Nom canonique | `FeedItemUserState` |
| Type | Entité |
| Langue | mix |
| Définition | État d'un `FeedItem` pour un user dans sa timeline : lu/non-lu, favori, archivé. L'absence d'instance vaut « non-lu, non-favori, non-archivé » (créée au premier marquage). |
| Cf. | F-044 |

---

## 10. Notifications et alertes

### 10.1 Notification

| | |
|---|---|
| Nom canonique | `Notification` |
| Type | Entité |
| Langue | EN |
| Définition | Message destiné à un user, lié à un événement (alerte favori, item de veille pertinent, etc.). |
| Statuts | `Unread`, `Read`, `Archived` |

### 10.2 NotificationChannel

| | |
|---|---|
| Nom canonique | `NotificationChannel` |
| Type | Enum |
| Langue | EN |
| Valeurs | `Email`, `Push`, `InApp`, `Webhook` (V3+) |

### 10.3 Alert

| | |
|---|---|
| Nom canonique | `Alert` |
| Type | Entité |
| Langue | EN |
| Définition | Notification spécifique générée automatiquement à partir d'une `WatchRule` ou d'un changement détecté sur un favori. |
| Distinction | `Notification` est générique, `Alert` est issu d'une règle automatique |

---

## 11. Sources de données externes (ports et adapters)

### 11.1 Convention de nommage des adapters

| Pattern | Exemple |
|---|---|
| `{Source}{Concept}Provider` | `InpiRneCompanyProvider`, `BodaccLegalNoticeProvider` |
| `{Source}{Concept}Adapter` | Synonyme accepté pour insister sur le rôle d'adapter |
| Préfixe `{Source}` | `Inpi`, `Bodacc`, `Insee`, `Euipo`, `Wipo`, `Epo` |

### 11.2 Ports principaux (domaine)

| Port | Définition |
|---|---|
| `ICompanyDataProvider` | Lecture de données entreprises depuis une source |
| `IIntellectualPropertyProvider` | Lecture de données PI |
| `IExternalContentSource` | Lecture de contenu daté (veille) |
| `IDocumentDownloader` | Téléchargement de documents binaires (bilans, fascicules) |

### 11.3 Adapters concrets (infrastructure)

| Adapter | Implémente | Source |
|---|---|---|
| `InpiRneCompanyProvider` | `ICompanyDataProvider` | INPI RNE |
| `InpiPiTrademarkProvider` | `IIntellectualPropertyProvider` | INPI PI marques |
| `InpiPiPatentProvider` | `IIntellectualPropertyProvider` | INPI PI brevets |
| `InpiDocumentDownloader` | `IDocumentDownloader` | INPI |
| `BodaccLegalNoticeProvider` | `IExternalContentSource` | BODACC |
| `RssFeedProvider` | `IExternalContentSource` | RSS / Atom |
| `SireneCompanyProvider` (futur) | `ICompanyDataProvider` | INSEE Sirene |

---

## 12. Concepts techniques transverses

### 12.1 Couches d'architecture

| Couche | Namespace | Définition |
|---|---|---|
| **Domain** | `Atlas.Domain` | Cœur métier : entités, value objects, interfaces de ports |
| **Application** | `Atlas.Application` | Use cases, services applicatifs, DTOs |
| **Application.Premium** | `Atlas.Application.Premium` | Use cases premium (vide en MVP 2, isolation pour open core futur) |
| **Infrastructure** | `Atlas.Infrastructure.*` | Adapters sortants : INPI, persistance, etc. |
| **Api** | `Atlas.Api` | Adapter entrant Web API ASP.NET Core |
| **Maui** | `Atlas.Maui` | Adapter entrant client multi-plateformes |

### 12.2 Conventions transverses

| Concept | Nom canonique | Notes |
|---|---|---|
| Conteneur de résultat | `Result<T>` ou `Result<T, TError>` | Pattern fonctionnel : succès / échec sans exception |
| Erreur métier | `DomainError` | Distincte d'`Exception` |
| Cas de validation | `ValidationResult` | Issue de FluentValidation |
| Pagination | `PagedResult<T>` | `{ Items, Page, PageSize, Total }` |
| Tri | `SortOrder` | Enum `Ascending` / `Descending` |
| Période temporelle | `DateRange` | Value object avec `From` et `To` |
| Lien (URL) | `Uri` natif .NET | Pas de wrapper custom |

### 12.3 Conventions d'audit

| Concept | Nom canonique | Présent sur |
|---|---|---|
| Date de création | `CreatedAt` (UTC) | Toute entité |
| Date de modification | `UpdatedAt` (UTC) | Toute entité mutable |
| Auteur de création | `CreatedBy` (UserId) | Quand pertinent |
| Auteur de modification | `UpdatedBy` (UserId) | Quand pertinent |
| Soft delete | `DeletedAt` (nullable) | Quand applicable |

### 12.4 Substrat de surveillance (ADR-013)

Le substrat de surveillance formalise la tuyauterie commune aux features qui suivent des entités au fil du temps (F-019 RNE, F-048 BODACC, F-057 sanctions, F-031 Judilibre). Deux stratégies départagées par le **critère des disparitions**, plus un runner partagé.

| Terme | Définition | Côté |
|---|---|---|
| **`MonitoredDimension`** (enum) | Quel volet est surveillé (`RneIdentity`, `BodaccAnnouncements`, `Sanctions`, `Judilibre`, …). | Domain |
| **`MonitoredChange`** (record) | Sortie commune des deux stratégies — devient un événement de timeline. Porte `Dimension`, `Kind` (`Added` / `Modified` / `Removed`), `Title`, `Summary`, `ExternalId?`. | Domain |
| **`ChangeKind`** (enum) | `Added` / `Modified` / `Removed`. **`Removed` est impossible côté flux** (un item append-only ne disparaît pas). | Domain |
| **`IStateMonitor<TState>`** | Stratégie n°1 : **état + diff**. Pour les dimensions où les **retraits comptent** (RNE, sanctions). `FetchCurrentStateAsync` + `Diff` retourne ajouts / modifs / retraits. | Domain (port) |
| **`IItemStreamMonitor<TItem>`** | Stratégie n°2 : **flux d'items append-only** (BODACC, Judilibre). `FetchItemsAsync` + `ExternalIdOf` (clé de dédup) + `ToChange`. | Domain (port) |
| **`MonitorContext`** | Contexte d'un cycle de surveillance : credentials (RNE) ou anonyme (BODACC), horloge, observabilité. | Domain |
| **`MonitorRunner`** | Runner d'orchestration partagé : itère le set surveillé, dédup cross-users, isole les échecs, idempotent, émet l'événement, observe. **Extrait à la troisième instance (F-057)**, pas avant. | Application |
| **`ExternalId`** | Identifiant stable issu de la source (numéro d'annonce BODACC, identifiant Judilibre, entrée+version de liste sanctions). Sert à la dédup `(UserId, ExternalId)`. | Domain (VO) |

**Garde-fou** : ne **jamais** fusionner les deux stratégies sous une interface unique « universelle » — perdre les retraits (régression métier) ou stocker un historique non borné (gaspillage) sont les deux échecs à éviter.

### 12.5 Matching conservateur (ADR-014)

Incarnation technique de la doctrine ADR-012 : produire des rapprochements **à vérifier**, jamais des verdicts. L'absence de disposition « confirmé » est **encodée dans le type** — l'état illégal est *irreprésentable*.

| Terme | Définition | Côté |
|---|---|---|
| **`MatchCandidate`** (record) | **Contrat de doctrine partagé par les 4 matchers** (F-026, F-047, F-055, F-031). Porte `Subject` (l'entité suivie), `Target` (ce qu'on a trouvé), `Confidence`, `Basis`. **Aucune disposition « confirmé / verdict »** — un matcher ne peut sortir qu'un candidat à vérifier. | Domain |
| **`MatchConfidence`** (enum) | `Low` / `Medium` / `High`. **Jamais `Certain`**. | Domain |
| **`MatchBasis`** (record) | Pourquoi ça a matché (`Method` : « dénomination exacte », « alias de liste », « phonétique »… ; `Evidence` : l'élément concret trouvé ; `Source` : source + date). Piste d'audit native. | Domain |
| **`MatchTarget`** (union / record) | Ce qui a été trouvé : item presse, entrée de liste sanctions, décision Judilibre, marque similaire. | Domain |
| **`NormalizedName`** | Forme normalisée d'une dénomination (suffixes SA/SAS/SARL gommés, accents et casse, variantes), utilisée par les matchers à base de noms. | Domain (VO) |
| **`ICompanyNameNormalizer`** | Noyau de normalisation **partagé par les 3 matchers à base de noms** (F-047, F-055, F-031). **Pas F-026**. Extraction prévue à l'arrivée du 3ᵉ matcher. | Domain (port) |
| **`INameInTextMatcher`** | Famille n°1 : chercher un nom **connu** dans un texte libre (F-047 presse, F-031 jurisprudence). | Domain (port) |
| **`INameAgainstListMatcher`** | Famille n°2 : rapprocher un nom d'une liste structurée — *record linkage* (F-055 sanctions, F-057 re-screening). | Domain (port) |
| **`ITrademarkSimilarityMatcher`** | Famille n°3 : similarité phonétique / visuelle / conceptuelle de marques + classes de Nice (F-026 antériorité). **Moteur entièrement à part** mais respecte la posture du contrat de doctrine. | Domain / Premium (port) |

**Garde-fou** : ne **jamais** ajouter de disposition « confirmé » au contrat (son absence *est* la décision). Ne **pas** forcer F-026 dans le noyau des noms.

### 12.6 Dossier entreprise — assemblage 360 (ADR-015)

Le dossier 360 (F-056) est un **read-model de composition côté Application** qui assemble des sections indépendantes auto-descriptives. Backbone : **résolution snapshot-first** (lit le substrat ADR-013 pour les entités suivies, à la demande sinon) ; **état porteur de doctrine** par section.

| Terme | Définition | Côté |
|---|---|---|
| **`CompanyDossier`** | Read-model du dossier d'une entreprise. `Subject : Siren` + liste de `DossierSection`. **N'est PAS un agrégat de domaine** (`Company` / `UniteLegale` le restent). | Application (read-model) |
| **`DossierSection`** | Une section du dossier — auto-descriptive (`Kind`, `State`, `AsOf?`, `Provenance?`, `Data?`). | Application (record) |
| **`DossierSectionKind`** (enum) | `Identity` / `Financials` / `PublicContracts` / `Ip` / `Listing` / `Structure` / `Risk` / `Events`. | Application |
| **`SectionState`** (enum, porteur de doctrine) | `Available` (donnée fraîche), `Stale` (présente mais périmée), `Unavailable` (échec / INPI non connecté), `NotApplicable` (sans objet — cotation d'une non-cotée), `Restricted` (existe mais non servi — structure DPIA, comptes confidentiels). **Distingue les 4 sens de « vide »**. | Application |
| **`AsOf`** | Date « as of » **par section** — la fraîcheur du dossier n'est **pas atomique**, c'est un patchwork qu'on présente honnêtement. | Application |
| **`Provenance`** | Source + base de la donnée affichée — auditabilité au niveau de la section. | Application |
| **`IDossierSectionResolver`** | Un résolveur par section : lit un snapshot ADR-013 (entité suivie) ou appelle le use case (à la demande), avec **timeout**, et **mappe tout échec en `Unavailable`** — jamais d'exception qui casse le dossier. | Application (port) |
| **`DossierContext`** | Contexte d'assemblage : identité utilisateur (pour ses credentials INPI le cas échéant), horloge, timeout par section. | Application |
| **`IDossierSummarizer`** | Port **premium** (F-050) — synthèse narrative IA du dossier, descriptive, sans verdict. | Domain (port) / Premium (impl) |

**Garde-fou** : chaque section doit **déclarer correctement son état**. L'UI doit afficher la fraîcheur par section et les 5 états honnêtement — jamais afficher `Unavailable` / `Restricted` comme « rien à signaler ».

### 12.7 Surface agentique MCP (ADR-016)

L'adapter MCP (`Atlas.Mcp`, F-052) est une **surface curée lecture-d'abord**, parallèle à `Atlas.Api` et `Atlas.Maui`, qui appelle les mêmes use cases MediatR. Le travail de sécurité scale avec le privilège : l'étroitesse de la surface **est** la première couche de défense.

| Terme | Définition | Côté |
|---|---|---|
| **`Atlas.Mcp`** | Adapter entrant MCP — projet (ou module) distinct. **Allowlist explicite d'outils**, pas d'exposition 1:1 des handlers, schémas d'entrée étroits. | Adapter |
| **`McpToolDescriptor`** | Métadonnée d'un outil exposé : nom, description **porteuse de doctrine**, schéma d'entrée minimal, scope requis. **Versionné et signé** (défense contre *rug pull* / poisoning). | Adapter |
| **Scopes MCP** | Taxonomie par groupe d'outils, en **moindre privilège progressif**. Socle `mcp:lecture-base` ; élévations ciblées `mcp:favoris.ecriture`, `mcp:veille.ecriture`. Jamais de scope « tout-en-un ». | Adapter / OAuth |
| **« Doctrine inline »** | Principe : `MatchCandidate` et `SectionState` ne sont **jamais aplatis** dans le mapping MCP — le caveat voyage **inline par item** (pas en métadonnée détachable). | Mapping |
| **« Contenu externe = donnée, jamais instruction »** | Principe : le texte externe retourné (presse, décisions, observations RNE, annonces) est **clairement délimité comme donnée**, jamais traité comme consigne — parade à l'**injection de prompt indirecte**. | Mapping |
| **`AuditedToolCall`** | Trace d'audit par appel d'outil : qui (`UserId`), quel outil, quels paramètres, quel résultat (résumé). Extension du Serilog existant. | Logging |
| **« Délégation utilisateur »** | L'adapter s'exécute strictement avec l'identité OAuth de l'utilisateur (ADR-011). Les credentials INPI restent **côté serveur**, déchiffrés en mémoire le temps de la requête — l'agent ne les voit **jamais**. | OAuth / KMS |

**Garde-fous** : pas d'outil « omnibus » à entrée libre. Surface d'écriture étroite + approbation humaine pour chaque écriture. Validation d'audience du jeton (pas de *token passthrough*).

### 12.8 Matching sectoriel par crosswalk éditorial (ADR-020)

Le rattachement d'un texte réglementaire (EUR-Lex, F-063) au **secteur (NAF)** des entités suivies (F-064) repose sur un **crosswalk éditorial** sujet → NAF : aucune table officielle EuroVoc → NACE n'existe, le pont est donc **construit, conservateur, et versionné dans le repo** (le `git diff` *est* l'audit de curation). La sortie reste un `MatchCandidate` (§12.5), jamais un verdict.

| Terme | Définition | Côté |
|---|---|---|
| **`SectorMappingRule`** (record) | Une règle du crosswalk : `Scheme` + `SourceKey` (clé source, stable/opaque) + `SourceLabel` (dénormalisé, pour l'œil du curateur) → `Divisions` (cible 1→N), `Scope`, `Curation`. **Donnée de référence versionnée** (fichier repo seedé en base, comme les `VeillePack` de F-042). | Domain |
| **`SourceScheme`** (enum) | Discriminant de la classification source : `EuroVoc`, `EurLexDirectory`, plus tard `JorfNor`, `FrCode`. La clé source varie selon le schéma ; la **cible est toujours NAF**. | Domain |
| **`NafDivision`** (VO) | Division NAF (**2 chiffres**, ex. `10`) — granularité plancher du crosswalk. Pas la section (trop grossière), pas le 5-positions (fausse précision = violation de doctrine). Les sections se dérivent par préfixe. | Domain |
| **`MappingScope`** (enum) | `Sectoral` / `Horizontal`. Levier **anti-noyade** : un texte horizontal (RGPD, droit du travail, fiscalité) n'est **pas masqué** mais **routé** vers un canal « réglementaire transverse » opt-in. Le bruit se maîtrise au routage, jamais en cachant un fait. | Domain |
| **`Curation`** (record) | Provenance de la règle elle-même : `ValidatedBy`, `ValidatedAt`, `Confidence` (`MappingConfidence`), `Rationale?`. La doctrine « tout a une source » repliée sur la table de référence — une correspondance *est* une affirmation. | Domain |
| **`ISectorClassifier`** | Service de **domaine pur** (pas d'I/O) : `(URIs EuroVoc + directory code) → divisions NAF candidates + Scope`. Charge le crosswalk en mémoire. L'adapter EUR-Lex **ne connaît pas le NAF** : il livre la classif native, le classifier traduit. | Domain (service) |
| **`FeedItemSectorMatch`** | Persistance d'un match sectoriel `FeedItem` ↔ secteur suivi (analogue de `FeedItemFavoriteMatch` de F-047). Porte un `MatchCandidate`, jamais un lien « confirmé ». | Application |

**Garde-fous** : crosswalk **conservateur** (dans le doute, ne pas mapper — un mapping manquant = silence honnête, un mapping faux = bruit + faux verdict) ; **validation humaine** de chaque règle (via PR) ; ne **jamais** fusionner la table éditoriale source→NAF avec la table **officielle** NACE rév.2 ↔ 2.1 (anti-pattern « fusionner deux natures », ADR-013).

### 12.9 Export auditable (ADR-021)

Artefact d'audit d'un dossier d'entité tierce, **scellé** par empreinte + horodatage et **auto-vérifiable**. Posture **« trace vérifiable », jamais « preuve légale »** ; granularité **par fait**, états honnêtes inclus (F-066). Prolonge `Provenance` (ADR-015) jusqu'à l'artefact.

| Terme | Définition | Côté |
|---|---|---|
| **`AuditExport`** | Artefact d'audit d'un dossier (sujet, date de génération, faits, états, intégrité), sérialisé puis scellé côté serveur. | Application |
| **`AuditedFact`** | Un fait exporté avec sa valeur, sa **provenance**, sa **référence source exacte** et son **`AsOf`** — granularité par fait. | Application |
| **`Integrity`** | Bloc d'intégrité : algo de hash, **hash SHA-256 du contenu canonicalisé**, date de scellement, token RFC 3161 optionnel. | Application |

**Garde-fou** : un export **n'affirme jamais une valeur légale** ; il atteste seulement *« voici ce qu'Atlas a vu, et la preuve que le contenu n'a pas changé depuis le scellement »*. Les états (`Unavailable`, `Restricted`…) sont exportés tels quels, jamais aplatis.

### 12.10 Historisation bi-temporelle (ADR-022)

Journal **append-only** des changements (réutilise les `MonitoredChange` d'ADR-013) + reconstruction de l'état d'une entité **à une date passée** (`asOf`). Bi-temporalité ciblée : *temps d'événement* vs *temps d'observation* (F-067/F-068).

| Terme | Définition | Côté |
|---|---|---|
| **`EntityChangeLogEntry`** | Entrée du journal append-only : un `MonitoredChange` horodaté sur deux axes (sujet, dimension, type, delta, `EventTime`, `ObservedAt`, provenance). | Application |
| **`IPointInTimeResolver`** | Port qui rejoue le journal jusqu'à un cutoff sur l'axe choisi pour reconstituer un `CompanyDossier` à une date. | Application (port) |
| **`TimeAxis`** | Axe temporel de reconstruction : *valid time* (événement) vs *transaction time* (observation). | Application |
| **`SectionState.Unobserved`** | **Nouvelle valeur** de l'enum `SectionState` (ADR-015) : période **antérieure au suivi** — trou explicite, jamais aplati en « inchangé ». | Domain |

**Garde-fou** : ne **jamais** combler un trou temporel par de la supposition — avant le début du suivi, l'état est `Unobserved`, pas « stable ». Backfill source (actes RNE, archive BODACC) borné et tracé.

### 12.11 Extraction documentaire (ADR-023)

Extraction de faits depuis les **actes** comme **aide à la lecture** : faits **candidats à vérifier**, **ancrés à la page**, jamais oracle. OCR + extraction **on-infra par défaut** (cloud/BYOAI = premium opt-in). Phasage index (F-069) → extraction (F-070).

| Terme | Définition | Côté |
|---|---|---|
| **`ExtractedFactCandidate`** | Fait candidat extrait d'un acte, **toujours à vérifier**, **ancré à la page** (sujet, `Kind`, valeur structurée, document source, page, `Confidence`). | Application |
| **`ExtractedFactKind`** | Type de fait extrait (`ShareTransfer` / `CapitalChange` / `OfficerChange` …). | Application (enum) |
| **`ExtractionConfidence`** | Score de confiance **affiché** de l'extraction — jamais « confirmé ». | Application |
| **`IDocumentFactExtractor`** | Port d'extraction de faits depuis un document ; **implémentation locale par défaut**, cloud/BYOAI en adapter **premium opt-in**. | Application (port) |
| **Index documentaire** | Capacité de classement (type/date/objet) + OCR plein-texte cherchable + saut à la page (F-069). Réutilise `AttachmentId` (F-013). | Application/Infra |

**Garde-fou** : un fait extrait est un **`ExtractedFactCandidate` « semble … — à vérifier »**, jamais un fait asséné ; les bilans (F-054) ne sont **pas re-parsés**.

### 12.12 Graphe d'écosystème (ADR-024)

Généralisation multi-arêtes de F-034 sur le **même substrat PostgreSQL borné** (1-2 sauts, pas de Neo4j). Arêtes **descriptives, jamais qualifiantes** ; le poids légal entité↔entité (open data léger) est séparé des arêtes impliquant une **personne** (cadre DPIA F-034).

| Terme | Définition | Côté |
|---|---|---|
| **`EdgeKind`** | Énumération des types d'arêtes (`Mandate`, `PublicContract`, `CoFiling`, `SharedAddress`). | Domain |
| **`GraphEdge`** | Arête typée et **qualifiée descriptivement** entre deux nœuds : libellé descriptif, provenance, année. | Domain |
| **`TraversalPolicy`** | Politique de traversée **bornée** : nombre de sauts max, **seuil de degré de hub** (traversée consciente des hubs). | Domain |

**Garde-fou** : une arête **relie**, elle ne **conclut** pas ; résolution nom→SIREN **conservatrice** (ADR-014) ; déposants PI individuels = régime F-034 (DPIA).

### 12.13 Usage agentique génératif (ADR-025)

Deux usages génératifs bâtis sur ADR-016 : **règles en langage naturel** (F-073) et **agent de sourcing** (F-074). Principe : **composer & faire remonter, jamais conclure** ; LLM hors de la boucle déterministe quand possible ; **BYOAI** (inférence côté agent de l'utilisateur).

| Terme | Définition | Côté |
|---|---|---|
| **Composition NL→règle** | Traduction d'une intention en **langage naturel** vers un **brouillon** de `FeedRule`/`WatchRule` structuré, **revu et validé** par l'utilisateur avant exécution déterministe (F-046). Le LLM **compose**, il n'exécute pas. | Application |
| **Sourcing à sortie candidate** | Pattern de l'agent de sourcing : à partir d'une *thèse*, traverse les outils MCP lecture et renvoie une **liste de candidats** `{ faits sourcés + critères matchés }` — **jamais de score, classement ni recommandation**. | Application |

**Garde-fou** : l'orchestration **ne conclut jamais** (cohérent ADR-012) ; **human-in-the-loop lecture-d'abord** ; aucune écriture/activation silencieuse ; les noms de types C# précis restent à arrêter à l'implémentation.

---

## 13. Faux-amis et pièges fréquents

Cette section liste les **erreurs sémantiques courantes** à éviter.

### 13.1 Entreprise vs Société

- En droit français : **toute société** est une entreprise, mais une entreprise n'est pas forcément une société (ex. entrepreneur individuel = entreprise sans société)
- Notre `UniteLegale` couvre tout ; `Company` est notre version générique
- **Ne pas créer de classe `Société`** en parallèle. Utiliser `UniteLegale` avec `FormeJuridique = "Société..."`

### 13.2 SIREN vs SIRET vs RCS

| Terme | Ce que c'est | Ne pas confondre avec |
|---|---|---|
| **SIREN** (9 chiffres) | Identifie une **unité légale** | SIRET (qui inclut l'établissement) |
| **SIRET** (14 chiffres) | Identifie un **établissement** d'une unité légale | RCS |
| **RCS** | Registre du Commerce et des Sociétés (ancien) | Remplacé par le **RNE** depuis 2023 |

### 13.3 Marque vs Brevet vs Dessin & Modèle

| Terme | Protège quoi | Office |
|---|---|---|
| **Marque** (`Trademark`) | Un signe distinctif (nom, logo) | INPI, EUIPO, OMPI |
| **Brevet** (`Patent`) | Une invention technique | INPI, OEB, OMPI |
| **Dessin & Modèle** (`Design`) | L'apparence d'un produit | INPI, EUIPO, OMPI |

### 13.4 Mandataire vs Dirigeant

- `Dirigeant` : exerce une fonction de direction (président, gérant)
- `Mandataire` : agit POUR le compte d'une entité (avocat PI déposant une marque pour son client)
- Ne pas les confondre dans le modèle

### 13.5 BE vs Dirigeant

- Un **dirigeant** est nommé / élu pour diriger
- Un **bénéficiaire effectif** est une personne contrôlant économiquement (≥25%)
- Souvent les deux se recouvrent mais pas toujours
- **Données BE** : régime spécial (cf. doc 04)

### 13.6 Veille vs Surveillance vs Monitoring

- En français, "veille" englobe les trois
- En anglais, on distingue `Watch` (surveillance passive), `Monitoring` (suivi actif), `Intelligence` (analyse)
- **On utilise `Veille` en FR (concept produit) et des sous-termes EN selon le contexte**

### 13.7 Account vs User

- `User` = personne physique
- `Account` = compte de facturation / souscription (peut regrouper plusieurs Users en V3+)
- En MVP 1 : 1-1 mais le concept reste distinct

### 13.8 Notification vs Alert

- `Notification` : générique, créée par un événement
- `Alert` : déclenchée par une `WatchRule` ou une détection de changement
- Toute `Alert` est une `Notification`, mais pas l'inverse

---

## 14. Maintenance du vocabulaire

### 14.1 Processus d'ajout d'un terme

1. **Avant** d'introduire un nouveau concept dans le code, l'ajouter ici
2. Choisir la langue selon les règles du §1.2
3. Documenter : définition, type, exemple, pièges
4. Soumettre la modification en PR avec le code qui l'utilise
5. Si le terme remplace un ancien : marquer l'ancien comme `Deprecated` avec un délai de migration

### 14.2 Processus de modification d'un terme

1. Ouvrir une issue dédiée pour discussion
2. Si validé : mettre à jour ce document + tous les usages (refactor IDE)
3. Documenter le changement dans le CHANGELOG du projet

### 14.3 Cohérence avec l'UI

- Les **labels affichés à l'utilisateur** peuvent différer du nom canonique (raisons UX / accessibilité)
- Exemple : `UniteLegale` en code, "Entreprise" dans l'UI
- Maintenir une **table de mapping** dans les fichiers de localisation (i18n)

### 14.4 Cohérence avec les commits

- Les messages de commit doivent utiliser le vocabulaire ubiquitaire
- Exemple : `feat(domain): add Siren value object with Luhn validation`
- Pas : `feat: ajout du SIREN avec contrôle`

### 14.5 Cohérence avec les tests

- Les noms de tests doivent utiliser le vocabulaire
- Exemple : `UniteLegaleTests.WhenSirenIsInvalid_ShouldThrowDomainError`

---

## Annexe — Index alphabétique des termes

Pour navigation rapide. À maintenir au fil des ajouts.

| Terme | Section |
|---|---|
| `Account` | §4.2 |
| `Address` | §5.7 |
| `Ape` | §3.5 |
| `Alert` | §10.3 |
| `AuditExport` | §12.9 |
| `AuditedFact` | §12.9 |
| `BeneficiaireEffectif` | §5.10 |
| `BopiPublication` | §6.6 |
| `Company` | §5.1 |
| `CompanyStatus` | §5.9 |
| `Curation` | §12.8 |
| `Design` | §6.4 |
| `Dirigeant` | §5.5 |
| `Document` | §7.1 |
| `EdgeKind` | §12.12 |
| `EntityChangeLogEntry` | §12.10 |
| `Etablissement` | §5.3 |
| `ExternalContentSource` (port) | §9.1, §11.2 |
| `ExtractedFactCandidate` | §12.11 |
| `Favorite` | §8.3 |
| `FeedItem` | §9.3 |
| `FeedItemSectorMatch` | §12.8 |
| `FeedSource` | §9.2 |
| `FinancialStatement` | §7.3 |
| `FormeJuridique` | §5.4 |
| `GraphEdge` | §12.12 |
| `IDocumentFactExtractor` (port) | §12.11 |
| `InpiCredentials` | §4.4 |
| `Integrity` | §12.9 |
| `IntellectualPropertyAsset` | §6.1 |
| `IPointInTimeResolver` (port) | §12.10 |
| `ISectorClassifier` | §12.8 |
| `LegalAct` | §7.2 |
| `Mandataire` | §5.6 |
| `MappingScope` | §12.8 |
| `MatchCandidate` | §12.5 |
| `Naf` | §3.4 |
| `NafDivision` | §12.8 |
| `NiceClassification` | §3.9 |
| `Notification` | §10.1 |
| `Patent` | §6.3 |
| `PatentFascicle` | §7.4 |
| `Result<T>` | §12.2 |
| `SearchQuery` | §8.1 |
| `SectorMappingRule` | §12.8 |
| `Session` | §4.5 |
| `Siren` | §3.1 |
| `Siret` | §3.2 |
| `SourceScheme` | §12.8 |
| `TimeAxis` | §12.10 |
| `Timeline` | §9.6 |
| `TraversalPolicy` | §12.12 |
| `Trademark` | §6.2 |
| `TvaNumber` | §3.6 |
| `UniteLegale` | §5.2 |
| `User` | §4.1 |
| `VeillePack` | §9.4 |
| `WatchRule` | §9.8 |

---

*Document évolutif. Toute modification doit être tracée et discutée. Le vocabulaire ubiquitaire est l'un des actifs les plus précieux du projet — il représente notre compréhension partagée du métier.*
