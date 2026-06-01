/* atlas-web-core.jsx — Atlas · client web (Blazor), socle partagé
   Tokens, icônes, données simulées, fiches, atomes et carte-section repliable.
   Doctrine (doc 12 / doc 14) : descriptif (aucun verdict/score) · badges NEUTRES
   (jamais rouge/vert sur une entité) · provenance « source · date » jamais masquée ·
   diffusion restreinte = fait de couverture (jamais caché) · success/error réservés
   aux états système · cibles ≥ 44 px · focus visible · rien par la seule couleur.
   Valeurs exactes issues de themes.json. */

const { useState, useRef, useEffect, useCallback } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const WEB_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  railBg:'#EEF4FA', onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', infoContainer:'#DCEAF6', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)', hairlineStrong:'rgba(19,32,46,0.16)',
  shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
  shadowLg:'0 1px 2px rgba(19,32,46,0.05), 0 8px 28px rgba(19,32,46,0.10)',
};
const WEB_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  railBg:'#101D29', onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', infoContainer:'#1A3243', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)', hairlineStrong:'rgba(232,238,244,0.18)',
  shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
  shadowLg:'0 1px 2px rgba(0,0,0,0.35), 0 10px 30px rgba(0,0,0,0.42)',
};
const TW = (dark) => dark ? WEB_DARK : WEB_LIGHT;

const WSANS = '"Atkinson Hyperlegible", system-ui, sans-serif';
const WSERIF = '"IBM Plex Serif", Georgia, serif';
const WMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const WEB_ICONS = {
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'circle-check':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 12l2 2l4 -4"/>',
  'building':'<path d="M3 21l18 0"/><path d="M9 8l1 0"/><path d="M9 12l1 0"/><path d="M9 16l1 0"/><path d="M14 8l1 0"/><path d="M14 12l1 0"/><path d="M14 16l1 0"/><path d="M5 21v-16a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v16"/>',
  'arrow-right':'<path d="M5 12l14 0"/><path d="M13 18l6 -6"/><path d="M13 6l6 6"/>',
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'chevron-left':'<path d="M15 6l-6 6l6 6"/>',
  'chevron-down':'<path d="M6 9l6 6l6 -6"/>',
  'x':'<path d="M18 6l-12 12"/><path d="M6 6l12 12"/>',
  'home':'<path d="M5 12l-2 0l9 -9l9 9l-2 0"/><path d="M5 12v7a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-7"/><path d="M9 21v-6a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v6"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
  'shield-check':'<path d="M11.46 20.846a12 12 0 0 1 -7.96 -14.846a12 12 0 0 0 8.5 -3a12 12 0 0 0 8.5 3a12 12 0 0 1 -.09 7.06"/><path d="M15 19l2 2l4 -4"/>',
  'pin':'<path d="M15 4.5l-4 4l-4 1.5l-1.5 1.5l7 7l1.5 -1.5l1.5 -4l4 -4"/><path d="M9 15l-4.5 4.5"/><path d="M14.5 4l5.5 5.5"/>',
  'lock':'<path d="M5 13a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v6a2 2 0 0 1 -2 2h-10a2 2 0 0 1 -2 -2v-6z"/><path d="M11 16a1 1 0 1 0 2 0a1 1 0 0 0 -2 0"/><path d="M8 11v-4a4 4 0 1 1 8 0v4"/>',
  'scale':'<path d="M7 20l10 0"/><path d="M6 6l6 -1l6 1"/><path d="M12 3l0 17"/><path d="M9 12l-3 -6l-3 6a3 3 0 0 0 6 0"/><path d="M21 12l-3 -6l-3 6a3 3 0 0 0 6 0"/>',
  'report-money':'<path d="M9 5h-2a2 2 0 0 0 -2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-12a2 2 0 0 0 -2 -2h-2"/><path d="M9 3m0 2a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v0a2 2 0 0 1 -2 2h-2a2 2 0 0 1 -2 -2z"/><path d="M14 11h-2.5a1.5 1.5 0 0 0 0 3h1a1.5 1.5 0 0 1 0 3h-2.5"/><path d="M12 17v1m0 -8v1"/>',
  'building-store':'<path d="M3 21l18 0"/><path d="M3 7v1a3 3 0 0 0 6 0v-1m0 1a3 3 0 0 0 6 0v-1m0 1a3 3 0 0 0 6 0v-1h-18l2 -4h14l2 4"/><path d="M5 21l0 -10.15"/><path d="M19 21l0 -10.15"/><path d="M9 21v-4a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v4"/>',
  'mood-search':'<path d="M3 12a9 9 0 1 0 9 -9"/><path d="M9 10l.01 0"/><path d="M15 10l.01 0"/>',
  'trademark':'<path d="M9 15v-6h-2m2 3h-2m13 3v-6l-2.5 4l-2.5 -4v6m-3 -6h-12"/>',
  'license':'<path d="M15 21h-9a3 3 0 0 1 -3 -3v-1h10v2a2 2 0 0 0 4 0v-14a2 2 0 1 1 2 2h-2"/><path d="M17 3a2 2 0 0 1 2 2v12a2 2 0 0 0 2 2"/><path d="M9 7l4 0"/><path d="M9 11l4 0"/>',
  'calendar':'<path d="M4 7a2 2 0 0 1 2 -2h12a2 2 0 0 1 2 2v12a2 2 0 0 1 -2 2h-12a2 2 0 0 1 -2 -2v-12z"/><path d="M16 3v4"/><path d="M8 3v4"/><path d="M4 11h16"/>',
  'eye':'<path d="M10 12a2 2 0 1 0 4 0a2 2 0 0 0 -4 0"/><path d="M21 12c-2.4 4 -5.4 6 -9 6c-3.6 0 -6.6 -2 -9 -6c2.4 -4 5.4 -6 9 -6c3.6 0 6.6 2 9 6"/>',
  'command':'<path d="M7 9a2 2 0 1 1 2 -2v10a2 2 0 1 1 -2 -2h10a2 2 0 1 1 -2 2v-10a2 2 0 1 1 2 2h-10"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: WEB_ICONS[name] || '' }} />
  );
}

