# F-060 — Vérificateur de présence d'un nom (multi-sources)

> **Statut** : 🔵 Proposé (figé, non planifié). Cible **V3+ — Won't have (yet)**.
> **Date** : 30 mai 2026.
> **Catégorie MoSCoW** : Won't have (yet).
> **Dépendances** : F-006 (recherche marque INPI PI — antériorité), F-005 (recherche dénomination RNE), value object `Siren`/validation (F-004), **F-026 / ADR-014** (`ITrademarkSimilarityMatcher` + noyau de normalisation + contrat `MatchCandidate`) ; UX : `docs/12-modele-ux-client-maui.md` §7 (3e mode de la Recherche).
> **Documents liés** : `02-roadmap-features.md` (V3, F-031 à F-040), `03-catalogue-apis-publiques.md` (sources), `08-vocabulaire-ubiquitaire.md`, `06-accessibilite.md`, `ADR-012` (doctrine descriptive), **`ADR-014`** (matching conservateur unifié), **`feature-surveillance-nom-F061.md`** (pendant temporel).

---

## Description

Depuis la Recherche, un **mode « Vérifier un nom »** : l'utilisateur saisit un nom (de marque, d'enseigne, de société envisagée) et l'app produit un **rapport de présence multi-sources** — un état des lieux factuel de ce nom à travers plusieurs registres, **sourcé et daté**, accompagné d'une liste de **noms proches à vérifier**.

Cette feature **rapporte des faits ; elle ne rend pas de verdict de disponibilité**. Elle outille la décision de l'utilisateur (notamment avant un dépôt de marque ou une création de société) sans se substituer à un conseil en propriété industrielle. C'est la distinction structurante de la fiche (voir « Doctrine — faits, jamais verdict »).

## Valeur user

Besoin réel et transverse à plusieurs personas (Cabinet PI au premier chef, mais aussi Investisseur/M&A et créateurs) : *« ce nom est-il déjà pris, et où ? »* avant d'engager un dépôt, un rachat ou une création.

**Angle différenciant Atlas.** Les outils existants tombent dans deux travers : soit ils ne couvrent qu'**une** source (un checker de domaine, ou un checker de marque isolé), soit ils affichent un **verdict irresponsable** (« DISPONIBLE ✓ ») qui induit en erreur sur un sujet à fort enjeu juridique. Atlas fait l'inverse : **agrège plusieurs registres** en un geste **et** reste rigoureusement **descriptif** — chaque résultat remonte à sa source datée. C'est l'ADN « dossier auditable » du produit appliqué à un nom plutôt qu'à une entreprise.

## Périmètre

**Dans le périmètre :**
- Mode **« Vérifier un nom »** comme 3e intention de la Recherche (pas de nouvel onglet — respecte le plafond à 5, cf. doc 12 §3).
- **Rapport multi-sources** pour un nom : marques (INPI PI), dénomination sociale (RNE), domaines web (WHOIS/DNS).
- **Noms proches** : pistes de similarité (orthographique/phonétique) présentées **à vérifier**.
- Chaque source affiche **sa provenance, sa date et son périmètre**.

**Hors périmètre (explicite) :**
- **Tout verdict de disponibilité** (« libre », « disponible », « vous pouvez déposer »). Interdit par doctrine (voir plus bas).
- **L'évaluation du risque de confusion** (appréciation juridique) : relève du conseil en PI, pas d'Atlas.
- **Le dépôt** de marque ou la **réservation** de domaine : Atlas informe, n'agit pas (pas de transaction).
- **La surveillance continue** d'un nom (alertes sur nouveaux dépôts proches) : extension possible *ultérieure*, à rapprocher du substrat de surveillance (F-055/F-057), hors périmètre de cette fiche.

## Complexité

**★★★** (2–3 semaines). La couche UI (3e mode + cartes-source) est modérée ; le poids vient de : (a) l'intégration **WHOIS/DNS** (source externe nouvelle, formats et rate-limits hétérogènes par registre), (b) le **branchement** sur le moteur de similarité **existant** (`ITrademarkSimilarityMatcher`, F-026/ADR-014 — pas de moteur à réécrire), (c) la **rigueur doctrinale** du rendu (ne jamais glisser vers le verdict — garantie par le contrat `MatchCandidate`).

