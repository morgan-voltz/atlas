/* atlas-fiche.jsx — Atlas · fiche entreprise (mobile)
   UNE page qui défile, faite de cartes-sections repliables (pas un hub).
   Doctrine : descriptif (aucun verdict/score) · provenance jamais masquée ·
   badge d'état NEUTRE · success/error réservés aux états système ·
   titres = vrais headings · état ouvert/replié annoncé · cibles ≥ 44 px ·
   focus visible · rien par la seule couleur. Tokens issus de themes.json. */

const { useState } = React;

// ── Tokens Atlas (valeurs exactes de themes.json) ────────────────────────────
const FICHE_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', success:'#1E7A40', error:'#B5281F',
  info:'#1B5E8A', focus:'#1565C0',
  hairline:'rgba(19,32,46,0.10)', shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const FICHE_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', success:'#5FC98A', error:'#F08A82',
  info:'#74C0E8', focus:'#8FC4F5',
  hairline:'rgba(232,238,244,0.11)', shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TF = (dark) => dark ? FICHE_DARK : FICHE_LIGHT;

const FSANS = '"IBM Plex Sans", system-ui, sans-serif';
const FSERIF = '"IBM Plex Serif", Georgia, serif';
const FMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const FICHE_ICONS = {
  'chevron-left':'<path d="M15 6l-6 6l6 6"/>',
  'chevron-right':'<path d="M9 6l6 6l-6 6"/>',
  'chevron-down':'<path d="M6 9l6 6l6 -6"/>',
  'shield-check':'<path d="M11.46 20.846a12 12 0 0 1 -7.96 -14.846a12 12 0 0 0 8.5 -3a12 12 0 0 0 8.5 3a12 12 0 0 1 -.09 7.06"/><path d="M15 19l2 2l4 -4"/>',
  'pin':'<path d="M15 4.5l-4 4l-4 1.5l-1.5 1.5l7 7l1.5 -1.5l1.5 -4l4 -4"/><path d="M9 15l-4.5 4.5"/><path d="M14.5 4l5.5 5.5"/>',
  'lock':'<path d="M5 13a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v6a2 2 0 0 1 -2 2h-10a2 2 0 0 1 -2 -2v-6z"/><path d="M11 16a1 1 0 1 0 2 0a1 1 0 0 0 -2 0"/><path d="M8 11v-4a4 4 0 1 1 8 0v4"/>',
  'arrow-right':'<path d="M5 12l14 0"/><path d="M13 18l6 -6"/><path d="M13 6l6 6"/>',
  'plus':'<path d="M12 5l0 14"/><path d="M5 12l14 0"/>',
  'eye':'<path d="M10 12a2 2 0 1 0 4 0a2 2 0 0 0 -4 0"/><path d="M21 12c-2.4 4 -5.4 6 -9 6c-3.6 0 -6.6 -2 -9 -6c2.4 -4 5.4 -6 9 -6c3.6 0 6.6 2 9 6"/>',
  'star':'<path d="M12 17.75l-6.172 3.245l1.179 -6.873l-5 -4.867l6.9 -1l3.086 -6.253l3.086 6.253l6.9 1l-5 4.867l1.179 6.873z"/>',
  'users-group':'<path d="M10 13a2 2 0 1 0 4 0a2 2 0 0 0 -4 0"/><path d="M8 21v-1a2 2 0 0 1 2 -2h4a2 2 0 0 1 2 2v1"/><path d="M15 5a2 2 0 1 0 4 0a2 2 0 0 0 -4 0"/><path d="M17 10h2a2 2 0 0 1 2 2v1"/><path d="M5 5a2 2 0 1 0 4 0a2 2 0 0 0 -4 0"/><path d="M3 13v-1a2 2 0 0 1 2 -2h2"/>',
  'report-money':'<path d="M9 5h-2a2 2 0 0 0 -2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-12a2 2 0 0 0 -2 -2h-2"/><path d="M9 3m0 2a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v0a2 2 0 0 1 -2 2h-2a2 2 0 0 1 -2 -2z"/><path d="M14 11h-2.5a1.5 1.5 0 0 0 0 3h1a1.5 1.5 0 0 1 0 3h-2.5"/><path d="M12 17v1m0 -8v1"/>',
  'scale':'<path d="M7 20l10 0"/><path d="M6 6l6 -1l6 1"/><path d="M12 3l0 17"/><path d="M9 12l-3 -6l-3 6a3 3 0 0 0 6 0"/><path d="M21 12l-3 -6l-3 6a3 3 0 0 0 6 0"/>',
  'building-store':'<path d="M3 21l18 0"/><path d="M3 7v1a3 3 0 0 0 6 0v-1m0 1a3 3 0 0 0 6 0v-1m0 1a3 3 0 0 0 6 0v-1h-18l2 -4h14l2 4"/><path d="M5 21l0 -10.15"/><path d="M19 21l0 -10.15"/><path d="M9 21v-4a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v4"/>',
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
      dangerouslySetInnerHTML={{ __html: FICHE_ICONS[name] || '' }} />
  );
}