// ── Données simulées ─────────────────────────────────────────────────────────
const ENTREPRISES = [
  { id:'e1', name:'Ateliers Beaumont', siren:'552 032 534', form:'SAS', ville:'Lyon' },
  { id:'e2', name:'Beaumont & Fils', siren:'410 220 110', form:'SARL', ville:'Lille' },
  { id:'e3', name:'Groupe Beaumont Immobilier', siren:'823 114 905', form:'SA', ville:'Paris' },
  { id:'e4', name:'SCI Beaumont', restricted:true, form:'SCI', ville:'—' },
  { id:'e5', name:'Beaumont Conseil', siren:'901 556 248', form:'SASU', ville:'Bordeaux' },
  { id:'e6', name:'Distribution Beaumont', siren:'334 870 661', form:'SARL', ville:'Nantes' },
];
const MARQUES = [
  { id:'m1', name:'BEAUMONT', siren:'INPI · FR · 2019', form:'Marque verbale', ville:'Cl. 19, 37' },
  { id:'m2', name:'Beaumont Atelier', siren:'INPI · FR · 2022', form:'Marque semi-figurative', ville:'Cl. 20' },
];
const COUNTS = { entreprises:6, marques:2 };

// ── Fiches (contenu du panneau détail) ───────────────────────────────────────
const FICHES = {
  e1: {
    kind:'entreprise', name:'Ateliers Beaumont', siren:'552 032 534', form:'SAS',
    badge:'Active', updated:'28 mai 2026',
    financesConfidential:true,
    identity:[['Forme juridique','SAS'],['Activité (NAF)','Travaux de menuiserie'],['Création','2014'],['Effectif','20 à 49 salariés'],['Capital social','120 000 €']],
    dirigeants:[['Président','Mme Claire Beaumont'],['Directeur général','—'],['Commissaire aux comptes','—']],
    bodacc:[{icon:'scale',label:"Ouverture d'une procédure de sauvegarde",date:'22 mai 2026'},{icon:'report-money',label:'Dépôt des comptes annuels 2024',date:'12 mars 2026'}],
  },
  e2: {
    kind:'entreprise', name:'Beaumont & Fils', siren:'410 220 110', form:'SARL',
    badge:'Active', updated:'27 mai 2026',
    finances:[['Chiffre d’affaires 2024','4,1 M€'],['Résultat net 2024','312 k€'],['Effectif déclaré','38']],
    identity:[['Forme juridique','SARL'],['Activité (NAF)','Commerce de gros de bois'],['Création','1998'],['Effectif','20 à 49 salariés'],['Capital social','75 000 €']],
    dirigeants:[['Gérant','M. Henri Beaumont'],['Gérant','M. Paul Beaumont'],['Commissaire aux comptes','Audit Nord SARL']],
    bodacc:[{icon:'report-money',label:'Dépôt des comptes annuels 2024',date:'30 avril 2026'}],
  },
  e3: {
    kind:'entreprise', name:'Groupe Beaumont Immobilier', siren:'823 114 905', form:'SA',
    badge:'Active', updated:'28 mai 2026',
    finances:[['Chiffre d’affaires 2024','22,8 M€'],['Résultat net 2024','1,9 M€'],['Effectif déclaré','146']],
    identity:[['Forme juridique','SA à conseil d’administration'],['Activité (NAF)','Promotion immobilière de logements'],['Création','2016'],['Effectif','100 à 199 salariés'],['Capital social','2 400 000 €']],
    dirigeants:[['Président du conseil','Mme Sophie Beaumont'],['Directeur général','M. Marc Vallin'],['Commissaire aux comptes','Deloitte & Associés']],
    bodacc:[{icon:'building-store',label:'Création d’un établissement secondaire (Lyon)',date:'14 avril 2026'},{icon:'report-money',label:'Dépôt des comptes annuels 2024',date:'02 avril 2026'}],
  },
  e4: {
    kind:'entreprise', name:'SCI Beaumont', siren:null, form:'SCI', restricted:true,
    badge:'Active', updated:'28 mai 2026',
  },
  e5: {
    kind:'entreprise', name:'Beaumont Conseil', siren:'901 556 248', form:'SASU',
    badge:'Active', updated:'26 mai 2026',
    financesConfidential:true,
    identity:[['Forme juridique','SASU'],['Activité (NAF)','Conseil pour les affaires et la gestion'],['Création','2021'],['Effectif','1 à 2 salariés'],['Capital social','5 000 €']],
    dirigeants:[['Président','Mme Inès Beaumont'],['Directeur général','—'],['Commissaire aux comptes','—']],
    bodacc:[{icon:'report-money',label:'Immatriculation au RNE',date:'09 septembre 2021'}],
  },
  e6: {
    kind:'entreprise', name:'Distribution Beaumont', siren:'334 870 661', form:'SARL',
    badge:'Radiée', updated:'21 mai 2026',
    identity:[['Forme juridique','SARL'],['Activité (NAF)','Commerce de détail alimentaire'],['Création','1989'],['Radiation','2023'],['Capital social','30 000 €']],
    dirigeants:[['Gérant (dernier)','M. Joseph Beaumont'],['Liquidateur','M. Joseph Beaumont']],
    bodacc:[{icon:'scale',label:'Clôture de liquidation amiable',date:'18 octobre 2023'},{icon:'scale',label:'Dissolution anticipée',date:'04 mars 2023'}],
  },
  m1: {
    kind:'marque', name:'BEAUMONT', siren:null, form:'Marque verbale',
    badge:'Enregistrée', updated:'28 mai 2026', depot:'INPI · FR · 2019',
    marque:[['Type','Marque verbale française'],['Numéro','4 552 109'],['Dépôt','12 mars 2019'],['Enregistrement','19 juillet 2019'],['Classes','19, 37'],['Titulaire','Ateliers Beaumont (SAS)']],
  },
  m2: {
    kind:'marque', name:'Beaumont Atelier', siren:null, form:'Marque semi-figurative',
    badge:'Enregistrée', updated:'28 mai 2026', depot:'INPI · FR · 2022',
    marque:[['Type','Marque semi-figurative française'],['Numéro','4 871 330'],['Dépôt','06 juin 2022'],['Enregistrement','21 octobre 2022'],['Classes','20'],['Titulaire','Ateliers Beaumont (SAS)']],
  },
};

