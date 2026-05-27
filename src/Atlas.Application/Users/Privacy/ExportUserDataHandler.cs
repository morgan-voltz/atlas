using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Privacy;

internal sealed class ExportUserDataHandler(
    IUserRepository userRepository,
    IInpiCredentialsRepository inpiCredentialsRepository,
    ISearchHistoryRepository searchHistoryRepository)
    : IRequestHandler<ExportUserDataQuery, Result<UserDataExportDto>>
{
    private const int HistoryLimit = 200;

    public async Task<Result<UserDataExportDto>> Handle(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<UserDataExportDto>.Fail(UserErrors.NotFound);
        }

        InpiCredentials? credentials = await inpiCredentialsRepository.GetByUserIdAsync(userId, cancellationToken);
        IReadOnlyList<SearchHistoryEntry> history =
            await searchHistoryRepository.GetRecentByUserAsync(userId, HistoryLimit, cancellationToken);

        InpiConnectionExportDto? inpiConnection = credentials is null
            ? null
            : new InpiConnectionExportDto(credentials.Status.ToString(), credentials.LastTestedAt);

        IReadOnlyList<SearchHistoryExportDto> searchHistory = history
            .Select(entry => new SearchHistoryExportDto(entry.Type.ToString(), entry.Query, entry.CreatedAt))
            .ToList();

        var export = new UserDataExportDto(
            user.Id.Value,
            user.Email.Value,
            user.Status.ToString(),
            user.CreatedAt,
            user.EmailVerifiedAt,
            user.TwoFactorEnabled,
            inpiConnection,
            searchHistory);

        return Result<UserDataExportDto>.Ok(export);
    }
}
