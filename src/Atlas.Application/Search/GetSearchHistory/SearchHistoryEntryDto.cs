namespace Atlas.Application.Search.GetSearchHistory;

public sealed record SearchHistoryEntryDto(string Type, string Query, DateTimeOffset CreatedAt);
