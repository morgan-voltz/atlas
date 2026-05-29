using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Messaging.Push.Fcm;

/// <summary>
/// Représentation des champs utiles d'un service account Firebase (F-020).
/// Le JSON complet contient d'autres champs ignorés ici (private_key_id, client_id, etc.).
/// </summary>
internal sealed class FcmServiceAccount
{
    [JsonPropertyName("client_email")]
    public string ClientEmail { get; set; } = string.Empty;

    [JsonPropertyName("private_key")]
    public string PrivateKey { get; set; } = string.Empty;

    [JsonPropertyName("token_uri")]
    public string TokenUri { get; set; } = "https://oauth2.googleapis.com/token";
}
