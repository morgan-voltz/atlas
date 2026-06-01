/* atlas-favoris.jsx — Atlas · écran Favoris / watchlists (mobile)
   Écran de GESTION du portefeuille surveillé (≠ fil : aucun badge « nouveauté »).
   Puces de listes défilables · tags filtrants · kebab ⋮ TOUJOURS visible (voie
   accessible) + swipe comme raccourci · états vides global et par liste.
   Doctrine : descriptif · badges neutres (jamais rouge/vert sur une entité) ·
   la couleur n'est jamais le seul indice (libellé toujours présent) ·
   cibles ≥ 44 px · focus visible · puces & tags navigables au clavier.
   rouge réservé à l'action système destructive « Retirer ». Tokens : themes.json. */

const { useState, useRef } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const FAV_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', warning:'#9A6B00', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)', menuShadow:'0 6px 24px rgba(19,32,46,0.16)',
  shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const FAV_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', warning:'#E0B341', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)', menuShadow:'0 8px 28px rgba(0,0,0,0.5)',
  shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TV = (dark) => dark ? FAV_DARK : FAV_LIGHT;

const VSANS = '"IBM Plex Sans", system-ui, sans-serif';
const VSERIF = '"IBM Plex Serif", Georgia, serif';
const VMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const FAV_ICONS = {
  'dots-vertical':'<path d="M12 12m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M12 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M12 5m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/>',
  'timeline':'<path d="M4 16l6 -7l5 5l5 -6"/><path d="M4 16m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M10 9m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M15 14m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M20 8m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/>',
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'trash':'<path d="M4 7l16 0"/><path d="M10 11l0 6"/><path d="M14 11l0 6"/><path d="M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2 -2l1 -12"/><path d="M9 7v-3a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v3"/>',
  'folder-plus':'<path d="M12 19h-7a2 2 0 0 1 -2 -2v-11a2 2 0 0 1 2 -2h4l3 3h7a2 2 0 0 1 2 2v3.5"/><path d="M16 19h6"/><path d="M19 16v6"/>',
  'file-import':'<path d="M14 3v4a1 1 0 0 0 1 1h4"/><path d="M5 13v-8a2 2 0 0 1 2 -2h7l5 5v11a2 2 0 0 1 -2 2h-5.5"/><path d="M5 19h7"/><path d="M8 16l-3 3l3 3"/>',
  'plus':'<path d="M12 5l0 14"/><path d="M5 12l14 0"/>',
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'star-off':'<path d="M8.243 7.34l-6.38 .925l-.113 .023a1 1 0 0 0 -.44 1.684l4.622 4.499l-1.09 6.355l-.013 .11a1 1 0 0 0 1.464 .944l5.706 -3l5.708 3l.1 .046a1 1 0 0 0 1.352 -1.1l-1.091 -6.355l4.624 -4.5l.078 -.085a1 1 0 0 0 -.633 -1.62l-6.38 -.926l-2.852 -5.78a1 1 0 0 0 -1.794 0l-2.853 5.78z"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'news':'<path d="M16 6h3a1 1 0 0 1 1 1v11a2 2 0 0 1 -4 0v-13a1 1 0 0 0 -1 -1h-10a1 1 0 0 0 -1 1v12a3 3 0 0 0 3 3h11"/><path d="M8 8l4 0"/><path d="M8 12l4 0"/><path d="M8 16l4 0"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: FAV_ICONS[name] || '' }} />
  );
}

