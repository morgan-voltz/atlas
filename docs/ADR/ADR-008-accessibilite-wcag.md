# ADR-008 — Engagement accessibilité WCAG 2.2 AA + RGAA 4.1.2

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

L'accessibilité numérique est légalement encadrée en France et en UE depuis l'**European Accessibility Act** entré en vigueur le 28 juin 2025. Au-delà de la conformité légale, c'est une **question éthique et stratégique** : 17% de la population française vit avec au moins une forme de handicap, et l'accessibilité est trop souvent reportée à un hypothétique "plus tard" qui n'arrive jamais.

Trois niveaux étaient envisageables :
- WCAG 2.2 niveau A (minimum strict, insuffisant pour un produit pro)
- **WCAG 2.2 niveau AA** (standard pro reconnu, exigence légale française)
- WCAG 2.2 niveau AAA (excellence, rarement applicable à tout)

Trois périmètres également :
- Visuel uniquement
- Visuel + moteur + cognitif (quasi-gratuit si bien fait)
- Les 4 axes dès MVP1 (auditif inclus)

## Décision

**Niveau cible** : **WCAG 2.2 AA + RGAA 4.1.2**, avec préparation à RGAA 5 attendu fin 2026.

**Périmètre** : **les 4 axes du handicap (visuel, auditif, moteur, cognitif) dès MVP 1**.

**Discipline** : l'accessibilité est intégrée comme **critère bloquant de Definition of Done** sur chaque feature. Une PR ne peut pas être mergée si la checklist accessibilité n'est pas passée.

## Rationale

- **Conformité légale anticipée** : initialement exempté (TPE < 10 salariés et < 2M€ CA), le projet sera soumis à l'EAA dès dépassement des seuils. Mieux vaut concevoir accessible que refondre.
- **Économie à long terme** : intégrer l'accessibilité dès le MVP coûte ~15-25% de temps en plus ; la rajouter après coup coûte 5 à 10 fois plus cher.
- **Différenciation produit** : la majorité des SaaS français en 2026 ne sont pas réellement accessibles. Argument commercial vis-à-vis de clients institutionnels (cabinets, associations, ESN).
- **Qualité globale** : un design accessible est presque toujours un meilleur design.
- **Couvrir les 4 axes** : faire le visuel correctement couvre automatiquement 80% du moteur (navigation clavier) et 80% du cognitif (structure sémantique). L'auditif a peu de surface en MVP 1.

## Conséquences

- **Positives** : robustesse, conformité, différenciation, qualité produit.
- **Négatives** : ~15-25% de temps en plus par feature MVP 1. Nécessite outillage (Accessibility Insights, Color Oracle, lecteurs d'écran) et discipline.
- **À prévoir** :
  - Checklist accessibilité dans le template de PR (doc 05)
  - Outils en CI (axe-core, accessibility scanner)
  - Tests manuels périodiques (lecteurs d'écran)
  - Page "Déclaration d'accessibilité" sur le site
  - Tests utilisateurs avec personnes en situation de handicap à partir du MVP 2
- **Référentiel détaillé** : voir document `06-accessibilite.md`
