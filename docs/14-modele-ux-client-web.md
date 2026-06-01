# Modèle UX — client web (Blazor)

> Doctrine d'expérience utilisateur du **client web** d'Atlas (`Atlas.Web`, Blazor).
> Ce document est un **delta** : il ne réénonce pas la doctrine commune, il décrit **uniquement ce qui diffère du natif**. Pour tout le tronc commun — les 6 règles, page-vs-carte, le list-detail (R3), les 5 destinations, la fiche, le flux, la Veille, la recherche, les favoris, le profil, le kit de composants, la doctrine « descriptif, jamais de verdict » — la référence reste **`docs/12-modele-ux-client-maui.md`**, qui est **agnostique de la techno**.
>
> **Version** : 1.0 — **Date** : 30 mai 2026 — **Portée** : `Atlas.Web`, **app authentifiée uniquement** (pas de pages publiques en v1).
> **Décisions cadre** : **ADR-017** (Blazor Web App — précisé ci-dessous en WASM pur), **ADR-002** (topologie : client pur de l'API), **ADR-008 / doc 06** (accessibilité).

---

## Pourquoi ce document

Le client web n'est **pas une refonte** : c'est le **troisième form-factor** d'un même produit (R1 — *un seul modèle mental, plusieurs densités*). 90 % de doc 12 s'applique tel quel. Mais le web introduit trois choses que le natif n'a pas, et qui méritent d'être tranchées : un **rendu et une topologie** propres (WASM pur), une **navigation** au format large (rail + list-detail à deux panneaux), et surtout une dimension absente du mobile — le **routing / les URLs** (chaque écran est une adresse partageable). Ce doc traite ces trois deltas, et rien d'autre.

Principe directeur : **le desktop web n'est pas un mobile étiré.** C'est l'endroit où la doctrine R4 (« desktop = densité & clavier ») et le list-detail (R3) s'expriment pleinement.

---

## 1. Périmètre & topologie (raffinement d'ADR-017)

**App authentifiée uniquement.** Le client web couvre l'application derrière login — pas de landing ni de pages marketing en v1.

**Conséquence : WASM pur** (raffinement de l'ADR-017, qui laissait WASM-vs-Auto ouvert). Le mode « Auto » de Blazor n'existe que pour optimiser le premier rendu de **pages publiques** (SSR → bascule WASM) ; sans public, il n'a plus d'objet et garderait un bref **circuit serveur** au démarrage — l'entorse exacte à la pureté « client pur » signalée par ADR-017. On prend donc **WASM pur** :
- **Plus simple** : un seul mode de rendu.
- **Plus pur (ADR-002)** : le client est 100 % consommateur de l'API, **zéro état serveur**, exactement comme MAUI.
- Le seul coût de WASM (poids de téléchargement initial) n'était un problème que pour le SEO/premier rendu public → **non-sujet derrière un login** (on charge une fois, l'app vit ensuite côté navigateur).

**Discipline d'architecture (rappel ADR-017)** : `Atlas.Web.Client` (l'assembly interactif) ne référence que **`Domain` + `Shared`**, jamais `Infrastructure` — même règle, même raison que MAUI (code décompilable côté navigateur). Test NetArchTest dédié. Auth : access token **en mémoire** + refresh via cookie **HttpOnly** rotatif (`atlas_refresh`) ; **jamais** de token en `localStorage` (anti-XSS).

---

## 2. Navigation — mêmes 5 destinations, présentation au format large

Les **5 destinations de doc 12 §3 sont inchangées** (Accueil, Veille, Recherche, Favoris, Profil) — c'est R1, et changer la nav trahirait la promesse multi-device. Ce qui change, c'est leur **présentation** :

| | Mobile (doc 12) | Web desktop (ce doc) |
|---|---|---|
| Chrome de nav | Barre d'onglets en bas (pouce) | **Rail / sidebar latéral gauche** (densité, convention pro) |
| Pourquoi | Accessible au pouce | Préserve l'espace **vertical** (listes/fiches/flux scrollent) ; consomme de l'horizontal, dont on a en abondance |

*Pourquoi le rail plutôt qu'une barre horizontale en haut* : la barre horizontale est le pattern des sites grand public/marketing ; le rail latéral est celui des **outils pros denses** (le public d'Atlas). Cohérent avec MAUI Shell, qui prévoyait déjà flyout/rail en desktop.

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

| Format | Largeur | Chrome de nav | Mise en page | Sert aussi |
|---|---|---|---|---|
| **Téléphone** | **< 640 px** | barre d'onglets en bas | empilement 1 colonne ; list-detail **empilé dans le temps** (liste → tap → fiche) | l'app **mobile** (MAUI — ADR-026) |
| **Intermédiaire** | **640 – 880 px** | rail | 1 colonne + rail ; fiche plein écran + retour | fenêtre desktop **réduite** / split-screen |
| **Full (desktop)** | **≥ 880 px** | rail | les 3 patrons à pleine expression (**list-detail à 2 panneaux**) | l'app **desktop** (Avalonia — ADR-026) |

*Principe* : les paliers sont définis par **largeur, jamais par appareil** (R2). C'est ce qui fait qu'un **même jeu de maquettes** couvre les trois clients — le **web** parcourt les trois largeurs ; le **mobile** = téléphone (tablette → intermédiaire/full) ; le **desktop** = full (+ intermédiaire/téléphone au redimensionnement de la fenêtre).