## APIs externes

| Source | Usage | Statut |
|---|---|---|
| **INPI PI** (marques) | Antériorité : marques au libellé identique/proche + classes | Déjà intégrée (F-006) |
| **INPI RNE** | Dénominations sociales existantes portant ce nom | Déjà intégrée (F-004/F-005) |
| **WHOIS / DNS** | État d'enregistrement des domaines (.fr, .com, .eu…) | **Nouvelle source — à intégrer.** Absente du catalogue `03-catalogue-apis-publiques.md` |

> **Pourquoi reporté (V3).** Deux raisons : (1) **source externe nouvelle** (WHOIS/DNS) hors du périmètre INPI/INSEE/BODACC actuel — à évaluer (fournisseur, coût, rate-limits, fiabilité par extension) ; (2) **sensibilité juridique maximale** — c'est l'écran où un mauvais cadrage ferait le plus de dégâts, donc il ne se livre pas à la légère. À reconsidérer selon la traction et les retours du persona Cabinet PI.

---

## Détails techniques

### Architecture (hexagonale, ADR-004)
- Port de domaine **`INameAvailabilityProbe`** (ou un port par registre) côté `Atlas.Domain`, renvoyant des **constats sourcés** (jamais un booléen « disponible »).
- Adapters : `InpiTrademarkPresenceAdapter` (réutilise F-006), `RneDenominationPresenceAdapter` (réutilise F-005), **`WhoisDomainAdapter`** (nouveau, isolé en `Atlas.Infrastructure.Whois`).
- **Agrégation en façade** : un use case `CheckNamePresence` interroge les sources **en parallèle**, borne chaque appel (timeout), et compose un DTO **discriminé par source** — sans fusion en indicateur unique.

### Similarité de noms — réutilise F-026 / ADR-014 (ne pas réinventer)
Le rapprochement **ne réécrit aucun algorithme** : il consomme les briques unifiées d'**ADR-014**.
- **Marques** : `ITrademarkSimilarityMatcher` (F-026) — similarité phonétique/visuelle/conceptuelle + recoupement des classes de Nice. Moteur dédié, en partie premium.
- **Dénominations** : le **noyau de normalisation des noms** (`ICompanyNameNormalizer`, ADR-014 — suffixes SA/SAS/SARL, accents, casse, variantes de raison sociale) + comparaison.
- **Domaines** : comparaison de chaînes normalisées.
- **Sortie** : des `MatchCandidate` (ADR-014) — chacun avec niveau de confiance + base explicable, disposition **« à vérifier » uniquement** (le verdict est *structurellement irreprésentable*). La feature ne fait donc qu'**afficher** des candidats, jamais conclure.

