# Catalogue des flux RSS pour la veille du projet

> Inventaire des flux RSS pertinents pour le projet, organisé en **cercles concentriques de pertinence** — du cœur métier (INPI, PI) jusqu'aux veilles plus éloignées mais utiles (design, produit, business).
> Objectif : alimenter une routine de veille structurée pour rester à jour sur les sujets qui touchent le projet, sa stack technique, son écosystème et son marché.

> **Note stratégique importante (mise à jour mai 2026)** : suite à l'ADR-009, ce catalogue a un **double usage** :
> 1. **Veille perso** du porteur du projet (usage initial)
> 2. **Catalogue de templates de veille** intégré directement dans le produit en MVP 2 (cf. cluster F-041 à F-050 dans la doc 02)
>
> Les flux les plus pertinents par segment client (Cabinet PI, Expert-comptable, Compliance, Investisseur, etc.) constitueront les **Packs de veille pré-curés** proposés aux utilisateurs à l'inscription. La constitution de ces packs sera dérivée directement de ce document.

**Version** : 1.1
**Date de dernière mise à jour** : 26 mai 2026

---

## Sommaire

- [Préambule et méthodologie](#préambule-et-méthodologie)
- [Cercle 1 — Cœur : INPI, propriété industrielle, RNE](#cercle-1--cœur--inpi-propriété-industrielle-rne)
- [Cercle 2 — Données publiques entreprises (France)](#cercle-2--données-publiques-entreprises-france)
- [Cercle 3 — Veille juridique et réglementaire](#cercle-3--veille-juridique-et-réglementaire)
- [Cercle 4 — Veille dev .NET / MAUI / ASP.NET](#cercle-4--veille-dev-net--maui--aspnet)
- [Cercle 5 — Open data et numérique public](#cercle-5--open-data-et-numérique-public)
- [Cercle 6 — Sécurité, cybersécurité, RGPD](#cercle-6--sécurité-cybersécurité-rgpd)
- [Cercle 7 — Accessibilité numérique](#cercle-7--accessibilité-numérique)
- [Cercle 8 — SaaS, indie hacking, open source business](#cercle-8--saas-indie-hacking-open-source-business)
- [Cercle 9 — Tech général](#cercle-9--tech-général)
- [Cercle 10 — Économie, entreprises françaises, marché](#cercle-10--économie-entreprises-françaises-marché)
- [Cercle 11 — Design, UX, produit](#cercle-11--design-ux-produit)
- [Cercle 12 — Cercles annexes (hors sujet direct mais utiles)](#cercle-12--cercles-annexes-hors-sujet-direct-mais-utiles)
- [Comment consommer : lecteurs et workflow](#comment-consommer--lecteurs-et-workflow)
- [Flux intégrables dans le projet lui-même](#flux-intégrables-dans-le-projet-lui-même)
- [Bundle OPML à importer](#bundle-opml-à-importer)

---

## Préambule et méthodologie

### Pourquoi le RSS en 2026

Beaucoup de gens pensent que le RSS est mort. **C'est faux**. La plupart des médias sérieux, blogs tech, sites institutionnels exposent encore (ou à nouveau) leurs flux. Le RSS reste **la meilleure technologie de veille** pour 3 raisons :

1. **Pas d'algorithme** : tu vois tout, dans l'ordre chronologique, sans curation imposée.
2. **Pas de pub** : pas de tracking, pas de récolte de données personnelles.
3. **Centralisable** : un seul outil pour 100 sources.

### Comment ce catalogue est organisé

Le catalogue est structuré en **cercles concentriques de pertinence** :

- **Cercle 1-3** : indispensable. Suivre tous les jours / toutes les semaines.
- **Cercle 4-6** : très utile. Suivre régulièrement, lecture sélective.
- **Cercle 7-9** : utile en complément. Lecture occasionnelle.
- **Cercle 10-12** : exploration. Lecture quand le temps le permet.

### Pour chaque flux, on documente

- **Nom** de la source
- **URL du site** principal
- **URL du flux** RSS / Atom (quand connue ou patternable)
- **Fréquence** estimée des publications
- **Langue**
- **Pertinence** ⭐ à ⭐⭐⭐⭐⭐ pour le projet
- **Note** quand utile

### Avertissement

Les URLs de flux sont des **patterns canoniques** ou des liens connus. Certaines peuvent avoir bougé. Procédure de vérification :
1. Visiter l'URL du site
2. Chercher "RSS", "Flux", "Feed" dans le footer ou la page contact
3. Inspecter le HTML : `<link rel="alternate" type="application/rss+xml">` dans le `<head>`
4. Tester les patterns `/feed`, `/rss`, `/feed.xml`, `/atom.xml`, `/rss.xml`

---

## Cercle 1 — Cœur : INPI, propriété industrielle, RNE

**Importance** : ⭐⭐⭐⭐⭐ — c'est le cœur du projet. À suivre quotidiennement.

### 1.1 INPI — Actualités

| | |
|---|---|
| Site | https://www.inpi.fr/ |
| Flux | https://www.inpi.fr/rss.xml *(à vérifier)* |
| Fréquence | ~2-5 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Communiqués officiels, évolutions des API, changements de procédures |

### 1.2 BOPI — Bulletin Officiel de la Propriété Industrielle

| | |
|---|---|
| Site | https://www.inpi.fr/bopi |
| Flux | Pas de RSS officiel connu, mais publication hebdomadaire à scraper |
| Fréquence | Hebdomadaire (vendredi) |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Source principale des publications de marques, brevets, D&M français. Intégrable au projet comme source |

### 1.3 EUIPO News

| | |
|---|---|
| Site | https://www.euipo.europa.eu/ |
| Flux | https://www.euipo.europa.eu/en/news/rss *(à vérifier)* |
| Fréquence | ~1-2 par semaine |
| Langue | EN / multilingue |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Évolutions des marques européennes, oppositions, jurisprudence |

### 1.4 OMPI / WIPO News

| | |
|---|---|
| Site | https://www.wipo.int/ |
| Flux | https://www.wipo.int/feeds/news.xml |
| Fréquence | ~5-10 par mois |
| Langue | EN / FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Couverture internationale de la PI |

### 1.5 OEB (EPO) — Office Européen des Brevets

| | |
|---|---|
| Site | https://www.epo.org/ |
| Flux | https://www.epo.org/news-events/news.rss *(à vérifier)* |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Brevets européens, jurisprudence, évolutions techniques |

### 1.6 USPTO — Patent and Trademark Office (USA)

| | |
|---|---|
| Site | https://www.uspto.gov/ |
| Flux | https://www.uspto.gov/about-us/news-updates/feed |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Veille internationale, tendances US |

### 1.7 IPKat — Blog de référence en PI

| | |
|---|---|
| Site | https://ipkitten.blogspot.com/ |
| Flux | https://ipkitten.blogspot.com/feeds/posts/default |
| Fréquence | ~5-7 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | LE blog incontournable de la PI européenne. Décrypte la jurisprudence, les évolutions doctrinales |

### 1.8 Marques & Brevets — Blog français spécialisé

| | |
|---|---|
| Site | https://www.marques-brevets.fr/ *(à vérifier)* |
| Flux | À chercher sur le site |
| Fréquence | Variable |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Vulgarisation française sur les marques et brevets |

### 1.9 Cabinet Plasseraud — Actualités PI

| | |
|---|---|
| Site | https://www.plass.com/ |
| Flux | https://www.plass.com/feed/ *(à vérifier)* |
| Fréquence | ~3-5 par mois |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Un des plus grands cabinets PI français, articles de qualité |

### 1.10 Cabinet Beau de Loménie

| | |
|---|---|
| Site | https://www.bdl-ip.com/ |
| Flux | À chercher |
| Fréquence | Variable |
| Langue | FR / EN |
| Pertinence | ⭐⭐⭐ |
| Note | Autre référence cabinet PI français |

---

## Cercle 2 — Données publiques entreprises (France)

**Importance** : ⭐⭐⭐⭐ — sources de données complémentaires intégrables au projet.

### 2.1 data.gouv.fr — Actualités

| | |
|---|---|
| Site | https://www.data.gouv.fr/ |
| Flux | https://www.data.gouv.fr/fr/posts/recent.atom |
| Fréquence | ~2-5 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Annonces de nouveaux datasets, évolutions des APIs gouvernementales |

### 2.2 BODACC — Annonces légales

| | |
|---|---|
| Site | https://www.bodacc.fr/ |
| Flux | Via Opendatasoft : https://bodacc-datadila.opendatasoft.com/ — flux RSS par recherche personnalisable |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Possibilité de configurer des flux RSS personnalisés (par mots-clés, par tribunal). Intégrable au projet |

### 2.3 INSEE — Actualités et publications

| | |
|---|---|
| Site | https://www.insee.fr/ |
| Flux | https://www.insee.fr/fr/information/rss.xml |
| Fréquence | ~5-10 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Statistiques économiques, démographiques, sectorielles |

### 2.4 INSEE — Nouvelles publications (Insee Première, Analyses)

| | |
|---|---|
| Flux | https://www.insee.fr/fr/statistiques/rss/publications.xml *(à vérifier)* |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Études économiques, analyses sectorielles |

### 2.5 DGE — Direction Générale des Entreprises

| | |
|---|---|
| Site | https://www.entreprises.gouv.fr/ |
| Flux | À chercher dans le footer |
| Fréquence | Variable |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Politiques publiques entreprises, dispositifs d'aide |

### 2.6 Bpifrance — Le Lab

| | |
|---|---|
| Site | https://lelab.bpifrance.fr/ |
| Flux | https://lelab.bpifrance.fr/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Études sur le tissu entrepreneurial français |

### 2.7 Banque de France — Publications

| | |
|---|---|
| Site | https://www.banque-france.fr/ |
| Flux | https://www.banque-france.fr/rss/publications-economiques |
| Fréquence | Hebdomadaire |
| Langue | FR / EN |
| Pertinence | ⭐⭐⭐ |
| Note | Conjoncture, statistiques monétaires, études macroéconomiques |

### 2.8 DILA — Direction de l'Information Légale et Administrative

| | |
|---|---|
| Site | https://www.dila.premier-ministre.gouv.fr/ |
| Flux | À chercher |
| Fréquence | Variable |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Éditeur du BODACC, BOAMP, JORF |

### 2.9 Service-Public.fr — Pro

| | |
|---|---|
| Site | https://entreprendre.service-public.fr/ |
| Flux | https://entreprendre.service-public.fr/rss-fil-actualite.xml *(à vérifier)* |
| Fréquence | ~3-5 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Actualités réglementaires affectant les entreprises |

---

## Cercle 3 — Veille juridique et réglementaire

**Importance** : ⭐⭐⭐⭐ — clé pour suivre les évolutions du cadre légal des données entreprises et PI.

### 3.1 Légifrance — Nouveautés textes

| | |
|---|---|
| Site | https://www.legifrance.gouv.fr/ |
| Flux | https://www.legifrance.gouv.fr/contenu/Media/Actualites/rss.xml *(à vérifier)* |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Lois, décrets, arrêtés publiés |

### 3.2 JORF — Journal Officiel

| | |
|---|---|
| Site | https://www.journal-officiel.gouv.fr/ |
| Flux | https://www.journal-officiel.gouv.fr/pages/donnees-en-ligne/ |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Publication officielle de référence. Intégrable au projet |

### 3.3 CNIL — Actualités

| | |
|---|---|
| Site | https://www.cnil.fr/ |
| Flux | https://www.cnil.fr/fr/rss.xml |
| Fréquence | ~3-5 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | RGPD, données personnelles, sanctions. Critique pour ta conformité |

### 3.4 Dalloz Actualité

| | |
|---|---|
| Site | https://www.dalloz-actualite.fr/ |
| Flux | https://www.dalloz-actualite.fr/rss.xml *(à vérifier)* |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Veille juridique généraliste de qualité |

### 3.5 Village de la Justice

| | |
|---|---|
| Site | https://www.village-justice.com/ |
| Flux | https://www.village-justice.com/articles/spip.php?page=backend |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Articles d'avocats et juristes sur de nombreux sujets dont PI et data |

### 3.6 Conseil d'État — Décisions

| | |
|---|---|
| Site | https://www.conseil-etat.fr/ |
| Flux | https://www.conseil-etat.fr/ressources/decisions-contentieuses/derniere-actualite-jurisprudentielle/feed *(à vérifier)* |
| Fréquence | Hebdomadaire |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Décisions impactant le droit public, dont droit numérique |

### 3.7 Cour de cassation — Communiqués

| | |
|---|---|
| Site | https://www.courdecassation.fr/ |
| Flux | À chercher |
| Fréquence | Variable |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Communiqués sur les décisions importantes (dont PI) |

### 3.8 EDPB — European Data Protection Board

| | |
|---|---|
| Site | https://www.edpb.europa.eu/ |
| Flux | https://www.edpb.europa.eu/news/all-news_en.rss *(à vérifier)* |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Décisions et guidelines européennes en matière de protection des données |

### 3.9 EDPS — European Data Protection Supervisor

| | |
|---|---|
| Site | https://edps.europa.eu/ |
| Flux | À chercher |
| Fréquence | ~1-2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Veille européenne sur la protection des données |

---

## Cercle 4 — Veille dev .NET / MAUI / ASP.NET

**Importance** : ⭐⭐⭐⭐ — stack technique du projet.

### 4.1 .NET Blog (Microsoft)

| | |
|---|---|
| Site | https://devblogs.microsoft.com/dotnet/ |
| Flux | https://devblogs.microsoft.com/dotnet/feed/ |
| Fréquence | ~5-7 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Blog officiel Microsoft sur .NET. Annonces majeures, tutoriels, bonnes pratiques |

### 4.2 ASP.NET Blog

| | |
|---|---|
| Site | https://devblogs.microsoft.com/aspnet/ |
| Flux | https://devblogs.microsoft.com/aspnet/feed/ |
| Fréquence | ~3-5 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Blog officiel ASP.NET Core |

### 4.3 .NET MAUI Blog

| | |
|---|---|
| Site | https://devblogs.microsoft.com/dotnet/category/maui/ |
| Flux | https://devblogs.microsoft.com/dotnet/category/maui/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Suivi spécifique MAUI |

### 4.4 .NET Foundation Blog

| | |
|---|---|
| Site | https://dotnetfoundation.org/blog |
| Flux | https://dotnetfoundation.org/blog/rss.xml *(à vérifier)* |
| Fréquence | Variable |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Annonces sur l'écosystème open source .NET |

### 4.5 Scott Hanselman

| | |
|---|---|
| Site | https://www.hanselman.com/blog/ |
| Flux | https://www.hanselman.com/blog/feed.aspx |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Personnalité influente de l'écosystème .NET, contenus pédagogiques |

### 4.6 Andrew Lock — .NET Escapades

| | |
|---|---|
| Site | https://andrewlock.net/ |
| Flux | https://andrewlock.net/rss.xml |
| Fréquence | Hebdomadaire |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Articles techniques très approfondis sur ASP.NET Core |

### 4.7 Steve Smith (Ardalis)

| | |
|---|---|
| Site | https://ardalis.com/ |
| Flux | https://ardalis.com/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Expert Clean Architecture .NET — particulièrement aligné avec l'archi hexagonale |

### 4.8 The Morning Brew (Chris Alcock)

| | |
|---|---|
| Site | https://blog.cwa.me.uk/ |
| Flux | https://feeds.feedburner.com/ReflectivePerspective |
| Fréquence | Quotidienne (jours ouvrés) |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Curation quotidienne d'articles .NET. Excellent pour ne rien rater |

### 4.9 dotnet Weekly

| | |
|---|---|
| Site | https://dotnetweekly.com/ |
| Flux | https://dotnetweekly.com/feed |
| Fréquence | Hebdomadaire |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Newsletter hebdomadaire en flux RSS |

### 4.10 Maarten Balliauw

| | |
|---|---|
| Site | https://blog.maartenballiauw.be/ |
| Flux | https://blog.maartenballiauw.be/feed.xml |
| Fréquence | ~1-2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Expert .NET, sécurité, performance |

### 4.11 Khalid Abuhakmeh

| | |
|---|---|
| Site | https://khalidabuhakmeh.com/ |
| Flux | https://khalidabuhakmeh.com/feed.xml |
| Fréquence | Hebdomadaire |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Tips and tricks .NET, EF Core, Rider |

### 4.12 JetBrains .NET Blog

| | |
|---|---|
| Site | https://blog.jetbrains.com/dotnet/ |
| Flux | https://blog.jetbrains.com/dotnet/feed/ |
| Fréquence | Hebdomadaire |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Si tu utilises Rider, suivi des évolutions |

### 4.13 Code Maze

| | |
|---|---|
| Site | https://code-maze.com/ |
| Flux | https://code-maze.com/feed/ |
| Fréquence | ~3-5 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Tutoriels .NET pratiques |

### 4.14 Milan Jovanović

| | |
|---|---|
| Site | https://www.milanjovanovic.tech/ |
| Flux | https://www.milanjovanovic.tech/rss/feed.xml |
| Fréquence | Hebdomadaire |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Architecture .NET, Clean Architecture, DDD. Très aligné avec ce projet |

### 4.15 Nick Chapsas — YouTube channel

| | |
|---|---|
| Site | https://www.youtube.com/@nickchapsas |
| Flux YouTube | https://www.youtube.com/feeds/videos.xml?channel_id=UCrkPsvLGln62OMZRO6K-llg |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Excellent contenu vidéo .NET. YouTube expose des flux RSS standard |

---

## Cercle 5 — Open data et numérique public

**Importance** : ⭐⭐⭐ — élargir l'horizon, identifier de nouvelles sources de données pour le projet.

### 5.1 Etalab

| | |
|---|---|
| Site | https://www.etalab.gouv.fr/ |
| Flux | https://www.etalab.gouv.fr/feed |
| Fréquence | ~3-5 par mois |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Mission de la DINUM dédiée à l'open data français |

### 5.2 DINUM

| | |
|---|---|
| Site | https://www.numerique.gouv.fr/ |
| Flux | https://www.numerique.gouv.fr/rss/ |
| Fréquence | ~2-3 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Direction du numérique de l'État |

### 5.3 beta.gouv.fr — Blog

| | |
|---|---|
| Site | https://beta.gouv.fr/ |
| Flux | https://beta.gouv.fr/rss-articles.xml |
| Fréquence | ~5-10 par mois |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Startups d'État, retours d'expérience, méthodes |

### 5.4 European Data Portal

| | |
|---|---|
| Site | https://data.europa.eu/ |
| Flux | https://data.europa.eu/en/news/feed |
| Fréquence | ~5 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Open data européen, datasets nouveaux |

### 5.5 Open Knowledge Foundation

| | |
|---|---|
| Site | https://blog.okfn.org/ |
| Flux | https://blog.okfn.org/feed/ |
| Fréquence | ~5 par mois |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | ONG internationale promouvant l'open data |

### 5.6 NextINpact / NextImpact (anciennement)

| | |
|---|---|
| Site | https://next.ink/ |
| Flux | https://next.ink/feed/ |
| Fréquence | ~5-10 par jour |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Couverture des politiques numériques françaises et européennes |

### 5.7 La Quadrature du Net

| | |
|---|---|
| Site | https://www.laquadrature.net/ |
| Flux | https://www.laquadrature.net/feed/ |
| Fréquence | ~3-5 par mois |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Association de défense des libertés numériques |

---

## Cercle 6 — Sécurité, cybersécurité, RGPD

**Importance** : ⭐⭐⭐⭐ — critique vu que tu stockes des credentials INPI.

### 6.1 ANSSI — Bulletins d'actualité et CERT-FR

| | |
|---|---|
| Site | https://www.cert.ssi.gouv.fr/ |
| Flux | https://www.cert.ssi.gouv.fr/feed/ |
| Fréquence | ~5-10 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐⭐ |
| Note | Alertes de sécurité officielles françaises. Indispensable |

### 6.2 ANSSI — Actualités

| | |
|---|---|
| Site | https://www.ssi.gouv.fr/ |
| Flux | https://www.ssi.gouv.fr/feed/actualite/ |
| Fréquence | ~2-3 par semaine |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Recommandations et publications de l'agence cybersécurité |

### 6.3 CISA (USA)

| | |
|---|---|
| Site | https://www.cisa.gov/ |
| Flux | https://www.cisa.gov/news.xml |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Alertes US, vulnérabilités critiques |

### 6.4 KrebsOnSecurity

| | |
|---|---|
| Site | https://krebsonsecurity.com/ |
| Flux | https://krebsonsecurity.com/feed/ |
| Fréquence | ~3-5 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Journalisme d'investigation cybersécurité par Brian Krebs |

### 6.5 The Hacker News

| | |
|---|---|
| Site | https://thehackernews.com/ |
| Flux | https://feeds.feedburner.com/TheHackersNews |
| Fréquence | ~5-10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Couverture quotidienne cyber. Attention volume élevé |

### 6.6 BleepingComputer

| | |
|---|---|
| Site | https://www.bleepingcomputer.com/ |
| Flux | https://www.bleepingcomputer.com/feed/ |
| Fréquence | ~5-10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Vulnérabilités, ransomware, fuites |

### 6.7 Schneier on Security

| | |
|---|---|
| Site | https://www.schneier.com/ |
| Flux | https://www.schneier.com/feed/atom/ |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Bruce Schneier, autorité historique de la cryptographie. Réflexions profondes |

### 6.8 OWASP — Blog

| | |
|---|---|
| Site | https://owasp.org/blog/ |
| Flux | https://owasp.org/blog/feed.xml |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Référence sécurité web, Top 10, ASVS |

### 6.9 IAPP — Daily Dashboard (RGPD / privacy)

| | |
|---|---|
| Site | https://iapp.org/news/ |
| Flux | https://iapp.org/news/atom |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | International Association of Privacy Professionals. Couverture RGPD mondiale |

### 6.10 EUR-Lex — Veille législative UE

| | |
|---|---|
| Site | https://eur-lex.europa.eu/ |
| Flux | À configurer via recherche personnalisée |
| Fréquence | Quotidienne |
| Langue | Multilingue |
| Pertinence | ⭐⭐ |
| Note | Suivi des nouveaux textes européens |

---

## Cercle 7 — Accessibilité numérique

**Importance** : ⭐⭐⭐⭐ — vu l'engagement accessibilité du projet (cf. doc 06).

### 7.1 W3C WAI — News

| | |
|---|---|
| Site | https://www.w3.org/WAI/ |
| Flux | https://www.w3.org/WAI/feed.xml *(à vérifier)* |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Source officielle WCAG |

### 7.2 WebAIM Blog

| | |
|---|---|
| Site | https://webaim.org/blog/ |
| Flux | https://webaim.org/blog/feed |
| Fréquence | ~1-2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Référence pratique de l'accessibilité web |

### 7.3 A11y Project

| | |
|---|---|
| Site | https://www.a11yproject.com/ |
| Flux | https://www.a11yproject.com/feed.xml |
| Fréquence | ~2-4 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Articles pratiques, checklists, ressources |

### 7.4 Deque — Blog

| | |
|---|---|
| Site | https://www.deque.com/blog/ |
| Flux | https://www.deque.com/blog/feed/ |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Éditeur d'axe-core, articles techniques de qualité |

### 7.5 TPGi (The Paciello Group)

| | |
|---|---|
| Site | https://www.tpgi.com/news-insights/ |
| Flux | https://www.tpgi.com/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Cabinet de référence en accessibilité |

### 7.6 Tanaguru / Access42 — Blogs français

| | |
|---|---|
| Site | https://blog.access42.net/ |
| Flux | https://blog.access42.net/feed/ |
| Fréquence | ~1-2 par mois |
| Langue | FR |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Référence française de l'accessibilité, RGAA |

### 7.7 Smashing Magazine — Accessibility tag

| | |
|---|---|
| Site | https://www.smashingmagazine.com/category/accessibility/ |
| Flux | https://www.smashingmagazine.com/category/accessibility/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Articles approfondis, tutoriels |

---

## Cercle 8 — SaaS, indie hacking, open source business

**Importance** : ⭐⭐⭐ — pour réfléchir au modèle économique et à la trajectoire du projet.

### 8.1 Indie Hackers — Articles

| | |
|---|---|
| Site | https://www.indiehackers.com/ |
| Flux | https://www.indiehackers.com/posts.rss *(à vérifier)* |
| Fréquence | ~10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Communauté de devs entrepreneurs solo |

### 8.2 A Smart Bear — Jason Cohen

| | |
|---|---|
| Site | https://longform.asmartbear.com/ |
| Flux | https://longform.asmartbear.com/index.xml |
| Fréquence | ~2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Articles de fond sur SaaS, par le fondateur de WP Engine. Longue forme, qualité maximale |

### 8.3 SaaStr

| | |
|---|---|
| Site | https://www.saastr.com/ |
| Flux | https://www.saastr.com/feed/ |
| Fréquence | ~5-10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Référence mondiale SaaS B2B |

### 8.4 Sifted (startups européennes)

| | |
|---|---|
| Site | https://sifted.eu/ |
| Flux | https://sifted.eu/feed |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Couverture des startups européennes, par Financial Times |

### 8.5 Open Source Initiative

| | |
|---|---|
| Site | https://opensource.org/ |
| Flux | https://opensource.org/feed/ |
| Fréquence | ~2-3 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Référence sur l'open source et ses licences |

### 8.6 Open Source Business — Blog (Joseph Jacks, Heather Meeker, etc.)

| | |
|---|---|
| Site | Divers (à compiler) |
| Pertinence | ⭐⭐⭐ |
| Note | Modèles économiques open source, dual licensing |

### 8.7 OSS Capital — Blog

| | |
|---|---|
| Site | https://oss.capital/ |
| Flux | À chercher |
| Fréquence | ~1-2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Fond d'investissement spécialisé open source. Insights stratégiques |

### 8.8 Frenchweb

| | |
|---|---|
| Site | https://www.frenchweb.fr/ |
| Flux | https://www.frenchweb.fr/feed |
| Fréquence | ~5 par jour |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Actualités startups françaises |

---

## Cercle 9 — Tech général

**Importance** : ⭐⭐ — culture générale et veille élargie.

### 9.1 Hacker News (front page)

| | |
|---|---|
| Site | https://news.ycombinator.com/ |
| Flux | https://news.ycombinator.com/rss |
| Fréquence | ~30 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Communauté tech la plus influente. Veille incontournable mais volume très élevé |

### 9.2 Hacker News (Best — meilleur signal/bruit)

| | |
|---|---|
| Flux | https://hnrss.org/best |
| Fréquence | ~5-10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐⭐ |
| Note | Service tiers permettant de filtrer HN |

### 9.3 Lobste.rs

| | |
|---|---|
| Site | https://lobste.rs/ |
| Flux | https://lobste.rs/rss |
| Fréquence | ~20 par jour |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Alternative à HN, plus orienté contenus techniques |

### 9.4 Ars Technica

| | |
|---|---|
| Site | https://arstechnica.com/ |
| Flux | https://feeds.arstechnica.com/arstechnica/index |
| Fréquence | ~10-15 par jour |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Magazine tech de qualité |

### 9.5 The Register

| | |
|---|---|
| Site | https://www.theregister.com/ |
| Flux | https://www.theregister.com/headlines.atom |
| Fréquence | ~20 par jour |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Actualité tech / enterprise, ton british |

### 9.6 LinuxFR

| | |
|---|---|
| Site | https://linuxfr.org/ |
| Flux | https://linuxfr.org/news.atom |
| Fréquence | ~3-5 par jour |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Communauté libriste française |

### 9.7 GitHub Trending (via service tiers)

| | |
|---|---|
| Flux | https://mshibanami.github.io/GitHubTrendingRSS/ |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Suivi des repos qui montent. Filtrable par langage |

### 9.8 Reddit r/dotnet, r/csharp, r/programming

| | |
|---|---|
| Flux pattern | `https://www.reddit.com/r/dotnet/.rss` |
| Fréquence | Très élevée |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Communautés Reddit |

---

## Cercle 10 — Économie, entreprises françaises, marché

**Importance** : ⭐⭐ — pour comprendre le contexte économique français et les besoins clients potentiels.

### 10.1 Les Échos — Entreprises

| | |
|---|---|
| Site | https://www.lesechos.fr/ |
| Flux | https://services.lesechos.fr/rss/les-echos-entreprises.xml |
| Fréquence | ~20 par jour |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Référence économique française |

### 10.2 L'Usine Digitale

| | |
|---|---|
| Site | https://www.usine-digitale.fr/ |
| Flux | https://www.usine-digitale.fr/rss/ |
| Fréquence | ~10 par jour |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Transformation numérique des entreprises |

### 10.3 BFM Business

| | |
|---|---|
| Site | https://www.bfmtv.com/economie/ |
| Flux | https://www.bfmtv.com/rss/economie/ |
| Fréquence | Quotidienne (élevée) |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Actualité éco grand public |

### 10.4 Capital

| | |
|---|---|
| Site | https://www.capital.fr/ |
| Flux | https://www.capital.fr/feed |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Magazine éco grand public |

### 10.5 Maddyness

| | |
|---|---|
| Site | https://www.maddyness.com/ |
| Flux | https://www.maddyness.com/feed/ |
| Fréquence | ~5-10 par jour |
| Langue | FR |
| Pertinence | ⭐⭐⭐ |
| Note | Startups, innovation, écosystème français |

### 10.6 Le Journal du Net

| | |
|---|---|
| Site | https://www.journaldunet.com/ |
| Flux | https://www.journaldunet.com/rss/ |
| Fréquence | Quotidienne |
| Langue | FR |
| Pertinence | ⭐⭐ |
| Note | Actualités digitales et économiques |

---

## Cercle 11 — Design, UX, produit

**Importance** : ⭐⭐ — pour l'UI / UX du projet MAUI.

### 11.1 NN/g — Nielsen Norman Group

| | |
|---|---|
| Site | https://www.nngroup.com/articles/ |
| Flux | https://www.nngroup.com/feed/rss/ |
| Fréquence | ~2-3 par semaine |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Référence mondiale en UX recherche-based |

### 11.2 Smashing Magazine

| | |
|---|---|
| Site | https://www.smashingmagazine.com/ |
| Flux | https://www.smashingmagazine.com/feed/ |
| Fréquence | Quotidienne |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Web design et développement front |

### 11.3 A List Apart

| | |
|---|---|
| Site | https://alistapart.com/ |
| Flux | https://alistapart.com/main/feed/ |
| Fréquence | ~1-2 par mois |
| Langue | EN |
| Pertinence | ⭐⭐⭐ |
| Note | Articles fondamentaux sur le web design |

### 11.4 UX Collective

| | |
|---|---|
| Site | https://uxdesign.cc/ |
| Flux | https://uxdesign.cc/feed |
| Fréquence | ~10 par jour |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Publication Medium spécialisée UX. Qualité variable mais beaucoup de matière |

### 11.5 Material Design Blog

| | |
|---|---|
| Site | https://m3.material.io/blog |
| Flux | À chercher |
| Fréquence | Variable |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Standard design Google. Applicable à MAUI Android |

### 11.6 Sidebar (curation design)

| | |
|---|---|
| Site | https://sidebar.io/ |
| Flux | https://sidebar.io/feed.xml |
| Fréquence | Quotidienne (5 liens) |
| Langue | EN |
| Pertinence | ⭐⭐ |
| Note | Curation quotidienne des meilleurs articles design |

---

## Cercle 12 — Cercles annexes (hors sujet direct mais utiles)

### 12.1 Stratech (long-form business & tech)

| | |
|---|---|
| Site | https://stratechery.com/ |
| Flux | https://stratechery.com/feed/ |
| Pertinence | ⭐⭐ |

### 12.2 Boris Wertz — Version One

| | |
|---|---|
| Site | https://versionone.vc/blog/ |
| Flux | https://versionone.vc/feed/ |
| Pertinence | ⭐⭐ |

### 12.3 Communautés .NET françaises (Discord, Slack)

Pas de RSS mais à mentionner :
- Discord .NET Foundation
- Slack MAUI France
- Forum Developpez.com (.NET section)

### 12.4 Newsletters (alternatives au RSS)

À considérer même si pas du RSS strict :
- **The Pragmatic Engineer** (Gergely Orosz) — gratuite et payante
- **Software Lead Weekly**
- **Refactoring** (Luca Rossi)

---

## Comment consommer : lecteurs et workflow

### Lecteurs RSS recommandés

| Outil | Plateforme | Avantages | Inconvénients |
|---|---|---|---|
| **FreshRSS** | Self-hosted | Open source, gratuit, full control | Configuration serveur |
| **Miniflux** | Self-hosted | Minimaliste, performant, OPML | Pas d'UI riche |
| **NetNewsWire** | macOS / iOS | Gratuit, natif, beau | macOS / iOS only |
| **Reeder 5** | macOS / iOS | Payant, UX excellente | Payant |
| **Feedly** | Cloud | Mature, multi-platform | Freemium agressif |
| **Inoreader** | Cloud | Riche en features | Freemium |
| **Thunderbird** | Desktop | Open source, polyvalent | UX vieillotte |
| **RSS Guard** | Desktop multiplate-formes | Open source, support multiple comptes | UX rugueuse |

**Recommandation** : **FreshRSS** ou **Miniflux** auto-hébergés (tu as déjà un GitLab self-hosted, ajoute un container Docker à côté). Tu garderas la maîtrise de tes données et tes choix de veille.

### Workflow de veille recommandé

#### Routine quotidienne (15 minutes max)

- Le matin café : ouvrir le lecteur RSS
- Parcourir les **Cercles 1-3** uniquement
- Marquer comme lu / favori / à approfondir

#### Routine hebdomadaire (1h)

- Le vendredi : parcourir les Cercles 4-6
- Lire en profondeur les 2-3 articles les plus pertinents
- Archiver dans un système de prise de notes (Obsidian, Logseq, Notion) les idées récurrentes

#### Routine mensuelle (2-3h)

- Parcourir les Cercles 7-12
- Faire le ménage : désabonner des sources qui ne génèrent plus de valeur
- Ajouter de nouvelles sources découvertes par recommandations

### Anti-patterns à éviter

- **Vouloir tout lire** : tu auras 500-1000 articles non lus par semaine. C'est OK. Le RSS, c'est de la pêche au filet, pas de la lecture obligatoire.
- **S'abonner trop vite** : tester une source 2 semaines avant de la garder définitivement.
- **Ignorer les sources françaises** : la pertinence locale prime souvent sur le volume international.
- **Ne pas archiver** : un article inspirant qu'on retrouve pas 3 mois plus tard = info perdue.

---

## Flux intégrables dans le projet lui-même

> **Cette section est désormais critique** (cf. ADR-009 + cluster F-041 à F-050 dans la doc 02). Elle ne décrit plus seulement des "possibilités lointaines" mais bien la **matière première du cluster Veille du MVP 2**.

Certains de ces flux RSS sont **utilisés comme sources de données** dans le produit (feature de veille pour les utilisateurs finaux), pas seulement pour la veille perso. La conception du cluster Veille (F-041 à F-050) s'appuie directement sur ce catalogue.

### Templates de veille pré-curés par métier — à constituer en MVP 2

À partir de ce catalogue, voici les **Packs de veille** initiaux à proposer aux utilisateurs à l'inscription. Chaque pack regroupe les flux les plus pertinents pour un segment professionnel.

#### Pack "Cabinet de Propriété Industrielle"
- INPI Actualités
- BOPI (via scraping)
- IPKat
- EUIPO News
- OMPI / WIPO News
- OEB (EPO) News
- Cour de cassation (décisions PI)
- Plasseraud, Beau de Loménie (blogs cabinets)
- Légifrance — textes droit propriété intellectuelle
- Dalloz Actualité (filtrée PI)

#### Pack "Expert-comptable"
- data.gouv.fr (nouveautés datasets entreprises)
- BODACC (annonces légales — flux personnalisé par tribunaux locaux)
- INSEE (publications économiques)
- Bpifrance Le Lab
- Service-Public Pro
- Légifrance (droit des sociétés, fiscalité)
- Les Échos — Entreprises
- Banque de France (statistiques)

#### Pack "Compliance / KYC / Anti-fraude"
- CNIL Actualités
- ANSSI / CERT-FR (bulletins de sécurité)
- IAPP Daily Dashboard
- EDPB
- EUR-Lex (veille législative UE)
- Schneier on Security
- BODACC (procédures collectives)
- Cour de cassation

#### Pack "Investisseur / M&A"
- Maddyness
- Frenchweb
- Sifted
- Les Échos — Entreprises
- BODACC (ventes de fonds, dissolutions)
- INPI (nouveaux dépôts brevets sur secteur)
- Bpifrance Le Lab
- DECP consolidé / API tabulaire data.gouv.fr (nouveaux marchés publics — cf. F-032)

#### Pack "Veille concurrentielle B2B"
- BODACC (paramétré sur secteur)
- INPI (nouveaux dépôts marques concurrents)
- Les Échos — Entreprises
- L'Usine Digitale
- Maddyness, Frenchweb (startups)
- Capital
- Forbes France

#### Pack "Développeur / Tech curieux" (pour les users dev / autodidactes)
- .NET Blog, ASP.NET Blog
- Hacker News (best)
- Lobste.rs
- Scott Hanselman
- GitHub Trending
- Smashing Magazine

### Architecture pour intégrer ces flux (rappel ADR-004 et F-041)

L'archi hexagonale facilite ça :
1. Port `IExternalContentSource` côté domaine (Core)
2. Adapter `RssFeedSource` (cas général), `BodaccSource` (cas spécifique), `InpiBopiSource` (futur), etc.
3. Un service applicatif `FeedAggregationService` orchestre les sources
4. Persister les items en BDD pour ne pas les re-traiter (dédup F-045)
5. Notifier les utilisateurs selon leurs préférences (F-046)

C'est la feature **différenciante phare** pour le SaaS : peu d'outils combinent ces sources avec les données entreprises (F-047).

### Modération et gouvernance du catalogue

Le catalogue de flux intégré au produit doit être :
- **Maintenu** : audit trimestriel pour vérifier que les flux sont vivants
- **Modéré** : éviter d'inclure des sources de qualité douteuse, polémiques, ou contraires à la ligne éditoriale
- **Évolutif** : ajout / retrait de flux au fil du temps
- **Documenté** : pour chaque flux, justifier sa présence dans tel ou tel pack

---

## Bundle OPML à importer

Un fichier **OPML** (Outline Processor Markup Language) permet d'importer en un clic une collection de flux dans un lecteur RSS.

Tu peux créer un fichier `veille-projet.opml` à partir de cette doc, structuré ainsi :

```xml
<?xml version="1.0" encoding="UTF-8"?>
<opml version="2.0">
  <head>
    <title>Veille projet — Bundle complet</title>
  </head>
  <body>
    <outline text="Cercle 1 — INPI / PI">
      <outline type="rss" text="INPI Actualités" xmlUrl="https://www.inpi.fr/rss.xml" />
      <outline type="rss" text="IPKat" xmlUrl="https://ipkitten.blogspot.com/feeds/posts/default" />
      <!-- etc. -->
    </outline>
    <outline text="Cercle 2 — Données publiques">
      <outline type="rss" text="data.gouv.fr" xmlUrl="https://www.data.gouv.fr/fr/posts/recent.atom" />
      <!-- etc. -->
    </outline>
    <!-- etc. -->
  </body>
</opml>
```

**Suggestion** : générer ce fichier OPML automatiquement à partir d'un script lisant ce document Markdown. Ainsi, quand on met à jour le catalogue, l'OPML est synchronisé.

---

## Maintenance du catalogue

Ce document doit être **vivant**. Suggestions :

1. **Tous les 3 mois** : vérifier que les flux les plus importants (Cercle 1-3) sont toujours actifs.
2. **Tous les 6 mois** : audit complet, retrait des sources mortes, ajout de nouvelles.
3. **À chaque fois qu'on lit un article exceptionnel** : se demander si la source est dans le catalogue.

---

*Document évolutif. Toute modification doit être tracée. Ce catalogue est le compagnon de veille du porteur du projet — il doit grandir et évoluer avec lui.*
