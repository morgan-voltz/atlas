/* atlas-veille.jsx — Atlas · écran Veille (mobile)
   Flux de LECTURE éditoriale (RSS packs métier + sources libres) — PAS un fil de
   mouvements d'entreprises. Deux moments : lecture (panneau replié) et gestion
   (panneau de filtres ouvert), plus la vue lecture d'un item.
   Doctrine : descriptif (titres = faits) · non-lu = pastille + graisse (jamais la
   couleur seule) · pas de rouge/vert pour qualifier un contenu · filtre actif
   visible même replié · mention d'entreprise = suggestion à vérifier ·
   cibles ≥ 44 px · focus visible. Tokens : themes.json. */

const { useState } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const VEI_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', infoContainer:'#DCEAF6', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)', scrim:'rgba(19,32,46,0.34)',
  sheetShadow:'0 -8px 40px rgba(19,32,46,0.20)',
  shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const VEI_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', infoContainer:'#1A3243', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)', scrim:'rgba(0,0,0,0.55)',
  sheetShadow:'0 -8px 40px rgba(0,0,0,0.55)',
  shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TVE = (dark) => dark ? VEI_DARK : VEI_LIGHT;

const ESANS = '"IBM Plex Sans", system-ui, sans-serif';
const ESERIF = '"IBM Plex Serif", Georgia, serif';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const VEI_ICONS = {
  'filter':'<path d="M4 4h16v2.172a2 2 0 0 1 -.586 1.414l-4.414 4.414v7l-6 2v-8.5l-4.48 -4.928a2 2 0 0 1 -.52 -1.345v-2.227z"/>',
  'adjustments':'<path d="M14 6m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 6l8 0"/><path d="M16 6l4 0"/><path d="M8 12m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 12l2 0"/><path d="M10 12l10 0"/><path d="M17 18m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 18l11 0"/><path d="M19 18l1 0"/>',
  'x':'<path d="M18 6l-12 12"/><path d="M6 6l12 12"/>',
  'plus':'<path d="M12 5l0 14"/><path d="M5 12l14 0"/>',
  'circle':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/>',
  'circle-check':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 12l2 2l4 -4"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'archive':'<path d="M3 4m0 2a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v0a2 2 0 0 1 -2 2h-14a2 2 0 0 1 -2 -2z"/><path d="M5 8v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-10"/><path d="M10 12l4 0"/>',
  'external-link':'<path d="M12 6h-6a2 2 0 0 0 -2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-6"/><path d="M11 13l9 -9"/><path d="M15 4h5v5"/>',
  'building':'<path d="M3 21l18 0"/><path d="M9 8l1 0"/><path d="M9 12l1 0"/><path d="M9 16l1 0"/><path d="M14 8l1 0"/><path d="M14 12l1 0"/><path d="M14 16l1 0"/><path d="M5 21v-16a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v16"/>',
  'arrow-right':'<path d="M5 12l14 0"/><path d="M13 18l6 -6"/><path d="M13 6l6 6"/>',
  'arrow-up':'<path d="M12 5l0 14"/><path d="M18 11l-6 -6"/><path d="M6 11l6 -6"/>',
  'chevron-left':'<path d="M15 6l-6 6l6 6"/>',
  'check':'<path d="M5 12l5 5l10 -10"/>',
  'news':'<path d="M16 6h3a1 1 0 0 1 1 1v11a2 2 0 0 1 -4 0v-13a1 1 0 0 0 -1 -1h-10a1 1 0 0 0 -1 1v12a3 3 0 0 0 3 3h11"/><path d="M8 8l4 0"/><path d="M8 12l4 0"/><path d="M8 16l4 0"/>',
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: VEI_ICONS[name] || '' }} />
  );
}

