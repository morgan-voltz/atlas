using System.Text.Json.Serialization;
using Atlas.Shared.Result;

namespace Atlas.App.Models;

/// <summary>
/// Contexte de sérialisation System.Text.Json *source-generated* pour les DTOs client. Indispensable
/// sur la tête WebAssembly (trimming AOT) : évite la sérialisation par réflexion (warning IL2026) qui
/// casserait au runtime une fois l'app trimmée.
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(CompanySummaryResponse))]
[JsonSerializable(typeof(PagedResult<CompanySummaryResponse>))]
[JsonSerializable(typeof(TimelineItemResponse))]
[JsonSerializable(typeof(FavoriteMentionResponse))]
internal sealed partial class AtlasJsonContext : JsonSerializerContext;
