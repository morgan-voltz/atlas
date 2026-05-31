# F-069 — Index documentaire intelligent (actes)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Document intelligence, 31 mai 2026). La **fondation faible risque** ; l'extraction de faits est **F-070**. Cadrée par **ADR-023**. Utile **seule** — aucun fait asséné.

**Description** : rendre les actes (téléchargés par F-013) **trouvables et lisibles**. Classer chaque acte par type / date / objet, l'**OCR**iser pour un **plein-texte cherchable**, et permettre le **saut à la page**. Un **index**, pas un extracteur : il *localise*, il ne *qualifie pas*.

**Valeur user** : l'**avocat** et l'**expert-comptable** trouvent *le bon acte et la bonne page* en secondes, au lieu de feuilleter des PDF. Utile immédiatement, **sans risque** d'erreur d'extraction. C'est aussi le socle technique de F-070.

**Complexité** : ★★★ (OCR + indexation plein-texte + classification légère ; pas d'extraction structurée).

**APIs externes** : — (consomme les binaires de F-013 ; **OCR local**, ADR-023).

**Dépendances** : F-013 (`CompanyAttachment`, téléchargement des actes), **ADR-023**, F-056 (dossier — surface de rendu). Patron premium F-050.

**Détails techniques** :
- **OCR local** (texte natif **et** scans) → texte indexé. **Classification légère** de l'acte (type / objet) à partir des métadonnées INPI (`CompanyAttachment.Type`, `Name`) + heuristiques sur le texte.
- **Plein-texte cherchable** (full-text PostgreSQL ou index local type Lucene) **+ saut à la page** (offsets / pages conservés).
- **Souverain (ADR-023 §2)** : OCR + index **on-infra** ; aucun document chez un tiers.
- **Doctrine** : l'index **localise**, il ne *qualifie pas*. Un acte **non océrisable / illisible** = **état honnête** (`Unavailable` / `Restricted`), jamais « rien » (« indisponible ≠ vide »).
- **Périmètre / compute** : on-demand pour les entités suivies ou à l'ouverture d'un document ; **cache**.
- **Placement hexagonal** : OCR + index = Infrastructure derrière un port ; rendu = Application / dossier.
- **Accessibilité (doc 06)** : résultats de recherche et navigation lisibles au lecteur d'écran ; le statut d'un acte (lisible / illisible) jamais porté par la seule couleur.
- **Hors-scope** : l'**extraction de faits structurés** (F-070) ; les **bilans** (couverts par F-054).
