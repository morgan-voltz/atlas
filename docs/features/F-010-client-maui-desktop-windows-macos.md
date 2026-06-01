# F-010 — Client MAUI desktop (Windows/macOS)

> ⛔ **ADR-029 (UI unifiée Uno Platform)** — Le client desktop devient une **cible de `Atlas.App` (Uno)** couvrant **Windows, macOS et Linux** (Linux natif = exigence de souveraineté), aux côtés du mobile et du web. ADR-029 remplace l'ADR-026 (qui avait requalifié le desktop en Avalonia) **et** l'ADR-017 (Blazor). L'amorce MAUI-Windows décrite ici sert de référence d'écrans et reste l'app **actuelle** jusqu'au spike Uno concluant.

> **Statut** : 🟡 Amorcé (MVP 1, 27 mai 2026). Le client desktop est la **même app MAUI** (projet unique multi-cible) que F-009 : tout le code (ViewModels, `AtlasApiClient`, écrans) est partagé. **Compilé Windows** (`net10.0-windows`) — a nécessité de passer les `[ObservableProperty]` en propriétés partielles (compatibilité WinRT, MVVMTK0045). Adaptation grand écran : fenêtre dimensionnée (1100×800, min 800×600) sur desktop. **Reste** : adaptations UX écran large plus poussées (panneaux multiples, raccourcis clavier), **packaging MSIX (Windows) / PKG (macOS)** et **build macCatalyst** (machine macOS requise), QA sur cible réelle.
> **Doctrine UX** : `docs/12-modele-ux-client-maui.md` impose **R4 — desktop = densité & clavier** : information visible d'un coup, raccourcis clavier, états au survol. L'adaptation est gouvernée par la **largeur** (R2, `VisualStateManager` + `AdaptiveTrigger`) et non par la plateforme — un mobile en paysage, une fenêtre desktop réduite ou un split-screen sont traités par la même mécanique.

**Description** : application desktop reprenant les fonctions du mobile, avec une UX adaptée écran large.

**Valeur user** : utilisateurs intensifs (pro) qui préfèrent une app desktop dédiée à une interface web.

**Complexité** : ★★★

**APIs externes** : utilise le backend du projet.

**Dépendances** : F-009 (la majeure partie du code est partagée).

**Détails techniques** :
- Adaptations UI pour grand écran (panneaux multiples, raccourcis clavier).
- Packaging MSIX (Windows) et PKG (macOS).
