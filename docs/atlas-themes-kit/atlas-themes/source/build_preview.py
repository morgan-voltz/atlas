# -*- coding: utf-8 -*-
"""Genere apercu-themes.html : un faux ecran Atlas pilote par variables CSS,
   avec selecteur de theme (7) et bascule clair/sombre. Les couleurs viennent
   de themes.json (source unique)."""
import json, os
ROOT = os.path.dirname(os.path.abspath(__file__))
data = json.load(open(os.path.join(ROOT, "themes.json"), encoding="utf-8"))

html = """<!DOCTYPE html>
<html lang="fr"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Apercu des themes - Atlas</title>
<style>
  * { box-sizing: border-box; }
  body { margin:0; font-family: system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
         background:#222; color:#eee; }
  .bar { position:sticky; top:0; z-index:10; background:#1a1a1a; padding:14px 18px;
         border-bottom:1px solid #333; display:flex; flex-wrap:wrap; gap:14px; align-items:center; }
  .bar h1 { font-size:15px; margin:0 10px 0 0; font-weight:600; letter-spacing:.04em; }
  .group { display:flex; gap:6px; flex-wrap:wrap; align-items:center; }
  .group .lbl { font-size:11px; text-transform:uppercase; letter-spacing:.08em; color:#999; margin-right:4px; }
  button.pill { border:1px solid #444; background:#2a2a2a; color:#ddd; border-radius:20px;
        padding:6px 14px; font-size:13px; cursor:pointer; }
  button.pill[aria-pressed="true"] { background:#fff; color:#111; border-color:#fff; font-weight:600; }
  button.pill.a11y { border-style:dashed; }
  .stage { display:flex; justify-content:center; padding:28px 16px 60px; }
  /* Le "device" est entierement pilote par les tokens du theme */
  .device { width:380px; max-width:100%; border-radius:22px; overflow:hidden;
        background:var(--background); color:var(--onBackground);
        box-shadow:0 20px 60px rgba(0,0,0,.5); border:1px solid var(--outline); }
  .topbar { background:var(--primary); color:var(--onPrimary); padding:16px 18px;
        display:flex; align-items:center; justify-content:space-between; }
  .topbar .t { font-size:16px; font-weight:700; }
  .topbar .av { width:30px; height:30px; border-radius:50%; background:var(--onPrimary);
        opacity:.9; }
  .body { padding:16px; }
  .search { display:flex; gap:8px; margin-bottom:16px; }
  .search input { flex:1; border:1.5px solid var(--outline); background:var(--surface);
        color:var(--onSurface); border-radius:10px; padding:11px 12px; font-size:14px; }
  .search input::placeholder { color:var(--onSurfaceVariant); }
  .search button { background:var(--primary); color:var(--onPrimary); border:none;
        border-radius:10px; padding:0 16px; font-weight:600; font-size:14px; cursor:pointer; }
  .card { background:var(--surface); border:1px solid var(--outline); border-radius:14px;
        padding:15px; margin-bottom:14px; }
  .card h2 { margin:0 0 2px; font-size:16px; color:var(--onSurface); }
  .card .siren { font-family:ui-monospace,monospace; font-size:12px; color:var(--onSurfaceVariant); }
  .meta { font-size:13px; color:var(--onSurfaceVariant); margin:8px 0 12px; line-height:1.5; }
  .chips { display:flex; gap:6px; flex-wrap:wrap; margin-bottom:12px; }
  .chip { font-size:11px; font-weight:600; padding:3px 9px; border-radius:12px; }
  .chip.s { background:var(--success); color:var(--onSuccess); }
  .chip.w { background:var(--warning); color:var(--onWarning); }
  .chip.e { background:var(--error); color:var(--onError); }
  .chip.i { background:var(--info); color:var(--onInfo); }
  .row { display:flex; gap:10px; }
  .btn-primary { flex:1; background:var(--primary); color:var(--onPrimary); border:none;
        border-radius:10px; padding:11px; font-weight:600; font-size:14px; cursor:pointer; }
  .btn-ghost { flex:1; background:var(--primaryContainer); color:var(--onPrimaryContainer);
        border:none; border-radius:10px; padding:11px; font-weight:600; font-size:14px; cursor:pointer; }
  .timeline { margin-top:4px; }
  .ti { background:var(--surfaceVariant); border-radius:10px; padding:11px 12px; margin-bottom:8px; }
  .ti .h { font-size:13px; font-weight:600; color:var(--onSurface); }
  .ti .d { font-size:12px; color:var(--onSurfaceVariant); margin-top:2px; }
  .focus-demo { margin-top:6px; }
  .focus-demo button { background:var(--surface); color:var(--onSurface);
        border:1.5px solid var(--outline); border-radius:10px; padding:10px 14px;
        font-size:14px; cursor:pointer; outline:3px solid var(--focus); outline-offset:2px; }
  .ratios { max-width:380px; margin:0 auto; font-size:12px; color:#bbb; }
  .ratios table { width:100%; border-collapse:collapse; margin-top:10px; }
  .ratios td { padding:5px 8px; border-bottom:1px solid #333; }
  .ratios td:last-child { text-align:right; font-family:ui-monospace,monospace; color:#7fe08a; }
  .note { max-width:380px; margin:18px auto 0; font-size:12px; color:#888; line-height:1.6; text-align:center; }
</style></head>
<body>
<div class="bar">
  <h1>Apercu des themes - Atlas</h1>
  <div class="group" id="themes"><span class="lbl">Theme</span></div>
  <div class="group" id="modes"><span class="lbl">Mode</span></div>
</div>

<div class="stage">
  <div>
    <div class="device" id="device">
      <div class="topbar"><span class="t">Atlas</span><span class="av"></span></div>
      <div class="body">
        <div class="search">
          <input placeholder="Rechercher une entreprise, un SIREN..." aria-label="Recherche">
          <button>OK</button>
        </div>
        <div class="card">
          <h2>Boulangerie Martin SARL</h2>
          <div class="siren">SIREN 552 032 534</div>
          <div class="meta">Boulangerie et patisserie (NAF 10.71C)<br>12 rue des Lilas, 75011 Paris</div>
          <div class="chips">
            <span class="chip s">Active</span>
            <span class="chip i">2 marques</span>
            <span class="chip w">RCS a jour</span>
            <span class="chip e">1 procedure</span>
          </div>
          <div class="row">
            <button class="btn-primary">Surveiller</button>
            <button class="btn-ghost">Favori</button>
          </div>
        </div>
        <div class="timeline">
          <div class="ti"><div class="h">Nouveau dirigeant nomme</div><div class="d">BODACC - il y a 2 jours</div></div>
          <div class="ti"><div class="h">Depot de marque "MARTIN BIO"</div><div class="d">INPI - il y a 5 jours</div></div>
        </div>
        <div class="focus-demo"><button>Bouton avec focus visible</button></div>
      </div>
    </div>
    <div class="ratios">
      <table id="ratios"></table>
    </div>
    <p class="note">Ceci est une demo : tout l'ecran est pilote par les tokens du theme actif,
       exactement comme l'app MAUI. Les ratios affiches sont calcules en direct.</p>
  </div>
</div>

<script>
const THEMES = __DATA__;
const TOKENS = ["background","surface","surfaceVariant","onBackground","onSurface",
  "onSurfaceVariant","outline","primary","onPrimary","primaryContainer","onPrimaryContainer",
  "success","onSuccess","warning","onWarning","error","onError","info","onInfo","focus"];
let curTheme = "Atlas", curDark = false;

function lin(c){c/=255;return c<=0.03928?c/12.92:Math.pow((c+0.055)/1.055,2.4);}
function lum(hex){const h=hex.replace('#','');
  return 0.2126*lin(parseInt(h.slice(0,2),16))+0.7152*lin(parseInt(h.slice(2,4),16))+0.0722*lin(parseInt(h.slice(4,6),16));}
function ratio(a,b){const l1=lum(a),l2=lum(b);const hi=Math.max(l1,l2),lo=Math.min(l1,l2);return (hi+0.05)/(lo+0.05);}

function apply(){
  const pal = THEMES[curTheme][curDark?"dark":"light"];
  const d = document.getElementById('device');
  TOKENS.forEach(t => d.style.setProperty('--'+t, pal[t]));
  // ratios
  const pairs = [["Texte principal","onSurface","surface"],
                 ["Texte secondaire","onSurfaceVariant","surface"],
                 ["Primaire / fond","primary","background"],
                 ["Texte sur primaire","onPrimary","primary"]];
  document.getElementById('ratios').innerHTML = pairs.map(([lbl,a,b])=>{
    const r = ratio(pal[a],pal[b]).toFixed(2);
    return `<tr><td>${lbl}</td><td>${r}:1</td></tr>`;}).join('');
  // pressed states
  document.querySelectorAll('#themes .pill').forEach(b=>b.setAttribute('aria-pressed', b.dataset.k===curTheme));
  document.querySelectorAll('#modes .pill').forEach(b=>b.setAttribute('aria-pressed', (b.dataset.m==='dark')===curDark));
}

const tc = document.getElementById('themes');
Object.entries(THEMES).forEach(([k,t])=>{
  const b=document.createElement('button');
  b.className='pill'+(t.kind==='a11y'?' a11y':''); b.dataset.k=k; b.textContent=t.label;
  b.onclick=()=>{curTheme=k;apply();}; tc.appendChild(b);
});
const mc = document.getElementById('modes');
[["light","Clair"],["dark","Sombre"]].forEach(([m,lbl])=>{
  const b=document.createElement('button'); b.className='pill'; b.dataset.m=m; b.textContent=lbl;
  b.onclick=()=>{curDark=(m==='dark');apply();}; mc.appendChild(b);
});
apply();
</script>
</body></html>
"""
html = html.replace("__DATA__", json.dumps(data, ensure_ascii=False))
out = os.path.join(ROOT, "apercu-themes.html")
open(out, "w", encoding="utf-8").write(html)
print("apercu-themes.html genere")
