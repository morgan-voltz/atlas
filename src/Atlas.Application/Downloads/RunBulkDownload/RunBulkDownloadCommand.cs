using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.RunBulkDownload;

/// <summary>
/// Exécution arrière-plan d'un job de téléchargement (F-014). Déclenchée par Hangfire après
/// la création du job (Pending). Idempotent : si le job est déjà <c>Ready</c> ou <c>Running</c>,
/// le handler n'agit pas (re-exécution Hangfire safe).
/// </summary>
public sealed record RunBulkDownloadCommand(Guid JobId) : IRequest<Result>;
