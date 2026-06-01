/* atlas-auth.jsx — Atlas · écrans d'entrée (mobile)
   Parcours onboarding : Ouverture · Connexion · Création · Vérification email ·
   Défi 2FA · Proposition INPI. Sobre, institutionnel, beaucoup de blanc.
   Doctrine : le bleu est la couleur de marque/action (jamais d'alerte) ·
   champs à labels explicites · mot de passe jamais affiché en clair ·
   cibles ≥ 44 px · focus visible. Tokens : themes.json. */

const { useState, useRef } = React;

// ── Tokens Atlas ─────────────────────────────────────────────────────────────
const AUTH_LIGHT = {
  background:'#F6F9FC', surface:'#FFFFFF', surfaceVariant:'#E7EFF8',
  onSurface:'#13202E', onSurfaceVariant:'#42596F', outline:'#7E97AE',
  primary:'#1B4F7E', onPrimary:'#FFFFFF', primaryContainer:'#D4E4F4',
  onPrimaryContainer:'#0B2236', info:'#1B5E8A', infoContainer:'#DCEAF6', focus:'#1565C0',
  fieldBg:'#FFFFFF', hairline:'rgba(19,32,46,0.12)', fieldBorder:'rgba(19,32,46,0.18)',
  shadow:'0 1px 2px rgba(19,32,46,0.04), 0 2px 8px rgba(19,32,46,0.05)',
};
const AUTH_DARK = {
  background:'#0D1722', surface:'#16222F', surfaceVariant:'#21303F',
  onSurface:'#E8EEF4', onSurfaceVariant:'#AABBCC', outline:'#5A7490',
  primary:'#7FB2E0', onPrimary:'#08121C', primaryContainer:'#1E4060',
  onPrimaryContainer:'#D4E4F4', info:'#74C0E8', infoContainer:'#1A3243', focus:'#8FC4F5',
  fieldBg:'#16222F', hairline:'rgba(232,238,244,0.12)', fieldBorder:'rgba(232,238,244,0.22)',
  shadow:'0 1px 2px rgba(0,0,0,0.30), 0 2px 10px rgba(0,0,0,0.28)',
};
const TA = (dark) => dark ? AUTH_DARK : AUTH_LIGHT;

const ASANS = '"IBM Plex Sans", system-ui, sans-serif';
const ASERIF = '"IBM Plex Serif", Georgia, serif';
const AMONO = '"IBM Plex Mono", ui-monospace, monospace';

// ── Icônes (tracés Tabler, SVG inline) ───────────────────────────────────────
const AUTH_ICONS = {
  'map':'<path d="M3 7l6 -3l6 3l6 -3v13l-6 3l-6 -3l-6 3v-13"/><path d="M9 4v13"/><path d="M15 7v13"/>',
  'mail':'<path d="M3 7a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v10a2 2 0 0 1 -2 2h-14a2 2 0 0 1 -2 -2v-10z"/><path d="M3 7l9 6l9 -6"/>',
  'shield-lock':'<path d="M12 3a12 12 0 0 0 8.5 3a12 12 0 0 1 -8.5 15a12 12 0 0 1 -8.5 -15a12 12 0 0 0 8.5 -3"/><path d="M12 11m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0"/><path d="M12 12l0 2.5"/>',
  'plug-connected':'<path d="M7 12l5 5l-1.5 1.5a3.536 3.536 0 1 1 -5 -5l1.5 -1.5z"/><path d="M17 12l-5 -5l1.5 -1.5a3.536 3.536 0 1 1 5 5l-1.5 1.5z"/><path d="M3 21l2.5 -2.5"/><path d="M18.5 5.5l2.5 -2.5"/><path d="M10 11l-2 2"/><path d="M13 14l-2 2"/>',
  'chevron-left':'<path d="M15 6l-6 6l6 6"/>',
  'lock':'<path d="M5 13a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v6a2 2 0 0 1 -2 2h-10a2 2 0 0 1 -2 -2v-6z"/><path d="M11 16a1 1 0 1 0 2 0a1 1 0 0 0 -2 0"/><path d="M8 11v-4a4 4 0 1 1 8 0v4"/>',
  'refresh':'<path d="M20 11a8.1 8.1 0 0 0 -15.5 -2m-.5 -4v4h4"/><path d="M4 13a8.1 8.1 0 0 0 15.5 2m.5 4v-4h-4"/>',
  'arrow-right':'<path d="M5 12l14 0"/><path d="M13 18l6 -6"/><path d="M13 6l6 6"/>',
  'info-circle':'<path d="M3 12a9 9 0 1 0 18 0a9 9 0 0 0 -18 0"/><path d="M12 9h.01"/><path d="M11 12h1v4h1"/>',
};
function Icon({ name, size = 20, color = 'currentColor', stroke = 1.9, style = {} }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round"
      aria-hidden="true" style={{ flexShrink:0, display:'block', ...style }}
      dangerouslySetInnerHTML={{ __html: AUTH_ICONS[name] || '' }} />
  );
}

