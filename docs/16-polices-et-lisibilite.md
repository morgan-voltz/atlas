# Polices & lisibilité

> **Source de vérité** de la doctrine de lisibilité d'Atlas : choix de police, espacement, taille. Les autres documents la **référencent** au lieu de la redire (principe « un seul propriétaire ») — notamment `docs/06-accessibilite.md` (qui mentionne le scaling de police) et `docs/12-modele-ux-client-maui.md` §8 (réglage de densité). En cas de divergence, **ce document fait foi** pour la lisibilité du texte.
>
> **Version** : 1.0 — **Date** : 30 mai 2026 — **Portée** : tous les clients (MAUI doc 12, web doc 14).
> **Cadre** : alimente l'accessibilité **bloquante** (ADR-008, WCAG 2.2 AA / RGAA) ; s'intègre au **système de thèmes** existant (`themes.json`) via le même mécanisme : une préférence utilisateur persistée.

---

## Le principe (à lire avant le catalogue)

L'intuition « une police spéciale dyslexie = la solution » est **largement contredite par la recherche** : les lecteurs dyslexiques ne lisent ni plus vite ni plus précisément avec OpenDyslexic ou Dyslexie qu'avec une bonne sans-serif — beaucoup préfèrent cette dernière. La dyslexie est un trouble du **traitement phonologique**, pas un trouble visuel ; la typo réduit la charge cognitive, elle ne « soigne » rien. Le facteur qui compte n'est pas le **dessin des lettres** mais **l'espacement** (lettres / mots / lignes) et la **taille**.

**Conséquence — alignée sur toute la doctrine Atlas.** Le bon standard d'accessibilité n'est **pas** d'imposer LA bonne police, c'est d'**offrir le choix + des réglages**. *La lisibilité est personnelle* — exactement comme « la couleur appartient à l'utilisateur » (thèmes) et « la densité est une préférence » (doc 12 §8). La typo accessible est le **troisième axe** de la même idée : donner le contrôle, ne pas imposer.

---

## 1. Le système typographique — trois rôles

Atlas n'a pas « une police » mais **trois rôles**, chacun choisi pour sa force :

| Rôle | Police par défaut | Pourquoi |
|---|---|---|
| **Corps** (texte lu longtemps : fiches, articles, listes) | **Atkinson Hyperlegible** | Lisibilité maximale, glyphes **non ambigus** ; c'est là que la lecture se joue. |
| **Titres** (scannés, hiérarchie, identité) | **IBM Plex Serif** | Identité de marque + hiérarchie visuelle ; on scanne, on ne lit pas longtemps. |
| **Mono** (identifiants : SIREN, codes) | **IBM Plex Mono** | Chiffres/lettres non ambigus (`0/O`, `1/l`) ; chasse fixe alignant les numéros. |

