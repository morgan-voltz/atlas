/* atlas-web-screens.jsx — Atlas · écran Recherche (client web Blazor)
   Patron 1 « list-detail » (doc 14 §3) : rail latéral + liste pleine largeur de sa
   zone (scan dense) + détail à largeur PLAFONNÉE (~720px, aligné à gauche, jamais
   étiré). Cliquer une ligne met à jour la fiche SANS changer de page ; la ligne
   sélectionnée reste mise en avant. Clavier : ↑/↓ dans la liste, Entrée pour ouvrir,
   focus visible. Sous le point de rupture (~860px) → empilement (R2) : rail → barre
   d’onglets en bas, liste seule → fiche plein écran avec retour. Même code, deux densités. */

const {
  TW, WSANS, WSERIF, WMONO, Icon,
  ENTREPRISES, MARQUES, COUNTS, FICHES,
  NeutralBadge, Provenance, DeepLink, Fields, Section,
  CoverageConfidential, ProcedureEvent,
} = window;

// ── Rail de navigation latéral (desktop) ─────────────────────────────────────
const NAV = [
  ['accueil','Accueil','home'], ['veille','Veille','rss'], ['recherche','Recherche','search'],
  ['favoris','Favoris','star'], ['profil','Profil','user'],
];
function Logo({ t }) {
  return (
    <div style={{ display:'flex', flexDirection:'column', alignItems:'center', gap:6, padding:'18px 0 14px' }}>
      <div aria-label="Atlas" style={{
        width:42, height:42, borderRadius:13, background:t.primary,
        display:'flex', alignItems:'center', justifyContent:'center',
        boxShadow:t.shadow, fontFamily:WSERIF, fontWeight:600, fontSize:24, color:t.onPrimary }}>A</div>
      <span style={{ fontSize:9.5, fontWeight:700, letterSpacing:'0.18em',
        color:t.onSurfaceVariant, textTransform:'uppercase' }}>Atlas</span>
    </div>
  );
}
function RailNav({ t, active = 'recherche' }) {
  return (
    <nav aria-label="Navigation principale" style={{
      width:76, flexShrink:0, background:t.railBg, borderRight:`1px solid ${t.hairline}`,
      display:'flex', flexDirection:'column', alignItems:'stretch' }}>
      <Logo t={t} />
      <div style={{ display:'flex', flexDirection:'column', gap:4, padding:'4px 8px' }}>
        {NAV.map(([key, label, icon]) => {
          const on = key === active; const col = on ? t.primary : t.onSurfaceVariant;
          return (
            <button key={key} aria-current={on ? 'page' : undefined} className="atlas-row"
              style={{
                position:'relative', minHeight:58, border:'none', cursor:'pointer',
                display:'flex', flexDirection:'column', alignItems:'center', justifyContent:'center', gap:5,
                borderRadius:13, padding:'8px 2px', fontFamily:WSANS,
                background: on ? t.primaryContainer : 'transparent', color:col }}>
              {on && <span style={{ position:'absolute', left:-8, top:'50%', transform:'translateY(-50%)',
                width:3, height:26, borderRadius:3, background:t.primary }} />}
              <Icon name={icon} size={22} color={col} stroke={on ? 2.2 : 1.9}
                style={{ fill: (on && key==='favoris') ? col : 'none' }} />
              <span style={{ fontSize:11, fontWeight: on ? 700 : 500, color:col, lineHeight:1 }}>{label}</span>
            </button>
          );
        })}
      </div>
    </nav>
  );
}

// ── Barre d’onglets (fenêtre réduite) ────────────────────────────────────────
function BottomTabBar({ t, active = 'recherche' }) {
  return (
    <nav aria-label="Navigation principale" style={{
      flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'7px 4px 10px' }}>
      {NAV.map(([key, label, icon]) => {
        const on = key === active; const col = on ? t.primary : t.outline;
        return (
          <button key={key} aria-current={on ? 'page' : undefined} className="atlas-row"
            style={{ flex:1, minHeight:52, background:'transparent', border:'none', cursor:'pointer',
              display:'flex', flexDirection:'column', alignItems:'center', gap:3,
              position:'relative', paddingTop:7, fontFamily:WSANS }}>
            {on && <span style={{ position:'absolute', top:0, width:22, height:2, borderRadius:2, background:t.primary }} />}
            <Icon name={icon} size={22} color={col} stroke={on ? 2.2 : 1.9}
              style={{ fill: (on && key==='favoris') ? col : 'none' }} />
            <span style={{ fontSize:11, fontWeight:on ? 700 : 500, color:col }}>{label}</span>
          </button>
        );
      })}
    </nav>
  );
}

