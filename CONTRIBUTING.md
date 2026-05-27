# Contribuer à Atlas

Merci de votre intérêt ! Ce guide résume l'essentiel. Pour le détail, voir
[`CLAUDE.md`](CLAUDE.md) et [`docs/05-strategie-repos.md`](docs/05-strategie-repos.md).

## Prérequis

- .NET 10 SDK
- PostgreSQL (pour les tests d'intégration et l'exécution locale)
- Docker (pour les tests d'intégration via Testcontainers)

## Workflow

1. Créez une branche depuis `main` (`feat/...`, `fix/...`, `docs/...`).
2. Implémentez en respectant l'[architecture](ARCHITECTURE.md) et le
   [vocabulaire ubiquitaire](docs/08-vocabulaire-ubiquitaire.md).
3. Ajoutez des tests pour tout comportement nouveau.
4. Ouvrez une Pull Request vers `main`.

## Commits

Format [Conventional Commits](https://www.conventionalcommits.org/) :

```
feat(domain): add Siren value object with Luhn validation
fix(api): handle 404 from INPI RNE gracefully
docs(adr): add ADR-010 about user authentication
```

## Definition of Done

Avant qu'une PR soit mergée :

- Le build CI passe, **y compris les tests d'architecture** (`Atlas.Architecture.Tests`).
  Si un test d'architecture échoue, ne le désactivez pas : repensez le placement du code.
- Les nouveaux comportements sont couverts par des tests.
- Le vocabulaire ubiquitaire est respecté ; les nouveaux concepts sont ajoutés au glossaire.
- La checklist d'accessibilité est passée pour toute modification de l'UI MAUI ou des endpoints
  publics (cf. [`docs/06-accessibilite.md`](docs/06-accessibilite.md)).
- **Aucun secret ni identifiant INPI** dans les logs, messages d'erreur, réponses d'API ou commits.

## Commandes utiles

```bash
dotnet build                 # build de la solution
dotnet test                  # tous les tests (Docker requis pour l'intégration)
dotnet format                # formatage selon .editorconfig
```

`TreatWarningsAsErrors` est activé : tout avertissement d'analyseur casse le build.
Les versions NuGet sont centralisées dans `Directory.Packages.props` (ne pas spécifier de
version dans un `<PackageReference>` individuel).
