# F-012 — Conformité RGPD MVP

> **Statut** : 🟡 Backend implémenté (MVP 1, 27 mai 2026). **Export** (art. 20) : `GET /account/export` → JSON structuré sans données sensibles (pas de hash, ni credentials/secret INPI). **Effacement** (art. 17) : `DELETE /account` → suppression de l'utilisateur avec **cascade** EF (FK `ON DELETE CASCADE`) sur compte, refresh tokens, credentials INPI, codes 2FA, historique — validé sur Postgres réel. **Reste à faire (UI/contenu)** : politique de confidentialité, page CGU, bannière cookies (côté MAUI/site, F-009/F-011).

**Description** : politique de confidentialité, page CGU, mécanismes d'export et de suppression du compte utilisateur, bannière cookies.

**Valeur user** : obligation légale, mais aussi gage de sérieux pour les early adopters.

**Complexité** : ★★★

**APIs externes** : aucune.

**Dépendances** : F-001.

**Détails techniques** : voir doc dédiée sécurité/RGPD pour les détails.

---

## MVP 2 — Should have (mois 4–8)

**Objectif** : étoffer le produit avec les fonctions qui en font un vrai outil utilisable au quotidien. Le repo est public, on cherche les premiers feedbacks utilisateurs.

> **Le MVP 2 intègre également le Cluster Veille (F-041 à F-050)**, qui est documenté dans une section dédiée plus bas. Le cluster veille est une **feature majeure différenciante** (cf. ADR-009) et constitue la moitié du périmètre fonctionnel de ce jalon.
