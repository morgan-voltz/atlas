/* atlas-profil.jsx — Atlas · écran Profil (mobile)
   HUB de réglages : un menu qui mène à des sous-pages (pas une longue page en vrac).
   Deux familles (Mon compte, Application), puis « Se déconnecter » détaché.
   Sous-pages sensibles : Connexion INPI (identifiant masqué, jamais le mot de
   passe) et Données & confidentialité (export bénin + zone danger isolée).
   Doctrine : badge d'état système discret, jamais alarmant, texte + icône ·
   rouge réservé aux actions système (déconnexion, suppression) · cibles ≥ 44 px ·
   focus visible. Tokens : themes.json. */

const { useState } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const PRO_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236',
  success:'#1E7A40', successContainer:'#DCEFE2',
  error:'#B5281F', errorContainer:'#FAEAE8',
  info:'#1B5E8A', infoContainer:'#DCEAF6', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)',
  shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const PRO_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4',
  success:'#5FC98A', successContainer:'#14301F',
  error:'#F08A82', errorContainer:'#331815',
  info:'#74C0E8', infoContainer:'#1A3243', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)',
  shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TP = (dark) => dark ? PRO_DARK : PRO_LIGHT;

const PSANS = '"IBM Plex Sans", system-ui, sans-serif';
const PSERIF = '"IBM Plex Serif", Georgia, serif';
const PMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const PRO_ICONS = {
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'chevron-left':'<path d="M15 6l-6 6l6 6"/>',
  'user-circle':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 10m-3 0a3 3 0 1 0 6 0a3 3 0 1 0 -6 0"/><path d="M6.168 18.849a4 4 0 0 1 3.832 -2.849h4a4 4 0 0 1 3.834 2.855"/>',
  'plug-connected':'<path d="M7 12l5 5l-1.5 1.5a3.536 3.536 0 1 1 -5 -5l1.5 -1.5z"/><path d="M17 12l-5 -5l1.5 -1.5a3.536 3.536 0 1 1 5 5l-1.5 1.5z"/><path d="M3 21l2.5 -2.5"/><path d="M18.5 5.5l2.5 -2.5"/><path d="M10 11l-2 2"/><path d="M13 14l-2 2"/>',
  'shield-lock':'<path d="M12 3a12 12 0 0 0 8.5 3a12 12 0 0 1 -8.5 15a12 12 0 0 1 -8.5 -15a12 12 0 0 0 8.5 -3"/><path d="M12 11m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M12 12l0 2.5"/>',
  'settings':'<path d="M10.325 4.317c.426 -1.756 2.924 -1.756 3.35 0a1.724 1.724 0 0 0 2.573 1.066c1.543 -.94 3.31 .826 2.37 2.37a1.724 1.724 0 0 0 1.065 2.572c1.756 .426 1.756 2.924 0 3.35a1.724 1.724 0 0 0 -1.066 2.573c.94 1.543 -.826 3.31 -2.37 2.37a1.724 1.724 0 0 0 -2.572 1.065c-.426 1.756 -2.924 1.756 -3.35 0a1.724 1.724 0 0 0 -2.573 -1.066c-1.543 .94 -3.31 -.826 -2.37 -2.37a1.724 1.724 0 0 0 -1.065 -2.572c-1.756 -.426 -1.756 -2.924 0 -3.35a1.724 1.724 0 0 0 1.066 -2.573c-.94 -1.543 .826 -3.31 2.37 -2.37c1 .608 2.296 .07 2.572 -1.065z"/><path d="M9 12a3 3 0 1 0 6 0a3 3 0 0 0 -6 0"/>',
  'layout-2':'<path d="M4 4m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z"/><path d="M4 14m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z"/><path d="M14 4m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z"/><path d="M14 14m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z"/>',
  'logout':'<path d="M14 8v-2a2 2 0 0 0 -2 -2h-7a2 2 0 0 0 -2 2v12a2 2 0 0 0 2 2h7a2 2 0 0 0 2 -2v-2"/><path d="M9 12h12l-3 -3"/><path d="M18 15l3 -3"/>',
  'circle-check':'<path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0"/><path d="M9 12l2 2l4 -4"/>',
  'lock':'<path d="M5 13a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v6a2 2 0 0 1 -2 2h-10a2 2 0 0 1 -2 -2v-6z"/><path d="M11 16a1 1 0 1 0 2 0a1 1 0 0 0 -2 0"/><path d="M8 11v-4a4 4 0 1 1 8 0v4"/>',
  'download':'<path d="M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2 -2v-2"/><path d="M7 11l5 5l5 -5"/><path d="M12 4l0 12"/>',
  'alert-triangle':'<path d="M12 9v4"/><path d="M10.363 3.591l-8.106 13.534a1.914 1.914 0 0 0 1.636 2.871h16.214a1.914 1.914 0 0 0 1.636 -2.87l-8.106 -13.536a1.914 1.914 0 0 0 -3.274 0z"/><path d="M12 16h.01"/>',
  'refresh':'<path d="M20 11a8.1 8.1 0 0 0 -15.5 -2m-.5 -4v4h4"/><path d="M4 13a8.1 8.1 0 0 0 15.5 2m.5 4v-4h-4"/>',
  'news':'<path d="M16 6h3a1 1 0 0 1 1 1v11a2 2 0 0 1 -4 0v-13a1 1 0 0 0 -1 -1h-10a1 1 0 0 0 -1 1v12a3 3 0 0 0 3 3h11"/><path d="M8 8l4 0"/><path d="M8 12l4 0"/><path d="M8 16l4 0"/>',
  'search':'<path d="M10 10m-7 0a7 7 0 1 0 14 0a7 7 0 1 0 -14 0"/><path d="M21 21l-6 -6"/>',
  'rss':'<path d="M5 19m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M4 4a16 16 0 0 1 16 16"/><path d="M4 11a9 9 0 0 1 9 9"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'user':'<path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"/><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: PRO_ICONS[name] || '' }} />
  );
}

