namespace Atlas.Application.Inpi;

public sealed record InpiConnectionStatusDto(
    bool Connected,
    string? Status,
    DateTimeOffset? LastTestedAt);