// ── Briques d'UI ──────────────────────────────────────────────────────────────
function Logo({ t, size = 56 }) {
  return (
    <span style={{ width:size, height:size, borderRadius:size * 0.28, background:t.primary,
      display:'inline-flex', alignItems:'center', justifyContent:'center', boxShadow:t.shadow }}>
      <Icon name="map" size={size * 0.5} color={t.onPrimary} stroke={1.7} />
    </span>
  );
}
function IconTile({ t, name }) {
  return (
    <span style={{ width:60, height:60, borderRadius:17, background:t.infoContainer,
      display:'inline-flex', alignItems:'center', justifyContent:'center' }}>
      <Icon name={name} size={28} color={t.info} />
    </span>
  );
}
function Field({ t, label, type = 'text', value, onChange, placeholder, hint, autoComplete, inputMode }) {
  const [focus, setFocus] = useState(false);
  return (
    <div style={{ marginTop:16 }}>
      <label style={{ display:'block', fontSize:12.5, fontWeight:600, color:t.onSurfaceVariant, marginBottom:6 }}>
        {label}</label>
      <input type={type} value={value} onChange={e => onChange(e.target.value)}
        placeholder={placeholder} autoComplete={autoComplete} inputMode={inputMode}
        onFocus={() => setFocus(true)} onBlur={() => setFocus(false)}
        style={{
          width:'100%', minHeight:50, padding:'0 14px', borderRadius:12, fontFamily:ASANS, fontSize:15,
          color:t.onSurface, background:t.fieldBg, outline:'none',
          border:`1.5px solid ${focus ? t.primary : t.fieldBorder}`,
          boxShadow: focus ? `0 0 0 3px ${t.infoContainer}` : 'none', transition:'border-color .15s, box-shadow .15s',
        }} />
      {hint && <p style={{ margin:'7px 2px 0', fontSize:11.5, lineHeight:1.45, color:t.outline }}>{hint}</p>}
    </div>
  );
}
function PrimaryBtn({ t, children, onClick }) {
  return (
    <button onClick={onClick} className="atlas-row" style={{
      width:'100%', minHeight:52, borderRadius:13, cursor:'pointer', border:'none',
      background:t.primary, color:t.onPrimary, fontFamily:ASANS, fontSize:15.5, fontWeight:600,
      display:'flex', alignItems:'center', justifyContent:'center', gap:8,
    }}>{children}</button>
  );
}
function OutlineBtn({ t, children, onClick }) {
  return (
    <button onClick={onClick} className="atlas-row" style={{
      width:'100%', minHeight:52, borderRadius:13, cursor:'pointer',
      background:'transparent', border:`1.5px solid ${t.outline}`, color:t.onSurface,
      fontFamily:ASANS, fontSize:15.5, fontWeight:600,
      display:'flex', alignItems:'center', justifyContent:'center', gap:8,
    }}>{children}</button>
  );
}
function TextLink({ t, children, onClick, tone = 'primary', center = true }) {
  return (
    <button onClick={onClick} className="atlas-row" style={{
      display:'block', margin: center ? '0 auto' : 0, minHeight:44, padding:'0 8px',
      background:'transparent', border:'none', cursor:'pointer',
      color: tone === 'muted' ? t.onSurfaceVariant : t.primary,
      fontFamily:ASANS, fontSize:13.5, fontWeight:600,
    }}>{children}</button>
  );
}
function BackBar({ t, onBack }) {
  return (
    <div style={{ flexShrink:0, padding:'46px 12px 0' }}>
      <button onClick={onBack} aria-label="Retour" className="atlas-row" style={{
        width:44, height:44, borderRadius:11, cursor:'pointer', border:'none', background:'transparent',
        display:'flex', alignItems:'center', justifyContent:'center' }}>
        <Icon name="chevron-left" size={22} color={t.onSurfaceVariant} />
      </button>
    </div>
  );
}
function Shell({ t, children, pad = '0 24px 36px' }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column',
      background:t.background, color:t.onSurface, fontFamily:ASANS, WebkitFontSmoothing:'antialiased' }}>
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:pad, overflow:'auto' }}>
        {children}
      </div>
    </div>
  );
}
function Title({ t, children }) {
  return <h1 style={{ margin:0, fontFamily:ASERIF, fontWeight:600, fontSize:25, lineHeight:1.12,
    color:t.onSurface, letterSpacing:'-0.01em' }}>{children}</h1>;
}