// ── Badge d'état système (discret, texte + icône, jamais alarmant) ───────────
function StatusBadge({ t, label, tone = 'success' }) {
  const color = tone === 'success' ? t.success : t.info;
  const bg = tone === 'success' ? t.successContainer : t.infoContainer;
  return (
    <span style={{ display:'inline-flex', alignItems:'center', gap:5, flexShrink:0,
      fontSize:12, fontWeight:600, padding:'3px 9px 3px 7px', borderRadius:8, background:bg, color }}>
      <Icon name="circle-check" size={13} color={color} />
      {label}
    </span>
  );
}

// ── Ligne de menu (ouvre une sous-page) ──────────────────────────────────────
function MenuRow({ t, icon, title, subtitle, badge, onClick, last }) {
  return (
    <button onClick={onClick} className="atlas-row" style={{
      width:'100%', minHeight:64, display:'flex', alignItems:'center', gap:13,
      padding:'12px 14px', background:'transparent', border:'none', cursor:'pointer', textAlign:'left',
      position:'relative', fontFamily:PSANS,
    }}>
      <span style={{ width:38, height:38, flexShrink:0, borderRadius:10, background:t.surfaceVariant,
        display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name={icon} size={20} color={t.onSurfaceVariant} />
      </span>
      <span style={{ flex:1, minWidth:0 }}>
        <span style={{ display:'block', fontSize:15, fontWeight:600, color:t.onSurface,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{title}</span>
        <span style={{ display:'block', fontSize:12.5, color:t.onSurfaceVariant, marginTop:2,
          whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{subtitle}</span>
      </span>
      {badge}
      <Icon name="chevron-right" size={18} color={t.outline} />
      {!last && <div style={{ position:'absolute', left:65, right:0, bottom:0, height:1, background:t.hairline }} />}
    </button>
  );
}
function SectionLabel({ t, children }) {
  return (
    <p style={{ margin:'0 0 7px', padding:'0 6px', fontSize:11.5, fontWeight:600,
      letterSpacing:'0.06em', textTransform:'uppercase', color:t.onSurfaceVariant }}>{children}</p>
  );
}
function MenuGroup({ t, children }) {
  return (
    <div style={{ background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:16,
      boxShadow:t.shadow, overflow:'hidden' }}>{children}</div>
  );
}

// ── En-tête de sous-page ─────────────────────────────────────────────────────
function SubHeader({ t, title, onBack }) {
  return (
    <div style={{ flexShrink:0, background:t.background, padding:'50px 8px 10px',
      borderBottom:`1px solid ${t.hairline}` }}>
      <button onClick={onBack} className="atlas-row" style={{
        display:'inline-flex', alignItems:'center', gap:3, minHeight:44, padding:'0 8px',
        background:'transparent', border:'none', cursor:'pointer', color:t.onSurfaceVariant, fontSize:14 }}>
        <Icon name="chevron-left" size={19} color={t.onSurfaceVariant} />
        <span>Profil</span>
      </button>
      <h1 style={{ margin:'4px 10px 0', fontFamily:PSERIF, fontWeight:600, fontSize:24,
        lineHeight:1.1, color:t.onSurface, letterSpacing:'-0.01em' }}>{title}</h1>
    </div>
  );
}

// ── Hub ──────────────────────────────────────────────────────────────────────
function Hub({ t, go }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:PSANS, WebkitFontSmoothing:'antialiased' }}>
      <div style={{ flexShrink:0, background:t.background, padding:'52px 16px 8px' }}>
        <h1 style={{ margin:0, fontFamily:PSERIF, fontWeight:600, fontSize:26, lineHeight:1.05,
          color:t.onSurface, letterSpacing:'-0.01em' }}>Profil</h1>
        <p style={{ margin:'5px 0 0', fontSize:13, color:t.onSurfaceVariant, fontFamily:PMONO }}>
          c.beaumont@cabinet-bma.fr</p>
      </div>

      <div style={{ flex:1, overflow:'auto', padding:'16px 16px 24px' }}>
        <SectionLabel t={t}>Mon compte</SectionLabel>
        <MenuGroup t={t}>
          <MenuRow t={t} icon="user-circle" title="Compte" subtitle="Email, mot de passe, 2FA"
            onClick={() => {}} />
          <MenuRow t={t} icon="plug-connected" title="Connexion INPI"
            subtitle="Identifiants pour l'accès aux données"
            badge={<StatusBadge t={t} label="Connecté" />} onClick={() => go('inpi')} />
          <MenuRow t={t} icon="shield-lock" title="Données & confidentialité"
            subtitle="Exporter mes données · supprimer le compte" onClick={() => go('data')} last />
        </MenuGroup>

        <div style={{ height:22 }} />
        <SectionLabel t={t}>Application</SectionLabel>
        <MenuGroup t={t}>
          <MenuRow t={t} icon="settings" title="Paramètres généraux"
            subtitle="Thème, densité, langue, notifications" onClick={() => {}} />
          <MenuRow t={t} icon="layout-2" title="Affichage & données"
            subtitle="Gabarits de fiche, sections masquées, accessibilité" onClick={() => {}} last />
        </MenuGroup>

        {/* Action détachée — pas un réglage */}
        <button className="atlas-row" style={{
          marginTop:30, width:'100%', minHeight:50, display:'flex', alignItems:'center', justifyContent:'center',
          gap:9, background:'transparent', border:`1px solid ${t.hairline}`, borderRadius:13, cursor:'pointer',
          color:t.error, fontFamily:PSANS, fontSize:15, fontWeight:600 }}>
          <Icon name="logout" size={18} color={t.error} />
          Se déconnecter
        </button>
      </div>

      <TabBar t={t} active={4} />
    </div>
  );
}

// ── Sous-page : Connexion INPI ───────────────────────────────────────────────
function InpiPage({ t, onBack }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:PSANS }}>
      <SubHeader t={t} title="Connexion INPI" onBack={onBack} />
      <div style={{ flex:1, overflow:'auto', padding:'16px 16px 24px' }}>
        <div style={{ background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:16,
          boxShadow:t.shadow, padding:'16px' }}>
          <div style={{ display:'flex', alignItems:'center', gap:9 }}>
            <Icon name="circle-check" size={20} color={t.success} />
            <span style={{ fontSize:16, fontWeight:600, color:t.onSurface }}>Connecté</span>
            <span style={{ marginLeft:'auto', fontSize:12, color:t.outline }}>testé le 28 mai 2026</span>
          </div>
          <div style={{ marginTop:14, paddingTop:14, borderTop:`1px solid ${t.hairline}`,
            display:'grid', gridTemplateColumns:'auto 1fr', gap:'8px 16px', fontSize:13.5, alignItems:'baseline' }}>
            <span style={{ color:t.onSurfaceVariant }}>Identifiant</span>
            <span style={{ color:t.onSurface, fontWeight:500, textAlign:'right', fontFamily:PMONO }}>c.beaumont@…</span>
            <span style={{ color:t.onSurfaceVariant }}>Mot de passe</span>
            <span style={{ color:t.outline, textAlign:'right', letterSpacing:'0.12em' }}>•••••••• (masqué)</span>
          </div>
          <div style={{ display:'flex', gap:10, marginTop:16 }}>
            <button className="atlas-row" style={{ flex:1, minHeight:46, borderRadius:12, cursor:'pointer',
              display:'inline-flex', alignItems:'center', justifyContent:'center', gap:7,
              background:t.primary, color:t.onPrimary, border:'none', fontFamily:PSANS, fontSize:13.5, fontWeight:600 }}>
              <Icon name="refresh" size={16} color={t.onPrimary} />Tester la connexion</button>
            <button className="atlas-row" style={{ flex:1, minHeight:46, borderRadius:12, cursor:'pointer',
              background:'transparent', color:t.onSurfaceVariant, border:`1px solid ${t.outline}`,
              fontFamily:PSANS, fontSize:13.5, fontWeight:600 }}>Déconnecter</button>
          </div>
          <div style={{ display:'flex', gap:8, marginTop:16, padding:'11px 12px', borderRadius:11,
            background:t.surfaceVariant }}>
            <Icon name="lock" size={15} color={t.onSurfaceVariant} style={{ marginTop:1 }} />
            <p style={{ margin:0, fontSize:12, lineHeight:1.5, color:t.onSurfaceVariant }}>
              Tes identifiants INPI sont chiffrés et ne sont jamais affichés. Ils servent uniquement
              à interroger l'INPI en ton nom.</p>
          </div>
        </div>
      </div>
      <TabBar t={t} active={4} />
    </div>
  );
}

