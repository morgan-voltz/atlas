# ADR-015 — Couche d'assemblage du dossier entreprise (sections auto-descriptives + résolution snapshot-first)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026

## Contexte

Le dossier 360 (**F-056**) compose ~8 sections (identité, finances, marchés, PI, cotation, structure, risque, événements). Ces sections n'ont **rien de comparable** par leur profil :
- **bon marché et fraîches en direct** : événements BODACC, marchés DECP ;
- **chères et lentes** : tout l'INPI (identité, bilans, PI) — credentials par utilisateur, rate-limité ;
- **déjà pré-calculées** : pour une entité **suivie**, l'état RNE, les sanctions, les items BODACC/Judilibre existent déjà dans les **snapshots de l'ADR-013** ;
- **conditionnelles** : structure (F-034, sous DPIA), finances (comptes parfois confidentiels), cotation (sans objet si non cotée).

Un assemblage naïf écrase **trois tensions** :
1. **La fraîcheur n'est pas atomique.** Le dossier est un *patchwork* où chaque morceau est frais à une date différente. Le présenter comme une photo cohérente à un instant T serait mentir.
2. **La latence.** Assembler les 8 en synchrone laisse la source la plus lente (l'INPI) prendre toute la réponse en otage.
3. **L'absence honnête.** « Section vide » a **quatre sens** incompatibles : *rien à signaler*, *pas pu récupérer*, *sans objet*, *restreint*. Les confondre, c'est présenter une absence comme un fait — le piège déjà neutralisé en F-059.

## Décision

**1. Sections indépendantes et auto-descriptives.** Chaque section se résout en `{ état, as-of, provenance, donnée? }`. L'**état est porteur de doctrine** : `Available` / `Stale` / `Unavailable` / `NotApplicable` / `Restricted` — il **distingue les quatre sens** de « vide ». Une section qui échoue ou expire tombe en `Unavailable` ; elle **ne fait jamais échouer le dossier entier**. Le dossier **rend toujours ce qu'il a**.

**2. Résolution « snapshot-first ».** Le substrat de surveillance (**ADR-013**) **maintient déjà** les snapshots de plusieurs dimensions pour les entités **suivies**. Le dossier d'une entité suivie **lit ces snapshots** plutôt que de refaire les appels coûteux. Pour une entité **non suivie** : assemblage à la demande avec cache court. Conséquence structurante : **le dossier 360 et le substrat de surveillance partagent la même donnée** — les snapshots ne servent pas qu'à émettre des événements, ils sont aussi le **pré-assemblage** du dossier. Une seule machinerie, deux usages.

**3. La fraîcheur par section est de premier rang.** Chaque section porte sa **date « as of »** et sa **provenance** — l'auditabilité (le fil rouge du produit) appliquée à la structure même du dossier.

**4. Le transport est secondaire et évolutif.** Parce que les sections sont indépendantes, renvoyer le dossier **composé d'un coup** ou **progressivement** (section par section) est un choix **non architectural**. En v1 : **réponse composée avec timeout par section** (simple ; le chemin snapshot rend les entités suivies rapides de toute façon). Le **progressif/streaming** en évolution ultérieure, quand les contrats de section seront stables — on ne paie pas cette complexité avant d'en avoir besoin.

**Placement hexagonal** : `CompanyDossier` est un **read-model côté Application** (pas un agrégat de domaine — `Company`/`UniteLegale` le restent). Chaque **résolveur de section** est un petit service Application qui, soit **lit un snapshot** (ADR-013), soit **appelle le use case** de la source, avec timeout, et **mappe tout échec en `Unavailable`**.

### Croquis (illustratif)

```csharp
// Application — read-model du dossier (pas un agrégat de domaine).
public sealed record CompanyDossier(Siren Subject, IReadOnlyList<DossierSection> Sections);

public sealed record DossierSection(
    DossierSectionKind Kind,    // Identity, Financials, PublicContracts, Ip, Listing, Structure, Risk, Events
    SectionState State,         // état PORTEUR DE DOCTRINE
    DateTimeOffset? AsOf,       // fraîcheur PROPRE à la section
    string? Provenance,         // source + base, pour l'audit
    object? Data);              // présent si State ∈ { Available, Stale }

public enum SectionState
{
    Available,     // donnée fraîche
    Stale,         // présente mais périmée (à rafraîchir)
    Unavailable,   // pas pu récupérer (échec / INPI non connecté)
    NotApplicable, // sans objet (p.ex. cotation d'une non-cotée)
    Restricted     // existe mais non servi (structure sous DPIA, comptes confidentiels…)
}

// Chaque résolveur : snapshot-first si entité suivie, sinon à la demande ;
// timeout par section ; tout échec → Unavailable (jamais d'exception qui casse le dossier).
public interface IDossierSectionResolver
{
    DossierSectionKind Kind { get; }
    Task<DossierSection> ResolveAsync(Siren siren, DossierContext ctx, CancellationToken ct);
}
```

## Rationale

- **Fraîcheur par section** = représentation honnête d'un patchwork ; l'auditabilité appliquée au dossier lui-même.
- **Sections indépendantes** = isolation de la latence (la source lente ne gate plus tout) **et** dégradation propre.
- **État porteur de doctrine** = les quatre sens de « vide » distingués ; aucune absence présentée comme un fait (même garde-fou que F-059).
- **Snapshot-first** = réutilise le substrat ADR-013 ; le dossier et la surveillance partagent la donnée → entités suivies quasi instantanées, **zéro duplication de fetch**. C'est la **cohérence d'architecture** qui paie : deux features qu'on croyait séparées reposent sur le même socle.
- **Transport secondaire** = on n'achète pas la complexité du streaming tant qu'elle n'est pas nécessaire.

## Conséquences

- **Positives** : le dossier rend toujours quelque chose ; fraîcheur et absence honnêtes ; entités suivies quasi instantanées ; pas de double fetch ; transport évolutif sans refonte.
- **Négatives** : chaque section doit **déclarer correctement son état** (discipline) ; la fraîcheur par section est davantage à exposer dans l'UI — mais c'est la chose honnête.
- **À prévoir** :
  - Nommer `CompanyDossier`, `DossierSection`, `SectionState`, `IDossierSectionResolver` dans le **doc 08**.
  - Le chemin snapshot-first **dépend de la mise en place du substrat ADR-013**.
  - L'UI doit rendre la **fraîcheur par section** et les **cinq états** honnêtement (ne jamais afficher `Unavailable`/`Restricted` comme un « rien à signaler »).
  - Cet ADR est le **backbone d'assemblage de F-056** — à référencer croisé.
