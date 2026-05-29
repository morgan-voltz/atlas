# CLAUDE.md

> Instructions persistantes pour Claude Code et les contributeurs humains du projet **Atlas**.
> Ce fichier est volontairement concis. Pour le détail, suivre les renvois vers `docs/`.

---

## Qu'est-ce qu'Atlas

Atlas est un **SaaS open source** développé en C#/.NET 10, qui agrège des données publiques françaises sur les entreprises (RNE) et la propriété industrielle (marques, brevets, dessins & modèles) accessibles via les APIs INPI. Il combine ces données avec un **moteur de veille** qui agrège flux RSS, BODACC, BOPI et actualités sectorielles. Architecture **hexagonale** (Ports & Adapters), backend **ASP.NET Core**, client multi-plateforme **MAUI**. Licence **AGPL v3**.

**Nom de code** : `Atlas` est un nom de code provisoire. Le nom définitif sera décidé ultérieurement et un refactoring de masse sera effectué (renommage de solution, namespaces, repo).

---

## Documents fondateurs (lecture obligatoire selon le contexte)

Avant toute contribution significative, consulter le document approprié dans `docs/`.

| Quand tu fais... | Lis d'abord |
|---|---|
| Une décision d'architecture ou un nouveau choix structurant | `docs/01-decisions-architecturales.md` |
| Une nouvelle feature | `docs/02-roadmap-features.md` |
| Une intégration d'une nouvelle API publique | `docs/03-catalogue-apis-publiques.md` |
| Tout ce qui touche aux credentials INPI, données personnelles, ou stockage de secrets | `docs/04-securite-rgpd.md` |
| La gestion du repo, des branches, ou de la CI | `docs/05-strategie-repos.md` |
| Toute interface utilisateur (MAUI, API publique) | `docs/06-accessibilite.md` |
| Une source de veille (RSS, BODACC, etc.) | `docs/07-flux-rss-veille.md` |
| Nommer une classe, méthode, ou concept métier | `docs/08-vocabulaire-ubiquitaire.md` |
| Comprendre où placer du code (couche, projet) | `docs/09-architecture-detaillee.md` |
| Créer un nouveau projet `.csproj` ou modifier la solution | `docs/10-layout-solution-dotnet.md` |
| Ajouter, modifier ou supprimer un endpoint HTTP | `docs/11-api-endpoints.md` |

---

## Stack technique

