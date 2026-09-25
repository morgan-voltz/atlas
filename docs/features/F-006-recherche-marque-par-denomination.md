# F-006 — Recherche marque par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — **bloqué côté INPI depuis le 29 juin 2026** (auth PI refusée, accès API PI à redemander — cf. diagnostic du 25 septembre ci-dessous). Port `IIntellectualPropertyProvider`, adapter `InpiPiTrademarkProvider` (auth PI via cookies `access_token` + `XSRF-TOKEN`, cache par compte, retry 401), endpoint `GET /trademarks?name=&page=&pageSize=`. **Limites fortes** : l'API INPI PI (`api-gateway.inpi.fr`) a l'auth la plus complexe de l'écosystème et je n'avais pas le contrat réel — le flux d'auth, le corps de `POST /services/apidiffusion/api/marques/search` et le mapping sont **best-effort, isolés dans l'adapter PI**, à confronter à l'API réelle (compte INPI requis). Tests via WireMock contre des payloads supposés.
>
> **⚠️ Diagnostic réel 31 mai 2026 (compte habilité, vérifié en curl direct contre l'INPI)** :
> - **Auth PI = OK** (primer CSRF 403 → login 200 ; le JWT porte `ROLE_API_MARQUES`/`BREVETS`/`MODELES` ; `refresh_token` posé). **RNE = validé**.
> - **Tout `/services/apidiffusion/api/...` échoue en prod, pas seulement `search`** : `POST .../marques/search` → **405 `Allow: GET`** ; `GET .../marques/notice/{id}` → **404** ; `GET .../marques/metadata` → **404** (endpoint trivial sans paramètre). → ce **n'est pas** un problème de verbe, de cookies ni de params : **les chemins de la doc ne correspondent plus à la prod 2026**. La doc technique PDF (`docs/INPI/Inpi_doc_tech_API_PI_v1.0_0.pdf`) date de 2021 ; `docs/INPI/APIDiffusionV2.json` documente toujours `POST /services/apidiffusion/api/marques/search` — **désynchronisé de la prod**.
> - **Correctif appliqué (PR fix/inpi-pi-session-token)** : ajout du cookie **`session_token=<refresh_token>`** + header **`x-forwarded-for`** sur les appels diffusion (tous deux requis par la doc §3.8/§4.4.5, manquaient). Nécessaire mais **insuffisant** seul (405/404 persistent) — à garder.
> - **Reste à obtenir (Morgan)** : le **vrai chemin + verbe + params de `search` en prod**. Le Swagger exposé (`/docs`, `/v3/api-docs`) est celui du **gateway JHipster** (users/routes), pas la diffusion PI ; `/api/gateway/routes` (qui listerait les vrais chemins) est en **403 admin**. → récupérer via **DevTools → Network sur une recherche réelle du portail data.inpi.fr**, ou auprès de `licences@inpi.fr`. Le code est isolé dans `InpiPiTrademarkProvider` ; seuls le path/verbe/params de `search` restent à ajuster.

> **⚠️ Diagnostic réel 25 septembre 2026** (mêmes identifiants qu'en mai, jamais changés ; nightly `bruno-inpi-e2e` avec les secrets du dépôt + rejeu `curl` direct) :
> - **Auth PI = KO.** Le primer CSRF fonctionne toujours (403 + cookie `XSRF-TOKEN`), mais `POST auth/login` répond **401 « Invalid credentials »** (JHipster) au compte web data.inpi.fr, quel que soit le corps (`username`, `login`, `email`, avec/sans `rememberMe`) ou les en-têtes de navigateur (User-Agent, Origin, Referer). Le **même compte** est accepté à l'instant par l'API RNE (`sso/login` 200) et par le portail. **Régression côté INPI** : la passerelle a été **redéployée le 29 juin 2026** (`/management/info` → gateway 1.2.0), un mois après la validation de mai.
> - Le portail ne se connecte plus via la passerelle : son login est `POST https://data.inpi.fr/login` (302), **derrière un challenge Cloudflare** (`cf-mitigated: challenge`, en GET comme en POST) → injouable depuis un backend.
> - Côté compte, la page « Mes accès APIs PI » liste **Marques, Brevets, Dessins et modèles** comme services à demander, **sans accès actif** ; seuls le RNE et le SFTP Opendata sont accordés. L'accès PI **existait en mai** (le JWT portait `ROLE_API_MARQUES`/`BREVETS`/`MODELES`) et n'apparaît plus comme accordé : il faut le **redemander**. Les identifiants SFTP sont refusés par la passerelle comme par `sso/login`.
> - Conséquence : l'adapter `InpiPiTrademarkProvider` est correct mais **sans chemin d'authentification**. F-006, F-007, F-015 et F-016 restent bloquées tant que l'accès API PI n'est pas ré-accordé. **Action (Morgan)** : **nouvelle demande d'accès API PI** (page « Mes accès APIs PI » du compte, ou `licences@inpi.fr`). Côté produit : prévoir que **RNE et PI n'auront pas les mêmes identifiants** (F-003 → deux couples par utilisateur). D'ici là, les étapes 09/10/11 de `90-INPI-E2E-CI` échouent en `inpi.invalid_credentials` (11/14 vertes, RNE validé).

**Description** : l'utilisateur saisit un nom de marque, le système retourne les marques françaises correspondantes (vivantes ou non).

**Valeur user** : équivalent F-005 pour la PI. Première fonction démontrant la couverture PI du produit.

**Complexité** : ★★★

**APIs externes** : INPI PI (`POST /services/apidiffusion/api/marques/search`).

**Dépendances** : F-003.

**Détails techniques** :
- Auth INPI PI est plus complexe (XSRF + access_token + refresh_token). Cette feature force à implémenter cet adapter dès le MVP 1.
- Parsing XML ou JSON selon le format demandé.
- Affichage : nom marque, déposant, date dépôt, n° dépôt, statut juridique.
