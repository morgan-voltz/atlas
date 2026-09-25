# F-016 — Recherche brevet par titre / inventeur / déposant

> **Statut** : 🟡 Backend implémenté (MVP 2, 29 mai 2026). `PatentSearchQuery(Title?, Inventor?, Applicant?, Page, PageSize)` + `PatentSummary` + `IIntellectualPropertyProvider.SearchPatentsAsync`. Endpoint `GET /patents?title=&inventor=&applicant=&page=&pageSize=` (route précédant celle paramétrée pour éviter collision avec F-015). Validation : ≥ 1 critère renseigné → 400 `patents.empty_search` sinon. Pagination clampée. Adapter via `POST /services/apidiffusion/api/brevets/search`. Mapping best-effort `PiPatentMapper.MapSummary`. **Reste** : confirmation contre l'API INPI réelle (syntaxe SolR exacte, structure de la réponse paginée) — **bloquée** : auth PI refusée par l'INPI depuis le 29/06/2026, accès API PI à redemander (cf. F-006, diagnostic du 25/09/2026). PR #40.
>
> **⚠️ Diagnostic réel 31 mai 2026** : comme F-006, **tout `/services/apidiffusion/api/...` échoue en prod** (`POST brevets/search` → 405 `Allow: GET` ; `metadata`/`notice` en GET → 404). Ce n'est pas un problème de verbe/cookies/params mais de **chemins prod ≠ doc 2021/V2**. Correctif appliqué (cookie `session_token` + header `x-forwarded-for`) — nécessaire mais insuffisant. **Le vrai contrat de `search` prod reste à obtenir** (DevTools sur le portail / `licences@inpi.fr`). Détail complet dans la fiche **F-006**. L'auth PI et le RNE sont validés.

**Description** : recherche avancée multi-critères sur la base brevets.

**Valeur user** : veille technologique, recherche d'antériorité brevet.

**Complexité** : ★★★

**APIs externes** : INPI PI brevets (`/search`).

**Dépendances** : F-015.

**Détails techniques** : maîtrise de la syntaxe de requête SolR INPI (`[DENM=...]`, `[TIT=...]`, etc.) à abstraire derrière un query builder C# fluent.
