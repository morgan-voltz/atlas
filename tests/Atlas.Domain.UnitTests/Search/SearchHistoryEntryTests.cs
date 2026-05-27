using Atlas.Domain.Search;
using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Search;

public class SearchHistoryEntryTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Record_trims_query_and_sets_fields()
    {
        var entry = SearchHistoryEntry.Record(UserId.New(), SearchType.CompanyByName, "  Renault  ", Now);

        entry.Query.Should().Be("Renault");
        entry.Type.Should().Be(SearchType.CompanyByName);
        entry.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public void Record_caps_query_length()
    {
        string longQuery = new('x', 300);

        var entry = SearchHistoryEntry.Record(UserId.New(), SearchType.TrademarkByName, longQuery, Now);

        entry.Query.Length.Should().Be(SearchHistoryEntry.MaxQueryLength);
    }
}
