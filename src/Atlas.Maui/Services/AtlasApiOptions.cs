namespace Atlas.Maui.Services;

internal static class AtlasApiOptions
{
    // Sur l'émulateur Android, 10.0.2.2 pointe vers le localhost de la machine hôte.
    // À adapter selon l'environnement de déploiement.
    public const string BaseUrl = "https://10.0.2.2:7201/";
}
