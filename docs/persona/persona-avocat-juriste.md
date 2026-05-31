# Persona — Avocat / juriste d'affaires

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #7).
> **Statut** : confirmé comme persona à part entière (≠ Cabinet PI). Déroulé le 29 mai 2026.
> **Lentille distinctive (verrouillée)** : **risque juridique + documents légaux (actes/statuts) + jurisprudence + validité transactionnelle**. À ne pas confondre avec la Compliance (screening **réglementaire récurrent**) ni l'Investisseur (**valeur & activité**). Trois angles parfois sur le même dossier, trois lentilles différentes.

---

## 1. En une phrase
Un juriste qui **sécurise et défend** les intérêts d'une entreprise dans ses opérations (M&A, contrats, restructurations) et ses litiges — corporate et contentieux des affaires.

## 2. Contexte & enjeux métier
- Intervient sur des **opérations** (acquisition, levée, cession, contrat) et des **contentieux** (commercial, sociétés).
- Doit produire une analyse **défendable et sourcée** — il **engage sa responsabilité**.
- Très **contraint par le temps** sur la due diligence juridique (data room, délais serrés).
- Soumis au **secret professionnel** et à une déontologie stricte.
- Sa valeur est dans l'**analyse**, pas dans la collecte — il veut arrêter de courir après les pièces.

## 3. Jobs-to-be-done
- « Vérifie l'historique et la validité juridique d'une cible avant la transaction. »
- « Trouve-moi la jurisprudence impliquant cette entreprise. »
- « Préviens-moi d'un contentieux ou d'une procédure touchant mon client / sa contrepartie. »
- « Constitue le volet *factuel* d'une due diligence juridique. »

## 4. Besoins clés
- Accès rapide aux **actes & statuts** (qui a signé quoi, quand, quelles clauses).
- **Jurisprudence rattachée à une entité** (pas seulement par mots-clés).
- **Signaux de risque légal** : procédures collectives, contentieux.
- Compréhension de la **structure de groupe** (liens entre entités).
- **Traçabilité** de bout en bout (chaque pièce sourcée et datée).

## 5. Usage d'Atlas
- **Fiche entreprise + actes/statuts (F-013)** : la matière première documentaire.
- **Signaux de risque (F-055)** : procédures (BODACC), et — si réactivé — contentieux (Judilibre).
- **Graphe de co-mandats descriptif (F-034)** : comprendre la structure et les liens, **sans qualification** (doctrine ADR-012).
- **Watchlists (F-053)** : suivre clients et contreparties d'un dossier.
- **Indicateurs financiers (F-054)** : contexte de valeur/solidité d'une cible.
- **Pack de veille « juridique d'affaires »** *(à créer)* : Légifrance (droit des sociétés/affaires), Cour de cassation (chambre commerciale), JADE (Conseil d'État), EUR-Lex, Dalloz Actualité.

## 6. Données & sources qui comptent
RNE (actes, statuts, dirigeants) · **Judilibre** (jurisprudence judiciaire) · **JADE** (juridictions administratives — utile sur les marchés publics annulés) · Légifrance (textes) · BODACC (procédures) · DECP (marchés, et leurs contentieux).

## 7. Ce que la concurrence ne sert pas
Deux mondes qui ne se parlent pas : les **outils de données d'entreprise** (type Pappers/Societe) n'ont **aucune profondeur jurisprudentielle** ; les **moteurs de jurisprudence** (legaltech) **ne partent pas de l'entité** et ne croisent pas identité, structure et risque. **Personne ne réunit** « identité légale + actes + structure + jurisprudence + signaux de risque » en une vue descriptive **autour d'une entreprise**. C'est exactement ta thèse — *croiser ce que personne ne croise* — appliquée au juridique.

## 8. Champs inexplorés & game-changers
- **Le dossier de due diligence juridique *descriptif*** `[agentique]` `[confiance]` `[temps]` : un assistant qui, autour d'une cible, rassemble en un dossier **sourcé et daté** : actes/statuts (F-013) + structure (graphe descriptif F-034) + jurisprudence rattachée (Judilibre) + procédures (BODACC) + marchés publics (DECP). Ce n'est **pas un avis juridique** — Atlas ne conseille jamais — c'est un **dossier de faits** que l'avocat analyse. Game-changer : aujourd'hui ce dossier se monte à la main, à travers cinq outils.
- **La jurisprudence rattachée à l'entité** `[confiance]` : croiser le nom de l'entreprise (personne morale, non pseudonymisée) avec Judilibre → « les décisions où cette boîte apparaît ». **Matching conservateur** (variantes de raison sociale, homonymes), formulation « mention potentielle à vérifier » (doctrine ADR-012). Les moteurs de jurisprudence ne partent jamais de l'entité ; Atlas, si.
- **Le cabinet souverain** `[souveraineté]` : Atlas auto-hébergé, les données de dossier ne sortent jamais — argument déontologique fort pour une profession au secret professionnel.
- **Le temps juridique** `[temps]` : rejouer l'historique des actes et de la gouvernance d'une boîte (qui a signé quoi, quand) — précieux en contentieux comme en transaction.

---

## Implication produit : réexaminer F-031 (Judilibre)

Comme pour le DECP, le motif de mise en sommeil de F-031 (« matching entre entités non-trivial ») **s'est affaibli** :
- Le **corpus s'est enrichi** : Cour de cassation (~535k décisions), arrêts de **cours d'appel civils/commerciaux depuis avril 2022**, tribunaux judiciaires civils en cours de déploiement. API gratuite via PISTE.
- Pour les **personnes morales**, le nom est généralement **conservé** (la pseudonymisation vise les personnes physiques) → le matching par entité redevient **faisable**.

⚠️ **À manier prudemment** (sous doctrine ADR-012) : décisions **pseudonymisées** pour les personnes physiques (on ne reconstitue jamais l'identité d'un particulier) ; **matching conservateur** sur la raison sociale ; **descriptif** — on liste les décisions où l'entité est mentionnée, **jamais** de « score de contentieux » ni d'interprétation.

## Garde-fou de positionnement (transversal)
Atlas **ne rend aucun avis juridique** (exercice du droit réservé + responsabilité). Il **rassemble des faits sourcés** ; l'avocat analyse. Même ligne que partout : on montre, on ne juge pas.

---

*Persona figé le 29 mai 2026. Lentille verrouillée (juridique documentaire + jurisprudentiel + transactionnel). Soulève un réexamen possible de F-031 (Judilibre), à traiter sous ADR-012. Prochain persona : au choix.*
