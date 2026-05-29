# -*- coding: utf-8 -*-
"""Genere ../charte-apps.html (charte des themes pour les apps MAUI),
   depuis themes.json + contrast_report.json. Style : charte-bureau.css (interne)."""
import json, os
ROOT = os.path.dirname(os.path.abspath(__file__))
BUILD = os.path.dirname(ROOT)
THEMES = json.load(open(os.path.join(ROOT, "themes.json"), encoding="utf-8"))
REPORT = json.load(open(os.path.join(ROOT, "contrast_report.json"), encoding="utf-8"))

def lin(c):
    c /= 255.0
    return c/12.92 if c <= 0.03928 else ((c+0.055)/1.055)**2.4
def lum(h):
    h=h.lstrip('#'); return 0.2126*lin(int(h[0:2],16))+0.7152*lin(int(h[2:4],16))+0.0722*lin(int(h[4:6],16))
def text_on(h):  # noir ou blanc lisible sur la pastille
    return "#000" if lum(h) > 0.4 else "#FFF"

KEY = [("background","Fond"),("surface","Surface"),("primary","Primaire"),
       ("onPrimary","S/Primaire"),("success","Succes"),("warning","Alerte"),
       ("error","Erreur"),("info","Info")]

def swatches(pal):
    out = []
    for tk, lbl in KEY:
        c = pal[tk]; tc = text_on(c)
        out.append(
          f'<div class="sw" style="background:{c};color:{tc}">'
          f'<span class="sw-l">{lbl}</span><span class="sw-h">{c}</span></div>')
    return '<div class="sw-row">' + "".join(out) + '</div>'

def theme_block(name, t):
    r = REPORT[name]
    kind = "Accessibilite" if t["kind"]=="a11y" else "Esthetique"
    badge = "badge--ok" if t["kind"]=="a11y" else "badge--info"
    rows = ""
    for lbl in ["texte principal","texte secondaire","primaire/fond","texte sur primaire"]:
        # map label keys used in report
        key = {"texte principal":"texte principal","texte secondaire":"texte secondaire",
               "primaire/fond":"primaire/fond","texte sur primaire":"texte sur primaire"}[lbl]
        rl = r["light"][key]; rd = r["dark"][key]
        rows += f"<tr><td>{lbl.capitalize()}</td><td>{rl}:1</td><td>{rd}:1</td></tr>"
    return f"""
<div class="theme-card no-break">
  <div class="theme-head">
    <h3>{t['label']}</h3><span class="badge {badge}">{kind}</span>
  </div>
  <div class="mode-lbl">Clair</div>
  {swatches(t['light'])}
  <div class="mode-lbl">Sombre</div>
  {swatches(t['dark'])}
  <table class="mini">
    <thead><tr><th>Contraste</th><th>Clair</th><th>Sombre</th></tr></thead>
    <tbody>{rows}</tbody>
  </table>
</div>"""

themes_html = "\n".join(theme_block(n, t) for n, t in THEMES.items())

# tokens reference
TOK_DESC = [
 ("background","Fond general de l'ecran."),
 ("surface","Fond des cartes, panneaux, champs."),
 ("surfaceVariant","Fond secondaire (items de timeline, zones discretes)."),
 ("onBackground / onSurface","Texte principal (>= 4.5:1)."),
 ("onSurfaceVariant","Texte secondaire, libelles (>= 4.5:1)."),
 ("outline","Bordures, separateurs (>= 3:1)."),
 ("primary","Couleur d'action : boutons, liens, accent."),
 ("onPrimary","Texte/icone sur la couleur primaire."),
 ("primaryContainer / onPrimaryContainer","Variante douce de l'accent + son texte."),
 ("success / warning / error / info","Etats semantiques + leurs 'on*' associes."),
 ("focus","Anneau de focus clavier (>= 3:1, toujours visible)."),
]
tok_rows = "".join(f"<tr><td><code>{a}</code></td><td>{b}</td></tr>" for a,b in TOK_DESC)

