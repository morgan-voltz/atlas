# Persona — Investisseur / M&A

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #4).
> **Statut** : déroulé le 29 mai 2026. **Le persona-convergence** : il croise plus de features qu'aucun autre (F-051, F-054, F-032, F-034, F-053, F-055, F-013, F-018).
> **Lentille distinctive** : évaluer la **valeur et l'activité réelle** d'une cible pour décider d'un **investissement / d'une acquisition**, et **suivre un portefeuille** de cibles et de participations. ≠ Avocat (validité juridique / contentieux), ≠ Compliance (risque réglementaire), ≠ Veille concurrentielle (marché).

---

## 1. En une phrase
Un professionnel qui **évalue** des entreprises pour investir ou acquérir, et qui **surveille** un portefeuille de cibles et de participations.

## 2. Contexte & enjeux métier
- Trois temps : **sourcing** (trouver des cibles), **due diligence** (évaluer), **suivi de portefeuille** (post-investissement).
- Décisions **à fort enjeu**, sous **contrainte de temps**.
- A besoin de **données fiables sur les PME françaises** — précisément là où les gros outils (centrés startups/grands comptes) sont faibles.
- Raisonne en **trajectoire** (évolution dans le temps), pas en photo instantanée.
- Son **pipeline de deals est confidentiel** → enjeu de souveraineté.

## 3. Jobs-to-be-done
- « Trouve-moi des cibles qui correspondent à ma thèse (secteur, taille, dynamique). »
- « Donne-moi le portrait complet d'une cible : finances, activité réelle, structure, risques. »
- « Surveille mon portefeuille et mon pipeline, préviens-moi de ce qui bouge. »
- « Suis les mouvements de mon secteur (levées, cessions, dépôts). »

## 4. Besoins clés
- **Portrait cross-source** d'une cible (le croisement, pas la donnée isolée).
- **Couverture fiable des PME françaises**.
- **Suivi de portefeuille / pipeline** dans le temps.
- **Trajectoire** pluriannuelle.
- Compréhension de la **structure et du contrôle** (dans les limites légales).

## 5. Usage d'Atlas (la convergence)
- **Indicateurs financiers (F-054)** : trajectoire financière de la cible.
- **Marchés publics (F-032)** : activité réelle (qui gagne quoi).
- **PI (F-018/F-025)** : actifs immatériels.
- **Graphe de co-mandats (F-034)** : structure et liens — **descriptif, sans bénéficiaires effectifs**.
- **Signaux de risque (F-055)** : risques sur la cible.
- **Cotation (F-051)** : si la cible est cotée.
- **Watchlists (F-053)** : le pipeline et le portefeuille.
- **Timeline (F-047)** + **pack veille « Investisseur / M&A »** (doc 07) : mouvements du secteur.
- **Bilans & actes (F-013)** : pièces de due diligence.

## 6. Données & sources qui comptent
RNE (identité, dirigeants) · ratios BCE/INPI + bilans-saisis · DECP · INPI PI · BODACC + sanctions (risque) · GLEIF/ISIN (coté) · presse.

## 7. Ce que la concurrence ne sert pas
Les plateformes (PitchBook ~10–25k$/an, Crunchbase, Dealroom, CB Insights, Grata) sont **chères, fermées, cloud, centrées startups/grands comptes**, et leur donnée **PME privée est peu fiable**. Affinity fait du **CRM relationnel** ; FactSet/Bloomberg, du **marché coté**. **Aucune** n'ancre son intelligence dans le **registre officiel français** pour offrir, sur le **tissu PME**, une vue **cross-source, descriptive et souveraine**. Le trou est **double** : couverture (PME françaises) **et** modèle (officiel + souverain + descriptif + croisé).

## 8. Champs inexplorés & game-changers
- **La vue 360 / dossier cible** `[confiance]` `[agentique]` `[temps]` — *le* game-changer : assembler autour d'une cible, **à partir de sources officielles**, identité (RNE) + trajectoire financière (F-054) + activité réelle (DECP, PI) + structure (F-034 descriptif) + signaux de risque (F-055) + cotation (F-051) — **sourcé, daté, descriptif**. Personne ne croise tout ça pour une PME française. C'est la matérialisation de ta thèse « croiser ce que personne ne croise ».
- **La couverture fiable du tissu PME** `[confiance]` : là où PitchBook/Crunchbase ont une donnée privée peu fiable, Atlas s'ancre dans le **registre officiel** → fiable sur les PME que les gros couvrent mal. Un avantage de **couverture**, pas seulement de feature.
- **Le suivi de portefeuille/pipeline souverain** `[souveraineté]` `[temps]` : surveiller pipeline + participations (watchlists + veille) en continu, **sur son infra**, le pipeline confidentiel ne sortant jamais — là où l'équivalent (le tableau de bord portefeuille de Pappers) est hébergé et fermé.
- **L'assistant deal agentique** `[agentique]` : un agent qui, sur une thèse ou une cible, **assemble le dossier descriptif** ou **surveille le pipeline** — sur l'infra du fonds. Le marché va vers l'« AI due diligence » et le « thesis-matched sourcing » ; Atlas en offre une version **souveraine et descriptive**.
- **La trajectoire dans le temps** `[temps]` : rejouer l'évolution d'une cible (finances, activité, dépôts) — l'investisseur pense trajectoire, pas instantané.

---

## Implication produit majeure : la « vue 360 » comme méta-feature
Ce persona est l'**argument le plus fort** pour une **méta-feature d'assemblage** : la *vue 360 / dossier cible* qui **orchestre** F-051 + F-054 + F-032 + F-034 + F-055 + F-018 en une seule vue descriptive, sourcée et temporelle. Ce n'est **pas une nouvelle source** — c'est la **couche de convergence**. C'est aussi le cas d'usage roi de l'angle **agentique/MCP**. Candidate sérieuse à une future fiche.

## Frontières (cohérentes avec la doctrine)
- **Aucun conseil en investissement**, aucune **recommandation**, aucun **verdict de valorisation**, aucune **prédiction de performance** (là où le marché IA y va) — **descriptif uniquement**, l'investisseur décide (ligne F-051).
- **Pas de bénéficiaires effectifs** : Atlas montre le **co-mandat descriptif** (dirigeants/mandats publics), **pas** la propriété effective (Sovim). Gap honnête.
- **Caveats de couverture** : confidentialité des comptes (F-054), faible part de cotées (F-051), seuil DECP (F-032).
- **Pas un CRM relationnel** (territoire Affinity / F-037) — hors cœur.
- Données de dirigeants sous **ADR-012**.

---

*Persona figé le 29 mai 2026. Persona-convergence : son existence justifie la « vue 360 / dossier cible » comme méta-feature d'assemblage. Descriptif, jamais de verdict de valorisation. Restent : Veille concurrentielle B2B, Développeur / Tech curieux.*
