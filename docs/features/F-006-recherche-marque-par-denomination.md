# F-006 — Recherche marque par dénomination

> **Statut** : 🟡 Implémenté (MVP 1, 27 mai 2026) — à valider contre l'API réelle. Port `IIntellectualPropertyProvider`, adapter `InpiPiTrademarkProvider` (auth PI via cookies `access_token` + `XSRF-TOKEN`, cache par compte, retry 401), endpoint `GET /trademarks?name=&page=&pageSize=`. **Limites fortes** : l'API INPI PI (`api-gateway.inpi.fr`) a l'auth la plus complexe de l'écosystème et je n'avais pas le contrat réel — le flux d'auth, le corps de `POST /services/apidiffusion/api/marques/search` et le mapping sont **best-effort, isolés dans l'adapter PI**, à confronter à l'API réelle (compte INPI requis). Tests via WireMock contre des payloads supposés.
>
> **⚠️ Diagnostic réel 31 mai 2026 (compte habilité)** : l'**auth PI fonctionne** (primer CSRF 403 → login 200 ; le JWT porte `ROLE_API_MARQUES`/`BREVETS`/`MODELES`) et le **RNE est validé**, mais `POST /services/apidiffusion/api/marques/search` renvoie **`405 Method Not Allowed` avec `Allow: GET`** : **l'API réelle attend `GET`, pas `POST`** (la spec `docs/INPI/APIDiffusionV2.json` documente POST mais est désynchronisée de la prod). Notre adapter envoie POST → l'API mappe ça en `502 inpi.unavailable`. **À corriger** : passer marques/brevets `search` en `GET` avec le bon format de paramètres query (contrat GET exact à établir — les essais `q=`/`query=[Mark=…]` renvoient un 404 Tomcat). Recherche du contrat en cours (Morgan). Le code est isolé dans `InpiPiTrademarkProvider`.

**Description** : l'utilisateur saisit un nom de marque, le système retourne les marques françaises correspondantes (vivantes ou non).

**Valeur user** : équivalent F-005 pour la PI. Première fonction démontrant la couverture PI du produit.

**Complexité** : ★★★

**APIs externes** : INPI PI (`POST /services/apidiffusion/api/marques/search`).

**Dépendances** : F-003.

**Détails techniques** :
- Auth INPI PI est plus complexe (XSRF + access_token + refresh_token). Cette feature force à implémenter cet adapter dès le MVP 1.
- Parsing XML ou JSON selon le format demandé.
- Affichage : nom marque, déposant, date dépôt, n° dépôt, statut juridique.
