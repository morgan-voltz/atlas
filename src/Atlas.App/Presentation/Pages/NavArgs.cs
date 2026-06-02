namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Argument de navigation vers <see cref="VerifierEmailPage"/>. Deux usages (doc 12 §10, parité M7) :
/// après inscription (<c>Email</c> seul → état d'attente + renvoi), ou via le lien d'activation
/// (<c>UserId</c> + <c>Token</c> → vérification immédiate). Passé en paramètre <see cref="Microsoft.UI.Xaml.Controls.Frame"/>
/// (intra-app) ou reconstruit depuis l'URL au démarrage (deep-link WASM, cf. <c>App.OnLaunched</c>).
/// </summary>
internal sealed record VerifyEmailArgs(string? Email, string? UserId, string? Token);

/// <summary>
/// Argument de navigation vers <see cref="ReinitialiserMotDePassePage"/> : <c>UserId</c> + <c>Token</c>
/// issus du lien d'email. Sans eux, la page affiche « lien invalide ».
/// </summary>
internal sealed record ResetPasswordArgs(string? UserId, string? Token);
