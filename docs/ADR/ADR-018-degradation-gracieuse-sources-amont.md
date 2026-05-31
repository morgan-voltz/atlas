# ADR-018 — Dégradation gracieuse des sources amont & code d'erreur dédié

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

Atlas compose ses écrans à partir de **plusieurs sources amont indépendantes** (RNE, INPI PI via `apidiffusion`, BODACC, flux RSS, demain EUIPO/EPO). Ces sources tombent — et tombent **séparément**. Un incident réel l'a montré : le service **INPI PI** (`apidiffusion`) a été **dé-enregistré de la passerelle** `api-gateway.inpi.fr` — `/swagger-resources` ne listait plus que `default` et `uaa`, et `/services/apidiffusion/v2/api-docs` renvoyait `404` — pendant que le RNE restait joignable.

Deux manques sont apparus.

1. **Le contrat ne nomme pas la cause.** Quand `apidiffusion` est down, `/trademarks` et `/patents` n'ont **aucun code dédié** (on a `inpi.unavailable` = 502, mais il est sémantiquement collé au *RNE*). Le risque : remonter un `500` muet, ou pire, **confondre trois faits distincts** — « source indisponible », « aucun résultat » (vide de couverture) et « problème d'accès » (auth). C'est exactement le piège vécu au debug : le premier réflexe a été d'accuser ses propres autorisations alors que la brique était simplement **down chez eux**.
2. **La doctrine de rendu existe mais n'est pas actée.** On la pratique déjà — carte « Source BODACC injoignable » (Accueil), `veille.fetch_failed` en *erreur locale* (doc 12 §14), règle « erreur locale, pas globale ». Mais elle n'est écrite nulle part comme **décision** ; rien ne garantit qu'un futur écran PI la respecte.

Cet ADR **généralise et fige** la doctrine (toute source amont, pas seulement la PI) et **introduit le code concret** pour la PI.

## Décision

Quatre principes pour « une source amont est indisponible ».

**1. Trois faits distincts, jamais confondus.** Le système — contrat *et* UI — sépare :
- **Indisponible** : on n'a *pas pu* interroger la source (down, timeout, dé-enregistrée). *On ne sait pas.*
- **Vide de couverture** : on a interrogé, la source répond **rien** (aucune marque, compte confidentiel). *On sait qu'il n'y a rien — ou qu'on n'a pas le droit de voir.*
- **Problème d'accès** : auth / droits (`inpi.invalid_credentials`, `inpi.api_access_not_allowed`, `inpi.not_connected`). *Le problème est chez l'utilisateur, réparable par lui.*

