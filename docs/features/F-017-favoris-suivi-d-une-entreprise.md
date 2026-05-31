# F-017 — Favoris : suivi d'une entreprise

> **Statut** : ✅ Backend implémenté (MVP 2, 28 mai 2026). Entité `CompanyFavorite` (UserId + Siren + NameSnapshot optionnel + AddedAt) avec index unique `(UserId, Siren)` et cascade FK RGPD sur User. Endpoints `POST /favorites/companies`, `DELETE /favorites/companies/{siren}`, `GET /favorites/companies`. Couvre la base du tableau de bord et débloque F-019 (alertes) puis F-047 (timeline mixte veille+favoris, feature phare). **Reste** : UI MAUI (onglet « Mes favoris »).

**Description** : l'utilisateur marque une entreprise en favori. La fiche est alors accessible depuis un onglet "Mes favoris" et automatiquement mise à jour à chaque consultation.

**Valeur user** : pose les bases du tableau de bord et des alertes.

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-004, F-001.
