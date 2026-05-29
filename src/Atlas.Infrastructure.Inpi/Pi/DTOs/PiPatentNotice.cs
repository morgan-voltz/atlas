using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Inpi.Pi.DTOs;

/// <summary>
/// DTO best-effort de la notice brevet INPI PI (F-015). Le contrat exact n'a pas été validé
/// contre l'API réelle : tous les champs sont optionnels et le mapping est défensif.
/// </summary>
internal sealed class PiPatentNotice
{
    [JsonPropertyName("numeroPublication")]
    public string? NumeroPublication { get; set; }

    [JsonPropertyName("titre")]
    public string? Titre { get; set; }

    [JsonPropertyName("deposant")]
    public string? Deposant { get; set; }

    [JsonPropertyName("inventeurs")]
    public List<string>? Inventeurs { get; set; }

    [JsonPropertyName("dateDepot")]
    public string? DateDepot { get; set; }

    [JsonPropertyName("datePublication")]
    public string? DatePublication { get; set; }

    [JsonPropertyName("statut")]
    public string? Statut { get; set; }

    [JsonPropertyName("abrege")]
    public string? Abrege { get; set; }
}
