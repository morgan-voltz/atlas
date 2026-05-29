# Modèle UX & navigation — client MAUI

> Doctrine d'expérience utilisateur du client **Atlas.Maui** (mobile *et* desktop, projet unique multi-cible).
> Ce document fixe **les règles d'UX et l'architecture de navigation**. Il ne décrit pas le langage visuel (cf. `themes.json` et la future charte d'app) ni le détail de l'accessibilité (cf. `docs/06-accessibilite.md`, exigence bloquante ADR-008).
>
> **Version** : 1.0 — **Date** : 29 mai 2026 — **Portée** : `Atlas.Maui` (Android, iOS, Windows, macOS).

---

## Pourquoi ce document

L'app MAUI va grossir (favoris, watchlists, veille, vue 360, portefeuille PI…). Sans règles posées **avant** l'accumulation, chaque nouvel écran se décide au cas par cas et l'app dérive. Ce mémo donne le « nord » qui rend les décisions suivantes quasi-automatiques et défendables : à chaque arbitrage, on se réfère à une règle, pas à une opinion.

Le fil rouge unique : **un seul modèle mental, deux densités.** Mobile et desktop partagent les mêmes concepts, le même vocabulaire et la même architecture d'information ; seules la *densité* et la *disposition* changent — jamais les concepts.

---

## 1. Les 6 règles du modèle adaptatif

### R1 — Un seul modèle mental, deux densités
Les concepts, le vocabulaire et l'architecture d'information sont identiques sur tous les formats. Ce qui change est la densité et l'agencement, jamais le sens.
*Pourquoi.* L'usage multi-device est une promesse du produit (ADR-001 : synchronisation multi-device). Un utilisateur qui passe du desktop au mobile doit se retrouver immédiatement, pas réapprendre l'app.

### R2 — On adapte à la largeur, pas à la plateforme
Le déclencheur de réagencement est la **largeur disponible**, pas l'OS. Un mobile en paysage, une tablette, une fenêtre desktop réduite ou un split-screen sont traités par la place dont ils disposent.
*Pourquoi.* Moins de cas particuliers, plus de robustesse. En pratique : 1 à 2 points de rupture (à calibrer), qui font passer de 1 → 2 → multi-panneaux.

### R3 — Le list-detail est l'épine dorsale
Toute relation « une liste → un élément » suit le même patron : côte à côte s'il y a la place, empilé (drill-down) sinon.
*Pourquoi.* Un seul patron maîtrisé couvre *recherche → fiche*, *favoris → fiche*, *watchlist → entité*, *flux → item*. Le patron est **récursif** : une carte peut elle-même ouvrir une page détail (cf. §2).

### R4 — Mobile = focus & pouce ; desktop = densité & clavier
Mobile : une tâche par écran, cibles tactiles ≥ 44 px, actions principales atteignables au pouce. Desktop : plus d'information visible d'un coup, raccourcis clavier, états au survol.
*Pourquoi.* Cela épouse les contextes des personas : consultation en rendez-vous / mobilité (avocat, expert-comptable) vs travail de fond au poste (investisseur, compliance).

### R5 — On dégrade par divulgation progressive, jamais par amputation
Quand la place manque, on replie, on empile, on met en sections — on ne **supprime** pas l'information.
*Pourquoi.* L'utilisateur est un professionnel qui engage sa responsabilité ; il ne doit jamais « perdre » une donnée selon le terminal qu'il tient.

### R6 — Pertinence avant exhaustivité (*less is more*)
Par défaut, on met en avant ce qui compte pour la tâche de cet utilisateur ; le reste reste **à portée**, pas absent. Deux mécanismes : des **défauts intelligents** (sections pertinentes selon le persona / les watchlists) et du **contrôle utilisateur** (sections repliables, épinglables, masquables, mémorisées d'un device à l'autre).
*Pourquoi.* Le besoin diffère par persona (la compliance ignore les classes de Nice, le cabinet PI ignore les marchés publics). Réduit aussi la surcharge cognitive — c'est de l'accessibilité (cf. `docs/06-accessibilite.md`).

> **Articulation R5 ↔ R6 — la règle qui les réconcilie : masquer ≠ amputer.**
> R5 traite la *contrainte* (l'écran est petit → on replie, tout reste atteignable). R6 traite le *choix* (la donnée n'intéresse pas cet utilisateur → on ne l'affiche pas par défaut, mais c'est signalé et à un geste).
> Garde-fous non négociables :
> 1. Masqué = **signalé + réversible** (ex. « +4 sections masquées », un tap pour ouvrir). Jamais d'information supprimée ni inatteignable.
> 2. **Provenance, date et caveats de couverture ne se masquent jamais.** Ils sont hors périmètre du *less* — c'est le socle de confiance.
> 3. C'est une **préférence, pas une vérité** : ce qu'on cache pour l'un, on le montre pour l'autre, honnêtement.

---

## 2. Page vs carte

La différence est un **coût** que l'on choisit, pas que l'on supprime.

| | **Page** | **Carte** |
|---|---|---|
| Nature | Destination vers laquelle on **navigue** | Bloc de contenu qu'on **fait défiler** |
| Entre dans la pile de retour | Oui | Non |
| Coût payé | Un tap + un changement de contexte | De la longueur de défilement / du scan |
| Quand l'utiliser | Une **tâche ou un approfondissement autonome** vers lequel on va intentionnellement | Une **facette** de l'objet courant qu'on veut scanner avec les autres |

**Règle de tri.** Facette à scanner d'un coup d'œil → carte. Tâche/approfondissement qui tient debout seul → page.

**Insight récursif.** *carte → page* n'est que R3 répété : la carte est l'aperçu, la page est le détail. L'app est donc récursive de bout en bout : *liste → fiche → carte → page détail*.

---

## 3. Architecture de navigation — 5 destinations, plafond inclus

La barre de navigation (onglets sur mobile, rail/flyout sur desktop via MAUI Shell) contient un jeu **fixe et restreint** de destinations.

| Destination | Job de l'utilisateur | Nature |
|---|---|---|
| **Accueil** | *Ce que MES entités ont fait* — flux d'actualité personnalisé (mouvements RNE/BODACC… sur mes favoris/watchlists) | Surveillance, *push* |
| **Recherche** | Trouver une entité ; point d'entrée vers la fiche | Action ponctuelle |
| **Veille** | *Ce qui se passe dans MON domaine* — packs RSS, actualité sectorielle et réglementaire | Lecture de fond, exploratoire |
| **Favoris / Watchlists** | *Qui je surveille* — la gestion ; c'est ici qu'on curate ce que l'Accueil affichera | Gestion |
| **Profil** | Compte, session (login/**logout**), choix du **thème**, préférences d'accessibilité | Réglages |

**Frontière à garder nette — Accueil vs Veille.** Le discriminateur : *« est-ce à propos d'une entité que je suis explicitement ? »* — oui → **Accueil** ; non, c'est de la lecture de fond → **Veille**. Exemple : un BODACC sur un client suivi va à l'Accueil ; un article Légifrance va à la Veille. Accueil et Favoris sont les deux faces d'une même pièce (*ce qu'ils ont fait* vs *qui je surveille*).

**Garde-fou de plafond.** Cinq destinations = le **maximum** d'une barre d'onglets mobile. C'est la ligne qu'on **ne franchit plus**. Toute future feature vit *dans* une de ces cinq, atteinte par drill-down — jamais en sixième onglet. (La *vue 360 / dossier cible* n'est pas un onglet : c'est une fiche enrichie atteinte depuis une entité.)

---

## 4. La fiche entreprise

La fiche est **une seule page qui défile, composée de cartes repliables** — pas un hub de sous-pages.
*Pourquoi.* L'entreprise reste **un seul objet mental** (cohérent avec R1 et la promesse multi-device), et c'est le support naturel de R6 (cartes pertinentes en haut, le reste replié-mais-signalé).

Les **approfondissements** qui sont de vraies tâches autonomes partent en **pages détail** : bilan complet, graphe de co-mandats des dirigeants, notice d'une marque. La ligne **provenance · date** reste épinglée et ne se replie jamais (garde-fou R6).

---

## 5. Le flux Accueil (Actu)

Un flux agrégé des mouvements des entités suivies (favoris, watchlists, flux suivis). C'est un list-detail (R3) : **carte « light »** (qui ? quoi a changé ? quand ? source) → tap → **détail** (fiche ou page concernée). C'est la surface transverse que tous les personas réclament : *« qu'est-ce qui a changé depuis ma dernière visite »*.

L'inspiration « réseau social » est utile pour le *scan rapide*, mais quatre réflexes sont à désamorcer car ils trahiraient la doctrine d'Atlas :

1. **Tri transparent, pas algorithme d'engagement.** Chronologique (ou pertinence explicite) par défaut, filtres visibles, aucun « pour toi » opaque. Un professionnel doit savoir *pourquoi* un item est là et être sûr que rien d'important n'est caché.
2. **Carte = fait, jamais verdict.** « Nouveau dépôt BODACC », « dirigeant ajouté » — daté et sourcé. Jamais « entreprise à risque ». (Cohérent ADR-012, descriptif sans qualification ; les couleurs `warning`/`error` de `themes.json` servent aux états système, pas à juger une entité.)
3. **Accessibilité — déjà spécifiée.** Le flux suit `docs/06-accessibilite.md` §11.5 : mise à jour silencieuse + indicateur « X nouveaux » à activer manuellement, sémantique feed/article, navigation clavier, badges de source jamais distingués par la couleur seule, **mode digest** par défaut pour qui a déclaré un besoin d'accessibilité.
4. **« Je suis à jour » plutôt que scroll infini.** Marqueur de fin (« tu as tout vu »), état lu/non-lu, option digest. On sert la *clôture* du professionnel au lieu de le capturer.

---

## 6. Kit de composants

Tout écran se compose d'un **kit fini** — jamais de composants uniques au cas par cas. *Pourquoi* : cohérence (R1), accessibilité réglée **une seule fois** par composant, et maquettage tractable. Trois couches, façon *atomic design*.

**Atomes** (pièces réutilisables vivant *dans* les cartes) :
- **Champ étiqueté** — `étiquette → valeur`, l'unité de base de la fiche.
- **Badge** — type, statut ou source. **Toujours descriptif, jamais un verdict ; jamais distingué par la seule couleur** (porte son texte — cf. `docs/06-accessibilite.md`).
- **Ligne de provenance** — `source · date`. Atome de confiance, présent partout où une donnée est affichée, **jamais masqué** (garde-fou R6).
- **Chiffre clé** — un nombre + son étiquette + sa provenance. Descriptif uniquement (ADR-012).

**Cartes** (conteneurs bornés) :
- **Carte-section** — facette repliable de la fiche (cf. §4).
- **Carte-aperçu** — ligne « light » des listes et du flux (spec en §7).

**États** (composants à part entière) : **chargement** (squelette calqué sur l'anatomie, pour éviter les sauts de mise en page), **vide**, **erreur**, **« à jour / tout vu »** (flux). L'état **vide** du flux Accueil amorce la boucle d'onboarding : *chercher → suivre → le flux se remplit*.

---

## 7. Composant : carte-aperçu (spec)

Le composant le plus réutilisé : une ligne compacte et tappable représentant une **entité** ou un **événement** sur une entité, toujours point d'entrée vers un détail (R3). Réutilisé tel quel sur cinq surfaces : résultats de recherche, favoris, lignes de watchlist, événements du flux Accueil, items de veille.

### Anatomie (slots)

| Slot | Présence | Contenu |
|---|---|---|
| Zone tactile | requis | Toute la ligne (≥ 44×44) |
| Indicateur de type | optionnel | Marqueur entité (entreprise / marque) ou événement |
| Ligne primaire | requis | Nom de l'entité concernée — 1 ligne, ellipsis |
| Ligne secondaire | requis | Contexte — 1 ligne, muette |
| Badge(s) | optionnel | Source et/ou type — max ~2 |
| Chevron de drill-down | requis si navigable | Affordance « ouvre le détail » |
| Pastille non-lu | variante événement | Combinée à la graisse, jamais la couleur seule |

### Deux variantes, une anatomie
- **Aperçu-entité** (recherche, favoris, watchlist) — primaire : *nom* ; secondaire : `SIREN · forme` (ou `déposant · classes` pour une marque).
- **Aperçu-événement** (flux Accueil, veille) — primaire : *entité concernée* ; secondaire : `libellé d'événement · date` ; badge : *source*.

### Règles de contenu
- SIREN groupé `552 032 534`, en police mono.
- Date relative puis absolue, **sans créer d'urgence** (accessibilité cognitive, doc 06).
- Libellé d'événement **clair, sans jargon** (« Nouveau dépôt BODACC », pas un code brut — doc 06 §11.5).

### États
Défaut · survol (desktop) · pressé (mobile) · **focus visible** (anneau, pas une simple couleur — doc 06 §4.7) · **sélectionné** (cas list-detail desktop : la ligne reste mise en avant tant que son détail est affiché) · **lu / non-lu** (variante événement) · **chargement** (squelette).

### Comportement
- Toute la ligne est la cible, et **la destination est une donnée de l'item** : chaque aperçu sait où il pointe (section de fiche, page détail, ou source).
- Actions secondaires (favori, lu, archiver) : boutons explicites distincts du tap de navigation, **toujours doublés d'une alternative clavier** (Espace = lu/non-lu, F = favori, A = archiver — doc 06 §11.5). Sur mobile, le swipe est un raccourci, jamais l'unique moyen.

### Responsive (R2/R4)
Mobile : pleine largeur, ≥ 44 px, actions au swipe / appui long / overflow. Desktop : plus dense dans le panneau-liste, actions au survol, sélection persistante, navigation aux flèches. Réagencement par la largeur, pas par l'OS.

### Contrat d'accessibilité (doc 06)
- Chaque ligne = un `article` au sein d'un `feed` / `CollectionView`.
- `SemanticProperties.Description` composant *entité + événement + date + source* dans un ordre de lecture logique.
- Cible ≥ 44×44 ; focus visible ; aucune information par la **couleur seule**.

### Garde-fous doctrine
Source présente sur tout événement (confiance) ; libellé = **fait, jamais verdict** ; badge neutre ; aucun signal d'engagement (« tendance », compteurs de popularité) sur l'item.

### Hors-périmètre
Dès qu'il faut plus de 2 lignes + badges, ce n'est plus une carte-aperçu : on descend dans la fiche (carte-section / page détail).

---

## 8. Composant : carte-section (spec)

Facette **repliable** de la fiche : un conteneur thématique (Identité, Dirigeants, Bilans, BODACC…) qui regroupe des atomes (champs étiquetés, badges, chiffres clés, provenance). Composant de structuration de la fiche (§4) ; pour une ligne de liste, c'est la carte-aperçu (§7).

### Anatomie (slots)

| Slot | Présence | Contenu |
|---|---|---|
| En-tête | requis | Titre de section + affordances |
| Épingle | optionnel | Remonte / maintient ouverte la section |
| Chevron replier/déployer | requis | Affordance d'ouverture |
| Indicateur | optionnel | Compteur, source globale |
| Corps | visible si déployé | Atomes ; éventuellement des cartes-aperçu en sous-liste |
| Lien d'approfondissement | optionnel | Vers une page détail (R3) |
| Ligne de provenance | requise si données sourcées | `source · date`, dans le corps, avec les données |

### États
Déployée · **repliée** · repliée-par-pertinence (R6, signalée) · **épinglée** · **vide-de-couverture** (cf. §9) · chargement (squelette de section) · **erreur locale**.

### Comportement
- L'en-tête entier replie/déploie (cible ≥ 44). L'épingle est une action secondaire (bouton + alternative clavier).
- L'état déployé/replié **et** l'épinglage **persistent par utilisateur, multi-device** (ADR-001).
- Le lien d'approfondissement ouvre une page détail, jamais une section qui enfle.

### Responsive (R2/R4)
Mobile : pleine largeur, repli par défaut plus marqué (R5/R6). Desktop : dans le panneau de droite du list-detail, plusieurs sections visibles d'un coup (densité), repli disponible mais moins nécessaire.

### Contrat d'accessibilité (doc 06)
En-tête = `HeadingLevel` (la fiche se lit comme un document, §4.1) ; l'état replié/déployé est annoncé ; l'épingle porte Description + Hint.

### Garde-fous doctrine
- **Jamais de donnée sans sa provenance** : la ligne `source · date` vit dans le corps, avec les données — replier cache l'une *et* l'autre, jamais l'une sans l'autre.
- **Erreur locale, pas globale** : une fiche compose plusieurs sources ; si l'une échoue, **sa** section affiche l'erreur (avec réessai) sans faire tomber le reste.
- Chiffres **descriptifs**, jamais de jauge rouge/verte (ADR-012).

### Hors-périmètre
Si le contenu devient une tâche autonome → page détail, pas une section géante.

---

## 9. Composants : les états (spec)

Quatre composants transverses, réutilisés par les listes, le flux, les sections et les pages. **Règle commune** : `danger`/`success` n'y servent qu'aux **états du système** (réseau, UI) — jamais à qualifier une entité (ADR-012).

### Chargement (squelette)
Occupe l'espace avec la *forme* du contenu à venir (aucun saut de mise en page) ; **calque l'anatomie** du composant remplacé ; plat, animation discrète respectant `prefers-reduced-motion` ; annonce « Chargement… » sans spammer le lecteur d'écran.

### Vide — deux familles à ne pas confondre
- **Vide d'onboarding** (flux d'un nouveau compte, watchlist sans entité) : message + **action d'amorçage** (« Cherche une entreprise pour la suivre »). C'est ici que le compte se matérialise en boucle d'usage.
- **Vide de couverture** (section sans donnée : comptes confidentiels, aucune marque) : message **honnête nommant la raison** (caveat de couverture), **sans** bouton — c'est un fait, pas une action.
- Distinction doctrinale : l'un **invite à agir**, l'autre **informe d'une limite**. Jamais d'urgence ni de culpabilisation.

### Erreur
Message **clair et non technique** (« Source RNE injoignable pour l'instant », pas un code brut) + **Réessayer** ; **portée locale** quand une section échoue ; jamais d'information par la couleur seule (icône **+** texte) ; **aucun détail sensible** dans le message (cf. `CLAUDE.md` : jamais de credentials/tokens exposés).

### « À jour / tout vu »
Marque la **fin** d'un flux — la clôture qui désamorce le scroll infini. Repère discret en fin de liste + date de dernière mise à jour, option digest rappelée. La fin est une **bonne nouvelle**, pas un manque.

### Contrat d'accessibilité (doc 06)
Chaque état est **annoncé** (chargement, erreur, vide, fin de flux) ; le focus est géré (il ne se perd pas ; sur erreur il va au message/bouton) ; aucune information par la seule couleur.

---

## 10. Implications techniques MAUI

- **Navigation** : MAUI `Shell` — onglets (mobile) / flyout ou rail (desktop). Les 5 destinations de §3 sont les routes de premier niveau.
- **Réagencement** : `VisualStateManager` + `AdaptiveTrigger` (sur la largeur de fenêtre, R2), `OnIdiom` pour les ajustements Phone/Desktop. Composants partagés, disposition variable.
- **Contrainte d'architecture** : `Atlas.Maui` ne référence que `Atlas.Domain` et `Atlas.Shared` ; toute donnée transite par l'API HTTP via `AtlasApiClient` (cf. `CLAUDE.md`, règles d'or de dépendance). Aucune logique métier côté client.
- **Session** : JWT en SecureStorage (Keychain/Keystore), refresh transparent sur 401 — l'expiration de session ne doit interrompre que lorsqu'une ré-authentification dure est nécessaire.

---

## 11. Ce que ce document ne tranche pas (encore)

Honnêteté de périmètre, à instruire plus tard :

- **Langage visuel détaillé** : `themes.json` pose déjà un système de tokens sémantiques (light/dark + 7 thèmes dont 2 a11y). La marque n'a volontairement **pas de couleur propriétaire** — la rigueur est dans la structure, la couleur appartient à l'utilisateur. Une charte d'app dédiée reste à formaliser (sort/non de l'or décoratif du print, etc.).
- **Valeurs exactes des points de rupture** (R2).
- **Composition de la vue 360 / dossier cible** : méta-feature d'assemblage, terrain n°1 de R6 (un même dossier qui se recompose selon le persona).

---

## Renvois

| Sujet | Référence |
|---|---|
| Accessibilité (exigence bloquante, DoD) | `docs/06-accessibilite.md`, ADR-008 |
| Accessibilité spécifique au flux/timeline | `docs/06-accessibilite.md` §11.5 |
| Multi-device / SaaS (justifie R1) | ADR-001 |
| Doctrine descriptive (pas de verdict) | ADR-012 |
| Sources de veille, packs par persona | `docs/07-flux-rss-veille.md` |
| Système de thèmes / tokens | `themes.json` |
| Features liées (clients, flux, favoris) | F-009/F-010, F-047, F-053, F-017/F-019, F-055, F-048 |
| Placement du code MAUI | `CLAUDE.md`, `docs/09-architecture-detaillee.md` |

> Candidat à être référencé depuis le tableau « Documents fondateurs » de `CLAUDE.md` (ligne « Toute interface utilisateur (MAUI) »), en complément de `docs/06-accessibilite.md`.

---

*Document figé le 29 mai 2026. Les 6 règles, l'architecture de navigation à 5 destinations et le kit de composants (atomes · cartes · états) constituent la doctrine UX de référence du client MAUI. Toute dérogation doit être justifiée et tracée (ADR dédié si la décision est structurante).*