// ── Champ de recherche ───────────────────────────────────────────────────────
function SearchField({ t, value, onChange, mode, showShortcut }) {
  const ref = useRef(null);
  const verify = mode === 'verify';
  return (
    <div className="atlas-field" style={{
      display:'flex', alignItems:'center', gap:10, minHeight:50,
      background:t.surface, border:`1px solid ${t.hairlineStrong}`, borderRadius:13,
      padding:'0 12px', boxShadow:t.shadow }}>
      <Icon name="search" size={19} color={t.outline} />
      <input ref={ref} value={value} onChange={e => onChange(e.target.value)}
        inputMode="search" enterKeyHint="search"
        aria-label={verify ? 'Vérifier la disponibilité d’un nom' : 'Rechercher une entreprise, un SIREN ou une marque'}
        placeholder={verify ? 'Nom ou dénomination à vérifier…' : 'Nom, SIREN ou marque…'}
        style={{ flex:1, minWidth:0, border:'none', outline:'none', background:'transparent',
          fontFamily:WSANS, fontSize:15, color:t.onSurface, padding:'13px 0' }} />
      {value ? (
        <button aria-label="Effacer" onClick={() => { onChange(''); ref.current && ref.current.focus(); }}
          className="atlas-row" style={{ width:30, height:30, flexShrink:0, borderRadius:8, cursor:'pointer',
            border:'none', background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center' }}>
          <Icon name="x" size={15} color={t.onSurfaceVariant} />
        </button>
      ) : showShortcut ? (
        <kbd style={{ flexShrink:0, display:'inline-flex', alignItems:'center', gap:3, height:24, padding:'0 8px',
          borderRadius:7, border:`1px solid ${t.hairline}`, background:t.surfaceVariant,
          fontFamily:WMONO, fontSize:12, fontWeight:600, color:t.onSurfaceVariant }}>
          <Icon name="command" size={12} color={t.onSurfaceVariant} stroke={2} />K</kbd>
      ) : null}
    </div>
  );
}

// ── Sélecteur de mode « Rechercher | Vérifier un nom » ───────────────────────
function ModeSelector({ t, mode, onChange }) {
  const opts = [['search','Rechercher'], ['verify','Vérifier un nom']];
  return (
    <div role="radiogroup" aria-label="Mode" style={{
      display:'flex', gap:4, padding:4, marginTop:10, background:t.surfaceVariant,
      borderRadius:12, border:`1px solid ${t.hairline}` }}>
      {opts.map(([key, label]) => {
        const on = key === mode;
        return (
          <button key={key} role="radio" aria-checked={on} onClick={() => onChange(key)} className="atlas-row"
            style={{ flex:1, minHeight:40, borderRadius:9, cursor:'pointer', fontFamily:WSANS, fontSize:13.5,
              fontWeight: on ? 700 : 500, border:'none',
              background: on ? t.surface : 'transparent',
              color: on ? t.onSurface : t.onSurfaceVariant,
              boxShadow: on ? t.shadow : 'none' }}>{label}</button>
        );
      })}
    </div>
  );
}

// ── Segments « Entreprises · 6 | Marques · 2 » ───────────────────────────────
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
    <div role="tablist" aria-label="Type de résultat" style={{ display:'flex', gap:8, padding:'12px 0 4px' }}>
      {tabs.map(([key, label, n], i) => {
        const on = key === active;
        return (
          <button key={key} ref={el => refs.current[i] = el} role="tab" aria-selected={on}
            tabIndex={on ? 0 : -1} onClick={() => onChange(key)} onKeyDown={e => onKey(e, i)}
            className="atlas-row" style={{
              minHeight:44, padding:'0 14px', borderRadius:11, cursor:'pointer',
              display:'inline-flex', alignItems:'center', gap:7, fontFamily:WSANS, fontSize:14,
              background: on ? t.primaryContainer : 'transparent',
              border:`1px solid ${on ? t.primaryContainer : t.hairline}`,
              color: on ? t.onPrimaryContainer : t.onSurfaceVariant, fontWeight: on ? 700 : 500 }}>
            <span>{label}</span>
            <span style={{ fontFamily:WMONO, fontSize:13, fontWeight:600, color: on ? t.primary : t.outline }}>{n}</span>
          </button>
        );
      })}
    </div>
  );
}

// ── Carte-aperçu de résultat ─────────────────────────────────────────────────
function ResultRow({ t, item, selected, onSelect, refEl }) {
  return (
    <div ref={refEl} role="option" aria-selected={selected} tabIndex={0}
      onClick={onSelect}
      onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onSelect(); } }}
      className="atlas-row" style={{
        display:'flex', alignItems:'center', gap:12, minHeight:62,
        padding:'11px 14px 11px', cursor:'pointer', position:'relative', borderRadius:12,
        background: selected ? t.primaryContainer : 'transparent',
        boxShadow: selected ? `inset 3px 0 0 ${t.primary}` : 'none',
        transition:'background .12s' }}>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:15, fontWeight:600, lineHeight:1.25,
          color: selected ? t.onPrimaryContainer : t.onSurface,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{item.name}</p>
        {item.restricted ? (
          <p style={{ margin:'3px 0 0', fontSize:13, lineHeight:1.3, color:t.outline, fontStyle:'italic' }}>
            Diffusion restreinte (INSEE)</p>
        ) : (
          <p style={{ margin:'3px 0 0', fontSize:13, lineHeight:1.3,
            color: selected ? t.onPrimaryContainer : t.onSurfaceVariant,
            whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>
            <span style={{ fontFamily:WMONO }}>{item.siren}</span>
            <span> · {item.form} · {item.ville}</span>
          </p>
        )}
      </div>
      {item.restricted ? (
        <span style={{ flexShrink:0, fontSize:12, fontWeight:600, padding:'2px 9px', borderRadius:7,
          background:t.surfaceVariant, color:t.onSurfaceVariant }}>restreint</span>
      ) : null}
      <Icon name="chevron-right" size={18} color={selected ? t.primary : t.outline} />
    </div>
  );
}

