using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.UnlikeVeillePack;

internal sealed class UnlikeVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeillePackLikeRepository likeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnlikeVeillePackCommand, Result>
{
    public async Task<Result> Handle(UnlikeVeillePackCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string normalizedCode = (request.Code ?? string.Empty).Trim().ToLowerInvariant();

        VeillePack? pack = await packRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (pack is null)
        {
            return Result.Fail(VeilleErrors.VeillePackNotFound);
        }

        VeillePackLike? like = await likeRepository.GetAsync(userId, pack.Id, cancellationToken);
        if (like is null)
        {
            // Idempotent : pas liké → succès silencieux.
            return Result.Ok();
        }

        await likeRepository.RemoveAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Décrément atomique borné à 0 (audit Lot 3, M7), après suppression effective du like.
        await packRepository.DecrementLikesAsync(pack.Id, cancellationToken);
        return Result.Ok();
    }
}
