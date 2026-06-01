/* atlas-recherche.jsx — Atlas · écran Recherche (mobile)
   Un champ unique intelligent qui détecte ET AFFICHE l'intention.
   État A : SIREN (9 chiffres) → carte d'accès direct, proposée (pas d'auto-ouverture).
   État B : texte → segments Entreprises/Marques + liste réactive (carte-aperçu).
   Doctrine : descriptif (aucun verdict) · badges neutres · diffusion restreinte
   = fait de couverture (jamais masqué) · success/error réservés aux états système ·
   champ qui annonce l'intention · segments navigables clavier · cibles ≥ 44 px ·
   focus visible. Tokens issus de themes.json. */

const { useState, useRef, useEffect } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const RECH_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', infoContainer:'#DCEAF6', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)', shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const RECH_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', infoContainer:'#1A3243', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)', shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TR = (dark) => dark ? RECH_DARK : RECH_LIGHT;

const RSANS = '"IBM Plex Sans", system-ui, sans-serif';
const RSERIF = '"IBM Plex Serif", Georgia, serif';
const RMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const RECH_ICONS = {
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'circle-check':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 12l2 2l4 -4"/>',
  'building':'<path d="M3 21l18 0"/><path d="M9 8l1 0"/><path d="M9 12l1 0"/><path d="M9 16l1 0"/><path d="M14 8l1 0"/><path d="M14 12l1 0"/><path d="M14 16l1 0"/><path d="M5 21v-16a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v16"/>',
  'arrow-right':'<path d="M5 12l14 0"/><path d="M13 18l6 -6"/><path d="M13 6l6 6"/>',
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'x':'<path d="M18 6l-12 12"/><path d="M6 6l12 12"/>',
  'trademark':'<path d="M9 15v-6h-2m2 3h-2m13 3v-6l-2.5 4l-2.5 -4v6m-3 -6h-12"/>',
  'mood-search':'<path d="M3 12a9 9 0 1 0 9 -9"/><path d="M9 10l.01 0"/><path d="M15 10l.01 0"/>',
  'news':'<path d="M16 6h3a1 1 0 0 1 1 1v11a2 2 0 0 1 -4 0v-13a1 1 0 0 0 -1 -1h-10a1 1 0 0 0 -1 1v12a3 3 0 0 0 3 3h11"/><path d="M8 8l4 0"/><path d="M8 12l4 0"/><path d="M8 16l4 0"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: RECH_ICONS[name] || '' }} />
  );
}

// ── Données simulées ─────────────────────────────────────────────────────────
const ENTREPRISES = [
  { id:1, name:'Ateliers Beaumont', siren:'552 032 534', form:'SAS', ville:'Lyon' },
  { id:2, name:'Beaumont & Fils', siren:'410 220 110', form:'SARL', ville:'Lille' },
  { id:3, name:'Groupe Beaumont Immobilier', siren:'823 114 905', form:'SA', ville:'Paris' },
  { id:4, name:'SCI Beaumont', restricted:true, form:'SCI', ville:'—' },
  { id:5, name:'Beaumont Conseil', siren:'901 556 248', form:'SASU', ville:'Bordeaux' },
  { id:6, name:'Distribution Beaumont', siren:'334 870 661', form:'SARL', ville:'Nantes' },
];
const MARQUES = [
  { id:1, name:'BEAUMONT', siren:'INPI · FR · 2019', form:'Marque verbale', ville:'Cl. 19, 37' },
  { id:2, name:'Beaumont Atelier', siren:'INPI · FR · 2022', form:'Marque semi-figurative', ville:'Cl. 20' },
];
const COUNTS = { entreprises:6, marques:2 };

// ── Champ unique intelligent ─────────────────────────────────────────────────
const digitsOf = (s) => (s.match(/\d/g) || []).join('');
const isSiren = (s) => digitsOf(s).length === 9 && /^[\d\s]+$/.test(s.trim());

