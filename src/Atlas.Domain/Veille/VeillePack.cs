using Atlas.Domain.Common;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Pack de veille pré-curé par segment professionnel (Cabinet PI, Expert-comptable, etc.). Référence un
/// ensemble de <see cref="FeedSource"/> partagées via des <see cref="VeillePackItem"/>. Cf. doc 08 §9.4, F-042.
/// </summary>
public sealed class VeillePack : Entity<VeillePackId>
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;

    private readonly List<VeillePackItem> _items = [];

    private VeillePack()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private VeillePack(VeillePackId id, string code, string name, string description, int version, DateTimeOffset createdAt)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Version = version;
        IsActive = true;
        CreatedAt = createdAt;
    }

    /// <summary>Identifiant stable et lisible du pack (slug), ex. <c>cabinet-pi</c>.</summary>
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    /// <summary>Version du pack, monotone croissante. Permet de signaler une mise à jour aux abonnés (re-sync).</summary>
    public int Version { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<VeillePackItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<FeedSourceId> SourceIds => _items.Select(item => item.SourceId).ToList();

    public static Result<VeillePack> Create(string code, string name, string description, DateTimeOffset now)
    {
        string trimmedCode = (code ?? string.Empty).Trim().ToLowerInvariant();
        if (trimmedCode.Length is 0 or > MaxCodeLength)
        {
            return Result<VeillePack>.Fail(VeilleErrors.InvalidVeillePack("code requis (≤ 64 caractères)."));
        }

        string trimmedName = (name ?? string.Empty).Trim();
        if (trimmedName.Length is 0 or > MaxNameLength)
        {
            return Result<VeillePack>.Fail(VeilleErrors.InvalidVeillePack("nom requis (≤ 200 caractères)."));
        }

        string trimmedDescription = (description ?? string.Empty).Trim();
        if (trimmedDescription.Length > MaxDescriptionLength)
        {
            return Result<VeillePack>.Fail(VeilleErrors.InvalidVeillePack("description trop longue (≤ 1000 caractères)."));
        }

        return Result<VeillePack>.Ok(new VeillePack(VeillePackId.New(), trimmedCode, trimmedName, trimmedDescription, 1, now));
    }

    /// <summary>Remplace l'ensemble des sources du pack (dédupliqué).</summary>
    public void SetSources(IEnumerable<FeedSourceId> sourceIds)
    {
        ArgumentNullException.ThrowIfNull(sourceIds);

        _items.Clear();
        foreach (FeedSourceId sourceId in sourceIds.Distinct())
        {
            _items.Add(new VeillePackItem(sourceId));
        }
    }

    public void BumpVersion() => Version++;

    public void Deactivate() => IsActive = false;
}
