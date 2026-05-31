# F-062 — Synchronisation des préférences utilisateur (multi-surface)

> **Statut** : ⬜ Spécifiée, **au périmètre V2 / MVP 2** (à implémenter). Comble une dette implicite : réalise une promesse déjà faite par ADR-001.
> **Date** : 30 mai 2026 (spec) ; classée V2 le 31 mai 2026.
> **Catégorie MoSCoW** : Should have (le principe est déjà invoqué partout ; seule la réalisation manque).
> **Origine** : ADR-001 **promet** la synchronisation multi-device (« un user qui utilise l'app sur desktop ET mobile doit retrouver ses données ») mais ne la spécifie pas ; doc 12 et doc 16 écrivent « persiste multi-device (ADR-001) » à chaque préférence — **sans feature qui le réalise**. Cette fiche comble ce trou.
> **Dépendances** : F-001 (compte/auth), patron de stockage utilisateur (cascade FK RGPD), `docs/12-modele-ux-client-maui.md` (§8 densité, §9 Profil/Affichage & données), `docs/16-polices-et-lisibilite.md` (police, espacement, taille), `themes.json` (thèmes), ADR-001 (multi-device), ADR-002 (clients purs de l'API).
> **Documents liés** : `02-roadmap-features.md`, `04-securite-rgpd.md`, `docs/14-modele-ux-client-web.md`.

---

## Description

Centraliser les **préférences utilisateur** côté serveur pour qu'elles **suivent l'utilisateur d'une surface à l'autre** (web, mobile, desktop) sans qu'il ait à les redéfinir. C'est la **réalisation concrète** de la synchronisation multi-device promise par ADR-001, appliquée aux préférences d'affichage et d'accessibilité.

Principe directeur, hérité de toute la doctrine : **distinguer ce qui est « qui je suis » (suit l'utilisateur) de ce qui est « où je suis » (propre à l'appareil).** Toutes les préférences ne doivent pas être synchronisées de force — certaines dépendent légitimement de l'appareil.

## Valeur user

Un utilisateur qui a réglé sa **police accessible**, son **gabarit de fiche** ou ses **sections masquées** ne veut pas refaire ce travail sur chaque appareil. Pour un utilisateur en situation de handicap (police/espacement/accessibilité), c'est même **essentiel** : ses réglages d'accessibilité doivent le suivre partout, immédiatement. À l'inverse, forcer la synchro du **thème** ou de la **densité** casserait leur raison d'être (voir périmètre).

## Périmètre — deux familles de préférences

### Préférences de **compte** (synchronisées — « qui je suis »)
Suivent l'utilisateur sur toutes ses surfaces :
- **Police** de corps, **espacement**, **taille** du texte (doc 16).
- **Réglages d'accessibilité** (doc 06 / doc 16).
- **Gabarit de fiche** par défaut, **sections masquées / épinglées** (doc 12 §4).
- **Langue**.

### Préférences d'**appareil** (locales — « où je suis »)
Ne se synchronisent **pas** ; chaque appareil garde les siennes (avec un défaut héritable, cf. évolution) :
- **Thème** clair/sombre — légitimement différent selon le contexte (sombre le soir sur mobile, clair au bureau).
- **Densité** — **conçue exprès** pour varier selon l'écran (compact sur desktop large, aéré sur mobile au pouce — doc 12 §8). La synchroniser annulerait son intérêt.

### Hors périmètre v1
- **Notifications** : configuration de **canaux** (push lié à l'installation, email…), par nature largement **par appareil** ; sous-système distinct, traité séparément.
- Les **données** métier (favoris, watchlists, historique) — déjà gérées par leurs features respectives (F-017/F-053…), ce n'est pas l'objet ici.

| Préférence | Famille | Synchronisée |
|---|---|---|
| Police, espacement, taille | Compte | ✅ |
| Accessibilité | Compte | ✅ |
| Gabarit de fiche, sections masquées/épinglées | Compte | ✅ |
| Langue | Compte | ✅ |
| Thème (clair/sombre) | Appareil | ❌ (défaut héritable) |
| Densité | Appareil | ❌ (défaut héritable) |
| Notifications | Appareil | Hors périmètre v1 |

## Résolution de conflit — last-write-wins **par clé**

Deux appareils peuvent modifier des préférences différentes (voire hors-ligne, cas réel en mobilité MAUI). Stratégie retenue :
- **Chaque préférence (clé) porte son propre horodatage** (`updatedAt`).
- À la synchro, **la version la plus récente de *chaque clé*** gagne — pas le bloc entier.
- Conséquence : modifier le thème sur mobile **n'écrase pas** la densité réglée sur desktop. Le seul vrai conflit (même clé, deux modifications concurrentes hors-ligne) se tranche par « plus récent gagne », perte minimale.

*Pourquoi pas le last-write-wins global* : trop grossier, ferait perdre des réglages non touchés. *Pourquoi pas « serveur fait foi » strict* : suppose qu'on ignore l'offline, or MAUI sera parfois hors-ligne.

## Complexité : ★★ (quelques jours)

Faible : un store clé-valeur horodaté côté serveur + endpoints + un client de préférences par surface. Pas de nouvelle source externe, pas de job. Le seul soin réel est la **discipline compte/appareil** et le **merge par clé**.

## Détails techniques

- **Entité** `UserPreference` (`UserId`, `Key`, `Value` (JSON/scalaire), `UpdatedAt`), index unique `(UserId, Key)`, cascade FK compte (RGPD). Les préférences d'**appareil** ne sont **pas** stockées ici (elles restent en stockage local de l'app — SecureStorage/localStorage selon la surface).
- **Endpoints** : `GET /me/preferences` (lecture), `PUT /me/preferences` (upsert par clé, avec `updatedAt`), éventuellement `PATCH` partiel. Sous l'auth existante (F-001).
- **Clients (ADR-002)** : web et MAUI consomment l'API comme tout le reste (clients purs) ; au login, on **récupère** les préférences de compte et on les applique ; à chaque changement, on **pousse** la clé modifiée.
- **Découplage rôle/valeur** : les clés correspondent aux tokens déjà définis (police, espacement, gabarit…) — pas de nouveau modèle de présentation, juste leur **persistance**.
- **Application immédiate** : un changement de préférence de compte se reflète à la prochaine synchro/au prochain login des autres surfaces.

## Cadre RGPD

Les préférences sont des **données utilisateur** : cascade FK compte, **incluses dans l'export** (art. 20), **supprimées** à la clôture du compte (art. 17). Aucune donnée sensible ; les réglages d'accessibilité peuvent toutefois être **révélateurs** (handicap) → à traiter avec la même confidentialité que le reste du compte, jamais exposés à des tiers.

## Accessibilité (enjeu direct)

C'est la feature qui **garantit que les réglages d'accessibilité suivent l'utilisateur** — donc elle *sert* l'accessibilité plus qu'elle n'en dépend. Un utilisateur qui configure sa police/espacement sur une surface les retrouve partout, sans refaire le travail. À ce titre, les préférences de compte liées à l'accessibilité ont la **priorité** d'application (dès le login, avant tout rendu de contenu dense).

## Découpage / jalons

1. **Store + endpoints** : `UserPreference` horodaté, `GET`/`PUT`, merge par clé.
2. **Client MAUI** : application au login + push à chaque changement ; gestion offline (file d'attente de sync).
3. **Client web** : idem (doc 14).
4. **(Évolution)** **défaut héritable** pour les préférences d'appareil : le compte porte une valeur « suggérée » (thème/densité), chaque appareil peut la suivre **ou** la surcharger localement — pour l'utilisateur qui préfère « régler une fois, partout ».

## Décisions ouvertes

- **Granularité des clés** : une clé par préférence, ou regroupement par bloc (perf vs finesse du merge) ?
- **Défaut héritable** (jalon 4) : v1 ou évolution ? (Tranché « évolution » par défaut, à confirmer.)
- **Notifications** : confirmer qu'elles sont bien hors périmètre v1 et traitées en sous-système par-appareil.
- **Offline MAUI** : politique de file d'attente et de re-sync au retour en ligne.

## À faire à l'intégration

- Ajouter **F-062** au catalogue `02-roadmap-features.md` (Should have).
- ✅ Fait (31 mai 2026) : les mentions « persiste multi-device (ADR-001) » de **doc 12** renvoient désormais à **F-062** (propriétaire de la *réalisation*), en précisant la famille (compte vs appareil — la densité est explicitée comme préférence d'appareil).
- Vérifier la cohérence avec `themes.json` : le **thème** est désormais explicitement une préférence **d'appareil** (non synchronisée par défaut) — à noter là où la persistance du thème était sous-entendue.
- Envisager un **ADR court** si la stratégie de sync (last-write-wins par clé + compte/appareil) doit être tracée comme décision structurante.

---

*Fiche figée le 30 mai 2026. Réalise la synchronisation multi-device promise par ADR-001. Principe : distinguer préférences de compte (suivent l'utilisateur — police, accessibilité, gabarits, langue) et préférences d'appareil (locales — thème, densité). Résolution : last-write-wins par clé horodatée. Comble une dette implicite invoquée dans doc 12 (et la lisibilité de doc 16) sans jamais avoir été spécifiée.*
