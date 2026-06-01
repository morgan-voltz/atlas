/* atlas-feed.jsx — Atlas · écran d'accueil (fil d'actualité entreprises)
   Doctrine : carte-aperçu = fait jamais verdict · source toujours présente ·
   non-lu = pastille + graisse (jamais la couleur seule) · success/error
   réservés aux états système · cibles ≥ 44 px. Tokens issus de themes.json.
   Icônes : SVG inline (tracés Tabler) — les polices d'icônes (PUA) sont
   filtrées par l'environnement d'aperçu. */

// ── Tokens Atlas (valeurs exactes de themes.json) ────────────────────────────
const ATLAS_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.09)', shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const ATLAS_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.10)', shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const T = (dark) => dark ? ATLAS_DARK : ATLAS_LIGHT;

const SANS = '"IBM Plex Sans", system-ui, sans-serif';
const SERIF = '"IBM Plex Serif", Georgia, serif';

// ── Icônes (tracés Tabler, rendus en SVG inline) ─────────────────────────────
const ICON_PATHS = {
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'arrows-sort':'<path d="M3 9l4 -4l4 4m-4 -4v14"/><path d="M21 15l-4 4l-4 -4m4 4v-14"/>',
  'arrow-up':'<path d="M12 5l0 14"/><path d="M18 11l-6 -6"/><path d="M6 11l6 -6"/>',
  'circle-check':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 12l2 2l4 -4"/>',
  'alert-triangle':'<path d="M12 9v4"/><path d="M10.363 3.591l-8.106 13.534a1.914 1.914 0 0 0 1.636 2.871h16.214a1.914 1.914 0 0 0 1.636 -2.87l-8.106 -13.536a1.914 1.914 0 0 0 -3.274 0z"/><path d="M12 16h.01"/>',
  'refresh':'<path d="M20 11a8.1 8.1 0 0 0 -15.5 -2m-.5 -4v4h4"/><path d="M4 13a8.1 8.1 0 0 0 15.5 2m.5 4v-4h-4"/>',
  'inbox':'<path d="M4 4m0 2a2 2 0 0 1 2 -2h12a2 2 0 0 1 2 2v12a2 2 0 0 1 -2 2h-12a2 2 0 0 1 -2 -2z"/><path d="M4 13h3l3 3h4l3 -3h3"/>',
  'news':'<path d="M16 6h3a1 1 0 0 1 1 1v11a2 2 0 0 1 -4 0v-13a1 1 0 0 0 -1 -1h-10a1 1 0 0 0 -1 1v12a3 3 0 0 0 3 3h11"/><path d="M8 8l4 0"/><path d="M8 12l4 0"/><path d="M8 16l4 0"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
  'file-text':'<path d="M14 3v4a1 1 0 0 0 1 1h4"/><path d="M17 21h-10a2 2 0 0 1 -2 -2v-14a2 2 0 0 1 2 -2h7l5 5v11a2 2 0 0 1 -2 2z"/><path d="M9 9l1 0"/><path d="M9 13l6 0"/><path d="M9 17l6 0"/>',
  'coins':'<path d="M9 14c0 1.657 2.686 3 6 3s6 -1.343 6 -3s-2.686 -3 -6 -3s-6 1.343 -6 3z"/><path d="M9 14v4c0 1.656 2.686 3 6 3s6 -1.344 6 -3v-4"/><path d="M3 6c0 1.072 1.144 2.062 3 2.598s4.144 .536 6 0c1.856 -.536 3 -1.526 3 -2.598c0 -1.072 -1.144 -2.062 -3 -2.598s-4.144 -.536 -6 0c-1.856 .536 -3 1.526 -3 2.598z"/><path d="M3 6v10c0 .888 .772 1.45 2 2"/><path d="M3 11c0 .888 .772 1.45 2 2"/>',
  'map-pin':'<path d="M9 11a3 3 0 1 0 6 0a3 3 0 0 0 -6 0"/><path d="M17.657 16.657l-4.243 4.243a2 2 0 0 1 -2.827 0l-4.244 -4.243a8 8 0 1 1 11.314 0z"/>',
  'gavel':'<path d="M13 10l7.383 7.418c.823 .82 .823 2.148 0 2.967a2.11 2.11 0 0 1 -2.976 0l-7.407 -7.385"/><path d="M6 9l4 4"/><path d="M13 10l-4 -4"/><path d="M3 21h7"/><path d="M6.793 15.793l-3.586 -3.586a1 1 0 0 1 0 -1.414l2.293 -2.293l.5 .5l3 -3l-.5 -.5l2.293 -2.293a1 1 0 0 1 1.414 0l3.586 3.586a1 1 0 0 1 0 1.414l-2.293 2.293l-.5 -.5l-3 3l.5 .5l-2.293 2.293a1 1 0 0 1 -1.414 0z"/>',
  'businessplan':'<path d="M16 6m-5 0a5 3 0 1 0 10 0a5 3 0 1 0 -10 0"/><path d="M11 6v4c0 1.657 2.239 3 5 3s5 -1.343 5 -3v-4"/><path d="M11 10v4c0 1.657 2.239 3 5 3s5 -1.343 5 -3v-4"/><path d="M11 14v4c0 1.657 2.239 3 5 3s5 -1.343 5 -3v-4"/><path d="M7 9h-2.5a1.5 1.5 0 0 0 0 3h1a1.5 1.5 0 0 1 0 3h-2.5"/><path d="M5 15v1m0 -8v1"/>',
  'building-bank':'<path d="M3 21l18 0"/><path d="M3 10l18 0"/><path d="M5 6l7 -3l7 3"/><path d="M4 10l0 11"/><path d="M20 10l0 11"/><path d="M8 14l0 3"/><path d="M12 14l0 3"/><path d="M16 14l0 3"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: ICON_PATHS[name] || '' }} />
  );
}

