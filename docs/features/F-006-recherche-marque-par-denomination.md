# F-006 — Recherche marque par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle. Port `IIntellectualPropertyProvider`, adapter `InpiPiTrademarkProvider` (auth PI via cookies `access_token` + `XSRF-TOKEN`, cache par compte, retry 401), endpoint `GET /trademarks?name=&page=&pageSize=`. **Limites fortes** : l'API INPI PI (`api-gateway.inpi.fr`) a l'auth la plus complexe de l'écosystème et je n'avais pas le contrat réel — le flux d'auth, le corps de `POST /services/apidiffusion/api/marques/search` et le mapping sont **best-effort, isolés dans l'adapter PI**, à confronter à l'API réelle (compte INPI requis). Tests via WireMock contre des payloads supposés.
>
> **⚠️ Diagnostic réel 31 mai 2026 (compte habilité, vérifié en curl direct contre l'INPI)** :
> - **Auth PI = OK** (primer CSRF 403 → login 200 ; le JWT porte `ROLE_API_MARQUES`/`BREVETS`/`MODELES` ; `refresh_token` posé). **RNE = validé**.
> - **Tout `/services/apidiffusion/api/...` échoue en prod, pas seulement `search`** : `POST .../marques/search` → **405 `Allow: GET`** ; `GET .../marques/notice/{id}` → **404** ; `GET .../marques/metadata` → **404** (endpoint trivial sans paramètre). → ce **n'est pas** un problème de verbe, de cookies ni de params : **les chemins de la doc ne correspondent plus à la prod 2026**. La doc technique PDF (`docs/INPI/Inpi_doc_tech_API_PI_v1.0_0.pdf`) date de 2021 ; `docs/INPI/APIDiffusionV2.json` documente toujours `POST /services/apidiffusion/api/marques/search` — **désynchronisé de la prod**.
> - **Correctif appliqué (PR fix/inpi-pi-session-token)** : ajout du cookie **`session_token=<refresh_token>`** + header **`x-forwarded-for`** sur les appels diffusion (tous deux requis par la doc §3.8/§4.4.5, manquaient). Nécessaire mais **insuffisant** seul (405/404 persistent) — à garder.
> - **Reste à obtenir (Morgan)** : le **vrai chemin + verbe + params de `search` en prod**. Le Swagger exposé (`/docs`, `/v3/api-docs`) est celui du **gateway JHipster** (users/routes), pas la diffusion PI ; `/api/gateway/routes` (qui listerait les vrais chemins) est en **403 admin**. → récupérer via **DevTools → Network sur une recherche réelle du portail data.inpi.fr**, ou auprès de `licences@inpi.fr`. Le code est isolé dans `InpiPiTrademarkProvider` ; seuls le path/verbe/params de `search` restent à ajuster.

**Description** : l'utilisateur saisit un nom de marque, le système retourne les marques françaises correspondantes (vivantes ou non).

**Valeur user** : équivalent F-005 pour la PI. Première fonction démontrant la couverture PI du produit.

**Complexité** : ★★★

**APIs externes** : INPI PI (`POST /services/apidiffusion/api/marques/search`).

**Dépendances** : F-003.

**Détails techniques** :
- Auth INPI PI est plus complexe (XSRF + access_token + refresh_token). Cette feature force à implémenter cet adapter dès le MVP 1.
- Parsing XML ou JSON selon le format demandé.
- Affichage : nom marque, déposant, date dépôt, n° dépôt, statut juridique.