// ── Données (articles éditoriaux) ────────────────────────────────────────────
const ARTICLES = [
  { id:1, source:'Les Échos', date:'il y a 2 h', dedup:3, unread:true, read:false, fav:false,
    title:'Réforme de la facturation électronique : le calendrier 2026 précisé',
    summary:"L'administration publie les modalités d'application pour les PME et précise les obligations de transmission.",
    mentions:'Ateliers Beaumont' },
  { id:2, source:'Légifrance', date:'il y a 5 h', dedup:0, unread:true, read:false, fav:false,
    title:'Décret relatif aux seuils de présentation des comptes',
    summary:"Le texte relève les seuils en deçà desquels une présentation simplifiée des comptes annuels est admise." },
  { id:3, source:'INSEE', date:'il y a 8 h', dedup:0, unread:true, read:false, fav:true,
    title:'Défaillances d’entreprises : note de conjoncture du 1ᵉʳ trimestre',
    summary:"Le nombre d'ouvertures de procédures collectives se stabilise après deux trimestres de hausse." },
  { id:4, source:'Banque de France', date:'hier', dedup:0, unread:false, read:true, fav:false,
    title:'Statistiques de crédit aux entreprises — T1 2026',
    summary:"L'encours de crédit mobilisé progresse légèrement sur le trimestre, porté par les TPE." },
];

// ── Bouton d'action discret (≥ 44 px) ────────────────────────────────────────
function ActionBtn({ t, icon, label, on, accent, onClick }) {
  return (
    <button aria-label={label} aria-pressed={on} onClick={onClick} className="atlas-row" style={{
      width:44, height:44, display:'flex', alignItems:'center', justifyContent:'center',
      background:'transparent', border:'none', cursor:'pointer', borderRadius:10, marginLeft:-6,
    }}>
      <Icon name={icon} size={19} color={on ? (accent || t.primary) : t.outline} />
    </button>
  );
}

// ── Carte d'article ──────────────────────────────────────────────────────────
function ArticleCard({ t, a, last, onOpen, onToggle }) {
  const dim = a.read;
  return (
    <article style={{ padding:'14px 16px', position:'relative', opacity: dim ? 0.62 : 1 }}>
      <div style={{ display:'flex', alignItems:'center', gap:8, marginBottom:5 }}>
        <span style={{ width:8, flexShrink:0, display:'flex' }}>
          {a.unread && <span style={{ width:8, height:8, borderRadius:'50%', background:t.info }} />}
        </span>
        <span style={{ fontSize:11.5, color:t.outline }}>{a.source} · {a.date}</span>
        {a.dedup > 0 && (
          <span style={{ fontSize:10.5, color:t.onSurfaceVariant, background:t.surfaceVariant,
            padding:'1px 7px', borderRadius:6, fontWeight:500 }}>{a.dedup} sources</span>
        )}
      </div>
      <button onClick={() => onOpen(a)} className="atlas-row" style={{
        display:'block', width:'100%', textAlign:'left', background:'transparent', border:'none',
        cursor:'pointer', padding:0, marginLeft:16 }}>
        <p style={{ margin:0, fontSize:15, lineHeight:1.32, color:t.onSurface,
          fontWeight: a.unread ? 700 : 500, fontFamily:ESANS }}>{a.title}</p>
        {a.summary && (
          <p style={{ margin:'4px 0 0', fontSize:12.5, lineHeight:1.45, color:t.onSurfaceVariant }}>{a.summary}</p>
        )}
      </button>
      <div style={{ display:'flex', gap:14, marginTop:6, marginLeft:10 }}>
        <ActionBtn t={t} icon={a.read ? 'circle-check' : 'circle'} label={a.read ? 'Marquer non lu' : 'Marquer lu'}
          on={a.read} accent={t.onSurfaceVariant} onClick={() => onToggle(a.id, 'read')} />
        <ActionBtn t={t} icon="star" label="Favori" on={a.fav} onClick={() => onToggle(a.id, 'fav')} />
        <ActionBtn t={t} icon="archive" label="Archiver" on={false} onClick={() => onToggle(a.id, 'arch')} />
      </div>
      {!last && <div style={{ position:'absolute', left:16, right:0, bottom:0, height:1, background:t.hairline }} />}
    </article>
  );
}

