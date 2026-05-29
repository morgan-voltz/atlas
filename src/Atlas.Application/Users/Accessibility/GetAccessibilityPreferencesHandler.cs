using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Accessibility;

internal sealed class GetAccessibilityPreferencesHandler(IUserRepository userRepository)
    : IRequestHandler<GetAccessibilityPreferencesQuery, Result<AccessibilityPreferencesDto>>
{
    public async Task<Result<AccessibilityPreferencesDto>> Handle(
        GetAccessibilityPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result<AccessibilityPreferencesDto>.Fail(UserErrors.NotFound);
        }

        UserAccessibilityPreferences prefs = user.AccessibilityPreferences;
        return Result<AccessibilityPreferencesDto>.Ok(
            new AccessibilityPreferencesDto(prefs.HighContrast, prefs.ReduceMotion, prefs.FontPreference));
    }
}
