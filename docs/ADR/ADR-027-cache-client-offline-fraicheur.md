# ADR-027 — Cache client hors-ligne : fraîcheur datée & grammaire d'états honnêtes

**Statut** : ✅ Accepté
**Date** : 31 mai 2026

## Contexte

L'app mobile MAUI doit rester consultable **hors connexion** (transports, zones rurales) — c'est le besoin de **F-029** (jusqu'ici un stub V3+). La **techno est déjà actée** : `Atlas.Maui/Storage/LocalStorage.cs` avec **SQLite-net-pcl** « pour offline », **Polly** côté client pour la résilience, **Refit** pour les appels typés. Et la **topologie est posée** (ADR-002) : les clients (MAUI / Avalonia / Web) sont des **consommateurs HTTP purs** ne référençant que `Domain` + `Shared` ; toute donnée transite par `Atlas.Api`.

Le vrai enjeu **n'est pas technique, il est doctrinal**. Servir du cache, pour Atlas, c'est montrer une donnée **qui n'est plus forcément à jour** — alors que tout le produit repose sur « **tout est daté, à jour ≠ vide** » : la fiche porte le bouclier « À jour · {date} », l'atome `source · date` est partout (doc 12), et `SectionState` (ADR-015) encode l'honnêteté. **Une donnée servie depuis le cache ne peut pas se présenter comme « à jour ».** Reste à décider **comment le hors-ligne entre dans la grammaire d'états honnêtes** — celle-là même qu'on a enrichie avec `Stale` (couche de confiance) et `Unobserved` (machine à remonter le temps).

## Décision

Six principes.

**1. Le cache mémorise des réponses datées — il ne réplique pas le domaine.** Le store SQLite conserve des **read-models / DTO** (ce que l'API a renvoyé) **+ la date de récupération** — **pas** de logique métier, **pas** d'appel externe, **pas** de secret. Le client reste **pur** (ADR-002) : c'est une couche « se souvenir de ce que l'API a dit, et quand », pas une réplication du `Domain`. La topologie est préservée, pas contournée.

**2. Une donnée en cache ne se présente JAMAIS comme « à jour ».** En ligne, le dossier affiche « À jour · {date} » (snapshot, ADR-013). Hors-ligne, **le même dossier servi du cache** affiche « **Hors-ligne · vu le {date}** » → mappé sur **`Stale`** (ADR-015). Cohérence stricte avec « tout est daté » : on ne cache pas l'information, on **date** sa fraîcheur.

**3. La grammaire d'états honnêtes, étendue à l'axe réseau.** Trois cas, jamais confondus :
- **en cache + hors-ligne** → `Stale` (« vu le {date} ») ;
- **pas en cache + hors-ligne** → `Unavailable` (« **indisponible hors-ligne** ») ;
- **jamais → vide** (un vide laisserait croire qu'il n'existe pas de donnée — mensonge).
C'est « à jour ≠ vide » appliqué au réseau, exactement comme `Unobserved` l'a appliqué au temps. **Distinction état global / par-item** : un **bandeau global** signale la *condition réseau* (« Mode hors-ligne — données du {date} ») — c'est un **état système** (traitement `danger`/système de doc 12 §14, **jamais** une qualification d'entité, ADR-012) ; la **fraîcheur par-item** (`vu le {date}`) reste portée par chaque section.

**4. Lecture seule en v1.** Aucune **mutation hors-ligne** (pas d'ajout de favori sans réseau à resynchroniser). Les écritures hors-ligne ouvrent toute la **résolution de conflits** — que **F-062** n'a tranchée que pour les *préférences* (clé-valeur à faible enjeu, last-write-wins par clé). Pour la donnée métier, la lecture seule délivre **80 % de la valeur** (consulter) sans le morceau dur. L'écriture hors-ligne = incrément ultérieur.

**5. Pas de TTL d'expiration dur — on garde et on date.** Une donnée en cache n'est **jamais** « expirée puis cachée » (ce serait un vide menteur) : elle est **montrée avec son âge**, et **rafraîchie au retour du réseau**. L'**éviction** existe (borne d'espace, LRU sur les dossiers — les favoris sont gardés), mais elle relève de la **place**, **jamais** de la fraîcheur : on ne masque pas une donnée parce qu'elle est « trop vieille ».

**6. Sécurité : cache protégé, purgé à la déconnexion.** Le cache (watchlist, dossiers) peut être **commercialement sensible** (cibles M&A) → **chiffrement au repos** + **purge à la déconnexion** (cohérent avec la révocation de session de doc 12). Les **identifiants INPI ne sont jamais mis en cache** (ils vivent en `SecureStorage`, chiffrés — cf. écran « tes identifiants restent chiffrés »).

### Croquis (illustratif)

```csharp
// Entrée de cache : un read-model renvoyé par l'API + sa date de récupération.
public sealed record CachedResource<T>(
    T Payload,               // DTO / read-model — PAS de Domain logic, PAS de secret
    DateTimeOffset FetchedAt, // « vu le … »
    string SourceRef);        // provenance héritée de l'API

// Résolution offline-aware : on ne ment jamais sur la fraîcheur.
SectionState ResolveOffline(bool isOnline, bool inCache) =>
    (isOnline, inCache) switch
    {
        (true,  _)     => SectionState.Available,    // chemin normal
        (false, true)  => SectionState.Stale,        // « hors-ligne · vu le {date} »
        (false, false) => SectionState.Unavailable,  // « indisponible hors-ligne » — JAMAIS vide
    };

// Bandeau global = état SYSTÈME (réseau), jamais une qualification d'entité (ADR-012).
// « Mode hors-ligne — données du {FetchedAt} »
```

## Rationale

- **Le cache comme « mémoire datée de l'API »** préserve la topologie client-pur (ADR-002) : aucun secret, aucune logique, aucun appel externe ne descend dans le client.
- **« Jamais à jour si ça vient du cache »** est la seule posture compatible avec le pilier du produit — c'est `Stale` réutilisé, pas un paradigme neuf.
- **La grammaire d'états** (cached→Stale / absent→Unavailable / jamais vide) est **déjà la doctrine** : on l'étend au réseau comme `Unobserved` l'a étendue au temps. Zéro concept nouveau pour l'utilisateur.
- **Lecture seule** évite la boîte de Pandore des conflits ; **pas de TTL dur** évite le vide menteur.

## Conséquences

- **Positives** : usage en mobilité sans trahir la doctrine ; réutilise SQLite/Polly (actés) + la grammaire d'états ; topologie préservée ; surface de conflit nulle (lecture seule).
- **Négatives** : **deux chemins de résolution** (online / offline-aware) à tenir dans les ViewModels ; chiffrement-au-repos + purge = discipline sécurité ; l'éviction LRU demande un soin (ne jamais évincer ce que l'utilisateur croit « épinglé »).
- **À prévoir** :
  - **F-029** (promue de stub à spec : le cache de lecture) et **F-075** (moteur de fraîcheur & sync) implémentent.
  - **doc 08** : nommer `CachedResource<T>`, la résolution offline-aware, le mapping réseau→`SectionState`.
  - **doc 12 §14** : ajouter l'**indicateur hors-ligne** (bandeau global *système* + fraîcheur datée par-item), aligné sur le composant Erreur (portée, jamais la couleur seule).
  - **doc 14 (web)** : acter que le **offline web est hors v1** — l'app est en **WASM pur** (zéro état serveur, mais l'API reste nécessaire aux données) ; un cache navigateur (IndexedDB / service worker) est un **mécanisme distinct** → **avenant futur**, comme le desktop **Avalonia** (stockage propre).
  - **Écriture hors-ligne** = incrément ultérieur (réutiliserait le merge par-clé de F-062, étendu au métier — non trivial).
  - **Références croisées** : **F-029 / F-075** ; **F-062** (précédent sync préférences, merge par clé), **F-017 / F-056** (contenu caché : favoris / dossier), **F-009** (MAUI mobile) ; **ADR-002** (client pur), **ADR-013** (snapshot / « à jour »), **ADR-015** (`SectionState`), **ADR-022** (`Unobserved`, état frère), **ADR-012** (état système ≠ verdict) ; **doc 06 / 12 / 14**.
