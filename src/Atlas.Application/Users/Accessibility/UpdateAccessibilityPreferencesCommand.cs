using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Accessibility;

public sealed record UpdateAccessibilityPreferencesCommand(
    Guid UserId,
    bool HighContrast,
    bool ReduceMotion,
    AccessibilityFontPreference FontPreference) : IRequest<Result>;
