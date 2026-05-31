# F-044 — Timeline unifiée de la veille

> **Statut** : ✅ Backend implémenté (MVP 2, 27 mai 2026). Timeline par utilisateur agrégeant abonnements libres (F-043) et packs (F-042), états par item (`FeedItemUserState` : lu / non lu / favori / archivé), filtrage par source/date/mot-clé, pagination. Endpoints `/timeline/*`. **Reste** : UI MAUI (cf. doc 06 §UI veille). PR #20.

**Description** : vue principale de la feature : une timeline chronologique inversée affichant tous les items des sources abonnées de l'utilisateur, avec filtres et tri.

**Valeur user** : l'interface où le user passe son temps. Le "Twitter de sa veille".

**Complexité** : ★★★★

**APIs externes** : aucune (consomme F-041).

**Dépendances** : F-041, F-042, F-043.

**Détails techniques** :
- UI MAUI accessible (cf. doc 06 — section UI veille)
- Pagination infinie ou par "load more"
- Marquer comme lu/non lu, favori, archivé
- Filtres : par source, par date, par mot-clé
