using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.GetInpiConnectionStatus;

public sealed record GetInpiConnectionStatusQuery(Guid UserId) : IRequest<Result<InpiConnectionStatusDto>>;