HTML = f"""<!DOCTYPE html><html lang="fr"><head><meta charset="utf-8">
<title>Charte des apps — Atlas</title>
<link rel="stylesheet" href="charte-bureau.css">
<style>:root{{ --titre-doc: "Charte des apps"; }}
.sw-row{{display:flex;gap:3pt;margin:3pt 0 6pt;}}
.sw{{flex:1;border-radius:3pt;padding:5pt 4pt;min-height:30pt;display:flex;
  flex-direction:column;justify-content:space-between;border:0.4pt solid rgba(0,0,0,.15);}}
.sw-l{{font-size:7pt;font-weight:600;}} .sw-h{{font-size:6pt;font-family:var(--f-mono);opacity:.85;}}
.mode-lbl{{font-size:7.5pt;font-weight:600;text-transform:uppercase;letter-spacing:.06em;
  color:var(--gris);margin-top:4pt;}}
.theme-card{{border:0.5pt solid var(--bleu-bordure);border-radius:5pt;padding:10pt 12pt;
  margin:0 0 10pt;}}
.theme-head{{display:flex;justify-content:space-between;align-items:center;}}
.theme-head h3{{margin:0;font-size:12pt;color:var(--bleu-encre);}}
table.mini{{width:100%;border-collapse:collapse;margin-top:6pt;font-size:8pt;}}
table.mini th{{background:var(--bleu-primaire);color:#fff;padding:3pt 6pt;text-align:left;border:0.4pt solid var(--bleu-primaire);}}
table.mini td{{padding:3pt 6pt;border:0.4pt solid var(--bleu-bordure);}}
table.mini td:nth-child(2),table.mini td:nth-child(3){{font-family:var(--f-mono);color:var(--bleu-primaire);}}
</style>
<meta name="author" content="Equipe Atlas">
<meta name="description" content="Charte graphique des applications MAUI : systeme de themes accessibles.">
</head><body>

<section class="cover">
  <div class="marque">Atlas</div>
  <div class="label">Charte d'interface — diffusion interne</div>
  <h1 class="titre">Charte des<br>applications</h1>
  <p class="sous-titre">Systeme de themes accessibles pour les clients MAUI : 7 themes, clair et sombre, conformes WCAG 2.2.</p>
  <div class="filet-or"></div>
  <div class="meta">
    <div><div class="k">Version</div><div class="v">1.0</div></div>
    <div><div class="k">Date</div><div class="v">29 mai 2026</div></div>
    <div><div class="k">Cible</div><div class="v">.NET MAUI 10</div></div>
    <div><div class="k">Ref.</div><div class="v">ADR-008 · doc 06</div></div>
    <div><div class="k">Themes</div><div class="v">7 × (clair/sombre)</div></div>
  </div>
</section>

<section class="toc"><h1>Sommaire</h1><ul>
  <li><a href="#s1"><span class="t-num">1.</span><span class="t-titre">Principes &amp; structure</span></a></li>
  <li><a href="#s2"><span class="t-num">2.</span><span class="t-titre">Le systeme de tokens</span></a></li>
  <li><a href="#s3"><span class="t-num">3.</span><span class="t-titre">Les 7 themes</span></a></li>
  <li><a href="#s4"><span class="t-num">4.</span><span class="t-titre">Mecanique clair / sombre</span></a></li>
  <li><a href="#s5"><span class="t-num">5.</span><span class="t-titre">Integration MAUI</span></a></li>
  <li><a href="#s6"><span class="t-num">6.</span><span class="t-titre">Accessibilite au-dela de la couleur</span></a></li>
  <li><a href="#s7"><span class="t-num">7.</span><span class="t-titre">Ajouter ou modifier un theme</span></a></li>
</ul></section>

<h1 id="s1"><span class="num">1.</span> Principes &amp; structure</h1>
<p class="chapeau">Cette charte n'est pas un CSS de document : c'est le systeme de couleurs des applications MAUI. Elle prolonge l'engagement WCAG 2.2 AA de l'ADR-008 et les regles du doc 06.</p>
<h2>Deux axes independants</h2>
<p>Un <strong>theme</strong> (l'identite coloree) et un <strong>mode</strong> (clair / sombre) sont deux dimensions distinctes. Le theme « Ocean » existe en Ocean-Clair et Ocean-Sombre. L'utilisateur choisit les deux separement ; le mode peut aussi suivre le systeme. Resultat : 7 themes × 2 modes = <strong>14 palettes</strong>.</p>
<div class="encadre encadre--cle">
  <div class="titre-enc">L'idee d'inclusivite la plus importante</div>
  <p>On ne fait <strong>pas</strong> de « theme daltonien ». La securite daltonisme est une <strong>regle que tous les themes respectent</strong> : aucune information n'est transmise par la couleur seule (toujours doubler d'une icone, d'un libelle ou d'une forme — cf. doc 06). Les themes d'accessibilite dedies repondent a d'autres besoins : <strong>Contraste eleve</strong> (malvoyance, ratios AAA 7:1) et <strong>Sepia</strong> (photophobie, fatigue, faible lumiere bleue).</p>
</div>
<h2>Composition de la gamme</h2>
<p>Cinq themes <strong>esthetiques</strong> (Atlas, Ocean, Foret, Ambre, Amethyste) — laisser le choix est en soi inclusif (confort cognitif) — et deux themes <strong>d'accessibilite</strong> (Contraste eleve, Sepia). Tous, sans exception, passent les ratios WCAG verifies automatiquement.</p>

<h1 id="s2"><span class="num">2.</span> Le systeme de tokens</h1>
<p class="chapeau">L'application ne reference jamais une couleur brute, uniquement des <strong>tokens semantiques</strong>. Changer de theme = reaffecter ces tokens. C'est ce qui rend le systeme maintenable et le rebrand trivial.</p>
<table><thead><tr><th style="width:38%">Token</th><th style="width:62%">Role</th></tr></thead>
<tbody>{tok_rows}</tbody></table>
<div class="encadre">
  <div class="titre-enc">Pourquoi des tokens et pas des couleurs</div>
  <p>Si un ecran ecrit « bleu #1B4F7E » en dur, il casse des qu'on change de theme. S'il ecrit <code>primary</code>, il s'adapte a chaque theme et chaque mode sans toucher l'ecran. C'est le meme principe que l'architecture hexagonale : l'ecran depend d'une abstraction (le token), pas d'un detail (la couleur).</p>
</div>

<h1 id="s3"><span class="num">3.</span> Les 7 themes</h1>
<p class="chapeau">Chaque theme en clair et sombre, avec ses couleurs cles et ses ratios de contraste verifies. Tous depassent le seuil AA (4.5:1 texte, 3:1 UI) ; le theme Contraste eleve vise AAA (7:1).</p>
{themes_html}

<h1 id="s4"><span class="num">4.</span> Mecanique clair / sombre</h1>
<p>Le mode est porte par <code>Application.Current.UserAppTheme</code> (natif MAUI) : <code>Light</code>, <code>Dark</code>, ou <code>Unspecified</code> (suit le systeme). Le theme, lui, est un <code>ResourceDictionary</code> echange a chaud. Le <code>ThemeManager</code> combine les deux : il resout le clair/sombre effectif, puis charge le bon des 14 dictionnaires.</p>
<div class="encadre encadre--cle">
  <div class="titre-enc">Trois etats de mode, pas deux</div>
  <p>Ne jamais forcer clair OU sombre sans offrir « suivre le systeme ». Beaucoup d'utilisateurs (dont malvoyants et photophobes) reglent ce choix au niveau de l'OS : le respecter par defaut est une regle d'accessibilite.</p>
</div>

<h1 id="s5"><span class="num">5.</span> Integration MAUI</h1>
<p class="chapeau">Les fichiers sont generes (14 ResourceDictionaries XAML, ThemeManager, enum). Voici comment les cabler.</p>
<h2>5.1 Cote XAML — toujours via tokens</h2>
<pre><code>&lt;!-- BIEN : l'ecran depend du token, pas de la couleur --&gt;
&lt;Border BackgroundColor="{{DynamicResource surface}}"
        Stroke="{{DynamicResource outlineBrush}}"&gt;
  &lt;Label Text="Boulangerie Martin"
         TextColor="{{DynamicResource onSurface}}"
         FontAutoScalingEnabled="True" /&gt;
&lt;/Border&gt;

&lt;!-- MAL : couleur en dur, casse au changement de theme --&gt;
&lt;Label TextColor="#1B4F7E" /&gt;</code></pre>
<p>Important : utiliser <code>DynamicResource</code> (et non <code>StaticResource</code>) pour que le swap de theme mette a jour l'ecran a chaud.</p>
<h2>5.2 Cote C# — basculer</h2>
<pre><code>// Au demarrage (App.xaml.cs)
_themeManager.Initialize();

// Quand l'utilisateur choisit dans les reglages
_themeManager.SetTheme(AppThemeId.Ocean);
_themeManager.SetMode(AppTheme.Dark);   // ou Light / Unspecified (systeme)</code></pre>
<h2>5.3 Selecteur de theme accessible</h2>
<p>Dans l'ecran de reglages, exposer les 7 themes (via <code>AppThemeId</code> et <code>ToLabel()</code>) et les 3 modes. Chaque option doit avoir un <code>SemanticProperties.Description</code> et etre atteignable au clavier. Ne jamais identifier un theme uniquement par sa pastille de couleur : toujours le nom.</p>

<h1 id="s6"><span class="num">6.</span> Accessibilite au-dela de la couleur</h1>
<p class="chapeau">La couleur n'est qu'un axe. Le doc 06 impose des regles que cette charte rappelle, car un theme accessible dans une UI inaccessible ne sert a rien.</p>
<ul class="puces">
  <li><strong>Taille de texte</strong> : <code>FontAutoScalingEnabled="True"</code> partout. L'app doit rester utilisable a la taille de police maximale de l'OS.</li>
  <li><strong>Focus visible</strong> : anneau <code>focus</code> de 3 px minimum sur tout element interactif (jamais un simple changement de teinte).</li>
  <li><strong>Zones tactiles</strong> : >= 44×44 px (iOS) / 48×48 dp (Android).</li>
  <li><strong>Jamais l'info par la couleur seule</strong> : un etat « erreur » porte une icone + un libelle, pas qu'un rouge.</li>
  <li><strong>Police dyslexie</strong> : proposer une option (Atkinson Hyperlegible ou OpenDyslexic) — orthogonale au theme.</li>
  <li><strong>Lecteur d'ecran</strong> : <code>SemanticProperties.Description</code> sur les elements ; tester VoiceOver / TalkBack.</li>
</ul>

<h1 id="s7"><span class="num">7.</span> Ajouter ou modifier un theme</h1>
<p class="chapeau">Tout part d'une source unique. On ne modifie jamais le XAML a la main.</p>
<ol class="num">
  <li>Editer <code>palettes.py</code> : ajouter une entree dans <code>THEMES</code> avec ses 20 tokens en clair et sombre.</li>
  <li>Lancer <code>python3 palettes.py</code> : le verificateur signale toute palette qui echoue WCAG. Corriger jusqu'a « TOUTES LES PALETTES PASSENT ».</li>
  <li>Lancer <code>python3 generate.py</code> : regenere le JSON, les XAML, le ThemeManager et l'enum.</li>
  <li>Lancer <code>python3 build_preview.py</code> : met a jour l'apercu interactif pour validation visuelle.</li>
</ol>
<div class="encadre encadre--alerte">
  <div class="titre-enc">Regle non negociable</div>
  <p>Aucun theme n'est ajoute s'il ne passe pas le verificateur de contraste. C'est l'equivalent, pour la charte, de la Definition of Done accessibilite qui bloque une PR (doc 06, §12).</p>
</div>

<section class="cloture">
  <p class="label">Cloture</p>
  <h2 style="margin-top:.2em;">Mentions &amp; versions</h2>
  <div class="bloc-mentions">
    <p><strong>Charte d'interface interne Atlas.</strong> Source unique : <code>palettes.py</code>. Livrables generes : XAML MAUI, ThemeManager, apercu HTML.</p>
    <p>Tous les ratios de ce document sont calcules automatiquement et verifies &gt;= WCAG 2.2 AA.</p>
  </div>
  <table class="versions"><thead><tr><th>Version</th><th>Date</th><th>Auteur</th><th>Changements</th></tr></thead>
  <tbody><tr><td><span class="mono">1.0</span></td><td>29 mai 2026</td><td>Equipe Atlas</td><td>Creation : 7 themes accessibles, systeme de tokens, integration MAUI.</td></tr></tbody></table>
</section>

</body></html>"""

out = os.path.join(BUILD, "charte-apps.html")
open(out, "w", encoding="utf-8").write(HTML)
print("charte-apps.html genere dans build/")
