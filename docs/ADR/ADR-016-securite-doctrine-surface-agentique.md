# ADR-016 — Sécurité & doctrine de la surface agentique (MCP)

**Statut** : ✅ Accepté
**Date** : 29 mai 2026

## Contexte

Le serveur MCP (**F-052**) expose Atlas à des **agents** via un nouvel adapter entrant `Atlas.Mcp`, par-dessus les use cases MediatR existants. F-052 a posé la feature (*read-first / write-guarded*, jamais de credentials ni de suppression de compte exposés) et **ADR-011** l'authentification (OAuth 2.1 via OpenIddict, PKCE, scopes, resource indicators RFC8707, jetons courts).

Reste à décider l'**architecture de sécurité et de préservation de doctrine** de cette surface — car le consommateur n'est plus un humain qui lit, mais un agent qui **interprète et agit**. Deux faits cadrent la décision :

1. **MCP est un protocole d'interopérabilité, pas un cadre de sécurité** : le travail de sûreté retombe sur l'auteur du serveur, et **croît avec le privilège exposé** (un serveur en lecture seule a une couche fine ; un serveur qui mute, une couche épaisse). Menaces nommées en 2026 : *confused deputy*, *tool poisoning / rug pull*, **injection de prompt indirecte via le contenu retourné**, *token passthrough*, scopes excessifs.
2. On a passé trois ADR (012, 014, 015) à encoder « **descriptif, jamais de verdict** » dans les types et les états. Un agent peut **aplatir** cette doctrine (« correspondance à vérifier » → « entreprise sanctionnée ») dans son résumé à l'utilisateur.

## Décision

Quatre principes pour la surface agentique.

**1. Surface curée, lecture d'abord (confinement).** L'adapter MCP **n'expose jamais les handlers 1:1**. C'est un **allowlist délibéré** : outils de **lecture** d'abord ; quelques **écritures** sous scope explicite **et** approbation humaine ; les use cases destructifs ou sensibles (suppression de compte, lecture/écriture de credentials, gestion INPI) **structurellement inatteignables**. **Pas d'outil « omnibus »** à entrée libre (le plus dangereux). Schéma d'entrée **le plus étroit possible** par outil. Scopes en **moindre privilège progressif** (socle `mcp:lecture-base`, élévation ciblée). *Garder la surface étroite, c'est garder la couche de sécurité fine et le rayon d'explosion minimal.*

**2. La doctrine voyage avec la donnée.** Parce que `MatchCandidate` n'a **aucun** champ « confirmé » (ADR-014) et que `SectionState` est explicite (ADR-015), **la sérialisation MCP hérite de la doctrine gratuitement**. À garantir : (a) **ne jamais aplatir** ces champs dans le mapping MCP ; (b) garder le caveat **inline par item** (pas en métadonnée détachable) ; (c) **réaffirmer la doctrine dans les descriptions d'outils** (« retourne des correspondances à vérifier, jamais des faits établis ; une section indisponible/restreinte n'est pas « rien à signaler » »).

**3. Le contenu externe est de la donnée, jamais une instruction.** Les outils renvoient du **texte externe** (presse, décisions, observations RNE, annonces) — surface d'**injection de prompt indirecte**. Parade : ce contenu est retourné **clairement délimité comme donnée**, **jamais exécuté** ni traité comme consigne ; et la **surface d'écriture est si étroite** qu'un agent détourné ne peut pas faire de dégâts. L'étroitesse (principe 1) **est** le confinement de l'injection.

**4. Délégation utilisateur ; les credentials ne traversent jamais.** L'adapter s'exécute **strictement avec l'identité déléguée de l'utilisateur** (OAuth, ADR-011), **jamais** avec un privilège serveur plus large (défense *confused deputy*). Les credentials INPI restent **côté serveur**, déchiffrés en mémoire le temps de la requête — l'agent ne les voit jamais. `Atlas.Mcp` est un **OAuth Resource Server** ; OpenIddict est le serveur d'autorisation ; on **valide l'audience** du jeton (pas de *token passthrough*).

**Transverse** : nos propres **descriptions d'outils sont versionnées et signées** (défense *rug pull* / poisoning de nos outils) ; **chaque appel d'outil est audité** (qui, quel outil, quels paramètres, quel résultat).

**Placement hexagonal** : `Atlas.Mcp` est un **adapter entrant** (primaire), parallèle à `Atlas.Api` et `Atlas.Maui`, appelant les **mêmes use cases MediatR** (zéro changement domaine, cf. F-052). La **curation (allowlist), le mapping des scopes et la sérialisation préservant la doctrine** vivent dans l'adapter.

### Croquis (illustratif)

```csharp
// Atlas.Mcp — adapter entrant. Surface CURÉE (allowlist explicite, pas d'exposition 1:1).
[McpServerTool(Name = "get_company_dossier")]
[Description("Retourne un dossier DESCRIPTIF d'une entreprise (faits sourcés et datés). " +
             "Les correspondances sont des CANDIDATS À VÉRIFIER, jamais des faits établis. " +
             "Une section peut être indisponible / restreinte / sans objet : " +
             "ne pas l'interpréter comme « rien à signaler ».")]
public async Task<CompanyDossierToolResult> GetCompanyDossierAsync(
    [Description("SIREN à 9 chiffres")] string siren,   // schéma d'entrée étroit, typé
    CancellationToken ct)
{
    // S'exécute avec l'identité DÉLÉGUÉE de l'utilisateur (OAuth). Credentials INPI : côté serveur, jamais exposés.
    var dossier = await _mediator.Send(new GetCompanyDossierQuery(siren), ct);
    // Le mapping PRÉSERVE SectionState, AsOf, Provenance et les MatchCandidate (« à vérifier ») inline.
    return Map(dossier);
}
// PAS d'outil omnibus. Les écritures sont des outils séparés, sous scope explicite + approbation humaine.
```

## Rationale

- **MCP = interop, pas sécurité** → la sûreté est notre travail, et elle **scale avec le privilège** : d'où la surface étroite/lecture-d'abord comme stratégie première.
- **Le travail sur les types paie une seconde fois** : la doctrine encodée dans `MatchCandidate`/`SectionState` (ADR-014/015) est **héritée au fil** — il suffit de ne pas la jeter dans le mapping.
- **La doctrine descriptive et la sécurité s'alignent** : un outil qui *montre des faits* est intrinsèquement moins dangereux qu'un outil qui *agit*. Le positionnement produit **est** une posture de sécurité.
- **Moindre privilège + identité déléguée** neutralisent le *confused deputy* ; **surface d'écriture étroite** confine l'injection indirecte.

## Conséquences

- **Positives** : couche de sécurité fine, rayon d'explosion minimal, doctrine qui survit au passage à l'agent, cohérence avec le positionnement produit, aboutissement logique des ADR-012/014/015.
- **Négatives** : **discipline de curation** — chaque nouvel outil doit être ajouté délibérément (schéma étroit + description porteuse de doctrine), jamais auto-exposé ; les écritures coûtent une UX d'approbation.
- **À prévoir** :
  - Nommer les **scopes** et la **taxonomie d'outils** (doc 08) ; **audit** des appels d'outils.
  - **Versionner/signer** nos descriptions d'outils ; surveiller l'évolution du spec MCP (révision attendue).
  - UX d'**approbation humaine** pour les écritures.
  - Références croisées : **F-052** (feature), **ADR-011** (auth), **ADR-012/014/015** (doctrine héritée).
