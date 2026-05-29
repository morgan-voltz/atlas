using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Accessibility;

public sealed record GetAccessibilityPreferencesQuery(Guid UserId)
    : IRequest<Result<AccessibilityPreferencesDto>>;
