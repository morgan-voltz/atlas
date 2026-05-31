# F-027 — Veille PI automatisée (règles utilisateur)

> **Reformulée 29 mai 2026** — précision du périmètre pour la distinguer de F-046 (cluster veille MVP 2). **F-046 = infrastructure générique** de règles de veille (filtres sur flux, mots-clés, secteurs, types d'annonces). **F-027 = application spécifiquement PI** : règles ciblées sur les nouveaux dépôts marques / brevets (concurrents nommés, classes de Nice, mots-clés). F-027 réutilise F-046 quand la sémantique tient, ou ajoute des règles dédiées PI au-dessus.

**Description** : l'utilisateur configure des règles ciblées PI — « alerte-moi à chaque nouveau dépôt de marque par mon concurrent X », « contenant le mot Y dans la classe Z ». Notifications email / push (F-019 / F-020).

**Valeur user** : surveillance concurrentielle PI proactive, impossible à faire manuellement à grande échelle. Couvre le besoin du pack « Veille concurrentielle B2B » (doc 07).

**Complexité** : ★★★★

**APIs externes** : INPI PI (+ EUIPO / OMPI quand F-039 ouvert en V3+).

**Dépendances** : F-018 (favoris marque / brevet), F-020 (push), F-046 (couche infrastructure des règles user — réutilisation).

> **Architecture liée** : à l'implémentation, F-027 et F-032 émettront des `FavoriteEvent` typés (`IpFiled`, `PublicContractAwarded`) dans la timeline F-047 — c'est ce qui alimente la **taxonomie de signaux de F-058**.
