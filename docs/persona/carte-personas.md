# Carte des personas — Atlas

> **But du document** : cartographier *qui* utilise Atlas, *pour quoi faire*, et — surtout — *quels champs inexplorés* pourraient devenir un jour des game-changers pour chacun.
> **Statut** : document d'exploration vivant. **Ce ne sont pas des engagements de features** — c'est une carte de territoires possibles. On en tirera des features plus tard, au cas par cas.
> **Date** : 29 mai 2026.
> **Source du roster** : packs de veille `07-flux-rss-veille.md` + ADR-009.

---

## Comment lire ce document

Chaque persona suit le même **gabarit en 8 champs** :

1. **En une phrase** — qui c'est.
2. **Contexte & enjeux métier** — son monde, ses pressions.
3. **Jobs-to-be-done** — les « travaux » pour lesquels il embaucherait un outil.
4. **Besoins clés** — ce qu'il lui faut concrètement.
5. **Usage d'Atlas** — comment il se sert de l'existant/prévu (ancré sur les F-xxx).
6. **Données & sources qui comptent** — ce qui a de la valeur pour lui.
7. **Ce que la concurrence ne sert pas** — la frustration actuelle, le trou.
8. **Champs inexplorés & game-changers** — le cœur de l'exploration, croisé avec tes vecteurs de rupture : **souveraineté**, **agentique**, **auditabilité/confiance**, **temps**, **ouverture/plateforme**, **accessibilité**.

> Les vecteurs de rupture sont notés `[souveraineté]`, `[agentique]`, `[confiance]`, `[temps]`, `[ouverture]`, `[accessibilité]` pour qu'on voie d'un coup d'œil quels leviers chaque idée actionne.

---

## Roster des personas

| # | Persona | Pack de veille associé | Statut dans la carte |
|---|---|---|---|
| 1 | **Expert-comptable** | ✅ | ✅ Déroulé (`persona-expert-comptable.md`) |
| 2 | **Cabinet de Propriété Industrielle** | ✅ | ✅ Déroulé (`persona-cabinet-pi.md`) |
| 3 | **Compliance / KYC / Anti-fraude** | ✅ | ✅ Déroulé (`persona-compliance-kyc.md`) |
| 4 | **Investisseur / M&A** | ✅ | ✅ Déroulé (`persona-investisseur-ma.md`) |
| 5 | **Veille concurrentielle B2B** | ✅ | ✅ Déroulé (`persona-veille-concurrentielle.md`) |
| 6 | **Développeur / Tech curieux** | ✅ | ✅ Déroulé (`persona-developpeur-tech.md`) |
| 7 | **Avocat / juriste d'affaires** | (pack à créer) | ✅ Confirmé — déroulé (`persona-avocat-juriste.md`) |

---

## 1. Expert-comptable ✅
**Déroulé** dans **`persona-expert-comptable.md`**. Suit un portefeuille de sociétés clientes (vie légale, financière, échéances). Moteur de F-053/F-019/F-054/F-048/F-047. Game-changer : le **« copilote de saison des comptes »** (agentique + souverain). **Complémentaire** des logiciels de production (Pennylane, Cegid…), pas concurrent.