// ── Données du fil (faits, neutres, sourcés) ─────────────────────────────────
const TYPE_ICON = {
  depot:'file-text', dirigeant:'user', capital:'coins', transfert:'map-pin',
  procedure:'gavel', cession:'businessplan', immatriculation:'building-bank',
};
const EVENTS = [
  { id:1, name:'Ateliers Beaumont', form:'SAS', type:'depot',
    label:'Nouveau dépôt de comptes annuels', source:'BODACC',
    when:"Aujourd'hui · 09:14", group:"Aujourd'hui", unread:true },
  { id:2, name:'Maïa Conseil', form:'SARL', type:'dirigeant',
    label:'Changement de gérant', source:'RNE',
    when:"Aujourd'hui · 08:02", group:"Aujourd'hui", unread:true },
  { id:3, name:'Néo Mobilités', form:'SAS', type:'capital',
    label:'Augmentation de capital', source:'BODACC',
    when:"Aujourd'hui · 07:30", group:"Aujourd'hui", unread:true },
  { id:4, name:'Groupe Vidal Logistique', form:'SA', type:'transfert',
    label:'Transfert de siège social', source:'RNE',
    when:'Hier · 17:48', group:'Hier', unread:false },
  { id:5, name:'SCI du Vieux Port', form:'SCI', type:'depot',
    label:'Dépôt d’acte — modification statutaire', source:'RNE',
    when:'Hier · 11:20', group:'Hier', unread:false },
  { id:6, name:'Comptoir Pharmaceutique Lyonnais', form:'SA', type:'procedure',
    label:'Ouverture d’une procédure de sauvegarde', source:'BODACC',
    when:'Hier · 09:05', group:'Hier', unread:false },
  { id:7, name:'Établissements Royer', form:'SARL', type:'cession',
    label:'Cession de fonds de commerce', source:'BODACC',
    when:'27 mai', group:'27 mai', unread:false },
  { id:8, name:'Lumen Énergie', form:'SAS', type:'immatriculation',
    label:'Immatriculation au RCS', source:'RNE',
    when:'26 mai', group:'26 mai', unread:false },
];
const groupOrder = ["Aujourd'hui",'Hier','27 mai','26 mai'];
const byGroup = (evts) => groupOrder
  .map(g => ({ g, items: evts.filter(e => e.group === g) }))
  .filter(x => x.items.length);