function SmartField({ t, value, onChange, siren }) {
  const ref = useRef(null);
  return (
    <div>
      <div className="atlas-field" style={{
        display:'flex', alignItems:'center', gap:10, minHeight:50,
        background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:13,
        padding:'0 12px', boxShadow:t.shadow,
      }}>
        <Icon name="search" size={19} color={t.outline} />
        <input ref={ref} value={value} onChange={e => onChange(e.target.value)}
          inputMode="search" enterKeyHint="search" aria-label="Rechercher une entreprise, un SIREN ou une marque"
          aria-describedby="intent-hint" placeholder="Nom, SIREN ou marque…"
          style={{
            flex:1, minWidth:0, border:'none', outline:'none', background:'transparent',
            fontFamily: siren ? RMONO : RSANS, fontSize:15, color:t.onSurface,
            padding:'13px 0',
          }} />
        {value && (
          <button aria-label="Effacer" onClick={() => { onChange(''); ref.current && ref.current.focus(); }}
            className="atlas-row" style={{
              width:30, height:30, flexShrink:0, borderRadius:8, cursor:'pointer', border:'none',
              background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center' }}>
            <Icon name="x" size={15} color={t.onSurfaceVariant} />
          </button>
        )}
      </div>
      {/* Indice d'intention — annoncé en aria-live */}
      <div id="intent-hint" aria-live="polite" style={{ minHeight:18, margin:'7px 2px 0' }}>
        {siren ? (
          <span style={{ display:'inline-flex', alignItems:'center', gap:6,
            fontSize:12, color:t.info, fontWeight:500 }}>
            <Icon name="circle-check" size={14} color={t.info} />
            SIREN valide détecté
          </span>
        ) : value.trim() ? (
          <span style={{ fontSize:12, color:t.onSurfaceVariant }}>Recherche par nom</span>
        ) : null}
      </div>
    </div>
  );
}

// ── Segments (tablist, navigable au clavier) ─────────────────────────────────
function Segments({ t, active, onChange }) {
  const tabs = [['entreprises','Entreprises', COUNTS.entreprises], ['marques','Marques', COUNTS.marques]];
  const refs = useRef([]);
  const onKey = (e, i) => {
    if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
      e.preventDefault();
      const next = e.key === 'ArrowRight' ? (i + 1) % tabs.length : (i - 1 + tabs.length) % tabs.length;
      onChange(tabs[next][0]); refs.current[next] && refs.current[next].focus();
    }
  };
  return (
    <div role="tablist" aria-label="Type de résultat" style={{ display:'flex', gap:8, padding:'14px 0 6px' }}>
      {tabs.map(([key, label, n], i) => {
        const on = key === active;
        return (
          <button key={key} ref={el => refs.current[i] = el} role="tab" aria-selected={on}
            tabIndex={on ? 0 : -1} onClick={() => onChange(key)} onKeyDown={e => onKey(e, i)}
            className="atlas-row" style={{
              minHeight:44, padding:'0 14px', borderRadius:11, cursor:'pointer',
              display:'inline-flex', alignItems:'center', gap:7, fontFamily:RSANS, fontSize:13.5,
              background: on ? t.primaryContainer : 'transparent',
              border:`1px solid ${on ? t.primaryContainer : t.hairline}`,
              color: on ? t.onPrimaryContainer : t.onSurfaceVariant,
              fontWeight: on ? 600 : 500,
            }}>
            <span>{label}</span>
            <span style={{ fontFamily:RMONO, fontSize:12.5, fontWeight:600,
              color: on ? t.primary : t.outline }}>{n}</span>
          </button>
        );
      })}
    </div>
  );
}

// ── Carte-aperçu de résultat ─────────────────────────────────────────────────
function ResultRow({ t, item, last }) {
  return (
    <div role="link" tabIndex={0} className="atlas-row" style={{
      display:'flex', alignItems:'center', gap:12, minHeight:60,
      padding:'12px 4px', cursor:'pointer', position:'relative',
    }}>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:15, fontWeight:500, lineHeight:1.25, color:t.onSurface,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{item.name}</p>
        {item.restricted ? (
          <p style={{ margin:'3px 0 0', fontSize:12.5, lineHeight:1.3, color:t.outline,
            display:'flex', alignItems:'center', gap:8 }}>
            <span style={{ fontStyle:'italic' }}>Diffusion restreinte (INSEE)</span>
          </p>
        ) : (
          <p style={{ margin:'3px 0 0', fontSize:12.5, lineHeight:1.3, color:t.onSurfaceVariant,
            whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>
            <span style={{ fontFamily:RMONO }}>{item.siren}</span>
            <span> · {item.form} · {item.ville}</span>
          </p>
        )}
      </div>
      {item.restricted
        ? <span style={{ flexShrink:0, fontSize:11, fontWeight:500, padding:'2px 8px',
            borderRadius:7, background:t.surfaceVariant, color:t.onSurfaceVariant }}>restreint</span>
        : null}
      <Icon name="chevron-right" size={18} color={t.outline} />
      {!last && <div style={{ position:'absolute', left:4, right:0, bottom:0, height:1, background:t.hairline }} />}
    </div>
  );
}

// ── Carte d'accès direct (État A) — proposée, jamais auto-ouverte ────────────
function DirectAccess({ t, siren }) {
  return (
    <button className="atlas-row" style={{
      marginTop:16, width:'100%', display:'flex', alignItems:'center', gap:14,
      background:t.surface, border:`1px solid ${t.primaryContainer}`, borderRadius:16,
      boxShadow:t.shadow, padding:'15px 16px', cursor:'pointer', textAlign:'left',
    }}>
      <span style={{ width:44, height:44, flexShrink:0, borderRadius:12,
        background:t.infoContainer, display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name="building" size={22} color={t.info} />
      </span>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:15, fontWeight:600, color:t.onSurface, fontFamily:RSANS }}>
          Entreprise <span style={{ fontFamily:RMONO }}>{siren}</span></p>
        <p style={{ margin:'3px 0 0', fontSize:13, color:t.onSurfaceVariant }}>Ouvrir la fiche</p>
      </div>
      <Icon name="arrow-right" size={20} color={t.primary} />
    </button>
  );
}

// ── États liste : chargement (squelette) & vide ──────────────────────────────
function SkeletonRow({ t, last }) {
  const bar = (w, h) => (
    <span className="atlas-sk" style={{ display:'block', width:w, height:h, borderRadius:5, background:t.surfaceVariant }} />
  );
  return (
    <div style={{ display:'flex', alignItems:'center', gap:12, minHeight:60, padding:'12px 4px', position:'relative' }}>
      <div style={{ flex:1, minWidth:0, display:'flex', flexDirection:'column', gap:8 }}>
        {bar('46%', 14)}{bar('66%', 12)}
      </div>
      {!last && <div style={{ position:'absolute', left:4, right:0, bottom:0, height:1, background:t.hairline }} />}
    </div>
  );
}
function EmptyResults({ t, query }) {
  return (
    <div style={{ textAlign:'center', padding:'46px 30px', display:'flex',
      flexDirection:'column', alignItems:'center' }}>
      <Icon name="mood-search" size={30} color={t.outline} />
      <p style={{ margin:'14px 0 0', fontSize:15, fontWeight:600, color:t.onSurface, fontFamily:RSANS }}>
        Aucun résultat pour « {query} »</p>
      <p style={{ margin:'6px 0 0', fontSize:13, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:250 }}>
        Vérifie l'orthographe, ou saisis un SIREN à 9 chiffres pour un accès direct.</p>
    </div>
  );
}

// ── Écran Recherche ──────────────────────────────────────────────────────────
// mode: 'auto' (dérive de la saisie), 'loading', 'empty'
function SearchScreen({ dark = false, initial = '', mode = 'auto' }) {
  const t = TR(dark);
  const [q, setQ] = useState(initial);
  const [seg, setSeg] = useState('entreprises');
  const siren = isSiren(q);
  const list = seg === 'entreprises' ? ENTREPRISES : MARQUES;

  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:RSANS, WebkitFontSmoothing:'antialiased' }}>
      <div style={{ flexShrink:0, background:t.background, padding:'52px 16px 8px' }}>
        <h1 style={{ margin:'0 0 14px', fontFamily:RSERIF, fontWeight:600, fontSize:26,
          lineHeight:1.05, color:t.onSurface, letterSpacing:'-0.01em' }}>Recherche</h1>
        <SmartField t={t} value={q} onChange={setQ} siren={siren} />
      </div>

      <div style={{ flex:1, overflow:'auto', padding:'0 16px 16px' }}>
        {/* État A — SIREN détecté : carte d'accès direct, pas de segments/liste */}
        {siren && mode === 'auto' && (
          <DirectAccess t={t} siren={q.trim()} />
        )}

        {/* État chargement */}
        {!siren && mode === 'loading' && (
          <div style={{ paddingTop:10 }}>
            {[0,1,2,3,4].map(i => <SkeletonRow key={i} t={t} last={i === 4} />)}
          </div>
        )}

        {/* État vide */}
        {!siren && mode === 'empty' && (
          <EmptyResults t={t} query={q.trim() || 'zxqw'} />
        )}

        {/* État B — texte : segments + liste réactive */}
        {!siren && mode === 'auto' && q.trim() && (
          <div>
            <Segments t={t} active={seg} onChange={setSeg} />
            <div>
              {list.map((item, i) => (
                <ResultRow key={item.id} t={t} item={item} last={i === list.length - 1} />
              ))}
            </div>
          </div>
        )}

        {/* Champ vide en mode auto : invite discrète */}
        {!siren && mode === 'auto' && !q.trim() && (
          <div style={{ textAlign:'center', padding:'46px 30px', display:'flex',
            flexDirection:'column', alignItems:'center' }}>
            <Icon name="search" size={26} color={t.outline} />
            <p style={{ margin:'12px 0 0', fontSize:14, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:240 }}>
              Cherche par nom, par SIREN (9 chiffres) ou par marque.</p>
          </div>
        )}
      </div>

      <TabBar t={t} active={1} />
    </div>
  );
}

// ── Barre d'onglets ──────────────────────────────────────────────────────────
const RECH_TABS = [
  ['Accueil','news'], ['Recherche','search'], ['Veille','rss'],
  ['Favoris','star'], ['Profil','user'],
];
function TabBar({ t, active = 1 }) {
  return (
    <div style={{ flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px' }}>
      {RECH_TABS.map(([label, icon], i) => {
        const on = i === active; const col = on ? t.primary : t.outline;
        return (
          <div key={label} style={{ flex:1, display:'flex', flexDirection:'column',
            alignItems:'center', gap:3, position:'relative', paddingTop:6, cursor:'pointer' }}>
            {on && <span style={{ position:'absolute', top:0, width:22, height:2, borderRadius:2, background:t.primary }} />}
            <Icon name={icon} size={22} color={col} stroke={on ? 2.2 : 1.9} />
            <span style={{ fontSize:10.5, fontWeight:on ? 600 : 500, color:col }}>{label}</span>
          </div>
        );
      })}
    </div>
  );
}

Object.assign(window, { SearchScreen });