| Composant | Choix |
|---|---|
| Runtime | .NET 10 LTS (supporté jusqu'à novembre 2028) |
| Langage | C# 13 (file-scoped namespaces, primary constructors, etc.) |
| Backend Web | ASP.NET Core 10 + Minimal APIs |
| Client | .NET MAUI 10 (Android, iOS, Windows, macOS) |
| Base de données | PostgreSQL via EF Core 10 + Npgsql |
| Messaging in-process | MediatR 13 (CQRS léger) |
| Validation | FluentValidation 12 |
| Résilience | Polly 8 |
| Logging | Serilog 9 + OpenTelemetry |
| Cache | StackExchange.Redis (prod), Microsoft.Extensions.Caching.Memory (dev) |
| Email | Brevo (provider France RGPD-compliant) |
| KMS | Azure Key Vault (option HashiCorp Vault pour self-hosted) |
| Hashage mots de passe | Argon2id (Konscious.Security.Cryptography.Argon2) |
| Tests | xUnit + FluentAssertions + NSubstitute + Testcontainers + NetArchTest |

**Important** : la gestion des versions NuGet est centralisée dans `Directory.Packages.props`. Ne **jamais** spécifier de version dans un `<PackageReference>` individuel.

---

## Vocabulaire ubiquitaire — règle hybride

Le projet utilise un vocabulaire **mixte français-anglais** selon des règles strictes (cf. `docs/08-vocabulaire-ubiquitaire.md`).

**Anglais** pour les concepts techniques génériques (`Repository`, `Service`, `Handler`, `Query`, `Command`, `Event`, `Result`) et pour les concepts métier ayant une traduction propre et standard (`Company`, `Trademark`, `Patent`, `Address`, `User`, `Account`).

**Français** pour les concepts spécifiquement français qui n'ont pas d'équivalent net (`UniteLegale`, `Etablissement`, `FormeJuridique`, `Mandataire`, `Dirigeant`, `VeillePack`) et pour les acronymes officiels (`Siren`, `Siret`, `Naf`, `Rne`, `Bopi`, `Bodacc`, `Inpi`).

Les **noms composés mixtes** sont acceptés et fréquents : `BopiPublication`, `InpiCredentials`, `RneCompanyProvider`, `VeilleSubscription`. La clarté prime sur la pureté linguistique.

**Avant d'introduire un terme nouveau dans le code, vérifier qu'il est dans le glossaire ou l'y ajouter.** Ne jamais introduire de synonyme silencieux.

---

## Architecture : règles d'or de dépendance

Le projet suit une architecture **hexagonale** stricte. Les règles de dépendance sont **automatiquement vérifiées** par les tests dans `tests/Atlas.Architecture.Tests`.

| Projet | Peut référencer |
|---|---|
| `Atlas.Shared` | (aucun autre projet de la solution) |
| `Atlas.Domain` | `Atlas.Shared` uniquement |
| `Atlas.Application` | `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Application.Premium` | `Atlas.Application`, `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Infrastructure.*` | `Atlas.Application`, `Atlas.Domain`, `Atlas.Shared` |
| `Atlas.Api` | tous les précédents |
| `Atlas.Maui` | **uniquement** `Atlas.Domain` et `Atlas.Shared` |

**Le principe à retenir** : les dépendances pointent toujours vers l'intérieur de l'hexagone. Le domaine ne sait rien des détails techniques. L'infrastructure implémente les ports définis par le domaine, jamais l'inverse.

**Cas particulier `Atlas.Maui`** : le client mobile ne doit **jamais** référencer `Atlas.Infrastructure.*` parce que le code serait livré sur les terminaux des utilisateurs et pourrait être décompilé. Toute interaction passe par l'API HTTP via `Atlas.Maui/Services/AtlasApiClient.cs`.

---

## Patterns à utiliser systématiquement

### Result<T> pour les erreurs métier

Ne **jamais** lever d'exception pour signaler une erreur métier (entreprise non trouvée, validation échouée, accès refusé). Utiliser le type `Result<T>` défini dans `Atlas.Shared`.

```csharp
// ❌ NON
public async Task<Company> GetBySirenAsync(string siren)
{
    if (!IsValidSiren(siren))
        throw new InvalidSirenException(siren);  // Mauvais
    // ...
}

// ✅ OUI
public async Task<Result<CompanyDto>> Handle(GetCompanyBySirenQuery req, CancellationToken ct)
{
    if (!Siren.TryParse(req.SirenValue, out var siren))
        return Result<CompanyDto>.Fail(new InvalidSirenError(req.SirenValue));
    // ...
}
```

Les exceptions sont réservées aux **erreurs vraiment exceptionnelles** : BDD down, fichier corrompu, dépendance externe indisponible.

### Strongly-typed IDs et value objects

Ne **jamais** utiliser des `string` ou `Guid` nus pour des identifiants métier. Toujours encapsuler dans un value object qui contient la validation.

```csharp
// ❌ NON
public async Task<Company?> GetBySirenAsync(string siren) { ... }

// ✅ OUI
public async Task<Company?> GetBySirenAsync(Siren siren) { ... }
```

Les value objects définis dans `Atlas.Domain` : `Siren`, `Siret`, `Naf`, `CompanyId`, `UserId`, `DepositNumber`, `NiceClassification`, etc.

### CQRS léger via MediatR

Toute interaction métier passe par un `IRequest<TResponse>` géré par MediatR. Les commands modifient l'état, les queries lisent l'état. Les handlers sont `internal sealed`.

```csharp
// Commande typique
public record AddCompanyToFavoritesCommand(string SirenValue) 
    : IRequest<Result>;

internal sealed class AddCompanyToFavoritesHandler 
    : IRequestHandler<AddCompanyToFavoritesCommand, Result>
{
    // Constructeur injection des ports
    // Handle() — validation → use case → result
}
```

### File-scoped namespaces

Toujours utiliser les file-scoped namespaces, jamais les block-scoped.

```csharp
// ❌ NON
namespace Atlas.Domain.Companies
{
    public class Company { ... }
}

// ✅ OUI
namespace Atlas.Domain.Companies;

public class Company { ... }
```

### Modificateurs d'accès restrictifs par défaut

Les classes sont `internal sealed` par défaut. Seules les classes qui font partie du **contrat public** d'un projet doivent être `public` : les entités domaine, les value objects, les ports d'interface, les DTOs exposés.

---

## Anti-patterns à proscrire

Plusieurs pratiques sont **strictement interdites** dans le projet. Elles sont soit dangereuses pour la sécurité, soit dommageables pour l'architecture, soit contraires aux engagements du projet.

**Ne jamais logger les `InpiCredentials`**, ni en clair ni d'aucune autre façon. Ces credentials sont déchiffrés en mémoire le temps d'une requête puis oubliés. Toute exposition est un incident de sécurité critique. Cf. `docs/04-securite-rgpd.md`.

**Ne jamais ajouter de dépendance externe à `Atlas.Domain`** au-delà de celles déjà autorisées (libs de base .NET, `MediatR.Contracts`, `FluentValidation.Abstractions`). Si tu penses avoir besoin de quelque chose, c'est probablement qu'il faut le mettre dans un port et l'implémenter ailleurs.

**Ne jamais sauter la checklist d'accessibilité** lors d'une PR sur l'UI MAUI ou les pages publiques. C'est une exigence bloquante de la Definition of Done (ADR-008). Cf. `docs/06-accessibilite.md`.

**Ne jamais utiliser `Newtonsoft.Json`** sauf raison technique impérieuse. Le standard du projet est `System.Text.Json`.

**Ne jamais introduire une nouvelle dépendance NuGet sans la déclarer dans `Directory.Packages.props`**. La gestion centralisée des versions est non-négociable.

**Ne jamais nommer une variable ou une classe en mélangeant français et anglais de manière incohérente.** Par exemple `entrepriseRepository` est interdit (faire `companyRepository` ou `uniteLegaleRepository`).

**Ne jamais retourner directement des entités du domaine via l'API**. Toujours passer par un `Dto` mappé dans `Atlas.Application`.

**Ne jamais exposer les `BeneficiaireEffectif` publiquement sans avis juridique**. Régime spécial depuis l'arrêt CJUE Sovim. Cf. `docs/04-securite-rgpd.md`.

---

## Commandes utiles

```bash
# Build de toute la solution
dotnet build

# Tests
dotnet test                                           # tous les tests
dotnet test tests/Atlas.Domain.UnitTests              # tests d'un seul projet
dotnet test --filter "Category=Architecture"          # seulement les tests d'archi

# Migrations EF Core
dotnet ef migrations add <Name> \
  --project src/Atlas.Infrastructure.Persistence \
  --startup-project src/Atlas.Api
dotnet ef database update \
  --project src/Atlas.Infrastructure.Persistence \
  --startup-project src/Atlas.Api

# Lancement local
dotnet run --project src/Atlas.Api                    # API
dotnet build src/Atlas.Maui -f net10.0-android        # MAUI Android
dotnet build src/Atlas.Maui -f net10.0-ios            # MAUI iOS

# Outils de qualité
dotnet format                                          # format selon .editorconfig
dotnet list package --outdated                        # vérifier les versions NuGet

# Ajout d'un package (en CPM, sans version dans le .csproj)
# 1. Ajouter <PackageVersion Include="X" Version="Y" /> dans Directory.Packages.props
# 2. Ajouter <PackageReference Include="X" /> dans le .csproj cible
```

---

## Workflow de PR (Definition of Done)

**Tout changement de code passe par une PR sur GitHub, jamais en push direct sur `main`.** Les docs seules peuvent aller en direct exceptionnellement, mais le réflexe par défaut reste la PR. La séquence est :

1. **Branche dédiée** depuis `main` à jour : `feat/F-NNN-titre-court`, `fix/...`, `docs/...` (Conventional Commits + numéro de feature concerné). Pas de travail sur `main`.
2. **Commit(s)** sur la branche en suivant Conventional Commits (cf. ci-dessous et `docs/05-strategie-repos.md`).
3. **Push** de la branche et **ouverture d'une PR** vers `main` (`gh pr create`), avec un titre court (< 70 chars), une description qui cite le numéro `F-XXX` et un test plan.
4. **Attendre que la CI passe au vert** sur la PR (cf. `.github/workflows/ci.yml` — build backend Release + tests unitaires + tests d'archi + tests d'intégration Docker). Si rouge, corriger sur la branche, pas en bypass.
5. **Merge uniquement quand la CI est verte** (`gh pr merge --squash` par défaut). Supprimer la branche après merge.
6. Vérifier post-merge que le **build CI sur `main`** reste vert.

Avant qu'une PR soit mergée dans `main`, vérifier que :

La PR a une description claire qui mentionne le numéro de feature concerné (`F-XXX` du backlog dans `docs/02-roadmap-features.md`).

Le build CI passe entièrement, y compris les tests d'architecture dans `Atlas.Architecture.Tests`. Si un test d'architecture échoue, **ne pas désactiver le test**. Réfléchir à pourquoi l'architecture refuse cette modification. C'est presque toujours signe d'un mauvais placement de code.

Les tests unitaires couvrent les nouveaux comportements ajoutés. Pas de seuil de couverture imposé, mais un comportement ajouté sans test est inacceptable.

La checklist d'accessibilité (cf. `docs/06-accessibilite.md` §12) est passée pour toute modification de l'UI MAUI ou des endpoints publics.

Le vocabulaire ubiquitaire est respecté. Les nouveaux concepts sont ajoutés au glossaire dans `docs/08-vocabulaire-ubiquitaire.md`.

Aucun `InpiCredentials` n'apparaît dans les logs, les messages d'erreur, ou les réponses d'API. Aucune chaîne ressemblant à un mot de passe, un token, ou une clé d'API n'est commitée.

Les commits suivent les Conventional Commits (cf. `docs/05-strategie-repos.md`). Exemples : `feat(domain): add Siren value object with Luhn validation`, `fix(api): handle 404 from INPI RNE gracefully`, `docs(adr): add ADR-010 about ...`.

---

## Quand tu (Claude Code) hésites

Le projet est délibérément structuré pour qu'il y ait **toujours un bon endroit** où mettre une nouvelle chose. Si tu hésites entre deux placements, applique la règle suivante.

Pour une **logique métier pure** sans aucune dépendance technique, c'est dans `Atlas.Domain`. Si tu y vois apparaître un `HttpClient`, un `DbContext`, ou un appel à `Console.WriteLine`, tu es au mauvais endroit.

Pour un **use case orchestrant le domaine et les ports**, c'est dans `Atlas.Application`. Le handler reçoit des dépendances injectées, valide les inputs, appelle les ports, retourne un `Result`.

Pour une **intégration concrète avec une technologie externe** (INPI, PostgreSQL, Redis, Brevo), c'est dans le projet `Atlas.Infrastructure.*` correspondant à la technologie. Si la technologie est nouvelle et n'a pas encore son projet, en créer un (cf. `docs/10-layout-solution-dotnet.md`).

Pour le **wiring DI et la configuration HTTP**, c'est dans `Atlas.Api`. C'est la composition root, le seul endroit qui voit tout.

Pour une **vue ou un ViewModel**, c'est dans `Atlas.Maui`. Et seulement si le concept est purement présentation client. La logique métier reste côté backend.

Si après ces règles tu ne sais toujours pas où mettre quelque chose, c'est probablement que la chose en question est mal définie ou qu'elle mélange plusieurs responsabilités. Découper avant de coder.

---

## Sécurité — les non-négociables

Cette section liste les règles de sécurité qui s'appliquent à **toute contribution**. Aucune dérogation n'est possible sans validation explicite et tracée dans un ADR.

Les mots de passe utilisateur sont hashés avec **Argon2id** uniquement. Pas de SHA-256, pas de bcrypt, pas de PBKDF2. Les paramètres sont définis dans `docs/04-securite-rgpd.md`.

Les **credentials INPI** sont chiffrés au repos avec **AES-256-GCM** et une clé gérée par le KMS. Le déchiffrement est limité au moment d'une requête vers l'INPI et la valeur en clair n'est jamais persistée ni loggée.

Les **JWT** ont une durée de vie courte (15 minutes pour l'access token). Le refresh token est rotatif et stocké côté serveur. La signature utilise une clé asymétrique (RS256 ou EdDSA).

Toutes les **entrées utilisateur** sont validées via FluentValidation avant d'atteindre le handler. Aucune confiance dans les inputs reçus par l'API.

Les **données personnelles** sont traitées conformément au RGPD : minimisation, anonymisation des logs, droit à l'effacement implémenté. Cf. `docs/04-securite-rgpd.md`.

---

## Liens vers les ressources

| Ressource | Localisation |
|---|---|
| Documentation INPI RNE | https://www.inpi.fr/sites/default/files/documentation_technique_api_rne.pdf |
| Documentation INPI PI | https://api-marques-doc.inpi.fr/ |
| Repository code (à venir) | GitHub : `<org>/atlas` |
| Repository docs internes | GitLab self-hosted : `atlas-internal` |
| CI/CD | GitHub Actions (à configurer) |

---

*Ce fichier évolue avec le projet. Toute modification structurelle de l'architecture, du vocabulaire, ou des règles de sécurité doit être répercutée ici en plus de la documentation détaillée.*