// ── Atomes ───────────────────────────────────────────────────────────────────
function SourceBadge({ source, t }) {
  return (
    <span style={{
      flexShrink:0, fontSize:11, fontWeight:600, letterSpacing:'0.04em',
      padding:'3px 7px', borderRadius:6, lineHeight:1.3,
      background:t.surfaceVariant, color:t.onSurfaceVariant, fontFamily:SANS,
      whiteSpace:'nowrap',
    }}>{source}</span>
  );
}
function Chevron({ t }) {
  return <Icon name="chevron-right" size={18} color={t.outline} />;
}
function FormChip({ form, t }) {
  return <span style={{ fontWeight:400, color:t.onSurfaceVariant, fontSize:13 }}> · {form}</span>;
}
function DateHead({ label, t }) {
  return (
    <div style={{
      fontFamily:SANS, fontSize:11.5, fontWeight:600, letterSpacing:'0.07em',
      textTransform:'uppercase', color:t.onSurfaceVariant,
      padding:'18px 20px 8px',
    }}>{label}</div>
  );
}
function UnreadDot({ t, show }) {
  return (
    <span style={{ width:8, flexShrink:0, display:'flex', justifyContent:'center' }}>
      {show && <span style={{ width:8, height:8, borderRadius:'50%', background:t.primary }} />}
    </span>
  );
}

// ── Variante A — Sobre (liste groupée, texte pur) ────────────────────────────
function RowA({ e, t, last }) {
  return (
    <div role="article" tabIndex={0} className="atlas-row" style={{
      display:'flex', alignItems:'center', gap:12, minHeight:64,
      padding:'13px 16px', cursor:'pointer', position:'relative',
    }}>
      <UnreadDot t={t} show={e.unread} />
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{
          margin:0, fontSize:15, lineHeight:1.25, color:t.onSurface,
          fontWeight:e.unread?700:500, whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis',
        }}>{e.name}<FormChip form={e.form} t={t} /></p>
        <p style={{
          margin:'3px 0 0', fontSize:13, lineHeight:1.3, color:t.onSurfaceVariant,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis',
        }}>{e.label} · {e.when}</p>
      </div>
      <SourceBadge source={e.source} t={t} />
      <Chevron t={t} />
      {!last && <div style={{ position:'absolute', left:36, right:0, bottom:0, height:1, background:t.hairline }} />}
    </div>
  );
}
function FeedA({ t }) {
  return (
    <div>
      {byGroup(EVENTS).map(({ g, items }) => (
        <div key={g}>
          <DateHead label={g} t={t} />
          <div style={{ margin:'0 16px', background:t.surface, borderRadius:16,
            boxShadow:t.shadow, overflow:'hidden' }}>
            {items.map((e, i) => <RowA key={e.id} e={e} t={t} last={i === items.length - 1} />)}
          </div>
        </div>
      ))}
    </div>
  );
}

// ── Variante B — Indicateur de type (cartes, pastille-type) ──────────────────
function TypeTile({ type, t }) {
  return (
    <div style={{
      width:42, height:42, flexShrink:0, borderRadius:11,
      background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center',
    }}>
      <Icon name={TYPE_ICON[type]} size={20} color={t.onSurfaceVariant} />
    </div>
  );
}
function RowB({ e, t }) {
  return (
    <div role="article" tabIndex={0} className="atlas-row" style={{
      display:'flex', alignItems:'center', gap:12, minHeight:64,
      padding:'13px 14px', margin:'0 16px 10px', cursor:'pointer',
      background:t.surface, borderRadius:14, boxShadow:t.shadow,
    }}>
      <TypeTile type={e.type} t={t} />
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:15, lineHeight:1.25, color:t.onSurface,
          fontWeight:e.unread?700:500, display:'flex', alignItems:'center', gap:7,
          whiteSpace:'nowrap', overflow:'hidden' }}>
          {e.unread && <span style={{ width:7, height:7, borderRadius:'50%', background:t.primary, flexShrink:0 }} />}
          <span style={{ overflow:'hidden', textOverflow:'ellipsis' }}>{e.name}<FormChip form={e.form} t={t} /></span>
        </p>
        <p style={{ margin:'3px 0 0', fontSize:13, lineHeight:1.3, color:t.onSurfaceVariant,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{e.label} · {e.when}</p>
      </div>
      <SourceBadge source={e.source} t={t} />
      <Chevron t={t} />
    </div>
  );
}
function FeedB({ t }) {
  return (
    <div>
      {byGroup(EVENTS).map(({ g, items }) => (
        <div key={g}>
          <DateHead label={g} t={t} />
          {items.map(e => <RowB key={e.id} e={e} t={t} />)}
        </div>
      ))}
    </div>
  );
}