// ── Case à cocher (rangée ≥ 44 px) ───────────────────────────────────────────
function CheckRow({ t, label, count, checked, last, onChange }) {
  return (
    <button role="checkbox" aria-checked={checked} onClick={onChange} className="atlas-row" style={{
      width:'100%', minHeight:48, display:'flex', alignItems:'center', gap:12, padding:'0 2px',
      background:'transparent', border:'none', cursor:'pointer', textAlign:'left',
      borderBottom: last ? 'none' : `1px solid ${t.hairline}`,
    }}>
      <span aria-hidden="true" style={{
        width:20, height:20, flexShrink:0, borderRadius:6, display:'flex', alignItems:'center', justifyContent:'center',
        background: checked ? t.primary : 'transparent',
        border:`1.5px solid ${checked ? t.primary : t.outline}`,
      }}>
        {checked && <Icon name="check" size={13} color={t.onPrimary} stroke={2.6} />}
      </span>
      <span style={{ flex:1, fontSize:14, color:t.onSurface, fontWeight: checked ? 500 : 400 }}>{label}</span>
      <span style={{ fontSize:12, color:t.outline, fontVariantNumeric:'tabular-nums' }}>{count}</span>
    </button>
  );
}

// ── Panneau de filtres (bottom sheet) ────────────────────────────────────────
function FilterSheet({ t, onClose }) {
  const [packs, setPacks] = useState({ ec:true, vc:false });
  const [src, setSrc] = useState({ echos:true, legi:true, insee:false });
  const tog = (set, key) => set(s => ({ ...s, [key]: !s[key] }));
  return (
    <React.Fragment>
      <div onClick={onClose} style={{ position:'absolute', inset:0, background:t.scrim, zIndex:20 }} />
      <div role="dialog" aria-label="Filtrer la veille" style={{
        position:'absolute', left:0, right:0, bottom:0, zIndex:21, maxHeight:'82%',
        background:t.background, borderRadius:'22px 22px 0 0', boxShadow:t.sheetShadow,
        display:'flex', flexDirection:'column', overflow:'hidden',
      }}>
        <div style={{ display:'flex', alignItems:'center', justifyContent:'space-between',
          padding:'18px 18px 12px', flexShrink:0 }}>
          <h2 style={{ margin:0, fontFamily:ESERIF, fontWeight:600, fontSize:20, color:t.onSurface }}>Filtrer</h2>
          <button aria-label="Fermer" onClick={onClose} className="atlas-row" style={{
            width:44, height:44, borderRadius:11, cursor:'pointer', border:'none', background:t.surfaceVariant,
            display:'flex', alignItems:'center', justifyContent:'center' }}>
            <Icon name="x" size={18} color={t.onSurfaceVariant} />
          </button>
        </div>
        <div style={{ overflow:'auto', padding:'0 18px 28px' }}>
          <SheetSection t={t} title="Packs métier">
            <CheckRow t={t} label="Expert-comptable" count={23} checked={packs.ec} onChange={() => tog(setPacks,'ec')} />
            <CheckRow t={t} label="Veille concurrentielle" count={11} checked={packs.vc} last onChange={() => tog(setPacks,'vc')} />
          </SheetSection>
          <SheetSection t={t} title="Sources libres">
            <CheckRow t={t} label="Les Échos — Entreprises" count={8} checked={src.echos} onChange={() => tog(setSrc,'echos')} />
            <CheckRow t={t} label="Légifrance" count={4} checked={src.legi} onChange={() => tog(setSrc,'legi')} />
            <CheckRow t={t} label="INSEE" count={6} checked={src.insee} onChange={() => tog(setSrc,'insee')} />
            <button className="atlas-row" style={{
              width:'100%', minHeight:48, display:'flex', alignItems:'center', gap:10, padding:'0 2px',
              background:'transparent', border:'none', cursor:'pointer', color:t.primary,
              fontFamily:ESANS, fontSize:14, fontWeight:600, marginTop:4 }}>
              <Icon name="plus" size={17} color={t.primary} />
              Ajouter un flux RSS
            </button>
          </SheetSection>
        </div>
      </div>
    </React.Fragment>
  );
}
function SheetSection({ t, title, children }) {
  return (
    <div style={{ marginTop:14, background:t.surface, border:`1px solid ${t.hairline}`,
      borderRadius:14, padding:'4px 14px 6px' }}>
      <h3 style={{ margin:'10px 2px 2px', fontSize:11, fontWeight:600, letterSpacing:'0.06em',
        textTransform:'uppercase', color:t.outline }}>{title}</h3>
      {children}
    </div>
  );
}

