using Atlas.Application.Veille.GetMySubscriptions;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.AddUserFeedSource;

internal sealed class AddUserFeedSourceHandler(
    IFeedSourceRepository sourceRepository,
    IVeilleSubscriptionRepository subscriptionRepository,
    IEnumerable<IExternalContentSource> contentSources,
    IFeedSubscriptionPolicy policy,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddUserFeedSourceCommand, Result<VeilleSubscriptionDto>>
{
    public async Task<Result<VeilleSubscriptionDto>> Handle(
        AddUserFeedSourceCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string url = request.Url?.Trim() ?? string.Empty;

        // 1. Modération : la source n'est pas dans la bibliothèque d'URLs interdites.
        if (!policy.IsUrlAllowed(url))
        {
            return Result<VeilleSubscriptionDto>.Fail(VeilleErrors.SourceBlocked);
        }

        // 2. Limite d'abonnements par compte (plafonnée en hébergé, illimitée en self-hosted).
        if (policy.MaxSubscriptionsPerUser is { } max)
        {
            int count = await subscriptionRepository.CountByUserAsync(userId, cancellationToken);
            if (count >= max)
            {
                return Result<VeilleSubscriptionDto>.Fail(VeilleErrors.SubscriptionLimitReached(max));
            }
        }

        DateTimeOffset now = clock.UtcNow;

        // 3. Construire la source candidate (valide et canonicalise l'URL).
        string name = string.IsNullOrWhiteSpace(request.Name) ? DeriveName(url) : request.Name!.Trim();
        Result<FeedSource> created = FeedSource.Create(name, url, FeedSourceType.Rss, policy.UserFeedPollingInterval, now);
        if (created.IsFailure)
        {
            return Result<VeilleSubscriptionDto>.Fail(created.Error!);
        }

        FeedSource candidate = created.Value!;

        // 4. Dédup par URL canonique : les sources sont partagées entre utilisateurs.
        FeedSource? source = await sourceRepository.GetByUrlAsync(candidate.Url, cancellationToken);
        if (source is null)
        {
            // Test de fetch/parsing avant de persister une source jamais vue (F-043 : validation).
            IExternalContentSource? provider = contentSources.FirstOrDefault(c => c.CanHandle(candidate.Type));
            if (provider is null)
            {
                return Result<VeilleSubscriptionDto>.Fail(VeilleErrors.FeedUnreachable);
            }

            Result<IReadOnlyList<FeedItemDraft>> fetch = await provider.FetchAsync(candidate, null, cancellationToken);
            if (fetch.IsFailure)
            {
                return Result<VeilleSubscriptionDto>.Fail(VeilleErrors.FeedUnreachable);
            }

            source = candidate;
            await sourceRepository.AddAsync(source, cancellationToken);
        }
        else if (await subscriptionRepository.ExistsAsync(userId, source.Id, cancellationToken))
        {
            // Déjà abonné à une source existante.
            return Result<VeilleSubscriptionDto>.Fail(VeilleErrors.AlreadySubscribed);
        }

        var subscription = VeilleSubscription.Create(userId, source.Id, now);
        await subscriptionRepository.AddAsync(subscription, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VeilleSubscriptionDto>.Ok(VeilleSubscriptionDto.From(subscription, source));
    }

    private static string DeriveName(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.Host : url;
}