// ── Variante C — Rail chronologique (timeline, nœuds lu/non-lu) ───────────────
function RowC({ e, t }) {
  return (
    <div role="article" tabIndex={0} className="atlas-row" style={{
      display:'flex', alignItems:'flex-start', gap:10, position:'relative',
      padding:'12px 12px 12px 26px', cursor:'pointer', minHeight:44,
    }}>
      <span style={{
        position:'absolute', left:1, top:15, width:11, height:11, borderRadius:'50%',
        background:e.unread ? t.primary : t.surface,
        border:`2px solid ${e.unread ? t.primary : t.outline}`, boxSizing:'border-box',
        boxShadow:`0 0 0 3px ${t.background}`,
      }} />
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:14.5, lineHeight:1.25, color:t.onSurface,
          fontWeight:e.unread?700:500, whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>
          {e.name}<FormChip form={e.form} t={t} /></p>
        <p style={{ margin:'2px 0 0', fontSize:13.5, lineHeight:1.3, color:t.onSurface,
          fontWeight:e.unread?500:400, whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>
          {e.label}</p>
        <div style={{ display:'flex', alignItems:'center', gap:8, marginTop:6 }}>
          <span style={{ fontSize:12, color:t.onSurfaceVariant }}>{e.when}</span>
          <SourceBadge source={e.source} t={t} />
        </div>
      </div>
      <Chevron t={t} />
    </div>
  );
}
function FeedC({ t }) {
  return (
    <div style={{ paddingTop:2 }}>
      {byGroup(EVENTS).map(({ g, items }) => (
        <div key={g}>
          <DateHead label={g} t={t} />
          <div style={{ position:'relative', margin:'0 16px 4px' }}>
            <div style={{ position:'absolute', left:6, top:18, bottom:18, width:2,
              background:t.hairline }} />
            {items.map(e => <RowC key={e.id} e={e} t={t} />)}
          </div>
        </div>
      ))}
    </div>
  );
}

const FEEDS = { A:FeedA, B:FeedB, C:FeedC };

// ── En-tête sobre ─────────────────────────────────────────────────────────────
function Header({ t, banner = false, sort = true }) {
  return (
    <div style={{ padding:'58px 16px 12px', background:t.background, flexShrink:0 }}>
      <div style={{ display:'flex', alignItems:'flex-start', justifyContent:'space-between', gap:12 }}>
        <div style={{ minWidth:0 }}>
          <h1 style={{ fontFamily:SERIF, fontWeight:600, fontSize:28, lineHeight:1.05,
            margin:0, color:t.onSurface, letterSpacing:'-0.01em' }}>Accueil</h1>
          <p style={{ margin:'5px 0 0', fontSize:13, color:t.onSurfaceVariant }}>
            Mes entreprises · 12 suivies</p>
        </div>
        <button aria-label="Rechercher une entreprise" style={{
          width:44, height:44, flexShrink:0, borderRadius:13, cursor:'pointer',
          background:t.surface, border:`1px solid ${t.hairline}`, boxShadow:t.shadow,
          display:'flex', alignItems:'center', justifyContent:'center', color:t.primary,
        }}>
          <Icon name="search" size={20} color={t.primary} />
        </button>
      </div>
      {sort && (
        <div style={{ display:'flex', alignItems:'center', gap:6, marginTop:13,
          fontSize:12.5, color:t.onSurfaceVariant }}>
          <Icon name="arrows-sort" size={15} color={t.onSurfaceVariant} />
          <span>Trié par date · récents d'abord</span>
        </div>
      )}
      {banner && (
        <button style={{
          marginTop:12, width:'100%', display:'flex', alignItems:'center', gap:8,
          padding:'11px 13px', borderRadius:11, border:'none', cursor:'pointer',
          background:t.primaryContainer, color:t.onPrimaryContainer,
          fontFamily:SANS, fontSize:13.5, fontWeight:600,
        }}>
          <Icon name="arrow-up" size={16} color={t.onPrimaryContainer} />
          <span>3 nouveaux mouvements · afficher</span>
        </button>
      )}
    </div>
  );
}

// ── Repère « à jour » (état système — success autorisé) ──────────────────────
function UpToDate({ t }) {
  return (
    <div style={{ textAlign:'center', padding:'26px 16px 30px',
      display:'flex', flexDirection:'column', alignItems:'center' }}>
      <Icon name="circle-check" size={20} color={t.success} />
      <p style={{ margin:'7px 0 0', fontSize:13.5, fontWeight:600, color:t.onSurface }}>Tu es à jour</p>
      <p style={{ margin:'3px 0 0', fontSize:12, color:t.onSurfaceVariant }}>
        Dernière mise à jour il y a 3 minutes</p>
      <span style={{ display:'inline-block', marginTop:12, fontSize:12, color:t.onSurfaceVariant,
        background:t.surfaceVariant, padding:'5px 12px', borderRadius:8 }}>Recevoir un digest</span>
    </div>
  );
}

// ── Barre d'onglets (5 destinations) ─────────────────────────────────────────
const TABS = [
  ['Accueil','news'], ['Recherche','search'], ['Veille','rss'],
  ['Favoris','star'], ['Profil','user'],
];
function TabBar({ t, active = 0 }) {
  return (
    <div style={{
      flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px',
    }}>
      {TABS.map(([label, icon], i) => {
        const on = i === active;
        const col = on ? t.primary : t.outline;
        return (
          <div key={label} style={{
            flex:1, display:'flex', flexDirection:'column', alignItems:'center', gap:3,
            position:'relative', paddingTop:6, cursor:'pointer',
          }}>
            {on && <span style={{ position:'absolute', top:0, width:22, height:2,
              borderRadius:2, background:t.primary }} />}
            <Icon name={icon} size={22} color={col} stroke={on ? 2.2 : 1.9} />
            <span style={{ fontSize:10.5, fontWeight:on ? 600 : 500, color:col }}>{label}</span>
          </div>
        );
      })}
    </div>
  );
}

// ── Coquille d'écran (header + zone défilante + tab bar) ─────────────────────
function Screen({ t, children, banner = false, sort = true, active = 0 }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:SANS,
      WebkitFontSmoothing:'antialiased' }}>
      <Header t={t} banner={banner} sort={sort} />
      <div style={{ flex:1, overflow:'auto' }}>{children}</div>
      <TabBar t={t} active={active} />
    </div>
  );
}