// ── Sous-page : Données & confidentialité ────────────────────────────────────
function DataPage({ t, onBack }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:PSANS }}>
      <SubHeader t={t} title="Données & confidentialité" onBack={onBack} />
      <div style={{ flex:1, overflow:'auto', padding:'16px 16px 28px' }}>
        <SectionLabel t={t}>Mes données</SectionLabel>
        <button className="atlas-row" style={{
          width:'100%', display:'flex', alignItems:'center', gap:13, padding:'15px 14px', minHeight:64,
          background:t.surface, border:`1px solid ${t.hairline}`, borderRadius:16, boxShadow:t.shadow,
          cursor:'pointer', textAlign:'left' }}>
          <span style={{ width:38, height:38, flexShrink:0, borderRadius:10, background:t.infoContainer,
            display:'flex', alignItems:'center', justifyContent:'center' }}>
            <Icon name="download" size={20} color={t.info} />
          </span>
          <span style={{ flex:1, minWidth:0 }}>
            <span style={{ display:'block', fontSize:15, fontWeight:600, color:t.onSurface }}>Exporter mes données</span>
            <span style={{ display:'block', fontSize:12.5, color:t.onSurfaceVariant, marginTop:2 }}>
              Archive JSON (favoris, listes, historique)</span>
          </span>
          <Icon name="chevron-right" size={18} color={t.outline} />
        </button>

        {/* Zone danger — nettement détachée */}
        <div style={{ height:30 }} />
        <SectionLabel t={t}>Zone sensible</SectionLabel>
        <div style={{ background:t.errorContainer, border:`1px solid ${t.error}`, borderRadius:16, padding:'16px' }}>
          <div style={{ display:'flex', alignItems:'center', gap:8 }}>
            <Icon name="alert-triangle" size={19} color={t.error} />
            <span style={{ fontSize:15, fontWeight:700, color:t.error }}>Supprimer mon compte</span>
          </div>
          <p style={{ margin:'10px 0 0', fontSize:12.5, lineHeight:1.55, color:t.onSurfaceVariant }}>
            Effacement sous 30 jours. Tes données personnelles et identifiants INPI sont supprimés ;
            certaines données de facturation sont conservées et anonymisées (obligation légale).</p>
          <button className="atlas-row" style={{
            marginTop:14, width:'100%', minHeight:48, borderRadius:12, cursor:'pointer',
            background:'transparent', border:`1.5px solid ${t.error}`, color:t.error,
            fontFamily:PSANS, fontSize:14, fontWeight:600 }}>
            Supprimer définitivement…</button>
        </div>
      </div>
      <TabBar t={t} active={4} />
    </div>
  );
}

// ── Barre d'onglets ──────────────────────────────────────────────────────────
const PRO_TABS = [['Accueil','news'], ['Recherche','search'], ['Veille','rss'], ['Favoris','star'], ['Profil','user']];
function TabBar({ t, active = 4 }) {
  return (
    <div style={{ flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px' }}>
      {PRO_TABS.map(([label, icon], i) => {
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

// ── Écran Profil ─────────────────────────────────────────────────────────────
// view: 'hub' | 'inpi' | 'data'
function ProfilScreen({ dark = false, view: initView = 'hub' }) {
  const t = TP(dark);
  const [view, setView] = useState(initView);
  if (view === 'inpi') return <InpiPage t={t} onBack={() => setView('hub')} />;
  if (view === 'data') return <DataPage t={t} onBack={() => setView('hub')} />;
  return <Hub t={t} go={setView} />;
}

Object.assign(window, { ProfilScreen });
