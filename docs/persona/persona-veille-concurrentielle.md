# Persona — Veille concurrentielle B2B

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #5).
> **Statut** : déroulé le 29 mai 2026. **Persona-cœur du cluster veille** : c'est *pour lui* qu'a été conçue la **timeline mixte (F-047)**, ta feature différenciante (ADR-009).
> **Lentille distinctive** : conscience **secteur & concurrents** pour **décider de sa stratégie**. ≠ Compliance (obligation réglementaire), ≠ Investisseur/Avocat (transaction), ≠ Cabinet PI (gestion d'actifs PI).

---

## 1. En une phrase
Un professionnel (stratégie, marketing, business dev) qui **surveille ses concurrents et son secteur** pour rester en avance et nourrir ses décisions.

## 2. Contexte & enjeux métier
- Doit savoir **ce que font les rivaux** : nouveaux produits (signalés par des dépôts PI), marchés remportés, mouvements légaux, recrutements, presse.
- L'ennemi, c'est la **surcharge d'information** — il faut transformer du signal en décision.
- Environnement **rapide** : l'info qui arrive trop tard ne vaut rien.
- Pas d'obligation légale qui le force ; c'est un **avantage compétitif** qu'il se construit.

## 3. Jobs-to-be-done
- « Préviens-moi quand un concurrent bouge (dépôt, marché, changement légal, presse). »
- « Surveille mon secteur : nouveaux entrants, tendances. »
- « Donne-moi le **signal**, pas le **bruit**. »
- « Fais-moi la synthèse de ce qui a bougé cette semaine. »

## 4. Besoins clés
- **Veille multi-sources** sur un set de concurrents + un secteur.
- **Croisement** signaux officiels (registre, PI, marchés) **+** presse.
- **Dédup / filtres / pertinence** (signal vs bruit).
- **Synthèse / digest**.

## 5. Usage d'Atlas
- **Timeline mixte (F-047)** — *le* cœur : flux RSS + événements RNE/BODACC + favoris, **croisés** chronologiquement.
- **Moteur veille (F-041 à F-046)** : RSS, packs, sources libres, dédup (F-045), filtres/règles (F-046).
- **Veille PI automatisée (F-027)** : dépôts marques/brevets des concurrents.
- **Watchlists (F-053)** : la liste de concurrents à suivre.
- **Marchés publics (F-032)** : les appels d'offres remportés par les concurrents.
- **PI (F-018/F-025)** + **BODACC (F-048)** : actifs et événements légaux du secteur.
- **Pack de veille « Veille concurrentielle B2B »** (doc 07) : BODACC (secteur), INPI (dépôts concurrents), Les Échos, L'Usine Digitale, Maddyness, Frenchweb, Capital, Forbes France.
- **Premium (F-050)** : résumés automatiques, scoring de pertinence, synthèse hebdomadaire.

## 6. Données & sources qui comptent
RNE (changements) · BODACC (événements légaux du secteur) · INPI PI (dépôts concurrents) · DECP (marchés remportés) · presse / RSS sectorielle.

## 7. Ce que la concurrence ne sert pas
Les plateformes de CI (Crayon, Klue, Kompyte, Contify) — **15 à 60k$/an** — surveillent l'**empreinte digitale** : sites, pages de prix, offres d'emploi, avis, réseaux sociaux, presse ; elles font battlecards, win/loss, intégration CRM. Les outils génériques (Feedly, Owler, Visualping) font de la veille presse/site, pas chère mais **sans croisement avec la donnée entreprise**. **Aucun** n'ancre la veille dans la **donnée officielle française** (registre, PI, marchés). **Aucun** ne réunit, dans **une seule timeline croisée avec le set suivi**, les **événements registre** d'un concurrent + ses **dépôts PI** + ses **marchés remportés** + la **presse**. C'est précisément **F-047**.

## 8. Champs inexplorés & game-changers
- **La timeline concurrentielle cross-source** `[temps]` `[confiance]` — *le* game-changer : F-047 élargie au secteur, où un **dépôt PI**, un **marché DECP remporté** ou un **événement légal** deviennent des **signaux concurrentiels de premier rang**, alignés chronologiquement avec la presse et croisés avec les concurrents suivis. Personne ne combine événements officiels structurés **et** presse pour la CI.
- **Le signal contre le bruit** `[confiance]` : dédup (F-045) + filtres/règles (F-046) + IA premium de pertinence/synthèse (F-050). La surcharge est la douleur n°1 de la CI ; Atlas **fait remonter le pertinent**, sans rendre de **verdict** sur le concurrent.
- **La veille concurrentielle souveraine** `[souveraineté]` : ta liste de concurrents **révèle ta stratégie**. En auto-hébergé, elle reste privée — là où la confier à un fournisseur CI cloud, c'est exposer ses intentions.
- **Le digest hebdo agentique** `[agentique]` `[temps]` : un agent / une synthèse « ce qui a bougé dans mon secteur cette semaine » — un briefing descriptif tiré du set suivi (premium F-050).
- **Les packs sectoriels communautaires** `[ouverture]` : des packs de veille par secteur, **curés par la communauté** (marketplace de templates, F-042 / ADR-009) — qui s'améliorent collectivement.

---

## Implications produit
Ce persona est la **raison d'être du cluster veille** (F-041 à F-050, **F-047** en tête) — déjà construit, et **cœur différenciant** d'Atlas (ADR-009). L'exploration ajoute trois pistes : (1) **enrichir la timeline de « signaux concurrentiels »** de premier rang (dépôt PI, marché DECP, événement légal traités comme tels) ; (2) le **digest / synthèse** (premium F-050) ; (3) les **packs sectoriels communautaires** (F-042).

## Frontières
- **Descriptif** : on fait remonter des événements et de la presse ; **pas** de « score de menace » concurrentielle ni de verdict stratégique (là où les plateformes CI parlent de « threats »).
- **Pas de scraping** des sites / prix / offres d'emploi des concurrents : le modèle RSS + données officielles **ne scrape pas**. C'est un **gap de couverture honnête** face à Crayon — assumé.
- **Matching presse conservateur** : le nom d'un concurrent peut coïncider avec un contenu sans rapport (homonymes) → prudence (F-047, volet 1).
- **Hygiène des sources** ajoutées par l'utilisateur (anti-SSRF, F-043). Données de dirigeants sous **ADR-012**.

---

*Persona figé le 29 mai 2026. Persona-cœur du cluster veille ; le game-changer (timeline cross-source) est déjà ta thèse différenciante. Reste : Développeur / Tech curieux.*
