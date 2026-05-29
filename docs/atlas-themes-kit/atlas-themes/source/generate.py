# -*- coding: utf-8 -*-
"""Genere, depuis palettes.py (source unique) :
   - themes.json                      (source machine, pour l'apercu HTML)
   - maui/Resources/Themes/*.xaml(.cs) (14 ResourceDictionaries + code-behind)
   - maui/Theming/AppThemeId.cs        (enum des 7 themes)
   - maui/Theming/ThemeManager.cs      (service de bascule runtime)
   - rapport de contraste (dict) pour la charte PDF
"""
import json, os
from palettes import THEMES, TOKENS, ratio

ROOT = os.path.dirname(os.path.abspath(__file__))
MAUI = os.path.join(ROOT, "maui")
THDIR = os.path.join(MAUI, "Resources", "Themes")
THEMING = os.path.join(MAUI, "Theming")
for d in (THDIR, THEMING):
    os.makedirs(d, exist_ok=True)

# ---------- 1. JSON ----------
with open(os.path.join(ROOT, "themes.json"), "w", encoding="utf-8") as f:
    json.dump(THEMES, f, ensure_ascii=False, indent=2)

# ---------- 2. XAML (14 dictionnaires + code-behind) ----------
NS = "Atlas.Maui.Resources.Themes"
def cls(name, mode): return f"{name}{mode.capitalize()}"

xaml_tpl_head = (
 '<?xml version="1.0" encoding="UTF-8" ?>\n'
 '<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"\n'
 '                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"\n'
 '                    x:Class="{ns}.{cls}">\n'
 '    <!-- Theme {label} - {mode} | genere depuis palettes.py, ne pas editer a la main -->\n'
)
for name, t in THEMES.items():
    for mode in ("light", "dark"):
        pal = t[mode]
        c = cls(name, mode)
        lines = [xaml_tpl_head.format(ns=NS, cls=c, label=t["label"], mode=mode)]
        for tk in TOKENS:
            lines.append(f'    <Color x:Key="{tk}">{pal[tk]}</Color>\n')
        for tk in TOKENS:
            lines.append(f'    <SolidColorBrush x:Key="{tk}Brush" Color="{{StaticResource {tk}}}" />\n')
        lines.append('</ResourceDictionary>\n')
        with open(os.path.join(THDIR, f"{c}.xaml"), "w", encoding="utf-8") as f:
            f.writelines(lines)
        # code-behind minimal
        cs = (f"using Microsoft.Maui.Controls;\n\n"
              f"namespace {NS};\n\n"
              f"public partial class {c} : ResourceDictionary\n{{\n"
              f"    public {c}() => InitializeComponent();\n}}\n")
        with open(os.path.join(THDIR, f"{c}.xaml.cs"), "w", encoding="utf-8") as f:
            f.write(cs)

# ---------- 3. Enum des themes ----------
enum_members = ",\n".join(f"    {n}" for n in THEMES)
labels = "\n".join(
    f'        AppThemeId.{n} => "{t["label"]}",' for n, t in THEMES.items())
enum_cs = f"""namespace Atlas.Maui.Theming;

/// <summary>Les 7 themes selectionnables par l'utilisateur.</summary>
public enum AppThemeId
{{
{enum_members}
}}

public static class AppThemeIdExtensions
{{
    /// <summary>Libelle affichable dans le selecteur de theme.</summary>
    public static string ToLabel(this AppThemeId id) => id switch
    {{
{labels}
        _ => id.ToString(),
    }};
}}
"""
with open(os.path.join(THEMING, "AppThemeId.cs"), "w", encoding="utf-8") as f:
    f.write(enum_cs)

# ---------- 4. ThemeManager (bascule runtime) ----------
# map (theme, mode) -> type du ResourceDictionary genere
factory = "\n".join(
    f"        [(AppThemeId.{n}, false)] = () => new Resources.Themes.{cls(n,'light')}(),\n"
    f"        [(AppThemeId.{n}, true)]  = () => new Resources.Themes.{cls(n,'dark')}(),"
    for n in THEMES)

tm_cs = f"""using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Atlas.Maui.Theming;

/// <summary>
/// Gere le theme actif (identite coloree) et le mode (clair/sombre/systeme).
/// Les deux axes sont independants. L'app reference uniquement des tokens
/// semantiques via {{DynamicResource Primary}}, etc. : un swap met tout a jour.
/// </summary>
public sealed class ThemeManager
{{
    const string KeyTheme = "atlas.theme";
    const string KeyMode  = "atlas.mode"; // "Light" | "Dark" | "System"

    ResourceDictionary? _current;

    static readonly Dictionary<(AppThemeId, bool), Func<ResourceDictionary>> Factory = new()
    {{
{factory}
    }};

    public AppThemeId Theme {{ get; private set; }} = AppThemeId.Atlas;
    public AppTheme Mode {{ get; private set; }} = AppTheme.Unspecified; // Unspecified = suit le systeme

    /// <summary>A appeler au demarrage (App.xaml.cs) pour restaurer le choix.</summary>
    public void Initialize()
    {{
        Theme = Enum.TryParse(Preferences.Get(KeyTheme, nameof(AppThemeId.Atlas)),
                              out AppThemeId t) ? t : AppThemeId.Atlas;
        Mode = Preferences.Get(KeyMode, "System") switch
        {{
            "Light" => AppTheme.Light,
            "Dark"  => AppTheme.Dark,
            _        => AppTheme.Unspecified,
        }};
        Apply();
    }}

    public void SetTheme(AppThemeId theme)
    {{
        Theme = theme;
        Preferences.Set(KeyTheme, theme.ToString());
        Apply();
    }}

    public void SetMode(AppTheme mode)
    {{
        Mode = mode;
        Preferences.Set(KeyMode, mode == AppTheme.Light ? "Light"
                               : mode == AppTheme.Dark ? "Dark" : "System");
        Apply();
    }}

    /// <summary>Resout clair/sombre effectif et echange le dictionnaire actif.</summary>
    public void Apply()
    {{
        var app = Application.Current;
        if (app is null) return;

        // Force le mode demande (sinon suit le systeme)
        app.UserAppTheme = Mode;

        bool isDark = Mode switch
        {{
            AppTheme.Light => false,
            AppTheme.Dark  => true,
            _ => app.RequestedTheme == AppTheme.Dark,
        }};

        var next = Factory[(Theme, isDark)]();

        var dicts = app.Resources.MergedDictionaries;
        if (_current is not null) dicts.Remove(_current);
        dicts.Add(next);
        _current = next;
    }}
}}
"""
with open(os.path.join(THEMING, "ThemeManager.cs"), "w", encoding="utf-8") as f:
    f.write(tm_cs)

# ---------- 5. Rapport de contraste (pour la charte) ----------
report = {}
key_pairs = [("texte principal", "onSurface", "surface"),
             ("texte secondaire", "onSurfaceVariant", "surface"),
             ("primaire/fond", "primary", "background"),
             ("texte sur primaire", "onPrimary", "primary")]
for name, t in THEMES.items():
    report[name] = {}
    for mode in ("light", "dark"):
        pal = t[mode]
        report[name][mode] = {lbl: round(ratio(pal[a], pal[b]), 2)
                              for lbl, a, b in key_pairs}
with open(os.path.join(ROOT, "contrast_report.json"), "w", encoding="utf-8") as f:
    json.dump(report, f, ensure_ascii=False, indent=2)

nfiles = len(THEMES) * 2 * 2 + 2
print(f"Genere : themes.json, {len(THEMES)*2} XAML (+code-behind), "
      f"AppThemeId.cs, ThemeManager.cs, contrast_report.json")
