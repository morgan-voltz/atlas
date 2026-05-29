# -*- coding: utf-8 -*-
"""Source unique des 7 themes Atlas (clair + sombre) en tokens semantiques.
   Modele Material-3-like : l'app ne reference QUE des tokens, jamais une
   couleur brute. Chaque palette doit passer les ratios WCAG 2.2 (verifies ici)."""

TOKENS = [
    "background", "surface", "surfaceVariant",
    "onBackground", "onSurface", "onSurfaceVariant", "outline",
    "primary", "onPrimary", "primaryContainer", "onPrimaryContainer",
    "success", "onSuccess", "warning", "onWarning",
    "error", "onError", "info", "onInfo", "focus",
]

THEMES = {
 "Atlas": {"label": "Atlas", "kind": "aesthetic",
  "light": dict(background="#F6F9FC", surface="#FFFFFF", surfaceVariant="#E7EFF8",
    onBackground="#13202E", onSurface="#13202E", onSurfaceVariant="#42596F",
    outline="#7E97AE", primary="#1B4F7E", onPrimary="#FFFFFF",
    primaryContainer="#D4E4F4", onPrimaryContainer="#0B2236",
    success="#1E7A40", onSuccess="#FFFFFF", warning="#8A5A00", onWarning="#FFFFFF",
    error="#B5281F", onError="#FFFFFF", info="#1B5E8A", onInfo="#FFFFFF",
    focus="#1565C0"),
  "dark": dict(background="#0D1722", surface="#16222F", surfaceVariant="#21303F",
    onBackground="#E8EEF4", onSurface="#E8EEF4", onSurfaceVariant="#AABBCC",
    outline="#5A7490", primary="#7FB2E0", onPrimary="#08121C",
    primaryContainer="#1E4060", onPrimaryContainer="#D4E4F4",
    success="#5FC98A", onSuccess="#08121C", warning="#E0B25C", onWarning="#08121C",
    error="#F08A82", onError="#08121C", info="#74C0E8", onInfo="#08121C",
    focus="#8FC4F5")},

 "Ocean": {"label": "Ocean", "kind": "aesthetic",
  "light": dict(background="#F2FAFB", surface="#FFFFFF", surfaceVariant="#DDF0F2",
    onBackground="#0F2326", onSurface="#0F2326", onSurfaceVariant="#3E5D60",
    outline="#5E9499", primary="#11696F", onPrimary="#FFFFFF",
    primaryContainer="#C8E8EA", onPrimaryContainer="#06292B",
    success="#1E7A40", onSuccess="#FFFFFF", warning="#8A5A00", onWarning="#FFFFFF",
    error="#B5281F", onError="#FFFFFF", info="#11696F", onInfo="#FFFFFF",
    focus="#0E7C9C"),
  "dark": dict(background="#08191B", surface="#102528", surfaceVariant="#173438",
    onBackground="#E3F1F2", onSurface="#E3F1F2", onSurfaceVariant="#A2C0C3",
    outline="#557D83", primary="#58C4CC", onPrimary="#06191B",
    primaryContainer="#13474C", onPrimaryContainer="#C8E8EA",
    success="#5FC98A", onSuccess="#06191B", warning="#E0B25C", onWarning="#06191B",
    error="#F08A82", onError="#06191B", info="#6FD0DC", onInfo="#06191B",
    focus="#7FE0E8")},

 "Foret": {"label": "Foret", "kind": "aesthetic",
  "light": dict(background="#F4FAF3", surface="#FFFFFF", surfaceVariant="#DDF0DA",
    onBackground="#15251A", onSurface="#15251A", onSurfaceVariant="#45604A",
    outline="#649A69", primary="#246B36", onPrimary="#FFFFFF",
    primaryContainer="#CDE9CC", onPrimaryContainer="#0C2A14",
    success="#246B36", onSuccess="#FFFFFF", warning="#8A5A00", onWarning="#FFFFFF",
    error="#B5281F", onError="#FFFFFF", info="#1B5E8A", onInfo="#FFFFFF",
    focus="#2E7D32"),
  "dark": dict(background="#0C1A0E", surface="#142717", surfaceVariant="#1C3620",
    onBackground="#E4F1E3", onSurface="#E4F1E3", onSurfaceVariant="#A8C3A9",
    outline="#57815C", primary="#6FC97E", onPrimary="#08160B",
    primaryContainer="#1A4523", onPrimaryContainer="#CDE9CC",
    success="#6FC97E", onSuccess="#08160B", warning="#E0B25C", onWarning="#08160B",
    error="#F08A82", onError="#08160B", info="#74C0E8", onInfo="#08160B",
    focus="#8FE09B")},

 "Ambre": {"label": "Ambre", "kind": "aesthetic",
  "light": dict(background="#FEF8F2", surface="#FFFFFF", surfaceVariant="#F8E6D2",
    onBackground="#2B1C0E", onSurface="#2B1C0E", onSurfaceVariant="#6B5238",
    outline="#A67C50", primary="#A8551A", onPrimary="#FFFFFF",
    primaryContainer="#F6D9BC", onPrimaryContainer="#3A1F08",
    success="#1E7A40", onSuccess="#FFFFFF", warning="#8A5A00", onWarning="#FFFFFF",
    error="#B5281F", onError="#FFFFFF", info="#1B5E8A", onInfo="#FFFFFF",
    focus="#C25E12"),
  "dark": dict(background="#1F160D", surface="#2A1F13", surfaceVariant="#3A2C1B",
    onBackground="#F1E5D6", onSurface="#F1E5D6", onSurfaceVariant="#C8B49B",
    outline="#8A6D47", primary="#E0A35C", onPrimary="#1F160D",
    primaryContainer="#5A3818", onPrimaryContainer="#F6D9BC",
    success="#9BC07A", onSuccess="#1F160D", warning="#E0B25C", onWarning="#1F160D",
    error="#F08A82", onError="#1F160D", info="#74C0E8", onInfo="#1F160D",
    focus="#F0B870")},

 "Amethyste": {"label": "Amethyste", "kind": "aesthetic",
  "light": dict(background="#F9F6FC", surface="#FFFFFF", surfaceVariant="#ECE2F5",
    onBackground="#211530", onSurface="#211530", onSurfaceVariant="#564166",
    outline="#9B82B4", primary="#6A3BA0", onPrimary="#FFFFFF",
    primaryContainer="#E4D4F2", onPrimaryContainer="#2A1640",
    success="#1E7A40", onSuccess="#FFFFFF", warning="#8A5A00", onWarning="#FFFFFF",
    error="#B5281F", onError="#FFFFFF", info="#1B5E8A", onInfo="#FFFFFF",
    focus="#7B3FF2"),
  "dark": dict(background="#160E20", surface="#221831", surfaceVariant="#2F2342",
    onBackground="#ECE4F4", onSurface="#ECE4F4", onSurfaceVariant="#BBAACC",
    outline="#7A6692", primary="#BB93E0", onPrimary="#140C1E",
    primaryContainer="#43295F", onPrimaryContainer="#E4D4F2",
    success="#5FC98A", onSuccess="#140C1E", warning="#E0B25C", onWarning="#140C1E",
    error="#F08A82", onError="#140C1E", info="#74C0E8", onInfo="#140C1E",
    focus="#C9A5EC")},

 "Contraste": {"label": "Contraste eleve", "kind": "a11y", "aaa": True,
  "light": dict(background="#FFFFFF", surface="#FFFFFF", surfaceVariant="#EDEDED",
    onBackground="#000000", onSurface="#000000", onSurfaceVariant="#1A1A1A",
    outline="#000000", primary="#00339A", onPrimary="#FFFFFF",
    primaryContainer="#CFDCF7", onPrimaryContainer="#001A5C",
    success="#005C27", onSuccess="#FFFFFF", warning="#5E3F00", onWarning="#FFFFFF",
    error="#8E0006", onError="#FFFFFF", info="#003D80", onInfo="#FFFFFF",
    focus="#00339A"),
  "dark": dict(background="#000000", surface="#000000", surfaceVariant="#1A1A1A",
    onBackground="#FFFFFF", onSurface="#FFFFFF", onSurfaceVariant="#E6E6E6",
    outline="#FFFFFF", primary="#9EC2FF", onPrimary="#000000",
    primaryContainer="#0A2A66", onPrimaryContainer="#D6E4FF",
    success="#6FE69B", onSuccess="#000000", warning="#FFD466", onWarning="#000000",
    error="#FF9C95", onError="#000000", info="#8FD0FF", onInfo="#000000",
    focus="#FFFFFF")},

 "Sepia": {"label": "Sepia (faible lumiere bleue)", "kind": "a11y",
  "light": dict(background="#F4ECD8", surface="#FBF5E6", surfaceVariant="#EADFC4",
    onBackground="#352B17", onSurface="#352B17", onSurfaceVariant="#5E4D30",
    outline="#9C824F", primary="#8A5226", onPrimary="#FFFFFF",
    primaryContainer="#E6D2A8", onPrimaryContainer="#3A2810",
    success="#496326", onSuccess="#FFFFFF", warning="#7E5200", onWarning="#FFFFFF",
    error="#94301C", onError="#FFFFFF", info="#36505E", onInfo="#FFFFFF",
    focus="#8A5226"),
  "dark": dict(background="#211B12", surface="#2B2418", surfaceVariant="#3A3122",
    onBackground="#ECE0C8", onSurface="#ECE0C8", onSurfaceVariant="#C4B48E",
    outline="#8B764B", primary="#D6A56B", onPrimary="#211B12",
    primaryContainer="#574122", onPrimaryContainer="#E6D2A8",
    success="#9BC07A", onSuccess="#211B12", warning="#E0B25C", onWarning="#211B12",
    error="#E8A08C", onError="#211B12", info="#8FB0C0", onInfo="#211B12",
    focus="#E6C68E")},
}


