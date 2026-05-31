# ADR-003 — Authentification INPI multi-tenant

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

Le SaaS doit interagir avec les API INPI au nom de ses utilisateurs. Trois stratégies étaient possibles :
- **Compte maître** : un compte INPI partagé par tout le SaaS.
- **Multi-tenant credentials** : chaque utilisateur fournit ses propres identifiants INPI.
- **Hybride** : compte maître pour fonctions limitées, credentials user pour fonctions avancées.

## Décision

**Multi-tenant credentials** : chaque utilisateur du SaaS crée son propre compte INPI et fournit ses identifiants à la plateforme, qui les stocke chiffrés.

## Rationale

- **Conformité aux CGU INPI** : les comptes INPI sont nominatifs. Faire passer N utilisateurs par un seul compte est une violation potentielle.
- **Protection juridique** : en cas de litige, chaque utilisateur est responsable de son propre accès. Le SaaS n'est qu'un opérateur technique.
- **Scalabilité naturelle** : pas de rate limit partagé, chaque user a sa propre enveloppe.
- **Pérennité** : si l'INPI durcit ses CGU, le SaaS n'est pas impacté.

## Conséquences

- **Positives** : posture juridique safe, scalabilité, alignement avec les patterns SaaS B2B modernes (Zapier, Make).
- **Négatives** : friction d'onboarding (chaque user doit créer un compte INPI). Risque de support utilisateur sur "comment je crée mon compte INPI ?".
- **À prévoir** :
  - Wizard d'onboarding très soigné pour guider l'utilisateur dans la création de son compte INPI.
  - Stockage chiffré des credentials (per-user encryption keys, idéalement via un KMS).
  - Rotation et révocation des credentials.
  - Tests de connectivité automatiques pour détecter les mots de passe expirés.

## Possible évolution future

La piste **hybride** reste ouverte. Si la pertinence émerge, on pourra ajouter un compte maître pour des fonctions "preview" (consultation de données publiques cachées en local par le SaaS) avant que l'utilisateur ne connecte son propre compte. Cette évolution est non-bloquante avec l'architecture hexagonale prévue.
