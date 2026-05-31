# ADR-013 — Substrat de surveillance des entités (snapshot/diff + flux d'items)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026

## Contexte

Quatre features de surveillance suivent une structure très proche : **F-019** (modifications RNE des favoris), **F-048** (annonces BODACC), **F-057** (re-screening sanctions), **F-031** (jurisprudence Judilibre rattachée à l'entité). Toutes itèrent un *set surveillé* (favoris / watchlists), interrogent une source par entité, détectent ce qui est « nouveau », et émettent un événement vers la timeline et les notifications.

Laissées telles quelles, ces features **réécrivent chacune** la même tuyauterie d'orchestration — d'où une duplication coûteuse et une dérive garantie (le même bug à corriger à quatre endroits, quatre niveaux de qualité). Mais une analyse fine montre qu'**elles ne se ramènent pas toutes au même modèle** : le critère décisif est la **détection des disparitions**.

- Certaines dimensions exigent de détecter des **retraits / modifications** : un dirigeant qui part (RNE), une entité qui **sort** d'une liste de sanctions (délistage). → il faut comparer à un **état stocké**.
- D'autres sont des **flux append-only** : une annonce BODACC ou une décision Judilibre **ne disparaît jamais**. → un simple **dédoublonnage des items déjà vus** suffit ; stocker et re-différer un « état » serait du volume non borné inutile.

Forcer les quatre dans une abstraction unique mutilerait la moitié des cas (perte des disparitions, ou stockage non borné). Ne rien factoriser laisserait la duplication proliférer.

## Décision

Mettre en place un **substrat de surveillance** à **deux stratégies** + **un runner d'orchestration partagé**.

**Deux stratégies** (le *quoi*), départagées par le critère des disparitions :

1. **État + diff** (`IStateMonitor<TState>`) — pour les dimensions où les retraits comptent (**RNE**, **sanctions**). On garde un cliché vivant de l'état courant par entité, on le compare au précédent, on émet les ajouts / modifications / **retraits**.
2. **Flux d'items** (`IItemStreamMonitor<TItem>`) — pour les dimensions append-only (**BODACC**, **Judilibre**). On récupère les items, on filtre ceux déjà vus (dédup par `ExternalId`), on émet les nouveaux.

**Un runner d'orchestration partagé** (le *comment*), qui porte **une seule fois** tout le boilerplate commun :
- itération du **set surveillé** ;
- **dédup cross-users** (un seul appel source par SIREN partagé, puis fan-out vers les N users qui le suivent — patron déjà inventé par F-048) ;
- **isolation des échecs** (un user dont l'INPI a expiré ne bloque pas les autres — déjà fait par F-019) ;
- **idempotence** (relancer le job ne ré-émet pas) ;
- gestion des **credentials** (source authentifiée RNE vs anonyme BODACC) ;
- **émission** de l'événement → notification → timeline / push ;
- **observabilité** (entités vérifiées, événements émis, échecs, latence).

**Placement hexagonal** (respecte les règles vérifiées par NetArchTest) :
- les **stratégies et leurs ports** → `Atlas.Domain` ;
- les **fetchers concrets** (savent appeler RNE, BODACC, sanctions, Judilibre) → `Atlas.Infrastructure.*` ;
- le **runner** (orchestration) → `Atlas.Application` ;
- le **job Hangfire** qui déclenche le runner → **adapter entrant**.

**Règle d'extraction (anti-abstraction prématurée)** : la **décision de cible** est prise maintenant, mais l'**extraction effective** du runner se fait **à la troisième instance** (lors de la construction de **F-057**), à partir de code qui existe et passe ses tests — pas d'une abstraction devinée d'avance. F-019 et F-048 restent autonomes jusque-là.

**Garde-fous** :
- **ne pas fusionner** les deux stratégies sous une interface unique « universelle » (ce serait retomber dans l'abstraction qui ment) ;
- **partager le runner, garder les fetchers concrets** : on ne mutualise pas ce qui varie (comment on appelle une source, ce qui compte comme un changement) ;
- **ne pas pré-câbler** de sources hypothétiques.

### Croquis des contrats (illustratif — à finaliser à l'extraction)

```csharp
// ── Domain ──────────────────────────────────────────────

// Stratégie 1 : état + diff (retraits détectés). Ex. RNE, sanctions.
public interface IStateMonitor<TState>
{
    MonitoredDimension Dimension { get; }                 // p.ex. RneIdentity, Sanctions
    bool RequiresCredentials { get; }                     // RNE = true, sanctions = false
    Task<TState?> FetchCurrentStateAsync(Siren siren, MonitorContext ctx, CancellationToken ct);
    IReadOnlyList<MonitoredChange> Diff(TState? previous, TState current);
}

// Stratégie 2 : flux d'items append-only (dédup). Ex. BODACC, Judilibre.
public interface IItemStreamMonitor<TItem>
{
    MonitoredDimension Dimension { get; }
    bool RequiresCredentials { get; }
    Task<IReadOnlyList<TItem>> FetchItemsAsync(Siren siren, MonitorContext ctx, CancellationToken ct);
    ExternalId ExternalIdOf(TItem item);                  // clé de dédup
    MonitoredChange ToChange(TItem item);
}

// Sortie commune aux deux stratégies → devient un événement de timeline.
public sealed record MonitoredChange(
    MonitoredDimension Dimension,
    ChangeKind Kind,            // Added / Modified / Removed (Removed impossible côté flux)
    string Title,
    string Summary,
    ExternalId? ExternalId);

// ── Application ─────────────────────────────────────────
// Le runner partagé : itère le set surveillé, dédup cross-users, isole les échecs,
// idempotent, émet l'événement, observe. Ne connaît que les ports ci-dessus.
```

## Rationale

- **Le critère des disparitions** est technique, pas esthétique : il découle de la **nature des données**, donc le découpage en deux stratégies n'est pas arbitraire.
- **Évite les deux échecs symétriques** : l'abstraction unique « tout est flux » perd les disparitions (régression métier) ; l'abstraction unique « tout est état » stocke un historique non borné (gaspillage). Deux stratégies, chacune à sa place.
- **Élimine la vraie duplication** : le boilerplate d'orchestration (le coûteux, le bug-prone) est mutualisé une fois.
- **Coût d'ajout d'une source** réduit à : un petit fetcher concret + le choix de sa stratégie.
- **Règle de trois respectée** : on abstrait du code éprouvé (3 instances réelles), pas une supposition.

## Conséquences

- **Positives** : un seul endroit pour l'orchestration (donc pour ses bugs), observabilité homogène, ajout de source bon marché, respect strict de l'hexagonal.
- **Négatives** : deux types à comprendre au lieu d'un (coût cognitif réel mais faible, car clairement nommés et distincts) ; discipline requise pour ne pas les fusionner « plus tard ».
- **À prévoir** :
  - **Décision ouverte (déclenchée par cet ADR)** : l'entité **`FavoriteEvent`** porte de plus en plus de types (`RneChanged`, `BodaccPublished`, + sanctions, PI, marchés, jurisprudence) et provient désormais de favoris **et** de watchlists, bientôt de consultations à la demande. Son nom **ne dit plus la vérité**. Faut-il la faire évoluer vers un **`EntityEvent`** plus large ? Renommage coûteux → à trancher **au moment de l'extraction (F-057)**, pas avant.
  - Extraire le runner lors de **F-057** ; refactorer F-019 et F-048 pour l'utiliser dans la foulée (tests d'architecture + tests métier au vert comme filet).
  - Nommer `MonitoredDimension`, `MonitoredChange`, le runner, dans le **doc 08** (pas de synonyme silencieux).
