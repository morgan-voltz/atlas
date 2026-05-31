# F-016 — Recherche brevet par titre / inventeur / déposant

> **Statut** : 🟡 Backend implémenté (MVP 2, 29 mai 2026). `PatentSearchQuery(Title?, Inventor?, Applicant?, Page, PageSize)` + `PatentSummary` + `IIntellectualPropertyProvider.SearchPatentsAsync`. Endpoint `GET /patents?title=&inventor=&applicant=&page=&pageSize=` (route précédant celle paramétrée pour éviter collision avec F-015). Validation : ≥ 1 critère renseigné → 400 `patents.empty_search` sinon. Pagination clampée. Adapter via `POST /services/apidiffusion/api/brevets/search`. Mapping best-effort `PiPatentMapper.MapSummary`. **Reste** : confirmation contre l'API INPI réelle (syntaxe SolR exacte, structure de la réponse paginée). PR #40.

**Description** : recherche avancée multi-critères sur la base brevets.

**Valeur user** : veille technologique, recherche d'antériorité brevet.

**Complexité** : ★★★

**APIs externes** : INPI PI brevets (`/search`).

**Dépendances** : F-015.

**Détails techniques** : maîtrise de la syntaxe de requête SolR INPI (`[DENM=...]`, `[TIT=...]`, etc.) à abstraire derrière un query builder C# fluent.
