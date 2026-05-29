# Catalogue des APIs publiques branchables

> Inventaire **exhaustif** des APIs publiques (françaises, européennes et internationales) qui peuvent être intégrées au projet en tant qu'**adapters sortants** dans l'architecture hexagonale.
> Pour chaque API : périmètre, authentification, format, rate limits, pertinence métier et complexité d'intégration.

> **Note stratégique importante (mise à jour mai 2026)** : suite à l'ADR-009, la **feature veille** devient une fonctionnalité différenciante majeure du produit en MVP 2. Plusieurs APIs de ce catalogue (BODACC, JORF, INPI Actualités, CNIL, etc.) seront également exploitées comme **sources de veille pour les utilisateurs finaux**, pas seulement comme sources de données brutes. La distinction entre "API pour requêtes ponctuelles" et "flux de veille en temps continu" est de plus en plus floue. Voir doc `07-flux-rss-veille.md` pour le catalogue de flux RSS associé.

**Version** : 1.1
**Date de dernière mise à jour** : 26 mai 2026

---

## Sommaire

- [Comment lire ce catalogue](#comment-lire-ce-catalogue)
- [1. APIs INPI (cœur du projet)](#1-apis-inpi-cœur-du-projet)
- [2. APIs entreprises françaises (hors INPI)](#2-apis-entreprises-françaises-hors-inpi)
- [3. APIs commande publique](#3-apis-commande-publique)
- [4. APIs justice & droit](#4-apis-justice--droit)
- [5. APIs propriété industrielle internationale](#5-apis-propriété-industrielle-internationale)
- [6. APIs géographiques](#6-apis-géographiques)
- [7. APIs immobilier & foncier](#7-apis-immobilier--foncier)
- [8. APIs environnement & énergie](#8-apis-environnement--énergie)
- [9. APIs consommation](#9-apis-consommation)
- [10. APIs économie & finance](#10-apis-économie--finance)
- [11. APIs emploi & formation](#11-apis-emploi--formation)
- [12. APIs santé](#12-apis-santé)
- [13. APIs international / cross-border](#13-apis-international--cross-border)
- [14. APIs méta & infrastructure](#14-apis-méta--infrastructure)
- [Récap synthétique par priorité](#récap-synthétique-par-priorité)

---

## Comment lire ce catalogue

Chaque API est documentée selon un template uniforme :

- **Nom officiel** et abréviation
- **URL de base** et **documentation officielle**
- **Responsable** (qui produit la donnée)
- **Périmètre** : ce que contient l'API
- **Authentification** : ouverte / clé / OAuth / nominative
- **Format** : JSON / XML / CSV / autre
- **Rate limits** : si publiés
- **Coût** : gratuit / freemium / payant
- **Pertinence projet** : ⭐ (très utile) à ⚪ (marginale)
- **Complexité d'intégration** : ★ (trivial) à ★★★★★ (très complexe)
- **Cas d'usage couplé à l'INPI**
- **Notes spécifiques**

---

## 1. APIs INPI (cœur du projet)

### 1.1 API RNE — Registre National des Entreprises

| | |
|---|---|
| URL de base | `https://registre-national-entreprises.inpi.fr/api` |
| Documentation | https://www.inpi.fr/ressources/formalites-dentreprises/acces-lapi-formalite-rne |
| Responsable | INPI |
| Périmètre | Données de toutes les entreprises françaises (sociétés, EI, libéraux, agricoles, artisans). Identité, adresses, dirigeants, NAF, observations, actes, bilans. |
| Auth | JWT Bearer obtenu via `POST /sso/login` |
| Format | JSON (données) / PDF (documents) |
| Rate limits | Non publiés. Empiriquement <10 req/s par compte. |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Fonction centrale du produit |
| Notes | Voir doc dédiée pour les routes détaillées |

### 1.2 API PI — Propriété Industrielle

| | |
|---|---|
| URL de base | `https://api-gateway.inpi.fr` |
| Documentation | https://api-gateway.inpi.fr/docs |
| Responsable | INPI |
| Périmètre | Brevets (FR, EP, WO, CCP), marques (FR, EU, WO), dessins & modèles (FR, WO) |
| Auth | XSRF-Token + access_token + refresh_token (cookies) |
| Format | JSON ou XML selon endpoint |
| Rate limits | Non publiés |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Complexité | ★★★★ (auth complexe) |
| Cas d'usage | Recherche marques, brevets, D&M français |
| Notes | Auth la plus tordue de l'écosystème INPI, à isoler dans un adapter solide dès le début |

### 1.3 API Guichet Unique

| | |
|---|---|
| URL de base | `https://guichet-unique.inpi.fr` |
| Documentation | https://www.inpi.fr/ressources/formalites-dentreprises/acces-aux-api-guichet-unique |
| Responsable | INPI |
| Périmètre | Dépôt de formalités d'entreprise (création, modification, cessation), comptes annuels |
| Auth | OAuth2 |
| Format | JSON |
| Rate limits | Non publiés |
| Coût | Gratuit |
| Pertinence | ⭐⭐ (segment niche : mandataires) |
| Complexité | ★★★★ |
| Cas d'usage | Permettre à l'utilisateur de déposer/modifier une formalité depuis l'app |
| Notes | Réservé à un usage de mandataire validé. Hors périmètre MVP. |

### 1.4 API Bénéficiaires Effectifs (BE)

| | |
|---|---|
| URL de base | INPI (accès restreint) |
| Documentation | https://www.inpi.fr/ |
| Responsable | INPI |
| Périmètre | Identité des bénéficiaires effectifs des sociétés (lutte anti-blanchiment) |
| Auth | Compte nominatif + démonstration d'intérêt légitime |
| Format | JSON / PDF |
| Coût | Gratuit |
| Pertinence | ⭐ (sujet sensible) |
| Complexité | ★★★★★ (juridique) |
| Cas d'usage | Compliance, KYC, AML |
| Notes | **Restriction d'accès depuis arrêt CJUE Sovim (nov. 2022).** Pas accessible librement. À éviter en MVP. |

---

## 2. APIs entreprises françaises (hors INPI)

### 2.1 API Sirene — INSEE

| | |
|---|---|
| URL de base | `https://api.insee.fr/entreprises/sirene/V3.11/` |
| Documentation | https://api.insee.fr/catalogue/ |
| Responsable | INSEE |
| Périmètre | Répertoire Sirene : toutes les unités légales et établissements français. Source officielle pour les SIREN/SIRET. |
| Auth | OAuth2 (clé d'API obtenue après inscription) |
| Format | JSON ou CSV |
| Rate limits | 30 req/min, 500 req/heure (peut être augmenté sur demande) |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Complète le RNE INPI sur les **établissements** (le RNE est plus orienté unité légale). Recherche en temps réel des nouveaux SIRET. |
| Notes | INSEE est plus rigoureux sur les rate limits que l'INPI. À documenter et respecter. |

### 2.2 API Recherche d'Entreprises — data.gouv.fr

| | |
|---|---|
| URL de base | `https://recherche-entreprises.api.gouv.fr` |
| Documentation | https://annuaire-entreprises.data.gouv.fr/donnees/api-entreprises |
| Responsable | DINUM (Direction Interministérielle du Numérique) |
| Périmètre | API d'agrégation : combine RNE + Sirene + RNA en une seule interface |
| Auth | Aucune (ouverte) |
| Format | JSON |
| Rate limits | 7 req/sec, modéré |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ |
| Complexité | ★ |
| Cas d'usage | **Alternative simple** pour la recherche par dénomination quand on ne veut pas appeler INPI/INSEE séparément. Bon pour les recherches floues / textuelles. |
| Notes | Idéal pour démarrer. Donnée moins complète que les sources directes mais largement suffisante pour l'autocomplete et la recherche. |

### 2.3 BODACC — Bulletin Officiel des Annonces Civiles et Commerciales

| | |
|---|---|
| URL de base | `https://bodacc-datadila.opendatasoft.com/api/explore/v2.1/catalog/datasets/` |
| Documentation | https://bodacc-datadila.opendatasoft.com/api/v2/console |
| Responsable | DILA (Direction de l'Information Légale et Administrative) |
| Périmètre | Annonces légales : créations d'entreprise, modifications, procédures collectives (RJ, LJ, sauvegarde), ventes de fonds, dissolutions, comptes annuels déposés. **Temps quasi-réel.** |
| Auth | Aucune (ouverte) |
| Format | JSON, CSV, RDF, Excel |
| Rate limits | Modéré (Opendatasoft standard) |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | **Veille temps réel impossible sans ça.** Alerte sur RJ d'un client, détection d'opportunités, suivi du marché. |
| Notes | Source incontournable. À intégrer dès le MVP 2. |

### 2.4 API Répertoire National des Associations (RNA)

| | |
|---|---|
| URL de base | Via API Recherche d'Entreprises ou data.gouv.fr |
| Documentation | https://www.data.gouv.fr/fr/datasets/repertoire-national-des-associations/ |
| Responsable | Ministère de l'Intérieur |
| Périmètre | ~1,5 million d'associations loi 1901 françaises |
| Auth | Aucune (data.gouv.fr) |
| Format | JSON / CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐ (extension naturelle du périmètre entreprise) |
| Complexité | ★★ |
| Cas d'usage | Étendre la couverture au-delà des seules entreprises (KYC pour associations subventionnées, suivi du tissu associatif). |

### 2.5 Journal Officiel — Associations

| | |
|---|---|
| URL de base | `https://www.journal-officiel.gouv.fr/associations/recherche/` |
| Documentation | https://www.journal-officiel.gouv.fr/pages/donnees-en-ligne/ |
| Responsable | DILA |
| Périmètre | Annonces de création, modification, dissolution d'associations publiées au JO |
| Auth | Aucune |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Complément du RNA pour suivre les évolutions associatives en temps réel. |

---

## 3. APIs commande publique

### 3.1 DECP — Données Essentielles de la Commande Publique

| | |
|---|---|
| URL de base | `https://www.data.gouv.fr/fr/datasets/donnees-essentielles-de-la-commande-publique/` |
| Documentation | https://decpinfo.data.gouv.fr/ |
| Responsable | DAJ (Direction des Affaires Juridiques de Bercy) + DINUM |
| Périmètre | Tous les marchés publics français de plus de 40 000€ HT : attributaires (SIRET), montants, durée, objet, acheteur. |
| Auth | Aucune (data.gouv.fr) |
| Format | JSON, XML, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Scoring "puissance publique" d'une entreprise : qui gagne des marchés, dans quel secteur, pour combien. Très utile pour BTP, IT, conseil. |
| Notes | Qualité variable selon les acheteurs. Certains champs souvent vides. Nécessite parfois un travail de nettoyage. |

### 3.2 BOAMP — Bulletin Officiel des Annonces des Marchés Publics

| | |
|---|---|
| URL de base | `https://www.boamp.fr/avis/recherche/` |
| Documentation | https://www.data.gouv.fr/fr/datasets/boamp/ |
| Responsable | DILA |
| Périmètre | Avis de marchés publics publiés (avant attribution) : appels d'offres, MAPA, etc. |
| Auth | Aucune |
| Format | JSON, XML, CSV (dépend du dataset) |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Veille business pour PME cherchant des marchés à candidater. |

### 3.3 TED — Tenders Electronic Daily (UE)

| | |
|---|---|
| URL de base | `https://ted.europa.eu/api/v3.0/notices/search` |
| Documentation | https://ted.europa.eu/en/release-notes/ted-api |
| Responsable | Office des publications de l'UE |
| Périmètre | Marchés publics européens (au-dessus des seuils européens) |
| Auth | Aucune |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★★★ |
| Cas d'usage | Couverture européenne pour entreprises cherchant des marchés transfrontaliers. |

---

## 4. APIs justice & droit

### 4.1 Judilibre — Décisions de justice

| | |
|---|---|
| URL de base | `https://api.piste.gouv.fr/cassation/judilibre/v1.0/` |
| Documentation | https://piste.gouv.fr/ |
| Responsable | Cour de cassation |
| Périmètre | Décisions de la Cour de cassation, des cours d'appel, et progressivement des tribunaux judiciaires. Anonymisées. |
| Auth | OAuth2 sur PISTE |
| Format | JSON |
| Rate limits | Définis par PISTE |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ (segment juristes / cabinets PI) |
| Complexité | ★★★★ |
| Cas d'usage | Suivi des contentieux marques (oppositions, contrefaçon), brevets, sociétés. Construction de jurisprudence sectorielle. |
| Notes | PISTE est le portail d'API du ministère de la Justice, avec auth OAuth2. Demande une inscription. |

### 4.2 Légifrance API

| | |
|---|---|
| URL de base | `https://api.piste.gouv.fr/dila/legifrance/lf-engine-app/` |
| Documentation | https://piste.gouv.fr/ |
| Responsable | DILA |
| Périmètre | Textes législatifs et réglementaires français (Codes, lois, décrets, arrêtés) |
| Auth | OAuth2 sur PISTE |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★★★★ |
| Cas d'usage | Automatisation de la vérification de conformité, citation de textes dans des rapports générés. |

### 4.3 JADE — Décisions du Conseil d'État

| | |
|---|---|
| URL de base | Via PISTE / data.gouv.fr |
| Documentation | https://www.data.gouv.fr/fr/datasets/jurisprudence-administrative-jade/ |
| Responsable | Conseil d'État |
| Périmètre | Décisions du Conseil d'État et des juridictions administratives |
| Auth | Selon canal (PISTE OAuth2 ou data.gouv.fr ouvert) |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★★★ |
| Cas d'usage | Contentieux administratifs des entreprises (marchés publics annulés, contentieux fiscal). |

### 4.4 Casier judiciaire — non disponible

> **À noter** : le casier judiciaire des personnes morales (B3) n'est **pas disponible en open data**. Accès uniquement par démarche officielle.

### 4.5 DG Trésor — Registre national des gels des avoirs

| | |
|---|---|
| URL de base | https://gels-avoirs.dgtresor.gouv.fr/ (API & fichiers) |
| Documentation | https://www.economie.gouv.fr/dgtresor/sanctions-financieres |
| Responsable | Direction générale du Trésor (DG Trésor) |
| Périmètre | Liste officielle des personnes physiques et entités frappées par une mesure de **gel des avoirs** (sanctions ONU + UE + nationales transposées en droit français). |
| Auth | Aucune |
| Format | Fichiers interopérables (CSV, XML, JSON) + API |
| Rate limits | Souples (mise à jour quotidienne du registre) |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ (segment compliance / KYC, **F-055**) |
| Complexité | ★★★ (matching nom + identifiants — exigence d'exactitude, ADR-012) |
| Cas d'usage | Screening sanctions souverain et officiel pour **F-055** (signaux de risque descriptif). Matching **conservateur** uniquement (« correspondance potentielle à vérifier » — jamais d'affirmation automatique : une fausse correspondance sanctions est diffamatoire). |
| Notes | Source officielle de référence en France. Alternative gratuite et souveraine à OpenSanctions (qui est gratuit en non-commercial seulement). |

### 4.6 Liste consolidée des sanctions financières de l'UE

| | |
|---|---|
| URL de base | https://webgate.ec.europa.eu/fsd/fsf (fichier consolidé) |
| Documentation | https://finance.ec.europa.eu/eu-and-world/sanctions-restrictive-measures_en |
| Responsable | Commission européenne (FISMA) |
| Périmètre | Liste consolidée des **sanctions financières** adoptées par l'UE (gels d'avoirs, interdictions de mise à disposition de fonds). Couvre les régimes ONU transposés au niveau UE et les régimes autonomes de l'UE. |
| Auth | Aucune (compte EU Login optionnel pour notifications) |
| Format | XML, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ (segment compliance / KYC, **F-055**) |
| Complexité | ★★★ |
| Cas d'usage | Complément européen à la liste DG Trésor pour **F-055**. Mêmes règles de matching conservateur. |
| Notes | À croiser avec la DG Trésor (qui transpose en droit français mais peut décaler de quelques jours). |

---

## 5. APIs propriété industrielle internationale

### 5.1 EUIPO — Office de l'UE pour la PI

| | |
|---|---|
| URL de base | `https://euipo.europa.eu/copla/` |
| Documentation | https://euipo.europa.eu/ohimportal/en/web/guest/online-services |
| Responsable | EUIPO |
| Périmètre | Marques européennes (EUTM) et dessins & modèles communautaires (RCD) |
| Auth | Inscription + clé d'API |
| Format | JSON, XML |
| Rate limits | Définis dans les ToS |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ (pour vraie veille marques) |
| Complexité | ★★★★ |
| Cas d'usage | Couverture européenne en complément de l'INPI PI. Indispensable pour vérification d'antériorité sérieuse. |

### 5.2 OMPI / WIPO — Global Brand Database & PATENTSCOPE

| | |
|---|---|
| URL de base | `https://www.wipo.int/branddb/` (marques) et `https://patentscope.wipo.int/` (brevets) |
| Documentation | https://patentscope.wipo.int/search/en/help/data.jsf |
| Responsable | OMPI (Organisation Mondiale de la PI) |
| Périmètre | Marques et brevets internationaux (système de Madrid pour les marques, PCT pour les brevets) |
| Auth | Inscription requise pour API ; recherche web ouverte |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ |
| Complexité | ★★★★ |
| Cas d'usage | Couverture mondiale marques et brevets. |

### 5.3 OEB / EPO — Open Patent Services (OPS)

| | |
|---|---|
| URL de base | `https://ops.epo.org/3.2/` |
| Documentation | https://www.epo.org/searching-for-patents/data/web-services/ops.html |
| Responsable | Office Européen des Brevets |
| Périmètre | Brevets européens et données associées (familles de brevets, citations, statut légal pays par pays) |
| Auth | OAuth2 + clé d'API |
| Format | JSON, XML |
| Rate limits | Quota mensuel selon le plan (gratuit jusqu'à 4 Go/semaine) |
| Coût | Gratuit (fair use) |
| Pertinence | ⭐⭐⭐⭐ |
| Complexité | ★★★★ |
| Cas d'usage | Données enrichies sur brevets, familles internationales, citations, indispensable pour veille technologique pro. |

### 5.4 USPTO — US Patent and Trademark Office

| | |
|---|---|
| URL de base | `https://developer.uspto.gov/api-catalog` |
| Documentation | https://developer.uspto.gov/ |
| Responsable | USPTO |
| Périmètre | Brevets et marques américains |
| Auth | Clé d'API |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⭐⭐ (couverture US si projet international) |
| Complexité | ★★★ |
| Cas d'usage | Veille internationale, antériorité US. |

---

## 6. APIs géographiques

### 6.1 BAN — Base Adresse Nationale

| | |
|---|---|
| URL de base | `https://api-adresse.data.gouv.fr/` |
| Documentation | https://adresse.data.gouv.fr/api-doc/adresse |
| Responsable | DINUM / IGN |
| Périmètre | Toutes les adresses françaises (~26 millions). Géocodage et géocodage inverse. |
| Auth | Aucune |
| Format | JSON, CSV |
| Rate limits | 50 req/s recommandé |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐⭐ |
| Complexité | ★ |
| Cas d'usage | Normalisation des adresses des entreprises, géocodage pour cartographie, recherche par proximité. |

### 6.2 IGN Géoplateforme

| | |
|---|---|
| URL de base | `https://data.geopf.fr/` |
| Documentation | https://geoservices.ign.fr/services-web |
| Responsable | IGN (Institut Géographique National) |
| Périmètre | Cartographie, fonds de plan, données topographiques |
| Auth | Clé d'API (gratuite) |
| Format | WMS, WFS, WMTS, vectoriel |
| Coût | Gratuit (depuis 2021) |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Fonds de carte pour la cartographie des entreprises dans l'app. |

### 6.3 OpenStreetMap (Nominatim, Overpass)

| | |
|---|---|
| URL de base | `https://nominatim.openstreetmap.org/` et `https://overpass-api.de/` |
| Documentation | https://nominatim.org/release-docs/develop/api/Overview/ |
| Responsable | OSM Foundation |
| Périmètre | Cartographie collaborative mondiale |
| Auth | Aucune (usage modéré recommandé) |
| Format | JSON, XML |
| Rate limits | 1 req/s sur le service public, illimité en self-hosted |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Alternative à IGN pour cartographie, géocodage de fallback. |

---

## 7. APIs immobilier & foncier

### 7.1 DVF — Demandes de Valeurs Foncières

| | |
|---|---|
| URL de base | `https://api.cquest.org/dvf` ou `https://app.dvf.etalab.gouv.fr/` |
| Documentation | https://www.data.gouv.fr/fr/datasets/demandes-de-valeurs-foncieres/ |
| Responsable | DGFIP (publié via Etalab) |
| Périmètre | Toutes les transactions immobilières françaises des 5 dernières années, avec prix, surface, type de bien, adresse. |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Valoriser le patrimoine immobilier d'une entreprise, scoring immobilier d'un dirigeant. |

### 7.2 Cadastre

| | |
|---|---|
| URL de base | `https://apicarto.ign.fr/api/cadastre/` |
| Documentation | https://apicarto.ign.fr/api/doc/cadastre |
| Responsable | IGN + DGFIP |
| Périmètre | Parcelles cadastrales, propriétés foncières (les propriétaires nominatifs ne sont pas en open data) |
| Auth | Aucune |
| Format | GeoJSON |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Identification des parcelles, surfaces, contexte foncier d'une activité. |

### 7.3 Géorisques

| | |
|---|---|
| URL de base | `https://www.georisques.gouv.fr/api/v1/` |
| Documentation | https://www.georisques.gouv.fr/articles-risques/api-georisques |
| Responsable | BRGM + Ministère Transition Écologique |
| Périmètre | Risques naturels (inondation, séisme), technologiques (sites SEVESO, ICPE), pollution des sols. |
| Auth | Aucune |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Due diligence immobilière, scoring de risque environnemental d'un site d'entreprise. |

### 7.4 DPE — Diagnostic de Performance Énergétique

| | |
|---|---|
| URL de base | `https://data.ademe.fr/datasets` |
| Documentation | https://data.ademe.fr/ |
| Responsable | ADEME |
| Périmètre | Tous les DPE réalisés en France (millions de lignes) |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★ |
| Cas d'usage | Niche : valorisation immobilière, scoring ESG sur le bâti. |

---

## 8. APIs environnement & énergie

### 8.1 ADEME — Base Carbone

| | |
|---|---|
| URL de base | `https://data.ademe.fr/datasets/base-carbone(r)` |
| Documentation | https://bilans-ges.ademe.fr/ |
| Responsable | ADEME |
| Périmètre | Facteurs d'émission CO2 pour tous les secteurs, transports, produits. Référence pour les bilans GES. |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Calcul d'empreinte carbone, scoring ESG des entreprises. |

### 8.2 ADEME — Bilans GES réglementaires

| | |
|---|---|
| URL de base | `https://bilans-ges.ademe.fr/bilans` |
| Documentation | https://bilans-ges.ademe.fr/ |
| Responsable | ADEME |
| Périmètre | Bilans GES déposés par les entreprises de plus de 500 salariés (obligation légale) |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Couplé au RNE : connaître l'empreinte carbone publique d'une grande entreprise. |

### 8.3 ENEDIS Open Data

| | |
|---|---|
| URL de base | `https://data.enedis.fr/api/v2/` |
| Documentation | https://data.enedis.fr/api/v2/console |
| Responsable | Enedis |
| Périmètre | Données de consommation électrique agrégées par zone géographique |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★ |
| Cas d'usage | Tendances macro de consommation, sans utilité directe pour le périmètre INPI. |

### 8.4 RTE — Réseau de Transport d'Électricité

| | |
|---|---|
| URL de base | `https://digital.iservices.rte-france.com/` |
| Documentation | https://data.rte-france.com/ |
| Responsable | RTE |
| Périmètre | Données du réseau électrique haute tension, production, consommation |
| Auth | OAuth2 |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⚪ |
| Complexité | ★★★ |
| Cas d'usage | Marginal pour le périmètre projet. |

### 8.5 Météo-France

| | |
|---|---|
| URL de base | `https://portail-api.meteofrance.fr/` |
| Documentation | https://portail-api.meteofrance.fr/web/fr/ |
| Responsable | Météo-France |
| Périmètre | Prévisions et observations météo |
| Auth | Clé d'API |
| Format | JSON, GRIB |
| Coût | Gratuit (quotas) |
| Pertinence | ⚪ |
| Complexité | ★★ |
| Cas d'usage | Marginal. |

---

## 9. APIs consommation

### 9.1 RappelConso

| | |
|---|---|
| URL de base | `https://rappel.conso.gouv.fr/api/` |
| Documentation | https://www.data.gouv.fr/fr/datasets/rappelconso0/ |
| Responsable | DGCCRF |
| Périmètre | Tous les rappels de produits en France |
| Auth | Aucune |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★ |
| Cas d'usage | Couplé au RNE : alerte si une entreprise suivie fait l'objet d'un rappel produit. Indicateur de risque réputationnel. |

### 9.2 Open Food Facts

| | |
|---|---|
| URL de base | `https://world.openfoodfacts.org/api/v2/` |
| Documentation | https://openfoodfacts.github.io/openfoodfacts-server/api/ |
| Responsable | Communauté Open Food Facts |
| Périmètre | Base de produits alimentaires mondiale |
| Auth | Aucune |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⭐ (segment agro) |
| Complexité | ★★ |
| Cas d'usage | Niche : enrichir une fiche entreprise agroalimentaire avec ses produits référencés. |

---

## 10. APIs économie & finance

### 10.1 INSEE — Indicateurs économiques (BDM)

| | |
|---|---|
| URL de base | `https://api.insee.fr/series/BDM/V1/` |
| Documentation | https://www.insee.fr/fr/information/2868055 |
| Responsable | INSEE |
| Périmètre | Indicateurs économiques nationaux et sectoriels (IPC, IPP, chômage, comptes nationaux) |
| Auth | Clé d'API |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Contextualisation sectorielle (comparer un bilan d'entreprise à des indicateurs macro de son secteur). |

### 10.2 Banque de France — Webstat

| | |
|---|---|
| URL de base | `https://webstat.banque-france.fr/` |
| Documentation | https://webstat.banque-france.fr/fr/api/ |
| Responsable | Banque de France |
| Périmètre | Statistiques monétaires, taux d'intérêt, crédits |
| Auth | Aucune (consultations) |
| Format | JSON, CSV |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★★ |
| Cas d'usage | Contexte économique, hors périmètre direct INPI. |

### 10.3 Eurostat

| | |
|---|---|
| URL de base | `https://ec.europa.eu/eurostat/api/dissemination/` |
| Documentation | https://ec.europa.eu/eurostat/data/web-services |
| Responsable | Commission européenne |
| Périmètre | Statistiques européennes |
| Auth | Aucune |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★★ |
| Cas d'usage | Benchmark européen sectoriel. |

---

## 11. APIs emploi & formation

### 11.1 France Travail (ex Pôle Emploi)

| | |
|---|---|
| URL de base | `https://api.francetravail.io/` |
| Documentation | https://francetravail.io/data/api |
| Responsable | France Travail |
| Périmètre | Offres d'emploi en France, ROME (Référentiel Opérationnel des Métiers et Emplois) |
| Auth | OAuth2 |
| Format | JSON |
| Coût | Gratuit (quotas) |
| Pertinence | ⭐ |
| Complexité | ★★★ |
| Cas d'usage | Niche : suivi des recrutements d'une entreprise comme indicateur de croissance. |

### 11.2 Onisep — Métiers et formations

| | |
|---|---|
| URL de base | `https://api.opendata.onisep.fr/api/1.0/` |
| Documentation | https://opendata.onisep.fr/ |
| Responsable | Onisep |
| Périmètre | Métiers, formations, établissements scolaires |
| Auth | Clé d'API |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⚪ |
| Complexité | ★★ |
| Cas d'usage | Hors périmètre projet. |

---

## 12. APIs santé

### 12.1 Health Data Hub

| | |
|---|---|
| URL de base | `https://www.health-data-hub.fr/` |
| Documentation | https://documentation-snds.health-data-hub.fr/ |
| Responsable | Plateforme des données de santé |
| Périmètre | Données de santé agrégées et anonymisées (SNDS) |
| Auth | Procédure d'accès stricte |
| Format | Selon dataset |
| Coût | Gratuit (mais accès restreint) |
| Pertinence | ⚪ |
| Complexité | ★★★★★ |
| Cas d'usage | Hors périmètre projet (accès soumis à autorisation). |

### 12.2 Annuaire santé — FINESS

| | |
|---|---|
| URL de base | Via data.gouv.fr |
| Documentation | https://www.data.gouv.fr/fr/datasets/finess-extraction-du-fichier-des-etablissements/ |
| Responsable | Ministère de la Santé |
| Périmètre | Tous les établissements sanitaires et médico-sociaux français |
| Auth | Aucune |
| Format | CSV, JSON |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★ |
| Cas d'usage | Niche : pour utilisateurs du secteur santé. |

---

## 13. APIs international / cross-border

### 13.1 OpenCorporates

| | |
|---|---|
| URL de base | `https://api.opencorporates.com/` |
| Documentation | https://api.opencorporates.com/documentation |
| Responsable | OpenCorporates Ltd |
| Périmètre | Plus grande base mondiale d'entreprises (220+ millions). Données provenant de tous les registres officiels. |
| Auth | Clé d'API (freemium) |
| Format | JSON, XML |
| Coût | Free tier limité + plans payants |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Suivre une entreprise française et ses filiales/parent à l'étranger. KYC international. |

### 13.2 GLEIF — Global Legal Entity Identifier

| | |
|---|---|
| URL de base | `https://api.gleif.org/api/v1/` |
| Documentation | https://www.gleif.org/en/lei-data/gleif-api |
| Responsable | GLEIF (Global Legal Entity Identifier Foundation) |
| Périmètre | Tous les codes LEI mondiaux (identifiants internationaux d'entités juridiques) |
| Auth | Aucune |
| Format | JSON, XML |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Identification cross-border, conformité aux réglementations financières internationales. |

### 13.3 Companies House (UK)

| | |
|---|---|
| URL de base | `https://api.company-information.service.gov.uk/` |
| Documentation | https://developer.company-information.service.gov.uk/ |
| Responsable | Companies House UK |
| Périmètre | Toutes les entreprises britanniques |
| Auth | Clé d'API |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⭐⭐ |
| Complexité | ★★ |
| Cas d'usage | Couverture UK pour entreprises ayant des filiales ou clients en Grande-Bretagne. |

### 13.4 EDGAR SEC (USA)

| | |
|---|---|
| URL de base | `https://www.sec.gov/edgar/sec-api-documentation` |
| Documentation | https://www.sec.gov/os/accessing-edgar-data |
| Responsable | SEC (Securities and Exchange Commission) |
| Périmètre | Documents financiers des entreprises cotées américaines |
| Auth | User-Agent identifiant requis |
| Format | JSON, XBRL |
| Coût | Gratuit |
| Pertinence | ⭐ |
| Complexité | ★★★ |
| Cas d'usage | Recherche d'entreprises US cotées (cas marginal). |

### 13.5 Handelsregister (Allemagne) — non ouvert

> **À noter** : le registre du commerce allemand n'est **pas open data**. Accès payant via `handelsregister.de`. À garder en tête pour les utilisateurs voulant des données allemandes.

---

## 14. APIs méta & infrastructure

### 14.1 data.gouv.fr

| | |
|---|---|
| URL de base | `https://www.data.gouv.fr/api/1/` |
| Documentation | https://doc.data.gouv.fr/ |
| Responsable | DINUM |
| Périmètre | Méta-API permettant de rechercher tous les datasets publiés par l'État français |
| Auth | Aucune (lecture) |
| Format | JSON |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★ |
| Cas d'usage | Découverte automatique de nouveaux datasets, mise à jour du catalogue. |

### 14.2 api.gouv.fr (catalogue)

| | |
|---|---|
| URL de base | `https://api.gouv.fr/` |
| Documentation | https://api.gouv.fr/ |
| Responsable | DINUM |
| Périmètre | Catalogue de toutes les APIs publiques françaises |
| Auth | Aucune |
| Format | Web (pas d'API directe) |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★ |
| Cas d'usage | Outil de référence pour découvrir de nouvelles APIs intégrables au projet. |

### 14.3 PISTE — Plateforme d'Intermédiation des Services de Tiers de l'État

| | |
|---|---|
| URL de base | `https://piste.gouv.fr/` |
| Documentation | https://piste.gouv.fr/ |
| Responsable | DINUM |
| Périmètre | Portail OAuth2 unifié pour de nombreuses APIs publiques (Légifrance, Judilibre, DGFIP, etc.) |
| Auth | OAuth2 |
| Format | Selon API cible |
| Coût | Gratuit |
| Pertinence | ⭐⭐⭐ |
| Complexité | ★★★ |
| Cas d'usage | Infrastructure d'authentification pour plusieurs APIs juridiques. |

### 14.4 API Entreprise (réservée aux administrations)

| | |
|---|---|
| URL de base | `https://entreprise.api.gouv.fr/` |
| Documentation | https://entreprise.api.gouv.fr/developpeurs |
| Responsable | DINUM |
| Périmètre | Agrégateur sur-mesure pour administrations : combine INPI, INSEE, DGFIP, Pôle Emploi, etc. |
| Auth | Token, accès réservé aux administrations |
| Format | JSON |
| Coût | Gratuit (réservé) |
| Pertinence | ⚪ (inaccessible pour le projet) |
| Complexité | N/A |
| Cas d'usage | **Inaccessible** : ne sert qu'aux administrations et missions de service public. À mentionner pour les utilisateurs publics éventuels mais pas intégrable autrement. |

---

## Récap synthétique par priorité

### Niveau 1 — INDISPENSABLES (MVP 1)

| API | Pertinence | Complexité |
|---|---|---|
| INPI RNE | ⭐⭐⭐⭐⭐ | ★★ |
| INPI PI | ⭐⭐⭐⭐⭐ | ★★★★ |
| BAN (adresses) | ⭐⭐⭐⭐ | ★ |

### Niveau 2 — DIFFÉRENCIANTES (MVP 2 / V2)

| API | Pertinence | Complexité |
|---|---|---|
| BODACC | ⭐⭐⭐⭐⭐ | ★★ |
| API Sirene INSEE | ⭐⭐⭐⭐⭐ | ★★ |
| API Recherche d'Entreprises | ⭐⭐⭐⭐ | ★ |
| DECP | ⭐⭐⭐⭐ | ★★★ |
| EUIPO | ⭐⭐⭐⭐ | ★★★★ |
| OEB/EPO OPS | ⭐⭐⭐⭐ | ★★★★ |
| Judilibre | ⭐⭐⭐⭐ | ★★★★ |

### Niveau 3 — ENRICHISSEMENTS (V3+)

| API | Pertinence | Complexité |
|---|---|---|
| OMPI / WIPO | ⭐⭐⭐⭐ | ★★★★ |
| Légifrance | ⭐⭐⭐ | ★★★★ |
| DVF | ⭐⭐ | ★★ |
| ADEME Bilan GES | ⭐⭐ | ★★ |
| RappelConso | ⭐⭐ | ★ |
| OpenCorporates | ⭐⭐⭐ | ★★★ |
| GLEIF | ⭐⭐ | ★★ |
| Companies House UK | ⭐⭐ | ★★ |

### Niveau 4 — MARGINALES (uniquement si segment spécifique)

| API | Pertinence | Notes |
|---|---|---|
| TED EU | ⭐⭐ | Marchés publics européens |
| BOAMP | ⭐⭐⭐ | Avant DECP dans le cycle de vie d'un marché |
| RNA + JO Associations | ⭐⭐ | Couverture associative |
| Géorisques | ⭐⭐ | Due diligence immobilière |
| Cadastre | ⭐⭐ | Foncier |
| France Travail | ⭐ | Indicateur recrutement |
| INSEE BDM | ⭐⭐ | Benchmarks macro |
| Open Food Facts | ⭐ | Segment agro |
| EDGAR SEC | ⭐ | US uniquement |
| USPTO | ⭐⭐ | US PI |

### Niveau 5 — INACCESSIBLES ou HORS PÉRIMÈTRE

| API | Raison |
|---|---|
| Bénéficiaires Effectifs INPI | Accès restreint (CJUE Sovim) |
| API Entreprise (gouv) | Réservée aux administrations |
| Health Data Hub | Accès sur autorisation |
| Casier judiciaire | Pas en open data |
| Handelsregister DE | Pas en open data |
| URSSAF, DGFIP détaillés | Secret professionnel |

---

*Ce catalogue est volontairement exhaustif pour servir de référence sur la durée. Toutes ces APIs ne seront pas intégrées : l'objectif est d'avoir une vision claire de **ce qui est branchable** quand le besoin émerge. L'architecture hexagonale permettra l'intégration progressive sans refonte.*