> **Changement vs maquettes initiales.** Les premières maquettes utilisaient IBM Plex Sans en corps. Le défaut **corps** bascule sur **Atkinson Hyperlegible** (accessibilité d'abord) ; **Plex Serif** reste aux titres, **Plex Mono** aux identifiants. Atkinson **n'ayant pas de variante monospace**, le mono reste impérativement Plex Mono (ou équivalent à chasse fixe et glyphes non ambigus).

---

## 2. Sélecteur de police (préférence utilisateur)

Le **corps** est choisissable par l'utilisateur. Options proposées :

| Police | Licence | Rôle |
|---|---|---|
| **Atkinson Hyperlegible** | Gratuite, licence ouverte (Braille Institute) | **Défaut.** Glyphes non ambigus, pensée basse vision, bénéfique à tous. |
| **Lexend** | Open-source (Google Fonts) | Alternative ; conçue autour de l'espacement, **recherche solide sur la vitesse de lecture**. |
| **OpenDyslexic** | Open-source, gratuite (compatible AGPL) | **Option**, jamais défaut. Base de lettre alourdie (anti-rotation b/d/p/q) ; aide nettement *certains*, pas tous. |
| **Sans-serif classique** (Verdana / **Comic Neue**) | Libres | Familières, bien espacées, recommandées par la British Dyslexia Association (Comic Neue = lettres peu symétriques, peu confondables ; équivalent libre et sobre). |

**Vigilance licence (cœur AGPL / auto-hébergeable)** : ne retenir que des polices **à licence ouverte/gratuite**. **Dyslexie** (payante) est **écartée** malgré sa notoriété — incompatible avec un cœur open-source.

---

## 3. Réglages d'espacement & de taille (le levier le plus efficace)

Selon la recherche, ce sont **les** leviers de lisibilité — plus que le choix de police. À exposer comme préférences :
- **Taille du texte** (scaling) — respecte aussi le réglage système (Dynamic Type / font scale OS).
- **Interlignage** (hauteur de ligne).
- **Espacement** lettres / mots.

Garde-fous (cohérents doc 12) :
- Les réglages agissent sur l'**espace**, jamais sur le **contenu** : on n'ampute rien (R5).
- La **cible tactile reste ≥ 44 px** quelle que soit la taille (doc 12 §8).
- La mise en page **reflow** proprement (WCAG 2.2 — reflow & espacement du texte) sans casser ni tronquer.

---

## 4. Intégration — même tuyauterie que les thèmes

Le choix de police et les réglages d'espacement/taille sont des **préférences utilisateur persistées**, branchées sur le **mécanisme existant du système de thèmes** (`themes.json`) — pas une mécanique nouvelle. Conséquences :
- **Persistance multi-device** (ADR-001), comme le thème et la densité.
- **Emplacement** : dans **Profil → « Affichage & données »** (doc 12 §9), regroupée avec la **densité**, l'**espacement/taille** et les autres préférences d'**accessibilité** — tout le levier de lisibilité au même endroit. (Le **thème**, lui, relève du goût et reste dans « Paramètres généraux ».) Un seul endroit « comment je lis l'app ».
- **Découplage rôle/police** : les rôles (corps/titres/mono) sont des **tokens** ; changer la police de corps ne touche ni les titres ni le mono.

---

## 5. Cohérence avec le reste de l'UI (déjà acquise)

La force de notre doctrine « jamais d'information par la couleur seule » paie ici : tout ce qui porte du sens est **déjà textuel**, donc **reste lisible quelle que soit la police choisie** —
- les **5 états de section** (ADR-015),
- la fraîcheur « as of » / provenance · date (doc 12),
- le **« à vérifier »** du matching (ADR-014),
- les badges (état administratif, source) qui portent leur libellé (doc 12 §11).

Changer de police ou agrandir le texte ne casse donc aucun signal — ils ne dépendent ni d'un glyphe précis ni d'une couleur.

---

## 6. Rappels de cadrage

- **Une police ne suffit pas à la conformité WCAG/RGAA** : elle est une pièce parmi contraste, navigation clavier, lecteur d'écran, focus visible, etc. (doc 06 / ADR-008 couvrent le reste).
- **Atkinson par défaut ≠ accessibilité réglée** : le vrai service rendu, c'est **le choix + les réglages d'espacement/taille**, pas la police par défaut prise isolément.
- **Le mono est non négociable pour les identifiants** : les SIREN exigent des glyphes non ambigus ; ne jamais basculer un SIREN sur une police de corps à chasse variable.

---

## 7. Ce que ce document ne tranche pas (encore)

- **Liste finale des polices embarquées** vs chargées (poids de l'app, surtout web/WASM — doc 14) : à arbitrer (auto-hébergement des fichiers de police pour la souveraineté).
- **Valeurs par défaut** d'interlignage/espacement et **plages** des curseurs de réglage.
- **Maquette du sélecteur** (Profil → Affichage & données) dans Claude Design.
- **APHont** (basse vision spécifique) : à évaluer comme option supplémentaire si le besoin émerge.

---

## Renvois

| Sujet | Référence |
|---|---|
| Accessibilité (cadre bloquant, contraste, clavier, lecteur d'écran) | `docs/06-accessibilite.md`, ADR-008 |
| Densité, thèmes, emplacement « Affichage & données » | `docs/12-modele-ux-client-maui.md` §8, §9 |
| Spécificités web (poids WASM, reflow) | `docs/14-modele-ux-client-web.md` |
| Système de tokens / thèmes | `themes.json` |
| Persistance multi-device des préférences | ADR-001 |
| Matching « à vérifier », états de section (restent textuels) | ADR-014, ADR-015 |

> **À faire à l'intégration** :
> - Dans `docs/06-accessibilite.md` et `docs/12-modele-ux-client-maui.md`, **remplacer toute règle de lisibilité/scaling par un renvoi vers ce doc 15** (un seul propriétaire).
> - Mettre à jour `themes.json` / les tokens typographiques : rôle **corps = Atkinson Hyperlegible** (défaut), **titres = IBM Plex Serif**, **mono = IBM Plex Mono**.
> - Auto-héberger les fichiers de police (licences ouvertes) pour la souveraineté et l'usage hors-ligne.

---

*Document figé le 30 mai 2026. Source de vérité de la lisibilité d'Atlas. Principe : la lisibilité est personnelle — on offre le choix (police) + les réglages (espacement, taille), on n'impose pas. Défaut : corps Atkinson Hyperlegible, titres IBM Plex Serif, identifiants IBM Plex Mono. Le service d'accessibilité vient de la combinaison choix + réglages, jamais d'une police miracle.*