// ── Atomes ───────────────────────────────────────────────────────────────────
function NeutralBadge({ t, children }) {
  return (
    <span style={{
      display:'inline-block', fontSize:12, fontWeight:500, padding:'2px 9px',
      borderRadius:7, background:t.surfaceVariant, color:t.onSurfaceVariant,
      verticalAlign:'middle',
    }}>{children}</span>
  );
}
function Provenance({ t, source, date }) {
  return (
    <div style={{
      borderTop:`1px solid ${t.hairline}`, marginTop:14, paddingTop:10,
      display:'flex', alignItems:'center', gap:7, fontSize:12, color:t.outline,
    }}>
      <Icon name="shield-check" size={14} color={t.outline} />
      <span>{source} · {date}</span>
    </div>
  );
}
function DeepLink({ t, label }) {
  return (
    <button style={{
      marginTop:14, minHeight:44, display:'inline-flex', alignItems:'center', gap:7,
      background:'transparent', border:'none', padding:'0', cursor:'pointer',
      color:t.primary, fontFamily:FSANS, fontSize:13.5, fontWeight:600,
    }}>
      <span>{label}</span>
      <Icon name="arrow-right" size={16} color={t.primary} />
    </button>
  );
}
function Fields({ t, rows }) {
  return (
    <div style={{
      display:'grid', gridTemplateColumns:'auto 1fr', gap:'11px 22px',
      fontSize:14, alignItems:'baseline',
    }}>
      {rows.map(([l, v, mono], i) => (
        <React.Fragment key={i}>
          <span style={{ color:t.onSurfaceVariant }}>{l}</span>
          <span style={{ color:t.onSurface, fontWeight:500, textAlign:'right',
            fontFamily: mono ? FMONO : FSANS, fontVariantNumeric:'tabular-nums' }}>{v}</span>
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
      boxShadow:t.shadow, margin:`0 16px ${last ? 0 : 12}px`, overflow:'hidden',
    }}>
      <div style={{ display:'flex', alignItems:'center', padding:'0 8px 0 18px' }}>
        <h2 style={{ margin:0, flex:1, minWidth:0 }}>
          <button aria-expanded={open} onClick={() => setOpen(o => !o)} className="atlas-row"
            style={{
              width:'100%', minHeight:56, display:'flex', alignItems:'center',
              padding:'10px 0', background:'transparent', border:'none', cursor:'pointer', textAlign:'left',
              fontFamily:FSANS,
            }}>
            <span style={{ fontSize:15.5, fontWeight:600, color:t.onSurface,
              whiteSpace:'nowrap', overflow:'hidden', textOverflow:'ellipsis' }}>{title}</span>
          </button>
        </h2>
        <button aria-pressed={pin} aria-label={pin ? 'Détacher la section' : 'Épingler la section'}
          onClick={(e) => { e.stopPropagation(); setPin(p => !p); }} className="atlas-row"
          style={{
            width:44, height:44, flexShrink:0, display:'flex', alignItems:'center', justifyContent:'center',
            background:'transparent', border:'none', cursor:'pointer', borderRadius:10,
          }}>
          <Icon name="pin" size={17} color={pin ? t.primary : t.outline}
            style={{ transform:'rotate(45deg)' }} />
        </button>
        <button aria-hidden="true" tabIndex={-1} onClick={() => setOpen(o => !o)}
          style={{
            width:38, height:44, flexShrink:0, display:'flex', alignItems:'center', justifyContent:'center',
            background:'transparent', border:'none', cursor:'pointer',
          }}>
          <Icon name="chevron-down" size={19} color={t.outline}
            style={{ transform:`rotate(${open ? 0 : -90}deg)`, transition:'transform .18s' }} />
        </button>
      </div>
      {open && (
        <div style={{ padding:'0 18px 15px' }}>
          <div style={{ borderTop:`1px solid ${t.hairline}`, paddingTop:14 }}>
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
// Vide de couverture (comptes confidentiels) — un FAIT, sans bouton.
function CoverageConfidential({ t }) {
  return (
    <div style={{ textAlign:'center', padding:'4px 6px 0' }}>
      <div style={{ display:'flex', justifyContent:'center' }}>
        <Icon name="lock" size={24} color={t.outline} />
      </div>
      <p style={{ margin:'10px 0 5px', fontSize:14.5, fontWeight:600, color:t.onSurface, fontFamily:FSANS }}>
        Comptes confidentiels</p>
      <p style={{ margin:0, fontSize:13, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:300, marginLeft:'auto', marginRight:'auto' }}>
        Cette société a opté pour la confidentialité de ses comptes annuels, possibilité ouverte aux petites entreprises — aucune donnée financière n'est publique.</p>
    </div>
  );
}
// Événement BODACC présenté comme un fait neutre (jamais en alerte).
function ProcedureEvent({ t, icon, label, date }) {
  return (
    <div style={{ display:'flex', gap:11, alignItems:'flex-start', padding:'2px 0' }}>
      <span style={{ width:32, height:32, flexShrink:0, borderRadius:8,
        background:t.surfaceVariant, display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name={icon} size={17} color={t.onSurfaceVariant} />
      </span>
      <div style={{ flex:1, minWidth:0 }}>
        <p style={{ margin:0, fontSize:14, fontWeight:500, lineHeight:1.3, color:t.onSurface }}>{label}</p>
        <p style={{ margin:'2px 0 0', fontSize:12.5, color:t.onSurfaceVariant }}>{date}</p>
      </div>
    </div>
  );
}
function ProceduresBody({ t }) {
  return (
    <div style={{ display:'flex', flexDirection:'column', gap:13 }}>
      <ProcedureEvent t={t} icon="scale" label="Ouverture d'une procédure de sauvegarde" date="22 mai 2026" />
      <ProcedureEvent t={t} icon="report-money" label="Dépôt des comptes annuels 2024" date="12 mars 2026" />
    </div>
  );
}

// ── Sections masquées par pertinence (R6) — signalées, réversibles ───────────
function MaskedSections({ t }) {
  const [open, setOpen] = useState(false);
  if (!open) {
    return (
      <button onClick={() => setOpen(true)} className="atlas-row"
        style={{
          margin:'0 16px', width:'calc(100% - 32px)', minHeight:52,
          display:'flex', alignItems:'center', justifyContent:'center', gap:9,
          background:'transparent', border:`1px dashed ${t.outline}`, borderRadius:14,
          cursor:'pointer', color:t.onSurfaceVariant, fontFamily:FSANS, fontSize:13.5, fontWeight:500,
          padding:'12px 16px',
        }}>
        <Icon name="eye" size={16} color={t.onSurfaceVariant} />
        <span>+3 sections masquées — Établissements, PI, Marchés publics</span>
      </button>
    );
  }
  return (
    <div>
      <p style={{ margin:'0 20px 10px', fontSize:11.5, fontWeight:600, letterSpacing:'0.07em',
        textTransform:'uppercase', color:t.onSurfaceVariant }}>Sections masquées par pertinence</p>
      <Section t={t} title="Établissements" provenance={{ source:'RNE', date:'28 mai 2026' }}>
        <Fields t={t} rows={[['Siège','Lyon 3ᵉ'], ['Établissements actifs','2'], ['Secondaires','Villeurbanne']]} />
      </Section>
      <Section t={t} title="Propriété intellectuelle" provenance={{ source:'INPI', date:'28 mai 2026' }}
        deepLink="Voir le portefeuille de marques">
        <Fields t={t} rows={[['Marques déposées','1'], ['Brevets','0'], ['Dessins & modèles','0']]} />
      </Section>
      <Section t={t} title="Marchés publics" last
        provenance={{ source:'DECP · data.gouv', date:'28 mai 2026' }}>
        <div style={{ display:'flex', gap:10, alignItems:'flex-start' }}>
          <Icon name="building-store" size={20} color={t.outline} style={{ marginTop:1 }} />
          <p style={{ margin:0, fontSize:13.5, lineHeight:1.5, color:t.onSurfaceVariant }}>
            Aucune attribution de marché public recensée sur les 24 derniers mois.</p>
        </div>
      </Section>
      <button onClick={() => setOpen(false)} className="atlas-row"
        style={{ margin:'12px 16px 0', width:'calc(100% - 32px)', minHeight:44,
          display:'flex', alignItems:'center', justifyContent:'center', gap:7,
          background:'transparent', border:'none', cursor:'pointer',
          color:t.primary, fontFamily:FSANS, fontSize:13, fontWeight:600 }}>
        Masquer à nouveau
      </button>
    </div>
  );
}

// ── En-tête d'identité (toujours visible, en haut) ───────────────────────────
function IdentityHeader({ t }) {
  const [followed, setFollowed] = useState(false);
  return (
    <div style={{ flexShrink:0, background:t.background, padding:'52px 16px 14px',
      borderBottom:`1px solid ${t.hairline}`, zIndex:5 }}>
      <button className="atlas-row" style={{ display:'flex', alignItems:'center', gap:4,
        minHeight:36, padding:'0 6px 0 0', background:'transparent', border:'none',
        cursor:'pointer', color:t.onSurfaceVariant, fontFamily:FSANS, fontSize:13 }}>
        <Icon name="chevron-left" size={18} color={t.onSurfaceVariant} />
        <span>Accueil</span>
      </button>
      <div style={{ display:'flex', alignItems:'flex-start', justifyContent:'space-between', gap:12, marginTop:6 }}>
        <div style={{ flex:1, minWidth:0 }}>
          <h1 style={{ margin:0, fontFamily:FSERIF, fontWeight:600, fontSize:22,
            lineHeight:1.15, color:t.onSurface, letterSpacing:'-0.01em' }}>Ateliers Beaumont</h1>
          <p style={{ margin:'6px 0 0', fontSize:13, color:t.onSurfaceVariant, lineHeight:1.4 }}>
            <span style={{ fontFamily:FMONO }}>552 032 534</span> · SAS · <NeutralBadge t={t}>Active</NeutralBadge>
          </p>
        </div>
        <button aria-pressed={followed} aria-label={followed ? 'Ne plus suivre' : 'Suivre cette entreprise'}
          onClick={() => setFollowed(f => !f)} className="atlas-row"
          style={{ width:44, height:44, flexShrink:0, borderRadius:13, cursor:'pointer',
            background:followed ? t.primaryContainer : t.surface,
            border:`1px solid ${followed ? t.primaryContainer : t.hairline}`, boxShadow:t.shadow,
            display:'flex', alignItems:'center', justifyContent:'center' }}>
          <Icon name="star" size={20} color={followed ? t.primary : t.outline}
            style={{ fill: followed ? t.primary : 'none' }} />
        </button>
      </div>
      <div style={{ display:'flex', alignItems:'center', gap:6, marginTop:11,
        fontSize:12, color:t.outline }}>
        <Icon name="shield-check" size={14} color={t.outline} />
        <span>À jour · 28 mai 2026</span>
      </div>
    </div>
  );
}

// ── Barre d'onglets ──────────────────────────────────────────────────────────
const FICHE_TABS = [
  ['Accueil','news'], ['Recherche','search'], ['Veille','rss'],
  ['Favoris','star'], ['Profil','user'],
];
function TabBar({ t, active = 0 }) {
  return (
    <div style={{ flexShrink:0, display:'flex', background:t.surface,
      borderTop:`1px solid ${t.hairline}`, padding:'8px 4px 22px' }}>
      {FICHE_TABS.map(([label, icon], i) => {
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

// ── Écran fiche (gabarit Comptable) ──────────────────────────────────────────
function FicheScreen({ dark = false }) {
  const t = TF(dark);
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:FSANS, WebkitFontSmoothing:'antialiased' }}>
      <IdentityHeader t={t} />
      <div style={{ flex:1, overflow:'auto', padding:'12px 0 16px' }}>
        <Section t={t} title="Bilans & finances" defaultOpen defaultPinned
          provenance={{ source:'INPI (greffe)', date:'28 mai 2026' }}>
          <CoverageConfidential t={t} />
        </Section>
        <Section t={t} title="BODACC · procédures" defaultOpen
          provenance={{ source:'BODACC', date:'28 mai 2026' }}
          deepLink="Voir toutes les annonces">
          <ProceduresBody t={t} />
        </Section>
        <Section t={t} title="Identité" defaultOpen
          provenance={{ source:'RNE', date:'28 mai 2026' }}>
          <Fields t={t} rows={[
            ['Forme juridique','SAS'],
            ['Activité (NAF)','Travaux de menuiserie'],
            ['Création','2014'],
            ['Effectif','20 à 49 salariés'],
            ['Capital social','120 000 €'],
          ]} />
        </Section>
        <Section t={t} title="Dirigeants" defaultOpen
          provenance={{ source:'RNE', date:'28 mai 2026' }}
          deepLink="Voir le graphe des dirigeants">
          <Fields t={t} rows={[
            ['Président','Mme Claire Beaumont'],
            ['Directeur général','—'],
            ['Commissaire aux comptes','—'],
          ]} />
        </Section>
        <MaskedSections t={t} />
      </div>
      <TabBar t={t} active={0} />
    </div>
  );
}

Object.assign(window, { FicheScreen });
