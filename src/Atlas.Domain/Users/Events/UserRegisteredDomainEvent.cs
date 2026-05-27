using Atlas.Domain.Common;

namespace Atlas.Domain.Users.Events;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    EmailAddress Email,
    DateTimeOffset OccurredOn) : IDomainEvent;