// ── Panneau LISTE ────────────────────────────────────────────────────────────
function ListPanel({ t, q, setQ, mode, setMode, seg, setSeg, selectedId, onSelect,
  showTitle, showShortcut, fullWidthZone }) {
  const list = seg === 'entreprises' ? ENTREPRISES : MARQUES;
  const rowRefs = useRef({});
  const onListKey = (e) => {
    if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
    e.preventDefault();
    const idx = list.findIndex(x => x.id === selectedId);
    const next = e.key === 'ArrowDown'
      ? Math.min(list.length - 1, idx + 1)
      : Math.max(0, idx < 0 ? 0 : idx - 1);
    const item = list[next];
    if (item) { onSelect(item.id); const el = rowRefs.current[item.id]; el && el.focus(); }
  };
  return (
    <div style={{ display:'flex', flexDirection:'column', height:'100%',
      background:t.background, minWidth:0 }}>
      <div style={{ flexShrink:0, padding:'22px 20px 10px', borderBottom:`1px solid ${t.hairline}` }}>
        {showTitle && (
          <h1 style={{ margin:'0 0 14px', fontFamily:WSERIF, fontWeight:600, fontSize:26,
            lineHeight:1.05, color:t.onSurface, letterSpacing:'-0.01em' }}>Recherche</h1>
        )}
        <SearchField t={t} value={q} onChange={setQ} mode={mode} showShortcut={showShortcut} />
        <ModeSelector t={t} mode={mode} onChange={setMode} />
        {mode === 'search'
          ? <Segments t={t} active={seg} onChange={setSeg} />
          : <p style={{ margin:'12px 2px 2px', fontSize:13, lineHeight:1.5, color:t.onSurfaceVariant }}>
              Saisis un nom : Atlas affiche les dénominations et marques proches déjà enregistrées (descriptif — aucun verdict de disponibilité).</p>}
      </div>
      <div role="listbox" aria-label="Résultats" tabIndex={-1} onKeyDown={onListKey}
        style={{ flex:1, overflow:'auto', padding:'8px 6px 16px' }}>
        <p style={{ margin:'6px 12px 4px', fontSize:12.5, color:t.outline }}>
          {list.length} résultat{list.length > 1 ? 's' : ''} · « {q || 'beaumont'} »</p>
        {list.map(item => (
          <ResultRow key={item.id} t={t} item={item}
            selected={item.id === selectedId} onSelect={() => onSelect(item.id)}
            refEl={el => rowRefs.current[item.id] = el} />
        ))}
      </div>
    </div>
  );
}

// ── Masquage par pertinence (R6) ─────────────────────────────────────────────
function MaskedSections({ t }) {
  const [open, setOpen] = useState(false);
  if (!open) {
    return (
      <button onClick={() => setOpen(true)} className="atlas-row"
        style={{ width:'100%', minHeight:52, display:'flex', alignItems:'center', justifyContent:'center', gap:9,
          background:'transparent', border:`1px dashed ${t.outline}`, borderRadius:14, cursor:'pointer',
          color:t.onSurfaceVariant, fontFamily:WSANS, fontSize:14, fontWeight:600, padding:'12px 16px' }}>
        <Icon name="eye" size={16} color={t.onSurfaceVariant} />
        <span>+3 sections masquées — Établissements, PI, Marchés publics</span>
      </button>
    );
  }
  return (
    <div>
      <p style={{ margin:'4px 2px 10px', fontSize:11.5, fontWeight:700, letterSpacing:'0.07em',
        textTransform:'uppercase', color:t.onSurfaceVariant }}>Sections masquées par pertinence</p>
      <Section t={t} title="Établissements" provenance={{ source:'RNE', date:'28 mai 2026' }}>
        <Fields t={t} rows={[['Siège','Lyon 3ᵉ'], ['Établissements actifs','2'], ['Secondaires','Villeurbanne']]} />
      </Section>
      <Section t={t} title="Propriété intellectuelle" provenance={{ source:'INPI', date:'28 mai 2026' }}
        deepLink="Voir le portefeuille de marques">
        <Fields t={t} rows={[['Marques déposées','1'], ['Brevets','0'], ['Dessins & modèles','0']]} />
      </Section>
      <Section t={t} title="Marchés publics" last provenance={{ source:'DECP · data.gouv', date:'28 mai 2026' }}>
        <div style={{ display:'flex', gap:10, alignItems:'flex-start' }}>
          <Icon name="building-store" size={20} color={t.outline} style={{ marginTop:1 }} />
          <p style={{ margin:0, fontSize:13.5, lineHeight:1.5, color:t.onSurfaceVariant }}>
            Aucune attribution de marché public recensée sur les 24 derniers mois.</p>
        </div>
      </Section>
      <button onClick={() => setOpen(false)} className="atlas-row"
        style={{ marginTop:12, minHeight:44, display:'inline-flex', alignItems:'center', gap:7,
          background:'transparent', border:'none', cursor:'pointer', padding:'0 4px',
          color:t.primary, fontFamily:WSANS, fontSize:13.5, fontWeight:700 }}>
        Masquer à nouveau
      </button>
    </div>
  );
}

