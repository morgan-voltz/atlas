# Atlas.Application.Premium

Ce projet existe pour materialiser la **separation entre les use cases gratuits et les use cases premium**, telle que decidee dans **ADR-009** (voir `docs/01-decisions-architecturales.md`).

## Pourquoi il est (presque) vide

En MVP 2, ce projet est volontairement vide. Aucun use case premium n'est encore implemente. Le projet est neanmoins cree et reference depuis `Atlas.Api` pour deux raisons :

1. **Discipline architecturale** : des qu'un developpeur voudra ajouter un use case premium (resumes IA, scoring de pertinence, fonctions de collaboration en equipe, etc.), il devra le mettre ici. Cela clarifie immediatement qu'on touche a la zone monetisee du produit.
2. **Reversibilite** : le jour ou on souhaitera extraire les modules premium pour les compiler dans un binaire separe, le packaging et les references sont deja en place.

## Ou placer un nouveau use case premium

Suivre exactement la meme structure que `Atlas.Application` :

```
Atlas.Application.Premium/
  <BoundedContext>/
    Commands/
      <UseCaseName>/
        <UseCaseName>Command.cs
        <UseCaseName>Handler.cs
        <UseCaseName>Validator.cs
    Queries/
      <UseCaseName>/
        <UseCaseName>Query.cs
        <UseCaseName>Handler.cs
        <UseCaseName>Dto.cs
```

Le handler reste `internal sealed` et doit retourner un `Result<T>`.

## Reference

- [`docs/01-decisions-architecturales.md`](../../docs/01-decisions-architecturales.md) — ADR-009 (isolation des features premium)
- [`docs/10-layout-solution-dotnet.md`](../../docs/10-layout-solution-dotnet.md) — section 4.4