// ── Données ──────────────────────────────────────────────────────────────────
const ALL = [
  { id:1, name:'Ateliers Beaumont', siren:'552 032 534', form:'SAS', lists:['concurrence'], tags:['directe'] },
  { id:2, name:'Néo Mobilités', siren:'823 114 905', form:'SAS', lists:['concurrence'], tags:['directe','surveiller'] },
  { id:3, name:'Maïa Conseil', siren:'410 220 110', form:'SARL', lists:['clients'], tags:[] },
  { id:4, name:'SCI du Vieux Port', siren:'901 556 248', form:'SCI', lists:['clients'], tags:[] },
  { id:5, name:'Groupe Vidal Logistique', siren:'334 870 661', form:'SA', lists:['concurrence'], tags:['surveiller'] },
  { id:6, name:'Distribution Royer', siren:'552 901 233', form:'SAS', lists:['concurrence'], tags:['directe'] },
  { id:7, name:'Lumen Énergie', siren:'788 442 019', form:'SAS', lists:['clients'], tags:[] },
  { id:8, name:'Comptoir Lyonnais', siren:'305 118 774', form:'SA', lists:['concurrence'], tags:['surveiller'] },
];
const LISTS = [
  { key:'tous', label:'Tous', count:12 },
  { key:'concurrence', label:'Concurrence', count:5 },
  { key:'clients', label:'Clients', count:8 },
  { key:'veille', label:'Veille presse', count:0 },
];
const TAGS = {
  directe:   { label:'directe',       tone:'info' },
  surveiller:{ label:'à surveiller',  tone:'warning' },
};
const tagColor = (t, tone) => tone === 'warning' ? t.warning : t.info;

// ── Puces de listes (tablist horizontal, navigable clavier) ──────────────────
function ListChips({ t, active, onChange }) {
  const refs = useRef([]);
  const onKey = (e, i) => {
    if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
      e.preventDefault();
      const n = e.key === 'ArrowRight' ? (i + 1) % LISTS.length : (i - 1 + LISTS.length) % LISTS.length;
      onChange(LISTS[n].key); refs.current[n] && refs.current[n].focus();
    }
  };
  return (
    <div role="tablist" aria-label="Listes surveillées" style={{
      display:'flex', gap:8, overflowX:'auto', padding:'14px 16px 4px',
      WebkitOverflowScrolling:'touch', scrollbarWidth:'none',
    }}>
      {LISTS.map((l, i) => {
        const on = l.key === active;
        return (
          <button key={l.key} ref={el => refs.current[i] = el} role="tab" aria-selected={on}
            tabIndex={on ? 0 : -1} onClick={() => onChange(l.key)} onKeyDown={e => onKey(e, i)}
            className="atlas-row" style={{
              flexShrink:0, minHeight:44, padding:'0 14px', borderRadius:999, cursor:'pointer',
              display:'inline-flex', alignItems:'center', gap:7, fontFamily:VSANS, fontSize:13.5,
              background: on ? t.primaryContainer : 'transparent',
              border:`1px solid ${on ? t.primaryContainer : t.hairline}`,
              color: on ? t.onPrimaryContainer : t.onSurfaceVariant, fontWeight: on ? 600 : 500,
            }}>
            <span>{l.label}</span>
            <span style={{ fontFamily:VMONO, fontSize:12.5, fontWeight:600,
              color: on ? t.primary : t.outline }}>{l.count}</span>
          </button>
        );
      })}
      <button className="atlas-row" style={{
        flexShrink:0, minHeight:44, padding:'0 14px', borderRadius:999, cursor:'pointer',
        display:'inline-flex', alignItems:'center', gap:6, fontFamily:VSANS, fontSize:13.5,
        background:'transparent', border:`1px dashed ${t.outline}`, color:t.onSurfaceVariant, fontWeight:500,
      }}>
        <Icon name="plus" size={15} color={t.onSurfaceVariant} />
        <span>Liste</span>
      </button>
    </div>
  );
}

