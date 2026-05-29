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

## 6. Implications techniques MAUI

- **Navigation** : MAUI `Shell` — onglets (mobile) / flyout ou rail (desktop). Les 5 destinations de §3 sont les routes de premier niveau.
- **Réagencement** : `VisualStateManager` + `AdaptiveTrigger` (sur la largeur de fenêtre, R2), `OnIdiom` pour les ajustements Phone/Desktop. Composants partagés, disposition variable.
- **Contrainte d'architecture** : `Atlas.Maui` ne référence que `Atlas.Domain` et `Atlas.Shared` ; toute donnée transite par l'API HTTP via `AtlasApiClient` (cf. `CLAUDE.md`, règles d'or de dépendance). Aucune logique métier côté client.
- **Session** : JWT en SecureStorage (Keychain/Keystore), refresh transparent sur 401 — l'expiration de session ne doit interrompre que lorsqu'une ré-authentification dure est nécessaire.

---

## 7. Ce que ce document ne tranche pas (encore)

Honnêteté de périmètre, à instruire plus tard :

- **Langage visuel détaillé** : `themes.json` pose déjà un système de tokens sémantiques (light/dark + 7 thèmes dont 2 a11y). La marque n'a volontairement **pas de couleur propriétaire** — la rigueur est dans la structure, la couleur appartient à l'utilisateur. Une charte d'app dédiée reste à formaliser (sort/non de l'or décoratif du print, etc.).
- **Valeurs exactes des points de rupture** (R2).
- **Composition de la vue 360 / dossier cible** : méta-feature d'assemblage, terrain n°1 de R6 (un même dossier qui se recompose selon le persona).
- **Taxonomie fine des cartes et composants** (carte section, ligne de liste, carte chiffres, états vide/erreur/chargement) — prochaine étape, et bon point d'entrée pour le maquettage (Claude Design).

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

*Document figé le 29 mai 2026. Les 6 règles et l'architecture de navigation à 5 destinations sont la doctrine UX de référence du client MAUI. Toute dérogation doit être justifiée et tracée (ADR dédié si la décision est structurante).*
