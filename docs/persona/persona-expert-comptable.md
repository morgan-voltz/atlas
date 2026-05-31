# Persona — Expert-comptable

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #1). *Premier persona déroulé — il a servi de modèle au gabarit, désormais logé comme les autres.*
> **Statut** : déroulé le 29 mai 2026.
> **Lentille distinctive** : suivre un **portefeuille de sociétés clientes** dans leur vie **légale, financière et leurs échéances** — un suivi **récurrent et fiable**, complémentaire de la production comptable. ≠ Compliance (obligation LCB-FT sur des tiers), ≠ Investisseur (valeur d'une cible), ≠ Avocat (validité juridique / contentieux).

---

## 1. En une phrase
Un professionnel du chiffre qui suit un **portefeuille de sociétés clientes** et doit, pour chacune, rester à jour sur sa situation légale, financière et ses obligations.

## 2. Contexte & enjeux métier
- Gère **dizaines à centaines** d'entreprises clientes en parallèle.
- Rythmé par des **échéances** (clôtures, dépôts de comptes, assemblées) — la « saison des comptes » est un pic d'activité.
- Soumis au **secret professionnel** et à une exigence de fiabilité : une donnée fausse dans un dossier, ça se paie.
- Se différencie sur la **valeur ajoutée conseil**, pas sur la saisie — il veut passer moins de temps à *collecter* et plus à *analyser*.

## 3. Jobs-to-be-done
- « Préviens-moi quand un de mes clients a un changement légal (dirigeant, siège, procédure). »
- « Donne-moi vite les comptes et actes d'une boîte avant un rendez-vous. »
- « Montre-moi la trajectoire financière d'un client sur plusieurs exercices. »
- « Aide-moi à passer la saison des comptes sans rien rater sur mon portefeuille. »

## 4. Besoins clés
- **Suivi de portefeuille** d'entreprises, organisé (watchlists).
- **Alertes** fiables sur les évolutions (RNE, BODACC).
- Accès rapide aux **bilans et actes**.
- **Indicateurs financiers** lisibles et **traçables** (défendables dans un dossier).

## 5. Usage d'Atlas
- **Watchlists (F-053)** : une liste par typologie de clients, import en masse des SIREN.
- **Favoris + rafraîchissement (F-017/F-019)** et **timeline (F-047)** : changements légaux poussés automatiquement.
- **BODACC (F-048)** : procédures collectives, dépôts de comptes.
- **Bilans (F-013)** + **indicateurs financiers descriptifs (F-054)** : ratios + tendances, avec provenance.
- **Pack de veille « Expert-comptable »** (doc 07) : BODACC, Légifrance, INSEE, Banque de France…

## 6. Données & sources qui comptent
RNE (identité, dirigeants, actes, bilans) · BODACC (procédures, dépôts) · ratios BCE/INPI + bilans-saisis · Légifrance (droit des sociétés, fiscalité) · INSEE.

## 7. Ce que la concurrence ne sert pas
Les outils du métier (Pennylane, Cegid, Sage, MyUnisoft, ACD…) sont des logiciels de **production comptable** : ils regardent **vers l'intérieur** du cabinet — saisie, révision, liasse fiscale, facture électronique (réforme Factur-X 2026), et de plus en plus d'IA sur l'OCR et la catégorisation. Ils gèrent la donnée que le cabinet **détient déjà**. Aucun ne **surveille la vie légale et registre *externe*** des sociétés clientes (procédures, changements de dirigeants, dépôts) comme une **couche de veille croisée**. Les outils de donnée entreprise (type Pappers) font de la consultation/surveillance, mais **à l'unité**, pas comme une couche d'intelligence **souveraine et cross-source** sur tout le portefeuille. **Atlas n'est donc pas un concurrent de la production comptable — il en est le complément** : la couche de **monitoring externe descriptif** du portefeuille.

## 8. Champs inexplorés & game-changers
- **Le « copilote de saison des comptes »** `[agentique]` `[souveraineté]` : un assistant qui, au moment des clôtures, parcourt tout le portefeuille via le serveur MCP, repère ce qui a bougé, rassemble les pièces, et **prépare une note descriptive par client** — sur l'infra du cabinet, avec ses propres credentials INPI. Aucun SaaS fermé ne laissera l'IA du comptable fouiller ainsi sa base.
- **Le portefeuille dans le temps** `[temps]` : rejouer la trajectoire d'un client sur N exercices, détecter une dérive (trésorerie, retards de dépôt) **avant** qu'elle ne devienne un problème — une vue longitudinale que personne n'offre.
- **Le dossier auditable** `[confiance]` : chaque chiffre remonte à sa source INPI datée, chaque ratio à sa formule publique → une pièce **défendable** face à un client, un associé, un contrôle. Les scores boîte noire ne peuvent pas.
- **Le cabinet souverain** `[souveraineté]` : Atlas auto-hébergé dans le cabinet, la donnée client ne sort jamais — argument fort pour une profession au secret professionnel.
- **Pack de veille communautaire** `[ouverture]` : un pack « expert-comptable » entretenu collectivement par la profession, qui s'améliore tout seul.

---

## Implications produit
Ce persona est un **moteur de plusieurs features cœur** : **F-053** (watchlists = le portefeuille de clients), **F-019** (rafraîchissement des favoris = le moteur d'alertes), **F-054** (indicateurs financiers), **F-048** (BODACC), **F-047** (timeline). Le **« copilote de saison des comptes »** est le **cas d'usage roi de l'agentique/MCP (F-052)** côté métier du chiffre. Piste : une **vue « monitoring externe du portefeuille »** (l'état légal/financier de tous les clients d'un coup d'œil), proche de la *vue 360* mais à l'échelle du portefeuille plutôt que d'une cible.

## Frontières
- **Pas un outil de production comptable** : ni saisie, ni révision, ni liasse, ni Factur-X — c'est le terrain de Pennylane/Cegid. Atlas est **complémentaire**, pas concurrent (gap de couverture assumé).
- **Descriptif, pas de conseil** : Atlas fournit faits et indicateurs ; l'expert-comptable analyse et conseille.
- **Confidentialité des comptes** (F-054) : beaucoup de TPE/PME déposent des comptes confidentiels → données financières partielles.
- Données de dirigeants sous **ADR-012**.

---

*Persona figé le 29 mai 2026. Premier persona déroulé (ex-modèle), désormais homogène avec les six autres. Complément de la production comptable ; game-changer = le copilote de saison des comptes.*
