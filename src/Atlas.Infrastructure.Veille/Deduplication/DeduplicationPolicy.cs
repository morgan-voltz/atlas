using Atlas.Domain.Veille;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Veille.Deduplication;

/// <summary>
/// Implémentation de <see cref="IDeduplicationPolicy"/> adossée à <see cref="VeilleOptions"/> (F-045).
/// Le seuil de similarité (0..1) est converti en distance de Hamming maximale sur les 64 bits du SimHash.
/// </summary>
internal sealed class DeduplicationPolicy(IOptions<VeilleOptions> options) : IDeduplicationPolicy
{
    private readonly VeilleOptions _options = options.Value;

    public int MaxHammingDistance => (int)(64 * (1 - _options.DeduplicationThreshold));

    public TimeSpan ClusterWindow => TimeSpan.FromHours(_options.ClusterWindowHours);
}
