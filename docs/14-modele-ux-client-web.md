# Modèle UX — déclinaison multi-surface (Uno)

> ✅ **ADR-029 (UI unifiée Uno Platform)** — Ce document décrit comment la **base UI unique `Atlas.App` (Uno)** se décline sur ses différentes **surfaces** : la grammaire de mise en page au format large (rail, list-detail à 2 panneaux, 3 patrons) et les **spécificités par cible** — la **tête WebAssembly** (routing/URLs, bouton Précédent du navigateur) et les têtes **natives** desktop/mobile (secure storage, navigation in-app). Le **web n'est plus un client séparé** : c'est une cible d'Uno parmi six. *Transition : le client Blazor `Atlas.Web.Client` reste l'app web en vigueur jusqu'au spike Uno concluant ; les principes ci-dessous (agnostiques) s'y appliquent déjà et se transposent à Uno.*

> Ce document est un **delta** : il ne réénonce pas la doctrine commune, il décrit **uniquement ce qui varie d'une surface à l'autre** (et ce que le format large ajoute au mobile). Pour tout le tronc commun — les 6 règles, page-vs-carte, le list-detail (R3), les 5 destinations, la fiche, le flux, la Veille, la recherche, les favoris, le profil, le kit de composants, la doctrine « descriptif, jamais de verdict » — la référence reste **`docs/12-modele-ux-client-maui.md`**, qui est **agnostique de la techno** (et le reste sous Uno).
>
> **Version** : 2.0 — **Date** : 1ᵉʳ juin 2026 — **Portée** : `Atlas.App` (Uno), **app authentifiée uniquement** (pas de pages publiques en v1).
> **Décisions cadre** : **ADR-029** (UI unifiée Uno — la tête WebAssembly tient lieu de « client web », en WASM pur ; remplace ADR-017/ADR-026), **ADR-002** (topologie : client pur de l'API), **ADR-008 / doc 06** (accessibilité).

---

## Pourquoi ce document

Avec Uno, il n'y a **pas plusieurs clients** mais **une seule app déclinée sur plusieurs surfaces** (R1 — *un seul modèle mental, plusieurs densités*). 90 % de doc 12 s'applique tel quel à toutes les têtes. Ce qui reste à trancher tient en deux blocs : (1) la **grammaire du format large** — commune à la tête WebAssembly affichée en plein écran et aux têtes desktop (rail, list-detail à deux panneaux, trois patrons de mise en page) ; (2) les **spécificités par cible** — au premier chef le **routing / les URLs** de la tête WebAssembly (chaque écran est une adresse partageable, le bouton Précédent du navigateur doit fonctionner), absent des têtes natives qui naviguent in-app. Ce doc traite ces deux blocs, et rien d'autre.

Principe directeur : **un grand écran n'est pas un mobile étiré.** C'est l'endroit où la doctrine R4 (« desktop = densité & clavier ») et le list-detail (R3) s'expriment pleinement — quelle que soit la tête (WASM en plein écran, desktop natif, tablette).

---

## 1. Périmètre & topologie (tête WebAssembly)

**App authentifiée uniquement.** La surface web (tête WebAssembly d'`Atlas.App`) couvre l'application derrière login — pas de landing ni de pages marketing en v1.

**Conséquence : WASM pur.** La tête WebAssembly d'Uno **est par nature** un client 100 % navigateur, sans circuit serveur — exactement la topologie « client pur » visée. Pas de mode hybride/SSR à arbitrer (ce qui était le point ouvert de l'ancien Blazor Web App) :
- **Plus simple** : un seul mode de rendu.
- **Plus pur (ADR-002)** : la tête WASM est 100 % consommatrice de l'API, **zéro état serveur**, exactement comme les têtes natives desktop/mobile.
- Le seul coût (poids de téléchargement initial — ~9,5 Mo en Release trim+Brotli, mesuré au spike) n'est un problème que pour le SEO/premier rendu public → **non-sujet derrière un login** (on charge une fois, l'app vit ensuite côté navigateur).

