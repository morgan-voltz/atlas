# Polices facilitantes — assets à déposer manuellement

> Lot 5d, suite directe du Lot 5c. Préférences accessibilité utilisateur
> (`DyslexiaFriendly` / `HighReadability`). Cf. `docs/06-accessibilite.md` §10.3.

## Pourquoi

Le **client MAUI** déclare deux polices facilitantes :

- **OpenDyslexic** (pour les utilisateurs déclarant « DyslexiaFriendly »).
- **Atkinson Hyperlegible** (pour les utilisateurs déclarant « HighReadability »).

Ces polices sont distribuées sous licence **SIL Open Font License 1.1**
(redistribution libre y compris commerciale, sous réserve de joindre le texte
de licence). Elles **ne sont pas redistribuées dans le repo** : les binaires
`.ttf` doivent être déposés manuellement par un contributeur après
téléchargement depuis la source officielle.

Tant que les fichiers ne sont pas présents, MAUI logge un avertissement au
build et `FontFamily` retombe sur `OpenSansRegular` au runtime. L'application
fonctionne sans rien casser, mais la préférence utilisateur n'a pas d'effet
visuel.

## Noms exacts attendus dans ce dossier

```
src/Atlas.Maui/Resources/Fonts/OpenDyslexic-Regular.ttf
src/Atlas.Maui/Resources/Fonts/AtkinsonHyperlegible-Regular.ttf
```

Toute autre casse ou variante (par ex. `OpenDyslexicAlta-Regular.ttf`) **ne sera
pas chargée** — `MauiProgram.cs` référence ces noms exacts via
`fonts.AddFont("...", "OpenDyslexicRegular")` /
`fonts.AddFont("...", "AtkinsonHyperlegibleRegular")`.

## Où télécharger

- **OpenDyslexic** : <https://opendyslexic.org/> — section *Download*.
  Récupérer le pack `compiled-ttf`, en extraire le `OpenDyslexic-Regular.ttf`.
- **Atkinson Hyperlegible** : <https://brailleinstitute.org/freefont> ou
  <https://www.fontsquirrel.com/fonts/atkinson-hyperlegible> — récupérer
  `AtkinsonHyperlegible-Regular.ttf`.

## Vérifier la licence

Chaque archive téléchargée contient un fichier `OFL.txt` ou `LICENSE`. **Joindre
ce fichier au dépôt** (à côté du `.ttf` correspondant, en le renommant si besoin
pour éviter la collision : `OpenDyslexic.OFL.txt`,
`AtkinsonHyperlegible.OFL.txt`) lors du commit.

## Une fois les fichiers déposés

Le build MAUI les détecte automatiquement (les `.ttf` sous `Resources/Fonts/`
sont auto-glob par le SDK MAUI, aucune modif `.csproj` requise). Bouger l'app
sur la page « Accessibilité » → choisir une police facilitante → cliquer
« Enregistrer » : tout le texte de l'app change immédiatement.

## Poids estimé

| Fichier | Poids approximatif |
|---|---|
| `OpenDyslexic-Regular.ttf` | ~80 KB |
| `AtkinsonHyperlegible-Regular.ttf` | ~50–150 KB |
| `OFL.txt` × 2 | ~5 KB |

Impact sur le bundle MAUI Android Release : négligeable.