// ── Vue lecture d'un item ────────────────────────────────────────────────────
function ItemReading({ t, a, onBack }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:ESANS }}>
      <div style={{ flexShrink:0, background:t.background, padding:'50px 8px 6px',
        borderBottom:`1px solid ${t.hairline}` }}>
        <button onClick={onBack} className="atlas-row" style={{
          display:'inline-flex', alignItems:'center', gap:3, minHeight:44, padding:'0 8px',
          background:'transparent', border:'none', cursor:'pointer', color:t.onSurfaceVariant, fontSize:14 }}>
          <Icon name="chevron-left" size={19} color={t.onSurfaceVariant} />
          <span>Veille</span>
        </button>
      </div>
      <div style={{ flex:1, overflow:'auto', padding:'18px 20px 28px' }}>
        <div style={{ display:'flex', alignItems:'center', gap:8, fontSize:12, color:t.outline }}>
          <span>{a.source} · {a.date}</span>
          {a.dedup > 0 && <span style={{ fontSize:10.5, color:t.onSurfaceVariant, background:t.surfaceVariant,
            padding:'1px 7px', borderRadius:6, fontWeight:500 }}>{a.dedup} sources</span>}
        </div>
        <h1 style={{ margin:'10px 0 0', fontFamily:ESERIF, fontWeight:600, fontSize:23,
          lineHeight:1.22, color:t.onSurface, letterSpacing:'-0.01em' }}>{a.title}</h1>

        <div style={{ marginTop:16, fontSize:14.5, lineHeight:1.62, color:t.onSurface }}>
          <p style={{ margin:'0 0 12px' }}>{a.summary}</p>
          <p style={{ margin:'0 0 12px', color:t.onSurfaceVariant }}>
            Le calendrier confirme une entrée en vigueur progressive : réception obligatoire pour
            toutes les entreprises à la première échéance, puis émission échelonnée selon la taille.
            Les modalités techniques de transmission via les plateformes agréées sont précisées.</p>
          <p style={{ margin:0, color:t.onSurfaceVariant }}>
            Les organisations professionnelles saluent la clarification du périmètre tout en
            demandant un accompagnement renforcé pour les plus petites structures.</p>
        </div>

        <a href="#" onClick={e => e.preventDefault()} className="atlas-row" style={{
          marginTop:20, width:'100%', minHeight:50, display:'flex', alignItems:'center', justifyContent:'center',
          gap:9, background:t.primary, color:t.onPrimary, borderRadius:13, textDecoration:'none',
          fontFamily:ESANS, fontSize:15, fontWeight:600 }}>
          <span>Lire sur {a.source}</span>
          <Icon name="external-link" size={18} color={t.onPrimary} />
        </a>
        <p style={{ margin:'8px 2px 0', fontSize:11.5, color:t.outline, textAlign:'center' }}>
          Ouvre le site de l'éditeur dans le navigateur</p>

        {/* Encart de mention — présenté comme une SUGGESTION à vérifier */}
        {a.mentions && (
          <div style={{ marginTop:22, background:t.surface, border:`1px solid ${t.hairline}`,
            borderRadius:14, padding:'14px 15px' }}>
            <p style={{ margin:0, fontSize:10.5, fontWeight:600, letterSpacing:'0.06em',
              textTransform:'uppercase', color:t.outline }}>Mention détectée · à vérifier</p>
            <div style={{ display:'flex', alignItems:'flex-start', gap:11, marginTop:9 }}>
              <span style={{ width:36, height:36, flexShrink:0, borderRadius:9, background:t.infoContainer,
                display:'flex', alignItems:'center', justifyContent:'center' }}>
                <Icon name="building" size={18} color={t.info} />
              </span>
              <div style={{ flex:1, minWidth:0 }}>
                <p style={{ margin:0, fontSize:13.5, lineHeight:1.45, color:t.onSurface }}>
                  Cet article <strong style={{ fontWeight:600 }}>semble mentionner</strong> {a.mentions}.
                  Atlas n'a pas confirmé le rapprochement.</p>
                <button className="atlas-row" style={{
                  marginTop:9, minHeight:44, display:'inline-flex', alignItems:'center', gap:6,
                  background:'transparent', border:'none', padding:0, cursor:'pointer',
                  color:t.primary, fontFamily:ESANS, fontSize:13.5, fontWeight:600 }}>
                  Voir la fiche {a.mentions}
                  <Icon name="arrow-right" size={16} color={t.primary} />
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
      <TabBar t={t} active={2} />
    </div>
  );
}

