# F-061 — Surveillance continue d'un nom

> **Statut** : 🔵 Proposé (figé, non planifié). Cible **V3+ — Won't have (yet)**.
> **Date** : 30 mai 2026.
> **Catégorie MoSCoW** : Won't have (yet).
> **Origine** : extension de F-060 (vérification ponctuelle) ; surnom de travail *« le F-019 des noms »*.
> **Doctrine** : sous **ADR-012** et **ADR-014** (contrat `MatchCandidate` — « à vérifier », jamais un verdict, structurellement).
> **Dépendances** : **F-060** (vérification ponctuelle multi-sources — le pendant à l'instant T), **F-026/ADR-014** (`ITrademarkSimilarityMatcher` + noyau de normalisation des noms), **F-019** (patron snapshot + diff + job), **F-047** (`FavoriteEvent` + timeline), **F-048** (patron polling + dédup `ExternalId`), **F-046** (`WatchRule` d'activation), **F-020** (push), **F-017/F-053** (entités suivies). WHOIS/DNS : source externe nouvelle (cf. F-060).
> **Documents liés** : `02-roadmap-features.md` (V3), `feature-verificateur-nom-F060.md`, `feature-rescreening-continu-F057.md` (jumeau structurel), `ADR-014`, `06-accessibilite.md`, `04-securite-rgpd.md`.

---

## Description

Une **surveillance continue** d'un nom : Atlas détecte au fil du temps l'**apparition de nouveaux éléments proches** d'un nom suivi — nouveaux **dépôts de marques** (INPI), nouvelles **dénominations sociales** (RNE), nouveaux **enregistrements de domaines** (WHOIS) — et **alerte** l'utilisateur, avec une **piste d'audit** (date, source, élément détecté, base du rapprochement).

C'est le **pendant temporel de F-060** : F-060 répond à « ce nom est-il déjà pris, *maintenant* ? » ; F-061 répond à « **préviens-moi quand quelque chose de proche apparaît** ». Exactement le mécanisme de **F-019** (snapshot + diff + job) et le **jumeau de F-057** (re-screening sanctions), transposé du domaine *sanctions* au domaine *noms/marques*.

Deux objets surveillables :
- **Un nom libre saisi** (ex. un projet de marque pas encore déposé) — nouveau concept de **« surveillance de nom »** persistante.
- **Une entité déjà suivie** (une marque en favori / watchlist) — via une `WatchRule` (patron F-046), comme F-057.

## Valeur user

Pour le **Cabinet PI** et les **titulaires de marques**, la **surveillance d'antériorité** (être alerté d'un dépôt proche dans les délais d'opposition) est un service à forte valeur — et récurrent (abonnements de surveillance payants chez les acteurs PI). Pour un **créateur**, surveiller un nom de projet *avant* dépôt évite la mauvaise surprise.

**Angle différenciant Atlas** : surveillance **multi-sources** (marque + société + domaine au même endroit), **souveraine** (le nom surveillé ne sort pas), **descriptive** (« correspondance à vérifier », jamais « conflit avéré »), avec **journal d'audit défendable**. Les services de surveillance PI classiques sont mono-source et coûteux ; Atlas combine et reste honnête.

## Périmètre

**Dans le périmètre :**
- Surveillance **opt-in, par source** d'un **nom libre** ou d'une **entité suivie**.
- Détection des **apparitions** d'éléments proches (nouveau dépôt / nouvelle dénomination / nouveau domaine).
- Alerte **timeline (F-047) + push (F-020)** + **journal d'audit** persisté.
- Réutilise le **moteur de similarité de F-026** (marques) et le **noyau de normalisation** (dénominations) — pas de nouveau matcher.

**Hors périmètre (explicite) :**
- **Aucun verdict** : ni « conflit », ni « risque de confusion avéré », ni « à déposer/à ne pas déposer ». Sortie = `MatchCandidate` « à vérifier » (ADR-014), structurellement.
- **L'opposition / l'action juridique** : Atlas alerte, l'utilisateur (ou son conseil PI) décide et agit.
- **La surveillance de personnes** (noms de dirigeants) : hors périmètre (même prudence que F-057).
- **Surveillance internationale** (EUIPO/OMPI) : v1 = périmètre français (INPI/RNE) ; l'international est une extension ultérieure.

## Complexité : ★★★★

