using Atlas.Application.Inpi;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.GetTrademark;

internal sealed class GetTrademarkHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    IIntellectualPropertyProvider intellectualPropertyProvider)
    : IRequestHandler<GetTrademarkQuery, Result<TrademarkDetailDto>>
{
    public async Task<Result<TrademarkDetailDto>> Handle(GetTrademarkQuery request, CancellationToken cancellationToken)
    {
        Result<InpiAccessCredentials> access =
            await InpiAccessResolver.ResolveAsync(inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<TrademarkDetailDto>.Fail(access.Error!);
        }

        Result<TrademarkDetail> detail = await intellectualPropertyProvider.GetTrademarkAsync(
            new DepositNumber(request.DepositNumber), access.Value!, cancellationToken);
        if (detail.IsFailure)
        {
            return Result<TrademarkDetailDto>.Fail(detail.Error!);
        }

        return Result<TrademarkDetailDto>.Ok(Map(detail.Value!));
    }

    private static TrademarkDetailDto Map(TrademarkDetail trademark)
    {
        IReadOnlyList<NiceClassDto> classes = trademark.ClassesNice
            .Select(nice => new NiceClassDto(nice.Number, nice.Label))
            .ToList();

        return new TrademarkDetailDto(
            trademark.Denomination,
            trademark.Deposant,
            trademark.DepositNumber.Value,
            trademark.DateDepot,
            trademark.DateEnregistrement,
            trademark.StatutJuridique,
            trademark.Type,
            trademark.HasImage,
            classes);
    }
}

internal sealed class GetTrademarkImageHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    IIntellectualPropertyProvider intellectualPropertyProvider)
    : IRequestHandler<GetTrademarkImageQuery, Result<TrademarkImageDto>>
{
    public async Task<Result<TrademarkImageDto>> Handle(GetTrademarkImageQuery request, CancellationToken cancellationToken)
    {
        Result<InpiAccessCredentials> access =
            await InpiAccessResolver.ResolveAsync(inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<TrademarkImageDto>.Fail(access.Error!);
        }

        Result<TrademarkImage> image = await intellectualPropertyProvider.GetTrademarkImageAsync(
            new DepositNumber(request.DepositNumber), access.Value!, cancellationToken);
        if (image.IsFailure)
        {
            return Result<TrademarkImageDto>.Fail(image.Error!);
        }

        return Result<TrademarkImageDto>.Ok(new TrademarkImageDto(image.Value!.Content, image.Value!.ContentType));
    }
}