### Rendu (cf. doc 12 §7)
- Une **carte par source**, chacune avec atomes provenance + badge **neutre**.
- **États par source** : chargement (squelette), **erreur locale** (une source injoignable n'efface pas les autres), vide (« aucun résultat trouvé », qui est un *fait*, pas un « disponible »).

### Endpoints (esquisse, à confirmer)

| Méthode | Path | Rôle |
|---|---|---|
| `GET` | `/names/check?q=` | Rapport multi-sources pour un nom (marques + dénomination + domaines). |
| `GET` | `/names/similar?q=` | Noms proches à vérifier. |

---

## Doctrine — faits, jamais verdict (section qui fait foi)

C'est le **cas le plus sensible de l'application**. Les garde-fous ne sont pas négociables (cohérent ADR-012, descriptif jamais prescriptif) :

1. **Faits, jamais verdict — garanti par le type.** Autorisé : « 2 marques au libellé proche trouvées (classes 35, 40) », « .eu non enregistré ». **Interdit** : « disponible », « libre », « vous pouvez déposer ». Le rapprochement passe par le contrat `MatchCandidate` (ADR-014), dont la disposition ne peut être que « à vérifier » : émettre un verdict est *structurellement impossible*, pas seulement déconseillé. Un faux « disponible » engagerait à tort la responsabilité de l'utilisateur sur une décision juridique à fort enjeu.
2. **Provenances distinctes, jamais fondues.** Trois sources = trois fiabilités = trois cartes, chacune avec source + date + périmètre. Aucun indicateur agrégé unique.
3. **Similarité = suggestion à vérifier**, jamais une affirmation de risque de confusion (même principe que les mentions d'articles en Veille, doc 12 §6).
4. **Couleurs neutres.** Un domaine enregistré ou une marque existante est un **fait**, pas un « mauvais » résultat : pas de rouge/vert de jugement (`success`/`danger` réservés aux états système).
5. **Note de cadrage permanente** à l'écran : *« Atlas rapporte ce qu'il a trouvé, sourcé et daté. Il ne conclut pas à la disponibilité juridique d'un nom — cette appréciation relève d'un conseil en propriété industrielle. »*

## Cadre RGPD

La requête (un nom) peut être journalisée pour le rate-limiting/sécurité (intérêt légitime) ; pas de donnée personnelle nouvelle au-delà de l'usage normal. Les résultats proviennent de registres publics. Respecter la licence Etalab et le `diffusionINSEE = "N"` côté RNE (ne pas rediffuser une dénomination en diffusion restreinte).

## Accessibilité (rappel doc 06)

- Le mode « Vérifier un nom » est un segment du sélecteur de Recherche, **navigable au clavier**, sélection annoncée.
- Chaque carte-source = bloc sémantique avec titre ; **aucune information par la couleur seule** (l'état d'un domaine est porté par le texte, pas par une pastille).
- Le rapport est **lisible par lecteur d'écran** dans un ordre logique (source → constat → provenance).

## Modèle économique

Coût marginal : les appels WHOIS (et leur éventuel fournisseur payant). Piste : feature du **cœur** pour les sources déjà intégrées (marques/RNE), avec le volet **domaines** éventuellement soumis à **quota en hébergé** (sur le modèle F-043), ou réservé à une offre supérieure si le coût WHOIS le justifie. À trancher selon le fournisseur retenu.

---

## Découpage / jalons

1. **Socle marques + dénomination** : 3e mode Recherche, rapport sur les sources **déjà intégrées** (F-006 + F-005), sans WHOIS. *(Livrable autonome, sans dépendance externe nouvelle.)*
2. **Noms proches** : algorithme de similarité + rangée « à vérifier ».
3. **Volet domaines (WHOIS)** : adapter isolé, après évaluation fournisseur. *(C'est le jalon qui justifie le classement V3.)*
4. **(Extension)** surveillance continue d'un nom : traitée dans sa **fiche dédiée F-061** (pendant temporel de cette feature).

## Décisions ouvertes (à trancher avant implémentation)

- **Fournisseur WHOIS/DNS** : lequel, à quel coût, avec quelle couverture d'extensions et quels rate-limits ? (Bloquant pour le jalon 3.)
- **Périmètre des extensions de domaine** par défaut (.fr/.com/.eu… jusqu'où ?).
- **Réglage de la similarité** : seuil et paramètres exposés par `ITrademarkSimilarityMatcher` (F-026) — pas un moteur propre.
- **Place économique** du volet domaines (cœur vs quota hébergé vs offre supérieure).
- **Lien avec F-061** : la vérification ponctuelle (F-060) et la surveillance continue (F-061) partagent le même matcher (F-026) et la même doctrine — garder les deux fiches alignées.

## À faire à l'intégration

- Ajouter **F-060** au catalogue `02-roadmap-features.md` (bucket V3+ — Won't have yet), avec un bloc *« Pourquoi reporté »* (source WHOIS nouvelle + sensibilité juridique).
- Ajouter la source **WHOIS/DNS** à `03-catalogue-apis-publiques.md` une fois le fournisseur évalué.
- La sous-section doc 12 §7 « Vérifier un nom » référence cette fiche ; conserver la cohérence du garde-fou « faits, jamais verdict » entre les deux documents.
- **Référencer F-061** (surveillance continue) comme pendant temporel, et **F-026/ADR-014** comme moteur de similarité réutilisé (ne pas réimplémenter).

---

*Fiche figée le 30 mai 2026. Feature V3, reportée pour deux raisons : intégration WHOIS/DNS (source externe nouvelle) et sensibilité juridique maximale. Principe non négociable : un rapport de présence factuel et sourcé, jamais un verdict de disponibilité (ADR-012).*
