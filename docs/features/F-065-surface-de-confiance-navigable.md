# F-065 — Surface de confiance navigable

> **Statut** : 📋 Spécifiée — non implémentée (cluster Confiance, 31 mai 2026). Volet **A** de la couche de confiance ; pendant *in-app* de l'export auditable (**F-066**). Promeut un atome existant — peut se livrer **seule**. Réutilise la granularité par-fait et la posture d'**ADR-021**.

**Description** : rendre l'atome **« ligne de provenance » (`source · date`)** — déjà présent partout, mais aujourd'hui *muet* — **traversable**. Taper la provenance d'un fait ou d'une section ouvre un **panneau « d'où vient cette donnée »** : source exacte, référence d'origine (acte INPI, annonce BODACC, CELEX, dataset), horodatage de récupération, **date de dernière confirmation** distincte de la **date de dernier changement**, et l'état (`SectionState`).

**Valeur user** : transforme la doctrine « tout est sourcé et daté » en **preuve consultable d'un geste**. Rassure tous les personas, et devient **indispensable au KYC/avocat** (vérifier la source d'un fait avant de s'appuyer dessus). C'est le pendant interactif de l'export — la confiance *dans* l'app, avant la confiance *exportée*.

**Complexité** : ★★ (le substrat est déjà là — `Provenance`/`AsOf`/`SectionState` d'ADR-015 ; l'effort est surtout UI + exposition des champs).

**APIs externes** : — (lit la provenance **déjà capturée** ; aucune source nouvelle).

**Dépendances** : F-056 (dossier 360 — porteur de `Provenance`/`AsOf`/`SectionState`), ADR-015, ADR-012 ; granularité par-fait et posture d'**ADR-021**.

**Détails techniques** :
- **Promotion de l'atome** « ligne de provenance » (doc 12) d'élément d'affichage en élément **interactif** : tap → **panneau de provenance**.
- Le panneau expose, par fait ou section : `Provenance` (source + base), `AsOf`, **dernière confirmation ≠ dernier changement** (deux dates — à ajouter au modèle si absentes), la **référence source exacte** (lien vers l'acte / l'annonce / le CELEX), et le `SectionState`.
- **Étend `Provenance`/`AsOf` vers le par-fait** quand c'est utile (ADR-021 §3) ; reste par-section sinon. Aucune refonte du modèle.
- **Doctrine** : descriptif ; `Stale` / `Unavailable` / `Restricted` montrés **honnêtement** (jamais « rien à signaler ») ; jamais d'information par la **couleur seule** (icône + texte, doc 06).
- **Accessibilité (doc 06)** : le panneau est **annoncé**, le focus est géré (entrée/sortie), libellés sans jargon.
- **Gain transverse** : même atome partout (fiche, dossier 360, items de veille) → la surface de confiance s'applique d'un coup à toutes les surfaces, sans composant par cas.
- **Hors-scope** : l'export lui-même (F-066) ; la reconstitution d'un **état passé complet** (machine à remonter le temps — piste distincte) — ici on expose *dernière confirmation / dernier changement*, pas l'historique entier.