// ── En-tête d’identité de la fiche ───────────────────────────────────────────
function IdentityHeader({ t, fiche, onBack }) {
  const [followed, setFollowed] = useState(false);
  return (
    <div style={{ marginBottom:18 }}>
      {onBack && (
        <button onClick={onBack} className="atlas-row" style={{ display:'inline-flex', alignItems:'center', gap:5,
          minHeight:44, padding:'0 8px 0 0', background:'transparent', border:'none', cursor:'pointer',
          color:t.onSurfaceVariant, fontFamily:WSANS, fontSize:14, fontWeight:600, marginBottom:4 }}>
          <Icon name="chevron-left" size={19} color={t.onSurfaceVariant} />
          <span>Résultats</span>
        </button>
      )}
      <div style={{ display:'flex', alignItems:'flex-start', justifyContent:'space-between', gap:16 }}>
        <div style={{ flex:1, minWidth:0 }}>
          <h2 style={{ margin:0, fontFamily:WSERIF, fontWeight:600, fontSize:28, lineHeight:1.12,
            color:t.onSurface, letterSpacing:'-0.01em' }}>{fiche.name}</h2>
          <p style={{ margin:'8px 0 0', fontSize:14.5, color:t.onSurfaceVariant, lineHeight:1.45,
            display:'flex', alignItems:'center', gap:8, flexWrap:'wrap' }}>
            {fiche.restricted
              ? <span style={{ fontStyle:'italic', color:t.outline }}>Diffusion restreinte (INSEE)</span>
              : <span style={{ fontFamily:WMONO }}>{fiche.siren || fiche.depot}</span>}
            <span aria-hidden="true">·</span><span>{fiche.form}</span>
            <NeutralBadge t={t}>{fiche.badge}</NeutralBadge>
          </p>
        </div>
        <button aria-pressed={followed} aria-label={followed ? 'Ne plus suivre' : 'Suivre cette entreprise'}
          onClick={() => setFollowed(f => !f)} className="atlas-row"
          style={{ width:46, height:46, flexShrink:0, borderRadius:13, cursor:'pointer',
            background: followed ? t.primaryContainer : t.surface,
            border:`1px solid ${followed ? t.primaryContainer : t.hairline}`, boxShadow:t.shadow,
            display:'flex', alignItems:'center', justifyContent:'center' }}>
          <Icon name="star" size={21} color={followed ? t.primary : t.outline}
            style={{ fill: followed ? t.primary : 'none' }} />
        </button>
      </div>
      <div style={{ display:'flex', alignItems:'center', gap:6, marginTop:12, fontSize:12.5, color:t.outline }}>
        <Icon name="shield-check" size={14} color={t.outline} />
        <span>À jour · {fiche.updated}</span>
      </div>
    </div>
  );
}

// ── Corps de la fiche (sections) ─────────────────────────────────────────────
function FicheSections({ t, fiche }) {
  if (fiche.restricted) {
    return (
      <Section t={t} title="Couverture" defaultOpen provenance={{ source:'INSEE · Sirene', date:fiche.updated }}>
        <div style={{ textAlign:'center', padding:'4px 6px 0' }}>
          <div style={{ display:'flex', justifyContent:'center' }}><Icon name="lock" size={24} color={t.outline} /></div>
          <p style={{ margin:'10px 0 5px', fontSize:15, fontWeight:700, color:t.onSurface, fontFamily:WSANS }}>
            Diffusion restreinte</p>
          <p style={{ margin:0, fontSize:13.5, lineHeight:1.55, color:t.onSurfaceVariant, maxWidth:420,
            marginLeft:'auto', marginRight:'auto' }}>
            Cette entité a demandé la non-diffusion de ses données auprès de l’INSEE (droit ouvert aux personnes physiques et à certaines structures). Son existence est un fait de couverture ; ses informations détaillées ne sont pas publiques.</p>
        </div>
      </Section>
    );
  }
  if (fiche.kind === 'marque') {
    return (
      <div>
        <Section t={t} title="Identité de la marque" defaultOpen defaultPinned
          provenance={{ source:'INPI', date:fiche.updated }}>
          <Fields t={t} rows={fiche.marque} />
        </Section>
        <Section t={t} title="Produits & services" last
          provenance={{ source:'INPI · classification de Nice', date:fiche.updated }}
          deepLink="Voir le détail des classes">
          <div style={{ display:'flex', gap:10, alignItems:'flex-start' }}>
            <Icon name="license" size={20} color={t.outline} style={{ marginTop:1 }} />
            <p style={{ margin:0, fontSize:14, lineHeight:1.55, color:t.onSurfaceVariant }}>
              Protégée pour les classes {fiche.marque.find(r => r[0] === 'Classes')[1]} — produits et services correspondants de la classification de Nice.</p>
          </div>
        </Section>
      </div>
    );
  }
  return (
    <div>
      <Section t={t} title="Bilans & finances" defaultOpen defaultPinned
        provenance={{ source:'INPI (greffe)', date:fiche.updated }}
        deepLink={fiche.finances ? 'Voir les exercices antérieurs' : undefined}>
        {fiche.financesConfidential
          ? <CoverageConfidential t={t} />
          : fiche.finances
            ? <Fields t={t} rows={fiche.finances} />
            : <p style={{ margin:0, fontSize:14, lineHeight:1.55, color:t.onSurfaceVariant }}>
                Aucun compte annuel n’a été déposé pour les exercices récents.</p>}
      </Section>
      {fiche.bodacc && fiche.bodacc.length > 0 && (
        <Section t={t} title="BODACC · procédures" defaultOpen
          provenance={{ source:'BODACC', date:fiche.updated }} deepLink="Voir toutes les annonces">
          <div style={{ display:'flex', flexDirection:'column', gap:14 }}>
            {fiche.bodacc.map((e, i) => <ProcedureEvent key={i} t={t} {...e} />)}
          </div>
        </Section>
      )}
      <Section t={t} title="Identité" defaultOpen provenance={{ source:'RNE', date:fiche.updated }}>
        <Fields t={t} rows={fiche.identity} />
      </Section>
      <Section t={t} title="Dirigeants" defaultOpen provenance={{ source:'RNE', date:fiche.updated }}
        deepLink="Voir le graphe des dirigeants">
        <Fields t={t} rows={fiche.dirigeants} />
      </Section>
      <MaskedSections t={t} />
    </div>
  );
}

