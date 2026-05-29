using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.UnpublishVeillePack;

internal sealed class UnpublishVeillePackHandler(
    IVeillePackRepository packRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnpublishVeillePackCommand, Result<VeillePackMarketplaceDto>>
{
    public async Task<Result<VeillePackMarketplaceDto>> Handle(
        UnpublishVeillePackCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string normalizedCode = (request.Code ?? string.Empty).Trim().ToLowerInvariant();

        VeillePack? pack = await packRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (pack is null)
        {
            return Result<VeillePackMarketplaceDto>.Fail(VeilleErrors.VeillePackNotFound);
        }

        if (!pack.AuthorUserId.HasValue || !pack.AuthorUserId.Value.Equals(userId))
        {
            return Result<VeillePackMarketplaceDto>.Fail(VeilleErrors.VeillePackForbidden);
        }

        Result unpublished = pack.Unpublish();
        if (unpublished.IsFailure)
        {
            return Result<VeillePackMarketplaceDto>.Fail(unpublished.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<VeillePackMarketplaceDto>.Ok(VeillePackMarketplaceDto.From(pack));
    }
}
