using Atlas.Application.Users.Accessibility;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

/// <summary>
/// Endpoints des préférences d'accessibilité utilisateur (cf. <c>docs/06-accessibilite.md</c>
/// §6.3). Synchronise high contrast / reduce motion / police facilitante multi-device.
/// </summary>
internal static class AccessibilityEndpoints
{
    public static IEndpointRouteBuilder MapAccessibilityEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/user/preferences/accessibility")
            .WithTags("Accessibility")
            .RequireAuthorization();

        group.MapGet("/", GetAsync);
        group.MapPut("/", UpdateAsync);

        return routes;
    }

    private static async Task<IResult> GetAsync(CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result<AccessibilityPreferencesDto> result =
            await sender.Send(new GetAccessibilityPreferencesQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> UpdateAsync(
        CurrentUser user,
        UpdateAccessibilityPreferencesRequest body,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new UpdateAccessibilityPreferencesCommand(
                user.Id,
                body.HighContrast,
                body.ReduceMotion,
                body.FontPreference),
            ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    public sealed record UpdateAccessibilityPreferencesRequest(
        bool HighContrast,
        bool ReduceMotion,
        AccessibilityFontPreference FontPreference);
}
