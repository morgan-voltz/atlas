using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.PublishVeillePack;

internal sealed class PublishVeillePackHandler(
    IVeillePackRepository packRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PublishVeillePackCommand, Result<VeillePackMarketplaceDto>>
{
    public async Task<Result<VeillePackMarketplaceDto>> Handle(
        PublishVeillePackCommand request,
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

        Result published = pack.Publish();
        if (published.IsFailure)
        {
            return Result<VeillePackMarketplaceDto>.Fail(published.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<VeillePackMarketplaceDto>.Ok(VeillePackMarketplaceDto.From(pack));
    }
}
