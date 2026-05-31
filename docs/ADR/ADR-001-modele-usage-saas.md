# ADR-001 — Modèle d'usage multi-utilisateur SaaS

**Statut** : ✅ Accepté
**Date** : 26 mai 2026

## Contexte

Le projet pouvait s'orienter en outil **mono-utilisateur** (application installée localement avec les identifiants INPI de l'utilisateur) ou en **multi-utilisateur SaaS** (plateforme hébergée gérant plusieurs comptes utilisateurs distincts).

## Décision

**Multi-utilisateur SaaS**, avec gestion centralisée des comptes utilisateurs, sessions, profils, abonnements (futurs) et données associées.

## Rationale

- Permet une **synchronisation multi-device** (un user qui utilise l'app sur son desktop ET son mobile MAUI doit retrouver ses données).
- Permet les **fonctions asynchrones** (alertes, veille, agents de surveillance) qui nécessitent un backend tournant 24/7.
- Permet une **base utilisateurs commune** facilitant le support, l'analyse d'usage, et plus tard la monétisation.
- Aligne avec la cible (multi-segments : cabinets, devs, etc.) où chaque user a un environnement personnalisé.

## Conséquences

- **Positives** : flexibilité d'évolution, modèle économique possible plus tard.
- **Négatives** : complexité accrue (auth, RGPD, sécurité, scalabilité). Coûts d'hébergement à anticiper.
- **À prévoir** : politique de confidentialité, conditions générales d'utilisation, registre des traitements RGPD.
