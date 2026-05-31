# F-005 — Recherche entreprise par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026). `ICompanyDataProvider.SearchByNameAsync` → `PagedResult<CompanySummary>` (SIREN, dénomination, ville, NAF) ; endpoint `GET /companies?name=&page=&pageSize=` (réutilise le client RNE + cache token). Mapping des items aligné sur la doc INPI v4.0 (navigation défensive, cf. F-004). **Reste à valider en réel** : pagination (par page vs curseur `searchAfter`) et `TotalCount` (forme de réponse de recherche non documentée publiquement). Debouncing 300 ms côté client (MAUI, F-009).

**Description** : l'utilisateur saisit tout ou partie d'un nom d'entreprise, le système retourne une liste de résultats avec pagination.

**Valeur user** : la majorité des recherches commencent sans connaître le SIREN. Cette fonction est complémentaire de F-004.

**Complexité** : ★★

**APIs externes** : INPI RNE (`GET /companies?companyName=...`).

**Dépendances** : F-003.

**Détails techniques** :
- Debouncing côté client (300ms) pour éviter de saturer l'API.
- Pagination avec curseur (`searchAfter`).
- Affichage des résultats avec SIREN, dénomination, ville, code NAF.
