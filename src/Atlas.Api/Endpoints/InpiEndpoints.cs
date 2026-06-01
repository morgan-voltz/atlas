using Atlas.Application.Inpi;
using Atlas.Application.Inpi.ConnectInpiAccount;
using Atlas.Application.Inpi.DisconnectInpiAccount;
using Atlas.Application.Inpi.GetInpiConnectionStatus;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class InpiEndpoints
{
    public static IEndpointRouteBuilder MapInpiEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/inpi").WithTags("INPI").RequireAuthorization();

        group.MapPost("/connection", ConnectAsync);
        group.MapGet("/connection", StatusAsync);
        group.MapDelete("/connection", DisconnectAsync);

        return routes;
    }

    private static async Task<IResult> ConnectAsync(
        ConnectInpiRequest request,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new ConnectInpiAccountCommand(user.Id, request.Username, request.Password), ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> StatusAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<InpiConnectionStatusDto> result = await sender.Send(new GetInpiConnectionStatusQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DisconnectAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new DisconnectInpiAccountCommand(user.Id), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }
}
