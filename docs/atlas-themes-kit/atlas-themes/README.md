# Kit de thèmes Atlas — apps MAUI

Système de 7 thèmes accessibles (clair + sombre = 14 palettes), conformes WCAG 2.2.
Voir la charte PDF `charte-apps.pdf` pour la documentation complète.

## Contenu

- `maui/Resources/Themes/*.xaml` — 14 ResourceDictionaries (à intégrer dans l'app MAUI).
- `maui/Theming/AppThemeId.cs` — enum des 7 thèmes + libellés.
- `maui/Theming/ThemeManager.cs` — service de bascule thème + mode (clair/sombre/système).
- `apercu-themes.html` — aperçu interactif (ouvrir dans un navigateur).
- `source/` — la chaîne de génération (source unique).

## Intégration rapide

1. Copier `maui/Resources/Themes/` et `maui/Theming/` dans le projet `Atlas.Maui`
   (ajuster le namespace `Atlas.Maui` si besoin).
2. Déclarer les clés de tokens dans l'app et référencer via `{DynamicResource primary}`, etc.
3. Au démarrage (`App.xaml.cs`) : `new ThemeManager().Initialize();`
4. Dans les réglages : `SetTheme(AppThemeId.Ocean)` / `SetMode(AppTheme.Dark)`.

## Modifier / ajouter un thème (source unique)

On ne modifie JAMAIS le XAML à la main. Tout part de `source/palettes.py` :

```
cd source
python3 palettes.py        # vérifie les contrastes WCAG (doit dire "TOUTES ... PASSENT")
python3 generate.py        # régénère JSON + XAML + ThemeManager + enum
python3 build_preview.py   # régénère l'aperçu interactif
python3 build_charter.py   # régénère la charte HTML (puis WeasyPrint -> PDF)
```

Aucun thème n'est ajouté s'il ne passe pas le vérificateur de contraste.