// ── Panneau DÉTAIL (texte plafonné ~720px, aligné à gauche) ──────────────────
function DetailPanel({ t, id, onBack, narrow }) {
  const fiche = FICHES[id];
  return (
    <div style={{ height:'100%', overflow:'auto', background:t.background }}>
      <div style={{ maxWidth:720, marginLeft: narrow ? 'auto' : 0, marginRight:'auto',
        padding: narrow ? '20px 20px 28px' : '34px 40px 48px' }}>
        {fiche ? (
          <React.Fragment>
            <IdentityHeader t={t} fiche={fiche} onBack={onBack} />
            <FicheSections t={t} fiche={fiche} />
          </React.Fragment>
        ) : (
          <div style={{ textAlign:'center', padding:'80px 20px', color:t.onSurfaceVariant }}>
            <Icon name="building" size={30} color={t.outline} style={{ margin:'0 auto' }} />
            <p style={{ margin:'14px 0 0', fontSize:15 }}>Sélectionne un résultat pour afficher sa fiche.</p>
          </div>
        )}
      </div>
    </div>
  );
}

// ── Écran Recherche — responsive (desktop list-detail / fenêtre réduite empilée) ──
function RechercheWeb({ dark = false, layout = 'desktop',
  initialQuery = 'beaumont', initialSeg = 'entreprises',
  initialSelected = 'e1', initialView = 'list' }) {
  const t = TW(dark);
  const [q, setQ] = useState(initialQuery);
  const [mode, setMode] = useState('search');
  const [seg, setSegRaw] = useState(initialSeg);
  const [selectedId, setSelectedId] = useState(initialSelected);
  const [view, setView] = useState(initialView); // fenêtre réduite : 'list' | 'detail'

  const setSeg = (s) => {
    setSegRaw(s);
    const first = (s === 'entreprises' ? ENTREPRISES : MARQUES)[0];
    if (first) setSelectedId(first.id);
  };
  const narrow = layout === 'narrow';

  const wrapStyle = {
    height:'100%', display:'flex', background:t.background, color:t.onSurface,
    fontFamily:WSANS, WebkitFontSmoothing:'antialiased', overflow:'hidden',
  };

  // ── Desktop : rail + liste + détail côte à côte ──
  if (!narrow) {
    return (
      <div style={wrapStyle}>
        <RailNav t={t} active="recherche" />
        <div style={{ flex:'0 0 39%', maxWidth:480, minWidth:340, borderRight:`1px solid ${t.hairlineStrong}`,
          display:'flex', flexDirection:'column', minHeight:0 }}>
          <ListPanel t={t} q={q} setQ={setQ} mode={mode} setMode={setMode} seg={seg} setSeg={setSeg}
            selectedId={selectedId} onSelect={setSelectedId} showTitle showShortcut fullWidthZone />
        </div>
        <div style={{ flex:1, minWidth:0 }}>
          <DetailPanel t={t} id={selectedId} narrow={false} />
        </div>
      </div>
    );
  }

  // ── Fenêtre réduite : empilement (liste seule → fiche plein écran + retour) ──
  return (
    <div style={{ ...wrapStyle, flexDirection:'column' }}>
      <div style={{ flex:1, minHeight:0 }}>
        {view === 'list' ? (
          <ListPanel t={t} q={q} setQ={setQ} mode={mode} setMode={setMode} seg={seg} setSeg={setSeg}
            selectedId={selectedId}
            onSelect={(id) => { setSelectedId(id); setView('detail'); }}
            showTitle showShortcut={false} />
        ) : (
          <DetailPanel t={t} id={selectedId} narrow onBack={() => setView('list')} />
        )}
      </div>
      <BottomTabBar t={t} active="recherche" />
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// Accueil (feed) & Veille — composants web responsive (desktop ↔ narrow)
// Mêmes tokens/rail/onglets que RechercheWeb. narrow = format intermédiaire
// (640–880 px) : rail → barre d'onglets, panneaux empilés (doc 14 §3).
// ═══════════════════════════════════════════════════════════════════════════

// Glyphes complémentaires (absents du jeu web-core) — tracés Tabler.
const WX = {
  'arrow-up':'<path d="M12 5l0 14"/><path d="M18 11l-6 -6"/><path d="M6 11l6 -6"/>',
  'adjustments':'<path d="M14 6m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 6l8 0"/><path d="M16 6l4 0"/><path d="M8 12m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 12l2 0"/><path d="M10 12l10 0"/><path d="M17 18m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/><path d="M4 18l11 0"/><path d="M19 18l1 0"/>',
  'filter':'<path d="M4 4h16v2.172a2 2 0 0 1 -.586 1.414l-4.414 4.414v7l-6 2v-8.5l-4.48 -4.928a2 2 0 0 1 -.52 -1.345v-2.227z"/>',
  'archive':'<path d="M3 4m0 2a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v0a2 2 0 0 1 -2 2h-14a2 2 0 0 1 -2 -2z"/><path d="M5 8v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-10"/><path d="M10 12l4 0"/>',
  'circle':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/>',
  'external-link':'<path d="M12 6h-6a2 2 0 0 0 -2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-6"/><path d="M11 13l9 -9"/><path d="M15 4h5v5"/>',
};
function Glyph({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color}
      strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"
      style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: WX[name] || '' }} />
  );
}

