# F-059 — Veille d'échéances PI (docketing assistif souverain)

> **Statut** : **V3+ conditionnel** — conditionnée non par une DPIA (comme F-034) mais par un **cadrage de responsabilité** explicite : c'est le prérequis bloquant ici. Origine : graine du persona **Cabinet de Propriété Industrielle** — la fonction **la plus à enjeu** du métier.
> **Frontière fondatrice** : Atlas **fait remonter** des échéances de façon **assistive** ; il n'est **pas** le registre de docketing autoritaire du cabinet et ne **garantit aucune** date. C'est l'équivalent docketing du « pas d'avis juridique ».

**Description** : une **couche de veille des échéances** posée sur le portefeuille PI (F-018/F-025) : renouvellements de marques, annuités de brevets, fenêtres d'opposition, dates de priorité. Atlas **calcule/lit** les échéances à venir et **rappelle** l'utilisateur à des délais configurables — **souverainement** (auto-hébergeable) et **ouvertement** (règles open-source).

C'est **assistif**, pas autoritaire : Atlas aide à ne rien rater, mais le **devoir de docketing reste celui du cabinet**, et aucune date n'est garantie.

**Valeur user** : pour le **Cabinet PI**, l'oubli d'une échéance est la faute cardinale. Aujourd'hui le marché est binaire — suites fermées chères grands comptes (Anaqua, PATTSY) **ou** docketing open-source isolé (phpIP) non relié au registre vivant. Un docketing **assistif, souverain, ouvert et connecté** à la donnée et à la veille n'existe pas.

**Complexité** : ★★★★ — le volume de code n'est pas énorme, mais l'**ingénierie de prudence** l'est : préférence lecture / calcul, signalement des dates calculées, moteur de règles par juridiction **gouverné**, et une **UX qui rend la non-autorité impossible à manquer**.

**APIs externes** : celles de F-018/F-027 — **INPI PI**, **EUIPO** (TMview/eSearch), **OMPI**. Aucune nouvelle — on lit les dates des notices déjà récupérées.

**Dépendances** : **F-018** (favoris PI), **F-025** (portefeuille IP), **F-007/F-016** (notices marque/brevet portant les dates), **F-019** (patron job + rappels), **F-047** (timeline), **F-020** (push), **F-046** (`WatchRule`/`Alert`).

**Hors-périmètre (le cœur de la prudence)** :
- **Pas le registre de docketing autoritaire** : Atlas ne se substitue pas au système de référence du cabinet.
- **Aucune garantie** d'exhaustivité ni d'exactitude des dates ; le cabinet **conserve son devoir** professionnel.
- **Aucune action** : pas de renouvellement automatique, pas de paiement d'annuité, pas de dépôt — Atlas **rappelle**, il n'**agit** pas.
- **Couverture honnêtement limitée** aux juridictions dont la date est lue ou la règle encodée (démarrer **FR / UE**).

**Détails techniques** :
- **Modèle** : `IpDeadline` (type : `TrademarkRenewal` / `PatentAnnuity` / `OppositionWindow` / `PriorityDeadline`), portant **la date, sa source (lue vs calculée), et la base de calcul** le cas échéant.
- **Préférence lecture > calcul** : si la notice fournit la date d'expiration, on la **lit** (pas de risque de règle erronée) ; sinon **calcul** via la règle de juridiction, **marqué « calculé — à vérifier »**.
- **Moteur de règles** : règles de renouvellement/annuité **encodées par juridiction**, comme un **artefact open-source auditable** et **extensible par la communauté** — mais **gouverné/curé**, car une règle fausse est dangereuse.
- **Job Hangfire** : calcule les échéances à venir du portefeuille, émet des **rappels** (`Alert` issue d'une `WatchRule`) aux délais configurés → timeline + email + push.
- **Auditabilité** : chaque échéance affiche **d'où elle vient** (date lue de la notice, ou règle + base de calcul + date) — défendable et vérifiable.

**Cadre légal & responsabilité (prérequis bloquant)** :
- **Disclaimer de non-autorité au niveau du design** (pas en petites lignes) : outil d'**assistance**, le cabinet reste responsable de son docketing ; aucune garantie.
- **Dates calculées toujours marquées** comme telles et « à vérifier ».
- **Couverture juridictionnelle affichée** clairement (ce qui est couvert / ce qui ne l'est pas).
- Données de titres dépendant de l'INPI PI (auth complexe, lacunes possibles) → ne jamais présenter une absence de donnée comme « aucune échéance ».

**Accessibilité (ADR-008)** : échéancier et rappels pleinement accessibles ; l'**urgence** (proche de l'échéance) **jamais** signalée par la seule couleur (libellé + date explicites) ; configuration des délais navigable au clavier.

**Modèle économique (ADR-006)** : lecture/calcul **déterministe** → **cœur / OSS**. Le **moteur de règles** est open-source (et communautaire, sous gouvernance). **Quotas hébergé** possibles (taille de portefeuille surveillé).

**Découpage / jalons** :
1. **Lecture + rappels** sur les **renouvellements de marques FR/UE** dont la date figure dans la notice. *(Le plus sûr, livrable autonome.)*
2. **Annuités de brevets** (lecture quand fournie).
3. **Calcul en repli gouverné** : règles par juridiction, dates marquées « calculées ».
4. **Fenêtres d'opposition / dates de priorité** (les plus courtes, les plus critiques — en dernier, avec le plus de prudence).

**Décisions ouvertes** :
- **Lecture vs calcul** : jusqu'où encoder des règles (risque) plutôt que se limiter aux dates **lues** ?
- **Juridictions de la v1** : FR + UE seulement ?
- **Gouvernance des règles communautaires** : qui valide une règle avant publication ?
- **Délais de rappel par défaut** et personnalisation.
- **UX de non-autorité** : comment rendre le disclaimer **impossible à ignorer** sans alourdir l'usage ?