Confondre « indisponible » et « vide » est un **mensonge par omission** (laisser croire qu'on a regardé et qu'il n'y a rien) — incompatible avec ADR-012. Confondre « indisponible » et « accès » envoie l'utilisateur réparer ce qui n'est pas cassé.

**2. Le contrat nomme la cause.** Nouveau code métier **`inpi.pi_unavailable`** (ProblemDetails RFC 7807, **HTTP 502** — cohérent avec `inpi.unavailable` et `veille.fetch_failed`), porté par `/trademarks*` et `/patents*`. La **classification vit dans l'adapter** (`InpiPiTrademarkProvider`, adapter brevets) : un 404 / 503 / timeout / connexion refusée de la passerelle, ou l'absence d'`apidiffusion`, **devient** `inpi.pi_unavailable` ; un 401 / 403 reste un code d'**accès**. Le client **clé son UX sur le code métier**, jamais sur le statut HTTP brut. Indice de réessai optionnel via une extension ProblemDetails `retryAfterSeconds` (jamais un code technique affiché à l'écran).

**3. Dégradation à portée locale.** Une page compose N sources ; si l'une tombe, **seule sa zone** passe en état dégradé — le reste vit (doc 12 §14, « erreur locale, pas globale »). Concrètement : segment **Marques** de la Recherche en erreur, segment **Entreprises** (RNE) intact ; **section PI** d'une fiche / vue 360 en erreur, reste du dossier intact — aligné sur `SectionState` (ADR-015) : une section PI indisponible est un **état de section explicite**, pas une section absente.

**4. Rendu client honnête et calme.** On **réutilise le composant Erreur** existant (doc 12 §14) — aucun composant neuf :
- Message **non technique** (« Source INPI Propriété Industrielle momentanément indisponible ») + **Réessayer**. Jamais le code brut, jamais de détail sensible.
- **Jamais rendu comme un vide** (« 0 résultat », « aucune marque ») — c'est le principe 1, version pixels.
- `danger` n'y qualifie qu'un **état système** (réseau), jamais une entité (ADR-012) ; **icône + texte**, jamais la couleur seule (doc 06).
- **Provenance gardée** : la source reste nommée (« INPI PI »), elle ne s'efface pas.
- **Accessibilité** : état **annoncé**, focus envoyé au message / bouton (doc 06).
- **Web** (doc 14) : un deep-link `/marque/{id}` pendant que la PI est down affiche l'**état dégradé**, jamais un `404` (« introuvable ») ni un écran cassé — « indisponible » ≠ « introuvable ».

### Croquis (illustratif)

```jsonc
// ProblemDetails renvoyé par GET /trademarks quand apidiffusion est down
{
  "type": "inpi.pi_unavailable",
  "title": "inpi.pi_unavailable",
  "status": 502,
  "detail": "Source INPI Propriété Industrielle momentanément indisponible.",
  "retryAfterSeconds": 120          // extension optionnelle, pour le back-off client
}
```

```csharp
// Adapter PI — la CLASSIFICATION de la panne vit ici (hexagonal : le domaine ignore HTTP).
try
{
    var raw = await _piGateway.SearchTrademarksAsync(name, ct);
    return Map(raw);
}
catch (PiUpstreamUnreachable)            // 404/503/timeout/connexion refusée, apidiffusion absent
{
    throw new SourceUnavailableException(Source.InpiPi);   // → mappé en `inpi.pi_unavailable` (502)
}
// 401/403 NE sont PAS attrapés ici : ils restent des codes d'ACCÈS (auth), pas d'indisponibilité.
```

## Rationale

- **Rendre la panne lisible**, c'est protéger l'utilisateur *et* le code : l'incident a montré qu'une indisponibilité amont **muette** se fait lire comme « mes droits » ou « rien à signaler ». Le contrat doit trancher dès la source.
- **Réutilisation, pas invention** : la grammaire d'états (doc 12 §14), `SectionState` (ADR-015) et la doctrine descriptive (ADR-012) **portent déjà** la solution — on l'**hérite**, comme la surface MCP hérite « indisponible ≠ rien à signaler » (ADR-016). Cet ADR est le pendant *client humain* de cette même règle.
- **Cohérence du contrat** : `inpi.pi_unavailable` (502) rejoint la famille `*.unavailable` / `fetch_failed` déjà en place ; le client se branche sur le **code**, ce qui découple l'UX du statut HTTP.
- **Sources indépendantes → pannes indépendantes** : la portée locale est la seule réponse honnête à une topologie multi-sources.

## Conséquences

- **Positives** : l'utilisateur sait *toujours* dans lequel des trois mondes il est ; une panne PI ne fait pas tomber la fiche ni faire mentir la Recherche ; zéro composant neuf ; doctrine désormais **opposable** à tout futur écran / source.
- **Négatives** : **discipline d'adapter** — chaque provider amont doit classer ses pannes (indisponible vs accès vs vide) plutôt que laisser fuiter un 500 ; un peu de mapping en plus par endpoint PI.
- **À prévoir** :
  - **doc 11** : ajouter `inpi.pi_unavailable` (502) aux tables d'erreurs `/trademarks` et `/patents`.
  - **doc 12 §14 / doc 14** : acter le rendu (erreur locale PI, deep-link dégradé).
  - **Adapter** : `SourceUnavailableException` + classification dans `InpiPiTrademarkProvider` et l'adapter brevets ; `SectionState` (ADR-015) doit porter un état « indisponible » explicite s'il ne l'a pas déjà.
  - **Hors-scope (volontaire)** : retry / back-off et **health-check** « INPI PI up ? » — back-end pur, à traiter dans un ADR / feature dédié (résilience). Ici on s'arrête au **contrat + rendu**.
  - **Harmonisation** : envisager de regrouper `inpi.unavailable` (RNE) et `veille.fetch_failed` sous une famille `*.source_unavailable` cohérente (non bloquant).
  - **Références croisées** : **F-006 / F-007** (marques), **F-015 / F-016** (brevets), **F-056** (vue 360), **F-058** ; **ADR-012** (descriptif), **ADR-015** (`SectionState`), **ADR-016** (même doctrine, surface agentique) ; **doc 11 / 12 / 14**.