Le gros est déjà fait ailleurs : snapshot/diff (**F-019**), polling + dédup cross-users (**F-048**), similarité marques (**F-026**), timeline (**F-047**), push (**F-020**). Le neuf : (a) le concept de **« surveillance de nom libre »** (objet persistant sans entité sous-jacente), (b) le **polling des nouveaux dépôts** BOPI/RNE filtré par similarité, (c) le **volet domaines** (WHOIS récurrent — bruyant et coûteux, d'où un jalon séparé), (d) la **maîtrise du bruit** (un dépôt « proche » génère facilement des faux positifs).

## APIs externes

| Source | Usage | Statut |
|---|---|---|
| **INPI PI** (BOPI / nouveaux dépôts marques) | Détecter les dépôts proches dans le temps | Intégrée (F-006) ; polling des nouveautés à ajouter |
| **INPI RNE** | Nouvelles dénominations proches | Intégrée (F-004/F-005) |
| **WHOIS / DNS** | Nouveaux enregistrements de domaines proches | **Nouvelle source** (cf. F-060) — volet séparé |

---

## Détails techniques (jumeau de F-057)

- **Snapshot d'état** : `NameWatchSnapshot` (un cliché vivant par cible surveillée — nom libre **ou** `(UserId, entité)`) capturant l'**ensemble des correspondances proches connues** (formes normalisées / hash, comme le hash dirigeants de F-019).
- **Diff** : `DiffWith(...)` retourne les correspondances **apparues** (et, le cas échéant, **disparues**).
- **Job Hangfire** `name-watch`, cron décalé des autres jobs (après `favorite-refresh` 03:00, `bodacc-polling` 04:00, `sanctions-rescreening` 05:00 → p.ex. `0 6 * * *`), désactivable via `BackgroundJobs:Enabled=false`.
- **Événement** : nouveau type de `FavoriteEvent` (ou événement dédié pour les noms libres) `NameMatchAppeared`, `ExternalId` = identifiant de dépôt/dénomination/domaine + source, pour la **dédup** (index unique partiel existant).
- **Dédup cross-users** : un seul calcul de similarité par nom partagé (patron F-048).
- **Matcher** : `ITrademarkSimilarityMatcher` (F-026) pour les marques ; noyau de normalisation (ADR-014) pour les dénominations ; comparaison de chaînes normalisées pour les domaines. **Sortie = `MatchCandidate`** → « à vérifier » garanti par le type.
- **Audit** : événements persistés (date, source, élément, base du rapprochement) → **journal défendable** (valeur PI).
- **Maîtrise du bruit** : seuil de similarité réglable ; regroupement des correspondances proches ; le volet domaines (très bruyant) est **opt-in séparé**.

## Doctrine — « à vérifier », jamais un verdict (fait foi)

Hérite de **ADR-014** : la sortie est un `MatchCandidate` portant un **niveau de confiance** et une **base explicable**, dont la disposition **ne peut être que « candidat / à vérifier »** — l'état « conflit avéré » est *structurellement irreprésentable*. Atlas signale *« un dépôt proche de votre nom est apparu, à vérifier »*, jamais *« votre marque est en conflit »*. L'appréciation du risque de confusion relève du conseil en PI.

## Cadre RGPD / légal

- Sources publiques (BOPI, RNE) ; respect Etalab et `diffusionINSEE = "N"`.
- Une **« surveillance de nom »** est une donnée utilisateur : cascade FK compte, incluse dans l'export (art. 20), supprimée à la clôture (art. 17).
- Atlas reste une **couche d'alerte descriptive** ; aucune garantie de conformité ni d'exhaustivité (la surveillance ne remplace pas une recherche d'antériorité professionnelle — à dire explicitement).

## Accessibilité (ADR-008 / doc 06)

Alertes et événements pleinement lisibles au lecteur d'écran ; mention « correspondance à vérifier » explicite (jamais portée par la seule couleur) ; journal d'audit consultable de façon accessible. Réutilise les patterns de la timeline (doc 12 §6) et des états (doc 12 §14).

## Modèle économique (ADR-006)

Volets marques/dénominations : rapprochement déterministe sur sources gratuites → **cœur / OSS**, avec **quota d'entités surveillées en hébergé** (patron F-043). Volet **domaines (WHOIS)** : selon le coût du fournisseur, possiblement **offre supérieure**. La similarité **sémantique IA** (si un jour) reste premium (`Atlas.Application.Premium`, ADR-006/ADR-014).

## Découpage / jalons

1. **Surveillance marques (socle)** : `NameWatchSnapshot` + diff + job, sur les nouveaux dépôts INPI proches (réutilise F-026). Cible : nom libre **et** entité suivie. *(Le cœur de la valeur PI.)*
2. **Volet dénominations RNE** : nouvelles sociétés au nom proche.
3. **Volet domaines (WHOIS)** : opt-in séparé (bruit + coût) — dépend de l'intégration WHOIS de F-060.
4. **(Option)** surveillance internationale (EUIPO/OMPI).

## Décisions ouvertes (à trancher avant implémentation)

- **F-060 vs F-026** : F-060 (rédigée avant la relecture d'ADR-014) évoque un moteur de similarité « à construire » ; **or F-026/ADR-014 fournit déjà `ITrademarkSimilarityMatcher`**. → À harmoniser : F-060 et F-061 **réutilisent F-026**, ne réinventent pas. (À corriger dans F-060.)
- **Objet « nom libre »** : nouveau concept persistant — où le ranger dans l'UI (un espace « Mes surveillances » ? sous Favoris ? sous le 3e mode Recherche F-060 ?). À spécifier en UX (doc 12).
- **Seuil de similarité** par défaut et réglage utilisateur (équilibre bruit / exhaustivité).
- **Fréquence** de polling et **quotas** (nombre de noms surveillés) en hébergé.
- **Volet domaines** : fournisseur WHOIS, coût, place économique (commun avec la décision ouverte de F-060).

## À faire à l'intégration

- Ajouter **F-061** au catalogue `02-roadmap-features.md` (V3+), avec un *« Pourquoi reporté »* (dépend de F-060 + WHOIS + maîtrise du bruit).
- **Corriger F-060** pour qu'elle réutilise explicitement F-026/ADR-014 (matcher de marques) au lieu d'un moteur « à construire ».
- Si un emplacement UX « Mes surveillances » est retenu, l'ajouter à `docs/12-modele-ux-client-maui.md` (probablement sous Favoris ou comme prolongement du 3e mode Recherche).

---

*Fiche figée le 30 mai 2026. Pendant temporel de F-060 et jumeau structurel de F-057 (« le F-019 des noms »). Réutilise massivement l'existant (snapshot/diff F-019, polling F-048, similarité F-026, timeline F-047, push F-020). Principe non négociable : « correspondance à vérifier », jamais un verdict — garanti par le contrat `MatchCandidate` (ADR-014).*
