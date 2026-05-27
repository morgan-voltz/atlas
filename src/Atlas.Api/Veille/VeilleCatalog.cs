using Atlas.Domain.Veille;

namespace Atlas.Api.Veille;

/// <summary>
/// Catalogue curé des sources de veille et des VeillePacks (F-042), dérivé de <c>docs/07-flux-rss-veille.md</c>.
/// Source de vérité unique partagée par <see cref="FeedSourceSeeder"/> (sources) et <see cref="VeillePackSeeder"/>
/// (packs). On ne référence que des flux RSS/Atom avec URL connue ; les sources « via scraping » (BODACC, BOPI,
/// DECP, GitHub Trending) rejoindront les packs quand leur provider <see cref="IExternalContentSource"/> existera.
/// </summary>
internal static class VeilleCatalog
{
    internal sealed record CatalogSource(string Name, string Url, FeedSourceType Type);

    internal sealed record CatalogPack(string Code, string Name, string Description, int Version, string[] SourceUrls);

    // --- Sources (cf. cercles doc 07). Quelques URLs sont marquées « à vérifier » dans la doc :
    //     un flux mort échoue proprement au polling (géré par F-041), sans casser le catalogue. ---
    public const string InpiActualites = "https://www.inpi.fr/rss.xml";
    public const string EuipoNews = "https://www.euipo.europa.eu/en/news/rss";
    public const string WipoNews = "https://www.wipo.int/feeds/news.xml";
    public const string EpoNews = "https://www.epo.org/news-events/news.rss";
    public const string IpKat = "https://ipkitten.blogspot.com/feeds/posts/default";
    public const string Plasseraud = "https://www.plass.com/feed/";
    public const string Legifrance = "https://www.legifrance.gouv.fr/contenu/Media/Actualites/rss.xml";
    public const string DallozActualite = "https://www.dalloz-actualite.fr/rss.xml";
    public const string Cnil = "https://www.cnil.fr/fr/rss.xml";
    public const string Edpb = "https://www.edpb.europa.eu/news/all-news_en.rss";
    public const string DataGouv = "https://www.data.gouv.fr/fr/posts/recent.atom";
    public const string Insee = "https://www.insee.fr/fr/information/rss.xml";
    public const string BpifranceLeLab = "https://lelab.bpifrance.fr/feed/";
    public const string BanqueDeFrance = "https://www.banque-france.fr/rss/publications-economiques";
    public const string ServicePublicPro = "https://entreprendre.service-public.fr/rss-fil-actualite.xml";
    public const string LesEchosEntreprises = "https://services.lesechos.fr/rss/les-echos-entreprises.xml";
    public const string CertFr = "https://www.cert.ssi.gouv.fr/feed/";
    public const string Iapp = "https://iapp.org/news/atom";
    public const string Schneier = "https://www.schneier.com/feed/atom/";
    public const string Maddyness = "https://www.maddyness.com/feed/";
    public const string Frenchweb = "https://www.frenchweb.fr/feed";
    public const string Sifted = "https://sifted.eu/feed";
    public const string UsineDigitale = "https://www.usine-digitale.fr/rss/";
    public const string Capital = "https://www.capital.fr/feed";
    public const string DotNetBlog = "https://devblogs.microsoft.com/dotnet/feed/";
    public const string AspNetBlog = "https://devblogs.microsoft.com/aspnet/feed/";
    public const string ScottHanselman = "https://www.hanselman.com/blog/feed.aspx";
    public const string HackerNewsBest = "https://hnrss.org/best";
    public const string Lobsters = "https://lobste.rs/rss";
    public const string SmashingMagazine = "https://www.smashingmagazine.com/feed/";

