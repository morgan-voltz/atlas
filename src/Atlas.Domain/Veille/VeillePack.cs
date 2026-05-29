using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Pack de veille : référence un ensemble de <see cref="FeedSource"/> partagées via des
/// <see cref="VeillePackItem"/>. Cf. doc 08 §9.4. Deux origines possibles :
/// <list type="bullet">
/// <item>**Système** (F-042) — pré-curé par l'équipe, <see cref="AuthorUserId"/> null, visibilité <c>System</c>.</item>
/// <item>**Utilisateur** (F-049 marketplace) — créé par un user, brouillon (<c>Private</c>) puis publiable au catalogue communautaire (<c>Public</c>).</item>
/// </list>
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

    private VeillePack(
        VeillePackId id,
        string code,
        string name,
        string description,
        int version,
        UserId? authorUserId,
        VeillePackVisibility visibility,
        DateTimeOffset createdAt)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Version = version;
        AuthorUserId = authorUserId;
        Visibility = visibility;
        IsActive = true;
        CreatedAt = createdAt;
        LikesCount = 0;
    }

    /// <summary>Identifiant stable et lisible du pack (slug), ex. <c>cabinet-pi</c>.</summary>
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    /// <summary>Version du pack, monotone croissante. Permet de signaler une mise à jour aux abonnés (re-sync).</summary>
    public int Version { get; private set; }

    /// <summary>Auteur du pack — null pour un pack système (F-042), renseigné pour un pack user (F-049).</summary>
    public UserId? AuthorUserId { get; private set; }

    /// <summary>Visibilité courante du pack (System / Private / Public).</summary>
    public VeillePackVisibility Visibility { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Compteur dénormalisé de likes communautaires (F-049), maintenu par <see cref="IncrementLikes"/> / <see cref="DecrementLikes"/>.</summary>
    public int LikesCount { get; private set; }

    public IReadOnlyCollection<VeillePackItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<FeedSourceId> SourceIds => _items.Select(item => item.SourceId).ToList();

    public bool IsSystemPack => Visibility == VeillePackVisibility.System;

    public bool IsUserPack => AuthorUserId.HasValue && !IsSystemPack;

    /// <summary>Crée un pack **système** (F-042) — pré-curé par l'équipe.</summary>
    public static Result<VeillePack> Create(string code, string name, string description, DateTimeOffset now) =>
        CreateInternal(code, name, description, authorUserId: null, VeillePackVisibility.System, now);

    /// <summary>Crée un pack **utilisateur** (F-049) en brouillon (<see cref="VeillePackVisibility.Private"/>).</summary>
    public static Result<VeillePack> CreateUserPack(
        UserId authorUserId,
        string code,
        string name,
        string description,
        DateTimeOffset now) =>
        CreateInternal(code, name, description, authorUserId, VeillePackVisibility.Private, now);

    private static Result<VeillePack> CreateInternal(
        string code,
        string name,
        string description,
        UserId? authorUserId,
        VeillePackVisibility visibility,
        DateTimeOffset now)
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

        return Result<VeillePack>.Ok(new VeillePack(
            VeillePackId.New(),
            trimmedCode,
            trimmedName,
            trimmedDescription,
            version: 1,
            authorUserId,
            visibility,
            now));
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

    /// <summary>Publie un pack utilisateur (Private → Public, F-049). Échec si pack système ou déjà publié.</summary>
    public Result Publish()
    {
        if (IsSystemPack)
        {
            return Result.Fail(VeilleErrors.VeillePackImmutable);
        }

        if (Visibility == VeillePackVisibility.Public)
        {
            return Result.Ok();
        }

        Visibility = VeillePackVisibility.Public;
        return Result.Ok();
    }

    /// <summary>Repasse un pack publié en brouillon (Public → Private, F-049). Échec si pack système.</summary>
    public Result Unpublish()
    {
        if (IsSystemPack)
        {
            return Result.Fail(VeilleErrors.VeillePackImmutable);
        }

        if (Visibility == VeillePackVisibility.Private)
        {
            return Result.Ok();
        }

        Visibility = VeillePackVisibility.Private;
        return Result.Ok();
    }

    /// <summary>Incrémente le compteur dénormalisé de likes (F-049). Appelé après ajout d'un <see cref="VeillePackLike"/>.</summary>
    public void IncrementLikes() => LikesCount += 1;

    /// <summary>Décrémente le compteur dénormalisé de likes (F-049). Plancher à 0 par sécurité.</summary>
    public void DecrementLikes() => LikesCount = Math.Max(0, LikesCount - 1);
}
