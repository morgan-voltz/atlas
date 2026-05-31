# F-026 — Recherche d'antériorité marque avec matching intelligent

**Description** : recherche d'antériorité avec matching phonétique (Soundex, Metaphone) et sémantique pour détecter les marques proches d'un terme cible, pas seulement les correspondances exactes.

**Valeur user** : la vérification d'antériorité est un acte juridique précieux que les cabinets facturent. Un outil qui automatise une partie de cette analyse a une vraie valeur.

**Complexité** : ★★★★★

**APIs externes** : INPI PI + EUIPO + OMPI.

**Dépendances** : F-006.

> **Architecture liée** : F-026 **respecte la posture** de **ADR-014** (matching conservateur unifié — produit des `MatchCandidate`, jamais de verdict de disponibilité), mais son **moteur reste totalement séparé** des 3 matchers à base de noms (F-047, F-055, F-031) : similarité phonétique / visuelle / conceptuelle + classes de Nice = mécanique entièrement à part. Les briques IA (sémantique, résumés) vivent dans `Atlas.Application.Premium` (F-050).

---

**Grappe 5 — Exposition tiers** *(F-028, F-052 — Atlas devient une plateforme : ouverte aux intégrateurs et aux agents IA)*