def _lin(c):
    c = c / 255.0
    return c / 12.92 if c <= 0.03928 else ((c + 0.055) / 1.055) ** 2.4

def luminance(hexc):
    h = hexc.lstrip("#")
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return 0.2126 * _lin(r) + 0.7152 * _lin(g) + 0.0722 * _lin(b)

def ratio(c1, c2):
    l1, l2 = luminance(c1), luminance(c2)
    hi, lo = max(l1, l2), min(l1, l2)
    return (hi + 0.05) / (lo + 0.05)

CHECKS = [
    ("onBackground", "background", 4.5, "texte"),
    ("onSurface", "surface", 4.5, "texte"),
    ("onSurfaceVariant", "surface", 4.5, "texte secondaire"),
    ("onSurfaceVariant", "surfaceVariant", 4.5, "texte sur variant"),
    ("onPrimary", "primary", 4.5, "texte sur primaire"),
    ("onPrimaryContainer", "primaryContainer", 4.5, "texte sur container"),
    ("onSuccess", "success", 4.5, "texte succes"),
    ("onWarning", "warning", 4.5, "texte alerte"),
    ("onError", "error", 4.5, "texte erreur"),
    ("onInfo", "info", 4.5, "texte info"),
    ("primary", "background", 3.0, "composant UI"),
    ("primary", "surface", 3.0, "composant UI"),
    ("outline", "surface", 3.0, "bordure"),
    ("focus", "surface", 3.0, "focus"),
    ("error", "surface", 3.0, "couleur erreur"),
]

def check_all():
    total_fail = 0
    for tname, t in THEMES.items():
        aaa = t.get("aaa", False)
        for mode in ("light", "dark"):
            pal = t[mode]
            missing = [tk for tk in TOKENS if tk not in pal]
            if missing:
                print(f"  !! {tname}/{mode} tokens manquants: {missing}")
                total_fail += len(missing)
            for fg, bg, mn, kind in CHECKS:
                if fg not in pal or bg not in pal:
                    continue
                req = 7.0 if (aaa and kind.startswith("texte")) else mn
                r = ratio(pal[fg], pal[bg])
                if r < req:
                    total_fail += 1
                    print(f"  FAIL {tname}/{mode}: {fg} sur {bg} = {r:.2f} "
                          f"(min {req}) [{kind}]")
    if total_fail == 0:
        print("  >>> TOUTES LES PALETTES PASSENT WCAG <<<")
    else:
        print(f"  >>> {total_fail} echec(s) a corriger <<<")
    return total_fail

if __name__ == "__main__":
    check_all()
