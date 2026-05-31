# F-033 — Indicateurs environnementaux descriptifs

> **Reframé & figé le 29 mai 2026** — remplace le stub V3+ « Score ESG / Bilan carbone ». **Le mot « score » est abandonné** : le scoring ESG est une industrie controversée et non normalisée. Atlas **affiche la donnée déclarée**, il n'invente pas de note (même logique que F-054 : ratios ≠ notation). Statut **V3+ *data-gated*** : la donnée n'est pas encore là à grande échelle.

**Description** : sur la fiche d'une entreprise, Atlas affiche ses **indicateurs environnementaux déclarés** quand ils existent — au premier chef ses **émissions de gaz à effet de serre** (scopes 1, 2, 3) issues du **BEGES** publié en open data par l'**ADEME**. Descriptif, factuel, jamais agrégé en un score propriétaire.

**Valeur user** : pour les personas **investisseur** et **compliance**, et la pression ESG croissante des donneurs d'ordre — voir l'empreinte déclarée d'une entreprise sans aller fouiller des rapports. Mais c'est aujourd'hui **data-limité** (voir couverture).

**Pourquoi V3+ *data-gated*** (et pas promu comme le DECP) : le paquet **Omnibus** (en vigueur le 18 mars 2026) a **resserré la CSRD** — seuils relevés à **1000 salariés ET 450 M€** de CA, **~80 %** des entreprises initialement visées sorties du champ, PME cotées exclues, vagues suivantes repoussées à **2028** (exercice 2027). Conséquence : la donnée ESG issue de la CSRD reste **rare et repoussée**. Le motif d'origine de F-033 (« à revoir quand la CSRD sera déployée ») est **renforcé**, pas levé. **Mais** une donnée est exploitable **dès maintenant**, indépendante de la CSRD : le **BEGES**, obligatoire pour les entreprises de **plus de 500 salariés** (scopes 1, 2 et 3 significatif), publié en **open data par l'ADEME**. C'est la porte d'entrée pragmatique.

**Complexité** : ★★★, **conditionnée à la disponibilité de la donnée**. Le travail est l'ingestion ADEME + l'affichage descriptif ; le facteur limitant est la couverture, pas le code.

**APIs externes** :
- **ADEME — Bilan GES** (open data) : émissions déclarées des entreprises françaises > 500 salariés. Source réaliste **aujourd'hui** (déjà cataloguée doc 03 §8.2).
- **Futur** : rapports CSRD (vague 1 déjà publiés par les très grandes entreprises) et, à terme, l'**ESAP** (European Single Access Point) qui agrégera les données de durabilité.

**Dépendances** : F-004 (fiche entreprise), ADR-006 (open core), ADR-004 (archi hexagonale).

**Hors périmètre (explicite)** :
- **Aucun score / notation / rating ESG** propriétaire.
- **Aucune donnée inventée** ou estimée là où rien n'est déclaré.

**Détails techniques** :
- Adapter d'ingestion des données ADEME BEGES, rapprochées par **SIREN**.
- Affichage descriptif (émissions par scope, évolution si plusieurs millésimes).
- Déterministe → **cœur open source**, aucun coût par utilisateur.
- Aucune dépendance lourde ; persistance EF Core / PostgreSQL existante.

**Couverture (caveat honnête)** : seules les entreprises **assujetties au BEGES** (> 500 salariés) déclarent → **la grande majorité des entreprises n'aura aucune donnée**. À afficher proprement (« non disponible »), comme la confidentialité pour F-054 ou le caractère coté pour F-051. C'est ce qui justifie le statut **data-gated**.

**Cadre légal & positionnement** : open data public, descriptif (données déclarées) → risque faible, pas de RGPD spécifique. Même ligne que le financier : on **montre la donnée**, on ne **note** pas.

**Accessibilité (rappel ADR-008)** : émissions et évolutions en tableau lisible au lecteur d'écran ; aucune information (ex. tendance) transmise par la couleur seule.

**Modèle économique** : déterministe, source gratuite → **cœur open source** (cohérent ADR-006).

**Découpage / jalons** :
1. **ADEME BEGES** : ingestion + matching SIREN + affichage descriptif des émissions. *(Cœur de la feature, livrable seul.)*
2. **Enrichissement futur** : rapports CSRD / ESAP quand la donnée sera disponible à plus grande échelle.

**Décisions ouvertes** :
- **Quand rouvrir** : suivre la maturité de la donnée (calendrier CSRD post-Omnibus, déploiement ESAP).
- **Autres jeux ADEME** éventuels à intégrer.
