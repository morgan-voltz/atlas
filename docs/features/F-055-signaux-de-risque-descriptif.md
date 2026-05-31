# F-055 — Signaux de risque (descriptif)

**Description** : Atlas regroupe, pour une entreprise, des **signaux de risque factuels** dans une vue cohérente — procédures collectives (BODACC, F-048), correspondances avec une **liste de sanctions / gel des avoirs** officielle, et **mentions presse** (timeline F-047). Strictement **descriptif** : Atlas **affiche des faits** (« en redressement judiciaire selon BODACC », « correspondance potentielle sur la liste de gel DG Trésor — à vérifier », « N articles la mentionnent »). Atlas **n'attribue aucun score de risque** et **ne qualifie jamais** une entreprise de « à risque ». L'utilisateur évalue lui-même.

**Valeur user** : pour la due diligence et la compliance légère (personas **Compliance / KYC** et **Investisseur / M&A**) — rassembler en un endroit les signaux qu'il faut aujourd'hui aller chercher dans cinq sources. Surface d'alerte, pas verdict.

**Constat qui définit le périmètre** : l'essentiel des signaux est **déjà dans le produit**. Procédures collectives = BODACC (F-048), mentions presse = timeline (F-047, volet 1). Cette feature n'est **pas un nouveau moteur** — c'est **assembler l'existant + ajouter une seule source neuve** : le screening sanctions officiel.

**Complexité** : ★★★ — assemblage (BODACC + presse déjà là) + ingestion d'une source sanctions + matching conservateur.

**APIs externes** :
- **Registre national des gels des avoirs — DG Trésor** : liste officielle des personnes/entités sanctionnées (ONU + UE + national), en **fichiers interopérables + API, mise à jour quotidienne**. Gratuit, officiel, souverain.
- **Liste consolidée des sanctions financières de l'UE**.
- **BODACC** (déjà intégré, F-048) et **presse** (veille existante, F-047).
- **Écarté** : OpenSanctions (gratuit en non-commercial seulement → licence requise en usage business). C'est aussi pourquoi le **PEP** reste hors cœur gratuit.

**Dépendances** : F-048 (BODACC — procédures collectives), F-047 (timeline + mentions presse), F-017 / F-053 (favoris & watchlists), **ADR-012** (doctrine descriptif + matching conservateur — gouverne cette fiche), ADR-004. Optionnel : F-031 Judilibre (contentieux, V3+).

**Hors périmètre (explicite)** :
- **Aucun score de risque**, aucun label « entreprise à risque », aucun verdict (ADR-012).
- **Aucun screening PEP** dans le cœur gratuit (données surtout licenciées).
- **Aucune base d'adverse media propriétaire** (World-Check, Dow Jones…).
- **Aucune certification de conformité AML** : Atlas *expose des signaux*, il ne certifie rien.
- Bénéficiaires effectifs exclus (CJUE Sovim).

**Détails techniques** :
- **Source sanctions (la seule vraie nouveauté)** : nouvel adapter (port `IExternalContentSource` ou `ISanctionsListProvider`) ingérant les listes DG Trésor + UE périodiquement, via un job Hangfire sur le modèle de `bodacc-polling`.
- **Matching conservateur** (principe d'exactitude, ADR-012 §5) : rapprocher sur nom + identifiants/date de naissance quand disponibles ; **ne jamais affirmer automatiquement** une correspondance ; afficher « correspondance potentielle, à vérifier ». Une fausse correspondance sanctions est **diffamatoire et grave**.
- **Assemblage (réutilisation pure)** : vue « signaux de risque » combinant `FavoriteEvent` BODACC (procédures collectives), correspondances sanctions, `FeedItemFavoriteMatch` (mentions presse). Possibilité de créer des `FavoriteEvent` de type « signal de risque » pour la timeline (F-047). **Aucun nouveau moteur de veille.**

**Cadre légal & positionnement** : **gouverné par ADR-012** — descriptif, matching conservateur, jamais de verdict. Les listes de sanctions sont de l'**open data officiel** (réutilisation libre). Atlas **expose** des signaux ; il ne certifie aucune conformité AML et ne qualifie aucune entité. Le matching conservateur protège l'exactitude (RGPD art. 5.1.d) et contre la diffamation.

**Accessibilité (rappel ADR-008)** :
- Un signal n'est **jamais** transmis par la couleur seule (pas de « rouge = risque ») : icône + libellé explicite.
- Signaux en liste/tableau lisibles au lecteur d'écran ; formulation « à vérifier » explicite.

**Modèle économique** : déterministe, sources gratuites → **cœur open source** (ADR-006). Le PEP (données licenciées) serait, le cas échéant, une option premium ou hors périmètre.

**Découpage / jalons** :
1. **Source sanctions** : ingestion DG Trésor + UE, matching conservateur, affichage « correspondance potentielle ». *(Seule vraie nouveauté.)*
2. **Vue assemblée** : « signaux de risque » réunissant BODACC + sanctions + presse.
3. **Alertes / timeline** (optionnel) : événements « signal de risque ».
4. **Contentieux** (futur) : Judilibre (F-031).

**Décisions ouvertes** :
- **Contentieux maintenant ou plus tard** (dépend de F-031 Judilibre).
- **Vue dédiée vs enrichissement de la fiche** existante.
- **Seuils d'alerte** sur signaux.
- **PEP** : hors périmètre tant qu'il n'y a pas de source gratuite exploitable.

---

**Grappe 4 — Outillage PI avancé** *(F-025, F-026 — killer feature pour les cabinets de propriété industrielle)*