## 2. Cabinet de Propriété Industrielle ✅
**Déroulé** dans **`persona-cabinet-pi.md`**. Persona-cœur (Atlas est né autour de l'INPI PI). Colonne vertébrale de F-025/F-026/F-027/F-039/F-040 ; l'exploration ouvre le vecteur **« docketing souverain »**.

## 3. Compliance / KYC / Anti-fraude ✅
**Déroulé** dans **`persona-compliance-kyc.md`**. Le plus sensible. Positionnement : couche de signaux **souveraine, descriptive, auditable** (raison d'être de F-055), **jamais** un moteur de verdict AML. Frontières dures : pas d'UBO public, pas de PEP/adverse-media propriétaire, pas de score. Soulève une frontière gardée (BYO-accès UBO pour assujettis → ADR dédié si jamais exploré).

## 4. Investisseur / M&A ✅
**Déroulé** dans **`persona-investisseur-ma.md`**. Persona-convergence (croise F-051/F-054/F-032/F-034/F-053/F-055). Game-changer : la **vue 360 / dossier cible** — méta-feature d'assemblage orchestrant ces features en une vue descriptive, sourcée, temporelle. Descriptif, jamais de verdict de valorisation.

## 5. Veille concurrentielle B2B ✅
**Déroulé** dans **`persona-veille-concurrentielle.md`**. Persona-cœur du cluster veille — c'est pour lui qu'est conçue la **timeline mixte (F-047)**, le cœur différenciant (ADR-009). Game-changer = timeline concurrentielle **cross-source** (dépôts PI + marchés DECP + événements légaux + presse, croisés). Frontière notable : **pas de scraping** des sites/prix concurrents (gap de couverture assumé face à Crayon).

## 6. Développeur / Tech curieux ✅
**Déroulé** dans **`persona-developpeur-tech.md`**. Persona-plateforme : il **construit sur** Atlas (API, MCP, webhooks, adapters), l'auto-héberge et l'étend. Game-changer = **Atlas comme substrat agentique souverain** (« ton agent, ton infra, ta donnée »). Moteur du modèle **open-core (ADR-006)** : haut d'entonnoir + base de contributeurs.

## 7. Avocat / juriste d'affaires ✅
**Confirmé comme persona à part entière** (≠ Cabinet PI). Lentille verrouillée : *risque juridique + actes/statuts + jurisprudence + validité transactionnelle*. Déroulé en profondeur dans **`persona-avocat-juriste.md`**. Soulève un réexamen de **F-031 (Judilibre)**, à traiter sous ADR-012.

---

---

## Pistes transverses & graines de features (émergées de la cartographie)

> Ces idées ne sont **pas** des engagements — ce sont les territoires que l'exploration a fait remonter, persona après persona.

- **Vue 360 / dossier cible** *(investisseur)* — **candidate n°1** : méta-feature d'**assemblage** orchestrant F-051 + F-054 + F-032 + F-034 + F-055 + F-018 en une vue descriptive, sourcée, temporelle. Aussi le cas d'usage roi de l'agentique/MCP.
- **Re-screening continu / « F-019 des sanctions »** *(compliance)* : alerter quand un tiers suivi correspond à une nouvelle mise à jour d'une liste de sanctions.
- **Docketing souverain** *(cabinet PI)* : gestion d'échéances PI, open-source et reliée au registre vivant (avec garde-fou de responsabilité).
- **Réexamen de F-031 (Judilibre)** *(avocat)* : corpus enrichi + matching d'entité faisable pour les personnes morales → à traiter sous ADR-012.
- **Signaux concurrentiels de premier rang + digest hebdo** *(veille concurrentielle)* : traiter dépôt PI / marché DECP / événement légal comme des signaux, plus une synthèse hebdomadaire (premium F-050).
- **Substrat agentique souverain / dimension plateforme** *(développeur)* : « ton agent, ton infra, ta donnée » ; moteur de l'open-core.
- **Frontière gardée — BYO-accès UBO** *(compliance)* : pour un utilisateur assujetti via son propre droit ; **exclu par défaut**, exigerait un ADR dédié.

**Fil rouge de toute la carte** : *descriptif, jamais de verdict.* Suivi ≠ conseil · ratios ≠ notation · signal ≠ score · graphe ≠ accusation · candidats ≠ disponibilité · faits ≠ avis juridique · événements ≠ menace.

**Vecteurs les plus structurants** : **souveraineté** et **agentique** reviennent partout ; leur convergence (le *substrat agentique souverain*) est l'avantage qu'aucun concurrent fermé ne peut copier.

---

*Cartographie **complète** le 29 mai 2026 — 7 personas (6 + avocat), **tous en fichiers dédiés** (`persona-*.md`). **Organisation** : cet index garde le roster, le gabarit, les résumés de chaque persona et la synthèse transverse ; le détail vit dans chaque fichier.*
