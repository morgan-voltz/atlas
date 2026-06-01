# F-075 — Moteur de fraîcheur & synchronisation du cache offline

> **Statut** : 📋 Spécifiée — non implémentée (cluster Offline, 31 mai 2026). La **machinerie** (comment le cache reste honnête) ; consomme/alimente **F-029**. Cadrée par **ADR-027**. **Descendant seul** (pas d'écriture hors-ligne en v1).

**Description** : la couche qui **alimente, rafraîchit et borne** le cache de F-029 : détecter le **retour de connexion**, **re-récupérer** les ressources cachées et **mettre à jour leur date**, suivre la **fraîcheur par entité**, et **évincer** par l'espace sans jamais masquer pour cause d'âge.

**Valeur user** : invisible mais structurante — c'est ce qui garantit que « vu le {date} » est **exact** et que le retour en ligne **rafraîchit** sans intervention. Sans elle, le cache de F-029 mentirait vite sur sa fraîcheur.

**Complexité** : ★★★★ (détection réseau + stratégie de rafraîchissement + éviction + résilience — le morceau dur de la piste).

**APIs externes** : aucune (re-consomme les endpoints de lecture existants).

**Dépendances** : **F-029** (le store qu'il alimente), **ADR-027**, ADR-013 (fraîcheur snapshot — source du « à jour » en ligne), Polly (acté, résilience).

**Détails techniques** :
- **Détection de connectivité** (MAUI `Connectivity`) : bascule **online ↔ offline** ; au **retour en ligne**, déclenche le rafraîchissement.
- **Stratégie de peuplement** : on **cache à la consultation** (ouvrir un dossier le met en cache) et on **garde les favoris** ; pas de pré-chargement massif (cache minimal v1, ADR-027).
- **Rafraîchissement au retour réseau** : re-`fetch` des ressources cachées → **met à jour `FetchedAt`** ; l'item repasse de `Stale` à `Available`. **Résilience Polly** (retry/backoff) sur les re-fetch.
- **Suivi de fraîcheur par entité** : chaque `CachedResource` porte sa propre `FetchedAt` (pas un horodatage global) — l'âge affiché par F-029 est **par-item**, jamais approximé.
- **Éviction = espace, pas fraîcheur (ADR-027 §5)** : **LRU sur les dossiers** (borne de taille), **favoris toujours gardés** ; on n'évince **jamais** parce qu'une donnée est « trop vieille » (ce serait un vide menteur). Ne **jamais** évincer ce que l'utilisateur croit épinglé.
- **Pas de file d'écriture en v1** : le moteur est **descendant seul** (serveur → cache). L'écriture hors-ligne (file montante + merge par clé façon F-062) est un **incrément ultérieur**.
- **Placement (ADR-002)** : entièrement dans `Atlas.Maui` (Storage/Services), derrière les ViewModels ; aucune logique métier ne descend dans le client.
- **Accessibilité (ADR-008)** : le passage offline→online et le rafraîchissement sont **annoncés** discrètement (pas de spam du lecteur d'écran).
- **Hors-scope** : le **rendu** des états (F-029) ; l'**écriture hors-ligne** ; le offline **web** (IndexedDB/service worker) et **desktop Avalonia** — mécanismes distincts, avenants futurs.
