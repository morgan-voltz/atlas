# Persona — Compliance / KYC / Anti-fraude

> **Fichier dédié** (rattaché à `carte-personas.md`, persona #3).
> **Statut** : déroulé le 29 mai 2026. **Le persona le plus sensible** — c'est ici que la tentation du marché pousse vers tout ce que la doctrine **ADR-012** et les contraintes légales **interdisent**. Les frontières priment.
> **Lentille distinctive** : **vérification & surveillance réglementaire récurrente** de ses tiers, **dictée par une obligation légale** (LCB-FT), avec **piste d'audit** pour le superviseur. ≠ Avocat (transaction/contentieux ponctuel) et ≠ Investisseur (valeur).

---

## 1. En une phrase
Un professionnel qui **vérifie et surveille en continu ses tiers** (clients, fournisseurs) pour satisfaire ses obligations anti-blanchiment (LCB-FT) et prévenir la fraude.

## 2. Contexte & enjeux métier
- **Entité assujettie** (banque, fintech, notaire, avocat, secteurs nouvellement concernés…) sous **obligation légale** LCB-FT.
- Pression réglementaire **montante** : paquet AML 2024, règlement **AMLR applicable le 10 juillet 2027**, supervision **AMLA dès 2028**.
- Cycle : **vérification à l'entrée en relation + surveillance continue** (re-screening).
- Doit produire une **décision défendable** avec **piste d'audit** pour le superviseur (ACPR/AMLA).
- Tension permanente : **faux positifs** coûteux (alertes ingérables) vs **faux négatifs** dangereux (sanction).
- Enjeu de **souveraineté** : envoyer sa liste de clients à un fournisseur cloud US est un risque en soi.

## 3. Jobs-to-be-done
- « Vérifie l'identité légale d'un tiers avant l'entrée en relation. »
- « Ce tiers est-il sous sanctions / gel des avoirs ? »
- « Surveille-le en continu et préviens-moi si quelque chose change. »
- « Documente une décision **défendable** devant le superviseur. »
- « Aide-moi à comprendre la structure / les liens d'une entité. »

## 4. Besoins clés
- **Vérification d'identité** (RNE).
- **Screening sanctions** (sources officielles).
- **Surveillance continue / re-screening** d'un portefeuille de tiers.
- **Traçabilité / piste d'audit** (chaque signal sourcé et daté).
- Compréhension de la **structure** d'une entité.
- **Souveraineté** des données.

## 5. Usage d'Atlas
- **Signaux de risque (F-055)** : sanctions DG Trésor / UE + procédures collectives (BODACC) + mentions presse — **descriptif, matching conservateur** (ADR-012).
- **Watchlists (F-053)** : le portefeuille de tiers à surveiller en continu.
- **Timeline (F-047)** : les changements poussés au fil de l'eau.
- **Graphe de co-mandats descriptif (F-034)** : comprendre la structure — **sans** bénéficiaires effectifs, **sans** verdict.
- **RNE** : identité légale, dirigeants.
- **Pack de veille « Compliance / KYC »** (doc 07) : CNIL, ANSSI/CERT-FR, EUR-Lex, BODACC, Cour de cassation.

## 6. Données & sources qui comptent
RNE (identité, dirigeants) · **Registre national des gels DG Trésor** + liste consolidée UE (sanctions officielles, gratuites) · BODACC (procédures) · presse (veille) · Cour de cassation.

## 7. Le trou — et l'honnêteté comme positionnement
Les gros acteurs (ComplyAdvantage, World-Check, Dow Jones) font du **scoring + UBO + PEP + case management** : propriétaire, cloud, et souvent un **score boîte noire**. Le marché va d'ailleurs vers l'**explicabilité** (ComplyAdvantage met en avant un « journal expliquant le raisonnement »).

**Atlas n'est PAS une suite KYC/AML** — et il faut le dire franchement (voir Frontières). Mais c'est exactement ce qui crée le trou qu'il peut occuper : une **couche de signaux souveraine, descriptive et auditable**, alimentée par des **sources officielles**, que l'agent de conformité **évalue lui-même**. Pas un verdict opaque : des **faits sourcés et défendables**. Et l'**auditabilité** + la **souveraineté** répondent pile aux deux exigences montantes du régulateur (décisions explicables) et des assujettis (données qui ne partent pas chez un tiers).

## 8. Champs inexplorés & game-changers
- **La couche de signaux auditable** `[confiance]` `[souveraineté]` : chaque signal remonte à sa **source officielle + date + niveau de confiance du rapprochement** → une **piste d'audit défendable** native. Là où les concurrents vendent un score inexplicable, Atlas montre *pourquoi* un signal s'est déclenché, à partir d'une source officielle.
- **La surveillance continue souveraine** `[souveraineté]` `[temps]` : re-screener un portefeuille de tiers (watchlists) contre les sanctions officielles + BODACC, **en continu, sur l'infra du client**, la liste de tiers ne sortant jamais. Le re-screening est un besoin AML cœur ; la souveraineté lève le frein « je n'envoie pas mes clients dans un cloud US ».
- **L'assistant de triage agentique** `[agentique]` : un agent rassemble, par tiers, le **dossier de signaux descriptif** (identité RNE + check sanctions + procédures + structure + presse) pour que l'agent de conformité l'**évalue** — il **ne décide jamais** à sa place.
- **L'honnêteté du périmètre comme confiance** `[confiance]` : en **ne simulant pas** d'UBO, de PEP ou de score, Atlas donne des **faits vérifiés** que le compliance officer assume — là où un score fournisseur crée son **propre** risque de conformité (faux positifs inexplicables).
- **(Frontière gardée) L'accès UBO en BYO-droit** `[souveraineté]` : l'exclusion des bénéficiaires effectifs vaut pour la **rediffusion publique** (Sovim + régime restreint DDADUE 2025). Mais une **entité assujettie** dispose de son **propre droit d'accès légal** au registre (AMLD6). Un modèle **BYO-accès** — calqué sur le BYO-credentials INPI de l'ADR-003 — où *l'utilisateur assujetti* accède à l'UBO **via son propre droit** (Atlas outille, ne rediffuse pas) est un champ **conceptuellement distinct** de l'exclusion. ⚠️ **À n'explorer que sous revue juridique + DPIA + ADR dédié.** Reste **exclu par défaut** ; noté ici comme territoire à étudier, **pas** comme engagement.

---

## Frontières (non négociables)
- **Pas de bénéficiaires effectifs en rediffusion publique** (Sovim, DDADUE 2025, doc 03 §1.4 / doc 04). La nuance « BYO-accès » ci-dessus est un sujet séparé et gardé.
- **Pas de PEP ni d'adverse-media propriétaire** (données licenciées ; OpenSanctions = licence commerciale).
- **Pas de score / verdict / label « à risque »** (ADR-012). Atlas n'est pas un moteur de décision AML.
- **Pas de certification de conformité** : Atlas est une **couche d'entrée**, l'entité assujettie reste responsable.
- **Matching conservateur** : un faux positif sanctions est grave → « correspondance potentielle à vérifier ».
- **RGPD** : l'utilisateur compliance est responsable de traitement de son screening ; Atlas fournit la couche de données. **DPIA probable** côté usage.

## Implications produit
Ce persona est la **raison d'être de F-055**, et il s'appuie sur F-053 (portefeuille), F-047 (timeline), F-034 (structure descriptive), le tout sous **ADR-012**. Graine de feature concrète : un mode **re-screening continu** — alerter quand un tiers suivi correspond à une **nouvelle mise à jour** d'une liste de sanctions (un « F-019 des sanctions »). Et un **flag** : si la frontière BYO-UBO était un jour explorée, elle exigerait son **propre ADR**.

---

*Persona figé le 29 mai 2026. Le plus sensible : positionnement en couche de signaux **souveraine, descriptive, auditable** — jamais un moteur de verdict AML. Frontières explicites. Prochain persona : au choix.*