// ── Filtres par tag (toggle, navigables) + accès timeline ────────────────────
function TagFilters({ t, active, onToggle }) {
  return (
    <div style={{ display:'flex', alignItems:'center', gap:8, padding:'10px 16px 0', flexWrap:'wrap' }}>
      <span style={{ fontSize:11.5, color:t.outline, fontWeight:600, letterSpacing:'0.04em' }}>TAGS</span>
      {Object.entries(TAGS).map(([key, tg]) => {
        const on = active.has(key);
        return (
          <button key={key} aria-pressed={on} onClick={() => onToggle(key)} className="atlas-row" style={{
            minHeight:34, padding:'0 11px', borderRadius:9, cursor:'pointer',
            display:'inline-flex', alignItems:'center', gap:7, fontFamily:VSANS, fontSize:12.5, fontWeight:500,
            background: on ? t.surfaceVariant : 'transparent',
            border:`1px solid ${on ? 'transparent' : t.hairline}`,
            color: on ? t.onSurface : t.onSurfaceVariant,
          }}>
            <span style={{ width:8, height:8, borderRadius:'50%', flexShrink:0,
              background:tagColor(t, tg.tone) }} />
            <span>{tg.label}</span>
          </button>
        );
      })}
    </div>
  );
}
function TimelineLink({ t, label }) {
  return (
    <button className="atlas-row" style={{
      margin:'12px 16px 0', width:'calc(100% - 32px)', minHeight:48, cursor:'pointer',
      display:'flex', alignItems:'center', gap:10, padding:'0 14px',
      background:t.surfaceVariant, border:`1px solid ${t.hairline}`, borderRadius:12,
      fontFamily:VSANS, fontSize:13.5, fontWeight:500, color:t.onSurface, textAlign:'left',
    }}>
      <Icon name="timeline" size={18} color={t.primary} />
      <span style={{ flex:1 }}>Voir l'activité de {label}</span>
      <Icon name="chevron-right" size={18} color={t.outline} />
    </button>
  );
}

// ── Menu kebab (voie accessible, toujours disponible) ────────────────────────
function KebabMenu({ t, onClose, onRemove }) {
  return (
    <React.Fragment>
      <div onClick={onClose} style={{ position:'fixed', inset:0, zIndex:30 }} />
      <div role="menu" style={{
        position:'absolute', top:48, right:8, zIndex:31, minWidth:208,
        background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:13,
        boxShadow:t.menuShadow, overflow:'hidden', padding:'5px',
      }}>
        <button role="menuitem" onClick={onRemove} className="atlas-row" style={{
          width:'100%', minHeight:46, display:'flex', alignItems:'center', gap:11, padding:'0 11px',
          background:'transparent', border:'none', cursor:'pointer', borderRadius:9,
          fontFamily:VSANS, fontSize:14, color:t.onSurface, textAlign:'left',
        }}>
          <Icon name="star-off" size={18} color={t.onSurfaceVariant} />
          <span>Retirer du suivi</span>
        </button>
        <button role="menuitem" onClick={onClose} className="atlas-row" style={{
          width:'100%', minHeight:46, display:'flex', alignItems:'center', gap:11, padding:'0 11px',
          background:'transparent', border:'none', cursor:'pointer', borderRadius:9,
          fontFamily:VSANS, fontSize:14, color:t.onSurface, textAlign:'left',
        }}>
          <Icon name="folder-plus" size={18} color={t.onSurfaceVariant} />
          <span>Ajouter à une liste</span>
        </button>
      </div>
    </React.Fragment>
  );
}

