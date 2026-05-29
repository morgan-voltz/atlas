namespace Atlas.Infrastructure.Messaging.Push.Wns;

/// <summary>
/// Configuration de l'adapter Windows Notification Service (F-020, Windows desktop).
/// WNS s'authentifie en OAuth2 <c>client_credentials</c> via
/// <c>https://login.live.com/accesstoken.srf</c>. <see cref="PackageSid"/> est l'identifiant
/// du package UWP / WinUI (format <c>ms-app://s-1-15-2-…</c>) ; <see cref="ClientSecret"/>
/// est le secret correspondant fourni par le Partner Center.
/// </summary>
internal sealed class WnsOptions
{
    public const string SectionName = "Wns";

    /// <summary>Package Security Identifier (SID) — identifiant du package UWP / MSIX.</summary>
    public string? PackageSid { get; set; }

    /// <summary>Secret client associé au PackageSid. **Ne jamais committer** — passer via secrets / KMS.</summary>
    public string? ClientSecret { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
