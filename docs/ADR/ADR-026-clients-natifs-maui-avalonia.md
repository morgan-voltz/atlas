# ADR-026 — Framework des clients natifs : MAUI pour le mobile, Avalonia pour le desktop (Windows / macOS / Linux)

**Statut** : ⛔ **Remplacé par [ADR-029](ADR-029-ui-unifiee-uno.md)** (UI unifiée Uno Platform couvre desktop **et** mobile **et** web). *(Était : ✅ Accepté.)* Conservé comme trace de la réflexion souveraineté/Linux qui a directement mené à ADR-029 ; Avalonia/MAUI restent un **repli** si le spike Uno échoue.
**Date** : 31 mai 2026

## Contexte

**ADR-007** fixait « **.NET MAUI** pour iOS / Android / Windows / macOS » comme client natif unique ; **ADR-017** a déjà raffiné le **volet web** (Blazor WASM pur). Restait un **angle mort assumé par défaut** : **MAUI ne cible pas Linux** (ni en exécution, ni officiellement en dev — cf. `docs/10`, condition `IsOSPlatform('windows')`).

Deux faits rendent cet angle mort **stratégique**, pas anecdotique :

1. **Bascule souveraine du poste de travail public FR/UE vers Linux.** L'**annonce DINUM du 8 avril 2026** acte la sortie de Windows pour les postes de l'État et un plan interministériel d'élimination des dépendances extra-européennes (échéance automne 2026) ; l'ordre de grandeur est de **200 000 à 400 000 postes basculés d'ici 2027-2028**, avec des précédents lourds (GendBuntu ~103 000 postes ; CNAM 80 000 agents). Ce public — **compliance, due diligence, analyse de marchés publics (F-032)** — est **aligné sur le pitch « renseignement souverain »** d'Atlas. **Horizon : V3.**
2. **MAUI et Avalonia ont des forces opposées.** **MAUI** : excellent **mobile** (raison d'être, Microsoft-officiel, push F-020 prêt côté backend), **nul sur Linux**. **Avalonia** : excellent **desktop + Linux** (Windows / macOS / Linux uniformes), **mobile réputé moins mûr**. Un « swap complet » échangerait une **force présente** (mobile) contre un **besoin futur** (Linux) **déjà couvert par le web** — mauvais marché.

**Moment opportun.** Les heads MAUI sont une **tranche fine** : F-009 (mobile) compilé Android, F-010 (desktop) compilé Windows, plus la plomberie (`AtlasApiClient`, token store, MVVM). Le **coût de bascule est faible maintenant**, douloureux après six mois d'investissement.

**Le capital réel n'est pas « du MAUI ».** C'est `Domain` + `Shared`, l'`AtlasApiClient` (Refit), les **ViewModels**, et la **doctrine UX** (`docs/12`, explicitement *agnostique de la techno*). Tout cela est **portable** ; seule la couche **Views** (XAML) + la plomberie plateforme est framework-spécifique.

## Décision

**Split par force** — chaque framework sur son terrain fort. Cinq principes.

**1. Mobile (Android / iOS) = MAUI.** On **conserve** la force mobile : valeur explicitement posée (« avocat / expert-comptable en rendez-vous », F-009), Microsoft-officiel, **push F-020 déjà prêt** côté backend (FCM / APNs), et cohérent avec le cursus de l'auteur (ADR-007). Le head mobile MAUI **n'est pas touché**.

**2. Desktop (Windows / macOS / Linux) = Avalonia.** **Un seul** framework desktop pour les **trois** OS, **Linux compris** — *future-proof* face à la bascule DINUM (public/souverain, V3). On **remplace les heads desktop MAUI (F-010)** par Avalonia plutôt que de les étoffer puis de les jeter : F-010 est à peine amorcé, autant bâtir le desktop directement là où il couvre aussi Linux.

**3. Web (navigateur, dont Linux aujourd'hui) = Blazor WASM (inchangé, ADR-017).** Le web couvre **déjà** les postes Linux dès aujourd'hui : le natif Linux n'est donc **pas un trou** à combler en urgence, c'est un **différenciateur** desktop pour V3.

**4. Noyau partagé entre les trois surfaces.** `Domain` + `Shared` + `AtlasApiClient` (Refit) + **ViewModels** + doctrine UX (`docs/12`) sont communs ; **seules les Views diffèrent** (MAUI XAML mobile / Avalonia XAML desktop / Razor web). **Discipline d'archi (ADR-002)** : le client Avalonia, **comme** MAUI et Blazor, est un **adapter entrant pur** → référence **`Domain` + `Shared` uniquement, jamais `Infrastructure`** (code décompilable). **Test NetArchTest dédié**, même règle qu'ADR-017 pour le web, étendue au projet desktop.

**5. La maturité mobile d'Avalonia se valide par banc d'essais, pas par intuition.** Le seul risque résiduel de ce split — « et si l'on voulait un jour **unifier** mobile + desktop sous Avalonia ? » — est traité par un **spike isolé, hors chemin de prod, daté**, répondant à **une seule question** : *Avalonia mobile est-il assez mûr pour nos écrans ?* Toute décision d'unification ultérieure se prendra **sur preuve**, jamais par anticipation.

### Croquis (illustratif)

```
Surface    Framework      Cibles
───────    ─────────      ──────
Mobile     MAUI           Android, iOS
Desktop    Avalonia       Windows, macOS, Linux        ← couvre la bascule souveraine V3
Web        Blazor WASM    tout navigateur (dont Linux aujourd'hui)
   │
   └── PARTAGÉ : Domain + Shared + AtlasApiClient (Refit) + ViewModels + doctrine UX (doc 12)
       SPÉCIFIQUE par surface : la couche Views (XAML MAUI / XAML Avalonia / Razor)
       RÈGLE : client = adapter entrant pur → Domain + Shared only (NetArchTest)

Banc d'essais (hors prod) : Avalonia mobile — question unique, verdict daté.
```

## Rationale

- **Chaque framework sur sa force** : on ne **troque** pas une force (mobile MAUI) contre une autre (desktop/Linux Avalonia) — on **prend les deux**.
- **Future-proof souverain sans parier le mobile** : Avalonia desktop adresse la bascule Linux publique (V3) ; MAUI garde le mobile mûr et la valeur « en rendez-vous ».
- **Timing** : le desktop MAUI (F-010) étant à peine amorcé, bâtir le desktop **directement** en Avalonia évite le double-investissement.
- **Coût contenu par le noyau partagé** : le **cher** (domaine, API client, ViewModels, UX) ne se réécrit pas ; seules les **Views** se dédoublent.
- **Risque isolé** : la maturité mobile d'Avalonia, seul inconnu, est **dérisquée par expérience** (principe 5), pas par foi.

## Conséquences

- **Positives** : couverture de **tous les OS, Linux desktop compris** (future-proof public/souverain) ; **desktop plus mûr** qu'avec MAUI ; **force mobile + push F-020 préservés** ; réutilisation du noyau ; discipline d'archi **homogène** (trois clients = adapters entrants purs, NetArchTest).
- **Négatives** : **deux couches Views natives** à maintenir (MAUI mobile + Avalonia desktop) — charge **réelle** pour un dev solo ; **deux dialectes XAML** ; **deux familles de packaging** desktop (MSIX / PKG / AppImage·Flatpak·.deb) en plus des stores mobiles ; sortie **partielle** du tout-MAUI du cursus (Avalonia à apprendre). *Atténuation* : ViewModels + doctrine UX partagés ; desktop livré **progressivement** (tranches verticales, comme `docs/15`).
- **À prévoir** :
  - **Amende ADR-007** : « MAUI pour iOS/Android/Windows/macOS » → « **MAUI pour le mobile (Android/iOS)** ; **Avalonia pour le desktop (Windows/macOS/Linux)** ». Marquer ADR-007 « **amendé par ADR-026** » et reporter dans l'**index doc 01**.
  - **F-010** (« client MAUI desktop ») : **requalifier** vers un **client desktop Avalonia** — le travail MAUI-Windows déjà fait sert de **référence d'écrans**, les **ViewModels se réutilisent**.
  - **F-009** (MAUI mobile) : **inchangé**.
  - **Banc d'essais Avalonia mobile** : spike **daté, isolé**, question unique (maturité) ; **consigner le verdict** (principe 5).
  - **NetArchTest** : étendre la règle « client → `Domain` + `Shared` only » au projet **Avalonia desktop**.
  - **doc 12** reste la **référence UX agnostique** ; un éventuel **delta desktop Avalonia** se traite comme `docs/14` l'a fait pour le web (delta, pas refonte).
  - **Packaging Linux** (AppImage / Flatpak / .deb) : à cadrer le moment venu (**hors MVP**).
  - **Références croisées** : **ADR-002** (clients HTTP purs), **ADR-004** (hexagonal), **ADR-007** (**amendé**), **ADR-017** (web — même discipline NetArchTest) ; **F-009 / F-010 / F-020 / F-029** ; **doc 12** (UX agnostique) / **doc 10** (layout solution, multi-targeting) ; contexte souverain : **migration DINUM du 8 avril 2026**, **F-032** (marchés publics — débouché public).
