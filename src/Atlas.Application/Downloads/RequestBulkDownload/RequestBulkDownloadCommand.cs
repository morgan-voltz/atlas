using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.RequestBulkDownload;

public sealed record RequestBulkDownloadCommand(Guid UserId, IReadOnlyList<string> Sirens)
    : IRequest<Result<Guid>>;
