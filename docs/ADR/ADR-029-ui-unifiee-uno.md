## ADR-029 — UI unifiée Uno Platform (remplace ADR-017 et ADR-026, amende ADR-007)

**Statut** : ✅ Accepté
**Date** : 30 mai 2026
**Remplace** : **ADR-017** (client web Blazor Web App) **et ADR-026** (MAUI mobile + Avalonia desktop) — désormais **caducs** (Uno couvre web **et** desktop **et** mobile).
**Amende** : **ADR-007** (stack .NET/MAUI) — la ligne « Clients » change (Uno remplace MAUI, Avalonia **et** Blazor Web App).

> **Numérotation** : confirmée **ADR-029** (ADR-027 = cache offline, ADR-028 = observabilité, déjà actés et mergés). ADR-026 (Avalonia/MAUI) est lui aussi supplanté : voir « Remplace » ci-dessus.

### Contexte

ADR-007 a choisi **.NET MAUI** (natif Android/iOS/Windows/macOS) + **Blazor Web App** (web, ADR-017) — explicitement *« contraint par le cursus actuel (MAUI étudié) »*. Deux évolutions rebattent les cartes :

1. **La souveraineté devient un critère d'architecture.** Atlas se vend *souverain* (renseignement français/européen, personas sensibles : avocats, compliance, secteur public). Un produit souverain doit **tourner sur Linux desktop** (bascule française/européenne hors Microsoft) et limiter sa dépendance à Microsoft. Or **MAUI n'a pas de cible Linux desktop officielle** (Linux n'arrive que via un partenariat de rendu… avec Avalonia), et Blazor ne couvre pas le natif.
2. **Aucun client natif n'est encore écrit.** Le projet a démarré par le **web Blazor exprès pour garder le choix ouvert**. Un squelette `Atlas.Web`/`Atlas.Web.Client` existe (ADR-017), mais aucun `Atlas.Maui`. Le coût de changement est donc **minimal maintenant** — il ne le sera plus dans six mois.

La pile front C#/.NET sérieuse en 2026 : **MAUI** (officiel, mobile mûr, pas de Linux natif), **Avalonia** (Linux Tier 1, indépendant, mais mobile suivant le cycle MAUI), **Uno Platform** (Win/macOS/Linux/iOS/Android/**WASM** depuis une base unique), **Blazor Hybrid** (réutilise le web, mais pas de Linux desktop officiel).

### Décision

**Atlas adopte Uno Platform comme framework UI unique sur toutes les surfaces — desktop (Windows, macOS, Linux), mobile (iOS, Android) et web (WebAssembly). Le client web Blazor (ADR-017) est abandonné ; MAUI n'est pas adopté.**

**1. Un seul paradigme d'UI.** L'interface est écrite **une seule fois** en **C# + XAML (dialecte WinUI)** et projetée sur les 6 cibles. Motivation centrale pour un **porteur solo** (profil plutôt back) : ne maintenir **qu'un** monde d'UI, et concentrer l'effort sur le domaine et l'architecture. Cela pousse *plus loin* la logique d'ADR-007 (« un seul stack à maîtriser ») : un seul stack **UI**, au lieu de MAUI + Blazor.

**2. Linux desktop = cible first-class.** Uno traite Linux nativement (pas un patch communautaire), ce qui satisfait l'exigence de souveraineté/inclusion (« un produit inclusif, quel que soit l'OS »).

