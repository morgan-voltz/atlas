# F-066 — Export auditable d'un dossier

> **Statut** : 📋 Spécifiée — non implémentée (cluster Confiance, 31 mai 2026). Volet **B** de la couche de confiance ; le **livrable**. Cadrée par **ADR-021**. Distincte de l'export RGPD (F-012) et de l'export de résultats (F-021).

**Description** : générer l'export d'un **dossier d'entité** portant la **traçabilité complète par fait** (provenance + `AsOf` + référence source exacte) **+ un manifeste**, **scellé** par une empreinte (hash) et un horodatage. C'est une **trace vérifiable** de l'état consulté à une date — **pas** un acte de certification ni un avis juridique.

**Valeur user** : le **livrable** que réclament les personas **KYC/compliance** et **avocat** — dossier de due diligence traçable, état daté, **inaltéré depuis l'export et auto-vérifiable**. Différenciation **souveraine** : l'export se génère sur l'infra Atlas (y compris auto-hébergée), la donnée sensible **ne transite pas par un tiers** — là où l'équivalent fermé est cloud.

**Complexité** : ★★★ (canonicalisation pour un hash stable + génération + wording juridique soigné).

**APIs externes** : — (option : autorité d'**horodatage RFC 3161** si l'horodatage qualifié est activé — évolution).

**Dépendances** : F-056 (dossier 360 — la matière), F-022 (rapport PDF — base de rendu à étendre), **ADR-021**, ADR-015. **Distincte** de F-012 (RGPD) et F-021 (export résultats).

**Détails techniques** :
- **Source** = `CompanyDossier` (read-model F-056 / ADR-015) : on sérialise sections + faits avec `Provenance`, `AsOf`, `SectionState` et la **référence source exacte** par fait.
- **Manifeste de traçabilité** : sujet (SIREN), date de génération, version produit, liste des faits `{ valeur, source, référence, AsOf }`, états de section.
- **Intégrité (ADR-021 §1)** : **hash SHA-256** du contenu **canonicalisé** (ordre déterministe + normalisation → un même dossier produit le même hash) + **horodatage**. **Auto-vérifiable** : re-hasher le bundle et comparer, sans Atlas. Évolution : **horodatage qualifié RFC 3161**.
- **États honnêtes inclus (ADR-021 §4)** : `Stale` / `Unavailable` / `Restricted` / `NotApplicable` figurent **dans** l'export, jamais aplatis — un audit montre aussi ce qu'on n'a pas pu voir.
- **Posture (ADR-021 §2)** : mention explicite portée par l'export — « **trace vérifiable** des données telles que consultées le {date} ; ni acte de certification, ni avis juridique ».
- **Format** : **PDF lisible** (étend F-022) **+ bundle structuré (JSON)** pour la vérification machine ; le hash couvre le bundle.
- **Souverain (ADR-021 §6)** : génération **et** scellement **côté serveur Atlas**, sans cloud tiers obligatoire.
- **Doctrine** : descriptif ; ne certifie **ni** la véracité de la source amont **ni** la recevabilité juridique (ADR-012).
- **Hors-scope** : **signature électronique qualifiée** d'une personne (eIDAS niveau qualifié) — au-delà du périmètre ; **horodatage qualifié RFC 3161** = évolution, pas v1.