// ── Données ──────────────────────────────────────────────────────────────────
const FEED_EVENTS = [
  { id:1, name:'Ateliers Beaumont', form:'SAS', label:'Nouveau dépôt de comptes annuels', source:'BODACC', when:"Aujourd'hui · 09:14", group:"Aujourd'hui", unread:true },
  { id:2, name:'Maïa Conseil', form:'SARL', label:'Changement de gérant', source:'RNE', when:"Aujourd'hui · 08:02", group:"Aujourd'hui", unread:true },
  { id:3, name:'Néo Mobilités', form:'SAS', label:'Augmentation de capital', source:'BODACC', when:"Aujourd'hui · 07:30", group:"Aujourd'hui", unread:true },
  { id:4, name:'Groupe Vidal Logistique', form:'SA', label:'Transfert de siège social', source:'RNE', when:'Hier · 17:48', group:'Hier', unread:false },
  { id:5, name:'SCI du Vieux Port', form:'SCI', label:'Dépôt d’acte — modification statutaire', source:'RNE', when:'Hier · 11:20', group:'Hier', unread:false },
  { id:6, name:'Comptoir Pharmaceutique Lyonnais', form:'SA', label:'Ouverture d’une procédure de sauvegarde', source:'BODACC', when:'Hier · 09:05', group:'Hier', unread:false },
];
const FEED_GROUPS = ["Aujourd'hui",'Hier'];
const VEILLE_ARTICLES = [
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

// ── Atomes communs ────────────────────────────────────────────────────────────
function SourcePill({ t, children }) {
  return (
    <span style={{ flexShrink:0, fontSize:11.5, fontWeight:600, letterSpacing:'0.03em',
      padding:'3px 8px', borderRadius:7, background:t.surfaceVariant, color:t.onSurfaceVariant,
      whiteSpace:'nowrap' }}>{children}</span>
  );
}
function DedupPill({ t, n }) {
  return (
    <span style={{ fontSize:11, color:t.onSurfaceVariant, background:t.surfaceVariant,
      padding:'2px 8px', borderRadius:6, fontWeight:500 }}>{n} sources rapportent</span>
  );
}
function DateHeadW({ t, label }) {
  return (
    <div style={{ fontSize:11.5, fontWeight:700, letterSpacing:'0.07em', textTransform:'uppercase',
      color:t.onSurfaceVariant, padding:'18px 4px 8px' }}>{label}</div>
  );
}
function UpToDateW({ t }) {
  return (
    <div style={{ textAlign:'center', padding:'26px 16px 34px', display:'flex',
      flexDirection:'column', alignItems:'center' }}>
      <Icon name="circle-check" size={20} color={t.success} />
      <p style={{ margin:'7px 0 0', fontSize:13.5, fontWeight:700, color:t.onSurface, fontFamily:WSANS }}>Vous êtes à jour</p>
      <p style={{ margin:'3px 0 0', fontSize:12, color:t.onSurfaceVariant }}>Dernière mise à jour il y a 3 minutes</p>
    </div>
  );
}

// ── Accueil (feed) ─────────────────────────────────────────────────────────────
function FeedEventCard({ t, e }) {
  return (
    <div role="article" tabIndex={0} className="atlas-row" style={{
      display:'flex', alignItems:'center', gap:12, padding:'13px 15px', cursor:'pointer',
      background:t.surface, border:`1px solid ${t.hairline}`,
      borderLeft:`3px solid ${e.unread ? t.primary : t.hairline}`,
      borderRadius:13, marginBottom:8, boxShadow:t.shadow }}>
      {e.unread && <span style={{ width:8, height:8, borderRadius:'50%', background:t.primary, flexShrink:0 }} />}
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:15, lineHeight:1.25, color:t.onSurface, fontWeight: e.unread ? 700 : 600,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>
          {e.name}<span style={{ fontWeight:400, color:t.onSurfaceVariant }}> · {e.form}</span></p>
        <p style={{ margin:'3px 0 0', fontSize:13, lineHeight:1.3, color:t.onSurfaceVariant,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{e.label} · {e.when}</p>
      </div>
      <SourcePill t={t}>{e.source}</SourcePill>
      <Icon name="chevron-right" size={18} color={t.outline} />
    </div>
  );
}
function FeedColumn({ t, banner }) {
  return (
    <div>
      {banner && (
        <button className="atlas-row" style={{ width:'100%', minHeight:44, display:'flex',
          alignItems:'center', justifyContent:'center', gap:8, borderRadius:11, border:'none', cursor:'pointer',
          background:t.primaryContainer, color:t.onPrimaryContainer, fontFamily:WSANS, fontSize:13.5, fontWeight:700 }}>
          <Glyph name="arrow-up" size={16} color={t.onPrimaryContainer} />
          <span>3 nouveaux mouvements · afficher</span>
        </button>
      )}
      {FEED_GROUPS.map(g => (
        <div key={g}>
          <DateHeadW t={t} label={g} />
          {FEED_EVENTS.filter(e => e.group === g).map(e => <FeedEventCard key={e.id} t={t} e={e} />)}
        </div>
      ))}
      <UpToDateW t={t} />
    </div>
  );
}
function AccueilHeader({ t }) {
  return (
    <div style={{ marginBottom:6 }}>
      <h1 style={{ margin:0, fontFamily:WSERIF, fontWeight:600, fontSize:28, lineHeight:1.05,
        color:t.onSurface, letterSpacing:'-0.01em' }}>Accueil</h1>
      <p style={{ margin:'6px 0 0', fontSize:14, color:t.onSurfaceVariant }}>
        Les mouvements de vos entités suivies, du plus récent au plus ancien · 12 suivies</p>
      <div style={{ display:'flex', alignItems:'center', gap:6, marginTop:11, fontSize:12.5, color:t.outline }}>
        <Icon name="shield-check" size={14} color={t.outline} />
        <span>Trié par date · récents d'abord</span>
      </div>
    </div>
  );
}
function AccueilWeb({ dark = false, layout = 'desktop' }) {
  const t = TW(dark);
  const narrow = layout === 'narrow';
  const wrap = { height:'100%', display:'flex', background:t.background, color:t.onSurface,
    fontFamily:WSANS, WebkitFontSmoothing:'antialiased', overflow:'hidden' };
  // Le feed est du contenu de LECTURE → colonne à largeur plafonnée (doc 14 §3, patron « document centré »).
  const column = (
    <div style={{ flex:1, overflow:'auto' }}>
      <div style={{ maxWidth:680, margin:'0 auto', padding: narrow ? '20px 16px 16px' : '30px 32px 28px' }}>
        <AccueilHeader t={t} />
        <FeedColumn t={t} banner />
      </div>
    </div>
  );
  if (!narrow) {
    return <div style={wrap}><RailNav t={t} active="accueil" />{column}</div>;
  }
  return (
    <div style={{ ...wrap, flexDirection:'column' }}>
      {column}
      <BottomTabBar t={t} active="accueil" />
    </div>
  );
}

// ── Veille (list-detail éditorial) ──────────────────────────────────────────────
function VeilleRow({ t, a, selected, onSelect }) {
  return (
    <div role="option" aria-selected={selected} tabIndex={0} onClick={onSelect}
      className="atlas-row" style={{
        padding:'12px 14px', cursor:'pointer', borderRadius:12, marginBottom:2,
        opacity: a.read ? 0.6 : 1,
        background: selected ? t.primaryContainer : 'transparent',
        boxShadow: selected ? `inset 3px 0 0 ${t.primary}` : 'none' }}>
      <div style={{ display:'flex', alignItems:'center', gap:8, marginBottom:4 }}>
        {a.unread && <span style={{ width:7, height:7, borderRadius:'50%', background:t.info, flexShrink:0 }} />}
        <span style={{ fontSize:11.5, color:t.outline }}>{a.source} · {a.date}</span>
        {a.dedup > 0 && <DedupPill t={t} n={a.dedup} />}
      </div>
      <p style={{ margin:0, fontSize:14.5, lineHeight:1.32, fontWeight: a.unread ? 700 : 500,
        color: selected ? t.onPrimaryContainer : t.onSurface }}>{a.title}</p>
    </div>
  );
}
function VeilleList({ t, arts, selectedId, onSelect, showTitle }) {
  return (
    <div style={{ display:'flex', flexDirection:'column', height:'100%', background:t.background, minWidth:0 }}>
      <div style={{ flexShrink:0, padding:'22px 18px 12px', borderBottom:`1px solid ${t.hairline}` }}>
        {showTitle && <h1 style={{ margin:'0 0 12px', fontFamily:WSERIF, fontWeight:600, fontSize:26,
          lineHeight:1.05, color:t.onSurface, letterSpacing:'-0.01em' }}>Veille</h1>}
        <div style={{ display:'flex', alignItems:'center', justifyContent:'space-between', gap:10 }}>
          <span style={{ display:'inline-flex', alignItems:'center', gap:7, minHeight:38, padding:'0 12px',
            borderRadius:999, background:t.infoContainer, color:t.info, fontSize:13, fontWeight:600 }}>
            <Glyph name="filter" size={14} color={t.info} /><span>Pack Compta · 23</span></span>
          <button aria-label="Filtres" className="atlas-row" style={{ width:40, height:40, flexShrink:0,
            borderRadius:11, cursor:'pointer', background:t.surface, border:`1px solid ${t.hairline}`,
            display:'flex', alignItems:'center', justifyContent:'center', boxShadow:t.shadow }}>
            <Glyph name="adjustments" size={19} color={t.onSurfaceVariant} /></button>
        </div>
      </div>
      <div role="listbox" aria-label="Articles" style={{ flex:1, overflow:'auto', padding:'8px 8px 16px' }}>
        {arts.map(a => <VeilleRow key={a.id} t={t} a={a} selected={a.id === selectedId} onSelect={() => onSelect(a.id)} />)}
      </div>
    </div>
  );
}
function VeilleReading({ t, a, narrow, onBack }) {
  return (
    <div style={{ height:'100%', overflow:'auto', background:t.background }}>
      <div style={{ maxWidth:680, marginLeft: narrow ? 'auto' : 0, marginRight:'auto',
        padding: narrow ? '18px 20px 28px' : '30px 40px 44px' }}>
        {narrow && (
          <button onClick={onBack} className="atlas-row" style={{ display:'inline-flex', alignItems:'center', gap:5,
            minHeight:44, padding:'0 8px 0 0', background:'transparent', border:'none', cursor:'pointer',
            color:t.onSurfaceVariant, fontFamily:WSANS, fontSize:14, fontWeight:600, marginBottom:6 }}>
            <Icon name="chevron-left" size={19} color={t.onSurfaceVariant} /><span>Veille</span></button>
        )}
        <div style={{ display:'flex', alignItems:'center', gap:8, fontSize:12.5, color:t.outline }}>
          <span>{a.source} · {a.date}</span>{a.dedup > 0 && <DedupPill t={t} n={a.dedup} />}</div>
        <h2 style={{ margin:'10px 0 0', fontFamily:WSERIF, fontWeight:600, fontSize:25, lineHeight:1.2,
          color:t.onSurface, letterSpacing:'-0.01em' }}>{a.title}</h2>
        <div style={{ marginTop:16, fontSize:15, lineHeight:1.62, color:t.onSurface }}>
          <p style={{ margin:'0 0 12px' }}>{a.summary}</p>
          <p style={{ margin:0, color:t.onSurfaceVariant }}>
            Le calendrier confirme une entrée en vigueur progressive. Les modalités techniques de
            transmission via les plateformes agréées sont précisées ; les organisations professionnelles
            demandent un accompagnement renforcé pour les plus petites structures.</p>
        </div>
        <p style={{ margin:'20px 0 0' }}>
          <a href="#" onClick={e => e.preventDefault()} style={{ display:'inline-flex', alignItems:'center', gap:7,
            color:t.primary, fontWeight:700, fontSize:14.5, textDecoration:'none' }}>
            <span>Lire sur {a.source}</span><Glyph name="external-link" size={16} color={t.primary} /></a>
          <span style={{ marginLeft:9, fontSize:12, color:t.outline }}>(vous quittez Atlas)</span></p>
        {a.mentions && (
          <div style={{ marginTop:22, background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:14, padding:'14px 15px' }}>
            <p style={{ margin:0, fontSize:10.5, fontWeight:700, letterSpacing:'0.06em', textTransform:'uppercase', color:t.outline }}>
              Mention détectée · à vérifier</p>
            <div style={{ display:'flex', alignItems:'flex-start', gap:11, marginTop:9 }}>
              <span style={{ width:36, height:36, flexShrink:0, borderRadius:9, background:t.infoContainer,
                display:'flex', alignItems:'center', justifyContent:'center' }}><Icon name="building" size={18} color={t.info} /></span>
              <div style={{ flex:1, minWidth:0 }}>
                <p style={{ margin:0, fontSize:13.5, lineHeight:1.45, color:t.onSurface }}>
                  Cet article <strong style={{ fontWeight:600 }}>semble mentionner</strong> {a.mentions}. Atlas n'a pas confirmé le rapprochement.</p>
                <button className="atlas-row" style={{ marginTop:9, minHeight:40, display:'inline-flex', alignItems:'center', gap:6,
                  background:'transparent', border:'none', padding:0, cursor:'pointer', color:t.primary, fontFamily:WSANS, fontSize:13.5, fontWeight:700 }}>
                  Voir la fiche {a.mentions}<Icon name="arrow-right" size={16} color={t.primary} /></button>
              </div>
            </div>
          </div>
        )}
        <div style={{ display:'flex', gap:10, marginTop:22, paddingTop:14, borderTop:`1px solid ${t.hairline}` }}>
          <button className="atlas-row" style={{ minHeight:40, padding:'0 12px', borderRadius:9, cursor:'pointer',
            display:'inline-flex', alignItems:'center', gap:6, background:t.surface, border:`1px solid ${t.outline}`,
            color:t.onSurface, fontFamily:WSANS, fontSize:13, fontWeight:600 }}>
            <Icon name="circle-check" size={15} color={t.onSurfaceVariant} />Marquer comme lu</button>
          <button className="atlas-row" style={{ minHeight:40, padding:'0 12px', borderRadius:9, cursor:'pointer',
            display:'inline-flex', alignItems:'center', gap:6, background:t.surface, border:`1px solid ${t.outline}`,
            color:t.onSurface, fontFamily:WSANS, fontSize:13, fontWeight:600 }}>
            <Icon name="star" size={15} color={t.outline} />Favori</button>
          <button className="atlas-row" style={{ minHeight:40, padding:'0 12px', borderRadius:9, cursor:'pointer',
            display:'inline-flex', alignItems:'center', gap:6, background:t.surface, border:`1px solid ${t.outline}`,
            color:t.onSurface, fontFamily:WSANS, fontSize:13, fontWeight:600 }}>
            <Glyph name="archive" size={15} color={t.outline} />Archiver</button>
        </div>
      </div>
    </div>
  );
}
function VeilleWeb({ dark = false, layout = 'desktop', initialSelected = 1, initialView = 'list' }) {
  const t = TW(dark);
  const narrow = layout === 'narrow';
  const [selectedId, setSelectedId] = useState(initialSelected);
  const [view, setView] = useState(initialView);
  const cur = VEILLE_ARTICLES.find(a => a.id === selectedId) || VEILLE_ARTICLES[0];
  const wrap = { height:'100%', display:'flex', background:t.background, color:t.onSurface,
    fontFamily:WSANS, WebkitFontSmoothing:'antialiased', overflow:'hidden' };
  if (!narrow) {
    return (
      <div style={wrap}>
        <RailNav t={t} active="veille" />
        <div style={{ flex:'0 0 40%', maxWidth:460, minWidth:320, borderRight:`1px solid ${t.hairlineStrong}`,
          display:'flex', flexDirection:'column', minHeight:0 }}>
          <VeilleList t={t} arts={VEILLE_ARTICLES} selectedId={selectedId} onSelect={setSelectedId} showTitle />
        </div>
        <div style={{ flex:1, minWidth:0 }}><VeilleReading t={t} a={cur} narrow={false} /></div>
      </div>
    );
  }
  return (
    <div style={{ ...wrap, flexDirection:'column' }}>
      <div style={{ flex:1, minHeight:0 }}>
        {view === 'list'
          ? <VeilleList t={t} arts={VEILLE_ARTICLES} selectedId={selectedId}
              onSelect={(id) => { setSelectedId(id); setView('reading'); }} showTitle />
          : <VeilleReading t={t} a={cur} narrow onBack={() => setView('list')} />}
      </div>
      <BottomTabBar t={t} active="veille" />
    </div>
  );
}

Object.assign(window, { RechercheWeb, AccueilWeb, VeilleWeb });
