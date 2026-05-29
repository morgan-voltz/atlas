using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.CreateUserVeillePack;

internal sealed class CreateUserVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeilleSubscriptionRepository subscriptionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserVeillePackCommand, Result<VeillePackMarketplaceDto>>
{
    public async Task<Result<VeillePackMarketplaceDto>> Handle(
        CreateUserVeillePackCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        // 1. Code unique au catalogue (System + User packs).
        string normalizedCode = (request.Code ?? string.Empty).Trim().ToLowerInvariant();
        if (await packRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
        {
            return Result<VeillePackMarketplaceDto>.Fail(VeilleErrors.VeillePackCodeAlreadyUsed);
        }

        // 2. Résolution des subscriptionIds en FeedSourceIds, restreinte aux abonnements de l'utilisateur.
        IReadOnlyList<VeilleSubscription> userSubscriptions =
            await subscriptionRepository.GetByUserAsync(userId, cancellationToken);

        HashSet<Guid> requestedIds = [.. (request.SubscriptionIds ?? [])];
        var sourceIds = userSubscriptions
            .Where(s => requestedIds.Contains(s.Id.Value))
            .Select(s => s.SourceId)
            .Distinct()
            .ToList();

        if (sourceIds.Count == 0)
        {
            return Result<VeillePackMarketplaceDto>.Fail(
                VeilleErrors.InvalidVeillePack("au moins une source requise (résolue depuis vos abonnements)."));
        }

        Result<VeillePack> created = VeillePack.CreateUserPack(
            userId,
            normalizedCode,
            request.Name,
            request.Description ?? string.Empty,
            clock.UtcNow);

        if (created.IsFailure)
        {
            return Result<VeillePackMarketplaceDto>.Fail(created.Error!);
        }

        VeillePack pack = created.Value!;
        pack.SetSources(sourceIds);

        await packRepository.AddAsync(pack, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VeillePackMarketplaceDto>.Ok(VeillePackMarketplaceDto.From(pack));
    }
}