// ── Barre d'onglets ──────────────────────────────────────────────────────────
const VEI_TABS = [['Accueil','news'], ['Recherche','search'], ['Veille','rss'], ['Favoris','star'], ['Profil','user']];
function TabBar({ t, active = 2 }) {
  return (
    <div style={{ flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px' }}>
      {VEI_TABS.map(([label, icon], i) => {
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

// ── Écran Veille ─────────────────────────────────────────────────────────────
// view: 'feed' | 'reading' ; panelOpen: bool
function VeilleScreen({ dark = false, view: initView = 'feed', panelOpen: initPanel = false }) {
  const t = TVE(dark);
  const [arts, setArts] = useState(ARTICLES);
  const [view, setView] = useState(initView);
  const [openId, setOpenId] = useState(initView === 'reading' ? 1 : null);
  const [panel, setPanel] = useState(initPanel);
  const [banner, setBanner] = useState(true);

  const onToggle = (id, key) => setArts(prev => prev.map(a => {
    if (a.id !== id) return a;
    if (key === 'read') return { ...a, read:!a.read, unread:a.read };
    if (key === 'fav') return { ...a, fav:!a.fav };
    return a;
  }));
  const open = (a) => { setOpenId(a.id); setView('reading'); };

  if (view === 'reading') {
    const a = arts.find(x => x.id === openId) || arts[0];
    return <ItemReading t={t} a={a} onBack={() => setView('feed')} />;
  }

  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', position:'relative',
      background:t.background, color:t.onSurface, fontFamily:ESANS, WebkitFontSmoothing:'antialiased', overflow:'hidden' }}>
      <div style={{ flexShrink:0, background:t.background, padding:'50px 16px 10px' }}>
        <h1 style={{ margin:0, fontFamily:ESERIF, fontWeight:600, fontSize:26, lineHeight:1.05,
          color:t.onSurface, letterSpacing:'-0.01em' }}>Veille</h1>
        <div style={{ display:'flex', alignItems:'center', justifyContent:'space-between', gap:10, marginTop:12 }}>
          {/* Filtre actif — visible même panneau replié */}
          <button onClick={() => setPanel(true)} className="atlas-row" style={{
            display:'inline-flex', alignItems:'center', gap:7, minHeight:44, padding:'0 13px',
            borderRadius:999, cursor:'pointer', background:t.infoContainer, border:'none',
            color:t.info, fontFamily:ESANS, fontSize:13, fontWeight:600 }}>
            <Icon name="filter" size={15} color={t.info} />
            <span>Pack Compta · 23</span>
          </button>
          <button aria-label="Ouvrir les filtres" onClick={() => setPanel(true)} className="atlas-row" style={{
            width:44, height:44, flexShrink:0, borderRadius:12, cursor:'pointer',
            background:t.surface, border:`1px solid ${t.hairline}`, boxShadow:t.shadow,
            display:'flex', alignItems:'center', justifyContent:'center' }}>
            <Icon name="adjustments" size={20} color={t.onSurfaceVariant} />
          </button>
        </div>
      </div>

      <div style={{ flex:1, overflow:'auto' }}>
        {banner && (
          <button onClick={() => setBanner(false)} className="atlas-row" style={{
            margin:'8px 16px 4px', width:'calc(100% - 32px)', minHeight:42, cursor:'pointer',
            display:'flex', alignItems:'center', justifyContent:'center', gap:7, borderRadius:999,
            background:t.surface, border:`1px solid ${t.info}`, color:t.info,
            fontFamily:ESANS, fontSize:13, fontWeight:600 }}>
            <Icon name="arrow-up" size={15} color={t.info} />
            <span>4 nouveaux items · afficher</span>
          </button>
        )}
        {arts.map((a, i) => (
          <ArticleCard key={a.id} t={t} a={a} last={i === arts.length - 1} onOpen={open} onToggle={onToggle} />
        ))}
      </div>

      <TabBar t={t} active={2} />
      {panel && <FilterSheet t={t} onClose={() => setPanel(false)} />}
    </div>
  );
}

Object.assign(window, { VeilleScreen });