**Discipline d'architecture (ADR-029 / ADR-002)** : `Atlas.App` ne référence que **`Domain` + `Shared`**, jamais `Infrastructure` — même règle, même raison sur **toutes** les têtes, et tout particulièrement la tête WASM (code décompilable côté navigateur). Test NetArchTest dédié. Auth : access token **en mémoire** + refresh via cookie **HttpOnly** rotatif (`atlas_refresh`) côté WASM ; **jamais** de token en `localStorage` (anti-XSS). Sur les têtes natives, le refresh est conservé via le **secure storage** de la plateforme (Keychain/Keystore/DPAPI), conformément à ADR-010.

---

## 2. Navigation — mêmes 5 destinations, présentation au format large

Les **5 destinations de doc 12 §3 sont inchangées** (Accueil, Veille, Recherche, Favoris, Profil) — c'est R1, et changer la nav trahirait la promesse multi-device. Ce qui change, c'est leur **présentation** :

| | Format mobile (doc 12) | Format large (ce doc) |
|---|---|---|
| Chrome de nav | Barre d'onglets en bas (pouce) | **Rail / sidebar latéral gauche** (densité, convention pro) |
| Pourquoi | Accessible au pouce | Préserve l'espace **vertical** (listes/fiches/flux scrollent) ; consomme de l'horizontal, dont on a en abondance |

*Pourquoi le rail plutôt qu'une barre horizontale en haut* : la barre horizontale est le pattern des sites grand public/marketing ; le rail latéral est celui des **outils pros denses** (le public d'Atlas). En XAML WinUI, c'est le rôle du **`NavigationView`** (mode `Left` au format large, repli en barre/`Top` ou menu compact au format réduit) — le commutateur de présentation est piloté par la largeur (cf. §3), pas par la plateforme.

**Plafond inchangé** : 5 destinations restent le maximum (doc 12 §3). Le rail ne lève pas le plafond ; il ne fait que présenter les mêmes cinq autrement.

---

## 3. Mise en page desktop — grammaire en 3 patrons

Sur grand écran, le risque n'est pas le manque de place mais son **mauvais remplissage** : colonne mobile centrée avec des marges désertes (gâchis), ou tout étiré en pleine largeur (texte illisible au-delà de ~75-90 caractères). La réponse d'Atlas tient en une **règle de largeur hybride** et **trois patrons** qui couvrent toute l'app.