// ── Écran 1 — Ouverture ───────────────────────────────────────────────────────
function Ouverture({ t, go }) {
  return (
    <Shell t={t} pad="0 24px 40px">
      <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
        justifyContent:'center', textAlign:'center' }}>
        <Logo t={t} size={64} />
        <div style={{ marginTop:18, fontFamily:ASERIF, fontWeight:600, fontSize:32,
          color:t.onSurface, letterSpacing:'-0.02em' }}>Atlas</div>
        <p style={{ margin:'10px 0 0', fontSize:14.5, lineHeight:1.5, color:t.onSurfaceVariant, maxWidth:264 }}>
          Le renseignement souverain sur les entreprises françaises</p>
      </div>
      <div style={{ display:'flex', flexDirection:'column', gap:11 }}>
        <PrimaryBtn t={t} onClick={() => go('connexion')}>Se connecter</PrimaryBtn>
        <OutlineBtn t={t} onClick={() => go('creation')}>Créer un compte</OutlineBtn>
      </div>
    </Shell>
  );
}

// ── Écran 2 — Connexion ───────────────────────────────────────────────────────
function Connexion({ t, go, onBack }) {
  const [email, setEmail] = useState('');
  const [pw, setPw] = useState('');
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', background:t.background,
      color:t.onSurface, fontFamily:ASANS }}>
      <BackBar t={t} onBack={onBack} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:'8px 24px 36px', overflow:'auto' }}>
        <Title t={t}>Se connecter</Title>
        <Field t={t} label="Email" type="email" value={email} onChange={setEmail}
          placeholder="vous@cabinet.fr" autoComplete="email" inputMode="email" />
        <Field t={t} label="Mot de passe" type="password" value={pw} onChange={setPw}
          placeholder="••••••••" autoComplete="current-password" />
        <div style={{ marginTop:20 }}>
          <PrimaryBtn t={t} onClick={() => go('verif')}>Se connecter</PrimaryBtn>
        </div>
        <div style={{ marginTop:8, display:'flex', justifyContent:'center' }}>
          <TextLink t={t}>Mot de passe oublié ?</TextLink>
        </div>
        <div style={{ flex:1 }} />
        <p style={{ textAlign:'center', fontSize:13.5, color:t.onSurfaceVariant, margin:0 }}>
          Pas de compte ?{' '}
          <button onClick={() => go('creation')} className="atlas-row" style={{ background:'none', border:'none',
            cursor:'pointer', color:t.primary, fontWeight:600, fontSize:13.5, fontFamily:ASANS, padding:'8px 2px' }}>
            Créer un compte</button>
        </p>
      </div>
    </div>
  );
}

