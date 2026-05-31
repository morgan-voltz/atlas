# Persona — Cabinet de Propriété Industrielle

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #2).
> **Statut** : déroulé le 29 mai 2026.
> **Lentille distinctive** : **gérer et défendre des *titres* de PI** (marques, brevets, dessins & modèles) comme des actifs — classifications, **échéances**, antériorité, oppositions, surveillance des dépôts. ≠ Avocat d'affaires (corporate/contentieux) et ≠ Investisseur (valeur). C'est presque un **persona-cœur** : Atlas est né autour de l'INPI PI.

---

## 1. En une phrase
Un spécialiste qui **dépose, gère et défend** des titres de propriété industrielle pour le compte de ses clients.

## 2. Contexte & enjeux métier
- Gère des **portefeuilles de titres**, souvent **multi-juridictions** (FR / UE / international).
- Les **échéances sont vitales** : un renouvellement de marque ou une annuité de brevet manqué = **perte du droit** = faute professionnelle. C'est la fonction la plus à enjeu.
- Vérifie l'**antériorité** avant un dépôt (marque proche existante, art antérieur).
- Suit les **oppositions** et contentieux PI.
- Surveille les **dépôts des concurrents** (nouvelles marques/brevets sur son secteur).
- Soumis au secret professionnel ; sa valeur est dans l'**analyse et la stratégie**, pas la saisie.

## 3. Jobs-to-be-done
- « Surveille les nouveaux dépôts de mes concurrents, en France et à l'international. »
- « Vérifie l'antériorité avant que mon client ne dépose. »
- « Ne me laisse **jamais** rater une échéance de renouvellement / d'annuité. »
- « Donne-moi l'état de mon portefeuille (statuts, oppositions). »
- « Relie un titre à la situation de son titulaire. »

## 4. Besoins clés
- **Gestion de portefeuille + docketing** (échéances, rappels) — le nerf de la guerre.
- **Recherche d'antériorité** (similarité phonétique / sémantique / figurative).
- **Veille des dépôts** concurrents, multi-offices.
- Données **cross-office** (INPI + EUIPO + OMPI + EPO) au même endroit.
- Suivi des **statuts et oppositions**.

## 5. Usage d'Atlas
- **Marques (F-006/F-007)** et **brevets (F-015/F-016)** : recherche + notices + images.
- **Favoris PI (F-018)** + **tableau de bord portefeuille IP (F-025)** : la vue portefeuille avec calendrier des renouvellements.
- **Antériorité avec matching intelligent (F-026)** : similarité au-delà de l'exact.
- **Veille PI automatisée (F-027)** + **timeline (F-047)** : alertes sur les nouveaux dépôts ciblés.
- **EUIPO/OMPI (F-039)** et **brevets mondiaux (F-040)** : l'ouverture cross-office.
- **Pack de veille « Cabinet PI »** (doc 07) : INPI Actualités, BOPI, IPKat, EUIPO/WIPO/EPO News, Cour de cassation (PI), blogs de cabinets.

## 6. Données & sources qui comptent
INPI PI (marques, brevets, D&M, **BOPI**) · **EUIPO** (TMview ~112M marques, eSearch plus, recherche d'image par IA) · **OMPI** (Global Brand Database, Madrid Monitor, PATENTSCOPE) · **EPO** (Espacenet ~140M brevets, API OPS) · Cour de cassation (décisions PI) · classifications (Nice, Vienne, CPC/IPC, Locarno).

## 7. Ce que la concurrence ne sert pas
Trois mondes, aucun complet : les **suites de gestion PI** (Anaqua, PATTSY, AppColl…) sont **fermées, chères, complexes et orientées grands comptes / US** ; les **outils gratuits des offices** (TMview, Espacenet) sont du **search siloté par office, sans gestion de portefeuille ni docketing** ; et l'**open-source existant** (phpIP) fait du docketing **standalone** — non relié au registre vivant, non croisé avec la donnée entreprise, sans veille. **Personne** ne réunit, dans l'écosystème français : **souverain + ouvert + cross-office + croisé avec l'identité de l'entreprise + veille + descriptif + accessible.**

## 8. Champs inexplorés & game-changers
- **Le docketing souverain & open-source** `[souveraineté]` `[ouverture]` : la gestion des échéances (la fonction la plus à enjeu) dans un outil auto-hébergeable et ouvert, **relié au registre vivant et à la veille** — là où la concurrence est soit chère et fermée, soit du docketing isolé. ⚠️ **Garde-fou** : Atlas **surface** les échéances de façon descriptive, il ne **remplace pas** le devoir de docketing du cabinet et ne **garantit** rien (responsabilité). Même esprit que « pas d'avis juridique ».
- **La veille PI cross-office native** `[temps]` `[agentique]` : surveiller les dépôts concurrents sur INPI + EUIPO + OMPI en un flux unifié dans la timeline (F-027 + F-047) — « qui dépose quoi dans mon secteur », au-delà d'un seul office.
- **L'antériorité assistée descriptive** `[confiance]` `[agentique]` : F-026 — faire remonter des **candidats** similaires (phonétique, sémantique, image) à examiner. EUIPO a déjà de la recherche d'image par IA et un outil de similarité des produits/services (« guide, non concluant »). Atlas **présente des candidats**, ne rend **jamais** un verdict de disponibilité — c'est l'analyse du conseil. Brique IA → premium (patron F-050).
- **Le titre croisé avec son titulaire** `[confiance]` : relier un titre PI à la situation RNE de son titulaire (le titulaire est-il en procédure collective ? → risque sur l'actif). Les outils PI voient les *titres*, pas l'*entreprise* derrière. Toi, les deux.
- **Auditabilité & accessibilité** `[confiance]` `[accessibilité]` : chaque donnée tracée à son office source ; et un outil PI réellement **accessible**, là où l'existant est « visuellement daté ».

---

## Implications produit
Ce persona est la **colonne vertébrale** de plusieurs features déjà à la roadmap : **F-025** (portefeuille IP), **F-026** (antériorité), **F-027** (veille PI), **F-039** (EUIPO/OMPI), **F-040** (brevets mondiaux). L'exploration ajoute un vecteur neuf : le **docketing souverain** (gestion d'échéances) — candidat à une future fiche, à cadrer avec le garde-fou de responsabilité.

## Garde-fous de positionnement
- **Docketing** : descriptif et assistant ; ne remplace pas le devoir professionnel du cabinet, ne garantit aucune échéance.
- **Antériorité** : Atlas fait remonter des candidats ; il ne conclut **jamais** à la disponibilité d'une marque (acte d'analyse réservé au conseil).
- **Technique** : l'auth de l'API INPI PI est la plus complexe de l'écosystème (doc 03 §1.2) ; les conditions de réutilisation de TMview/Espacenet sont à vérifier source par source.

---

*Persona figé le 29 mai 2026. Persona-cœur, bien servi par la roadmap PI existante ; l'exploration ouvre le vecteur « docketing souverain ». Prochain persona : au choix.*
