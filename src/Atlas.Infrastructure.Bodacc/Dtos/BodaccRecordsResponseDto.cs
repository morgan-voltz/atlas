using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Bodacc.Dtos;

/// <summary>Réponse Opendatasoft <c>annonces-commerciales</c> (F-048).</summary>
internal sealed class BodaccRecordsResponseDto
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("results")]
    public List<BodaccRecordDto> Results { get; set; } = [];
}

internal sealed class BodaccRecordDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("dateparution")]
    public string? DateParution { get; set; }

    /// <summary>Libellé du type d'annonce (« Création », « Procédure collective », « Vente », …).</summary>
    [JsonPropertyName("typeavis_lib")]
    public string? TypeLabel { get; set; }

    [JsonPropertyName("tribunal")]
    public string? Tribunal { get; set; }

    /// <summary>Champ texte représentant le commerçant / la dénomination.</summary>
    [JsonPropertyName("commercant")]
    public string? Commercant { get; set; }

    /// <summary>Champ texte « ville » de la publication, utilisé comme part de l'excerpt si non vide.</summary>
    [JsonPropertyName("ville")]
    public string? Ville { get; set; }
}
