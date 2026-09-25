# F-015 — Recherche brevet par numéro

> **Statut** : ✅ Backend implémenté (MVP 2, commit `85ae014`). Endpoint `GET /patents/{publicationNumber}` qui interroge l'INPI PI brevets via `IIntellectualPropertyProvider.GetPatentAsync`. Route paramétrée placée **après** la route de recherche (F-016) pour éviter la collision. Mapping best-effort `PiPatentMapper.MapDetail`. **Reste** : confirmation contre l'API INPI réelle (structure JSON exacte) et couverture Bruno. ⚠️ La validation réelle dépend de l'accès PI (cf. F-006 : depuis le 29/06/2026 l'**auth PI est refusée par l'INPI** — 401 sur `auth/login` — et le compte n'a pas d'identifiant technique PI ; accès API PI à redemander. La notice par numéro `GET /brevets/notice/...` est à tester une fois l'accès obtenu).

**Description** : recherche d'un brevet par son numéro de publication (FR, EP, WO), affichage de la notice + image d'abrégé.

**Valeur user** : couverture du périmètre PI au-delà des marques.

**Complexité** : ★★

**APIs externes** : INPI PI brevets.

**Dépendances** : F-003.