**3. Le client reste consommateur pur de l'API (ADR-002 préservé).** L'UI Uno (y compris la cible WASM, décompilable dans le navigateur — même contrainte que l'ancien `Atlas.Web.Client`) ne référence que **`Domain` + `Shared`**, **jamais** `Infrastructure`. Test **NetArchTest** dédié, comme prévu pour MAUI/Blazor. Toute logique sensible et tout appel externe restent côté `Atlas.Api`.

**4. Auth inchangée.** Le pattern reste celui d'ADR-017/ADR-010 : access token JWT court **en mémoire** + refresh **cookie HttpOnly rotatif** côté WASM ; stockage sécurisé natif (équivalent SecureStorage) côté desktop/mobile. Rien à réinventer côté serveur.

**5. La doctrine UX est préservée intégralement.** `docs/12-modele-ux-client-maui.md` (les 6 règles, list-detail, page-vs-carte, 5 destinations, kit, états) **est agnostique de la techno** — elle reste la référence et se mappe en XAML Uno. Les patrons de mise en page desktop (doc 14 §3) restent valides ; seule la techno d'implémentation change.

**6. Limite assumée — souveraineté desktop, pas mobile.** Sur iOS/Android, .NET dépend des workloads Microsoft (cycle MAUI) **quel que soit le framework** (Uno comme Avalonia). La souveraineté gagnée est donc **réelle sur desktop (Linux/Win/macOS)**, et l'argument commercial doit être formulé ainsi — ne pas prétendre « souverain partout » tant qu'il y a du mobile .NET.

### Rationale

- **Cohérence ADR-007 renforcée** : tout-.NET, et désormais **une seule UI** au lieu de deux paradigmes (MAUI XAML + Blazor Razor) qu'un solo aurait dû maintenir en parallèle.
- **Souveraineté réelle (desktop)** : Linux first-class + framework indépendant de la roadmap de Microsoft.
- **Coût de changement minimal** : décision prise **avant** tout client natif et avec un simple squelette web — fenêtre idéale.
- **Topologie et sécurité intactes** : la règle « client → Domain + Shared, jamais Infrastructure » et l'auth cookie/bearer sont reprises telles quelles (ADR-002, ADR-010).
- **Profil porteur** : « écrire l'UI une fois » sert un dev orienté back qui veut minimiser sa surface front.

### Conséquences

- **Positives** : une seule UI pour 6 cibles ; Linux natif ; indépendance desktop vis-à-vis de Microsoft ; doctrine UX (doc 12) préservée ; `Domain`/`Shared` réutilisés ; un seul paradigme à apprendre/maintenir (solo).
- **Négatives / coûts** :
  - **Abandon du squelette Blazor** (`Atlas.Web`/`Atlas.Web.Client`, marqués « fait » en ADR-017) — du code réel est jeté. Assumé, car minime à ce stade.
  - **Courbe XAML WinUI** : dialecte nouveau pour le porteur (différent du XAML MAUI et de Razor).
  - **Setup Uno réputé touffu** ; écosystème plus petit que Microsoft ; **natif par-OS** (notifications, biométrie, fichiers) parfois à implémenter manuellement — « écrire une fois » ≠ « sans effort front ».
  - **WASM .NET lourd au chargement** — atténué par « app authentifiée seule » (pas de SEO), mais à surveiller (poids initial).
  - **Pari sur un framework unique** plus petit que l'écosystème Microsoft : pas de plan B immédiat si Uno ralentit.
- **À prévoir** :
  - **Spike Uno recommandé avant de retirer le squelette Blazor** : un écran d'Atlas (carte-aperçu + liste, doc 12 §11) sur les 6 cibles, pour valider XAML WinUI + rendu Linux + poids WASM. *Ne supprimer `Atlas.Web` qu'après un spike concluant* (ordre sûr).
  - **Refondre `docs/14`** : ce n'est plus « le delta web de Blazor » mais « Uno, une base UI multi-cible » ; le web devient une **cible d'Uno** parmi d'autres, pas un client séparé.
  - **Amender `ADR-007`** : ligne « Clients » → *Uno Platform (Windows, macOS, Linux, iOS, Android, WebAssembly)* ; retirer MAUI et Blazor Web App.
  - **Marquer `ADR-017` comme remplacé** par le présent ADR.
  - Mettre à jour `docs/10` (layout solution : `Atlas.App` Uno remplace `Atlas.Maui` + `Atlas.Web`), `CLAUDE.md`, et le test NetArchTest (cible Uno → Domain + Shared).
  - ✅ **Numérotation confirmée** : ADR-029 (027 = cache offline, 028 = observabilité).

---

*ADR figé le 30 mai 2026. Atlas adopte **Uno Platform** comme UI unique sur Windows/macOS/Linux/iOS/Android/WebAssembly. Remplace ADR-017 (Blazor web, caduc) et ADR-026 (MAUI+Avalonia, caduc), amende ADR-007 (MAUI/Avalonia/Blazor retirés). Motivation : souveraineté (Linux desktop natif, indépendance Microsoft côté desktop) + un seul paradigme d'UI pour un porteur solo. Décision prise tôt (aucun client natif écrit) pour un coût de changement minimal. La doctrine UX (doc 12) est préservée — agnostique de la techno. Spike Uno recommandé avant de retirer le squelette Blazor existant.*
