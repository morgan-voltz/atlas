using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Accessibility;

internal sealed class UpdateAccessibilityPreferencesHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateAccessibilityPreferencesCommand, Result>
{
    public async Task<Result> Handle(
        UpdateAccessibilityPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        var prefs = new UserAccessibilityPreferences(
            HighContrast: request.HighContrast,
            ReduceMotion: request.ReduceMotion,
            FontPreference: request.FontPreference);

        user.UpdateAccessibilityPreferences(prefs);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