Les valeurs **640 px** (rail ↔ barre d'onglets) et **880 px** (apparition du list-detail à 2 panneaux) sont celles **déjà en place dans le code** et **font foi** — elles **tranchent** le point « valeurs exactes des points de rupture » laissé ouvert par doc 12 §16, côté web.

**Maquettes de référence (3 formats)** : [`docs/design/`](design/README.md) — `phone_mode/` (téléphone), maquettes « fenêtre réduite » (intermédiaire), maquettes « desktop » (full).

---

## 4. Routing & URLs — la dimension propre au web

Le mobile « navigue » ; le web **adresse**. Chaque écran est une **URL** : copiable, bookmarkable, partageable, et le **bouton Précédent du navigateur doit fonctionner**. C'est entièrement neuf vs doc 12.

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

- **Adaptation par media queries / largeur de fenêtre** (équivalent web de `AdaptiveTrigger`/`OnIdiom`) — toujours R2 (on adapte à la largeur, pas à l'OS).
- **Clavier de premier ordre** (R4) : raccourcis (recherche `/` ou `Ctrl-K`, flèches dans les listes, `Esc` pour fermer un panneau), focus visible obligatoire (doc 06), ordre de tabulation logique.
- **Survol** : les actions secondaires (favori, archiver) peuvent se révéler au survol sur desktop — mais **toujours doublées d'un accès permanent** (kebab), car le survol n'existe ni au clavier ni au tactile (cohérent doc 12 §8 « jamais le swipe/survol seul »).
- **Densité** : la préférence de densité (doc 12 §8) s'applique ; le web peut afficher la variante compacte par défaut sur très grands écrans.
- **Titre de page** (`<title>`) et **favicon** reflètent l'écran courant (ex. « Ateliers Beaumont — Atlas ») — utile pour les onglets multiples, usage typique du desktop.

---

## 6. Ce qui est strictement identique à doc 12

Pour éviter toute dérive, rappel explicite : **aucune** de ces choses ne change sur le web —
les 6 règles ; page-vs-carte ; le kit (atomes, carte-aperçu, carte-section, états) ; la fiche (anatomie, ordre, gabarits R6) ; le flux Accueil et ses garde-fous ; la Veille (frontière avec l'Accueil, palier de lecture, pont vers fiche « à vérifier ») ; la recherche (champ intelligent, segments, `diffusionINSEE`) ; favoris/watchlists ; profil ; la doctrine « descriptif, jamais de verdict » ; l'accessibilité (doc 06, dont les `aria-*` sont natifs au web). Les composants se **mappent en Razor** (l'export HTML de Claude Design s'y transpose plus directement qu'en XAML).

---

## 7. Ce que ce document ne tranche pas (encore)

- **Pages publiques / SEO** : hors périmètre v1 (app authentifiée seule). Si un jour landing/marketing → rouvrir le choix WASM-pur vs modèle unifié (le SSR redeviendrait utile) ; ce serait un avenant à l'ADR-017.
- ~~Valeurs exactes des points de rupture web~~ → **tranchées** : **640 px** et **880 px** (cf. §3, « Les 3 formats »).
- **PWA / hors-ligne** (installable, cache) : non traité ; à évaluer selon le besoin.
- **Maquettage** des écrans web (rail + deux panneaux) dans Claude Design — l'export HTML est ici un atout direct.

---

## Renvois

| Sujet | Référence |
|---|---|
| Doctrine UX commune (tronc) | `docs/12-modele-ux-client-maui.md` |
| Décision framework web | `ADR-017` (à amender : acter **WASM pur** pour l'app authentifiée) |
| Topologie client pur de l'API | `ADR-002` |
| Accessibilité (exigence bloquante) | `docs/06-accessibilite.md`, ADR-008 |
| Auth (cookie refresh, JWT) | `docs/11-api-endpoints.md` (`/auth/*`), ADR-010 |
| Placement `Atlas.Web` dans la solution | `docs/10-layout-solution-dotnet.md` |
| Maquettes (3 formats : téléphone / intermédiaire / full) | [`docs/design/`](design/README.md) |
| Clients natifs (mobile MAUI, desktop Avalonia) | `ADR-026` |

> **À faire à l'intégration** :
> - **Amender `ADR-017`** pour acter **WASM pur** dans le cas « app authentifiée uniquement » (en conséquence de ce doc), en gardant le modèle unifié comme option si des pages publiques apparaissent un jour.
> - Référencer ce doc 14 depuis `CLAUDE.md` (ligne « interface utilisateur ») à côté de doc 12 et doc 06.

---

*Document figé le 30 mai 2026 ; amendé le 1ᵉʳ juin 2026 (§3 « Les 3 formats responsive » : paliers téléphone / intermédiaire / full, breakpoints 640 / 880 px figés, mapping largeur × plateforme). Delta web de la doctrine UX (doc 12, agnostique et faisant référence pour le tronc commun). Décisions propres au web : app authentifiée seule → WASM pur ; mêmes 5 destinations en rail latéral ; **mise en page desktop en 3 patrons** (list-detail / document centré / tableau de bord) sous une règle de largeur hybride (texte plafonné, listes pleine largeur) ; couche routing/URLs qui matérialise les garde-fous doctrinaux (route protégée, diffusion restreinte, deep-link dégradé).*