// ── Écran 3 — Création ────────────────────────────────────────────────────────
function Creation({ t, go, onBack }) {
  const [email, setEmail] = useState('');
  const [pw, setPw] = useState('');
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', background:t.background,
      color:t.onSurface, fontFamily:ASANS }}>
      <BackBar t={t} onBack={onBack} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:'8px 24px 36px', overflow:'auto' }}>
        <Title t={t}>Créer un compte</Title>
        <Field t={t} label="Email professionnel" type="email" value={email} onChange={setEmail}
          placeholder="vous@cabinet.fr" autoComplete="email" inputMode="email" />
        <Field t={t} label="Mot de passe" type="password" value={pw} onChange={setPw}
          placeholder="••••••••" autoComplete="new-password"
          hint="8 caractères minimum, avec chiffres et lettres." />
        <div style={{ marginTop:18 }}>
          <PrimaryBtn t={t} onClick={() => go('verif')}>Créer mon compte</PrimaryBtn>
        </div>
        <div style={{ display:'flex', gap:8, marginTop:14, padding:'0 2px' }}>
          <Icon name="info-circle" size={15} color={t.outline} style={{ marginTop:2 }} />
          <p style={{ margin:0, fontSize:12, lineHeight:1.5, color:t.onSurfaceVariant }}>
            Un email de vérification te sera envoyé. La connexion INPI te sera proposée ensuite
            (tu pourras la faire plus tard).</p>
        </div>
        <div style={{ flex:1 }} />
        <p style={{ textAlign:'center', fontSize:13.5, color:t.onSurfaceVariant, margin:0 }}>
          Déjà inscrit ?{' '}
          <button onClick={() => go('connexion')} className="atlas-row" style={{ background:'none', border:'none',
            cursor:'pointer', color:t.primary, fontWeight:600, fontSize:13.5, fontFamily:ASANS, padding:'8px 2px' }}>
            Se connecter</button>
        </p>
      </div>
    </div>
  );
}

// ── Écran 4 — Vérification email ──────────────────────────────────────────────
function VerifEmail({ t, go, onBack }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', background:t.background,
      color:t.onSurface, fontFamily:ASANS }}>
      <BackBar t={t} onBack={onBack} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:'8px 24px 36px' }}>
        <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
          justifyContent:'center', textAlign:'center' }}>
          <IconTile t={t} name="mail" />
          <h1 style={{ margin:'20px 0 0', fontFamily:ASERIF, fontWeight:600, fontSize:23, color:t.onSurface }}>
            Vérifie ta boîte mail</h1>
          <p style={{ margin:'10px 0 0', fontSize:14, lineHeight:1.55, color:t.onSurfaceVariant, maxWidth:280 }}>
            Un lien de vérification a été envoyé à<br />
            <span style={{ fontFamily:AMONO, fontWeight:600, color:t.onSurface }}>vous@cabinet.fr</span></p>
        </div>
        <div style={{ display:'flex', flexDirection:'column', gap:10 }}>
          <PrimaryBtn t={t} onClick={() => go('twofa')}>
            <Icon name="refresh" size={17} color={t.onPrimary} />Renvoyer le lien</PrimaryBtn>
          <div style={{ display:'flex', justifyContent:'center' }}>
            <TextLink t={t} onClick={onBack}>Modifier l'adresse</TextLink>
          </div>
          <p style={{ textAlign:'center', fontSize:11.5, color:t.outline, margin:'2px 0 0' }}>
            Pense à vérifier tes spams.</p>
        </div>
      </div>
    </div>
  );
}

// ── Écran 5 — Défi 2FA (6 cases) ──────────────────────────────────────────────
function OtpInput({ t, value, onChange }) {
  const refs = useRef([]);
  const set = (i, v) => {
    const d = v.replace(/\D/g, '').slice(-1);
    const next = value.split('');
    next[i] = d || '';
    onChange(next.join(''));
    if (d && i < 5) refs.current[i + 1] && refs.current[i + 1].focus();
  };
  const key = (e, i) => {
    if (e.key === 'Backspace' && !value[i] && i > 0) refs.current[i - 1] && refs.current[i - 1].focus();
  };
  return (
    <div style={{ display:'flex', gap:9, justifyContent:'center', marginTop:24 }}>
      {[0,1,2,3,4,5].map(i => {
        const filled = !!value[i];
        return (
          <input key={i} ref={el => refs.current[i] = el} value={value[i] || ''}
            onChange={e => set(i, e.target.value)} onKeyDown={e => key(e, i)}
            inputMode="numeric" maxLength={1} aria-label={`Chiffre ${i + 1}`}
            style={{
              width:46, height:56, textAlign:'center', borderRadius:12, fontFamily:AMONO,
              fontSize:22, fontWeight:600, color:t.onSurface, background:t.fieldBg, outline:'none',
              border:`1.5px solid ${filled ? t.primary : t.fieldBorder}`,
              boxShadow: filled ? `0 0 0 3px ${t.infoContainer}` : 'none', transition:'border-color .12s',
            }} />
        );
      })}
    </div>
  );
}
function TwoFA({ t, go, onBack }) {
  const [code, setCode] = useState('428');
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', background:t.background,
      color:t.onSurface, fontFamily:ASANS }}>
      <BackBar t={t} onBack={onBack} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:'8px 24px 36px' }}>
        <div style={{ textAlign:'center', marginTop:8 }}>
          <Title t={t}>Code de vérification</Title>
          <p style={{ margin:'10px 0 0', fontSize:14, lineHeight:1.5, color:t.onSurfaceVariant }}>
            Saisis le code à 6 chiffres envoyé à ton appareil.</p>
        </div>
        <OtpInput t={t} value={code} onChange={setCode} />
        <div style={{ marginTop:26 }}>
          <PrimaryBtn t={t} onClick={() => go('inpi')}>Vérifier</PrimaryBtn>
        </div>
        <div style={{ display:'flex', justifyContent:'center', marginTop:8 }}>
          <TextLink t={t} tone="muted">Utiliser un code de secours</TextLink>
        </div>
        <div style={{ flex:1 }} />
      </div>
    </div>
  );
}