### Règle de largeur (hybride)
On applique le bon traitement selon la **nature** du contenu (même esprit que page-vs-carte, doc 12 §2) :
- **Contenu textuel** (corps de fiche, article, vue lecture) → **largeur plafonnée** (~70-90 caractères). Étiré, il devient illisible.
- **Listes, tableaux, grilles de cartes** (résultats, favoris, timeline) → **prennent la largeur** (plus de colonnes/d'items visibles = densité utile).
- En une phrase : **on n'étire jamais du texte ; on remplit la largeur avec plus de colonnes de liste ou plus de zones juxtaposées.**

### Densité par défaut
Défaut **équilibré** (aéré mais riche — on n'agresse pas le nouvel arrivant), variante **compacte** disponible via la préférence de densité (doc 12 §8, préférence d'appareil — F-062). L'app **propose**, n'impose pas.

### Les 3 patrons
Le **rail** (§2) est constant dans les trois ; chaque écran de l'app tombe dans **l'un de ces trois patrons** — pas besoin d'en inventer d'autres.

**Patron 1 — List-detail** *(Recherche, Favoris, Veille)* — la signature desktop.
Le list-detail (R3), que le mobile **empile dans le temps** (liste → tap → fiche), existe ici **spatialement** : liste (cartes-aperçu) à **gauche**, élément sélectionné à **droite**, côte à côte. La liste prend la largeur de sa zone (scan dense) ; le **détail est plafonné** (lecture). La ligne sélectionnée **reste mise en avant** tant que son détail est affiché (l'état « sélectionné », doc 12 §12, prend enfin son sens). On clique à gauche, le détail se met à jour à droite **sans changer de page**. Clavier : flèches dans la liste, Entrée pour ouvrir. Sous le point de rupture (fenêtre réduite, split-screen) → **retour à l'empilement mobile** (R2). Même code, deux densités.

**Patron 2 — Document centré** *(fiche atteinte en direct par URL, vue lecture d'article, sous-pages Profil)*.
Une colonne de contenu **plafonnée et centrée**, avec une colonne d'**appoint optionnelle** (sommaire des sections de la fiche, métadonnées, provenance). C'est ce qui empêche une fiche ouverte en plein écran de devenir illisible sur 1400 px.

**Patron 3 — Tableau de bord / hub** *(Profil ; plus tard la vue 360)*.
Une **grille de zones autonomes** : la largeur est exploitée par **juxtaposition**, pas par étirement — chaque zone garde une taille de lecture confortable. (En posant ce patron, on pose aussi, sans le maquetter, le squelette de la future **vue 360** — un dossier-cible recomposé en zones.)

C'est l'expression aboutie de R4 (« desktop = densité & clavier ») : plus d'information d'un coup, navigation clavier, états au survol — sans jamais sacrifier la lisibilité.

### Les 3 formats (paliers responsive)

La même app se décline en **trois largeurs**, articulées par les **deux points de rupture** déjà implémentés dans le client :

| Format | Largeur | Chrome de nav | Mise en page | Têtes Uno concernées |
|---|---|---|---|---|
| **Téléphone** | **< 640 px** | barre d'onglets en bas | empilement 1 colonne ; list-detail **empilé dans le temps** (liste → tap → fiche) | têtes **iOS/Android** ; WASM sur petit écran |
| **Intermédiaire** | **640 – 880 px** | rail | 1 colonne + rail ; fiche plein écran + retour | fenêtre **desktop réduite** / split-screen ; tablette ; WASM |
| **Full (desktop)** | **≥ 880 px** | rail | les 3 patrons à pleine expression (**list-detail à 2 panneaux**) | tête **desktop** (Skia : Win/macOS/Linux) ; WASM plein écran |

*Principe* : les paliers sont définis par **largeur, jamais par appareil** (R2). C'est ce qui fait qu'une **même base UI** (et un même jeu de maquettes) couvre toutes les surfaces — chaque **tête Uno** parcourt les largeurs que son contexte d'affichage lui donne : la tête mobile vit surtout en « téléphone » (tablette → intermédiaire/full), la tête desktop en « full » (+ intermédiaire/téléphone au redimensionnement), la tête WASM parcourt les trois selon la fenêtre du navigateur.

Les valeurs **640 px** (rail ↔ barre d'onglets) et **880 px** (apparition du list-detail à 2 panneaux) sont celles **déjà en place dans le code** et **font foi** — elles **tranchent** le point « valeurs exactes des points de rupture » laissé ouvert par doc 12 §16, côté web.

**Maquettes de référence (3 formats)** : [`docs/design/`](design/README.md) — `phone_mode/` (téléphone), maquettes « fenêtre réduite » (intermédiaire), maquettes « desktop » (full).

---

## 4. Routing & URLs — spécificité de la tête WebAssembly

Les têtes natives (desktop/mobile) « naviguent » in-app ; la tête **WebAssembly adresse**. Sur le web, chaque écran est une **URL** : copiable, bookmarkable, partageable, et le **bouton Précédent du navigateur doit fonctionner**. C'est la principale dimension propre à cette tête vs doc 12. (Uno fournit un `FrameNavigationView`/routeur qui mappe les vues sur des chemins ; sur la tête WASM, ces chemins se reflètent dans l'URL du navigateur. Sur les têtes natives, la même pile de navigation existe mais sans barre d'adresse — les garde-fous ci-dessous restent valables, ils s'appliquent alors aux transitions de vue plutôt qu'aux URLs.)

### Schéma d'URL (esquisse)
| Écran | Route |
|---|---|
| Destinations | `/accueil`, `/veille`, `/recherche`, `/favoris`, `/profil` |
| Fiche entreprise | `/entreprise/{siren}` |
| Fiche marque | `/marque/{id}` |
| Recherche avec requête | `/recherche?q=...&type=entreprise` (état partageable) |
| Liste / watchlist | `/favoris/{watchlistId}` |
| Sous-pages profil | `/profil/compte`, `/profil/inpi`, `/profil/donnees`… |

### Le routing matérialise les garde-fous doctrinaux
Une URL partageable doit respecter les mêmes règles que le reste de l'app — le routing **incarne** la doctrine, il ne la contourne pas :
- **Route protégée → redirection propre vers `/login`** si la session est absente/expirée (jamais un écran cassé ou un `inpi.not_connected` brut). Après login, retour à l'URL demandée.
- **URL d'entité en diffusion restreinte** (`diffusionINSEE = "N"`) : la page **n'expose pas** de données restreintes même si on arrive par URL directe — elle affiche le **caveat de couverture** (doc 12 §7), comme dans les résultats. Une URL ne doit jamais être une porte dérobée qui contourne un fait de couverture.
- **Deep-link sans données** : une fiche atteinte par URL alors qu'INPI n'est pas connecté affiche l'**état dégradé honnête** (doc 12 §10 — bandeau « connecte ton compte INPI »), pas une erreur.
- **Profondeur & retour** : la pile de navigation du navigateur reflète le parcours réel ; le « retour » revient à l'origine effective (ce qui répond, côté web, au trou « comportement du retour » listé en doc 12 §16 — sur le web, c'est l'historique du navigateur qui le résout nativement).

---

## 5. Conventions web (delta de comportement)

- **Adaptation par `AdaptiveTrigger` / `VisualStateManager`** (XAML WinUI, identique sur toutes les têtes — y compris WASM, qui n'a donc plus besoin de media queries CSS) — toujours R2 (on adapte à la largeur, pas à l'OS).
- **Clavier de premier ordre** (R4) : raccourcis (recherche `/` ou `Ctrl-K`, flèches dans les listes, `Esc` pour fermer un panneau), focus visible obligatoire (doc 06), ordre de tabulation logique.
- **Survol** : les actions secondaires (favori, archiver) peuvent se révéler au survol sur desktop — mais **toujours doublées d'un accès permanent** (kebab), car le survol n'existe ni au clavier ni au tactile (cohérent doc 12 §8 « jamais le swipe/survol seul »).
- **Densité** : la préférence de densité (doc 12 §8) s'applique ; le web peut afficher la variante compacte par défaut sur très grands écrans.
- **Titre de page** (`<title>`) et **favicon** — spécifiques à la tête WebAssembly — reflètent l'écran courant (ex. « Ateliers Beaumont — Atlas »), utile pour les onglets multiples du navigateur. (Sur les têtes natives, l'équivalent est le titre de la fenêtre desktop.)

---

## 6. Ce qui est strictement identique à doc 12

Pour éviter toute dérive, rappel explicite : **aucune** de ces choses ne change d'une surface à l'autre —
les 6 règles ; page-vs-carte ; le kit (atomes, carte-aperçu, carte-section, états) ; la fiche (anatomie, ordre, gabarits R6) ; le flux Accueil et ses garde-fous ; la Veille (frontière avec l'Accueil, palier de lecture, pont vers fiche « à vérifier ») ; la recherche (champ intelligent, segments, `diffusionINSEE`) ; favoris/watchlists ; profil ; la doctrine « descriptif, jamais de verdict » ; l'accessibilité (doc 06). Avec Uno, **un seul jeu de composants** (UserControls XAML WinUI) sert **toutes** les têtes : le kit s'écrit **une fois en XAML**, et l'accessibilité passe par les **AutomationProperties** (projetées en `aria-*` sur la tête WASM, et vers les API natives — Narrator/VoiceOver/TalkBack — sur les têtes desktop/mobile). Les maquettes HTML de `docs/design` servent de **référence visuelle** et se transposent en XAML (mapping moins direct qu'en Razor, mais une seule fois pour six cibles).

---

## 7. Ce que ce document ne tranche pas (encore)

- **Pages publiques / SEO** : hors périmètre v1 (app authentifiée seule). Si un jour landing/marketing → cela exigerait un rendu serveur (SSR), que la tête WASM pure ne fait pas ; ce serait alors un site distinct ou un avenant à l'**ADR-029**.
- ~~Valeurs exactes des points de rupture~~ → **tranchées** : **640 px** et **880 px** (cf. §3, « Les 3 formats »). À reconfirmer en XAML (`AdaptiveTrigger.MinWindowWidth`) à l'intégration Uno.
- **PWA / hors-ligne** (tête WASM installable, cache) : non traité ; à évaluer selon le besoin (le cache offline mobile est cadré par ADR-027).
- **Maquettage** des écrans au format large (rail + deux panneaux) : les maquettes HTML de `docs/design` restent la référence visuelle, à porter en XAML WinUI.

---

## Renvois

| Sujet | Référence |
|---|---|
| Doctrine UX commune (tronc) | `docs/12-modele-ux-client-maui.md` |
| Décision UI unifiée (Uno, 6 cibles) | `ADR-029` (remplace ADR-017 Blazor et ADR-026 MAUI/Avalonia) |
| Topologie client pur de l'API | `ADR-002` |
| Accessibilité (exigence bloquante) | `docs/06-accessibilite.md`, ADR-008 |
| Auth (cookie refresh côté WASM, secure storage natif, JWT) | `docs/11-api-endpoints.md` (`/auth/*`), ADR-010 |
| Placement `Atlas.App` dans la solution | `docs/10-layout-solution-dotnet.md` |
| Maquettes (3 formats : téléphone / intermédiaire / full) | [`docs/design/`](design/README.md) |

> **État d'intégration Uno (1ᵉʳ juin 2026)** :
> - ✅ **Kit et écrans portés en XAML WinUI** (UserControls : Chip/Provenance/LabeledField, cartes, SectionCard, ListDetailView, RailShell ; pages Accueil/Recherche/Veille/Favoris/Profil/Login) — cf. `docs/15` §7 (U3/U4).
> - ✅ **Routage** câblé : rail `NavigationView` → `Frame` ; **URL/hash sur la tête WASM** (deep-link + back/forward), no-op natif. *(Reconfirmer les breakpoints en `AdaptiveTrigger.MinWindowWidth` reste un raffinement : le list-detail bascule actuellement par mesure de largeur en code.)*
> - ✅ `CLAUDE.md` référence doc 12/14/06.
> - ⏳ Reste : têtes mobiles (iOS/Android) ; validation du routing WASM en navigateur.

---

*Document figé le 30 mai 2026 ; amendé le 1ᵉʳ juin 2026 (§3 paliers téléphone / intermédiaire / full, breakpoints 640 / 880 px) ; **refondu le 1ᵉʳ juin 2026** (ADR-029) de « delta web Blazor » en « déclinaison multi-surface d'Uno » — le web devient la tête WebAssembly d'`Atlas.App`. Delta de la doctrine UX (doc 12, agnostique et faisant référence pour le tronc commun). Décisions : tête WASM = client pur, WASM pur ; mêmes 5 destinations en rail (`NavigationView`) ; **mise en page format large en 3 patrons** (list-detail / document centré / tableau de bord) sous une règle de largeur hybride (texte plafonné, listes pleine largeur) ; **routing/URLs** propre à la tête WASM, matérialisant les garde-fous doctrinaux (route protégée, diffusion restreinte, deep-link dégradé). Kit écrit une fois en XAML pour les six cibles.*
