using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Downloads;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.RequestBulkDownload;

internal sealed class RequestBulkDownloadHandler(
    IBulkDownloadJobRepository repository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<RequestBulkDownloadCommand, Result<Guid>>
{
    /// <summary>Durée de vie d'une archive avant expiration et suppression.</summary>
    private static readonly TimeSpan ArchiveTtl = TimeSpan.FromHours(24);

    public async Task<Result<Guid>> Handle(RequestBulkDownloadCommand request, CancellationToken cancellationToken)
    {
        if (request.Sirens is null || request.Sirens.Count == 0)
        {
            return Result<Guid>.Fail(BulkDownloadErrors.EmptySirens);
        }

        if (request.Sirens.Count > BulkDownloadJob.MaxSirens)
        {
            return Result<Guid>.Fail(BulkDownloadErrors.TooManySirens(BulkDownloadJob.MaxSirens));
        }

        // Validation des SIREN — on rejette tôt si l'un est invalide plutôt que de découvrir
        // l'erreur côté job en arrière-plan.
        var validated = new List<string>(request.Sirens.Count);
        foreach (string raw in request.Sirens)
        {
            Result<Siren> parsed = Siren.Create(raw);
            if (parsed.IsFailure)
            {
                return Result<Guid>.Fail(BulkDownloadErrors.InvalidSiren(raw ?? string.Empty));
            }
            validated.Add(parsed.Value.Value);
        }

        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(request.UserId),
            validated,
            clock.UtcNow,
            ArchiveTtl);

        await repository.AddAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(job.Id.Value);
    }
}
