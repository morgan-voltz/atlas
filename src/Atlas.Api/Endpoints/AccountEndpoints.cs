using System.Security.Claims;
using Atlas.Application.Users.Privacy;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/account").WithTags("Account").RequireAuthorization();

        group.MapGet("/export", ExportAsync);
        group.MapDelete("/", DeleteAsync);

        return routes;
    }

    private static async Task<IResult> ExportAsync(ClaimsPrincipal principal, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<UserDataExportDto> result = await sender.Send(new ExportUserDataQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DeleteAsync(ClaimsPrincipal principal, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new DeleteAccountCommand(userId), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }
}
