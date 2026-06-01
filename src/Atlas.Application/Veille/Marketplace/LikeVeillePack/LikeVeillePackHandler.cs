using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.LikeVeillePack;

internal sealed class LikeVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeillePackLikeRepository likeRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LikeVeillePackCommand, Result>
{
    public async Task<Result> Handle(LikeVeillePackCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string normalizedCode = (request.Code ?? string.Empty).Trim().ToLowerInvariant();

        VeillePack? pack = await packRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (pack is null || !pack.IsActive)
        {
            return Result.Fail(VeilleErrors.VeillePackNotFound);
        }

        if (pack.Visibility != VeillePackVisibility.Public)
        {
            return Result.Fail(VeilleErrors.VeillePackNotPublic);
        }

        if (await likeRepository.ExistsAsync(userId, pack.Id, cancellationToken))
        {
            // Idempotent : déjà liké → succès sans double-comptage.
            return Result.Ok();
        }

        var like = VeillePackLike.Create(pack.Id, userId, clock.UtcNow);
        await likeRepository.AddAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Incrément atomique APRÈS la persistance du like (audit Lot 3, M7) : le compteur ne bouge que
        // si la ligne de like a bien été créée, et sans lost update entre likes concurrents.
        await packRepository.IncrementLikesAsync(pack.Id, cancellationToken);
        return Result.Ok();
    }
}
