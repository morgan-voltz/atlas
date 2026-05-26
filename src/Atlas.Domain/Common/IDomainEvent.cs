namespace Atlas.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
