# ADR-006 — Modèle économique

**Statut** : ✅ Accepté (phasage)
**Date** : 26 mai 2026

## Contexte

Plusieurs modèles économiques étaient envisageables : open core (libre + premium fermé), OSS + SaaS hébergé payant, pur libre + donations / support, ou aucun.

## Décision

**Phasage en 4 étapes** :

1. **Mois 0–6** : 100% open source, gratuit, **aucune monétisation**. Focus : adoption, visibilité, communauté.
2. **Mois 6–12** : maintien open source, premier revenu via **support / consulting** sur demande.
3. **Mois 12–24** : lancement d'une **version SaaS hébergée payante** (instance gérée, alertes, backups, support). Le code reste 100% libre, on monétise la commodité.
4. **Mois 24+** : selon traction, potentiel passage à un modèle **open core** (fonctions premium fermées) ou maintien du modèle pur SaaS-hébergé.

## Rationale

- **Pas de monétisation prématurée** : éviter de brûler des opportunités de feedback gratuit avec des early adopters payants.
- **Construire la réputation d'abord** : un projet open source bien fait est un atout de carrière, indépendamment de toute monétisation.
- **Modèle éprouvé** : c'est la trajectoire de Plausible Analytics, Posthog, Supabase, Cal.com.
- **Compatible avec la situation actuelle** : étudiant + dev indé, sans levée, peut se permettre 6 mois sans revenu direct du projet.

## Conséquences

- **Positives** : flexibilité, validation marché organique, communauté construite avant la monétisation.
- **Négatives** : aucun revenu pendant 6+ mois. Demande de la patience et de la discipline pour ne pas céder à la tentation de monétiser trop tôt.
- **Indicateurs à suivre** :
  - Mois 6 : 100+ stars GitHub, 5+ users actifs, 2+ articles techniques publiés.
  - Mois 12 : 500+ stars, 50+ users, premiers contacts pour du support payant.
  - Mois 24 : 100+ users sur l'instance hébergée payante.