// ── Atomes ───────────────────────────────────────────────────────────────────
function NeutralBadge({ t, children }) {
  return (
    <span style={{
      display:'inline-block', fontSize:13, fontWeight:600, padding:'2px 10px',
      borderRadius:7, background:t.surfaceVariant, color:t.onSurfaceVariant,
      verticalAlign:'middle', letterSpacing:'0.01em',
    }}>{children}</span>
  );
}
function Provenance({ t, source, date }) {
  return (
    <div style={{
      borderTop:`1px solid ${t.hairline}`, marginTop:16, paddingTop:11,
      display:'flex', alignItems:'center', gap:7, fontSize:12.5, color:t.outline,
    }}>
      <Icon name="shield-check" size={14} color={t.outline} />
      <span>{source} · {date}</span>
    </div>
  );
}
function DeepLink({ t, label }) {
  return (
    <button className="atlas-row" style={{
      marginTop:14, minHeight:44, display:'inline-flex', alignItems:'center', gap:7,
      background:'transparent', border:'none', padding:'0 4px 0 0', cursor:'pointer',
      color:t.primary, fontFamily:WSANS, fontSize:14, fontWeight:700, borderRadius:8,
    }}>
      <span>{label}</span>
      <Icon name="arrow-right" size={16} color={t.primary} />
    </button>
  );
}
function Fields({ t, rows }) {
  return (
    <div style={{
      display:'grid', gridTemplateColumns:'minmax(120px,auto) 1fr', gap:'12px 28px',
      fontSize:14.5, alignItems:'baseline',
    }}>
      {rows.map(([l, v, mono], i) => (
        <React.Fragment key={i}>
          <span style={{ color:t.onSurfaceVariant }}>{l}</span>
          <span style={{ color:t.onSurface, fontWeight:600, textAlign:'right',
            fontFamily: mono ? WMONO : WSANS, fontVariantNumeric:'tabular-nums' }}>{v}</span>
        </React.Fragment>
      ))}
    </div>
  );
}