// ── Écran principal : un fil, une variante de ligne ──────────────────────────
function FeedScreen({ variant = 'A', dark = false }) {
  const t = T(dark);
  const Feed = FEEDS[variant];
  return (
    <Screen t={t} banner={true}>
      <Feed t={t} />
      <UpToDate t={t} />
    </Screen>
  );
}

// ── État : vide d'onboarding ─────────────────────────────────────────────────
function EmptyScreen({ dark = false }) {
  const t = T(dark);
  return (
    <Screen t={t} sort={false}>
      <div style={{ height:'100%', display:'flex', flexDirection:'column',
        alignItems:'center', justifyContent:'center', textAlign:'center', padding:'0 40px 60px' }}>
        <div style={{ width:64, height:64, borderRadius:18, background:t.surface,
          boxShadow:t.shadow, display:'flex', alignItems:'center', justifyContent:'center' }}>
          <Icon name="inbox" size={30} color={t.outline} />
        </div>
        <p style={{ margin:'20px 0 0', fontFamily:SERIF, fontSize:19, fontWeight:600, color:t.onSurface }}>
          Ton fil est vide</p>
        <p style={{ margin:'8px 0 0', fontSize:14, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:260 }}>
          Suis une entreprise pour voir ses mouvements — dépôts, changements, procédures —
          apparaître ici.</p>
        <button style={{ marginTop:24, display:'inline-flex', alignItems:'center', gap:8,
          minHeight:48, padding:'0 22px', borderRadius:13, border:'none', cursor:'pointer',
          background:t.primary, color:t.onPrimary, fontFamily:SANS, fontSize:15, fontWeight:600 }}>
          <Icon name="search" size={18} color={t.onPrimary} />
          Rechercher une entreprise</button>
      </div>
    </Screen>
  );
}