// ── Carte-aperçu favori (kebab toujours visible + swipe raccourci) ───────────
const ACTION_W = 104;
function FavoriRow({ t, item, last, onRemove, demoOpen = false, demoSwiped = false }) {
  const [dx, setDx] = useState(demoSwiped ? -ACTION_W : 0);
  const [menu, setMenu] = useState(demoOpen);
  const drag = useRef(null);
  const down = (e) => { drag.current = { x:e.clientX, base:dx, moved:false }; e.currentTarget.setPointerCapture(e.pointerId); };
  const move = (e) => {
    if (!drag.current) return;
    const d = e.clientX - drag.current.x;
    if (Math.abs(d) > 4) drag.current.moved = true;
    setDx(Math.max(-ACTION_W, Math.min(0, drag.current.base + d)));
  };
  const up = () => { if (!drag.current) return; setDx(dx < -ACTION_W / 2 ? -ACTION_W : 0); drag.current = null; };

  return (
    <li style={{ position:'relative', listStyle:'none' }}>
      <div style={{ position:'relative', overflow:'hidden', borderRadius:dx ? 12 : 0 }}>
        {/* Action révélée par swipe — raccourci (rouge = action système destructive) */}
        <button aria-hidden={dx === 0} tabIndex={-1} onClick={() => onRemove(item.id)} style={{
          position:'absolute', inset:0, display:'flex', alignItems:'center', justifyContent:'flex-end',
          paddingRight:24, gap:8, background:t.error, border:'none', cursor:'pointer',
          color:t.onPrimary, fontFamily:VSANS, fontSize:13, fontWeight:600,
        }}>
          <Icon name="trash" size={18} color={t.onPrimary} />
          <span>Retirer</span>
        </button>
        {/* Avant-plan */}
        <div onPointerDown={down} onPointerMove={move} onPointerUp={up} onPointerCancel={up}
          style={{
            position:'relative', display:'flex', alignItems:'center', gap:10, minHeight:60,
            padding:'12px 6px 12px 16px', background:t.surface, touchAction:'pan-y',
            transform:`translateX(${dx}px)`, transition: drag.current ? 'none' : 'transform .22s ease',
          }}>
          <div style={{ flex:1, minWidth:0 }}>
            <p style={{ margin:0, fontSize:15, fontWeight:500, lineHeight:1.25, color:t.onSurface,
              whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{item.name}</p>
            <p style={{ margin:'3px 0 0', fontSize:12.5, lineHeight:1.3, color:t.onSurfaceVariant }}>
              <span style={{ fontFamily:VMONO }}>{item.siren}</span>
              <span> · {item.form}</span></p>
          </div>
          <button aria-label={`Actions pour ${item.name}`} aria-haspopup="menu" aria-expanded={menu}
            onClick={() => setMenu(m => !m)} className="atlas-row" style={{
              width:44, height:44, flexShrink:0, borderRadius:10, cursor:'pointer', border:'none',
              background:'transparent', display:'flex', alignItems:'center', justifyContent:'center' }}>
            <Icon name="dots-vertical" size={20} color={t.outline} />
          </button>
        </div>
      </div>
      {!last && dx === 0 && (
        <div style={{ position:'absolute', left:16, right:0, bottom:0, height:1, background:t.hairline }} />
      )}
      {menu && <KebabMenu t={t} onClose={() => setMenu(false)}
        onRemove={() => { setMenu(false); onRemove(item.id); }} />}
    </li>
  );
}

// ── États vides ──────────────────────────────────────────────────────────────
function EmptyGlobal({ t }) {
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
      justifyContent:'center', textAlign:'center', padding:'0 40px 60px' }}>
      <div style={{ width:64, height:64, borderRadius:18, background:t.surface, boxShadow:t.shadow,
        display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name="star" size={28} color={t.outline} />
      </div>
      <p style={{ margin:'20px 0 0', fontFamily:VSERIF, fontSize:19, fontWeight:600, color:t.onSurface }}>
        Aucune entreprise suivie</p>
      <p style={{ margin:'8px 0 0', fontSize:14, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:270 }}>
        Cherche une entreprise pour la suivre — elle apparaîtra ici, et ses mouvements dans ton fil.</p>
      <button style={{ marginTop:24, display:'inline-flex', alignItems:'center', gap:8,
        minHeight:48, padding:'0 22px', borderRadius:13, border:'none', cursor:'pointer',
        background:t.primary, color:t.onPrimary, fontFamily:VSANS, fontSize:15, fontWeight:600 }}>
        <Icon name="search" size={18} color={t.onPrimary} />
        Rechercher une entreprise</button>
    </div>
  );
}
function EmptyList({ t, label }) {
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
      justifyContent:'center', textAlign:'center', padding:'30px 40px 60px' }}>
      <div style={{ width:60, height:60, borderRadius:16, background:t.surface, boxShadow:t.shadow,
        display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name="folder-plus" size={26} color={t.outline} />
      </div>
      <p style={{ margin:'18px 0 0', fontFamily:VSERIF, fontSize:18, fontWeight:600, color:t.onSurface }}>
        « {label} » est vide</p>
      <p style={{ margin:'8px 0 0', fontSize:13.5, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:260 }}>
        Ajoute des entreprises déjà suivies, ou importe une liste de SIREN.</p>
      <div style={{ display:'flex', gap:10, marginTop:22 }}>
        <button style={{ display:'inline-flex', alignItems:'center', gap:7, minHeight:46, padding:'0 16px',
          borderRadius:12, cursor:'pointer', background:t.primary, color:t.onPrimary, border:'none',
          fontFamily:VSANS, fontSize:14, fontWeight:600 }}>
          <Icon name="plus" size={16} color={t.onPrimary} />Ajouter</button>
        <button style={{ display:'inline-flex', alignItems:'center', gap:7, minHeight:46, padding:'0 16px',
          borderRadius:12, cursor:'pointer', background:'transparent', color:t.onSurface,
          border:`1px solid ${t.outline}`, fontFamily:VSANS, fontSize:14, fontWeight:600 }}>
          <Icon name="file-import" size={16} color={t.onSurface} />Importer des SIREN</button>
      </div>
    </div>
  );
}

