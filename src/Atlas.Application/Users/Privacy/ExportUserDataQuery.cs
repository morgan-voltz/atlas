using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Privacy;

public sealed record ExportUserDataQuery(Guid UserId) : IRequest<Result<UserDataExportDto>>;