// ── État : chargement (squelette calqué sur l'anatomie de C) ─────────────────
function SkeletonRowC({ t }) {
  const bar = (w, h) => (
    <span className="atlas-sk" style={{ display:'block', width:w, height:h,
      borderRadius:5, background:t.surfaceVariant }} />
  );
  return (
    <div style={{ display:'flex', gap:10, position:'relative',
      padding:'12px 12px 12px 26px', minHeight:64 }}>
      <span style={{ position:'absolute', left:1, top:15, width:11, height:11, borderRadius:'50%',
        background:t.surface, border:`2px solid ${t.outline}`, boxSizing:'border-box',
        boxShadow:`0 0 0 3px ${t.background}` }} />
      <div style={{ flex:1, minWidth:0, display:'flex', flexDirection:'column', gap:8 }}>
        {bar('52%', 13)}{bar('74%', 12)}
        <div style={{ display:'flex', gap:8, marginTop:1 }}>{bar(58, 11)}{bar(40, 15)}</div>
      </div>
    </div>
  );
}
function LoadingScreen({ dark = false }) {
  const t = T(dark);
  return (
    <Screen t={t}>
      <DateHead label="Chargement…" t={t} />
      <div style={{ position:'relative', margin:'0 16px 4px' }}>
        <div style={{ position:'absolute', left:6, top:18, bottom:18, width:2, background:t.hairline }} />
        {[0,1,2,3].map(i => <SkeletonRowC key={i} t={t} />)}
      </div>
    </Screen>
  );
}

// ── État : fin de liste (« à jour », quelques lignes lues) ───────────────────
function EndScreen({ dark = false }) {
  const t = T(dark);
  const tail = EVENTS.slice(-3).map(e => ({ ...e, unread:false }));
  return (
    <Screen t={t}>
      <DateHead label="Plus tôt" t={t} />
      <div style={{ margin:'0 16px', background:t.surface, borderRadius:16,
        boxShadow:t.shadow, overflow:'hidden' }}>
        {tail.map((e, i) => <RowA key={e.id} e={e} t={t} last={i === tail.length - 1} />)}
      </div>
      <UpToDate t={t} />
    </Screen>
  );
}

// ── Erreur de source — élément LOCAL dans le fil (doctrine §9) ───────────────
// Le fil agrège plusieurs sources ; si l'une échoue, seule SA portion affiche
// l'erreur (avec réessai), les autres sources restent affichées. error = état
// système, porté par icône + texte (jamais la couleur seule).
function InlineSourceError({ t, source }) {
  return (
    <div role="alert" style={{ margin:'0 16px 12px', background:t.surface,
      border:`1px solid ${t.hairline}`, borderRadius:13, padding:'13px 14px',
      display:'flex', gap:11, alignItems:'flex-start' }}>
      <span style={{ width:34, height:34, flexShrink:0, borderRadius:9,
        background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name="alert-triangle" size={18} color={t.error} />
      </span>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:14, fontWeight:600, color:t.onSurface }}>
          Source {source} injoignable</p>
        <p style={{ margin:'3px 0 0', fontSize:12.5, lineHeight:1.45, color:t.onSurfaceVariant }}>
          Ses mouvements ne sont pas chargés pour l'instant. Les autres sources s'affichent normalement.</p>
        <button style={{ marginTop:11, display:'inline-flex', alignItems:'center', gap:6,
          minHeight:44, padding:'0 16px', borderRadius:10, cursor:'pointer',
          background:'transparent', color:t.primary, border:`1px solid ${t.outline}`,
          fontFamily:SANS, fontSize:13.5, fontWeight:600 }}>
          <Icon name="refresh" size={15} color={t.primary} />
          Réessayer</button>
      </div>
    </div>
  );
}
function RailGroup({ t, children }) {
  return (
    <div style={{ position:'relative', margin:'0 16px 4px' }}>
      <div style={{ position:'absolute', left:6, top:18, bottom:18, width:2, background:t.hairline }} />
      {children}
    </div>
  );
}
// État : une source du fil est tombée (ici BODACC) — le reste s'affiche.
function ErrorScreen({ dark = false }) {
  const t = T(dark);
  const failed = 'BODACC';
  return (
    <Screen t={t} banner={false}>
      {byGroup(EVENTS).map(({ g, items }) => {
        const here = g === "Aujourd'hui";
        const shown = here ? items.filter(e => e.source !== failed) : items;
        return (
          <div key={g}>
            <DateHead label={g} t={t} />
            {here && <InlineSourceError t={t} source={failed} />}
            {shown.length > 0 && (
              <RailGroup t={t}>{shown.map(e => <RowC key={e.id} e={e} t={t} />)}</RailGroup>
            )}
          </div>
        );
      })}
    </Screen>
  );
}

Object.assign(window, { FeedScreen, EmptyScreen, LoadingScreen, EndScreen, ErrorScreen });
