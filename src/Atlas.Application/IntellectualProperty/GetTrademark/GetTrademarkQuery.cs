using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.GetTrademark;

public sealed record GetTrademarkQuery(Guid UserId, string DepositNumber) : IRequest<Result<TrademarkDetailDto>>;

public sealed record GetTrademarkImageQuery(Guid UserId, string DepositNumber) : IRequest<Result<TrademarkImageDto>>;