// ── Barre d'onglets ──────────────────────────────────────────────────────────
const FAV_TABS = [['Accueil','news'], ['Recherche','search'], ['Veille','rss'], ['Favoris','star'], ['Profil','user']];
function TabBar({ t, active = 3 }) {
  return (
    <div style={{ flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px' }}>
      {FAV_TABS.map(([label, icon], i) => {
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

// ── Écran Favoris ────────────────────────────────────────────────────────────
// mode: 'auto' | 'empty-global' | 'empty-list'
function FavorisScreen({ dark = false, initialList = 'tous', mode = 'auto', demo = false }) {
  const t = TV(dark);
  const [active, setActive] = useState(mode === 'empty-list' ? 'veille' : initialList);
  const [tags, setTags] = useState(new Set());
  const [removed, setRemoved] = useState(new Set());
  const meta = LISTS.find(l => l.key === active) || LISTS[0];

  const toggleTag = (k) => setTags(prev => {
    const n = new Set(prev); n.has(k) ? n.delete(k) : n.add(k); return n;
  });
  const onRemove = (id) => setRemoved(prev => new Set(prev).add(id));

  let items = active === 'tous' ? ALL : ALL.filter(e => e.lists.includes(active));
  if (tags.size) items = items.filter(e => e.tags.some(tg => tags.has(tg)));
  items = items.filter(e => !removed.has(e.id));

  const isList = active !== 'tous';
  const sub = mode === 'empty-global'
    ? 'Aucune entreprise surveillée'
    : active === 'tous'
      ? `${meta.count} entreprises surveillées`
      : `${meta.label} · ${meta.count} entreprise${meta.count > 1 ? 's' : ''}`;

  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:VSANS, WebkitFontSmoothing:'antialiased' }}>
      <div style={{ flexShrink:0, background:t.background, padding:'52px 16px 0' }}>
        <h1 style={{ margin:0, fontFamily:VSERIF, fontWeight:600, fontSize:26, lineHeight:1.05,
          color:t.onSurface, letterSpacing:'-0.01em' }}>Favoris</h1>
        <p style={{ margin:'5px 0 0', fontSize:13, color:t.onSurfaceVariant }}>{sub}</p>
      </div>

      {mode !== 'empty-global' && <ListChips t={t} active={active} onChange={setActive} />}
      {mode === 'auto' && isList && <TagFilters t={t} active={tags} onToggle={toggleTag} />}
      {mode === 'auto' && isList && <TimelineLink t={t} label={`« ${meta.label} »`} />}

      <div style={{ flex:1, overflow:'auto', display:'flex', flexDirection:'column',
        padding: mode === 'auto' ? '8px 16px 16px' : '0' }}>
        {mode === 'empty-global' ? <EmptyGlobal t={t} />
          : mode === 'empty-list' ? <EmptyList t={t} label={meta.label} />
          : (
            <ul style={{ margin:0, padding:0 }}>
              {items.map((item, i) => (
                <FavoriRow key={item.id} t={t} item={item} last={i === items.length - 1}
                  onRemove={onRemove}
                  demoOpen={demo && i === 0} demoSwiped={demo && i === 1} />
              ))}
            </ul>
          )}
      </div>

      <TabBar t={t} active={3} />
    </div>
  );
}

Object.assign(window, { FavorisScreen });
