using Atlas.Application.Search;
using Atlas.Application.Search.GetSearchHistory;
using Atlas.Domain.Common;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Search;

public class SearchHistoryHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly ISearchHistoryRepository _repository = Substitute.For<ISearchHistoryRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public SearchHistoryHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Recorder_persists_entry_then_prunes()
    {
        var handler = new RecordSearchHistoryHandler(_repository, _clock, _unitOfWork);

        await handler.Handle(
            new SearchPerformedNotification(Guid.NewGuid(), SearchType.CompanyBySiren, "552032534"),
            CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<SearchHistoryEntry>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).PruneAsync(Arg.Any<UserId>(), 200, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSearchHistory_returns_mapped_entries()
    {
        var userId = UserId.New();
        _repository.GetRecentByUserAsync(Arg.Any<UserId>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchHistoryEntry>
            {
                SearchHistoryEntry.Record(userId, SearchType.CompanyBySiren, "552032534", Now),
                SearchHistoryEntry.Record(userId, SearchType.TrademarkByName, "nike", Now),
            });

        Result<IReadOnlyList<SearchHistoryEntryDto>> result = await new GetSearchHistoryHandler(_repository)
            .Handle(new GetSearchHistoryQuery(userId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Type.Should().Be("CompanyBySiren");
        result.Value![0].Query.Should().Be("552032534");
    }
}
