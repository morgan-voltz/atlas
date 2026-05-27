# Audit profond Atlas — Partie 7 : `Atlas.Maui`

> Rapport **lecture seule** (audit allégé : workload mobile, pas de tests unitaires). Sévérités : 🔴 P0 · 🟠 P1 · 🟡 P2. Date : 2026-05-28.

## Contexte de la partie

Client multi-plateforme .NET MAUI 10 (TFM : `net10.0-android`/`-ios`/`-maccatalyst`/`-windows`). Stade précoce mais **fonctionnel** : 4 pages (Login, CompanySearch, CompanyDetail, SearchHistory) en MVVM (CommunityToolkit.Mvvm `ObservableObject`/`[RelayCommand]`), services `AtlasApiClient`/`ITokenStore`/`SecureStorageTokenStore`, `Models/ApiModels.cs` (DTO locaux).

## Verdict global

**Santé : bon socle, deux points à traiter (accessibilité, build iOS/Mac).** La **règle d'or est respectée** (référence uniquement Domain + Shared), le stockage des tokens est **sécurisé** (SecureStorage natif), MVVM propre. Mais l'**accessibilité (exigence DoD bloquante) est absente** et le **build iOS/MacCatalyst échoue** (CA1711 + warnings-as-errors).

## Points forts (à préserver)

- ✅ **Pureté hexagonale respectée** : `Atlas.Maui.csproj` ne référence que `Atlas.Domain` + `Atlas.Shared` (aucun `Infrastructure.*`/`Application`/`Api`). Aucun `using Atlas.Infrastructure`/EF/Npgsql. Tout passe par `AtlasApiClient` (HTTP). *(Mais non enforcé par un test d'archi — cf. Partie 6.)*
- ✅ **Tokens en stockage sécurisé** : `SecureStorageTokenStore` via `ISecureStorage` (Keychain iOS / Keystore Android), **pas de clair, pas de `Preferences`**. Bearer attaché à chaque requête, refresh géré, aucun secret loggé, base URL **HTTPS**.
- ✅ **MVVM** correct (CommunityToolkit), file-scoped namespaces partout, 4 TFM net10.

## Constats & recommandations

### 🟠 P1 — Accessibilité absente (exigence DoD bloquante)
Les 4 pages XAML n'ont **aucun** `SemanticProperties` (Description/Hint/HeadingLevel), **aucun** `AutomationProperties`, pas de mise à l'échelle de police, pas de contraste documenté, items de liste `Label`+`TapGestureRecognizer` sans rôle sémantique. Or l'UI MAUI existe → la **checklist accessibilité (doc 06 §12, ADR-008)** aurait dû s'appliquer ; elle est **bloquante** dans la DoD pour toute PR touchant l'UI.
**Reco** : implémenter le socle accessibilité (labels sémantiques, rôles, ordre de focus, contraste WCAG 2.1 AA, scaling police) sur les pages existantes, et l'inclure désormais à chaque page. Sans cela, toute PR UI viole la DoD.

### 🟠 P1 — Build iOS/MacCatalyst cassé (CA1711 + warnings-as-errors)
`Directory.Build.props` active `TreatWarningsAsErrors=true`. Les classes `Platforms/iOS/AppDelegate.cs:6` et `Platforms/MacCatalyst/AppDelegate.cs:6` déclenchent **CA1711** (« le nom de type ne doit pas finir par 'Delegate' ») → **erreurs de build** sur ces TFM (constaté lors du build initial de session). Or le nom `AppDelegate` est **imposé par la plateforme** (bridging natif `[Register("AppDelegate")]`) → faux positif bloquant.
**Reco** : suppression **scopée** (editorconfig ciblant `Platforms/iOS/**` et `Platforms/MacCatalyst/**`, ou `#pragma`/`[SuppressMessage]`) pour CA1711 sur ces fichiers. Rétablit le build mobile Apple.

### 🟡 P2 — Base URL par défaut = émulateur Android
`AtlasApiOptions` a une base URL par défaut `https://10.0.2.2:7201/` (hôte localhost de l'émulateur Android). À **rendre configurable par environnement** (dev/staging/prod) avant tout packaging réel.

### 🟡 P2 — Refresh token via cookie implicite (à fiabiliser)
Le refresh repose sur un cookie httpOnly « porté par le `CookieContainer` du handler », mais l'`HttpClient` est créé **sans `HttpClientHandler`/`CookieContainer` explicite** → dépend du comportement implicite par plateforme. Sur natif MAUI, la persistance d'un cookie httpOnly n'est pas garantie de façon homogène.
**Reco** : configurer explicitement le handler (CookieContainer) ou transporter le refresh token autrement côté mobile (SecureStorage), et tester sur chaque plateforme. Pas de certificate pinning (recommandé en mobile sensible, non bloquant).

## Tableau de synthèse (priorisé)

| # | Sévérité | Constat | Effort |
|---|---|---|---|
| 1 | 🟠 P1 | Accessibilité absente (DoD bloquante, doc 06/ADR-008) | M |
| 2 | 🟠 P1 | Build iOS/MacCatalyst cassé (CA1711 + warnings-as-errors) | XS |
| 3 | 🟡 P2 | Base URL par défaut = émulateur (rendre configurable) | S |
| 4 | 🟡 P2 | Refresh token via cookie implicite (handler explicite + pinning) | S |

## Note transverse

La pureté Maui est correcte mais **non gardée par un test** → à enforcer via NetArchTest (Partie 6, item 3) ; c'est la règle la plus critique pour ce projet (décompilation client). L'accessibilité rejoint le chantier UI à venir (F-044 timeline MAUI, non encore livrée).
