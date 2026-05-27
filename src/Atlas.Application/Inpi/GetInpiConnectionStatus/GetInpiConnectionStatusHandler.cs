using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.GetInpiConnectionStatus;

internal sealed class GetInpiConnectionStatusHandler(
    IInpiCredentialsRepository credentialsRepository)
    : IRequestHandler<GetInpiConnectionStatusQuery, Result<InpiConnectionStatusDto>>
{
    public async Task<Result<InpiConnectionStatusDto>> Handle(
        GetInpiConnectionStatusQuery request,
        CancellationToken cancellationToken)
    {
        InpiCredentials? credentials =
            await credentialsRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);

        InpiConnectionStatusDto status = credentials is null
            ? new InpiConnectionStatusDto(Connected: false, Status: null, LastTestedAt: null)
            : new InpiConnectionStatusDto(
                Connected: true,
                Status: credentials.Status.ToString(),
                LastTestedAt: credentials.LastTestedAt);

        return Result<InpiConnectionStatusDto>.Ok(status);
    }
}
