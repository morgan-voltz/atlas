using System.IO.Compression;
using Atlas.Application.Inpi;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Downloads;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Storage;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Downloads.RunBulkDownload;

internal sealed class RunBulkDownloadHandler(
    IBulkDownloadJobRepository jobs,
    IInpiCredentialsRepository inpiCredentials,
    ICryptoService crypto,
    ICompanyDataProvider companyProvider,
    IFileStorage storage,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    ILogger<RunBulkDownloadHandler> logger) : IRequestHandler<RunBulkDownloadCommand, Result>
{
    public async Task<Result> Handle(RunBulkDownloadCommand request, CancellationToken cancellationToken)
    {
        BulkDownloadJob? job = await jobs.GetByIdAsync(new BulkDownloadJobId(request.JobId), cancellationToken);
        if (job is null)
        {
            return Result.Fail(BulkDownloadErrors.NotFound);
        }

        // Idempotence : si le job est déjà finalisé, on ne refait rien.
        if (job.Status is BulkDownloadStatus.Ready or BulkDownloadStatus.Failed)
        {
            return Result.Ok();
        }

        Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
            inpiCredentials, crypto, job.UserId.Value, cancellationToken);
        if (access.IsFailure)
        {
            job.MarkAsFailed(access.Error!.Message, clock.UtcNow);
            jobs.Update(job);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        job.MarkAsRunning();
        jobs.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // Construction de l'archive ZIP en mémoire puis écriture dans IFileStorage.
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (string rawSiren in job.SirenList)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Result<Siren> parsed = Siren.Create(rawSiren);
                    if (parsed.IsFailure)
                    {
                        continue;
                    }
                    await AppendCompanyDocumentsAsync(archive, parsed.Value, access.Value!, cancellationToken);
                }
            }

            zipStream.Position = 0;
            string archiveKey = $"bulk/{job.Id.Value:N}.zip";
            await storage.SaveAsync(archiveKey, zipStream, "application/zip", cancellationToken);

            job.MarkAsReady(archiveKey, clock.UtcNow);
            jobs.Update(job);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Bulk download : echec sur le job {JobId}.", job.Id.Value);
            }
            job.MarkAsFailed(ex.Message, clock.UtcNow);
            jobs.Update(job);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }

    private async Task AppendCompanyDocumentsAsync(
        ZipArchive archive,
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct)
    {
        Result<IReadOnlyList<CompanyAttachment>> attachmentsResult =
            await companyProvider.GetAttachmentsAsync(siren, credentials, ct);
        if (attachmentsResult.IsFailure || attachmentsResult.Value!.Count == 0)
        {
            return;
        }

        foreach (CompanyAttachment attachment in attachmentsResult.Value!)
        {
            ct.ThrowIfCancellationRequested();
            if (attachment.IsConfidential)
            {
                continue;
            }

            Result<AttachmentContent> contentResult =
                await companyProvider.DownloadAttachmentAsync(siren, attachment.Id, credentials, ct);
            if (contentResult.IsFailure)
            {
                continue;
            }

            AttachmentContent content = contentResult.Value!;
            string safeName = SanitizeFileName(content.FileName);
            ZipArchiveEntry entry = archive.CreateEntry($"{siren.Value}/{safeName}", CompressionLevel.Optimal);
            await using Stream entryStream = entry.Open();
            await content.Stream.CopyToAsync(entryStream, ct);
            await content.Stream.DisposeAsync();
        }
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        var builder = new System.Text.StringBuilder(name.Length);
        foreach (char c in name)
        {
            builder.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
        }
        string clean = builder.ToString().Trim();
        return clean.Length == 0 ? "document.pdf" : clean;
    }
}
