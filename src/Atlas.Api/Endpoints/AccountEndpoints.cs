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

    private static async Task<IResult> ExportAsync(CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result<UserDataExportDto> result = await sender.Send(new ExportUserDataQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DeleteAsync(CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result result = await sender.Send(new DeleteAccountCommand(user.Id), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }
}
