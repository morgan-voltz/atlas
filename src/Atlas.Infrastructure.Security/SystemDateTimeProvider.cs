using Atlas.Domain.Common;

namespace Atlas.Infrastructure.Security;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