// ── Carte-section repliable ──────────────────────────────────────────────────
function Section({ t, title, defaultOpen = false, defaultPinned = false,
  provenance, deepLink, children, last = false }) {
  const [open, setOpen] = useState(defaultOpen);
  const [pin, setPin] = useState(defaultPinned);
  return (
    <section style={{
      background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:16,
      boxShadow:t.shadow, marginBottom:last ? 0 : 14, overflow:'hidden',
    }}>
      <div style={{ display:'flex', alignItems:'center', padding:'0 8px 0 20px' }}>
        <h3 style={{ margin:0, flex:1, minWidth:0 }}>
          <button aria-expanded={open} onClick={() => setOpen(o => !o)} className="atlas-row"
            style={{
              width:'100%', minHeight:56, display:'flex', alignItems:'center',
              padding:'10px 0', background:'transparent', border:'none', cursor:'pointer',
              textAlign:'left', fontFamily:WSERIF,
            }}>
            <span style={{ fontSize:17, fontWeight:600, color:t.onSurface,
              whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{title}</span>
          </button>
        </h3>
        <button aria-pressed={pin} aria-label={pin ? 'Détacher la section' : 'Épingler la section'}
          onClick={(e) => { e.stopPropagation(); setPin(p => !p); }} className="atlas-row"
          style={{
            width:44, height:44, flexShrink:0, display:'flex', alignItems:'center',
            justifyContent:'center', background:'transparent', border:'none',
            cursor:'pointer', borderRadius:10,
          }}>
          <Icon name="pin" size={17} color={pin ? t.primary : t.outline} style={{ transform:'rotate(45deg)' }} />
        </button>
        <button aria-hidden="true" tabIndex={-1} onClick={() => setOpen(o => !o)}
          style={{
            width:40, height:44, flexShrink:0, display:'flex', alignItems:'center',
            justifyContent:'center', background:'transparent', border:'none', cursor:'pointer',
          }}>
          <Icon name="chevron-down" size={19} color={t.outline}
            style={{ transform:`rotate(${open ? 0 : -90}deg)`, transition:'transform .18s' }} />
        </button>
      </div>
      {open && (
        <div style={{ padding:'0 20px 17px' }}>
          <div style={{ borderTop:`1px solid ${t.hairline}`, paddingTop:15 }}>
            {children}
            {deepLink && <DeepLink t={t} label={deepLink} />}
          </div>
          {provenance && <Provenance t={t} {...provenance} />}
        </div>
      )}
    </section>
  );
}

// ── Corps spécifiques ─────────────────────────────────────────────────────────
function CoverageConfidential({ t }) {
  return (
    <div style={{ textAlign:'center', padding:'4px 6px 0' }}>
      <div style={{ display:'flex', justifyContent:'center' }}>
        <Icon name="lock" size={24} color={t.outline} />
      </div>
      <p style={{ margin:'10px 0 5px', fontSize:15, fontWeight:700, color:t.onSurface, fontFamily:WSANS }}>
        Comptes confidentiels</p>
      <p style={{ margin:0, fontSize:13.5, lineHeight:1.55, color:t.onSurfaceVariant, maxWidth:380, marginLeft:'auto', marginRight:'auto' }}>
        Cette société a opté pour la confidentialité de ses comptes annuels, possibilité ouverte aux petites entreprises — aucune donnée financière n’est publique.</p>
    </div>
  );
}
function ProcedureEvent({ t, icon, label, date }) {
  return (
    <div style={{ display:'flex', gap:12, alignItems:'flex-start', padding:'2px 0' }}>
      <span style={{ width:34, height:34, flexShrink:0, borderRadius:9,
        background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name={icon} size={18} color={t.onSurfaceVariant} />
      </span>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:14.5, fontWeight:600, lineHeight:1.35, color:t.onSurface }}>{label}</p>
        <p style={{ margin:'3px 0 0', fontSize:13, color:t.onSurfaceVariant }}>{date}</p>
      </div>
    </div>
  );
}

Object.assign(window, {
  WEB_LIGHT, WEB_DARK, TW, WSANS, WSERIF, WMONO,
  Icon, ENTREPRISES, MARQUES, COUNTS, FICHES,
  NeutralBadge, Provenance, DeepLink, Fields, Section,
  CoverageConfidential, ProcedureEvent,
});
