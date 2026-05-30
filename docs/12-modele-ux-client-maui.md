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
| **Profil** | Compte, session (login/**logout**), choix du **thème**, **densité du contenu**, préférences d'accessibilité | Réglages |

**Frontière à garder nette — Accueil vs Veille.** Le discriminateur : *« est-ce à propos d'une entité que je suis explicitement ? »* — oui → **Accueil** ; non, c'est de la lecture de fond → **Veille**. Exemple : un BODACC sur un client suivi va à l'Accueil ; un article Légifrance va à la Veille. Accueil et Favoris sont les deux faces d'une même pièce (*ce qu'ils ont fait* vs *qui je surveille*).

**Garde-fou de plafond.** Cinq destinations = le **maximum** d'une barre d'onglets mobile. C'est la ligne qu'on **ne franchit plus**. Toute future feature vit *dans* une de ces cinq, atteinte par drill-down — jamais en sixième onglet. (La *vue 360 / dossier cible* n'est pas un onglet : c'est une fiche enrichie atteinte depuis une entité.)

---

## 4. La fiche entreprise

La fiche est **une seule page qui défile, composée de cartes repliables** — pas un hub de sous-pages.
*Pourquoi.* L'entreprise reste **un seul objet mental** (cohérent avec R1 et la promesse multi-device), et c'est le support naturel de R6 (cartes pertinentes en haut, le reste replié-mais-signalé).

Les **approfondissements** qui sont de vraies tâches autonomes partent en **pages détail** : bilan complet, graphe de co-mandats des dirigeants, notice d'une marque. La ligne **provenance · date** reste épinglée et ne se replie jamais (garde-fou R6).

### Anatomie de l'écran
Un **en-tête d'identité** toujours visible (nom, SIREN, forme juridique, état administratif, action *suivre*, et l'« à jour » global) → une **pile de cartes-sections** (§13) qui défile → des **liens d'approfondissement** vers les pages détail. Sur desktop, la fiche occupe le panneau de droite du list-detail (résultats à gauche — R3/R4).

### Ordre par défaut des sections
Du plus consulté au plus spécialisé : **Identité → Dirigeants → Bilans/Finances → BODACC/Procédures → Établissements → PI → Marchés publics.** (MVP : les quatre premières ; le reste arrive avec les features.)

### Pertinence (R6) : gabarits + préférences
« Ne pas afficher » recouvre **trois choses distinctes** qu'on ne confond jamais :

| Catégorie | Qui décide | Par défaut | Réversible |
|---|---|---|---|
| **Pas besoin** (pertinence métier) | Le système (via gabarit) | Replié, **signalé** (« +N masquées ») | Oui, 1 tap |
| **Pas envie** (préférence perso) | L'utilisateur (toggle / épingle) | Selon son choix | Oui |
| **Pas disponible** (couverture / doctrine) | Personne — c'est un fait | **Toujours montré** (caveat) | N/A |

**Mécanisme retenu — gabarits pré-réglés + affinage.**
- Le **défaut intelligent** (« pas besoin ») est piloté par des **gabarits de fiche** que l'utilisateur **choisit explicitement** — pas d'auto-détection opaque (cohérent avec le tri transparent du flux, §5). Un gabarit fixe l'**ordre** et les sections **ouvertes par défaut**.
- Les gabarits dérivent directement des **7 personas** (rubrique « Données & sources qui comptent ») : Comptable, Cabinet PI, Compliance, Investisseur, Avocat… Un gabarit **« Général »** est le défaut pour qui ne choisit rien.
- L'utilisateur **affine** par section : un simple **activé/désactivé** (« pas envie ») + l'**épingle** (remonter). Le choix utilisateur **prime toujours** sur le gabarit.
- **Garde-fou (catégorie 3, non négociable) :** le toggle est une préférence *utilisateur* ; il ne devient **jamais** un moyen pour le *système* de cacher un fait de couverture. Une section **affichée** ne supprime jamais son caveat (« Comptes confidentiels », « UBO exclu par doctrine »…). Masquer = choix éclairé de l'utilisateur, **jamais** une omission trompeuse du système (cohérent ADR-012).
- Gabarit, toggles et épingles vivent dans **Profil** et **persistent multi-device** (ADR-001).

> Terme à figer au glossaire (`docs/08-vocabulaire-ubiquitaire.md`) : **« gabarit de fiche »** (vue d'affichage pré-réglée), pour éviter tout synonyme silencieux. Ce mécanisme de recomposition par gabarit est aussi la **graine de la vue 360 / dossier cible** (cf. §16).

---

## 5. Le flux Accueil (Actu)

Un flux agrégé des mouvements des entités suivies (favoris, watchlists, flux suivis). C'est un list-detail (R3) : **carte « light »** (qui ? quoi a changé ? quand ? source) → tap → **détail** (fiche ou page concernée). C'est la surface transverse que tous les personas réclament : *« qu'est-ce qui a changé depuis ma dernière visite »*.

L'inspiration « réseau social » est utile pour le *scan rapide*, mais quatre réflexes sont à désamorcer car ils trahiraient la doctrine d'Atlas :

1. **Tri transparent, pas algorithme d'engagement.** Chronologique (ou pertinence explicite) par défaut, filtres visibles, aucun « pour toi » opaque. Un professionnel doit savoir *pourquoi* un item est là et être sûr que rien d'important n'est caché.
2. **Carte = fait, jamais verdict.** « Nouveau dépôt BODACC », « dirigeant ajouté » — daté et sourcé. Jamais « entreprise à risque ». (Cohérent ADR-012, descriptif sans qualification ; les couleurs `warning`/`error` de `themes.json` servent aux états système, pas à juger une entité.)
3. **Accessibilité — déjà spécifiée.** Le flux suit `docs/06-accessibilite.md` §11.5 : mise à jour silencieuse + indicateur « X nouveaux » à activer manuellement, sémantique feed/article, navigation clavier, badges de source jamais distingués par la couleur seule, **mode digest** par défaut pour qui a déclaré un besoin d'accessibilité.
4. **« Je suis à jour » plutôt que scroll infini.** Marqueur de fin (« tu as tout vu »), état lu/non-lu, option digest. On sert la *clôture* du professionnel au lieu de le capturer.

---

## 6. La Veille

*Ce qui se passe dans MON domaine* — la **lecture de fond** éditoriale (vs l'Accueil = *ce que mes entités suivies ont fait*). C'est le « Twitter de sa veille » : articles RSS des **packs métier** (doc 07) et des **sources libres** (F-043), dédupliqués et triés chronologiquement.

> **Frontière avec l'Accueil — support technique.** Une **même** timeline backend (F-044/F-047) alimente les deux écrans, via des **filtres** différents : l'Accueil borne sur les mouvements d'entités suivies (`mentionsFavoritesOnly`, `kind = FavoriteEvent`), la Veille montre le contenu éditorial (`kind = RssItem`). Discriminateur inchangé (§3) : *à propos d'une entité que je suis explicitement ?* — oui → Accueil ; non, lecture de fond → Veille. **La Veille reste éditoriale ; on n'y rejoue pas les mouvements de favoris.**

### Lire d'abord, gérer ensuite (R6)
Un **flux unique** chronologique occupe l'écran (la lecture, là où l'utilisateur passe son temps). La **gestion des sources** vit dans un **panneau de filtres rétractable** — repliée par défaut, à un geste.
- **Pas de segmentation par pack en onglets permanents** : beaucoup d'utilisateurs n'ont qu'un pack (leur métier). Le **filtre par pack** vit *dans* le panneau (cases à cocher packs + sources), aux côtés de **« Ajouter un flux RSS »** (F-043).
- **Le filtre actif reste visible quand le panneau est replié** (badge « Pack Compta · 23 » — doc 06 §11.5).

### Le flux
- **Carte d'item** = list-detail (R3) : titre, source, date relative+absolue, extrait, états. La **déduplication** (F-045) affiche **« N sources rapportent »** sur un item regroupé.
- **États par item** (F-044) : lu / non-lu, favori, archiver — chacun avec son alternative clavier (Espace = lu, F = favori, A = archiver — doc 06 §11.5).
- **Mise à jour silencieuse + « X nouveaux items · afficher »** (révélation manuelle), comme l'Accueil — jamais d'irruption automatique (doc 06 §11.5).

### Ouvrir un item — palier de lecture
Taper un item ouvre d'abord une **vue lecture interne** (titre + extrait + métadonnées), *puis* un lien sortant **honnête** vers la source (« Lire sur Les Échos ↗ » — l'utilisateur sait qu'il quitte Atlas et vers où). C'est R3 prolongé d'un cran : *item → vue lecture → article externe*. Justifié aussi par les flux qui ne fournissent qu'un extrait.

### Pont vers une fiche (entité mentionnée) — garde-fou
Quand un article mentionne une entreprise connue (`mentionedFavorites`, F-047), la **vue lecture** propose un lien **« cet article mentionne X — voir sa fiche »**.
- **Uniquement dans le détail (la vue lecture), jamais dans la liste** : sinon le flux Veille se transformerait en cartes d'entité et refairait l'Accueil. La liste reste éditoriale.
- **Proposition vérifiable, jamais un verdict** : le matching automatique peut produire des faux positifs (homonymes, mention en passant). L'app **signale une mention**, elle ne **qualifie pas** la pertinence (cohérent ADR-012 et « l'app montre ce qu'elle a compris, l'utilisateur décide »).

### États
Vide d'onboarding (aucune source → « choisis un pack métier pour démarrer »), chargement (squelette d'items), erreur de source (locale : une source injoignable n'efface pas le flux — `veille.fetch_failed`), « à jour / tout vu ». Cf. §14.

### Accessibilité (doc 06 §11.5 — déjà spécifiée en détail)
Timeline = `feed` / `CollectionView`, chaque item = `article` avec heading de titre ; mise à jour silencieuse + indicateur manuel ; navigation clavier (flèches entre items, Tab pour entrer dans le détail) ; filtres actifs visibles + annonce « Filtre appliqué : N résultats » ; libellés sans jargon.

---

## 7. La recherche

Point d'entrée vers la fiche (et vers la marque). C'est avant tout un **assemblage de briques connues** — carte-aperçu, list-detail (R3), états — plus trois décisions propres.

### Champ unique intelligent
Un seul champ de saisie ; l'app **détecte l'intention** : **9 chiffres validés par Luhn → SIREN** ; **sinon → recherche par nom**. Un `552032534` qui échoue à Luhn n'est **pas** présenté comme un SIREN — repli silencieux sur la recherche texte. *Principe* : **l'app montre ce qu'elle a compris** (« SIREN détecté »), elle ne devine jamais en silence.

### Deux états guidés par l'intention
- **SIREN détecté → accès direct.** Pas de segments, pas de liste : une **carte d'accès direct** unique (« Entreprise 552 032 534 — Ouvrir la fiche »). Elle est **proposée (un tap), jamais ouverte automatiquement** — *principe transverse : l'app détecte, l'utilisateur décide ; suggérer n'est pas agir à la place de.* Cela protège de l'erreur de frappe et garde l'humain aux commandes (cohérent avec le tri transparent du flux, §5, et les défauts intelligents de la fiche, §4).
- **Texte → segments + liste réactive.** Des segments **Entreprises | Marques** (univers issus d'API distinctes — RNE vs INPI PI) avec **compteur par segment**. Chaque segment affiche sa propre **provenance** et son propre **caveat de couverture**. Résultats = **carte-aperçu** (variante entité/marque) ; liste → fiche = list-detail (R3).

### Comportement réactif
Recherche **réactive pendant la frappe** (debounce 300 ms, déjà prévu côté client — F-009). États **chargement / vide / erreur** = composants déjà spécifiés (§14). Pagination (par page ou curseur `searchAfter`) via « charger plus » / défilement.

### Garde-fou de couverture (catégorie 3)
Le flag **`diffusionINSEE = "N"`** (diffusion restreinte) est honoré **dès les résultats** : la ligne indique « Diffusion restreinte (INSEE) » + badge `restreint`, sans masquer l'existence de l'entité. C'est le même principe que « Comptes confidentiels » sur la fiche (§4) : un fait de couverture ne se cache jamais.

### Accessibilité (doc 06)
Le champ annonce l'intention détectée (SIREN vs nom) ; les segments sont un groupe d'onglets navigable au clavier ; la liste réactive met à jour **sans voler le focus** ni interrompre la frappe (même esprit que la mise à jour silencieuse du flux, doc 06 §11.5) ; chaque résultat = carte-aperçu avec son contrat d'accessibilité (§12).

### Troisième mode — « Vérifier un nom » (rapport de présence multi-sources)

> **Statut : 🔵 Proposé / futur — non implémenté.** Contrairement au reste de ce document (qui décrit de l'existant), cette sous-section conçoit une feature à venir. Elle introduit une **source externe nouvelle** (WHOIS/DNS) absente du catalogue actuel (`docs/03-catalogue-apis-publiques.md`). À tracer en fiche feature dédiée avant implémentation.

Un **sélecteur** en haut de la Recherche ajoute un mode : **« Rechercher » | « Vérifier un nom »**. Même onglet, intention différente (la Recherche devient le point d'entrée pour *interroger un nom* — soit pour trouver, soit pour vérifier). Respecte le **plafond à 5 onglets** (§3) : aucun nouvel onglet.

**Ce que c'est — et ce que ce n'est pas.** Un **rapport de présence multi-sources** : pour un nom saisi, l'app interroge plusieurs registres et **rapporte des faits, sourcés et datés**. Ce n'est **pas** un verdict de disponibilité.
- **Marque (INPI PI)** : nombre de marques au libellé identique/proche + classes.
- **Dénomination (RNE)** : entreprises portant ce nom.
- **Domaines web (WHOIS)** : état d'enregistrement par extension (.fr/.com/.eu…).
- **Noms proches** : pistes de similarité présentées **à vérifier**, jamais comme un score de risque.

**Garde-fous doctrine (le cas le plus sensible de l'app).**
1. **Faits, jamais verdict.** « 2 marques proches trouvées », « .eu non enregistré » — oui. « Disponible » / « libre » — **jamais**. Un faux « disponible » induirait une décision juridique à fort enjeu : c'est précisément ce qu'Atlas, outil **descriptif** (ADR-012), refuse. Une note explicite le dit : *Atlas rapporte ce qu'il a trouvé ; il ne conclut pas à la disponibilité juridique — cela relève d'un conseil en propriété industrielle.*
2. **Trois sources = trois provenances distinctes**, jamais fondues en un seul indicateur. Chaque carte porte sa source + date + périmètre.
3. **Similarité = suggestion à vérifier** (même principe que les mentions d'articles en Veille, §6), pas une affirmation de risque de confusion.
4. **Couleurs neutres** : un domaine enregistré ou une marque existante est un **fait**, pas un « mauvais » résultat — pas de rouge/vert de jugement.

**Réutilisation** : chaque carte-source réutilise les atomes (provenance, badge neutre) ; les états chargement/erreur sont par source (erreur locale, §14) — une source injoignable n'efface pas les autres.

---

## 8. Favoris / Watchlists

*Qui je surveille* — la gestion du portefeuille (vs l'Accueil = *ce qu'ils ont fait*). Écran neutre : **pas d'indicateur de mouvement par entité** (ce job appartient à l'Accueil ; le dupliquer ici brouillerait la frontière et risquerait des compteurs divergents). Repose sur **F-017** (favoris = set surveillé), **F-053** (watchlists) et **F-030** (tags).

### Modèle mental — hybride « Tous + listes »
L'écran est **agnostique du modèle de données** (Modèle A favoris-plats ou B liste-par-défaut) : dans les deux cas l'utilisateur voit *un ensemble surveillé + des listes nommées*. En-tête = un sélecteur horizontal **« Tous · N » + une puce par liste** (« Concurrence · 5 »…) + **« + Liste »**. « Tous » est sélectionné par défaut.
*Pourquoi l'hybride.* Sert les deux mentalités sans en imposer une : peu de favoris → on voit直接 ses entreprises ; portefeuille organisé → on ouvre un dossier. C'est R3 (les listes filtrent, on descend dans l'entité) et R6 (l'essentiel d'abord, le reste à portée).

### Contenu & divulgation contextuelle (R6)
- **Corps** = **cartes-aperçu** (variante entité, §12), neutres. Aucun composant neuf.
- **Tags (F-030)** : une rangée de filtres **n'apparaît que dans une liste** (pas sur « Tous »). Un tag n'est **jamais** identifié par la couleur seule — libellé + pastille d'appoint (doc 06).
- **Timeline par liste** : sur une liste sélectionnée, un accès **« Voir l'activité de cette liste »** ouvre une timeline filtrée (`watchlistId`, au-dessus de F-047). C'est l'**angle différenciant** (vs Pappers) ; distinct de l'Accueil global. N'apparaît, lui aussi, que dans le contexte d'une liste.
- *Principe* : les fonctions avancées (tags, timeline-par-liste) ne se montrent **que dans leur contexte** — R6 appliqué à l'écran lui-même.

### Actions de gestion
Par carte : **swipe (raccourci) + kebab ⋮ (filet visible et accessible)** — jamais le swipe seul (cohérent §12 et doc 06 : voie clavier obligatoire). Actions : retirer du suivi, ajouter/déplacer vers une liste. Le kebab donne un point d'entrée d'actions **identique mobile et desktop**.

### Création & import en masse
Le menu **« + Liste »** propose : *créer une liste vide* / *créer depuis un import de SIREN*. L'import (coller / CSV, jusqu'à plusieurs centaines) est un **job asynchrone** (Hangfire, patron F-014) → il faut un **état « import en cours »** et un **rapport de fin** (N ajoutés / M doublons / K invalides), lisible et pas seulement visuel (doc 06). Mini-parcours à part, non détaillé ici (cf. §16).

### États
- **Vide global** (aucun favori) : onboarding « cherche une entreprise pour la suivre » (cf. §14).
- **Vide de liste** : « cette liste est vide → importer / ajouter ».
- **Import en cours / rapport d'import** : voir ci-dessus.

### Accessibilité (doc 06)
Sélecteur de listes = groupe navigable au clavier, sélection annoncée ; tags jamais par la couleur seule ; cartes et actions atteignables au clavier (le kebab porte l'alternative non-swipe) ; rapport d'import lisible par lecteur d'écran.

---

## 9. Profil

Réglages et compte. **Hub** (menu → sous-pages), *pas* une longue page : les domaines sont hétérogènes et certains **sensibles** (mot de passe, INPI, suppression) — un hub évite de tomber par accident sur du sensible au milieu du cosmétique. (Inverse assumé de la fiche §4, qui est un seul objet mental → une page ; ici, des domaines distincts → des pages.)

### Organisation — 5 entrées en 2 familles + déconnexion
Regroupement **par nature de la donnée / conséquence**, pas par ressemblance :

**Mon compte** (sensible)
- **Compte** : email, mot de passe, **2FA** (F-002, codes de secours). Toute modification exige le mot de passe actuel.
- **Connexion INPI** : page dédiée (F-003) — composant **vital** (sans lui, aucune donnée métier). Affiche **statut + date de test + tester/déconnecter**. Garde-fou absolu : `InpiCredentials` **jamais affiché en clair, jamais retourné par l'API** (vocabulaire doc 08). Trois états : connecté / invalide (reconnecter, sans dramatiser) / non connecté (onboarding).
- **Données & confidentialité** : **export** RGPD (art. 20, JSON) en haut = action bénigne ; **suppression de compte** (art. 17) en **zone danger isolée** en bas, confirmation forte (re-saisie). Le texte **dit la vérité** : effacement sous 30 j, mais données de facturation conservées et **anonymisées** par obligation légale — jamais « tout est effacé » (cohérent avec l'honnêteté des caveats partout ailleurs).

**Application** (confort / affichage)
- **Paramètres généraux** : thème (7 thèmes `themes.json`), **densité** (§8/§5), langue, notifications (push/email par catégorie).
- **Affichage & données** : c'est le **mode « personnaliser »** de la fiche (§4) — gabarit par défaut, sections masquées (R6), préférences d'accessibilité.

**Se déconnecter** : action isolée en bas, teinte `danger` (état système, pas verdict). **Confirmation requise** car non trivialement réversible (re-saisie email + mot de passe pour revenir). Le message dit ce qui se passe et rassure : les credentials INPI restent chiffrés côté serveur.

### Notes transverses
- Le statut INPI (« Connecté ») est visible **dès le hub** — l'info vitale ne demande pas d'entrer.
- Le `success`/`danger` employés ici qualifient des **états du système** (connexion OK, action destructive), jamais une entité (ADR-012).
- Le hub porte 6 lignes : confortable pour un écran de réglages (≠ barre d'onglets plafonnée à 5), mais c'est le maximum — pas de 6e famille à la légère.
- **Plomberie d'auth** déjà en place : access token court (15 min) + refresh rotatif (cf. §15) ; la déconnexion révoque la session.

---

## 10. Authentification & entrée dans l'app

> Écrans rencontrés **avant** la barre de navigation. Placés ici par commodité de lecture (les écrans applicatifs §4–§9 d'abord), mais ce sont chronologiquement les **premiers** que voit l'utilisateur.

Le parcours réel n'a pas 2 mais jusqu'à 4 moments (backend `/auth/*`, ADR-010), plus le **double palier** propre à Atlas : *se connecter à l'app ne suffit pas — il faut aussi lier son compte INPI* (F-003) pour accéder aux données.

**Parcours complet** : Ouverture (choix) → **Connexion** (→ défi 2FA si activé) **ou** **Création** (→ vérification email → proposition INPI zappable) → app (en mode dégradé honnête si INPI non lié).

### Ouverture — écran de choix
Marque Atlas + positionnement, puis deux portes nettes : **Se connecter** (primaire) / **Créer un compte** (secondaire). L'intention est déclarée d'emblée.

### Connexion
Email + mot de passe ; « Mot de passe oublié ? » ; renvoi vers la création.
- **Message d'erreur sûr mais utile** : `invalid_credentials` → « identifiant ou mot de passe incorrect » (ne jamais révéler *lequel* est faux, ni qu'un email existe). `email_not_verified` → message **spécifique et actionnable** (renvoyer le lien). `account_locked` → expliquer le verrouillage temporaire sans dramatiser.

### Création de compte
Email professionnel + mot de passe (règle visible). **Annonce honnête du parcours** dès l'inscription : *« un email de vérification sera envoyé ; la connexion INPI te sera proposée ensuite (tu pourras la faire plus tard) »* — aucun palier caché.

### Vérification d'email (`PendingEmailVerification`)
État d'attente, jamais un cul-de-sac : **« Renvoyer le lien »** (avec throttle anti-spam), **« Modifier »** l'adresse, rappel spam. C'est le maillon où l'on perd des gens sans porte de sortie.

### Défi 2FA (`twoFactorRequired` → `/auth/2fa/verify`)
Saisie du code TOTP 6 chiffres (clavier numérique, focus auto) **et** lien **« Utiliser un code de secours »** (les 10 codes one-time) — sans quoi un téléphone perdu = verrouillage dehors. Erreur sobre, pas de reverrouillage brutal.

### Proposition INPI — le maillon-clé (choix « proposer mais zapper »)
Après vérif email : explique le palier (*« Atlas interroge l'INPI en ton nom ; tes identifiants restent chiffrés et ne sont jamais affichés »*), bouton **« Connecter mon compte INPI »**, et lien **« Plus tard » explicite** (jamais caché — *l'app guide, l'utilisateur décide*).
- **Garde-fou (à figer) : zapper INPI ne casse pas l'app, ça la met en mode dégradé explicite.** Sans INPI, recherche et fiches sont indisponibles → l'app l'**explique** (bandeau calme persistant + états vides « lie ton compte INPI ») au lieu de planter sur `inpi.not_connected`. C'est notre doctrine « état vide = fait + action » appliquée à l'absence d'INPI. Liable à tout moment depuis Profil (§9).

### Doctrine transverse
Le bleu est **marque/action**, jamais un verdict. L'app **annonce le parcours** (pas de palier ni d'étape caché). Les credentials (mot de passe app, INPI) ne sont jamais affichés en clair.

---

## 11. Kit de composants

Tout écran se compose d'un **kit fini** — jamais de composants uniques au cas par cas. *Pourquoi* : cohérence (R1), accessibilité réglée **une seule fois** par composant, et maquettage tractable. Trois couches, façon *atomic design*.

**Atomes** (pièces réutilisables vivant *dans* les cartes) :
- **Champ étiqueté** — `étiquette → valeur`, l'unité de base de la fiche.
- **Badge** — type, statut ou source. **Toujours descriptif, jamais un verdict ; jamais distingué par la seule couleur** (porte son texte — cf. `docs/06-accessibilite.md`).
- **Ligne de provenance** — `source · date`. Atome de confiance, présent partout où une donnée est affichée, **jamais masqué** (garde-fou R6).
- **Chiffre clé** — un nombre + son étiquette + sa provenance. Descriptif uniquement (ADR-012).

**Cartes** (conteneurs bornés) :
- **Carte-section** — facette repliable de la fiche (cf. §4).
- **Carte-aperçu** — ligne « light » des listes et du flux (spec en §12).

**États** (composants à part entière) : **chargement** (squelette calqué sur l'anatomie, pour éviter les sauts de mise en page), **vide**, **erreur**, **« à jour / tout vu »** (flux). L'état **vide** du flux Accueil amorce la boucle d'onboarding : *chercher → suivre → le flux se remplit*.

---

## 12. Composant : carte-aperçu (spec)

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

### Densité (préférence)
La densité est une **préférence utilisateur, jamais une divergence de design** : **un seul composant, deux densités**. La direction par défaut est **« aérée »** (date et source sur leur propre ligne) ; un réglage optionnel propose une variante **compacte** (lignes resserrées) pour les usages intensifs au poste.
- Le réglage agit sur l'**espacement** (padding vertical, interligne, date/source sur une ligne dédiée ou condensées). Il ne touche **ni au contenu, ni à l'anatomie** : on resserre l'espace, on ne retire jamais d'information (R5, masquer ≠ amputer).
- **Garde-fou** : même en mode compact, la **cible tactile reste ≥ 44 px** — on resserre le visuel, pas la zone tappable.
- Vit dans **Profil** (avec le thème et l'accessibilité) et **persiste par utilisateur, multi-device** (ADR-001).

### Contrat d'accessibilité (doc 06)
- Chaque ligne = un `article` au sein d'un `feed` / `CollectionView`.
- `SemanticProperties.Description` composant *entité + événement + date + source* dans un ordre de lecture logique.
- Cible ≥ 44×44 ; focus visible ; aucune information par la **couleur seule**.

### Garde-fous doctrine
Source présente sur tout événement (confiance) ; libellé = **fait, jamais verdict** ; badge neutre ; aucun signal d'engagement (« tendance », compteurs de popularité) sur l'item.

### Hors-périmètre
Dès qu'il faut plus de 2 lignes + badges, ce n'est plus une carte-aperçu : on descend dans la fiche (carte-section / page détail).

---

## 13. Composant : carte-section (spec)

Facette **repliable** de la fiche : un conteneur thématique (Identité, Dirigeants, Bilans, BODACC…) qui regroupe des atomes (champs étiquetés, badges, chiffres clés, provenance). Composant de structuration de la fiche (§4) ; pour une ligne de liste, c'est la carte-aperçu (§12).

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
Déployée · **repliée** · repliée-par-pertinence (R6, signalée) · **épinglée** · **vide-de-couverture** (cf. §14) · chargement (squelette de section) · **erreur locale**.

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

## 14. Composants : les états (spec)

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

## 15. Implications techniques MAUI

- **Navigation** : MAUI `Shell` — onglets (mobile) / flyout ou rail (desktop). Les 5 destinations de §3 sont les routes de premier niveau.
- **Réagencement** : `VisualStateManager` + `AdaptiveTrigger` (sur la largeur de fenêtre, R2), `OnIdiom` pour les ajustements Phone/Desktop. Composants partagés, disposition variable.
- **Contrainte d'architecture** : `Atlas.Maui` ne référence que `Atlas.Domain` et `Atlas.Shared` ; toute donnée transite par l'API HTTP via `AtlasApiClient` (cf. `CLAUDE.md`, règles d'or de dépendance). Aucune logique métier côté client.
- **Session** : JWT en SecureStorage (Keychain/Keystore), refresh transparent sur 401 — l'expiration de session ne doit interrompre que lorsqu'une ré-authentification dure est nécessaire.

---

## 16. Ce que ce document ne tranche pas (encore)

Honnêteté de périmètre, à instruire plus tard :

- **Langage visuel détaillé** : `themes.json` pose déjà un système de tokens sémantiques (light/dark + 7 thèmes dont 2 a11y). La marque n'a volontairement **pas de couleur propriétaire** — la rigueur est dans la structure, la couleur appartient à l'utilisateur. Une charte d'app dédiée reste à formaliser (sort/non de l'or décoratif du print, etc.).
- **Valeurs exactes des points de rupture** (R2).
- **« Vérifier un nom » (3e mode Recherche, §7)** : feature **proposée, non implémentée**. Nécessite une fiche feature (F-0xx) et une **source externe nouvelle** (WHOIS/DNS) hors catalogue actuel. Garde-fou central à tenir : rapport factuel, **jamais** un verdict de disponibilité juridique.
- **Parcours détaillé de l'import en masse de SIREN** (Favoris §8) : écran de saisie (coller/CSV), état « import en cours », et **rapport de fin** (N ajoutés / M doublons / K invalides). Mini-parcours asynchrone à spécifier au même niveau que les autres écrans.
- **Composition de la vue 360 / dossier cible** : méta-feature d'assemblage, terrain n°1 de R6 (un même dossier qui se recompose selon le persona). S'appuiera sur le mécanisme de **gabarits de fiche** désormais figé (§4).
- **Comportement du retour (navigation)** : le retour doit **refléter l'origine** (Accueil / Recherche / Favoris), pas coder une destination en dur. *(trou repéré sur la maquette fiche — à figer quand on traitera la navigation)*
- **Mode « personnaliser »** : emplacement concret de l'**épingle** et des **toggles d'affinage** R6 (vue par défaut vs mode édition). *(trou repéré sur la maquette fiche)*

---

## Renvois

| Sujet | Référence |
|---|---|
| Accessibilité (exigence bloquante, DoD) | `docs/06-accessibilite.md`, ADR-008 |
| Accessibilité spécifique au flux/timeline | `docs/06-accessibilite.md` §11.5 |
| Multi-device / SaaS (justifie R1) | ADR-001 |
| Doctrine descriptive (pas de verdict) | ADR-012 |
| Sources de veille, packs par persona | `docs/07-flux-rss-veille.md` |
| Features Veille (timeline, packs, dédup, mentions) | F-042 (packs), F-043 (sources libres), F-044 (timeline), F-045 (dédup), F-046 (filtres/règles), F-047 (mentions favoris), F-048 (BODACC) ; endpoints `docs/11-api-endpoints.md` |
| Système de thèmes / tokens | `themes.json` |
| Features liées (clients, flux, favoris) | F-009/F-010, F-047, F-053, F-017/F-019, F-055, F-048 |
| Features de recherche (SIREN, nom, marque) | F-004, F-005, F-006 ; sources : `docs/03-catalogue-apis-publiques.md` |
| « Vérifier un nom » (3e mode, futur) | Proposé — à tracer en fiche feature ; source WHOIS/DNS à ajouter ; F-006 (marques INPI) pour l'antériorité |
| Favoris / Watchlists / tags | F-017 (favoris), F-053 (listes + import), F-030 (annotations & tags), F-047 (timeline) |
| Authentification & entrée (login, inscription, 2FA, INPI) | F-001, F-002, F-003 ; endpoints `docs/11-api-endpoints.md` (`/auth/*`, `/inpi/connection`) ; ADR-010, ADR-003 |
| Profil / compte / sécurité / RGPD | F-001 (auth), F-002 (2FA), F-003 (connexion INPI), F-012 (export & suppression) ; `docs/04-securite-rgpd.md`, ADR-010 |
| Placement du code MAUI | `CLAUDE.md`, `docs/09-architecture-detaillee.md` |

> Candidat à être référencé depuis le tableau « Documents fondateurs » de `CLAUDE.md` (ligne « Toute interface utilisateur (MAUI) »), en complément de `docs/06-accessibilite.md`.

---

*Document figé le 29 mai 2026. Les 6 règles, l'architecture de navigation à 5 destinations et le kit de composants (atomes · cartes · états) constituent la doctrine UX de référence du client MAUI. Toute dérogation doit être justifiée et tracée (ADR dédié si la décision est structurante).*