    public static IReadOnlyList<CatalogSource> AllSources { get; } =
    [
        new("INPI — Actualités", InpiActualites, FeedSourceType.Rss),
        new("EUIPO News", EuipoNews, FeedSourceType.Rss),
        new("OMPI / WIPO News", WipoNews, FeedSourceType.Rss),
        new("OEB (EPO) — Actualités", EpoNews, FeedSourceType.Rss),
        new("IPKat", IpKat, FeedSourceType.Atom),
        new("Cabinet Plasseraud — Actualités PI", Plasseraud, FeedSourceType.Rss),
        new("Légifrance — Nouveautés textes", Legifrance, FeedSourceType.Rss),
        new("Dalloz Actualité", DallozActualite, FeedSourceType.Rss),
        new("CNIL — Actualités", Cnil, FeedSourceType.Rss),
        new("EDPB — European Data Protection Board", Edpb, FeedSourceType.Rss),
        new("data.gouv.fr — Actualités", DataGouv, FeedSourceType.Atom),
        new("INSEE — Actualités", Insee, FeedSourceType.Rss),
        new("Bpifrance — Le Lab", BpifranceLeLab, FeedSourceType.Rss),
        new("Banque de France — Publications", BanqueDeFrance, FeedSourceType.Rss),
        new("Service-Public.fr — Pro", ServicePublicPro, FeedSourceType.Rss),
        new("Les Échos — Entreprises", LesEchosEntreprises, FeedSourceType.Rss),
        new("ANSSI — CERT-FR", CertFr, FeedSourceType.Rss),
        new("IAPP — Daily Dashboard", Iapp, FeedSourceType.Atom),
        new("Schneier on Security", Schneier, FeedSourceType.Atom),
        new("Maddyness", Maddyness, FeedSourceType.Rss),
        new("Frenchweb", Frenchweb, FeedSourceType.Rss),
        new("Sifted", Sifted, FeedSourceType.Rss),
        new("L'Usine Digitale", UsineDigitale, FeedSourceType.Rss),
        new("Capital", Capital, FeedSourceType.Rss),
        new(".NET Blog", DotNetBlog, FeedSourceType.Rss),
        new("ASP.NET Blog", AspNetBlog, FeedSourceType.Rss),
        new("Scott Hanselman", ScottHanselman, FeedSourceType.Rss),
        new("Hacker News (Best)", HackerNewsBest, FeedSourceType.Rss),
        new("Lobste.rs", Lobsters, FeedSourceType.Rss),
        new("Smashing Magazine", SmashingMagazine, FeedSourceType.Rss),
    ];

    public static IReadOnlyList<CatalogPack> Packs { get; } =
    [
        new(
            "cabinet-pi",
            "Cabinet de Propriété Industrielle",
            "Veille PI : offices (INPI, EUIPO, OMPI, OEB), jurisprudence et cabinets de référence.",
            1,
            [InpiActualites, EuipoNews, WipoNews, EpoNews, IpKat, Plasseraud, Legifrance, DallozActualite]),
        new(
            "expert-comptable",
            "Expert-comptable",
            "Données publiques entreprises, publications économiques et actualité réglementaire.",
            1,
            [DataGouv, Insee, BpifranceLeLab, BanqueDeFrance, ServicePublicPro, Legifrance, LesEchosEntreprises]),
        new(
            "compliance",
            "Compliance / KYC / Anti-fraude",
            "RGPD, sécurité et conformité : CNIL, CERT-FR, IAPP, EDPB.",
            1,
            [Cnil, CertFr, Iapp, Edpb, Schneier]),
        new(
            "investisseur",
            "Investisseur / M&A",
            "Écosystème startups, financement et signaux entreprises.",
            1,
            [Maddyness, Frenchweb, Sifted, LesEchosEntreprises, InpiActualites, BpifranceLeLab]),
        new(
            "veille-concurrentielle",
            "Veille concurrentielle B2B",
            "Actualité économique et tech pour suivre marché et concurrents.",
            1,
            [LesEchosEntreprises, UsineDigitale, Maddyness, Frenchweb, Capital]),
        new(
            "developpeur",
            "Développeur / Tech curieux",
            "Veille .NET, ASP.NET et culture tech générale.",
            1,
            [DotNetBlog, AspNetBlog, ScottHanselman, HackerNewsBest, Lobsters, SmashingMagazine]),
    ];
}
