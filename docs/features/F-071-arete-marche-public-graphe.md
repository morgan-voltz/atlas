# F-071 — Arête « marché public » du graphe (acheteur ↔ titulaire)

> **Statut** : 📋 Spécifiée — non implémentée (cluster Graphe d'écosystème, 31 mai 2026). **Fondation faible risque** ; **quasi gratuite** (F-032 ingère déjà la donnée). Cadrée par **ADR-024**, sur le substrat **F-034**.

**Description** : ajouter au graphe d'écosystème une **arête « marché public »** reliant une entreprise à l'**acheteur public** dont elle est **titulaire** (DECP), exposée dans la traversée bornée autour d'une entité focus. Propre par SIREN, descriptive.

**Valeur user** : révèle l'**activité réelle relationnelle** — quelles entreprises servent quels acheteurs publics (et, avec prudence hub, qui sert les mêmes). Personas **M&A** et **veille concurrentielle**. Réutilise une donnée **déjà en base** : coût marginal minime.

**Complexité** : ★★ (la donnée *et* le mapping SIRET→SIREN existent dans F-032 ; l'essentiel est de l'**exposer comme arête** + la conscience des hubs).

**APIs externes** : — (consomme F-032 / DECP, déjà ingéré).

**Dépendances** : **F-034** (substrat graphe), **F-032** (DECP, titulaire SIRET→SIREN), **ADR-024**, ADR-008.

**Détails techniques** :
- Nouvelle **arête typée `PublicContract`** (Entreprise → acheteur), dérivée des marchés de F-032 (le **SIRET→SIREN du titulaire est déjà fait**).
- Exposée dans la **traversée bornée `WITH RECURSIVE`** de F-034, autour de l'entité focus (1-2 sauts, cache).
- **Hub-aware (ADR-024 §5)** : un acheteur public majeur est un nœud à **très haut degré** → on montre l'**arête directe** (société ↔ acheteur X), on **n'infère pas** un lien société↔société transitif via un gros acheteur. Seuil de degré appliqué.
- **Qualification descriptive** : « titulaire d'un marché de {acheteur} ({année}) » — **jamais** « fournisseur attitré ». Provenance DECP + date.
- **Légalement léger (ADR-024 §3)** : entités et contrats publics, **pas de DPIA**.
- **Accessibilité (ADR-008)** : équivalent **tabulaire** (« Entreprise A — titulaire de marchés des acheteurs X, Y »), aucune information par la couleur ou la position seule.
- **Caveats hérités de F-032** : seuil **> 40 000 € HT**, couverture **partielle** (« aucun marché public connu » proprement, pas un vide ambigu).
- **Hors-scope** : l'arête **co-dépôts PI** (F-072) ; toute **inférence de dépendance économique** (verdict) ; l'arête adresse partagée.