// ── Écran 6 — Proposition INPI ────────────────────────────────────────────────
function PropositionINPI({ t, go, onBack }) {
  return (
    <div style={{ height:'100%', display:'flex', flexDirection:'column', background:t.background,
      color:t.onSurface, fontFamily:ASANS }}>
      <BackBar t={t} onBack={onBack} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', padding:'8px 24px 36px' }}>
        <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
          justifyContent:'center', textAlign:'center' }}>
          <IconTile t={t} name="plug-connected" />
          <h1 style={{ margin:'20px 0 0', fontFamily:ASERIF, fontWeight:600, fontSize:23, color:t.onSurface }}>
            Connecte ton compte INPI</h1>
          <p style={{ margin:'10px 0 0', fontSize:14, lineHeight:1.55, color:t.onSurfaceVariant, maxWidth:296 }}>
            Atlas interroge l'INPI en ton nom. Tes identifiants restent chiffrés et ne sont jamais affichés.</p>
        </div>
        <div style={{ display:'flex', flexDirection:'column', gap:10 }}>
          <PrimaryBtn t={t} onClick={() => go('done')}>
            <Icon name="plug-connected" size={18} color={t.onPrimary} />Connecter mon compte INPI</PrimaryBtn>
          <div style={{ display:'flex', justifyContent:'center' }}>
            <TextLink t={t} tone="muted" onClick={() => go('done')}>Plus tard</TextLink>
          </div>
          <div style={{ display:'flex', gap:8, marginTop:4, padding:'11px 13px', borderRadius:12,
            background:t.surfaceVariant }}>
            <Icon name="info-circle" size={15} color={t.onSurfaceVariant} style={{ marginTop:1 }} />
            <p style={{ margin:0, fontSize:12, lineHeight:1.5, color:t.onSurfaceVariant }}>
              Sans connexion INPI, la recherche et les fiches resteront indisponibles — réactivable
              à tout moment depuis ton profil.</p>
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Écran de fin (confirmation minimale) ─────────────────────────────────────
function Done({ t }) {
  return (
    <Shell t={t}>
      <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center',
        justifyContent:'center', textAlign:'center' }}>
        <Logo t={t} size={56} />
        <p style={{ margin:'16px 0 0', fontFamily:ASERIF, fontWeight:600, fontSize:20, color:t.onSurface }}>
          Bienvenue dans Atlas</p>
        <p style={{ margin:'6px 0 0', fontSize:13.5, color:t.onSurfaceVariant }}>Ton compte est prêt.</p>
      </div>
    </Shell>
  );
}

// ── Écran d'entrée (nav interne) ──────────────────────────────────────────────
const SCREENS = { ouverture:Ouverture, connexion:Connexion, creation:Creation,
  verif:VerifEmail, twofa:TwoFA, inpi:PropositionINPI, done:Done };
const BACK = { connexion:'ouverture', creation:'ouverture', verif:'creation',
  twofa:'verif', inpi:'twofa' };

function AuthScreen({ dark = false, screen = 'ouverture' }) {
  const t = TA(dark);
  const [cur, setCur] = useState(screen);
  const C = SCREENS[cur] || Ouverture;
  return <C t={t} go={setCur} onBack={() => setCur(BACK[cur] || 'ouverture')} />;
}

Object.assign(window, { AuthScreen });
